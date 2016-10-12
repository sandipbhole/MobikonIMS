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
    public class OutletController : Controller
    {
        MID.OutletDAL outletDAL = new MID.OutletDAL();
        MID.CityDAL cityDAL = new MID.CityDAL();
        MID.CountryDAL countryDAL = new MID.CountryDAL();
        MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
        MID.ClientDAL clientDAL = new MID.ClientDAL();
        MID.StatusDAL statusDAL = new MID.StatusDAL();

        public ActionResult Outlet(string sortOrder, string statusOutletFilter,string clientNameFilter, string outletNameFilter,  string countryNameFilter, string cityNameFilter,string clientNameSearch, string outletNameSearch,  string countryNameSearch,string cityNameSearch, string statusOutletSearch, int? page, string paging)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Outlet List";

            List<MIM.Outlet> responseGetOutlet = new List<MIM.Outlet>();
            List<MIM.Outlet> responseOutlet = new List<MIM.Outlet>();

            ViewBag.currentSort = sortOrder;
            //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

            if (!string.IsNullOrEmpty(countryNameSearch))
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "Outlet");
            else
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "Outlet", selectedCountryName: countryNameFilter);

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "Outlet");
            else
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "Outlet", selectedCityName: cityNameFilter);

            if (String.IsNullOrEmpty(sortOrder))
                sortOrder = "createDateSortDesc";

            ViewBag.createDateSort = sortOrder == "createDateSortDesc" ? "createDateSort" : "createDateSortDesc";
            ViewBag.outletNameSort = sortOrder == "outletNameSort" ? "outletNameSortDesc" : "outletNameSort";
            ViewBag.cityNameSort = sortOrder == "cityNameSort" ? "cityNameSortDesc" : "cityNameSort";
            ViewBag.countryNameSort = sortOrder == "countryNameSort" ? "countryNameSortDesc" : "countryNameSort";
            ViewBag.clientNameSort = sortOrder == "clientNameSort" ? "clientNameSortDesc" : "clientNameSort";

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            bool statusOutlet = true;
            if (statusOutletSearch == "false")
                statusOutlet = false;

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

            if (!string.IsNullOrEmpty(outletNameSearch))
                ViewBag.outletNameFilter = outletNameSearch;
            else
            {
                outletNameSearch = outletNameFilter;
                ViewBag.outletNameFilter = outletNameSearch;
            }

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameFilter = cityNameSearch;
            else
            {
                cityNameSearch = cityNameFilter;
                ViewBag.cityNameFilter = cityNameSearch;
            }

            if (!string.IsNullOrEmpty(countryNameSearch))
                ViewBag.countryNameFilter = countryNameSearch;
            else
            {
                countryNameSearch = countryNameFilter;
                ViewBag.countryNameFilter = countryNameSearch;
            }

            if (!string.IsNullOrEmpty(statusOutletSearch))
                ViewBag.statusClientFilter = statusOutletSearch;
            else
            {
                statusOutletSearch = statusOutletFilter;
                ViewBag.statusOutletFilter = statusOutletSearch;
            }


            //if (!string.IsNullOrEmpty(clientNameSearch))
            //    ViewBag.clientNameFilter = clientNameSearch;
            //if (!string.IsNullOrEmpty(outletNameSearch))
            //    ViewBag.outletNameFilter = outletNameSearch;
            //if (!string.IsNullOrEmpty(cityNameSearch))
            //    ViewBag.cityNameFilter = cityNameSearch;              
            //if (!string.IsNullOrEmpty(countryNameSearch))
            //    ViewBag.countryNameFilter = countryNameSearch;
            //if (!string.IsNullOrEmpty(statusOutletSearch))
            //    ViewBag.statusOutletFilter = statusOutletSearch;

            responseGetOutlet = outletDAL.GetOutletList(statusOutlet:statusOutlet);
            var outlets = responseGetOutlet.AsQueryable();

            if (!string.IsNullOrEmpty(outletNameSearch))
                outlets = outlets.Where(outlet => outlet.outletName.ToUpper().Trim().Contains(outletNameSearch.ToUpper().Trim()));

            if (!string.IsNullOrEmpty(clientNameSearch))
                outlets = outlets.Where(outlet => outlet.clientName.ToUpper().Trim().Contains(clientNameSearch.ToUpper().Trim()));

            if (!string.IsNullOrEmpty(cityNameSearch))
            {
                if (cityNameSearch != "All" && cityNameSearch != "Select city")
                    outlets = outlets.Where(outlet => outlet.cityName.ToUpper().Trim()== cityNameSearch.ToUpper().Trim());
            }
            if (!string.IsNullOrEmpty(countryNameSearch))
            {
                if (countryNameSearch != "All" && cityNameSearch != "Select country")
                    outlets = outlets.Where(outlet => outlet.countryName.ToUpper().Trim() == countryNameSearch.ToUpper().Trim());
            }
            switch (sortOrder)
            {
                case "createDateSort":
                    outlets = outlets.OrderBy(outlet => outlet.createdDate);
                    break;
                case "outletNameSortDesc":
                    outlets = outlets.OrderByDescending(outlet => outlet.outletName);
                    break;
                case "outletNameSort":
                    outlets = outlets.OrderBy(outlet => outlet.outletName);
                    break;
                case "cityNameSortDesc":
                    outlets = outlets.OrderByDescending(outlet => outlet.cityName);
                    break;
                case "cityNameSort":
                    outlets = outlets.OrderBy(outlet => outlet.cityName);
                    break;
                case "countryNameSortDesc":
                    outlets = outlets.OrderByDescending(outlet => outlet.countryName);
                    break;
                case "countryNameSort":
                    outlets = outlets.OrderBy(outlet => outlet.countryName);
                    break;                     
                case "clientNameSortDesc":
                    outlets = outlets.OrderByDescending(outlet => outlet.clientName);
                    break;
                case "clientNameSort":
                    outlets = outlets.OrderBy(outlet => outlet.clientName);
                    break;
                default:  // createDateDesc"
                    outlets = outlets.OrderByDescending(outlet => outlet.createdDate);
                    break;
            }

            foreach (var response in outlets)
            {
                responseOutlet.Add(new MIM.Outlet()
                {
                    outletID = response.outletID,
                    outletName = response.outletName,
                    cityID = response.cityID,
                    cityName = response.cityName,
                    userID = response.userID,
                    userName = response.userName,                      
                    address = response.address,
                    createdDate = response.createdDate,
                    countryID = response.countryID,
                    countryName = response.countryName,
                    activated = response.activated,
                    clientName = response.clientName,
                    seperator = mC.CommonMobikonIMS.seperator
                });
            }
            int pageNumber = (page ?? 1);
            return View(responseOutlet.ToPagedList(pageNumber, pageSize));           
        }
        public FileResult DisplayPDF(string filePath)
        {
            return File(filePath, "application/pdf");
        }

        public void ExportToPDF(string outletName)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(outletName: outletName);           

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

        public void ExportToExcel(string outletName)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(outletName: outletName);

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

        public ActionResult DeviceTransactionDetail(long serialNo)
        {
            MIM.DeviceTransaction responseGetDeviceTransaction = new MIM.DeviceTransaction();
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (serialNo == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetail(serialNo:serialNo);
            responseGetDeviceTransaction.seperator = mC.CommonMobikonIMS.seperator;
            if (string.IsNullOrEmpty(responseGetDeviceTransaction.productSerial))
            {
                return HttpNotFound();
            }
            return View(responseGetDeviceTransaction);
        }

        public ActionResult DeviceHistory(string sortOrder, string productSerialFilter,string deviceDetailsFilter,string clientNameFilter, string outletNameFilter, string cityNameFilter, string statusFilter, string productSerialSearch,string deviceDetailsSearch ,string clientNameSearch,  string outletNameSearch, string statusSearch, string cityNameSearch, int? page, string paging)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";

            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
            List<MIM.DeviceTransaction> responseDeviceTransaction = new List<MIM.DeviceTransaction>();

            Session["ClientName"] = null;
            Session["ProductSerial"] = null;

            if (Session["OutletName"] == null)
                Session["OutletName"] = outletNameSearch;

            if (string.IsNullOrEmpty(outletNameSearch))
                outletNameSearch = Session["OutletName"].ToString();

            if (Session["OutletName"].ToString() != outletNameSearch)
                Session["OutletName"] = outletNameSearch;

            //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();
            ViewBag.currentSort = sortOrder;

            if (!string.IsNullOrEmpty(clientNameSearch))
                ViewBag.clientNameSearch = clientDAL.GetClientNameList(pageName: "DeviceHistory", statusClient: true );
            else
                ViewBag.clientNameSearch = clientDAL.GetClientNameList(pageName: "DeviceHistory", statusClient: true,selectedClientName: clientNameFilter);
            //ViewBag.productSerialSearch = deviceDAL.GetProductSerialList(statusDevice:mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString(), pageName: "DeviceHistory",showBlockedDevice:true);                        

            if (!string.IsNullOrEmpty(cityNameSearch))
                 ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory",selectedCityName:cityNameSearch);
            else
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory", selectedCityName: cityNameFilter);

            //ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "DeviceHistory",  selectedCountryName:countryNameSearch);

            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory");
            else
                ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory", selectedStatusName: statusFilter);

            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "deliveryDateSortDesc";

            ViewBag.productSerialSort = sortOrder == "productSerialSort" ? "productSerialSortDesc" : "productSerialSort";
            ViewBag.deviceDetailsSort = sortOrder == "deviceDetailsSort" ? "deviceDetailsSortDesc" : "deviceDetailsSort";
            ViewBag.deliveryDateSort = sortOrder == "deliveryDateSort" ? "deliveryDateSortDesc" : "deliveryDateSort";
            ViewBag.clientNameSort = sortOrder == "clientNameSort" ? "clietNameSortDesc" : "clientNameSort";               
            ViewBag.cityNameSort = sortOrder == "cityNameSort" ? "cityNameSortDesc" : "cityNameSort";
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
            if (!string.IsNullOrEmpty(outletNameSearch))
                page = 1;
            if (!string.IsNullOrEmpty(statusSearch))
                page = 1;
            if (!string.IsNullOrEmpty(cityNameSearch))
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


            if (!string.IsNullOrEmpty(outletNameSearch))
                ViewBag.outletNameFilter = outletNameSearch;
            else
            {
                outletNameSearch = outletNameFilter;
                ViewBag.outletNameFilter = outletNameSearch;
            }

            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.statusFilter = statusSearch;
            else
            {
                statusSearch = statusFilter;
                ViewBag.statusFilter = statusSearch;
            }

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameFilter = cityNameSearch;
            else
            {
                cityNameSearch = cityNameFilter;
                ViewBag.cityNameFilter = cityNameSearch;
            }



            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetailList(outletName:outletNameSearch);
            var deviceTransactions = responseGetDeviceTransaction.AsQueryable();

            if (!string.IsNullOrEmpty(productSerialSearch))
            {   
                //if (productSerialSearch != "All")
                deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.productSerial.ToUpper().Trim().Contains(productSerialSearch.ToUpper().Trim()));
            }

            if (!string.IsNullOrEmpty(deviceDetailsSearch))
                deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.deviceDetails.ToUpper().Trim().Contains(deviceDetailsSearch.ToUpper().Trim()));

            if (!string.IsNullOrEmpty(statusSearch))
            {
                if (statusSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.status.ToUpper().Trim() == statusSearch.ToUpper().Trim());
            }

            if (!string.IsNullOrEmpty(cityNameSearch))
            {
                if (cityNameSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.cityName.ToUpper().Trim()== cityNameSearch.ToUpper().Trim());
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
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.cityName);
                    break;
                case "cityNameSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.cityName);
                    break;
                case "stausSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.status );
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

        public ActionResult CreateOutlet()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Add Outlet";

            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true);
            ViewBag.countryName = countryDAL.GetCountryNameList();
            ViewBag.cityName = cityDAL.GetCityNameList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOutlet(MIM.Outlet requestSetOutlet, string command)
        {
            bool result = false;
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("CreateOutlet");

            if (command == "Close")
                return RedirectToAction("Outlet");

            if (command == "Save")
            {
                requestSetOutlet.userName = Session["UserName"].ToString();

                ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, selectedClientID: requestSetOutlet.clientID, selectedClientName: requestSetOutlet.clientName);
                ViewBag.countryName = countryDAL.GetCountryNameList(selectedCountryID: requestSetOutlet.countryID, selectedCountryName: requestSetOutlet.countryName);
                ViewBag.cityName = cityDAL.GetCityNameList(selectedCityID: requestSetOutlet.cityID, selectedCityName: requestSetOutlet.cityName);

                if (ModelState.IsValid)
                {    
                    bool alreadyExists = outletDAL.CheckOutletName(requestSetOutlet.outletName);
                    if (alreadyExists == true)
                    {
                        ViewBag.Message = "Outlet name already exists.";
                        return View();
                    }
                   result = outletDAL.InsertOutlet(requestSetOutlet);
                    if (result == true)
                        return RedirectToAction("Outlet");
                    else
                        ViewBag.message = "Outlet not saved successfully.";
                }
                 return View(requestSetOutlet);
                
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult OutletDetails(long outletID, bool statusOutlet)
        {
            MIM.Outlet responseGetOutlet = new MIM.Outlet();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            //if (string.IsNullOrEmpty(outletName))
            if (outletID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Outlet Details";

            responseGetOutlet = outletDAL.GetOutlet(outletID:outletID, statusOutlet: statusOutlet);
            responseGetOutlet.seperator = mC.CommonMobikonIMS.seperator;

            if (string.IsNullOrEmpty(responseGetOutlet.outletName))
            {
                return HttpNotFound();
            }
            return View(responseGetOutlet);
        }

        public ActionResult UpdateOutlet()
        {
            MIM.Outlet responseGetOutlet = new MIM.Outlet();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.outletName = outletDAL.GetOutletNameList();
            ViewBag.countryNameSearch = countryDAL.GetCountryNameList();
            ViewBag.cityNameSearch = cityDAL.GetCityNameList();
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOutlet(MIM.Outlet requestSetOutlet, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
            {
                return RedirectToAction("UpdateOutlet", "Outlet", new { outletID = requestSetOutlet.outletID });
            }

            if (command == "Close")
                return RedirectToAction("Outlet");

            if (command == "Save")
            {
                requestSetOutlet.userName = Session["UserName"].ToString();

                if (ModelState.IsValid)
                {
                    bool updateOutlet = outletDAL.EditOutlet(requestSetOutlet);
                    if (updateOutlet == true)
                        return RedirectToAction("Outlet");
                }

                ViewBag.Message = "Outlet not updated successfully.";
                return View(requestSetOutlet);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult EditOutlet(long outletID, bool statusOutlet)
        {
            MIM.Outlet responseGetOutlet = new MIM.Outlet();


            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Edit Outlet";

            //if (string.IsNullOrEmpty(outletName))
            if (outletID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            responseGetOutlet = outletDAL.GetOutlet(outletID: outletID,statusOutlet:statusOutlet);
           
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true,selectedClientID:responseGetOutlet.clientID,selectedClientName:responseGetOutlet.clientName);
            ViewBag.countryName = countryDAL.GetCountryNameList(selectedCountryID:responseGetOutlet.countryID, selectedCountryName:responseGetOutlet.countryName);
            ViewBag.cityName = cityDAL.GetCityNameList(selectedCityID:responseGetOutlet.cityID, selectedCityName:responseGetOutlet.cityName);


            if (string.IsNullOrEmpty(responseGetOutlet.outletName))
            {
                return HttpNotFound();
            }

            return View(responseGetOutlet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOutlet(MIM.Outlet requestSetOutlet, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
            {
                return RedirectToAction("EditOutlet", "Outlet", new { outletID = requestSetOutlet.outletID });
            }

            if (command == "Close")
                return RedirectToAction("Outlet");

            bool updateOutlet = false;

            if (command == "Save")
            {
                requestSetOutlet.userName = Session["UserName"].ToString();
                ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, selectedClientID: requestSetOutlet.clientID, selectedClientName: requestSetOutlet.clientName);
                ViewBag.countryName = countryDAL.GetCountryNameList(selectedCountryID: requestSetOutlet.countryID, selectedCountryName: requestSetOutlet.countryName);
                ViewBag.cityName = cityDAL.GetCityNameList(selectedCityID: requestSetOutlet.cityID, selectedCityName: requestSetOutlet.cityName);

                if (ModelState.IsValid)
                {                    
                    updateOutlet = outletDAL.EditOutlet(requestSetOutlet);
                    if (updateOutlet == true)
                        return RedirectToAction("Outlet");
                    else
                        ViewBag.Message = "Outlet not updated successfully.";
                }
                return View(requestSetOutlet);
            }
            return RedirectToAction("Login", "Login");
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

            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetail(serialNo:serialNo);
            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true,selectedClientID:responseGetDeviceTransaction.clientID, selectedClientName:responseGetDeviceTransaction.clientName);
            ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID:responseGetDeviceTransaction.statusID, selectedStatusName:responseGetDeviceTransaction.status);
            ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID:responseGetDeviceTransaction.outletID, selectedOutletName:responseGetDeviceTransaction.outletName);
            ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner:responseGetDeviceTransaction.companyOwner);

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
                return RedirectToAction("CreateDeviceTransaction", "Outlet");

            if (command == "Save")
            {
                ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, selectedClientID: requestSetDeviceTransaction.clientID, selectedClientName: requestSetDeviceTransaction.clientName);
                ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: requestSetDeviceTransaction.statusID, selectedStatusName: requestSetDeviceTransaction.status);
                ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID: requestSetDeviceTransaction.outletID, selectedOutletName: requestSetDeviceTransaction.outletName);
                ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: requestSetDeviceTransaction.companyOwner);

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
                        return RedirectToAction("DeviceHistory", "Outlet", new { outletIDSearch = requestSetDeviceTransaction.deviceID, productSerialSearch = requestSetDeviceTransaction.productSerial });
                    else
                        ViewBag.Message = "Device transaction not updated successfully.";
                }

                return View(requestSetDeviceTransaction);
            }
            return RedirectToAction("Login", "Login");
        }

        public JsonResult GetClientwiseOutletName(string clientName)
        {
            List<MIM.Outlet> responseGetOutlet = new List<MIM.Outlet>();
            responseGetOutlet = outletDAL.GetOutletList(statusOutlet:true,clientName:clientName);
            return Json(responseGetOutlet, JsonRequestBehavior.AllowGet);
            //return View(responseGetDevice);          
        }

        public JsonResult GetOutletnamewiseOutlet(string outletName)
        {
            MIM.Outlet responseGetOutlet = new MIM.Outlet();

            responseGetOutlet = outletDAL.GetOutlet(outletName:outletName,statusOutlet:true);
            return Json(responseGetOutlet, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult GetStatusnamewiseStatus(string statusName)
        {
            MIM.Status responseGetStatus = new MIM.Status();

            responseGetStatus = statusDAL.GetStatusID(statusName);
            return Json(responseGetStatus, JsonRequestBehavior.AllowGet);
        }

    }
}