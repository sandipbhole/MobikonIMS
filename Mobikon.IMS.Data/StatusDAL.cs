using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Data;
using System.Web.Mvc;

using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{
    public class StatusDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(StatusDAL));

        public MIM.Status responseGetStatus = new MIM.Status();
        public bool resetPassword = false;

        public StatusDAL()
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

        internal int FindStatusID(string statusName)
        {
            EntityConnection entityConnection = new EntityConnection();
            int statusID = 0;

            try
            {
                var selectStatus = from status in entityConnection.dbMobikonIMSDataContext.STATUS
                                   where status.STATUSNAME == statusName
                                   select status;
                foreach (var response in selectStatus)
                {
                    statusID = response.STATUSID;
                }
                return statusID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return statusID;
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

        public MIM.Status GetStatusID(string statusName)
        {
            logger.Info("GetStatusID");

            EntityConnection entityConnection = new EntityConnection();
            MIM.Status responseGetStatus = new MIM.Status();

            try
            {
                var selectStatus = from status in entityConnection.dbMobikonIMSDataContext.STATUS
                                   where status.STATUSNAME == statusName
                                   select status;
                foreach (var response in selectStatus)
                {
                    responseGetStatus.statusID = response.STATUSID;
                    responseGetStatus.statusName = response.STATUSNAME;
                }
                return responseGetStatus;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);               
                return responseGetStatus;
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

        public List<SelectListItem> GetStatusNameList(string pageName="",System.Nullable<int> selectedStatusID = 0,string selectedStatusName = "",string role = "")
        {
            logger.Info("GetStatusNameList");
            List<SelectListItem> statusList = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                if (role == "Sales")
                {
                    statusList.Add(new SelectListItem
                    {
                        Text = "In Stock",
                        Value = "In Stock",
                         Selected = true
                    });
                    statusList.Add(new SelectListItem
                    {
                        Text = "Blocked",
                        Value = "Blocked"                       
                    });
                   
                    return statusList;
                }

                var selectStatus = from status in entityConnection.dbMobikonIMSDataContext.STATUS
                                   select status;

                foreach (var response in selectStatus)
                {
                    if (selectedStatusID >= 1 && string.IsNullOrEmpty(selectedStatusName))
                    {
                        statusList.Add(new SelectListItem
                        {
                            Text = response.STATUSNAME,
                            Value = response.STATUSNAME,
                            Selected = selectedStatusID == response.STATUSID ? true : false
                        });
                    }
                    else if (selectedStatusID <= 0 && !string.IsNullOrEmpty(selectedStatusName))
                    {
                        if (selectedStatusName != "All")
                        {
                            statusList.Add(new SelectListItem
                            {
                                Text = response.STATUSNAME,
                                Value = response.STATUSNAME,
                                Selected = selectedStatusName == response.STATUSNAME ? true : false
                            });
                        }
                        else
                        {
                            statusList.Add(new SelectListItem
                            {
                                Text = response.STATUSNAME,
                                Value = response.STATUSNAME                               
                            });
                        }
                    }
                    else
                    {
                        statusList.Add(new SelectListItem
                        {
                            Text = response.STATUSNAME,
                            Value = response.STATUSNAME
                        });
                    }
                }

                if (pageName == "Device" || pageName == "DeviceHistory")
                {
                    if (string.IsNullOrEmpty(selectedStatusName) || selectedStatusName == "All")
                    {
                        statusList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All",
                            Selected = true,
                        });
                    }
                    else
                    {
                        statusList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All"
                        });
                    }
                }
                return statusList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);                 
                return statusList;
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
