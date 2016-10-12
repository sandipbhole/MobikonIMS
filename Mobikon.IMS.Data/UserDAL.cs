using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using Microsoft.Win32;
using System.Reflection;

using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{
    public class UserDAL
    {     
        private static readonly ILog logger = LogManager.GetLogger(typeof(UserDAL));

        public MIM.User responseUser = new MIM.User();
        public bool resetPassword = false;

        public UserDAL()
        {
            InitializeLog4Net();
        }

        private void InitializeLog4Net()
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.AppendToFile = true;
            appender.Name = "ServiceLogger";
            string path = "C:\\MobikonIMS"; //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            appender.File = path + "\\MobikonIMS_" + DateTime.Now.ToString("dd-MM-yyyy") + ".log";

            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout
            {
                ConversionPattern = "%date{{yyyy-MM-dd-HH:mm:ss.fff}} [%thread] %-5level %logger - %message%newline"
            };
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            //appender.MaxFileSize = 10240;
            //appender.MaxSizeRollBackups = 5;

            log4net.Repository.Hierarchy.Hierarchy repository = LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
            repository.Root.AddAppender(appender);
            repository.Root.Level = Level.All;
            repository.Configured = true;
            repository.RaiseConfigurationChanged(EventArgs.Empty);

            //logger.Info("Log4NET initialized successfully.");
        }
    
        public bool CheckUserExist(string userName)
        {          
            EntityConnection entityConnection = new EntityConnection();
            MIM.User responseGetUser = new MIM.User();

            try
            {
                var checkUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                where user.USERNAME == userName
                                select user;
                
                foreach (var response in checkUser)
                {
                    responseGetUser.userName = response.USERNAME;
                }

                if (!string.IsNullOrEmpty(responseGetUser.userName))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);               

                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        
        public bool LicenseExpired()
        {
            //string path = @"C:\ProgramData\MobikonIMS";
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);

            path = path.Replace("file:\\", "");

            bool exists = System.IO.Directory.Exists(path);

            //if (!exists)
            //    System.IO.Directory.CreateDirectory(path);

            path = Path.Combine(path, "Mobikon.dll");
            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                //Create a file to write to. 
                using (StreamWriter streamWriter = fileInfo.CreateText())
                {                   
                    streamWriter.WriteLine(Base64Encode(DateTime.Now.AddMonths(6).ToShortDateString()));
                }
            }

            //Open the file to read from. 
            using (StreamReader streamReader = fileInfo.OpenText())
            {
                string s = "";
                while ((s = streamReader.ReadLine()) != null)
                {
                    if (Convert.ToDateTime(DateTime.Now.ToShortDateString()) > Convert.ToDateTime(Base64Decode(s)))
                        return true;
                }

                if (string.IsNullOrEmpty(s))
                    return false;
                else
                    return true;
            }           
        }

        public List<MIM.User> GetUserEmail()
        {            
            EntityConnection entityConnection = new EntityConnection();
            List<MIM.User> responseGetUser = new List<Message.User>();

            try
            { 
                var userList = from user in entityConnection.dbMobikonIMSDataContext.USERs
                               where user.ACTIVATED == true && user.EMAIL != null 
                               orderby user.USERNAME
                           select user;

                foreach (var response in userList)
                {
                    responseGetUser.Add(new MIM.User
                    {
                        email = response.EMAIL,
                        userName = response.USERNAME
                    });
                }
                return responseGetUser;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);                

                return responseGetUser;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }

        }

        public int GetUserTotalCount()
        {           
            EntityConnection entityConnection = new EntityConnection();
            int totalCount = 0;

            try
            {
                totalCount = (from user in entityConnection.dbMobikonIMSDataContext.USERs
                                  select user).Count();
                return totalCount;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);                

                return totalCount;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }
        
        //Code to check current password
        public bool CheckCurrentPassword(MIM.User requestSetUser)
        {           
            EntityConnection entityConnection = new EntityConnection();
            MIM.User responseGetUser = new MIM.User();

            try
            {
                var checkUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                where user.PASSWORD == Base64Encode(requestSetUser.currentPassword) && user.USERNAME == requestSetUser.userName
                                select user;

                foreach (var response in checkUser)
                {
                    responseGetUser.currentPassword = response.PASSWORD;
                }

                if (!string.IsNullOrEmpty(responseGetUser.Password))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);              

                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private string FindRole(int roleID)
        {
            EntityConnection entityConnection = new EntityConnection();
            string roleName=string.Empty;

            try
            {
                var selectRole = from roleList in entityConnection.dbMobikonIMSDataContext.ROLEs
                                 where roleList.ROLEID == roleID
                                 select roleList;

                foreach (var response in selectRole)
                {
                    roleName = response.ROLENAME;
                }
                return roleName;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);                

                return roleName;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        internal long FindUserID(string userName)
        {
            EntityConnection entityConnection = new EntityConnection();
            long userID = 0;

            try
            {
                var selectUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                 where user.USERNAME == userName
                                 select user;

                foreach (var response in selectUser)
                {
                    userID = response.USERID;
                }
                return userID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);                

                return userID;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        private int FindRoleID(string roleName)
        {
            int roleID = 0;
            EntityConnection entityConnection = new EntityConnection();
            try
            { 
                var selectRole = from roleList in entityConnection.dbMobikonIMSDataContext.ROLEs
                                 where roleList.ROLENAME == roleName
                                 select roleList;

                foreach (var response in selectRole)
                {
                    roleID= response.ROLEID;
                }
                return roleID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);               

                return roleID;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        //public int ExpirationChecking()
        //{
        //    logger.Info("LoginDAL.ExpirationChecking");

        //    int result = 0;

        //    var selectExpirationChecking = entityConnection.dbMobikonIMSDataContext.EXPIRATIONCHECKING();
        //    foreach (var response in selectExpirationChecking)
        //    {
        //        result = response.RESULT;
        //    }
        //    return result;
        //}

        public List<SelectListItem> GetUserNameList(string pageName="")
        {
            logger.Info("GetAllUserName");            
            List<SelectListItem> roleList = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                if (pageName == "DeviceHistory")
                {
                    roleList.Add(new SelectListItem
                    {
                        Text = "All",
                        Value = "All",
                        Selected = true
                    });
                }

                var selectUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                 where user.ACTIVATED == true
                                 orderby user.USERNAME
                                 select user;

                foreach (var response in selectUser)
                {
                    roleList.Add(new SelectListItem
                    {
                        Text = response.USERNAME,
                        Value = response.USERNAME,
                    });
                }

                return roleList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();

                return roleList;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public List<MIM.User> GetStatuswiseUsers(bool userStatus)
        {
            List<MIM.User> responseGetUser = new List<MIM.User>();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var selectUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                 join role in entityConnection.dbMobikonIMSDataContext.ROLEs on user.ROLEID equals role.ROLEID
                                 where user.ACTIVATED == userStatus
                                 select new
                                 {
                                     user.USERID,
                                     user.USERNAME,
                                     user.ACTIVATED,
                                     user.ROLEID,
                                     role.ROLENAME,
                                     user.RESETPASSWORD,
                                     user.LOCKEDOUTENABLED,
                                     user.EMAIL
                                 };

                foreach (var response in selectUser)
                {
                    responseGetUser.Add(new MIM.User()
                    {
                       userID = response.USERID,
                       userName = response.USERNAME,
                       activated = response.ACTIVATED,
                       roleID = response.ROLEID,
                       roleName = response.ROLENAME,
                       resetPassword = response.RESETPASSWORD,
                       lockedOutEnabled = response.LOCKEDOUTENABLED,
                       email = response.EMAIL
                    });
                }
                return responseGetUser;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();

                return responseGetUser;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public MIM.ResetPassword GetResetPasswordUser(long userID, bool userStatus)
        {
            MIM.ResetPassword responseGetResetPassword = new MIM.ResetPassword();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var selectResetPassword = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                 join role in entityConnection.dbMobikonIMSDataContext.ROLEs on user.ROLEID equals role.ROLEID
                                 where user.ACTIVATED == userStatus && user.USERID == userID
                                 select new
                                 {
                                     user.USERID,
                                     user.USERNAME,
                                     user.ROLEID,
                                     role.ROLENAME,
                                     user.RESETPASSWORD,
                                     user.LOCKEDOUTENABLED,
                                     user.EMAIL,
                                     user.ACTIVATED
                                 };

                foreach (var response in selectResetPassword)
                {
                    responseGetResetPassword.userID = response.USERID;
                    responseGetResetPassword.userName = response.USERNAME;                   
                    responseGetResetPassword.resetPassword = response.RESETPASSWORD;
                    responseGetResetPassword.email = response.EMAIL;               
                }
                return responseGetResetPassword;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();

                return responseGetResetPassword;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public MIM.User GetUser(long userID,bool userStatus)
        {
            MIM.User responseGetUser = new MIM.User();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var selectUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                 join role in entityConnection.dbMobikonIMSDataContext.ROLEs on user.ROLEID equals role.ROLEID
                                 where user.ACTIVATED == userStatus && user.USERID == userID
                                 orderby user.USERNAME 
                                 select new
                                 {
                                     user.USERID,
                                     user.USERNAME,                                   
                                     user.ROLEID,
                                     role.ROLENAME,
                                     user.RESETPASSWORD,
                                     user.LOCKEDOUTENABLED,
                                     user.EMAIL,
                                     user.ACTIVATED
                                 };

                foreach (var response in selectUser)
                {
                    responseGetUser.userID = response.USERID;
                    responseGetUser.userName = response.USERNAME;
                    responseGetUser.activated = response.ACTIVATED;
                    responseGetUser.roleName = response.ROLENAME;
                    responseGetUser.roleID = response.ROLEID;
                    responseGetUser.resetPassword = response.RESETPASSWORD;
                    responseGetUser.lockedOutEnabled = response.LOCKEDOUTENABLED;
                    responseGetUser.email = response.EMAIL;                   
                }
                return responseGetUser;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);               
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();

                return responseGetUser;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }
        public bool EditUser(MIM.User requestSetUser)
        {          
            EntityConnection entityConnection = new EntityConnection();
            RoleDAL roleDAL = new RoleDAL();
         
            try
            {
                var updateUser = from user in entityConnection.dbMobikonIMSDataContext.USERs
                                   where user.USERID == requestSetUser.userID
                                   select user;

                foreach (var user in updateUser)
                {
                    user.ROLEID = roleDAL.FindRoleID(requestSetUser.roleName);
                    user.USERNAME = requestSetUser.userName;
                    user.EMAIL = requestSetUser.email;                            
                    user.ACTIVATED = requestSetUser.activated;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                return true;
            }
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                    {
                        // All database values overwrite current values.
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public bool Register(MIM.User requestSetUser)
        {           
            EntityConnection entityConnection = new EntityConnection();
            USER user = new USER();

            try
            {
                bool result = false;
                //user.userID = requestSetUser.userID;
                user.USERNAME = requestSetUser.userName;
                user.PASSWORD = Base64Encode(requestSetUser.Password);
                user.LOCKEDOUTENABLED = requestSetUser.lockedOutEnabled;
                user.EMAIL = requestSetUser.email;
                user.ACTIVATED = requestSetUser.activated;
                user.ROLEID = FindRoleID(requestSetUser.roleName);
                user.RESETPASSWORD = true;

                // Add the new object to the users collection.
                entityConnection.dbMobikonIMSDataContext.USERs.InsertOnSubmit(user);
                // Submit the change to the database.            
                entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                result = true;

                if (result == true)
                    SendEmail(requestSetUser.userName, requestSetUser.Password, requestSetUser.email, "Register");

                return result;
            }            
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                //Console.WriteLine(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                    {
                        // All database values overwrite current values.
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        private void SendEmail(string username,string mobikonIMSPassword,string email,string taskName)
        {
            logger.Info("SendEmail");

            List<MIM.User> responseGetUser = new List<MIM.User>();
            List<MIM.Settings> responseGetSystemSettings = new List<MIM.Settings>();

            string emailList = string.Empty;
            string userName = string.Empty;
            string password = string.Empty;
            string smtpServer = string.Empty;
            string enableSsl = string.Empty;
            int port = 0;
            MailMessage mailMessage = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();
          
            SettingsDAL systemSettingsDAL = new SettingsDAL();

            try
            {
                responseGetSystemSettings = systemSettingsDAL.GetAllSettings();
               
                foreach (MIM.Settings response in responseGetSystemSettings)
                {
                    if (response.settingsName == "User Name")
                    {
                        if (string.IsNullOrEmpty(response.settingsValue))
                        {
                            logger.Error("Email address is blank.");
                            return;
                        }
                        else
                            userName = response.settingsValue;
                    }

                    if (response.settingsName == "Password")
                    {
                        if (string.IsNullOrEmpty(response.settingsValue))
                        {
                            logger.Error("Password is blank.");
                            return;
                        }
                        else
                            password = Base64Decode(response.settingsValue);
                    }

                    if (response.settingsName == "SMTP")
                    {
                        if (string.IsNullOrEmpty(response.settingsValue))
                        {
                            logger.Error("SMTP server is blank.");
                            return;
                        }
                        else
                            smtpServer = response.settingsValue;
                    }

                    if (response.settingsName == "EnableSsl")
                    {
                        if (string.IsNullOrEmpty(response.settingsValue))
                        {
                            logger.Error("EnableSsl is blank.");
                            return;
                        }
                        else
                            enableSsl = response.settingsValue;
                    }

                    if (response.settingsName == "Port")
                    {
                        if (string.IsNullOrEmpty(response.settingsValue) || response.settingsValue == "0")
                        {
                            logger.Error("SMTP port is blank or Zero.");
                            return;
                        }
                        else
                            port = Convert.ToInt32(response.settingsValue);
                    }
                }

                mailMessage.From = new MailAddress(userName, "Konekt Marketing Systems Pvt. Ltd.");  

                smtpClient.Host = smtpServer;
                smtpClient.Port = port;
                smtpClient.UseDefaultCredentials = true;

                mailMessage.To.Add(email);
                
                string body = string.Empty;

                if (taskName == "Register")
                {
                    mailMessage.Subject = "Welcome to MobikonIMS";
                    body = "Hi " + username + "," + "<p> Thank you signing up on MobikonIMS. Here is your username and password. </p>";
                    body = body + "<p> UserName: " + username + "<br/> Password: " + mobikonIMSPassword + " <br/> Login URL: http://beta.mobikontech.com/MobikonIMS/ </p>";
                    body = body + "<p> Please do not reply to this email. </p>";
                }
                else if (taskName == "Reset Password")
                {
                    mailMessage.Subject = "Reset Password";
                    body = "Hi " + username + "," + "<p> Thank you resetting password successfully. Here is your new password. </p>";
                    body = body + "<p> UserName: " + username + "<br/> New Password: " + mobikonIMSPassword + " <br/> Login URL: http://beta.mobikontech.com/MobikonIMS/ </p>";
                    body = body + "<p> Please do not reply to this email. </p>";
                }
                else if (taskName == "Forget Password")
                {
                    mailMessage.Subject = "Forget Password";
                    body = "Hi " + username + "," + "<p> Thank you retrieving password successfully. Here is your password. </p>";
                    body = body + "<p> UserName: " + username + "<br/> Password: " + mobikonIMSPassword + " <br/> Login URL: http://beta.mobikontech.com/MobikonIMS/ </p>";
                    body = body + "<p> Please do not reply to this email. </p>";
                }

                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                if (enableSsl == "True")
                {
                    smtpClient.EnableSsl = true;
                    NetworkCredential networkCredential = new NetworkCredential(userName, password);
                    smtpClient.Credentials = networkCredential;
                }
                else
                {
                    smtpClient.EnableSsl = false;
                }
                //smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                smtpClient.Send(mailMessage);

                logger.Info("Email send successfully.");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                smtpClient.Dispose();
                mailMessage.Dispose();

            }
            finally
            {
                smtpClient.Dispose();
                mailMessage.Dispose();
            }
        }

        public bool DeleteUser(MIM.User requestSetUser)
        {           
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var updateUserList = from userList in entityConnection.dbMobikonIMSDataContext.USERs
                                     where userList.USERID == requestSetUser.userID && userList.ACTIVATED == false
                                     select userList;

                foreach (USER userList in updateUserList)
                {
                    userList.ACTIVATED = true;
                 
                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                return true;
            }            
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                //Console.WriteLine(ex.Message);
                foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                {
                    // All database values overwrite current values.
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }             
               entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
              
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
               
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }

        }

        public bool ResetPassword(MIM.User requestSetUser)
        {
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                bool result = false;
                var updateUserList = from userList in entityConnection.dbMobikonIMSDataContext.USERs
                                     where userList.USERID == requestSetUser.userID && userList.USERNAME == requestSetUser.userName && userList.ACTIVATED == true
                                     select userList;

                foreach (USER userList in updateUserList)
                {
                    userList.PASSWORD = Base64Encode(requestSetUser.Password);
                    userList.RESETPASSWORD = true;
                    //userList.EMAIL = requestSetUser.email;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                result = true;

                if (result == true)
                    SendEmail(requestSetUser.userName, requestSetUser.Password, requestSetUser.email, "Reset Password");

                return true;
            }           
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                //Console.WriteLine(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                    {
                        // All database values overwrite current values.
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }             
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public bool ForgetPassword(MIM.User requestSetUser)
        {
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                string password = string.Empty;
                string email = string.Empty;

                if (requestSetUser.userID == 0)
                    requestSetUser.userID = FindUserID(requestSetUser.userName);


                var forgetPassword = from userList in entityConnection.dbMobikonIMSDataContext.USERs
                                     where userList.USERID == requestSetUser.userID && userList.USERNAME == requestSetUser.userName && userList.ACTIVATED == true
                                     select userList;

                foreach (USER response in forgetPassword)
                {
                    password = Base64Decode(response.PASSWORD);
                    email = response.EMAIL;
                }

                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(email))
                    SendEmail(requestSetUser.userName, password, email, "Forget Password");

                return true;
            }
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                //Console.WriteLine(ex.Message);
                foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                {
                    // All database values overwrite current values.
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
                entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }


        public bool ChangePassword(MIM.User requestSetUser)
        {           
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                if (requestSetUser.userID == 0)
                    requestSetUser.userID = FindUserID(requestSetUser.userName);


                var updateUserList = from userList in entityConnection.dbMobikonIMSDataContext.USERs
                                    where userList.USERID == requestSetUser.userID && userList.PASSWORD == Base64Encode(requestSetUser.currentPassword) && userList.ACTIVATED == true
                                    select userList;

                foreach (USER userList in updateUserList)
                {
                    userList.PASSWORD = Base64Encode(requestSetUser.Password);
                    userList.RESETPASSWORD = false;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                return true;
            }           
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                //Console.WriteLine(ex.Message);
                foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                {
                    // All database values overwrite current values.
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
               entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
               
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }             
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public bool AuthenticateUser(MIM.Login requestSetLogin)
        {
            EntityConnection entityConnection = new EntityConnection();
            //MobikonIMSDataContext dbMobikonIMSDataContext = new MobikonIMSDataContext();

            try
            {
                //var selectUser = CompiledQuery.Compile((MobikonIMSDataContext context) => from userList in context.USERs
                //                                      join roleList in context.ROLEs on userList.ROLEID equals roleList.ROLEID
                //                                      where userList.USERNAME == requestSetLogin.userName && userList.PASSWORD == Base64Encode(requestSetLogin.password) && userList.ACTIVATED == true

                //                                      select new
                //                                      {
                //                                          userList.USERID,
                //                                          userList.USERNAME,
                //                                          userList.ACTIVATED,
                //                                          userList.ROLEID,
                //                                          roleList.ROLENAME,
                //                                          userList.RESETPASSWORD
                //                                      });

                var selectUser = from userList in entityConnection.dbMobikonIMSDataContext.USERs
                                 join roleList in entityConnection.dbMobikonIMSDataContext.ROLEs on userList.ROLEID equals roleList.ROLEID
                                 where userList.USERNAME == requestSetLogin.userName && userList.PASSWORD == Base64Encode(requestSetLogin.password) && userList.ACTIVATED == true

                                 select new
                                 {
                                     userList.USERID,
                                     userList.USERNAME,
                                     userList.ACTIVATED,
                                     userList.ROLEID,
                                     roleList.ROLENAME,
                                     userList.RESETPASSWORD
                                 };
                //foreach (var response in selectUser(dbMobikonIMSDataContext))
                foreach (var response in selectUser)
                {
                    responseUser.userID = response.USERID;
                    responseUser.userName = response.USERNAME.Trim();
                    responseUser.roleID = response.ROLEID;
                    responseUser.roleName = response.ROLENAME.Trim();
                    responseUser.activated = response.ACTIVATED;
                    responseUser.resetPassword = response.RESETPASSWORD;
                }

                if (string.IsNullOrEmpty(responseUser.userName))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
               

                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }
    }
}
