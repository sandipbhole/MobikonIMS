using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Data;
using System.Web.Mvc;

using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{
    public class OutletDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(OutletDAL));
       
      
        public OutletDAL()
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

        public int GetOutletTotalCount()
        {
            logger.Info("GetOutletTotalCount");
            EntityConnection entityConnection = new EntityConnection();
            int totalCount = 0;
            try
            { 
                totalCount = (from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs
                              select outlet).Count();
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
                      entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();  entityConnection.dbMobikonIMSDataContext.Connection.Close();
                }
            }
        }

        internal bool ChangeOutletActivateStatus(long clientID,bool status)
        {
            logger.Info("ChangeOutletActivateStatus");
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var updateOutlet = from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs
                                   where outlet.CLIENTID == clientID
                                   select outlet;

                foreach (OUTLET outlet in updateOutlet)
                {
                    outlet.ACTIVATED = status;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                return true;
            }
            // Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
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

        internal long FindOutletID(string OutletName)
        {
            EntityConnection entityConnection = new EntityConnection();
            long outletID = 0;
            try
            {
                var selectOutlet = from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs
                                   where outlet.OUTLETNAME == OutletName
                                   select outlet;

                foreach (var response in selectOutlet)
                {
                    outletID = response.OUTLETID;
                }
                return outletID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
               
                return outletID;
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

        public List<SelectListItem> GetOutletName(long OutletID)
        {
            EntityConnection entityConnection = new EntityConnection();
            List<SelectListItem> responseGetOutlet = new List<SelectListItem>(); ;

            try {
                var selectOutlet = from Outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs
                                   where Outlet.OUTLETID == OutletID
                                   select Outlet;

                foreach (var response in selectOutlet)
                {
                    responseGetOutlet.Add(new SelectListItem
                    {
                        Text = response.OUTLETNAME,
                        Value = response.OUTLETNAME,
                        Selected = true,
                    });
                }
                return responseGetOutlet;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                       entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                }
                return responseGetOutlet;
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

        public MIM.Outlet GetOutlet(long outletID = 0, string outletName = "", long clientID = 0, string clientName = "", bool statusOutlet = false)
        {
            logger.Info("GetOutlet");
            MIM.Outlet responseGetOutlet= new MIM.Outlet();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectOutlet = from outlet in entityConnection.dbMobikonIMSDataContext.GETOUTLET(outletID, outletName, clientID, clientName, statusOutlet)                                 
                                   select new
                                   {
                                       outlet.ACTIVATED,
                                       outlet.CLIENTADDRESS,
                                       outlet.CLIENTCITY,
                                       outlet.CLIENTCITYID,
                                       outlet.CLIENTCOUNTRY,
                                       outlet.CLIENTCOUNTRYID,
                                       outlet.CLIENTID,
                                       outlet.CLIENTNAME,
                                       outlet.CREATEDDATE,
                                       outlet.OUTLETADDRESS,
                                       outlet.OUTLETCITY,
                                       outlet.OUTLETCITYID,
                                       outlet.OUTLETCOUNTRY,
                                       outlet.OUTLETCOUNTRYID,
                                       outlet.OUTLETID,
                                       outlet.OUTLETNAME,
                                       outlet.USERID,
                                       outlet.USERNAME
                                   };

                foreach (var response in selectOutlet)
                {
                    responseGetOutlet.clientID = response.CLIENTID;
                    responseGetOutlet.clientName = response.CLIENTNAME;
                    responseGetOutlet.address = response.OUTLETADDRESS;
                    responseGetOutlet.cityID = response.OUTLETCITYID;
                    responseGetOutlet.cityName = response.OUTLETCITY;
                    responseGetOutlet.countryID = response.OUTLETCOUNTRYID;
                    responseGetOutlet.countryName = response.OUTLETCOUNTRY;
                    responseGetOutlet.activated = response.ACTIVATED;
                    responseGetOutlet.userID = response.USERID;
                    responseGetOutlet.userName = response.USERNAME;
                    responseGetOutlet.outletID = response.OUTLETID;
                    responseGetOutlet.outletName = response.OUTLETNAME;
                }
                return responseGetOutlet;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                 
                return responseGetOutlet;
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

        public List<SelectListItem> GetOutletNameList(string pageName= "", long? selectedOutletID = 0,string selectedOutletName = "")
        {
            logger.Info("GetOutletNameList");

            List<SelectListItem> responseGetOutlet = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectClient = from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs   
                                   where outlet.ACTIVATED == true                                
                                   orderby outlet.OUTLETNAME
                                   select new
                                   {
                                      outlet.OUTLETID,
                                      outlet.OUTLETNAME                                     
                                   };

                foreach (var response in selectClient)
                {
                   
                    if (selectedOutletID <= 0 && string.IsNullOrEmpty(selectedOutletName))
                    {
                        responseGetOutlet.Add(new SelectListItem
                        {
                            Text = response.OUTLETNAME,
                            Value = response.OUTLETNAME                            
                        });
                    }
                    else if (selectedOutletID <= 0 && !string.IsNullOrEmpty(selectedOutletName))
                    {
                        if (selectedOutletName != "All")
                        {
                            responseGetOutlet.Add(new SelectListItem
                            {
                                Text = response.OUTLETNAME,
                                Value = response.OUTLETNAME,
                                Selected = selectedOutletName == response.OUTLETNAME ? true : false,
                            });
                        }
                        else
                        {
                            responseGetOutlet.Add(new SelectListItem
                            {
                                Text = response.OUTLETNAME,
                                Value = response.OUTLETNAME
                            });
                        }
                    }
                    else if (selectedOutletID >= 1 && string.IsNullOrEmpty(selectedOutletName))
                    {
                        responseGetOutlet.Add(new SelectListItem
                        {
                            Text = response.OUTLETNAME,
                            Value = response.OUTLETNAME,
                            Selected = selectedOutletID == response.OUTLETID ? true : false,
                        });
                    }
                    else if (selectedOutletID >= 1 && !string.IsNullOrEmpty(selectedOutletName))
                    {

                        if (selectedOutletName != "All")
                        {
                            responseGetOutlet.Add(new SelectListItem
                            {
                                Text = response.OUTLETNAME,
                                Value = response.OUTLETNAME,
                                Selected = selectedOutletID == response.OUTLETID ? true : false,
                            });
                        }
                        else
                        {
                            responseGetOutlet.Add(new SelectListItem
                            {
                                Text = response.OUTLETNAME,
                                Value = response.OUTLETNAME
                            });
                        }
                    }
                }    
                
                if (pageName == "Outlet" || pageName == "DeviceHistory")
                {
                    if(string.IsNullOrEmpty(selectedOutletName) || selectedOutletName == "All")
                    {
                        responseGetOutlet.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All",
                            Selected = true
                        });
                    }
                    else
                    {
                        responseGetOutlet.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All"
                        });
                    }
                }                
                return responseGetOutlet;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                       
                return responseGetOutlet;
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

        public List<SelectListItem> GetClientwiseOutletNameList(string pageName = "", long outletID = 0, long clientID = 0,string clientName = "")
        {
            logger.Info("GetClientwiseOutletNameList");

            List<SelectListItem> responseGetOutlet = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectClient = from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs  
                                   join client    in entityConnection.dbMobikonIMSDataContext.CLIENTs on outlet.CLIENTID equals client.CLIENTID  
                                   where outlet.ACTIVATED == true                         
                                   orderby outlet.OUTLETNAME
                                   select new
                                   {
                                       outlet.OUTLETID,
                                       outlet.OUTLETNAME,
                                       outlet.CLIENTID,
                                       client.CLIENTNAME
                                   };
                if (clientID != 0)
                {
                    foreach (var response in selectClient.Where(client=> client.CLIENTID == clientID))
                    {
                        responseGetOutlet.Add(new SelectListItem
                        {
                            Text = response.OUTLETNAME,
                            Value = response.OUTLETNAME,
                            Selected = outletID == response.OUTLETID ? true : false,
                        });
                    }
                }
                if (!string.IsNullOrEmpty(clientName))
                {
                    foreach (var response in selectClient.Where(client => client.CLIENTNAME == clientName))
                    {
                        responseGetOutlet.Add(new SelectListItem
                        {
                            Text = response.OUTLETNAME,
                            Value = response.OUTLETNAME,
                            Selected = outletID == response.OUTLETID ? true : false,
                        });
                    }
                }

                if (pageName == "DeviceHistory")
                {
                    responseGetOutlet.Add(new SelectListItem
                    {
                        Text = "All",
                        Value = "All",
                        Selected = true
                    });
                }
                return responseGetOutlet;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                    
                return responseGetOutlet;
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

        public List<MIM.OutletRpt> GetOutletListRpt(long outletID = 0, long clientID = 0, bool statusOutlet = false, string outletName = "", string clientName = "")
        {
            logger.Info("GetOutletListRpt");

            List<MIM.OutletRpt> responseGetOutletRpt = new List<MIM.OutletRpt>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectOutlet = from outlet in entityConnection.dbMobikonIMSDataContext.GETOUTLET(outletID, outletName, clientID, clientName, statusOutlet)
                                   select new
                                   {
                                       outlet.ACTIVATED,
                                       outlet.CLIENTADDRESS,
                                       outlet.CLIENTCITY,
                                       outlet.CLIENTCITYID,
                                       outlet.CLIENTCOUNTRY,
                                       outlet.CLIENTID,
                                       outlet.CLIENTNAME,
                                       outlet.CREATEDDATE,
                                       outlet.OUTLETADDRESS,
                                       outlet.OUTLETCITYID,
                                       outlet.OUTLETCOUNTRY,
                                       outlet.OUTLETCOUNTRYID,
                                       outlet.OUTLETID,
                                       outlet.OUTLETNAME,
                                       outlet.USERID,
                                       outlet.USERNAME,
                                       outlet.OUTLETCITY
                                   };

                foreach (var response in selectOutlet)
                {
                    responseGetOutletRpt.Add(new MIM.OutletRpt
                    {
                        OutletName = response.OUTLETNAME,                      
                        UserName = response.USERNAME,                      
                        ClientName = response.CLIENTNAME, 
                        City = response.OUTLETCITY,
                        Country = response.OUTLETCOUNTRY,                      
                        Address = response.OUTLETADDRESS,
                        Activated = response.ACTIVATED == true ? "True" : "False",                      
                        ClientCity = response.CLIENTCITY,
                        ClientAddress = response.CLIENTADDRESS,
                        ClientCountry = response.CLIENTCOUNTRY
                    });
                }
                return responseGetOutletRpt;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                   
                return responseGetOutletRpt;
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

        public List<MIM.Outlet> GetOutletList(long outletID = 0,long clientID = 0,bool statusOutlet = false,string outletName = "",string clientName = "")
        {
            logger.Info("GetOutletList");

            List<MIM.Outlet> responseGetOutlet = new List<MIM.Outlet>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectOutlet = from outlet in entityConnection.dbMobikonIMSDataContext.GETOUTLET(outletID, outletName, clientID, clientName, statusOutlet)
                                   select outlet;
                                   //select new
                                   //{
                                   //    outlet.ACTIVATED,
                                   //    outlet.CLIENTADDRESS,
                                   //    outlet.CLIENTCITY,                                   
                                   //    outlet.CLIENTCITYID,
                                   //    outlet.CLIENTCOUNTRY,
                                   //    outlet.CLIENTID,
                                   //    outlet.CLIENTNAME,
                                   //    outlet.CREATEDDATE,
                                   //    outlet.OUTLETADDRESS,
                                   //    outlet.OUTLETCITYID,
                                   //    outlet.OUTLETCOUNTRY,
                                   //    outlet.OUTLETCOUNTRYID,
                                   //    outlet.OUTLETID,
                                   //    outlet.OUTLETNAME,
                                   //    outlet.USERID,
                                   //    outlet.USERNAME,                                      
                                   //    outlet.OUTLETCITY
                                   //};
                
                foreach (var response in selectOutlet)
                {
                    responseGetOutlet.Add(new MIM.Outlet
                    {
                        outletName = response.OUTLETNAME,
                        outletID = response.OUTLETID,
                        userName = response.USERNAME,
                        userID = response.USERID,
                        clientName = response.CLIENTNAME,
                        clientID = response.CLIENTID,
                        cityID = response.OUTLETCITYID,
                        cityName = response.OUTLETCITY,
                        countryName = response.OUTLETCOUNTRY,
                        countryID = response.OUTLETCOUNTRYID,
                        address = response.OUTLETADDRESS,
                        activated = response.ACTIVATED,
                        createdDate = response.CREATEDDATE,
                        clientCity = response.CLIENTCITY,
                        clientAddress = response.CLIENTADDRESS,
                        clientCountry = response.CLIENTCOUNTRY
                    });
                } 
                return responseGetOutlet;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                       
                return responseGetOutlet;
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
        public bool CheckOutletName(string outletName)
        {
            logger.Info("CheckOutletName");
            EntityConnection entityConnection = new EntityConnection();
            MIM.Outlet responseGetOutlet = new MIM.Outlet();
            int checkOutlet = 0;
            try
            {
                checkOutlet = (from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs
                                  where outlet.OUTLETNAME == outletName
                                  select outlet).Count();
                
                if (checkOutlet == 0)
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

        public bool EditOutlet(MIM.Outlet requestSetOutlet)
        {
            logger.Info("EditOutlet");
            EntityConnection entityConnection = new EntityConnection();
            UserDAL userDAL = new UserDAL();
            CityDAL cityDAL = new CityDAL();
            ClientDAL clientDAL = new ClientDAL();
       

            try
            {
                var updateOutlet = from outlet in entityConnection.dbMobikonIMSDataContext.OUTLETs
                                   where outlet.OUTLETID == requestSetOutlet.outletID
                                   select outlet;

                foreach (OUTLET outlet in updateOutlet)
                {
                    outlet.OUTLETNAME = requestSetOutlet.outletName;
                    outlet.ADDRESS = requestSetOutlet.address;
                    outlet.CREATEDDATE = DateTime.Now;
                    outlet.CITYID = cityDAL.FindCityID(requestSetOutlet.cityName);
                    outlet.USERID = userDAL.FindUserID(requestSetOutlet.userName);
                    outlet.CLIENTID = clientDAL.FindClientID(requestSetOutlet.clientName);
                    outlet.ACTIVATED = requestSetOutlet.activated;

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
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                    entityConnection.dbMobikonIMSDataContext.Dispose();
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

        public bool InsertOutlet(MIM.Outlet requestSetOutlet)
        {
            logger.Info("InsertOutlet");
           
            EntityConnection entityConnection = new EntityConnection();
            OUTLET outlet = new OUTLET();
            UserDAL userDAL = new UserDAL();
            CityDAL cityDAL = new CityDAL();
            ClientDAL clientDAL = new ClientDAL();
          
            try
            {
                outlet.OUTLETNAME = requestSetOutlet.outletName;
                outlet.ADDRESS = requestSetOutlet.address;
                outlet.CREATEDDATE = DateTime.Now;
                outlet.CITYID = cityDAL.FindCityID(requestSetOutlet.cityName);
                outlet.USERID = userDAL.FindUserID(requestSetOutlet.userName);
                outlet.CLIENTID = clientDAL.FindClientID(requestSetOutlet.clientName);
                outlet.ACTIVATED = requestSetOutlet.activated;

                // Add the new object to the clients collection.
                entityConnection.dbMobikonIMSDataContext.OUTLETs.InsertOnSubmit(outlet);
                // Submit the change to the database.            
                entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();

                return true;
            }
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    //Console.WriteLine(ex.Message);
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
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
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
    }
}
