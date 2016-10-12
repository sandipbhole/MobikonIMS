using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{
    public class RoleDAL
    {       
        private static readonly ILog logger = LogManager.GetLogger(typeof(RoleDAL));

        public MIM.User responseGetUser = new MIM.User();


        public RoleDAL()
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

        public List<SelectListItem> GetRoleList(string pageName = "", System.Nullable<int> selectedRoleID = 0, string selectedRoleName = "")
        {
            EntityConnection entityConnection = new EntityConnection();
            List<SelectListItem> roleList = new List<SelectListItem>();

            try
            { 
                var selectRole = from role in entityConnection.dbMobikonIMSDataContext.ROLEs
                                 where role.ACTIVATED == true
                                 select role;

                foreach (var response in selectRole)
                {
                    if (selectedRoleID <= 0 && string.IsNullOrEmpty(selectedRoleName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.ROLENAME,
                            Value = response.ROLENAME
                        });
                    }
                    if (selectedRoleID > 0 && string.IsNullOrEmpty(selectedRoleName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.ROLENAME,
                            Value = response.ROLENAME,
                            Selected = selectedRoleID == response.ROLEID ? true : false,
                        });
                    }
                    if (!string.IsNullOrEmpty(selectedRoleName) && selectedRoleID <= 0)
                    {
                        if (selectedRoleName != "All")
                        {
                            roleList.Add(new SelectListItem
                            {
                                Text = response.ROLENAME,
                                Value = response.ROLENAME,
                                Selected = selectedRoleName == response.ROLENAME ? true : false,
                            });
                        }
                        else
                        {
                            roleList.Add(new SelectListItem
                            {
                                Text = response.ROLENAME,
                                Value = response.ROLENAME
                            });
                        }
                    }
                }

                if (pageName == "Users")
                {
                    if (string.IsNullOrEmpty(selectedRoleName) || selectedRoleName == "All")
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All",
                            Selected = true

                        });
                    }
                    else
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All"
                        });
                    }
            }
            return roleList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);               
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

        internal int FindRoleID(string roleName)
        {
            EntityConnection entityConnection = new EntityConnection();
            int roleID = 0;

            try
            {
                var selectRole = from role in entityConnection.dbMobikonIMSDataContext.ROLEs
                                 where role.ROLENAME == roleName
                                 select role;

                foreach (var response in selectRole)
                {
                    roleID = response.ROLEID;
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
    }
}
