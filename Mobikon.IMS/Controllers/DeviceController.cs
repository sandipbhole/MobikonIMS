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
    public class DeviceController : Controller
    {
        MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
        MID.ClientDAL clientDAL = new MID.ClientDAL();
        MID.StatusDAL statusDAL = new MID.StatusDAL();
        MID.OutletDAL outletDAL = new MID.OutletDAL();
        MID.CityDAL cityDAL = new MID.CityDAL();
        MID.CountryDAL countryDAL = new MID.CountryDAL();

        // GET: Device
        public ActionResult Device(string sortOrder,string deviceTypeFilter, string productSerialFilter,string deviceDetailsFilter, string statusDeviceFilter, string deviceTypeSearch,string productSerialSearch,  string deviceDetailsSearch, int? page, string statusDeviceSearch, string paging)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");         

            List<MIM.Device> responseGetDevice = new List<MIM.Device>();
            List<MIM.Device> responseDevice = new List<MIM.Device>();

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Device List";

     

            if (!string.IsNullOrEmpty(statusDeviceSearch))
                ViewBag.statusDeviceSearch = statusDAL.GetStatusNameList(pageName: "Device",role:Session["Role"].ToString());
            else
                ViewBag.statusDeviceSearch = statusDAL.GetStatusNameList(pageName: "Device", selectedStatusName: statusDeviceFilter, role: Session["Role"].ToString());

            if (!string.IsNullOrEmpty(deviceTypeSearch))
                ViewBag.deviceTypeSearch = deviceDAL.GetDeviceTypeList(pageName: "Device");
            else
                ViewBag.deviceTypeSearch = deviceDAL.GetDeviceTypeList(pageName: "Device", selectedDeviceType:deviceTypeFilter);



            ViewBag.currentSort = sortOrder;
            //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

            if (String.IsNullOrEmpty(sortOrder))
                sortOrder = "createDateSortDesc";

            ViewBag.createDateSort = sortOrder == "createDateSortDesc" ? "createDateSort" : "createDateSortDesc";
            ViewBag.deviceTypeSort = sortOrder == "deviceTypeSort" ? "deviceTypeSortDesc" : "deviceTypeSort";
            ViewBag.productSerialSort = sortOrder == "productSerialSort" ? "productSerialSortDesc" : "productSerialSort";
            ViewBag.deviceDetailsSort = sortOrder == "deviceDetailsSort" ? "deviceDetailsSortDesc" : "deviceDetailsSort";
            ViewBag.statusSort = sortOrder == "statusDeviceSort" ? "statusDeviceSortDesc" : "statusDeviceSort";


            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging); 

            if (productSerialSearch != null)                
                page = 1;
            if (statusDeviceSearch != null)
                page = 1;
            if (deviceDetailsSearch != null)
                page = 1;
            if (deviceTypeSearch != null)
                page = 1;

            if (!string.IsNullOrEmpty(productSerialSearch))
                ViewBag.productSerialFilter = productSerialSearch;
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
            if (Session["Role"].ToString() != "Sales")
            {
                if (!string.IsNullOrEmpty(statusDeviceSearch))
                    ViewBag.statusDeviceFilter = statusDeviceSearch;
                else
                {
                    statusDeviceSearch = statusDeviceFilter;
                    ViewBag.statusDeviceFilter = statusDeviceSearch;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(statusDeviceSearch))
                {
                    statusDeviceSearch = "In Stock";
                    ViewBag.statusDeviceFilter = statusDeviceSearch;
                }
                else if (!string.IsNullOrEmpty(statusDeviceSearch))
                {                   
                    ViewBag.statusDeviceFilter = statusDeviceSearch;
                }
                else
                {
                    statusDeviceSearch = statusDeviceFilter;
                    ViewBag.statusDeviceFilter = statusDeviceSearch;
                }
            }

            if (!string.IsNullOrEmpty(deviceTypeSearch))
                ViewBag.deviceTypeFilter = deviceTypeSearch;
            else
            {
                deviceTypeSearch = deviceTypeFilter;
                ViewBag.deviceTypeFilter = deviceTypeSearch;
            }



            //if (!string.IsNullOrEmpty(productSerialSearch))
            //    ViewBag.productSerialFilter = productSerialSearch;
            //if (!string.IsNullOrEmpty(deviceDetailsSearch))
            //    ViewBag.deviceDetailsFilter = deviceDetailsSearch;
            //if (!string.IsNullOrEmpty(statusDeviceSearch))
            //    ViewBag.statusDeviceFilter = statusDeviceSearch;
            //if (!string.IsNullOrEmpty(deviceTypeSearch))
            //    ViewBag.deviceTypeFilter = deviceTypeSearch;


            responseGetDevice = deviceDAL.GetDeviceList(statusDevice:mC.CommonMobikonIMS.StatusDevice.All.ToString(),showBlockedDevice: true);
            var devices = responseGetDevice.AsQueryable();

            if (!string.IsNullOrEmpty(deviceDetailsSearch))
            {
                devices = devices.Where(device => device.deviceDetails.Trim().ToUpper().Contains(deviceDetailsSearch.Trim().ToUpper()));
            }

            if (!string.IsNullOrEmpty(productSerialSearch))
            {
                devices = devices.Where(device => device.productSerial.Trim().ToUpper().Contains(productSerialSearch.Trim().ToUpper()));
                //if (productSerialSearch != "All")
                //{
                //    deviceDetailsSearch = string.Empty;
                //    statusDeviceSearch = "All";
                //    devices = devices.Where(device => device.productSerial.Trim().Contains(productSerialSearch.Trim()));
                //}
            }
            if (!string.IsNullOrEmpty(deviceTypeSearch))
            {
                if (deviceTypeSearch != "All")
                    devices = devices.Where(device => device.deviceType.ToUpper().Contains(deviceTypeSearch.ToUpper()));
            }

            if (!string.IsNullOrEmpty(statusDeviceSearch))
            {
                if (statusDeviceSearch != "All")
                    devices = devices.Where(device => device.status.ToUpper().Contains(statusDeviceSearch.ToUpper()));

                //if (statusDeviceSearch == "All")
                //{
                //    devices = devices.Where(device => device.status != mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString());                      
                //}
            }                 

            switch (sortOrder)
            {
                case "createDateSort":
                    devices = devices.OrderBy(device => device.createdDate);
                    break;
                case "productSerialSortDesc":
                    devices = devices.OrderByDescending(device => device.productSerial);
                    break;
                case "productSerialSort":
                    devices = devices.OrderBy(device => device.productSerial);
                    break;
                case "deviceTypeSortDesc":
                    devices = devices.OrderByDescending(device => device.deviceType);
                    break;
                case "deviceTypSorte":
                    devices = devices.OrderBy(device => device.deviceType);
                    break;
                case "deviceDetailsSortDesc":
                    devices = devices.OrderByDescending(device => device.deviceDetails);
                    break;
                case "deviceDetailsSort":
                    devices = devices.OrderBy(device => device.deviceDetails);
                    break;
                case "statusDeviceSortDesc":
                    devices = devices.OrderByDescending(device => device.status);
                    break;               
                case "statusDeviceSort":
                    devices = devices.OrderBy(device => device.status);
                    break;
                default:  // createDateSortDesc                      
                    devices = devices.OrderByDescending(device => device.createdDate);
                    break;
            }

            foreach (var response in devices)
            {
                responseDevice.Add(new MIM.Device()
                {
                    deviceID = response.deviceID,
                    productSerial = response.productSerial,
                    deviceDetails = response.deviceDetails,
                    status = response.status,
                    note = response.note,
                    companyOwner = response.companyOwner,
                    userName = response.userName ,
                    statusID = response.statusID ,
                    userID = response .userID ,
                    blockedDate =response.blockedDate,
                    deviceTag = response.deviceTag ,
                    deviceType = response.deviceType ,
                    currentStatus = response.currentStatus  ,
                    serialNo = response.serialNo,
                    seperator = mC.CommonMobikonIMS.seperator
                });
            }
            ViewBag.deviceCount = "Total Device: " + responseDevice.Count;

            int pageNumber = (page ?? 1);
            return View(responseDevice.ToPagedList(pageNumber, pageSize));  
        }

        public ActionResult UnblockDevice(string productSerial, string statusDevice)
        {
            MIM.Device responseGetDevice = new MIM.Device();
            MIM.DeviceTransaction responseGetDeviceTransaction = new MIM.DeviceTransaction();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (string.IsNullOrEmpty(productSerial))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true, selectedClientID: responseGetDeviceTransaction.clientID, selectedClientName: responseGetDeviceTransaction.clientName);
        
            ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID: responseGetDeviceTransaction.outletID, selectedOutletName:responseGetDeviceTransaction.outletName);

            responseGetDevice = deviceDAL.GetDevice(productSerial:productSerial , statusDevice:statusDevice);

            responseGetDeviceTransaction.productSerial = responseGetDevice.productSerial;
            responseGetDeviceTransaction.deviceDetails = responseGetDevice.deviceDetails;
            responseGetDeviceTransaction.deviceID = responseGetDevice.deviceID;

            ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID:responseGetDevice.statusID, selectedStatusName:responseGetDevice.status);
            if (string.IsNullOrEmpty(responseGetDevice.productSerial))
            {
                return HttpNotFound();
            }

            return View(responseGetDeviceTransaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnblockDevice(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("EditDevice");

            if (command == "Close")
                return RedirectToAction("Device");

            if (command == "Save")
            {
                requestSetDeviceTransaction.userName = Session["UserName"].ToString();
                if (ModelState.IsValid)
                {
                    bool result = deviceDAL.InsertDeviceTransaction(requestSetDeviceTransaction);
                    if (result == true)
                        return RedirectToAction("Device");
                }
                ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true, selectedClientID: requestSetDeviceTransaction.clientID, selectedClientName:requestSetDeviceTransaction.clientName);               
                ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID:requestSetDeviceTransaction.outletID, selectedOutletName: requestSetDeviceTransaction.outletName);

                ViewBag.Message = "Device not unblocked successfully.";
                return View(requestSetDeviceTransaction);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult BlockDevice(string productSerial,string statusDevice)
        {
            MIM.Device responseGetDevice = new MIM.Device();
            MIM.DeviceTransaction responseGetDeviceTransaction = new MIM.DeviceTransaction();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (string.IsNullOrEmpty(productSerial))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true, selectedClientID: responseGetDeviceTransaction.clientID, selectedClientName:responseGetDeviceTransaction.clientName);
          
            ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID:responseGetDeviceTransaction.outletID, selectedOutletName:responseGetDeviceTransaction.outletName);

            responseGetDevice = deviceDAL.GetDevice(productSerial:productSerial, statusDevice:statusDevice);

            responseGetDeviceTransaction.productSerial = responseGetDevice.productSerial;
            responseGetDeviceTransaction.deviceDetails = responseGetDevice.deviceDetails;
            responseGetDeviceTransaction.deviceID = responseGetDevice.deviceID;

            ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: responseGetDevice.statusID, selectedStatusName: responseGetDevice.status);
            if (string.IsNullOrEmpty(responseGetDevice.productSerial))
            {
                return HttpNotFound();
            }

            return View(responseGetDeviceTransaction);
        }

        public ActionResult BlockUnblockDevice(string devices, int? page)
        {
            bool result = false;

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");
            if (string.IsNullOrEmpty(devices))
                return RedirectToAction("Device");

            List<MIM.DeviceTransaction> requestDeviceTransaction = new List<MIM.DeviceTransaction>();

            if (page == null)
                page = 1;

            string[] DeviceList = devices.Split(',');           

            foreach (var device in DeviceList)
            {
                var ValueAndStatus = device.ToString().Split('|');
                if (ValueAndStatus.Length > 0)
                {
                    string status = ValueAndStatus[2] == "Blocked" ? "In Stock" : "Blocked";

                    if (status == "Blocked")
                    {
                        //if (DeviceList.Length > 6 )
                        //{
                        //    ViewBag.Message = "You cannot block more than 6 devices. Since you have already blocked 6 devices.";
                        //    return RedirectToAction("Device");
                        //}

                        int totalBlockedDevice = deviceDAL.BlockedDevicesCount(Session["UserName"].ToString());
                        if (totalBlockedDevice + DeviceList.Length > 6)
                        {
                            Session["ValidationMessage"] = "You cannot block more than 6 devices. Since you have already blocked 6 devices.";
                            return RedirectToAction("Device");
                        }
                    }
                }
            }

            foreach (var device in DeviceList)
            {
                var ValueAndStatus = device.ToString().Split('|');
                if (ValueAndStatus.Length > 0)
                {
                    MIM.DeviceTransaction requestSetDeviceTransaction = new MIM.DeviceTransaction();

                    requestSetDeviceTransaction.userName = Session["UserName"].ToString();
                    requestSetDeviceTransaction.deviceID = Convert.ToInt64(ValueAndStatus[0]);
                    requestSetDeviceTransaction.productSerial = ValueAndStatus[1].ToString();
                    requestSetDeviceTransaction.deviceDetails = ValueAndStatus[3].ToString();
                    requestSetDeviceTransaction.outletID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["OutletID"]);
                    requestSetDeviceTransaction.outletName = System.Configuration.ConfigurationManager.AppSettings["OutletName"];
                    string status = ValueAndStatus[2] == "Blocked" ? "In Stock" : "Blocked";
                    requestSetDeviceTransaction.status = status;                   

                    if (ModelState.IsValid)
                    {
                        if (true == deviceDAL.InsertDeviceTransaction(requestSetDeviceTransaction: requestSetDeviceTransaction))
                        {
                            requestDeviceTransaction.Add(new MIM.DeviceTransaction()
                            {
                                deviceID = requestSetDeviceTransaction.deviceID,
                                productSerial = requestSetDeviceTransaction.productSerial,
                                outletID = requestSetDeviceTransaction.outletID,
                                deviceDetails = requestSetDeviceTransaction.deviceDetails,
                                outletName = requestSetDeviceTransaction.outletName,
                                status = requestSetDeviceTransaction.status,
                                userName = requestSetDeviceTransaction.userName
                            });

                            result = true;
                        }                        
                    }
                }
            }

            deviceDAL.SendEmail(requestSetDeviceTransaction: requestDeviceTransaction);
            if (result == true)
                return RedirectToAction("Device", new { page = page });

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BlockDevice(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("EditDevice");

            if (command == "Close")
                return RedirectToAction("Device");

            if (command == "Save")
            {
                requestSetDeviceTransaction.userName = Session["UserName"].ToString();
                if (ModelState.IsValid)
                {
                    bool result = deviceDAL.InsertDeviceTransaction(requestSetDeviceTransaction);
                    if (result == true)
                        return RedirectToAction("Device");
                }
                ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true, selectedClientID:requestSetDeviceTransaction.clientID, selectedClientName:requestSetDeviceTransaction.clientName);
              
                ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID:requestSetDeviceTransaction.outletID, selectedOutletName: requestSetDeviceTransaction.outletName);

                ViewBag.Message = "Device not blocked successfully.";
                return View(requestSetDeviceTransaction);
            }
            return RedirectToAction("Login", "Login");
        }
        
        public void ExportToPDF(string productSerial)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: productSerial);

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=DeviceInventory.pdf");
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

        public void ExportToExcel(string productSerial)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: productSerial);
            grid.DataSource = responseGetDeviceTransactionRpt;
            grid.DataBind();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=DeviceInventory.xls");
            Response.ContentType = "application/excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Write(sw.ToString());

            Response.End();
        }

        public ActionResult DeviceHistory(string sortOrder, string productSerialFilter,string deviceDetailsFilter, string clientNameFilter, string outletNameFilter, string statusFilter, string cityNameFilter, string productSerialSearch,string deviceDetailsSearch, string clientNameSearch, string outletNameSearch, string statusSearch,string cityNameSearch, int? page, string paging)
        {
            if (Session["UserName"] == null)          
                return RedirectToAction("Login", "Login");

    
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Device History";

            Session["ClientName"] = null;
            Session["OutletName"] = null;

            if (Session["ProductSerial"] == null)
                Session["ProductSerial"] = productSerialSearch;

            if (string.IsNullOrEmpty(productSerialSearch))
                productSerialSearch = Session["ProductSerial"].ToString();

            if (Session["ProductSerial"].ToString() != productSerialSearch)
                Session["ProductSerial"] = productSerialSearch;

            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
            List<MIM.DeviceTransaction> responseDeviceTransaction = new List<MIM.DeviceTransaction>();

            ViewBag.clientNameSearch = clientDAL.GetClientNameList(statusClient:true, pageName: "DeviceHistory");
            ViewBag.outletNameSearch = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
            ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory");
            ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory");

            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "createdDateSortDesc";

            ViewBag.deliveryDateSort = sortOrder== "deliveryDateSortDesc" ? "deliveryDateSort" : "deliveryDateSort";
            ViewBag.clientNameSort = sortOrder == "clientNameSort" ? "clietNameSortDesc" : "clientNameSort";
            ViewBag.outletNameSort = sortOrder == "outletNameSort" ? "outletNameSortDesc" : "outletNameSort";
            ViewBag.cityNameSort = sortOrder == "cityNameSort" ? "cityNameSortDesc" : "cityNameSort";
            ViewBag.stausSort = sortOrder == "statusSort" ? "statusSortDesc" : "statuseSort";

            ViewBag.currentSort = sortOrder;
           // ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            if (!string.IsNullOrEmpty(productSerialSearch))
                page = 1;         
            if (!string.IsNullOrEmpty(clientNameSearch))
                page = 1;
            if (!string.IsNullOrEmpty(outletNameSearch))
                page = 1;
            if (!string.IsNullOrEmpty(cityNameSearch))
                page = 1;
            if (!string.IsNullOrEmpty(statusSearch))
                page = 1;

            if (!string.IsNullOrEmpty(productSerialSearch))
                ViewBag.productSerialFilter = productSerialSearch;          
            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.clientNameFilter = clientNameSearch;
            if (!string.IsNullOrEmpty(outletNameSearch))
                ViewBag.outletNameFilter = outletNameSearch;
            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameFilter = cityNameSearch;
            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.countryNameFilter = statusSearch;

            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionHistory(productSerial:productSerialSearch);
            var deviceTransactions = responseGetDeviceTransaction.AsQueryable();
         
            if (!string.IsNullOrEmpty(clientNameSearch))
            {
                if (clientNameSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.clientName.ToUpper().Trim().Contains(clientNameSearch.ToUpper().Trim()));
            }
            if (!string.IsNullOrEmpty(outletNameSearch))
            {
                if (outletNameSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.outletName.ToUpper().Trim().Contains(outletNameSearch.ToUpper().Trim()));
            }
            if (!string.IsNullOrEmpty(statusSearch))
            {
                if (statusSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.status.ToUpper().Trim() == statusSearch.ToUpper().Trim());
            }
            if (!string.IsNullOrEmpty(cityNameSearch))
            {
                if (cityNameSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.clientCityName.ToUpper().Trim() == cityNameSearch.ToUpper().Trim());
            }

            switch (sortOrder)
            {
                case "createdDateSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.createdDate);
                    break;
                case "clientNameSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.clientName);
                    break;
                case "clientNameSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.clientName);
                    break;
                case "cityNameSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.clientCityName);
                    break;
                case "cityNameSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.clientCityName);
                    break;
                case "statusSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.status);
                    break;
                case "statusSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.status);
                    break;
                case "deviceDetailsSort":
                    deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.deviceDetails);
                    break;
                case "deviceDetailsSortDesc":
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.deviceDetails);
                    break;
                default:  // createdDateSortDesc
                    deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.createdDate);
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

        public ActionResult ShowDevices(string sortOrder, string productSerialFilter, string deviceDetailsFilter, string clientNameFilter, string outletNameFilter,string statusFilter, string productSerialSearch, string deviceDetailsSearch,  string clientNameSearch, string outletNameSearch, string statusSearch, int? page, string paging)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];

                ViewBag.menuName = "Device List";

                List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
                List<MIM.DeviceTransaction> responseDeviceTransaction = new List<MIM.DeviceTransaction>();

                int pageSize = 0;
                if (string.IsNullOrEmpty(paging))
                    pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); // mC.CommonMobikonIMS.selectedPageSize;
                else
                    pageSize = Convert.ToInt16(paging);

                if (productSerialSearch != null)
                    page = 1;
                if (deviceDetailsSearch != null)
                    page = 1;
                if (clientNameSearch != null)
                    page = 1;
                if (outletNameSearch != null)
                    page = 1;
                if (statusSearch != null)
                    page = 1;

                if (Session["Status"] == null)
                    Session["Status"] = statusSearch;

                if (string.IsNullOrEmpty(statusSearch))
                    statusSearch = Session["Status"].ToString();

                if (Session["Status"].ToString() != statusSearch)
                    Session["Status"] = statusSearch;


                if (!string.IsNullOrEmpty(clientNameSearch))
                    ViewBag.clientNameSearch = clientDAL.GetClientNameList(pageName: "DeviceHistory");
                else
                    ViewBag.clientNameSearch = clientDAL.GetClientNameList(pageName: "DeviceHistory", selectedClientName: clientNameFilter);

                if (!string.IsNullOrEmpty(outletNameSearch))
                    ViewBag.outletNameSearch = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
                else
                   ViewBag.outletNameSearch = outletDAL.GetOutletNameList(pageName: "DeviceHistory", selectedOutletName: outletNameFilter);

                ViewBag.currentSort = sortOrder;
               // ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

                if (string.IsNullOrEmpty(sortOrder))
                    sortOrder = "deliveryDateSortDesc";

                ViewBag.deliveryDateSort = sortOrder == "deliveryDateSortDesc" ? "deliveryDateSort" : "deliveryDateSortDesc";
                ViewBag.productSerialSort = sortOrder == "productSerialSort" ? "productSerialSortDesc" : "productSerialSort";
                ViewBag.clientNameSort = sortOrder == "clientNameSort" ? "clietNameSortDesc" : "clientNameSort";
                ViewBag.outletNameSort = sortOrder == "outletNameSort" ? "outletNameSortDesc" : "outletNameSort";  

                if (!string.IsNullOrEmpty(productSerialSearch))
                    ViewBag.productSerialFilter = productSerialSearch;
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

                if (!string.IsNullOrEmpty(clientNameSearch))
                    ViewBag.clientNamelFilter = clientNameSearch;
                else
                {
                    clientNameSearch = clientNameFilter;
                    ViewBag.clientNameFilter = clientNameSearch;
                }

                if (!string.IsNullOrEmpty(statusSearch))
                    ViewBag.statuslFilter = statusSearch;
                else
                {
                    statusSearch = statusFilter;
                    ViewBag.statusFilter = statusSearch;
                }


                responseGetDeviceTransaction = deviceDAL.GetStatusDeviceList(statusSearch);
                var deviceTransactions = responseGetDeviceTransaction.AsQueryable();

                if (!string.IsNullOrEmpty(productSerialSearch))
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.productSerial.ToUpper().Trim().Contains(productSerialSearch.ToUpper().Trim()));

                if (!string.IsNullOrEmpty(deviceDetailsSearch))
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.deviceDetails.ToUpper().Trim().Contains(deviceDetailsSearch.ToUpper().Trim()));

                if (!string.IsNullOrEmpty(clientNameSearch))
                {
                    if (clientNameSearch != "All")
                        deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.clientName.ToUpper().Trim().Contains(clientNameSearch.ToUpper().Trim()));
                }
                if (!string.IsNullOrEmpty(outletNameSearch))
                {
                    if (outletNameSearch != "All")
                        deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.outletName.ToUpper().Trim().Contains(outletNameSearch.ToUpper().Trim()));
                }
                
                switch (sortOrder)
                {
                    case "deliveryDateSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.deliveryDate);
                        break;
                    case "clientNameSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.clientName);
                        break;
                    case "clientNameSortDesc":
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.clientName);
                        break;                    
                    case "deviceDetailsSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.deviceDetails);
                        break;
                    case "deviceDetailsSortDesc":
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.deviceDetails);
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
                        currentStatus = response.currentStatus,
                        seperator = mC.CommonMobikonIMS.seperator
                    });
                }

                ViewBag.deviceCount = "Total Device: " + responseDeviceTransaction.Count;
                int pageNumber = (page ?? 1);
                return View(responseDeviceTransaction.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Login", "Login");

            }
        }

        public ActionResult DeviceTransaction(string sortOrder, string productSerialFilter, string deviceDetailsFilter, string cityNameFilter,  string clientNameFilter, string outletNameFilter,string statusFilter, string productSerialSearch, string deviceDetailsSearch, string outletNameSearch, string clientNameSearch, string cityNameSearch,string statusSearch, int? page, string paging)
        {
            if (Session["UserName"] != null)
            {
                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];

                ViewBag.menuName = "Device Inventory List";

                List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
                List<MIM.DeviceTransaction> responseDeviceTransaction = new List<MIM.DeviceTransaction>();


                if (!string.IsNullOrEmpty(clientNameSearch))                
                    ViewBag.clientNameSearch = clientDAL.GetClientNameList(pageName: "DeviceHistory");
                else
                    ViewBag.clientNameSearch = clientDAL.GetClientNameList(pageName: "DeviceHistory",selectedClientName: clientNameFilter);

                if (!string.IsNullOrEmpty(outletNameSearch))
                    ViewBag.outletNameSearch = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
                else
                    ViewBag.outletNameSearch = outletDAL.GetOutletNameList(pageName: "DeviceHistory", selectedOutletName: outletNameFilter);

                if (!string.IsNullOrEmpty(cityNameSearch))
                    ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory");
                else
                    ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "DeviceHistory", selectedCityName: cityNameFilter);

                if (!string.IsNullOrEmpty(statusSearch))
                    ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory");
                else
                    ViewBag.statusSearch = statusDAL.GetStatusNameList(pageName: "DeviceHistory", selectedStatusName:statusFilter);

                ViewBag.currentSort = sortOrder;               
                //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();
           
                if (string.IsNullOrEmpty(sortOrder))
                    sortOrder = "createdDateSortDesc";

                ViewBag.deliveryDateSort = sortOrder == "createdDateSortDesc"? "createdDateSort" : "createdDateSortDesc";
                ViewBag.productSerialSort = sortOrder == "productSerialSort" ? "productSerialSortDesc" : "productSerialSort";
                ViewBag.clientNameSort = sortOrder == "clientNameSort" ? "clietNameSortDesc" : "clientNameSort";
                ViewBag.outletNameSort = sortOrder == "outletNameSort" ? "outletNameSortDesc" : "outletNameSort";
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
                if (!string.IsNullOrEmpty(clientNameSearch))
                    page = 1;
                if (!string.IsNullOrEmpty(outletNameSearch))
                    page = 1;
                if (!string.IsNullOrEmpty(cityNameSearch))
                    page = 1;
                if (!string.IsNullOrEmpty(statusSearch))
                    page = 1;

                if (!string.IsNullOrEmpty(productSerialSearch))
                    ViewBag.productSerialFilter = productSerialSearch;
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
                if (!string.IsNullOrEmpty(statusSearch))
                    ViewBag.statusFilter = statusSearch;
                else
                {
                    statusSearch = statusFilter;
                    ViewBag.statusFilter = statusSearch;
                }
                if (!string.IsNullOrEmpty(outletNameSearch))
                    ViewBag.outletNameFilter = outletNameSearch;
                else
                {
                    outletNameSearch = outletNameFilter;
                    ViewBag.outletNameFilter = outletNameSearch;
                }

                if (!string.IsNullOrEmpty(clientNameSearch))
                    ViewBag.clientNamelFilter = clientNameSearch;
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


                responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetailList(productSerial:productSerialSearch);
                var deviceTransactions = responseGetDeviceTransaction.AsQueryable();

                if (!string.IsNullOrEmpty(deviceDetailsSearch))              
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.deviceDetails.ToUpper().Trim().Contains(deviceDetailsSearch.ToUpper().Trim()));
              
                if (!string.IsNullOrEmpty(clientNameSearch))
                {
                    if (clientNameSearch != "All")
                        deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.clientName.ToUpper().Trim().Contains(clientNameSearch.ToUpper().Trim()));
                }
                if (!string.IsNullOrEmpty(outletNameSearch))
                {
                    if (outletNameSearch != "All")
                        deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.outletName.ToUpper().Trim().Contains(outletNameSearch.ToUpper().Trim()));
                }
                if (!string.IsNullOrEmpty(statusSearch))
                {
                    if (statusSearch != "All")
                        deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.status.ToUpper().Trim() == statusSearch.ToUpper().Trim());
                }
                if (!string.IsNullOrEmpty(cityNameSearch))
                {
                    if (cityNameSearch != "All")
                        deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.cityName.ToUpper().Trim() == cityNameSearch.ToUpper().Trim());
                }
                switch (sortOrder)
                {
                    case "createdDateSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.createdDate);
                        break;
                    case "clientNameSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.clientName);
                        break;
                    case "clientNameSortDesc":
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.clientName);
                        break;
                    case "cityNameSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.cityName);
                        break;
                    case "cityNameSortDesc":
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.cityName);
                        break;
                    case "statusSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.status);
                        break;
                    case "statusSortDesc":
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.status);
                        break;
                    case "deviceDetailsSort":
                        deviceTransactions = deviceTransactions.OrderBy(deviceTransaction => deviceTransaction.deviceDetails);
                        break;
                    case "deviceDetailsSortDesc":
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.deviceDetails);
                        break;
                    default:  // createdDateSortDesc
                        deviceTransactions = deviceTransactions.OrderByDescending(deviceTransaction => deviceTransaction.createdDate);
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
                        currentStatus = response.currentStatus,
                        seperator = mC.CommonMobikonIMS.seperator
                    });
                }

                ViewBag.deviceCount = "Total Device: " + responseDeviceTransaction.Count;
                int pageNumber = (page ?? 1);
                return View(responseDeviceTransaction.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return RedirectToAction("Login", "Login");

            }
        }

        public ActionResult CreateDevice()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            MID.StatusDAL statusDAL = new MID.StatusDAL();

            ViewBag.deviceType = deviceDAL.GetDeviceTypeList();
            ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList();

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Add Device";

            ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusName:"In Stock");
            return View(); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDevice(MIM.Device requestSetDevice, string command)
        {
            bool result = false;
            List<MIM.DeviceTransaction> requestSetDeviceTransaction = new List<MIM.DeviceTransaction>();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("CreateDevice");

            if (command == "Close")
                return RedirectToAction("MobikonIMS", "MobikonIMS");

            if (command == "Save")
            {
                //requestSetDevice.status = "In Stock";
                requestSetDevice.userName = Session["UserName"].ToString();

                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];
                ViewBag.menuName = "Add Device";

                ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: requestSetDevice.statusID, selectedStatusName: requestSetDevice.status);
                ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: requestSetDevice.companyOwner);
                ViewBag.deviceType = deviceDAL.GetDeviceTypeList(selectedDeviceType: requestSetDevice.deviceType);

                if (ModelState.IsValid)
                {
                    //if ((Session["Role"].ToString() != "Sales"|| Session["Role"].ToString() != "Accounts" || Session["Role"].ToString() != "Administrator") && requestSetDevice.status == "Blocked")                    
                    //{
                    //    ViewBag.message = "Not authorized to block this device.";
                    //    return View(requestSetDevice);
                    //}

                    if (true == deviceDAL.CheckDeviceExist(requestSetDevice.productSerial))
                    {
                        ViewBag.message = "Device already exists.";
                        return View(requestSetDevice);
                    }

                    if (requestSetDevice.status == "Blocked")
                    {
                        if (Session["Role"].ToString() == "Accounts")
                        {
                            ViewBag.message = "Not authorized to block this device.";
                            return View(requestSetDevice);
                        }

                        int totalBlockedDevice = deviceDAL.BlockedDevicesCount(Session["UserName"].ToString());
                        if (totalBlockedDevice > 6)
                        {
                            ViewBag.message = "You cannot block more than 6 devices. Since you have already blocked 6 devices.";
                            return View(requestSetDevice);
                        }
                    }

                    result = deviceDAL.InsertDevice(requestSetDevice);
                    if (result == true)
                    {
                        if (requestSetDevice.status == "Blocked" || requestSetDevice.status == "In Stock" || requestSetDevice.status == "Deployed-Active")
                        {
                            //Converting requestSetDeviceTransaction object requestDeviceTransaction List Object for SendEmail
                            requestSetDeviceTransaction.Add(new MIM.DeviceTransaction
                            {
                                status = requestSetDevice.status,
                                //statusID = statusDAL.FindStatusID(requestSetDevice.status),
                                deviceDetails = requestSetDevice.deviceDetails,
                                deviceID = requestSetDevice.deviceID,
                                productSerial = requestSetDevice.productSerial,
                                //userID = userDAL.FindUserID(requestSetDevice.userName),
                                userName = requestSetDevice.userName,
                                deviceType = requestSetDevice.deviceType,
                                createdDate = requestSetDevice.createdDate
                            });

                            deviceDAL.SendEmail(requestSetDeviceTransaction: requestSetDeviceTransaction);
                        }
                    }
                    else
                    {
                        ViewBag.message = "Device not saved successfully.";
                    }
                    return RedirectToAction("Device");
                }
                return View(requestSetDevice);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult CreateDeviceTransaction()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.dcFile = "";
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Add Device Inventory";


            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true);
            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString(),showBlockedDevice:true);
            ViewBag.statusName = statusDAL.GetStatusNameList();
            ViewBag.outletName = outletDAL.GetOutletNameList();

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Yes", Value = "Yes", Selected = true });
            items.Add(new SelectListItem { Text = "No", Value = "No" });
            items.Add(new SelectListItem { Text = "Pending", Value = "Pending" });

            ViewBag.companyOwner = items;

            return View();
        }

       
        public FileResult DisplayPDF(string filePath)
        {
            //if (System.IO.File.Exists(filePath))
            //{
                return File(filePath, "application/pdf");
            //}           
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDeviceTransaction(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<MIM.DeviceTransaction> requestDeviceTransaction = new List<MIM.DeviceTransaction>();
            List<SelectListItem> items = new List<SelectListItem>();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("CreateDevice");

            if (command == "Close")
                return RedirectToAction("Device");          

            if (command == "Save")
            {
                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];
                ViewBag.menuName = "Add Device Inventory";

                ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, selectedClientID: requestSetDeviceTransaction.clientID, selectedClientName: requestSetDeviceTransaction.clientName);
                ViewBag.productSerial = deviceDAL.GetProductSerialList(selectedDeviceID: requestSetDeviceTransaction.deviceID, selectedProductSerial: requestSetDeviceTransaction.productSerial, statusDevice: mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString(), showBlockedDevice: true);
                ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: requestSetDeviceTransaction.statusID, selectedStatusName: requestSetDeviceTransaction.status);
                ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID: requestSetDeviceTransaction.outletID, selectedOutletName: requestSetDeviceTransaction.outletName);
                ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: requestSetDeviceTransaction.companyOwner);

                if (ModelState.IsValid)
                {
                    requestSetDeviceTransaction.userName = Session["UserName"].ToString();
                    
                    foreach (string upload in Request.Files)
                    {
                        string path = deviceDAL.GetDCFileFolderPath();
                        bool folderExists = Directory.Exists(path);
                        if (!folderExists)
                            Directory.CreateDirectory(path);

                        if (!string.IsNullOrEmpty(Request.Files[upload].FileName))
                        {
                            Session["DCFile"] = Request.Files[upload].FileName;

                            if (string.IsNullOrEmpty(requestSetDeviceTransaction.dc))
                            {
                                ViewBag.Message = "Please provide delivery challan.";
                                return View(requestSetDeviceTransaction);
                            }

                            string fileName = requestSetDeviceTransaction.dc + Path.GetExtension(Request.Files[upload].FileName);//Path.GetFileName(Request.Files[upload].FileName);
                            Request.Files[upload].SaveAs(Path.Combine(path, fileName));
                            requestSetDeviceTransaction.dcFile = Path.Combine(path, fileName);
                        }
                        else if (!string.IsNullOrEmpty(Session["DCFile"].ToString()))
                        {
                            if (string.IsNullOrEmpty(requestSetDeviceTransaction.dc))
                            {
                                ViewBag.Message = "Please provide delivery challan.";
                                return View(requestSetDeviceTransaction);
                            }
                            string fileName = requestSetDeviceTransaction.dc + Path.GetExtension(Session["DCFile"].ToString());//Path.GetFileName(Request.Files[upload].FileName);
                            Request.Files[upload].SaveAs(Path.Combine(path, fileName));
                            requestSetDeviceTransaction.dcFile = Path.Combine(path, fileName);
                        }
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

                    //    if (requestSetDeviceTransaction.rdcDate.ToString().Length <= 0)
                    //    {
                    //        if (string.IsNullOrEmpty(requestSetDeviceTransaction.rdc))
                    //        {
                    //            ViewBag.Message = "Please provide rdc.";
                    //            return View(requestSetDeviceTransaction);
                    //        }
                    //    }

                    //    if (!string.IsNullOrEmpty(requestSetDeviceTransaction.rdc))
                    //    {
                    //        if (requestSetDeviceTransaction.rdcDate.ToString().Length <= 0)
                    //        {
                    //            ViewBag.Message = "Please provide rdc date.";
                    //            return View(requestSetDeviceTransaction);
                    //        }
                    //    }

                    //    if (requestSetDeviceTransaction.hicDate.ToString().Length <= 0)
                    //    {
                    //        if (string.IsNullOrEmpty(requestSetDeviceTransaction.hic))
                    //        {
                    //            ViewBag.Message = "Please provide hic.";
                    //            return View(requestSetDeviceTransaction);
                    //        }
                    //    }

                    //    if (!string.IsNullOrEmpty(requestSetDeviceTransaction.hic))
                    //    {
                    //        if (requestSetDeviceTransaction.hicDate.ToString().Length <= 0)
                    //        {
                    //            ViewBag.Message = "Please provide hic date.";
                    //            return View(requestSetDeviceTransaction);
                    //        }
                    //    }                    

                    //if ((Session["Role"].ToString() != "Sales"|| Session["Role"].ToString() != "Operations" || Session["Role"].ToString() != "Administrator") && requestSetDeviceTransaction.status == "Blocked")
                    if (requestSetDeviceTransaction.status == "Blocked")
                    { 
                        if (Session["Role"].ToString() == "Accounts")
                        {
                            ViewBag.Message = "Not authorized to block this device.";
                            return View(requestSetDeviceTransaction);
                        }

                        int totalBlockedDevice = deviceDAL.BlockedDevicesCount(Session["UserName"].ToString());
                        if (totalBlockedDevice > 6)
                        {
                            ViewBag.message = "You cannot block more than 6 devices. Since you have already blocked 6 devices.";
                            return View(requestSetDeviceTransaction);
                        }
                    }

                    if (requestSetDeviceTransaction.status != "Blocked")
                    {
                        if (Session["UserName"].ToString() != deviceDAL.CheckBlockDeviceByUser(deviceID:requestSetDeviceTransaction.deviceID,productSerial:requestSetDeviceTransaction.productSerial, userName:requestSetDeviceTransaction.userName))
                        {
                            ViewBag.Message = "You cannot change the status of this device. This device already blocked by another user.";
                            return View(requestSetDeviceTransaction);
                        }
                    }

                    requestSetDeviceTransaction.createdDate = System.DateTime.Now;
                    bool result = deviceDAL.InsertDeviceTransaction(requestSetDeviceTransaction,"NewDeviceInventory");

                    if (result == true)
                    {
                        if (requestSetDeviceTransaction.status == "Blocked" || requestSetDeviceTransaction.status == "In Stock" || requestSetDeviceTransaction.status == "Deployed-Active")
                        {
                            requestDeviceTransaction.Add(new MIM.DeviceTransaction()
                            {
                                deviceID = requestSetDeviceTransaction.deviceID,
                                productSerial = requestSetDeviceTransaction.productSerial,
                                outletID = requestSetDeviceTransaction.outletID,
                                outletName = requestSetDeviceTransaction.outletName,
                                status = requestSetDeviceTransaction.status,
                                userName = requestSetDeviceTransaction.userName
                            });

                            deviceDAL.SendEmail(requestSetDeviceTransaction: requestDeviceTransaction);
                        }
                        return RedirectToAction("DeviceTransaction", "Device");
                    }
                    else
                    {
                        ViewBag.message = "Device transaction not saved successfully.";
                    }
                }

                Session["DCFile"] = null;
                return View(requestSetDeviceTransaction);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult DeviceDetails(long deviceID, string statusDevice)
        {
            MIM.Device responseGetDevice = new MIM.Device();
           
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
                                   
            //if (string.IsNullOrEmpty(productSerial))
            if (deviceID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Device Details";

            responseGetDevice = deviceDAL.GetDevice(deviceID:deviceID, statusDevice:statusDevice);
            responseGetDevice.seperator = mC.CommonMobikonIMS.seperator;

            if (string.IsNullOrEmpty(responseGetDevice.deviceDetails))
            {
                return HttpNotFound();
            }
            return View(responseGetDevice);              
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

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetail(serialNo:serialNo);
            responseGetDeviceTransaction.seperator = mC.CommonMobikonIMS.seperator;

            if (string.IsNullOrEmpty(responseGetDeviceTransaction.productSerial))
            {
                return HttpNotFound();
            }
            return View(responseGetDeviceTransaction);               
        }

        public ActionResult UpdateDevice()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
            
            ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList();

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.statusName = statusDAL.GetStatusNameList();
            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice:mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString(), showBlockedDevice:true);
            ViewBag.deviceType = deviceDAL.GetDeviceTypeList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDevice(MIM.Device requestSetDevice,string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("UpdateDevice");

            if (command == "Close")
                return RedirectToAction("MobikonIMS", "MobikonIMS");

            if (command == "Save")
            {
                ViewBag.productSerial = deviceDAL.GetProductSerialList(selectedDeviceID: requestSetDevice.deviceID, selectedProductSerial: requestSetDevice.productSerial, statusDevice: mC.CommonMobikonIMS.StatusDevice.NotSoldDevices.ToString(), showBlockedDevice: false);
                ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: requestSetDevice.statusID, selectedStatusName: requestSetDevice.status);
                ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: requestSetDevice.companyOwner);

                if (ModelState.IsValid)
                {
                    requestSetDevice.userName = Session["UserName"].ToString();
                    if (requestSetDevice.status == "Blocked")
                    {
                        if (Session["Role"].ToString() == "Accounts")
                        {
                            ViewBag.Message = "Not authorized to block this device.";
                            return View(requestSetDevice);
                        }
                    }

                    if (requestSetDevice.status != "Blocked")
                    {
                        if (Session["UserName"].ToString() != deviceDAL.CheckBlockDeviceByUser(deviceID: requestSetDevice.deviceID, productSerial:requestSetDevice.productSerial, userName:requestSetDevice.userName))
                        {
                            ViewBag.Message = "You cannot change the status of this device. This device already blocked by another user.";
                            return View(requestSetDevice);
                        }
                    }
                    bool result = deviceDAL.EditDevice(requestSetDevice,Session["Role"].ToString());

                    if (result == true)
                        return RedirectToAction("Device");
                }
                ViewBag.message = "Device not updated successfully.";               
                return View(requestSetDevice);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult EditDevice(long deviceID, string statusDevice)
        {
            MIM.Device responseGetDevice = new MIM.Device();
            
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
           
            //if (string.IsNullOrEmpty(productSerial))
            if (deviceID == 0)
            {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Edit Device";

            responseGetDevice = deviceDAL.GetDevice(deviceID: deviceID, statusDevice:statusDevice);

            Session["PreviousProductSerial"] = responseGetDevice.productSerial;
            Session["PreviousStatus"] = responseGetDevice.status;

            ViewBag.deviceType = deviceDAL.GetDeviceTypeList(selectedDeviceType:responseGetDevice.deviceType );
            ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: responseGetDevice.companyOwner);
            ViewBag.status = statusDAL.GetStatusNameList(selectedStatusName: responseGetDevice.status);

            if (string.IsNullOrEmpty(responseGetDevice.deviceDetails))
            {
                return HttpNotFound();
            }

            return View(responseGetDevice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDevice(MIM.Device requestSetDevice, string command)
        {
            List<MIM.DeviceTransaction> requestSetDeviceTransaction = new List<MIM.DeviceTransaction>();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("EditDevice", new { deviceID = requestSetDevice.deviceID});

            if (command == "Close")
                return RedirectToAction("MobikonIMS", "MobikonIMS");

            bool result = false;

            if (command == "Save")
            {
                requestSetDevice.userName = Session["UserName"].ToString();

                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];
                ViewBag.menuName = "Edit Device";

                ViewBag.deviceType = deviceDAL.GetDeviceTypeList(selectedDeviceType:requestSetDevice.deviceType);
                ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: requestSetDevice.companyOwner);
                ViewBag.status = statusDAL.GetStatusNameList(selectedStatusName: requestSetDevice.status);

                if (ModelState.IsValid)
                {
                    if (Session["PreviousProductSerial"].ToString().Trim() != requestSetDevice.productSerial.Trim())
                    {
                        if (true == deviceDAL.CheckDeviceExist(requestSetDevice.productSerial))
                        {
                            ViewBag.message = "Device already exists.";
                            return View(requestSetDevice);
                        }
                    }


                    if (requestSetDevice.status == "Blocked")
                    {
                        if (Session["Role"].ToString() == "Accounts")
                        {
                            ViewBag.Message = "Not authorized to block this device.";
                            return View(requestSetDevice);
                        }

                        int totalBlockedDevice = deviceDAL.BlockedDevicesCount(Session["UserName"].ToString());
                        if (totalBlockedDevice > 6)
                        {
                            ViewBag.message = "You cannot block more than 6 devices. Since you have already blocked 6 devices.";
                            return View(requestSetDevice);
                        }
                    }

                    if (Session["Role"].ToString() == "Sales")
                    {
                        if (Session["PreviousStatus"].ToString() == "Blocked")
                        {
                            if (requestSetDevice.status != "Blocked")
                            {
                                if (Session["UserName"].ToString() != deviceDAL.CheckBlockDeviceByUser(deviceID: requestSetDevice.deviceID, productSerial: requestSetDevice.productSerial, userName: requestSetDevice.userName))
                                {
                                    ViewBag.Message = "You cannot change the status of this device. This device already blocked by another user.";
                                    return View(requestSetDevice);
                                }
                            }
                        }
                    }

                    result = deviceDAL.EditDevice(requestSetDevice,Session["Role"].ToString());
                    if (result == true)
                    {
                        if (Session["PreviousStatus"].ToString() != requestSetDevice.status)
                        {
                            if (requestSetDevice.status == "Blocked" || requestSetDevice.status == "In Stock" || requestSetDevice.status == "Deployed-Active")
                            {
                                //Converting requestSetDeviceTransaction object requestDeviceTransaction List Object for SendEmail
                                requestSetDeviceTransaction.Add(new MIM.DeviceTransaction
                                {
                                    status = requestSetDevice.status,
                                    //statusID = statusDAL.FindStatusID(requestSetDevice.status),
                                    deviceDetails = requestSetDevice.deviceDetails,
                                    deviceID = requestSetDevice.deviceID,
                                    productSerial = requestSetDevice.productSerial,
                                    //userID = userDAL.FindUserID(requestSetDevice.userName),
                                    userName = requestSetDevice.userName,
                                    deviceType = requestSetDevice.deviceType,
                                    createdDate = requestSetDevice.createdDate
                                });

                                deviceDAL.SendEmail(requestSetDeviceTransaction: requestSetDeviceTransaction);
                            }
                        }

                        Session["PreviousStatus"] = null;
                        Session["PreviousProductSerial"] = null;

                        return RedirectToAction("Device");
                    }   
                    else
                    {
                        ViewBag.Message = "Device not updated successfully.";
                    }                 
                }
                return View(requestSetDevice);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult DeleteDevice(long deviceID, string productSerial, string statusDevice)
        {
            MIM.Device responseGetDevice = new MIM.Device();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            //if (string.IsNullOrEmpty(productSerial))
            if (deviceID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Edit Device";

            responseGetDevice = deviceDAL.GetDevice(deviceID: deviceID, productSerial: productSerial, statusDevice: statusDevice);
                    

            if (string.IsNullOrEmpty(responseGetDevice.deviceDetails))
            {
                return HttpNotFound();
            }

            return View(responseGetDevice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDevice(long deviceID, string productSerial, string statusDevice, string command)
        {
            List<MIM.DeviceTransaction> requestSetDeviceTransaction = new List<MIM.DeviceTransaction>();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("DeleteDevice", new { deviceID = deviceID, productSerial = productSerial });

            if (command == "Close")
                return RedirectToAction("MobikonIMS", "MobikonIMS");

            bool result = false;

            if (command == "Delete")
            {
                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];
                ViewBag.menuName = "Delete Device";               

                if (ModelState.IsValid)
                {
                   
                    result = deviceDAL.DeleteDevice(deviceID,productSerial);
                    if (result == true)
                    {                        
                        return RedirectToAction("Device");
                    }
                    else
                    {
                        ViewBag.Message = "Device not deleted successfully.";
                    }
                }
                return View("Device");
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult EditDeviceTransaction(long serialNo)
        {
            MIM.DeviceTransaction responseGetDeviceTransaction = new MIM.DeviceTransaction();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (serialNo  == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            responseGetDeviceTransaction = deviceDAL.GetDeviceTransactionDetail(serialNo: serialNo);
            Session["PreviousStatus"] = responseGetDeviceTransaction.status;

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Edit Device Inventory ";

            ViewBag.clientName = clientDAL.GetClientNameList(statusClient:true, selectedClientID:responseGetDeviceTransaction.clientID, selectedClientName: responseGetDeviceTransaction.clientName);
            ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: responseGetDeviceTransaction.statusID, selectedStatusName: responseGetDeviceTransaction.status);
            ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID: responseGetDeviceTransaction.outletID, selectedOutletName: responseGetDeviceTransaction.outletName);
            ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner:responseGetDeviceTransaction.companyOwner);

            if (string.IsNullOrEmpty(responseGetDeviceTransaction.productSerial))
            {
                return HttpNotFound();
            }

            return View(responseGetDeviceTransaction);
        }

        public JsonResult GetProductSerialwiseDevice(string productSerial)
        {
            MIM.Device responseGetDevice = new MIM.Device();
            responseGetDevice = deviceDAL.GetDevice(productSerial:productSerial, statusDevice:mC.CommonMobikonIMS.StatusDevice.All.ToString());
            return Json(responseGetDevice,JsonRequestBehavior.AllowGet);
            //return View(responseGetDevice);          
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDeviceTransaction(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            List < MIM.DeviceTransaction> requestDeviceTransaction = new List<MIM.DeviceTransaction>();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("EditDeviceTransaction");

            if (command == "Close")
                return RedirectToAction("CreateDeviceTransaction", "Device");

            if (command == "Save")
            {
                ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
                ViewBag.loginDateTime = Session["LoginDateTime"];
                ViewBag.menuName = "Edit Device Inventory ";

                ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, selectedClientID: requestSetDeviceTransaction.clientID, selectedClientName: requestSetDeviceTransaction.clientName);
                ViewBag.statusName = statusDAL.GetStatusNameList(selectedStatusID: requestSetDeviceTransaction.statusID, selectedStatusName: requestSetDeviceTransaction.status);
                ViewBag.outletName = outletDAL.GetOutletNameList(selectedOutletID: requestSetDeviceTransaction.outletID, selectedOutletName: requestSetDeviceTransaction.outletName);
                ViewBag.companyOwner = deviceDAL.GetCompanyOwnerList(selectedCompanyOwner: requestSetDeviceTransaction.companyOwner);

                if (ModelState.IsValid)
                {
                    requestSetDeviceTransaction.userName = Session["UserName"].ToString();
                    //if ((Session["Role"].ToString() == "Sales" || Session["Role"].ToString() == "Operations" || Session["Role"].ToString() == "Administrators" ) && requestSetDeviceTransaction.status == "Blocked")
                    
                    if (requestSetDeviceTransaction.status == "Blocked")
                    {
                        if (Session["Role"].ToString() == "Accounts")
                        {
                            ViewBag.Message = "Not authorized to block this device.";
                            return View(requestSetDeviceTransaction);
                        }

                        int totalBlockedDevice = deviceDAL.BlockedDevicesCount(Session["UserName"].ToString());
                        if (totalBlockedDevice > 6)
                        {
                            ViewBag.message = "You cannot block more than 6 devices. Since you have already blocked 6 devices.";
                            return View(requestSetDeviceTransaction);
                        }
                    }
                    

                    if (Session["Role"].ToString() == "Sales")
                    {
                        if (Session["PreviousStatus"].ToString() == "Blocked")
                        {
                            if (requestSetDeviceTransaction.status != "Blocked")
                            {
                                if (Session["UserName"].ToString() != deviceDAL.CheckBlockDeviceByUser(deviceID: requestSetDeviceTransaction.deviceID, productSerial: requestSetDeviceTransaction.productSerial, userName: requestSetDeviceTransaction.userName))
                                {
                                    ViewBag.Message = "You cannot change the status of this device. This device already blocked by another user.";
                                    return View(requestSetDeviceTransaction);
                                }
                            }
                        }
                    }

                    if (requestSetDeviceTransaction.dcDate.ToString().Length <= 0)
                    {
                        ViewBag.Message = "Please provide delivery challan date.";
                        return View(requestSetDeviceTransaction);
                        
                    }

                    foreach (string upload in Request.Files)
                    {
                        string path = deviceDAL.GetDCFileFolderPath();
                        bool folderExists = Directory.Exists(path);
                        if (!folderExists)
                            Directory.CreateDirectory(path);

                        Session["DCFile"] = Request.Files[upload].FileName;

                        if (!string.IsNullOrEmpty(Session["DCFile"].ToString()))
                        {
                            if (string.IsNullOrEmpty(requestSetDeviceTransaction.dc))
                            {
                                ViewBag.Message = "Please provide delivery challan.";
                                return View(requestSetDeviceTransaction);
                            }

                            string fileName = requestSetDeviceTransaction.dc + Path.GetExtension(Request.Files[upload].FileName);//Path.GetFileName(Request.Files[upload].FileName);
                            Request.Files[upload].SaveAs(Path.Combine(path, fileName));
                            requestSetDeviceTransaction.dcFile = Path.Combine(path, fileName);
                        }
                        //else if (!string.IsNullOrEmpty(Session["DCFile"].ToString()))
                        //{
                        //    if (string.IsNullOrEmpty(requestSetDeviceTransaction.dc))
                        //    {
                        //        ViewBag.Message = "Please provide delivery challan.";
                        //        return View(requestSetDeviceTransaction);
                        //    }
                        //    string fileName = requestSetDeviceTransaction.dc + Path.GetExtension(Session["DCFile"].ToString());//Path.GetFileName(Request.Files[upload].FileName);
                        //    Request.Files[upload].SaveAs(Path.Combine(path, fileName));
                        //    requestSetDeviceTransaction.dcFile = Path.Combine(path, fileName);
                        //}
                    }                    

                    requestSetDeviceTransaction.deliveryDate = requestSetDeviceTransaction.dcDate;
                    requestSetDeviceTransaction.createdDate = System.DateTime.Now;

                    bool result = deviceDAL.EditDeviceTransaction(requestSetDeviceTransaction);
                    if (result == true)
                    {
                        if (requestSetDeviceTransaction.status == "Blocked" || requestSetDeviceTransaction.status == "In Stock" || requestSetDeviceTransaction.status == "Deployed-Active")
                        {
                            requestDeviceTransaction.Add(new MIM.DeviceTransaction()
                            {
                                deviceID = requestSetDeviceTransaction.deviceID,
                                productSerial = requestSetDeviceTransaction.productSerial,
                                deviceDetails = requestSetDeviceTransaction.deviceDetails,
                                outletID = requestSetDeviceTransaction.outletID,
                                outletName = requestSetDeviceTransaction.outletName,
                                status = requestSetDeviceTransaction.status,
                                userName = requestSetDeviceTransaction.userName
                            });

                            deviceDAL.SendEmail(requestSetDeviceTransaction: requestDeviceTransaction);
                        }

                        Session["PreviousStatus"] = null;
                        //return RedirectToAction("DeviceHistory", "Device", new { productSerialSearch = requestSetDeviceTransaction.productSerial });
                        return RedirectToAction("Device");
                    }
                    else
                    { 
                        ViewBag.Message = "Device transaction not updated successfully.";
                    }
                }

                Session["DCFile"] = null;
                return View(requestSetDeviceTransaction);
            }
            return RedirectToAction("Login", "Login");
        }

    }
}
