using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using PagedList;

using mC = Mobikon.IMS.Common;
using MID = Mobikon.IMS.Data;
using MIM = Mobikon.IMS.Message;

namespace Mobikon.IMS.Controllers
{
    public class LoginController : Controller
    {
        MID.UserDAL userDAL = new MID.UserDAL();
        MID.RoleDAL roleDAL = new MID.RoleDAL();

        public ActionResult Login()
        {           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(MIM.Login requestSetLogin)
        {
            MID.DeviceDAL deviceDAL = new MID.DeviceDAL();
            if (ModelState.IsValid)
            {
                if (true == userDAL.LicenseExpired())
                {
                    return RedirectToAction("Error", "MobikonIMS");
                }

                if (true == userDAL.AuthenticateUser(requestSetLogin))
                {
                    FormsAuthentication.SetAuthCookie(requestSetLogin.userName, requestSetLogin.RememberMe);
                    Session["Role"] = userDAL.responseUser.roleName;
                    Session["UserName"] = requestSetLogin.userName;
                    Session["UserID"] = userDAL.responseUser.userID;
                    Session["ResetPassword"] = userDAL.responseUser.resetPassword;
                    Session["ValidationMessage"] = string.Empty;

                    deviceDAL.RemoveBlockStatus();
                   
                    if (userDAL.responseUser.resetPassword == false)
                    {
                        return RedirectToAction("MobikonIMS", "MobikonIMS");
                    }
                    else
                    {
                        //return View("ChangePassword", "~/Views/Shared/_LoginLayout.cshtml");
                        return RedirectToAction("ChangePassword", "Login");
                    }
                }
                else
                {
                    ViewBag.message = "The user name or password provided is incorrect.";
                    return View(requestSetLogin);
                }
            }
            else
            {
                ViewBag.message = "The user name or password provided is incorrect.";
                return View(requestSetLogin);
            }           
        }

        //public ActionResult Logout()
        //{
        //    FormsAuthentication.SignOut();
        //    return RedirectToAction("Home","Home");
        //} 

        public ActionResult ForgetPassword()
        { 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassword(MIM.ForgetPassword requestSetForgetPassword, string command)
        {            
            if (command == "Cancel")
                //return RedirectToAction("ForgetPassword");
                return RedirectToAction("Login", "Login");

            //if (command == "Close")
            //    return RedirectToAction("Login", "Login");

            if (command == "Send")
            {
                if (ModelState.IsValid)
                {
                    MIM.User requestSetUser = new MIM.User();
                    requestSetUser.userName = requestSetForgetPassword.userName; 
                  
                    if (true == userDAL.ForgetPassword(requestSetUser))
                        return RedirectToAction("Login", "Login");
                }
                ViewBag.message = "Password not send successfully.";
                return View(requestSetForgetPassword);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult ChangePassword()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Change Password";

            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ChangePassword(MIM.ChangePassword requestSetChangePassword, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")            
                return RedirectToAction("ChangePassword");
            
             if (command == "Close")            
                return RedirectToAction("Login", "Login");

            if (command == "Save")
            {
                if (ModelState.IsValid)
                {
                    MIM.User requestSetUser = new MIM.User();
                    requestSetUser.userName = requestSetChangePassword.userName;
                    requestSetUser.userID = Convert.ToInt64(Session["UserID"]);
                    requestSetUser.Password = requestSetChangePassword.Password;
                    requestSetUser.currentPassword = requestSetChangePassword.currentPassword;
                   
                    //if (false == userDAL.CheckUserExist(requestSetChangePassword.userName, requestSetUser.currentPassword))
                   // {
                        //ViewBag.Message = "Current Password Doesn't Match";
                       //  ModelState.AddModelError("CurrentPassword", "Your current password isn't correct");
                    //    return View();
                    // }

                    requestSetUser.resetPassword = false;
                    bool result = userDAL.ChangePassword(requestSetUser);

                    if (result == true)
                    {
                        Session["Role"] = null;
                        Session["UserName"] = null;
                        return RedirectToAction("Login");
                    }
                }
                ViewBag.message = "Password not changed successfully.";
                return View(requestSetChangePassword);
            }        
            return RedirectToAction("Login", "Login");
        }

        public ActionResult Register()
        { 
            ViewBag.roleName = roleDAL.GetRoleList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MIM.RegisterUser requestSetRegisterUser, string command)
        {
            MIM.User requestSetUser = new MIM.User();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                    return RedirectToAction("Register");
                
            if (command == "Close")
                return RedirectToAction("MobikonIMS", "MobikonIMS");
          
            ViewBag.roleName = roleDAL.GetRoleList();
            if (command == "Save")
            {
                if (ModelState.IsValid)
                {
                    if (true == userDAL.CheckUserExist(requestSetRegisterUser.userName))
                    {
                        ViewBag.Message = "User name already exists.";
                        return View();
                    }

                    requestSetUser.Password = requestSetRegisterUser.Password;
                    requestSetUser.currentPassword = requestSetRegisterUser.Password;
                    requestSetUser.userName = requestSetRegisterUser.userName;
                    requestSetUser.userID = requestSetRegisterUser.userID;
                    requestSetUser.roleName = requestSetRegisterUser.roleName;
                    requestSetUser.roleID = requestSetRegisterUser.roleID;
                    requestSetUser.email = requestSetRegisterUser.email;
                    requestSetUser.activated = true;
                    requestSetUser.lockedOutEnabled = false;

                    //bool checkCurrentPassword = userDAL.CheckCurrentPassword(requestSetUser);
                    //if ( checkCurrentPassword == false)
                    //{
                    //    ViewBag.Message = "Current Password is wrong";
                    //    return RedirectToAction("Users");
                    //}         
                    bool result = userDAL.Register(requestSetUser);

                    if (result == true)
                        return RedirectToAction("Users");
                    else
                        ViewBag.Message = "User not registered successfully.";
                } 
               
                return View(requestSetUser);                
            }
            return RedirectToAction("Login","Login");
        }

        public ActionResult Delete()
        {
            if (Session["UserName"] != null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Delete ";

            MID.UserDAL userDAL = new MID.UserDAL();
            ViewBag.userName = userDAL.GetUserNameList();
            return View();
        }

        public ActionResult ResetPassword(long userID,bool statusUser)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Reset Password";

            MIM.ResetPassword responseGetResetPassword = new MIM.ResetPassword();

            responseGetResetPassword = userDAL.GetResetPasswordUser(userID, statusUser);
            return View(responseGetResetPassword);           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(MIM.ResetPassword requestSetResetPassword, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            if (command == "Cancel")
                return RedirectToAction("ResetPassword");

            if (command == "Close")
                return RedirectToAction("Login", "Login");

            if (command == "Save")
            {
                if (ModelState.IsValid)
                {
                    MIM.User requestSetUser = new MIM.User();                   

                    requestSetUser.Password = requestSetResetPassword.Password;                   
                    requestSetUser.userName = requestSetResetPassword.userName;
                    requestSetUser.userID= requestSetResetPassword.userID;
                    requestSetUser.email = requestSetResetPassword.email;

                    requestSetUser.resetPassword = true;
                   
                    if ( true == userDAL.ResetPassword(requestSetUser))                                          
                        return RedirectToAction("Users");                    
                    else
                        ViewBag.message = "Password not changed successfully.";
                }
               
                return View(requestSetResetPassword);
            }
            return RedirectToAction("Login", "Login");
        }
        public ActionResult EditUser(long userID, bool statusUser)
        {
            MIM.User responseGetUser = new MIM.User();

            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");

            //if (string.IsNullOrEmpty(userName)
            if (userID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.userName = "Welcome " + Session["UserName"] + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];

            ViewBag.menuName = "Edit User";

            responseGetUser = userDAL.GetUser(userID: userID, userStatus: statusUser);
            ViewBag.roleName = roleDAL.GetRoleList(selectedRoleName: responseGetUser.roleName);

            if (string.IsNullOrEmpty(responseGetUser.userName))
            {
                return HttpNotFound();
            }

            return View(responseGetUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(MIM.User requestSetUser, string command)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");


            if (command == "Cancel")
            {
                return RedirectToAction("EditUser", "Login", new { userID = requestSetUser.userID });
            }

            if (command == "Close")
                return RedirectToAction("Users");
           
            ViewBag.roleName = roleDAL.GetRoleList(selectedRoleName: requestSetUser.roleName);

            if (command == "Save")
            {
                //if (ModelState.IsValid)
                //{

                    if (true == userDAL.EditUser(requestSetUser))
                        return RedirectToAction("Users");
                    else
                        ViewBag.Message = "User not updated successfully.";
                //}              

                //return View(requestSetUser);
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult Users(string sortOrder, string userNameFilter, string roleNameFilter, string userNameSearch, string roleNameSearch, int? page, string statusUserSearch, string paging)
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");          
            
            ViewBag.userName = "Welcome " + Session["UserName"]  + " (" + Session["Role"] + ")";
            ViewBag.loginDateTime = Session["LoginDateTime"];
            ViewBag.menuName = "User List";

            List<MIM.User> responseGetUser = new List<MIM.User>();
            List<MIM.User> responseUser = new List<MIM.User>();
            bool statusUser = true;

            ViewBag.currentSort = sortOrder;
            //ViewBag.pagingList = mC.CommonMobikonIMS.FillPaging();

            if (!string.IsNullOrEmpty(roleNameSearch))
                ViewBag.roleNameSearch = roleDAL.GetRoleList(pageName:"Users");
            else
                ViewBag.roleNameSearch = roleDAL.GetRoleList(pageName: "Users",selectedRoleName: userNameFilter);

            if (String.IsNullOrEmpty(sortOrder))
                sortOrder = "userNameSort";

            //ViewBag.paging = mC.CommonMobikonIMS.FillPaging();
            ViewBag.userNameSort = sortOrder == "userNameSort" ? "userNameSortDesc" : "userNameSort";
            ViewBag.roleNameSort = sortOrder == "roleNameSort" ? "roleNameSortDesc" : "roleNameSort";

            int pageSize = 0;
            if (string.IsNullOrEmpty(paging))
                pageSize = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["PageSize"]); //mC.CommonMobikonIMS.selectedPageSize;
            else
                pageSize = Convert.ToInt16(paging);

            if (statusUserSearch == "false")
                statusUser = false;               

            if (userNameSearch != null)
                page = 1;
            //else
            //    userNameSearch = currentFilter;

            if (roleNameSearch != null)
                page = 1;
            if (userNameSearch != null)
                page = 1;

            if (!string.IsNullOrEmpty(userNameSearch))
                ViewBag.userNameFilter = userNameSearch;
            else
            {
                userNameSearch = userNameFilter;
                ViewBag.cityNameFilter = userNameSearch;
            }

            if (!string.IsNullOrEmpty(roleNameSearch))
                ViewBag.roleNameFilter = roleNameSearch;
            else
            {
                roleNameSearch = roleNameFilter;
                ViewBag.roleNameFilter = roleNameSearch;
            }

            responseGetUser = userDAL.GetStatuswiseUsers(statusUser);
            var users = responseGetUser.AsQueryable();

            if (!String.IsNullOrEmpty(userNameSearch))
                users = users.Where(user => user.userName.ToUpper().Trim().Contains(userNameSearch.ToUpper().Trim()));

            if (!String.IsNullOrEmpty(roleNameSearch))
            {
                if (roleNameSearch != "All")
                    users = users.Where(user => user.roleName.ToUpper().Trim().Contains(roleNameSearch.ToUpper().Trim()));
            }

            switch (sortOrder)
            {
                case "userNameSortDesc":
                    users = users.OrderByDescending(user => user.userName);
                    break;
                case "roleNameSortDesc":
                    users = users.OrderByDescending(user => user.roleName);
                    break;
                case "roleNameSort":
                    users = users.OrderBy(user => user.roleName);
                    break;
                default:  // Name ascending 
                    users = users.OrderBy(user => user.userName);
                    break;
            }

            foreach (var response in users)
            {
                responseUser.Add(new MIM.User()
                {
                    userID = response.userID,
                    userName = response.userName,
                    roleID = response.roleID,
                    email = response.email,
                    roleName = response.roleName,
                    activated = response.activated,
                    lockedOutEnabled = response.lockedOutEnabled,
                    resetPassword = response.resetPassword,
                    seperator = mC.CommonMobikonIMS.seperator
                });
            }

            ViewBag.userCount = "Total Users: " + responseUser.Count;
            int pageNumber = (page ?? 1);
            return View(responseUser.ToPagedList(pageNumber, pageSize));                
        }

        public ActionResult UserDetails(long userID, bool statusUser)
        {
            MIM.User responseGetUser= new MIM.User();
           
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Login");
                
            if (userID == 0)                    
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);                    

            responseGetUser= userDAL.GetUser(userID, statusUser);
            responseGetUser.seperator = mC.CommonMobikonIMS.seperator;

            if (string.IsNullOrEmpty(responseGetUser.userName))
            {
                return HttpNotFound();
            }
            return View(responseGetUser); 
        }
    }
}
