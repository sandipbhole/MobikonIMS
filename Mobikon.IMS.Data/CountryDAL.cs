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
    public class CountryDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CountryDAL));

        public MIM.Country responseGetCountry = new MIM.Country();
        public bool resetPassword = false;

        public CountryDAL()
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

        internal int FindCountryID()
        {
            EntityConnection entityConnection = new EntityConnection();
            int countryID = 0;
            try
            {
                var selectCountry = from country in entityConnection.dbMobikonIMSDataContext.COUNTRies
                                    select country;
                foreach (var response in selectCountry)
                {
                    countryID = response.COUNTRYID;
                }
                return countryID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
               
                return countryID;
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

        internal int FindCountryID(string countryName)
        {
            EntityConnection entityConnection = new EntityConnection();
            int countryID = 0;

            try {
                var selectCountry = from country in entityConnection.dbMobikonIMSDataContext.COUNTRies
                                    where country.COUNTRYNAME == countryName
                                    select country;

                foreach (var response in selectCountry)
                {
                    countryID = response.COUNTRYID;
                }
                return countryID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
               
                return countryID ;
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

        public List<SelectListItem> GetCountryNameList(string pageName="", System.Nullable<int> selectedCountryID = 0,string selectedCountryName="")
        {
            logger.Info("GetCountryNameList");
            List<SelectListItem> roleList = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var selectCountry = from country in entityConnection.dbMobikonIMSDataContext.COUNTRies
                                    orderby country.COUNTRYNAME
                                    select country;

                foreach (var response in selectCountry)
                {
                    if (selectedCountryID >= 1 && string.IsNullOrEmpty(selectedCountryName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.COUNTRYNAME,
                            Value = response.COUNTRYNAME,
                            Selected = selectedCountryID == response.COUNTRYID ? true : false,
                        });
                    }
                    else if (selectedCountryID <= 0 && !string.IsNullOrEmpty(selectedCountryName))
                    {
                        if (selectedCountryName != "All")
                        {
                            roleList.Add(new SelectListItem
                            {
                                Text = response.COUNTRYNAME,
                                Value = response.COUNTRYNAME,
                                Selected = selectedCountryName == response.COUNTRYNAME ? true : false,
                            });
                        }
                        else
                        {
                            roleList.Add(new SelectListItem
                            {
                                Text = response.COUNTRYNAME,
                                Value = response.COUNTRYNAME                              
                            });
                        }                     
                    }
                    else if (selectedCountryID >= 1 && !string.IsNullOrEmpty(selectedCountryName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.COUNTRYNAME,
                            Value = response.COUNTRYNAME,
                            Selected = selectedCountryID == response.COUNTRYID ? true : false,
                        });
                    }
                    else if (selectedCountryID <= 0 && string.IsNullOrEmpty(selectedCountryName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.COUNTRYNAME,
                            Value = response.COUNTRYNAME
                        });
                    }
                }

                if (pageName == "Client" || pageName == "Outlet" || pageName == "DeviceHistory")
                {
                    if (string.IsNullOrEmpty(selectedCountryName) || selectedCountryName == "All")
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
    }
}
