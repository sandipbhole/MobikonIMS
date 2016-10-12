using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.Security;
using System.Web.Script.Serialization;
using PagedList;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;

using mC = Mobikon.IMS.Common;
using MID = Mobikon.IMS.Data;
using MIM = Mobikon.IMS.Message;


namespace Mobikon.IMS.Controllers
{
    public class ClientController : Controller
    {
        MID.ClientDAL clientDAL = new MID.ClientDAL();
        MID.CityDAL cityDAL = new MID.CityDAL();
        MID.CountryDAL countryDAL = new MID.CountryDAL();
        MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
        MID.OutletDAL outletDAL = new MID.OutletDAL();
        MID.StatusDAL statusDAL = new MID.StatusDAL();

        public ActionResult Client(string sortOrder, string statusClientFilter ,string countryNameFilter,string cityNameFilter, string clientNameFilter, string statusClientSearch,string countryNameSearch, string clientNameSearch, string cityNameSearch, int? page,  string paging)
        {
            if (Session["UserName"] == null)
               return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Brand List";

            List<MIM.Client> responseGetClient = new List<MIM.Client>();
            List<MIM.Client> responseClient = new List<MIM.Client>();

            ViewBag.currentSort = sortOrder;
            //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

            ViewBag.outletNameSearch = outletDAL.GetOutletNameList(pageName:"Client");

            if (!string.IsNullOrEmpty(countryNameSearch))
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "Client");
            else
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "Client",selectedCountryName:countryNameFilter);

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "Client");
            else
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "Client",selectedCityName:cityNameFilter);           

            if (String.IsNullOrEmpty(sortOrder))
                sortOrder = "createDateSortDesc";

            ViewBag.createDateSort = sortOrder == "createDateSortDesc" ? "createDateSort" : "createDateSortDesc";
            ViewBag.clientNameSort = sortOrder == "clientNameSort" ? "clientNameSortDesc" : "clientNameSort";
            ViewBag.cityNameSort = sortOrder == "cityNameSort" ? "cityNameSortDesc" : "cityNameSort";
            ViewBag.countryNameSort = sortOrder == "countryNameSort" ? "countryNameSortDesc" : "countryNameSort";
                      

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            bool statusClient = true;
            if (statusClientSearch == "false")
                statusClient = false;

            if (clientNameSearch != null)
                page = 1;
            if (cityNameSearch != null)
                page = 1;
            if (countryNameSearch != null)
                page = 1;

            if (!string.IsNullOrEmpty(clientNameSearch))
                ViewBag.clientNameFilter = clientNameSearch;
            else
            {
                clientNameSearch = clientNameFilter;
                ViewBag.clientNameFilter = clientNameSearch;
            }

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameFilter = cityNameSearch;
            else
            {
                cityNameSearch = cityNameFilter;
                ViewBag.cityNameFilter = cityNameSearch;
            }

            if (!string.IsNullOrEmpty(statusClientSearch))
                ViewBag.statusClientFilter = statusClientSearch;
            else
            {
                statusClientSearch = statusClientFilter;
                ViewBag.statusClientFilter = statusClientSearch;
            }

            if (!string.IsNullOrEmpty(countryNameSearch))
                ViewBag.countryNameFilter = countryNameSearch;
            else
            {
                countryNameSearch = countryNameFilter;
                ViewBag.countryNameFilter = countryNameSearch;
            }

            responseGetClient = clientDAL.GetClientList(statusClient:statusClient);           
            var clients = responseGetClient.AsQueryable();

            if (!string.IsNullOrEmpty(clientNameSearch))
                clients = clients.Where(client => client.clientName.ToUpper().Trim().Contains(clientNameSearch.ToUpper().Trim()));
                
            if (!string.IsNullOrEmpty(cityNameSearch))
            { 
                if (cityNameSearch != "All" && cityNameSearch != "Select city")
                    clients = clients.Where(client => client.cityName.ToUpper().Trim() == cityNameSearch.ToUpper().Trim());
            }
            if (!string.IsNullOrEmpty(countryNameSearch))
            {
                if (countryNameSearch != "All" && cityNameSearch != "Select country")
                    clients = clients.Where(client => client.countryName.ToUpper().Trim() == countryNameSearch.ToUpper().Trim());
            }
            switch (sortOrder)
            {
                case "createDateSort":
                    clients = clients.OrderBy(client => client.createdDate);
                    break;
                case "clientNameSortDesc":
                    clients = clients.OrderByDescending(client => client.clientName);
                    break;
                case "cityNameSortDesc":
                    clients = clients.OrderByDescending(client => client.cityName);
                    break;
                case "cityNameSort":
                    clients = clients.OrderBy(client => client.cityName);
                    break;
                case "countryNameSortDesc":
                    clients = clients.OrderByDescending(client => client.countryName);
                    break;
                case "countryNameSort":
                    clients = clients.OrderBy(client => client.countryName);
                    break;
                case "ClientNameSort":
                    clients = clients.OrderBy(client => client.clientName);
                    break;
                default:  // createDateDesc
                    clients = clients.OrderByDescending(client => client.createdDate);
                    break;
            }

            foreach (var response in clients)
            {
                responseClient.Add(new MIM.Client()
                {
                    clientID = response.clientID,
                    clientName = response.clientName,
                    cityID = response.cityID,
                    cityName = response.cityName,
                    userID = response.userID,
                    userName = response.userName,
                    activated = response.activated,
                    address = response.address,
                    createdDate =response.createdDate,
                    countryID = response.countryID,
                    countryName = response.countryName,
                    seperator = mC.CommonMobikonIMS.seperator
                });
            }
            int pageNumber = (page ?? 1);
            return View(responseClient.ToPagedList(pageNumber, pageSize));           
        }

        public void ExportToPDF(string clientName)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(clientName: clientName);

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=DeviceHistory.pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            StringWriter swr = new StringWriter();
            HtmlTextWriter htmlwr = new HtmlTextWriter(swr);
            grid.AllowPaging = false;
            grid.DataSource = responseGetDeviceTransactionRpt;
            grid.DataBind();
            grid.RenderControl(htmlwr);
            StringReader srr = new StringReader(swr.ToString());
            Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
            PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

            pdfdoc.Open();
            htmlparser.Parse(srr);
            pdfdoc.Close();


            Response.Write(pdfdoc);
            Response.End();
        }

        public FileResult DisplayPDF(string filePath)
        {
            return File(filePath, "application/pdf");
        }

        public void ExportToExcel(string clientName)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(clientName: clientName);
            grid.DataSource = responseGetDeviceTransactionRpt;
            grid.DataBind();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=DeviceHistory.xls");
            Response.ContentType = "application/excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Write(sw.ToString());

            Response.End();
        }

        public ActionResult EditDeviceTransaction(long serialNo)
        {
            MIM.DeviceTransaction responseGetDeviceTransaction = new MIM.DeviceTransaction();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (serialNo == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetail(serialNo: serialNo);
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")"; ;
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true, selectedClientID: responseGetDeviceTransaction.clientID, selectedClientName:responseGetDeviceTransaction.clientName);         
            ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID:responseGetDeviceTransaction.statusID, selectedStatusName:responseGetDeviceTransaction.status);        
            ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID:responseGetDeviceTransaction.outletID, selectedOutletName:responseGetDeviceTransaction.outletName);
            ViewBag.companyOwner = ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner:responseGetDeviceTransaction.companyOwner);

            if (string.IsNullOrEmpty(responseGetDeviceTransaction.productSerial))
            {
                return HttpNotFound();
            }

            return View(responseGetDeviceTransaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDeviceTransaction(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("EditDeviceTransaction");

            if (command == "Close")
                return RedirectToAction("CreateDeviceTransaction","Device");

            if (command == "Save")
            {
                ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, selectedClientID: requestSetDeviceTransaction.clientID, selectedClientName: requestSetDeviceTransaction.clientName);
                ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: requestSetDeviceTransaction.statusID, selectedStatusName: requestSetDeviceTransaction.status);
                ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID: requestSetDeviceTransaction.outletID, selectedOutletName: requestSetDeviceTransaction.outletName);
                ViewBag.companyOwner = ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner:requestSetDeviceTransaction.companyOwner);

                if (ModelState.IsValid)
                {
                    requestSetDeviceTransaction.userName = Session["UserName"].ToString();
                    foreach (string upload in Request.Files)
                    {
                        //if (!Request.Files[upload].HasFile()) continue;
                        string path = deviceDAL.GetDCFileFolderPath();
                        string fileName = requestSetDeviceTransaction.dc + Path.GetExtension(Request.Files[upload].FileName);//Path.GetFileName(Request.Files[upload].FileName);
                        Request.Files[upload].SaveAs(Path.Combine(path, fileName));
                        requestSetDeviceTransaction.dcFile = Path.Combine(path, fileName);
                    }

                    if (requestSetDeviceTransaction.dcDate.ToString().Length > 0)
                    {
                        requestSetDeviceTransaction.deliveryDate = requestSetDeviceTransaction.dcDate;

                        if (string.IsNullOrEmpty(requestSetDeviceTransaction.dc))
                        {
                            ViewBag.Message = "Please provide DC (Delivery Challan No.).";
                            return View(requestSetDeviceTransaction);
                        }
                    }

                    if (requestSetDeviceTransaction.rdcDate.ToString().Length > 0)
                    {
                        if (requestSetDeviceTransaction.rdc.Length > 0)
                        {
                            ViewBag.Message = "Please provide rdc.";
                            return View(requestSetDeviceTransaction);
                        }
                    }

                    if (string.IsNullOrEmpty(requestSetDeviceTransaction.rdc))
                    {
                        if (requestSetDeviceTransaction.rdcDate.ToString().Length > 0)
                        {
                            ViewBag.Message = "Please provide rdc date.";
                            return View(requestSetDeviceTransaction);
                        }
                    }

                    if (requestSetDeviceTransaction.hicDate.ToString().Length > 0)
                    {
                        if (requestSetDeviceTransaction.hic.Length > 0)
                        {
                            ViewBag.Message = "Please provide hic.";
                            return View(requestSetDeviceTransaction);
                        }
                    }

                    if (string.IsNullOrEmpty(requestSetDeviceTransaction.hic))
                    {
                        if (requestSetDeviceTransaction.hicDate.ToString().Length > 0)
                        {
                            ViewBag.Message = "Please provide hic date.";
                            return View(requestSetDeviceTransaction);
                        }
                    }
                    bool result = deviceDAL.EditDeviceTransaction(requestSetDeviceTransaction);

                    if (result == true)
                        return RedirectToAction("DeviceHistory","Client", new { clientIDSearch = requestSetDeviceTransaction.clientID, productSerialSearch = requestSetDeviceTransaction.productSerial });
                    else
                        ViewBag.Message = "Device transaction not updated successfully.";
                }               
                
                return View(requestSetDeviceTransaction);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult DeviceTransactionDetail(long serialNo)
        {
            MIM.DeviceTransaction responseGetDeviceTransaction = new MIM.DeviceTransaction();
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
                
            if (serialNo == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")"; ;
            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetail();
            responseGetDeviceTransaction.seperator = mC.CommonMobikonIMS.seperator;

            if (string.IsNullOrEmpty(responseGetDeviceTransaction.productSerial))
            {
                return HttpNotFound();
            }
            return View(responseGetDeviceTransaction); 
        }

        public ActionResult DeviceHistory(string sortOrder, string productSerialFilter, string deviceDetailsFilter, string clientNameFilter, string cityNameFilter,string statusFilter,  string productSerialSearch,string clientNameSearch,string deviceDetailsSearch, string cityNameSearch,string statusSearch, int? page, string paging)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Device History";

           List <MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
            List<MIM.DeviceTransaction> responseDeviceTransaction = new List<MIM.DeviceTransaction>();

            Session["ProductSerial"] = null;
            Session["OutletName"] = null;

            if (Session["ClientName"] == null)
                Session["ClientName"] = clientNameSearch;

            if (string.IsNullOrEmpty(clientNameSearch))
                clientNameSearch = Session["ClientName"].ToString();

            if (Session["ClientName"].ToString() != clientNameSearch)
                Session["ClientName"] = clientNameSearch;              

            ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();
            //ViewBag.currentSort = sortOrder;
            //ViewBag.productSerialSearch = deviceDAL.GetProductSerialList(statusDevice:mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString(), pageName: "DeviceHistory",showBlockedDevice:false);

            if (!string.IsNullOrEmpty(clientNameSearch))
                ViewBag.outletNameSearch = outletDAL.GetClientwiseOutletNameList(pageName:"DeviceHistory",clientName:clientNameSearch);
            else
                ViewBag.outletNameSearch = outletDAL.GetClientwiseOutletNameList(pageName: "DeviceHistory", clientName: clientNameFilter);

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory", selectedCityName: cityNameSearch);
            else
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory", selectedCityName: cityNameFilter);
            
            //ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName:"DeviceHistory",selectedCountryName:countryNameSearch);


            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory");
            else
                ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory", selectedStatusName: statusFilter);


            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "deliveryDateDesc";

            ViewBag.deliveryDateSort = sortOrder == "deliveryDateSort" ? "deliveryDateDesc" : "deliveryDateSort";
            ViewBag.productSerialSort = sortOrder == "productSerialSort" ? "productSerialSortDesc" : "productSerialSort";
            ViewBag.deviceDetailsSort = sortOrder == "deviceDetailsSort" ? "deviceDetailsSortDesc" : "deviceDetailsSort";
            ViewBag.cityNameSort = sortOrder == "cityNameSort" ? "cityNameSort" : "cityNameSortDesc";
            ViewBag.statusSort = sortOrder == "statusSort" ? "statusSortDesc" : "statusSort";
           

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            if (!string.IsNullOrEmpty(productSerialSearch))
                page = 1;
            if (!string.IsNullOrEmpty(deviceDetailsSearch))
                page = 1;
            if (!string.IsNullOrEmpty(clientNameSearch))
                page = 1;                                 
            if (!string.IsNullOrEmpty(cityNameSearch))
                page = 1;
            if (!string.IsNullOrEmpty(statusSearch))
                page = 1;

            if (!string.IsNullOrEmpty(productSerialSearch))
                ViewBag.productSerialSearch = productSerialSearch;
            else
            {
                productSerialSearch = productSerialFilter;
                ViewBag.productSerialFilter = productSerialSearch;
            }


            if (!string.IsNullOrEmpty(deviceDetailsSearch))
                ViewBag.deviceDetailsFilter = deviceDetailsSearch;
            else
            {
                deviceDetailsSearch = deviceDetailsFilter;
                ViewBag.deviceDetailsFilter = deviceDetailsSearch;
            }

            if (!string.IsNullOrEmpty(clientNameSearch))
                ViewBag.clientNameFilter = clientNameSearch;
            {
                clientNameSearch = clientNameFilter;
                ViewBag.clientNameFilter = clientNameSearch;
            }  
                                  
            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameFilter = cityNameSearch;
            else
            {
                cityNameSearch = cityNameFilter;
                ViewBag.cityNameFilter = cityNameSearch;
            }

            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.statusFilter = statusSearch;
            else
            {
                statusSearch = statusFilter;
                ViewBag.statusFilter = statusSearch;
            }

            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetailList(clientName:clientNameSearch);
            var deviceTransactions = responseGetDeviceTransaction.AsQueryable();

            if (!string.IsNullOrEmpty(productSerialSearch))
            {
                if (productSerialSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.productSerial.ToUpper().Trim().Contains(productSerialSearch.ToUpper().Trim()));
            }

            if (!string.IsNullOrEmpty(deviceDetailsSearch))
                deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.deviceDetails.ToUpper().Trim().Contains(deviceDetailsSearch.ToUpper().Trim()));
                       
            if (!string.IsNullOrEmpty(cityNameSearch))
            {
                if (cityNameSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.cityName.ToUpper().Trim() == cityNameSearch.ToUpper().Trim());
            }

            if (!string.IsNullOrEmpty(statusSearch))
            {
                if (statusSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.status.ToUpper().Trim() == statusSearch.ToUpper().Trim());
            }

            switch (sortOrder)
            {
                case "productSerialSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.productSerial);
                    break;
                case "productSerialSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.productSerial);
                    break;
                case "deviceDetailsSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.deviceDetails);
                    break;
                case "deviceDetailsSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.deviceDetails);
                    break;
                case "cityNameSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.clientCityName);
                    break;
                case "cityNameSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.status);
                    break;
                case "statusSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.clientCityName);
                    break;
                case "statusSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.status);
                    break;
                case "deliveryDateSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.deliveryDate);
                    break;
                default:  // deliveryDateSortDesc
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.deliveryDate);
                    break;
            }

            foreach (var response in deviceTransactions)
            {
                responseDeviceTransaction.Add(new MIM.DeviceTransaction()
                {
                    serialNo = response.serialNo,
                    deviceID = response.deviceID,
                    productSerial = response.productSerial,
                    deviceDetails = response.deviceDetails,
                    status = response.status,
                    statusID = response.statusID,
                  
                    companyOwner = response.companyOwner,
                    address = response.address,
                    cityID = response.cityID,
                    cityName = response.cityName,
                    clientID = response.clientID,
                    clientName = response.clientName,
                    countryID = response.countryID,
                    countryName = response.countryName,
                    damagedOldDevice = response.damagedOldDevice,
                    dc = response.dc,
                    dcDate = response.dcDate,
                    deliveryDate = response.deliveryDate,
                    hic = response.hic,
                    hicDate = response.hicDate,
                    insuranceClaim = response.insuranceClaim,
                    insured = response.insured,
                    outletID = response.outletID,
                    outletName = response.outletName,
                    rdc = response.rdc,
                    rdcDate = response.rdcDate,
                    remarks = response.remarks,
                    transferOwnershipDate = response.transferOwnershipDate,
                    userID = response.userID,
                    userName = response.userName,
                    clientCityName = response.clientCityName,
                    clientCountryName = response.clientCountryName,
                    dcFile = response.dcFile,
                    seperator = mC.CommonMobikonIMS.seperator
                });
            }
            int pageNumber = (page ?? 1);
            return View(responseDeviceTransaction.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult CreateClient()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Add Brand"; 

            ViewBag.countryName = countryDAL.GetCountryNameList();
            ViewBag.cityName = cityDAL.GetCityNameList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClient(MIM.Client requestSetClient, string command)
        {
            bool result = false;
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("CreateClient");

            if (command == "Close")
                return RedirectToAction("Client");

            ViewBag.cityName = cityDAL.GetCityNameList();
            ViewBag.countryName = countryDAL.GetCountryNameList();

            if (command == "Save")
            {
                if (ModelState.IsValid)
                {
                    requestSetClient.userName = Session["UserName"].ToString();

                    bool alreadyExists = clientDAL.CheckClientName(requestSetClient.clientName);
                    if (alreadyExists == true)
                    {
                        ViewBag.Message = "Client name already exists.";
                        return View();
                    }

                    result = clientDAL.InsertClient(requestSetClient);

                    if (result == true)
                        return RedirectToAction("Client");
                    else
                        ViewBag.message = "Client not saved successfully.";
                }                     
                        
                return View(requestSetClient);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult ClientDetails(long clientID,bool statusClient)
        {
            MIM.Client responseGetClient = new MIM.Client();
            
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
            
            if (clientID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Brand Details";

            responseGetClient = clientDAL.GetClient(clientID:clientID, statusClient:statusClient);
            responseGetClient.seperator = mC.CommonMobikonIMS.seperator;

            if (string.IsNullOrEmpty(responseGetClient.clientName))
            {
                return HttpNotFound();
            }
            return View(responseGetClient);
        }

        public ActionResult UpdateClient()
        {
            MIM.Client responseGetClient = new MIM.Client();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")"; 

            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true);
            ViewBag.countryName = countryDAL.GetCountryNameList();
            ViewBag.cityName = cityDAL.GetCityNameList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateClient(MIM.Client requestSetClient, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
            {
                return RedirectToAction("UpdateClient", "Client", new { clientID = requestSetClient.clientID });
            }

            if (command == "Close")
                return RedirectToAction("Client");

            if (command == "Save")
            {
                requestSetClient.userName = Session["UserName"].ToString();
                ViewBag.ClientName = clientDAL.GetClientNameList(selectedClientName: requestSetClient.clientName);
                ViewBag.countryName = countryDAL.GetCountryNameList(selectedCountryName: requestSetClient.countryName);
                ViewBag.cityName = cityDAL.GetCityNameList(selectedCityName: requestSetClient.cityName);

                if (ModelState.IsValid)
                {
                    bool updateClient = clientDAL.EditClient(requestSetClient);
                    if (updateClient == true)
                        return RedirectToAction("Client");
                }

                ViewBag.Message = "Client not updated successfully.";
                return View(requestSetClient);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult EditClient(long clientID,bool statusClient)
        {
            MIM.Client responseGetClient = new MIM.Client();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            //if (string.IsNullOrEmpty(clientName))
            if (clientID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Edit Brand";

            responseGetClient = clientDAL.GetClient(clientID: clientID, statusClient:statusClient);
            ViewBag.countryName = countryDAL.GetCountryNameList(selectedCountryID:responseGetClient.countryID, selectedCountryName:responseGetClient.countryName);
            ViewBag.cityName = cityDAL.GetCityNameList(selectedCityID:responseGetClient.cityID, selectedCityName: responseGetClient.cityName);

            if (string.IsNullOrEmpty(responseGetClient.clientName))
            {
                return HttpNotFound();
            }

            return View(responseGetClient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClient(MIM.Client requestSetClient, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
           

            if (command == "Cancel")
            {
                return RedirectToAction("EditClient","Client", new { clientID = requestSetClient.clientID});
            }

            if (command == "Close")
                return RedirectToAction("Client");

            bool updateClient = false;

            if (command == "Save")
            {
                requestSetClient.userName = Session["UserName"].ToString();
                ViewBag.countryName = countryDAL.GetCountryNameList(selectedCountryID: requestSetClient.countryID, selectedCountryName: requestSetClient.countryName);
                ViewBag.cityName = cityDAL.GetCityNameList(selectedCityID: requestSetClient.cityID, selectedCityName: requestSetClient.cityName);

                if (ModelState.IsValid)
                {
                    updateClient = clientDAL.EditClient(requestSetClient);
                    if (updateClient == true)
                        return RedirectToAction("Client");
                    else
                        ViewBag.Message = "Brand not updated successfully.";
                }                

                return View(requestSetClient);
            }
            return RedirectToAction("Login", "Login");
        }

        public JsonResult GetClientnamewiseClient(string clientName)
        {
            MIM.Client responseGetClient = new MIM.Client();

            responseGetClient = clientDAL.GetClient(0,clientName,true);
            return Json(responseGetClient,JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCountrywiseCity(string countryName)
        {
            List<MIM.City> responseGetCity = new List<MIM.City>();

            responseGetCity = cityDAL.GetCityList(countryName);
            return Json(responseGetCity, JsonRequestBehavior.AllowGet);
        }
    }   
}