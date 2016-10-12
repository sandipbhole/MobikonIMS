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
    public class ClientDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ClientDAL));

        public ClientDAL()
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

        internal long FindClientID(string clientName)
        {
            EntityConnection entityConnection = new EntityConnection();
            long clientID = 0;

            try
            {
                var selectClient = from client in entityConnection.dbMobikonIMSDataContext.CLIENTs
                                   where client.CLIENTNAME == clientName
                                   select client;

                foreach (var response in selectClient)
                {
                    clientID = response.CLIENTID;
                }
                return clientID;
            }
            catch (Exception ex)
            {                
                logger.Error(ex.Message);
                return clientID;
            }
            finally
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                     entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
                }
            }
        }
        
        public bool CheckClientName(string clientName)
        {
            logger.Info("CheckClientName");
            EntityConnection entityConnection = new EntityConnection();
            MIM.Client responseGetClient = new MIM.Client();
            int  checkClient = 0;

            try
            {
                checkClient = (from client in entityConnection.dbMobikonIMSDataContext.CLIENTs
                                  where client.CLIENTNAME == clientName
                                  select client).Count();

                //foreach (var response in checkClient)
                //{
                //    responseGetClient.clientName = response.CLIENTNAME;
                //}

                //if (!string.IsNullOrEmpty(responseGetClient.clientName))
                if (checkClient == 0)
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

        public bool InsertClient(MIM.Client requestSetClient)
        {
            logger.Info("InsertClient");

            UserDAL userDAL = new UserDAL();
            CityDAL cityDAL = new CityDAL();
            
            EntityConnection entityConnection = new EntityConnection();
            CLIENT client = new CLIENT();
         
            try
            {
                //client.clientID = requestSetClient.clientID;
                client.CLIENTNAME = requestSetClient.clientName;
                client.ADDRESS = requestSetClient.address;
                client.ACTIVATED = requestSetClient.activated;
                client.USERID = userDAL.FindUserID(requestSetClient.userName);
                client.CITYID = cityDAL.FindCityID(requestSetClient.cityName);
                client.CREATEDDATE = DateTime.Now;

                // Add the new object to the clients collection.
                entityConnection.dbMobikonIMSDataContext.CLIENTs.InsertOnSubmit(client);
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

        public bool EditClient(MIM.Client requestSetClient)
        {
            logger.Info("EditClient");
            EntityConnection entityConnection = new EntityConnection();
            CityDAL cityDAL = new CityDAL();
            UserDAL userDAL = new UserDAL();

            OutletDAL outletDAL = new OutletDAL();

            try
            {
                var updateClient = from client in entityConnection.dbMobikonIMSDataContext.CLIENTs
                                   where client.CLIENTID == requestSetClient.clientID
                                   select client;

                foreach (CLIENT client in updateClient)
                {
                    client.CLIENTNAME = requestSetClient.clientName;
                    client.ADDRESS = requestSetClient.address;
                    client.CREATEDDATE = DateTime.Now;
                    client.CITYID = cityDAL.FindCityID(requestSetClient.cityName);
                    client.USERID = userDAL.FindUserID(requestSetClient.userName);
                    client.ACTIVATED = requestSetClient.activated;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }

                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                //Change Outlet Activated status depends on Client Activated status.
                if (requestSetClient.activated == false)
                {
                    return outletDAL.ChangeOutletActivateStatus(requestSetClient.clientID, requestSetClient.activated);
                }
                else
                {
                    return outletDAL.ChangeOutletActivateStatus(requestSetClient.clientID, requestSetClient.activated);
                }
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

        public List<SelectListItem> GetClientName(long clientID)
        {
            logger.Info("GetClientName");

            EntityConnection entityConnection = new EntityConnection();          
            List<SelectListItem> responseGetClient = new List<SelectListItem>();

            try
            {
                var selectClient = from client in entityConnection.dbMobikonIMSDataContext.CLIENTs
                                   where client.CLIENTID == clientID
                                   select client;

                foreach (var response in selectClient)
                {
                    responseGetClient.Add(new SelectListItem
                    {
                        Text = response.CLIENTNAME,
                        Value = response.CLIENTNAME,
                        Selected = true,
                    });
                }
                return responseGetClient;
            }
            catch (Exception ex)
            {                
                logger.Error(ex.Message);
                return responseGetClient;
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


        public List<SelectListItem> GetClientNameList(bool statusClient = false,string pageName = "", System.Nullable<long> selectedClientID = 0, string selectedClientName = "")
        {
            logger.Info("GetClientNameList");
          
            List<SelectListItem> responseGetClient = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectClient = from client in entityConnection.dbMobikonIMSDataContext.GETCLIENT(0, string.Empty, statusClient)
                                   select new
                                   {
                                       client.CLIENTNAME,
                                       client.CLIENTID
                                   };

                foreach (var response in selectClient)
                {
                    if (selectedClientID >= 1 && string.IsNullOrEmpty(selectedClientName))
                    {
                        responseGetClient.Add(new SelectListItem
                        {
                            Text = response.CLIENTNAME,
                            Value = response.CLIENTNAME,
                            Selected = selectedClientID == response.CLIENTID ? true : false,
                        });
                    }
                    else if (selectedClientID <= 0 && !string.IsNullOrEmpty(selectedClientName))
                    {
                        if (selectedClientName != "All")
                        {
                            responseGetClient.Add(new SelectListItem
                            {
                                Text = response.CLIENTNAME,
                                Value = response.CLIENTNAME,
                                Selected = selectedClientName == response.CLIENTNAME ? true : false,
                            });
                        }
                        else
                        {
                            responseGetClient.Add(new SelectListItem
                            {
                                Text = response.CLIENTNAME,
                                Value = response.CLIENTNAME
                            });
                        }
                    }
                    if (selectedClientID >= 1 && !string.IsNullOrEmpty(selectedClientName))
                    {
                        responseGetClient.Add(new SelectListItem
                        {
                            Text = response.CLIENTNAME,
                            Value = response.CLIENTNAME,
                            Selected = selectedClientID == response.CLIENTID ? true : false,
                        });
                    }
                    else
                    {
                        responseGetClient.Add(new SelectListItem
                        {
                            Text = response.CLIENTNAME,
                            Value = response.CLIENTNAME
                        });
                    }
                }

                if (pageName == "Client" || pageName == "Outlet" || pageName == "DeviceHistory")
                {
                    if (string.IsNullOrEmpty(selectedClientName) || selectedClientName == "All")
                    {
                        responseGetClient.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All",
                            Selected = true,
                        });
                    }
                    else
                    {
                        responseGetClient.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All"                           
                        });
                    }
                }

                return responseGetClient;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                              
                return responseGetClient;
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

        public MIM.Client GetClient(long clientID = 0,string clientName = "",bool statusClient = false)
        {
            logger.Info("GetClient");
            MIM.Client responseGetClient = new Message.Client();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectClient = from client in entityConnection.dbMobikonIMSDataContext.GETCLIENT(clientID, clientName, statusClient)
                                   select client;
                                   //select new
                                   //{
                                   //    client.CLIENTNAME,
                                   //    client.CLIENTID,
                                   //    client.ADDRESS,
                                   //    client.CITYID,
                                   //    client.ACTIVATED,
                                   //    client.CITYNAME,
                                   //    client.USERID,
                                   //    client.USERNAME,
                                   //    client.COUNTRYID,
                                   //    client.COUNTRYNAME
                                   //};

                foreach (var response in selectClient)
                {
                    responseGetClient.clientID = response.CLIENTID;
                    responseGetClient.clientName = response.CLIENTNAME;
                    responseGetClient.address = response.ADDRESS;
                    responseGetClient.cityID = response.CITYID;
                    responseGetClient.cityName = response.CITYNAME;
                    responseGetClient.countryID = response.COUNTRYID;
                    responseGetClient.countryName = response.COUNTRYNAME;
                    responseGetClient.activated = response.ACTIVATED;
                    responseGetClient.userID = response.USERID;
                    responseGetClient.userName = response.USERNAME;                  
                }
                return responseGetClient;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State != ConnectionState.Closed)
                {
                   entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
               
                return responseGetClient;
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

        public int GetClientTotalCount()
        {
            logger.Info("GetClientTotalCount");
            EntityConnection entityConnection = new EntityConnection();
            int totalCount = 0;
            try
            {
                totalCount = (from client in entityConnection.dbMobikonIMSDataContext.CLIENTs
                                  select client).Count();
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

        public List<MIM.Client> GetClientList(long clientID = 0,string clientName ="",bool statusClient=false,bool export=false)
        {
            logger.Info("GetClientnamewiseClient");
            List<MIM.Client> responseGetClient = new List<MIM.Client>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectClient = from client in entityConnection.dbMobikonIMSDataContext.GETCLIENT(clientID, clientName, statusClient)
                                   select client;
                                   //select new
                                   //{
                                   //    client.CLIENTNAME,
                                   //    client.CLIENTID,
                                   //    client.ADDRESS,
                                   //    client.CITYID,
                                   //    client.ACTIVATED,
                                   //    client.CITYNAME,
                                   //    client.USERID,
                                   //    client.USERNAME,
                                   //    client.COUNTRYID,
                                   //    client.COUNTRYNAME
                                   //};
                if (export == false)
                {
                    foreach (var response in selectClient)
                    {
                        responseGetClient.Add(new MIM.Client
                        {
                            clientID = response.CLIENTID,
                            clientName = response.CLIENTNAME,
                            address = response.ADDRESS,
                            cityID = response.CITYID,
                            cityName = response.CITYNAME,
                            countryID = response.COUNTRYID,
                            countryName = response.COUNTRYNAME,
                            activated = response.ACTIVATED,
                            userID = response.USERID,
                            userName = response.USERNAME                           
                        });
                    }
                    return responseGetClient;
                }
                else
                {
                    foreach (var response in selectClient)
                    {
                        responseGetClient.Add(new MIM.Client
                        {                           
                            clientName = response.CLIENTNAME,
                            address = response.ADDRESS,                           
                            cityName = response.CITYNAME,                          
                            countryName = response.COUNTRYNAME,
                            activated = response.ACTIVATED,                           
                            userName = response.USERNAME,
                        });
                    }
                }
                return responseGetClient;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                  
                return responseGetClient;
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
