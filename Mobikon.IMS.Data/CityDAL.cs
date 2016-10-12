using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Data;
using System.Web;
using MIM = Mobikon.IMS.Message;
using System.Web.Mvc;
using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{

    public class CityDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CityDAL));    
        public MIM.City responseGetCity = new MIM.City();       
        public bool resetPassword = false;

        public CityDAL()
        {
            InitializeLog4Net();
        }

        private void InitializeLog4Net()
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.AppendToFile = true;
            appender.Name = "ServiceLogger";
            string path = "C:\\MobikonIMS";//System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
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

        internal int FindCityID()
        {
            EntityConnection entityConnection = new EntityConnection();
            int cityID = 0;

            try
            {
              
                var selectCity = from city in entityConnection.dbMobikonIMSDataContext.CITies
                                 orderby city.CITYNAME
                                 select city;

                foreach (var response in selectCity)
                {
                    cityID = response.CITYID;
                }
                return cityID;

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
               
                return cityID;
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
        public bool CheckCityExists(string cityName,string countryName)
        {
            logger.Info("CheckCityExists");
            EntityConnection entityConnection = new EntityConnection();
            MIM.City responseGetCity = new MIM.City();
            int selectCity = 0;

            try
            {
                 selectCity = (from city in entityConnection.dbMobikonIMSDataContext.CITies
                                  join country in entityConnection.dbMobikonIMSDataContext.COUNTRies on city.COUNTRYID equals country.COUNTRYID
                                  where city.CITYNAME == cityName && country.COUNTRYNAME == countryName
                                  orderby city.CITYNAME
                                  select new
                                  {
                                      city.CITYNAME,
                                      city.CITYID,
                                      city.COUNTRYID,
                                      country.COUNTRYNAME
                                  }).Count();
                
                if (selectCity == 0)
                    return false;
                else
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


        public bool InsertCity(MIM.City requestSetCity)
        {
            logger.Info("InsertCity");
            EntityConnection entityConnection = new EntityConnection();
            CITY city = new CITY();
            CountryDAL countryDAL = new CountryDAL();

            try
            {
                //city.cityID = requestSetUser.cityID;
                city.CITYNAME = requestSetCity.cityName;
                city.COUNTRYID = countryDAL.FindCountryID(requestSetCity.countryName);
             
                // Add the new object to the citys collection.
                entityConnection.dbMobikonIMSDataContext.CITies.InsertOnSubmit(city);
                // Submit the change to the database.            
                entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();

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
                       entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Fetching)
                {
                       entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
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

        public bool EditCity(MIM.City requestSetCity)
        {
            EntityConnection entityConnection = new EntityConnection();
            logger.Info("EditCity");
            CountryDAL countryDAL = new CountryDAL();

            try
            {
                var updateCityList = from cityList in entityConnection.dbMobikonIMSDataContext.CITies
                                     where cityList.CITYID == requestSetCity.cityID
                                     select cityList;

                foreach (CITY cityList in updateCityList)
                {
                    cityList.CITYNAME = requestSetCity.cityName;
                    cityList.COUNTRYID = countryDAL.FindCountryID(requestSetCity.countryName);

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
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                    {
                        // All database values overwrite current values.
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                       entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                       entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                }
                return false;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public MIM.City GetCityDetails(long cityID)
        {
            logger.Info("GetCityDetails");
            MIM.City responseGetCity = new Message.City();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectCity = from city in entityConnection.dbMobikonIMSDataContext.CITies
                                 join country in entityConnection.dbMobikonIMSDataContext.COUNTRies on city.COUNTRYID equals country.COUNTRYID
                                 where city.CITYID == cityID
                                 orderby city.CITYNAME
                                 select new
                                 {
                                     city.CITYNAME,
                                     city.CITYID,
                                     city.COUNTRYID,
                                     country.COUNTRYNAME
                                 };

                foreach (var response in selectCity)
                {
                    responseGetCity.cityID = response.CITYID;
                    responseGetCity.cityName = response.CITYNAME;
                    responseGetCity.countryID = response.COUNTRYID;
                    responseGetCity.countryName = response.COUNTRYNAME;
                }
                return responseGetCity;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
               
                return responseGetCity;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public List<MIM.City> GetCityList(string countryName="")
        {
            logger.Info("GetCityList");
            List<MIM.City> responseGetCity = new List<Message.City>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectCity = from city in entityConnection.dbMobikonIMSDataContext.CITies                                  
                                   join country in entityConnection.dbMobikonIMSDataContext.COUNTRies on city.COUNTRYID equals country.COUNTRYID                                   
                                   orderby city.CITYNAME
                                   select new
                                   {
                                       city.CITYNAME,
                                       city.CITYID,
                                       city.COUNTRYID,                                      
                                       country.COUNTRYNAME                                      
                                   };
                if (!string.IsNullOrEmpty(countryName))
                {
                    foreach (var response in selectCity.Where(city => city.COUNTRYNAME == countryName))
                    {
                        responseGetCity.Add(new MIM.City()
                        {
                            cityID = response.CITYID,
                            cityName = response.CITYNAME,
                            countryID = response.COUNTRYID,
                            countryName = response.COUNTRYNAME
                        });
                    }
                }
                else
                {
                    foreach (var response in selectCity)
                    {
                        responseGetCity.Add(new MIM.City()
                        {
                            cityID = response.CITYID,
                            cityName = response.CITYNAME,
                            countryID = response.COUNTRYID,
                            countryName = response.COUNTRYNAME
                        });
                    }
                }
                return responseGetCity;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                         
                return responseGetCity;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        internal int FindCityID(string cityName)
        {
            EntityConnection entityConnection = new EntityConnection();
            int cityID = 0;
            try
            {
                var selectCity = from city in entityConnection.dbMobikonIMSDataContext.CITies
                                 where city.CITYNAME == cityName
                                 select city;
                foreach (var response in selectCity)
                {
                    cityID = response.CITYID;
                }
                return cityID;
            }
            catch (Exception ex)
            {              
                logger.Error(ex.Message);
                return cityID;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
                }
            }
        }

        public List<SelectListItem> GetCityNameList(string pageName="", System.Nullable<int> selectedCityID=0,string selectedCityName="")
        {
            logger.Info("GetCityNameList");
            List<SelectListItem> roleList = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var selectCity = from city in entityConnection.dbMobikonIMSDataContext.CITies
                                 orderby city.CITYNAME
                                 select city;

                foreach (var response in selectCity)
                {
                    if (selectedCityID >= 1 && string.IsNullOrEmpty(selectedCityName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.CITYNAME,
                            Value = response.CITYNAME,
                            Selected = selectedCityID == response.CITYID ? true : false,
                        });
                    }
                    else if (selectedCityID <= 0 && !string.IsNullOrEmpty(selectedCityName))
                    {
                        if (selectedCityName != "All")
                        {
                            roleList.Add(new SelectListItem
                            {
                                Text = response.CITYNAME,
                                Value = response.CITYNAME,
                                Selected = selectedCityName == response.CITYNAME ? true : false,
                            });
                        }
                        else
                        {
                            roleList.Add(new SelectListItem
                            {
                                Text = response.CITYNAME,
                                Value = response.CITYNAME                               
                            });
                        }
                    }
                    else if (selectedCityID >= 1 && !string.IsNullOrEmpty(selectedCityName))
                    {
                        roleList.Add(new SelectListItem
                        {
                            Text = response.CITYNAME,
                            Value = response.CITYNAME,
                            Selected = selectedCityID == response.CITYID ? true : false,
                        });
                    } 
                    else 
                    {                      
                        roleList.Add(new SelectListItem
                        {
                            Text = response.CITYNAME,
                            Value = response.CITYNAME   
                                                    
                        });
                    }
                }

                if (pageName == "Client"|| pageName == "Outlet" || pageName == "DeviceHistory")
                {
                    if (string.IsNullOrEmpty(selectedCityName) || selectedCityName == "All")
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
