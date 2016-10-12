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

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.menuName = "Device Inventory Report";

            ViewBag.productSerial = deviceDAL.GetProductSerialList(statusDevice: mC.CommonMobikonIMS.StatusDevice.All.ToString(), pageName: "DeviceHistory", showBlockedDevice: true);
            ViewBag.clientName = clientDAL.GetClientNameList(statusClient: true, pageName: "DeviceHistory");
            ViewBag.outletName = outletDAL.GetOutletNameList(pageName: "DeviceHistory");
            //ViewBag.cityName = cityDAL.GetCityNameList(pageName:"DeviceHistory");
            //ViewBag.countryName= countryDAL.GetCountryNameList(pageName:"DeviceHistory");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void DeviceHistory(MIM.DeviceTransaction requestSetDeviceTransaction, string command)
        {
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();
            var grid = new System.Web.UI.WebControls.GridView();
            responseGetDeviceTransactionRpt = deviceDAL.GetDeviceTransactionDetailListRpt(productSerial: requestSetDeviceTransaction.productSerial, clientName: requestSetDeviceTransaction.clientName, outletName: requestSetDeviceTransaction.outletName, fromDate: requestSetDeviceTransaction.fromDate, toDate: requestSetDeviceTransaction.toDate);
            grid.AllowPaging = false;
            grid.DataSource = responseGetDeviceTransactionRpt;
            grid.DataBind();

            if (command == "Export to Excel")
            {
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=DeviceHistory.xls");
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
                Response.AddHeader("content-disposition", "attachment;filename=DeviceHistory.pdf");
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

                //else
                //{
                //    if (responseGetDeviceTransactionRpt.Count >= 1)
                //    {
                //        Response.ContentType = "application/pdf";
                //        Response.AddHeader("content-disposition", "attachment;filename=DeviceHistory.pdf");
                //        Response.Cache.SetCacheability(HttpCacheability.NoCache);

                //        StringWriter swr = new StringWriter();
                //        HtmlTextWriter htmlwr = new HtmlTextWriter(swr);

                //        grid.RenderControl(htmlwr);
                //        StringReader srr = new StringReader(swr.ToString());
                //        Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                //        HTMLWorker htmlparser = new HTMLWorker(pdfdoc);
                //        PdfWriter.GetInstance(pdfdoc, Response.OutputStream);

                //        pdfdoc.Open();
                //        htmlparser.Parse(srr);
                //        pdfdoc.Close();
                //        Response.Write(pdfdoc);
                //    }
                //    Response.End();
            }
        }
        public ActionResult Device()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
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
                Response.AddHeader("content-disposition", "attachment; filename=DeviceHistory.xls");
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
                Response.AddHeader("content-disposition", "attachment;filename=DeviceHistory.pdf");
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
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
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
                Response.AddHeader("content-disposition", "attachment; filename=OutletHistory.xls");
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
                Response.AddHeader("content-disposition", "attachment;filename=DeviceHistory.pdf");
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