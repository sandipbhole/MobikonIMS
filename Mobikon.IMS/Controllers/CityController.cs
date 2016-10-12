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
    public class CityController : Controller
    {
        MID.CityDAL cityDAL = new MID.CityDAL();
        MID.CountryDAL countryDAL = new MID.CountryDAL();

        public ActionResult City(string sortOrder, string countryNameFilter,string cityNameFilter,  string countryNameSearch, string cityNameSearch, int? page, string paging)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuNameName = "City List";

            List <MIM.City> responseGetCity = new List<MIM.City>();
            List<MIM.City> responseCity = new List<MIM.City>();            

            if (!string.IsNullOrEmpty(countryNameSearch))
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "Client");
            else
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(pageName: "Client", selectedCountryName: countryNameFilter);

            if (!string.IsNullOrEmpty(cityNameSearch))
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "Client");
            else
                ViewBag.cityNameSearch = cityDAL.GetCityNameList(pageName: "Client", selectedCityName: cityNameFilter);

            ViewBag.currentSort = sortOrder;
           // ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();
            
            if (String.IsNullOrEmpty(sortOrder))
                sortOrder = "countryNameSort";

            ViewBag.cityNameSort = sortOrder == "cityNameSortDesc" ? "cityNameSort" : "cityNameSortDesc";
            ViewBag.countryNameSort = sortOrder == "countryNameSortDesc" ? "countryNameSort" : "countryNameSortDesc";                   

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            if (!string.IsNullOrEmpty(cityNameSearch))
                page = 1;
            if (!string.IsNullOrEmpty(countryNameSearch))
                page = 1;

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


            responseGetCity = cityDAL.GetCityList();
            var cities = responseGetCity.AsQueryable();

            //if (!string.IsNullOrEmpty(cityNameSearch))
            //    ViewBag.cityNameFilter = cityNameSearch;           
            //if (!string.IsNullOrEmpty(countryNameSearch))
            //    ViewBag.countryNameFilter = countryNameSearch;

            if (!string.IsNullOrEmpty(cityNameSearch))
            {
                if (cityNameSearch != "All" && cityNameSearch != "Select city")
                    cities = cities.Where(client => client.cityName.ToUpper().Trim() == cityNameSearch.Trim());
            }

            if (!string.IsNullOrEmpty(countryNameSearch))
            {
                if (countryNameSearch != "All" && cityNameSearch != "Select country")
                    cities = cities.Where(client => client.countryName.ToUpper().Trim() == countryNameSearch.Trim());
            }

            switch (sortOrder)
            {                
                case "cityNameSortDesc":
                    cities = cities.OrderByDescending(client => client.cityName);
                    break;
                case "cityNameSort":
                    cities = cities.OrderBy(client => client.cityName);
                    break;
                case "countryNameSort":
                    cities = cities.OrderBy(client => client.countryName);
                    break;
                default: //CountryNameDortDesc
                    cities = cities.OrderByDescending(client => client.countryName);
                    break;              
            }

            foreach (var response in cities)
            {
                responseCity.Add(new MIM.City()
                {
                    cityID = response.cityID,
                    cityName = response.cityName,                    
                    countryID = response.countryID,
                    countryName = response.countryName,
                    seperator = mC.CommonMobikonIMS.seperator
                });
            }
            int pageNumber = (page ?? 1);
            return View(responseCity.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult EditCity(long cityID)
        {
            MIM.City responseGetCity = new MIM.City();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (cityID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }           
            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Edit city";

            responseGetCity = cityDAL.GetCityDetails(cityID);
            ViewBag.countryNameSearch = countryDAL.GetCountryNameList(selectedCountryName: responseGetCity.countryName);
           
            if (string.IsNullOrEmpty(responseGetCity.cityName))
            {
                return HttpNotFound();
            }

            return View(responseGetCity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCity(MIM.City requestSetCity, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("EditCity");

            if (command == "Close")
                return RedirectToAction("CreateCity", "City");

            if (command == "Save")
            {
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList(selectedCountryName: requestSetCity.countryName);

                if (ModelState.IsValid)
                {  
                    bool result = cityDAL.EditCity(requestSetCity);

                    if (result == true)
                        return RedirectToAction("City", "City", new { cityNameSearch = requestSetCity.cityName, countryNameSearch = requestSetCity.countryName });
                }

                return View(requestSetCity);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult CreateCity()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "Add City";

            ViewBag.countryNameSearch = countryDAL.GetCountryNameList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCity(MIM.City requestSetCity, string command)
        {
            
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("CreateCity");

            if (command == "Close")
                return RedirectToAction("City");

            if (command == "Save")
            {
                ViewBag.countryNameSearch = countryDAL.GetCountryNameList();
                if (ModelState.IsValid)
                {  
                    bool alreadyExists = cityDAL.CheckCityExists(requestSetCity.cityName,requestSetCity.countryName);
                    if (alreadyExists == true)
                    {
                        ViewBag.Message = "City name already exists.";
                        return View();
                    }

                    bool result = cityDAL.InsertCity(requestSetCity);

                    if (result == true)
                        return RedirectToAction("City");
                }
                ViewBag.message = "City not saved successfully.";
                return View(requestSetCity);
            }
            return RedirectToAction("Login", "Login");
        }

    }
}