using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PagedList;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using DotNet.Highcharts.Samples.Models;
using Point = DotNet.Highcharts.Options.Point;
using System.Drawing;

using mC = Mobikon.IMS.Common;
using MID = Mobikon.IMS.Data;
using MIM = Mobikon.IMS.Message;
namespace MobikonIMS.Controllers
{
    public class MobikonIMSController : Controller
    {
        MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
        MID.StatusDAL statusDAL = new MID.StatusDAL();
        MID.CityDAL cityDAL = new MID.CityDAL();
        public ActionResult MobikonIMS(string sortOrder, string statusFilter,string fromDateFilter,string toDateFiler, string statusSearch,string fromDateSearch,string toDateSearch, int? page, string paging)
        {           
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ") ";

            DateTime serverTime = DateTime.Now;
            DateTime utcTime = serverTime.ToUniversalTime(); // convert it to Utc using timezone setting of server computer
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzi); // convert from utc to local

            Session["LoginDateTime"] = " Login In: " + String.Format("{0:dd/MM/yyyy hh:mm tt}", localTime);
            ViewBag.loginDateTime = Session["LoginDateTime"];
     
            ViewBag.menuName = "Device Inventory Dashboard";

            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
            List<MIM.DeviceTransaction> responseDeviceTransaction = new List<MIM.DeviceTransaction>();
            
            ViewBag.fromDateSearch = "01/01" + "/" + DateTime.Now.Year;
            ViewBag.toDateSearch = System.DateTime.Now.ToShortDateString();
            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "statusSort";

            ViewBag.statusSort = sortOrder == "statusSort" ? "statusSortDesc" : "statusSort";

            ViewBag.currentSort = sortOrder;
            //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

            if (string.IsNullOrEmpty(fromDateSearch))
                fromDateSearch = ViewBag.fromDateSearch;

            if (string.IsNullOrEmpty(toDateSearch))
                toDateSearch = ViewBag.toDateSearch;

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            if (statusSearch != null)
                page = 1;

            if (!string.IsNullOrEmpty(statusSearch))
                ViewBag.statusFilter = statusSearch;
            else
            {
                statusSearch = statusFilter;
                ViewBag.statusFilter = statusSearch;
            }

            ViewBag.fromDateFilter = fromDateSearch;
            ViewBag.toDateFilter = toDateSearch;

            responseGetDeviceTransaction = deviceDAL.ShowDashBoard(Convert.ToDateTime(fromDateSearch), Convert.ToDateTime(toDateSearch));
            var deviceTransactions = responseGetDeviceTransaction.AsQueryable();
            
            if (!string.IsNullOrEmpty(statusSearch))
            {
                if (statusSearch != "All")
                    deviceTransactions = deviceTransactions.Where(deviceTransaction => deviceTransaction.status.ToUpper().Trim() == statusSearch.ToUpper().Trim());
            }

            if (Session["Role"].ToString() != "Sales")
            {
                foreach (var response in deviceTransactions)
                {
                    responseDeviceTransaction.Add(new MIM.DeviceTransaction()
                    {
                        status = response.status,
                        //deviceType = response.deviceType,
                        deviceCount = response.deviceCount,
                        //cityName = response.cityName,
                    });
                }
            }
            else
            {
                foreach (var response in deviceTransactions.Where(status => status.status == "In Stock"))
                {
                    responseDeviceTransaction.Add(new MIM.DeviceTransaction()
                    {
                        status = response.status,
                        //deviceType = response.deviceType,
                        deviceCount = response.deviceCount,
                        //cityName = response.cityName,
                    });
                }

                foreach (var response in deviceTransactions.Where(status => status.status == "Blocked"))
                {
                    responseDeviceTransaction.Add(new MIM.DeviceTransaction()
                    {
                        status = response.status,
                        //deviceType = response.deviceType,
                        deviceCount = response.deviceCount,
                        //cityName = response.cityName,
                    });
                }
            }
            int pageNumber = (page ?? 1);
            return View(responseDeviceTransaction.ToPagedList(pageNumber, pageSize));            
        }

        public ActionResult Error()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            return View();
        }

        public ActionResult Contact()
        {            
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            return View();               
        }

        public ActionResult LogOut()
        {
            Session["Role"] = null;
            Session["UserName"] = null;
            Session["ProductSerial"] = null;
            Session["OutletName"] = null;
            Session["ClientName"] = null;
            Session["DCFile"] = null;

            return RedirectToAction("Login", "Login");
        }
    }
}
