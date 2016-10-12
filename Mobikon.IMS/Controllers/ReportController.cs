using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    public class ReportController : Controller
    {
        MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
        MID.ClientDAL clientDAL = new MID.ClientDAL();
        MID.StatusDAL statusDAL = new MID.StatusDAL();
        MID.OutletDAL outletDAL = new MID.OutletDAL();
        MID.CityDAL cityDAL = new MID.CityDAL();
        MID.CountryDAL countryDAL = new MID.CountryDAL();

        // GET: Report
        public ActionResult DeviceHistory()
        {

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            MID.UserDAL userDAL = new MID.UserDAL();
            MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
            MID.ClientDAL clientDAL = new MID.ClientDAL();        
            MID.OutletDAL outletDAL = new MID.OutletDAL();  
    

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Device Inventory Report";

            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.All.ToString(), pageName: "DeviceHistory", showBlockedDevice: true);
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, pageName: "DeviceHistory");
            ViewBag.outletName = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
            ViewBag.userNameList = userDAL.GetUserNameList(pageName: "DeviceHistory");
            //ViewBag.cityName = cityDAL.GetCityNameList(pageName:"DeviceHistory");
            //ViewBag.countryName= countryDAL.GetCountryNameList(pageName:"DeviceHistory");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void DeviceHistory(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            List<MIM.DeviceTransactionRpt> responseDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();

            if (string.IsNullOrEmpty(requestSetDeviceTransaction.userName))
                requestSetDeviceTransaction.userName = Session["UserName"].ToString();

            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: requestSetDeviceTransaction.productSerial, clientName: requestSetDeviceTransaction.clientName, outletName: requestSetDeviceTransaction.outletName, fromDate: requestSetDeviceTransaction.fromDate, toDate: requestSetDeviceTransaction.toDate);

            grid.AllowPaging = false;

            if (requestSetDeviceTransaction.userName != "All")
            {
                responseDeviceTransactionRpt = responseGetDeviceTransactionRpt.Where(response => response.UserName == requestSetDeviceTransaction.userName).ToList();
                grid.DataSource = responseDeviceTransactionRpt;
            }
            else
            {
                grid.DataSource = responseGetDeviceTransactionRpt;
            }
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=Device Inventory Report.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Write(sw.ToString());

                Response.End();
            }
            else
            {
                //if (responseGetDeviceRpt.Count >= 1)
                //{
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Device Inventory Report.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                StringWriter swr = new StringWriter();
                HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                grid.RenderControl(htmlwr);
                StringReader srr = null;

                if (responseGetDeviceTransactionRpt.Count >= 1)
                {
                    srr = new StringReader(swr.ToString());
                }
                else
                {
                    srr = new StringReader("No Records Found !! ");
                }

                Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                pdfdoc.Open();
                htmlparser.Parse(srr);
                pdfdoc.Close();
                Response.Write(pdfdoc);
                Response.End();

            }
        }

        public ActionResult BlockDevice()
        {

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            MID.UserDAL userDAL = new MID.UserDAL();
            MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
            MID.ClientDAL clientDAL = new MID.ClientDAL();
            MID.OutletDAL outletDAL = new MID.OutletDAL();


            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Block Device Report";

            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.All.ToString(), pageName: "DeviceHistory", showBlockedDevice: true);
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, pageName: "DeviceHistory");
            ViewBag.outletName = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
            ViewBag.userNameList = userDAL.GetUserNameList(pageName: "DeviceHistory");
            //ViewBag.cityName = cityDAL.GetCityNameList(pageName:"DeviceHistory");
            //ViewBag.countryName= countryDAL.GetCountryNameList(pageName:"DeviceHistory");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void BlockDevice(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            List<MIM.DeviceTransactionRpt> responseDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();

            if (string.IsNullOrEmpty(requestSetDeviceTransaction.userName))
                requestSetDeviceTransaction.userName = Session["UserName"].ToString();

            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: requestSetDeviceTransaction.productSerial, clientName: requestSetDeviceTransaction.clientName, outletName: requestSetDeviceTransaction.outletName, fromDate: requestSetDeviceTransaction.fromDate, toDate: requestSetDeviceTransaction.toDate);

            grid.AllowPaging = false;

            if (requestSetDeviceTransaction.userName != "All")
            {
                responseDeviceTransactionRpt = responseGetDeviceTransactionRpt.Where(response => response.UserName == requestSetDeviceTransaction.userName).ToList();
                grid.DataSource = responseDeviceTransactionRpt.Where(response => response.Status.ToUpper() == "BLOCKED");
            }
            else
            {
                grid.DataSource = responseGetDeviceTransactionRpt.Where(response => response.Status.ToUpper() == "BLOCKED").ToList(); 
            }
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=Block Device Report.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Write(sw.ToString());

                Response.End();
            }
            else
            {                
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Block Device Report.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                StringWriter swr = new StringWriter();
                HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                grid.RenderControl(htmlwr);
                StringReader srr = null;

                if (responseGetDeviceTransactionRpt.Count >= 1)
                {
                    srr = new StringReader(swr.ToString());
                }
                else
                {
                    srr = new StringReader("No Records Found !! ");
                }

                Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                pdfdoc.Open();
                htmlparser.Parse(srr);
                pdfdoc.Close();
                Response.Write(pdfdoc);
                Response.End();
            }
        }

        public ActionResult InStockDevice()
        {

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            MID.UserDAL userDAL = new MID.UserDAL();
            MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
            MID.ClientDAL clientDAL = new MID.ClientDAL();
            MID.OutletDAL outletDAL = new MID.OutletDAL();


            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "In Stock Device Report";

            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.All.ToString(), pageName: "DeviceHistory", showBlockedDevice: true);
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, pageName: "DeviceHistory");
            ViewBag.outletName = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
            ViewBag.userNameList = userDAL.GetUserNameList(pageName: "DeviceHistory");
            //ViewBag.cityName = cityDAL.GetCityNameList(pageName:"DeviceHistory");
            //ViewBag.countryName= countryDAL.GetCountryNameList(pageName:"DeviceHistory");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void InStockDevice(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            List<MIM.DeviceTransactionRpt> responseDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();

            if (string.IsNullOrEmpty(requestSetDeviceTransaction.userName))
                requestSetDeviceTransaction.userName = Session["UserName"].ToString();

            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: requestSetDeviceTransaction.productSerial, clientName: requestSetDeviceTransaction.clientName, outletName: requestSetDeviceTransaction.outletName, fromDate: requestSetDeviceTransaction.fromDate, toDate: requestSetDeviceTransaction.toDate);

            grid.AllowPaging = false;

            if (requestSetDeviceTransaction.userName != "All")
            {
                responseDeviceTransactionRpt = responseGetDeviceTransactionRpt.Where(response => response.UserName == requestSetDeviceTransaction.userName).ToList();
                grid.DataSource = responseDeviceTransactionRpt.Where(response => response.Status.ToUpper() == "IN STOCK");
            }
            else
            {
                grid.DataSource = responseGetDeviceTransactionRpt.Where(response => response.Status.ToUpper() == "IN STOCK").ToList();
            }
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=In Stock Device Report.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Write(sw.ToString());

                Response.End();
            }
            else
            {
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=In Stock Device Report.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                StringWriter swr = new StringWriter();
                HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                grid.RenderControl(htmlwr);
                StringReader srr = null;

                if (responseGetDeviceTransactionRpt.Count >= 1)
                {
                    srr = new StringReader(swr.ToString());
                }
                else
                {
                    srr = new StringReader("No Records Found !! ");
                }

                Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                pdfdoc.Open();
                htmlparser.Parse(srr);
                pdfdoc.Close();
                Response.Write(pdfdoc);
                Response.End();
            }
        }

        public ActionResult DeployedActiveDevice()
        {

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            MID.UserDAL userDAL = new MID.UserDAL();
            MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
            MID.ClientDAL clientDAL = new MID.ClientDAL();
            MID.OutletDAL outletDAL = new MID.OutletDAL();


            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Deployed-Active Device Report";

            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.All.ToString(), pageName: "DeviceHistory", showBlockedDevice: true);
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, pageName: "DeviceHistory");
            ViewBag.outletName = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
            ViewBag.userNameList = userDAL.GetUserNameList(pageName: "DeviceHistory");
            //ViewBag.cityName = cityDAL.GetCityNameList(pageName:"DeviceHistory");
            //ViewBag.countryName= countryDAL.GetCountryNameList(pageName:"DeviceHistory");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void DeployedActiveDevice(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            List<MIM.DeviceTransactionRpt> responseDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();

            if (string.IsNullOrEmpty(requestSetDeviceTransaction.userName))
                requestSetDeviceTransaction.userName = Session["UserName"].ToString();

            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: requestSetDeviceTransaction.productSerial, clientName: requestSetDeviceTransaction.clientName, outletName: requestSetDeviceTransaction.outletName, fromDate: requestSetDeviceTransaction.fromDate, toDate: requestSetDeviceTransaction.toDate);

            grid.AllowPaging = false;

            if (requestSetDeviceTransaction.userName != "All")
            {
                responseDeviceTransactionRpt = responseGetDeviceTransactionRpt.Where(response => response.UserName == requestSetDeviceTransaction.userName).ToList();
                grid.DataSource = responseDeviceTransactionRpt.Where(response => response.Status.ToUpper() == "DEPLOYED-ACTIVE");
            }
            else
            {
                grid.DataSource = responseGetDeviceTransactionRpt.Where(response => response.Status.ToUpper() == "DEPLOYED-ACTIVE").ToList();
            }
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=Deployed-Active Device Report.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Write(sw.ToString());

                Response.End();
            }
            else
            {
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Deployed-Active Device Repor.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                StringWriter swr = new StringWriter();
                HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                grid.RenderControl(htmlwr);
                StringReader srr = null;

                if (responseGetDeviceTransactionRpt.Count >= 1)
                {
                    srr = new StringReader(swr.ToString());
                }
                else
                {
                    srr = new StringReader("No Records Found !! ");
                }

                Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                pdfdoc.Open();
                htmlparser.Parse(srr);
                pdfdoc.Close();
                Response.Write(pdfdoc);
                Response.End();
            }
        }

        public ActionResult Device()
        {
            MID.DeviceDAL deviceDAL = new MID.DeviceDAL();           
            MID.StatusDAL statusDAL = new MID.StatusDAL();
           

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Device report";

            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.All.ToString(), pageName: "Device", showBlockedDevice: true);
            ViewBag.status = statusDAL.GetStatusNameList(pageName: "Device");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Device(MIM.Device requestSetDevice, string command)
        {

            List<MIM.DeviceRpt> responseGetDeviceRpt = new List<MIM.DeviceRpt>();
            var grid = new System.Web.UI.WebControls.GridView();

            responseGetDeviceRpt = deviceDAL.GetDeviceListRpt(productSerial: requestSetDevice.productSerial, deviceDetails: requestSetDevice.deviceDetails, statusDevice: requestSetDevice.status, showBlockedDevice: true);

            grid.AllowPaging = false;
            grid.DataSource = responseGetDeviceRpt;
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=DeviceList.xls");
                //Response.ContentType = "application/excel";
                Response.ContentType = "application/vnd.openxmlformats - officedocument.spreadsheetml.sheet";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.End();
            }
            else
            {
                //if (responseGetDeviceRpt.Count >= 1)
                //{
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=DeviceInventory.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                StringWriter swr = new StringWriter();
                HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                grid.RenderControl(htmlwr);
                StringReader srr = null;

                if (responseGetDeviceRpt.Count >= 1)
                {
                    srr = new StringReader(swr.ToString());
                }
                else
                {
                    srr = new StringReader("No Records Found !! ");
                }

                Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                pdfdoc.Open();
                htmlparser.Parse(srr);
                pdfdoc.Close();
                Response.Write(pdfdoc);
                Response.End();
                //}
            }
        }

        public ActionResult Outlet(string clientNameSearch, string statusClientSearch)
        {
            
            MID.ClientDAL clientDAL = new MID.ClientDAL();         
            MID.OutletDAL outletDAL = new MID.OutletDAL();
            MID.CityDAL cityDAL = new MID.CityDAL();
            MID.CountryDAL countryDAL = new MID.CountryDAL();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Outlet Report";

            ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, pageName: "Outlet");
            ViewBag.outletName = outletDAL.GetOutletNameList(pageName: "Outlet");
            ViewBag.countryName = countryDAL.GetCountryNameList(pageName: "Outlet");
            ViewBag.cityName = cityDAL.GetCityNameList(pageName: "Outlet");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void Outlet(MIM.Outlet requestSetOutlet, string command)
        {
            List<MIM.OutletRpt> responseGetOutletRpt = new List<MIM.OutletRpt>();
            var grid = new System.Web.UI.WebControls.GridView();

            responseGetOutletRpt = outletDAL.GetOutletListRpt(clientName: requestSetOutlet.clientName, statusOutlet: requestSetOutlet.activated, outletName: requestSetOutlet.outletName);

            grid.AllowPaging = false;
            grid.DataSource = responseGetOutletRpt;
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=OutletList.xls");
                Response.ContentType = "application/excel";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.End();
            }
            else
            {
                //if (responseGetDeviceRpt.Count >= 1)
                //{
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=OutletList.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);

                StringWriter swr = new StringWriter();
                HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                grid.RenderControl(htmlwr);
                StringReader srr = null;

                if (responseGetOutletRpt.Count >= 1)
                {
                    srr = new StringReader(swr.ToString());
                }
                else
                {
                    srr = new StringReader("No Records Found !! ");
                }

                Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                pdfdoc.Open();
                htmlparser.Parse(srr);
                pdfdoc.Close();
                Response.Write(pdfdoc);
                Response.End();
                
            }
        }
    }
}