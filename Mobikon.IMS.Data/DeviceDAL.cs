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
using System.Reflection;

using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{
    public class DeviceDAL
    {
        //private EntityConnection entityConnection = new EntityConnection();
        private static readonly ILog logger = LogManager.GetLogger(typeof(DeviceDAL));
        StatusDAL statusDAL = new StatusDAL();
        UserDAL userDAL = new UserDAL();
        OutletDAL outletDAL = new OutletDAL();
        SettingsDAL systemSettingsDAL = new SettingsDAL();

        public DeviceDAL()
        {
            InitializeLog4Net();
        }

        private void InitializeLog4Net()
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.AppendToFile = true;
            appender.Name = "ServiceLogger";

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;

            string path = "C:\\MobikonIMS"; // System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

        public List<MIM.Settings> GetSettings()
        {
            logger.Info("GetSettings");
            EntityConnection entityConnection = new EntityConnection();
            List<MIM.Settings> responseGetSettings = new List<MIM.Settings>();

            try
            {
                var getSettings = from settings in entityConnection.dbMobikonIMSDataContext.SETTINGs
                                  select settings;

                foreach (var response in getSettings)
                {
                    responseGetSettings.Add(new MIM.Settings
                    {
                        settingsValue = response.SETTINGSVALUE,
                        settingsName = response.SETTINGSNAME
                    });
                }
                return responseGetSettings;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return responseGetSettings;
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

        public string GetDCFileFolderPath()
        {
            logger.Info("GetDCFileFolderPath");
            string settingsValue = string.Empty;
            EntityConnection entityConnection = new EntityConnection();
            MIM.Settings responseGetSettings = new MIM.Settings();

            try
            {
                var getSettings = from settings in entityConnection.dbMobikonIMSDataContext.SETTINGs
                                  where settings.SETTINGSNAME == "DC File Folder Path"
                                  select settings;

                foreach (var response in getSettings)
                {
                    settingsValue = response.SETTINGSVALUE;
                }
                return settingsValue;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return settingsValue;
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

        public bool CheckDeviceExist(string productSerial)
        {
            logger.Info("CheckDeviceExist");
            EntityConnection entityConnection = new EntityConnection();
            MIM.Device responseGetDevice = new MIM.Device();

            try
            {
                var checkDevice = from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                  where device.PRODUCTSERIAL.Trim() == productSerial.Trim()
                                  select device;

                foreach (var response in checkDevice)
                {
                    responseGetDevice.deviceDetails = response.DEVICEDETAILS;
                }

                if (!string.IsNullOrEmpty(responseGetDevice.deviceDetails))
                    return true;

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

        public int GetDeviceTotalCount()
        {
            logger.Info("GetDeviceTotalCount");
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                int totalCount = (from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                  select device).Count();
                return totalCount;
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

        public MIM.Device GetDevice(long deviceID = 0, string productSerial = "", string devicedetails = "", string statusDevice = "")
        {
            logger.Info("GetDevice");

            MIM.Device responseGetDevice = new MIM.Device();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDevice = from device in entityConnection.dbMobikonIMSDataContext.GETDEVICE(deviceID, productSerial, devicedetails, statusDevice)
                                   select device;

                foreach (var response in selectDevice)
                {
                    responseGetDevice.companyOwner = response.COMPANYOWNER;
                    responseGetDevice.deviceDetails = response.DEVICEDETAILS;
                    responseGetDevice.note = response.NOTE;
                    responseGetDevice.productSerial = response.PRODUCTSERIAL;
                    responseGetDevice.status = response.STATUSNAME;
                    responseGetDevice.statusID = response.STATUSID;
                    responseGetDevice.userID = response.USERID;
                    responseGetDevice.userName = response.USERNAME;
                    responseGetDevice.deviceID = response.DEVICEID;
                    responseGetDevice.createdDate = response.CREATEDDATE;
                    responseGetDevice.blockedDate = response.BLOCKEDDATE;
                    responseGetDevice.deviceTag = response.DEVICETAG;
                    responseGetDevice.deviceType = response.DEVICETYPE;
                    responseGetDevice.serialNo = response.SERIALNO;
                    responseGetDevice.currentStatus = response.CURRENTSTATUS;

                }

                return responseGetDevice;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return responseGetDevice;

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

        public void RemoveBlockStatus()
        {
            logger.Info("RemoveBlockStatus");

            EntityConnection entityConnection = new EntityConnection();
            int result = 0;
            try
            {
                var removeBlockStatus = from device in entityConnection.dbMobikonIMSDataContext.REMOVEBLOCKSTATUS()
                                        select new
                                        {
                                            device.RESULT
                                        };
                foreach (var response in removeBlockStatus)
                {
                    result = response.RESULT;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                }
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

        public List<MIM.Device> GetDeviceList(long deviceID = 0, string productSerial = "", string deviceDetails = "", string statusDevice = "", bool showBlockedDevice = false)
        {
            logger.Info("GetDeviceList");

            List<MIM.Device> responseGetDevice = new List<MIM.Device>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDevice = from device in entityConnection.dbMobikonIMSDataContext.GETDEVICE(deviceID, productSerial, deviceDetails, statusDevice)

                                   select new
                                   {
                                       device.PRODUCTSERIAL,
                                       device.DEVICEDETAILS,
                                       device.COMPANYOWNER,
                                       device.STATUSID,
                                       device.STATUSNAME,
                                       device.USERID,
                                       device.USERNAME,
                                       device.NOTE,
                                       device.DEVICEID,
                                       device.CREATEDDATE,
                                       device.BLOCKEDDATE,
                                       device.DEVICETAG,
                                       device.DEVICETYPE,
                                       device.SERIALNO,
                                       device.CURRENTSTATUS
                                   };

                if (showBlockedDevice == false)
                {
                    foreach (var response in selectDevice.Where(device => device.STATUSNAME != "Blocked"))
                    {
                        responseGetDevice.Add(new MIM.Device
                        {
                            companyOwner = response.COMPANYOWNER,
                            deviceDetails = response.DEVICEDETAILS,
                            note = response.NOTE,
                            productSerial = response.PRODUCTSERIAL,
                            status = response.STATUSNAME,
                            statusID = response.STATUSID,
                            userID = response.USERID,
                            userName = response.USERNAME,
                            deviceID = response.DEVICEID,
                            createdDate = response.CREATEDDATE,
                            blockedDate = response.BLOCKEDDATE,
                            deviceTag = response.DEVICETAG,
                            deviceType = response.DEVICETYPE,
                            currentStatus = response.CURRENTSTATUS,
                            serialNo = response.SERIALNO
                        });
                    }

                    return responseGetDevice;
                }
                else
                {
                    foreach (var response in selectDevice)
                    {
                        responseGetDevice.Add(new MIM.Device
                        {
                            companyOwner = response.COMPANYOWNER,
                            deviceDetails = response.DEVICEDETAILS,
                            note = response.NOTE,
                            productSerial = response.PRODUCTSERIAL,
                            status = response.STATUSNAME,
                            statusID = response.STATUSID,
                            userID = response.USERID,
                            userName = response.USERNAME,
                            deviceID = response.DEVICEID,
                            createdDate = response.CREATEDDATE,
                            blockedDate = response.BLOCKEDDATE,
                            deviceTag = response.DEVICETAG,
                            deviceType = response.DEVICETYPE,
                            currentStatus = response.CURRENTSTATUS,
                            serialNo = response.SERIALNO
                        });
                    }
                }
                return responseGetDevice;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return responseGetDevice;
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

        public List<MIM.DeviceRpt> GetDeviceListRpt(long deviceID = 0, string productSerial = "", string deviceDetails = "", string statusDevice = "", bool showBlockedDevice = false)
        {
            logger.Info("GetDeviceListRpt");

            List<MIM.DeviceRpt> responseGetDeviceRpt = new List<MIM.DeviceRpt>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDevice = from device in entityConnection.dbMobikonIMSDataContext.GETDEVICE(deviceID, productSerial, deviceDetails, statusDevice)

                                   select new
                                   {
                                       device.PRODUCTSERIAL,
                                       device.DEVICEDETAILS,
                                       device.COMPANYOWNER,
                                       device.STATUSID,
                                       device.STATUSNAME,
                                       device.USERID,
                                       device.USERNAME,
                                       device.NOTE,
                                       device.DEVICEID,
                                       device.CREATEDDATE,
                                       device.BLOCKEDDATE,
                                       device.DEVICETAG,
                                       device.DEVICETYPE
                                   };

                if (showBlockedDevice == false)
                {
                    foreach (var response in selectDevice.Where(device => device.STATUSNAME != "Blocked"))
                    {
                        responseGetDeviceRpt.Add(new MIM.DeviceRpt
                        {
                            CompanyOwned = response.COMPANYOWNER,
                            DeviceDetails = response.DEVICEDETAILS,
                            Note = response.NOTE,
                            DeviceSerial = response.PRODUCTSERIAL,
                            Status = response.STATUSNAME,
                            UserName = response.USERNAME,
                            DeviceTag = response.DEVICETAG,
                            DeviceType = response.DEVICETYPE,
                            BlockedDate = String.Format("{0:dd/MM/yyyy}", response.BLOCKEDDATE)
                        });
                    }

                    return responseGetDeviceRpt;
                }
                else
                {
                    foreach (var response in selectDevice)
                    {
                        responseGetDeviceRpt.Add(new MIM.DeviceRpt
                        {
                            CompanyOwned = response.COMPANYOWNER,
                            DeviceDetails = response.DEVICEDETAILS,
                            Note = response.NOTE,
                            DeviceSerial = response.PRODUCTSERIAL,
                            Status = response.STATUSNAME,
                            UserName = response.USERNAME,
                            DeviceTag = response.DEVICETAG,
                            DeviceType = response.DEVICETYPE,
                            BlockedDate = String.Format("{0:dd/MM/yyyy}", response.BLOCKEDDATE)
                        });
                    }
                }

                return responseGetDeviceRpt;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceRpt;
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

        public List<SelectListItem> GetCompanyOwnerList(string selectedCompanyOwner = "")
        {
            logger.Info("GetCompanyOwnerList");
            List<SelectListItem> items = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(selectedCompanyOwner))
            {
                items.Add(new SelectListItem { Text = "Yes", Value = "Yes", Selected = selectedCompanyOwner == "Yes" ? true : false });
                items.Add(new SelectListItem { Text = "No", Value = "No", Selected = selectedCompanyOwner == "No" ? true : false });
                items.Add(new SelectListItem { Text = "Pending", Value = "Pending", Selected = selectedCompanyOwner == "Pending" ? true : false });
            }
            else
            {
                items.Add(new SelectListItem { Text = "Yes", Value = "Yes", Selected = true });
                items.Add(new SelectListItem { Text = "No", Value = "No" });
                items.Add(new SelectListItem { Text = "Pending", Value = "Pending" });
            }

            return items;
        }

        public List<SelectListItem> GetDeviceTypeList(string pageName = "", string selectedDeviceType = "")
        {
            logger.Info("GetDeviceTypeList");

            List<SelectListItem> deviceTypeList = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDeviceType = from deviceType in entityConnection.dbMobikonIMSDataContext.DEVICETYPEs
                                       orderby deviceType.DEVICETYPE1
                                       select new
                                       {
                                           deviceType.SERIALNO,
                                           deviceType.DEVICETYPE1
                                       };
                foreach (var response in selectDeviceType)
                {
                    deviceTypeList.Add(new SelectListItem
                    {
                        Text = response.DEVICETYPE1,
                        Value = response.DEVICETYPE1,
                        Selected = response.DEVICETYPE1 == selectedDeviceType ? true : false
                    });
                }

                if (pageName == "DashBoard" || pageName == "Device")
                {
                    if (string.IsNullOrEmpty(selectedDeviceType))
                    {
                        deviceTypeList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All",
                            Selected = true
                        });
                    }
                }

                return deviceTypeList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return deviceTypeList;
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


        public List<SelectListItem> GetProductSerialList(long selectedDeviceID = 0, string selectedProductSerial = "", string deviceDetails = "", string statusDevice = "", string pageName = "", bool showBlockedDevice = false)
        {
            logger.Info("GetProductSerialList");

            List<SelectListItem> productSerialList = new List<SelectListItem>();
            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDevice = from device in entityConnection.dbMobikonIMSDataContext.GETDEVICE(0, string.Empty, deviceDetails, statusDevice)
                                   select new
                                   {
                                       device.PRODUCTSERIAL,
                                       device.DEVICEDETAILS,
                                       device.COMPANYOWNER,
                                       device.STATUSID,
                                       device.STATUSNAME,
                                       device.USERID,
                                       device.NOTE,
                                       device.DEVICEID,
                                       device.CREATEDDATE,
                                       device.BLOCKEDDATE,
                                       device.DEVICETAG,
                                       device.DEVICETYPE
                                   };
                if (showBlockedDevice == false)
                {
                    if (selectedDeviceID >= 1 && string.IsNullOrEmpty(selectedProductSerial))
                    {
                        foreach (var response in selectDevice.Where(device => device.STATUSNAME != "Blocked"))
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL,
                                Selected = response.DEVICEID == selectedDeviceID ? true : false
                            });
                        }
                    }
                    else if (selectedDeviceID <= 0 && !string.IsNullOrEmpty(selectedProductSerial))
                    {
                        foreach (var response in selectDevice.Where(device => device.STATUSNAME != "Blocked"))
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL,
                                Selected = response.PRODUCTSERIAL == selectedProductSerial ? true : false
                            });
                        }
                    }
                    else if (selectedDeviceID >= 1 && !string.IsNullOrEmpty(selectedProductSerial))
                    {
                        foreach (var response in selectDevice.Where(device => device.STATUSNAME != "Blocked"))
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL,
                                Selected = response.DEVICEID == selectedDeviceID ? true : false
                            });
                        }
                    }
                    else
                    {
                        foreach (var response in selectDevice.Where(device => device.STATUSNAME != "Blocked"))
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL
                            });
                        }
                    }
                }
                if (showBlockedDevice == true)
                {
                    if (selectedDeviceID >= 1 && string.IsNullOrEmpty(selectedProductSerial))
                    {
                        foreach (var response in selectDevice)
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL,
                                Selected = response.DEVICEID == selectedDeviceID ? true : false
                            });
                        }
                    }
                    else if (selectedDeviceID <= 0 && !string.IsNullOrEmpty(selectedProductSerial))
                    {
                        if (selectedProductSerial != "All")
                        {
                            foreach (var response in selectDevice)
                            {
                                productSerialList.Add(new SelectListItem
                                {
                                    Text = response.PRODUCTSERIAL,
                                    Value = response.PRODUCTSERIAL,
                                    Selected = response.PRODUCTSERIAL == selectedProductSerial ? true : false
                                });
                            }
                        }
                        else
                        {
                            foreach (var response in selectDevice)
                            {
                                productSerialList.Add(new SelectListItem
                                {
                                    Text = response.PRODUCTSERIAL,
                                    Value = response.PRODUCTSERIAL
                                });
                            }
                        }
                    }
                    else if (selectedDeviceID >= 1 && !string.IsNullOrEmpty(selectedProductSerial))
                    {
                        foreach (var response in selectDevice)
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL,
                                Selected = response.DEVICEID == selectedDeviceID ? true : false
                            });
                        }
                    }
                    else
                    {
                        foreach (var response in selectDevice)
                        {
                            productSerialList.Add(new SelectListItem
                            {
                                Text = response.PRODUCTSERIAL,
                                Value = response.PRODUCTSERIAL
                            });
                        }
                    }
                }

                if (pageName == "Device" || pageName == "DeviceHistory")
                {
                    if (string.IsNullOrEmpty(selectedProductSerial) || selectedProductSerial == "All")
                    {
                        productSerialList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All",
                            Selected = true,
                        });
                    }
                    else
                    {
                        productSerialList.Add(new SelectListItem
                        {
                            Text = "All",
                            Value = "All"
                        });
                    }
                }
                return productSerialList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return productSerialList;
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

        internal long FindDeviceID(string productSerial)
        {
            EntityConnection entityConnection = new EntityConnection();
            long deviceID = 0;

            try
            {
                var selectDevice = from user in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                   where user.PRODUCTSERIAL == productSerial
                                   select user;

                foreach (var response in selectDevice)
                {
                    deviceID = response.DEVICEID;
                }
                return deviceID;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return deviceID;
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

        internal string FindDeviceDetails(string productSerial)
        {
            EntityConnection entityConnection = new EntityConnection();
            string deviceDetails = string.Empty;

            try
            {
                var selectDevice = from user in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                   where user.PRODUCTSERIAL.Trim() == productSerial.Trim()
                                   select user;

                foreach (var response in selectDevice)
                {
                    deviceDetails = response.DEVICEDETAILS;
                }
                return deviceDetails;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return deviceDetails;
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

        private void ChangeCurrentStatus(MIM.DeviceTransaction requestSetDeviceTransaction)
        {
            logger.Info("ChangeCurrentStatus");

            EntityConnection entityConnection = new EntityConnection();

            try
            {

                var updateDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.DEVICETRANSACTIONs
                                              where deviceTransaction.DEVICEID == requestSetDeviceTransaction.deviceID
                                              select deviceTransaction;

                foreach (DEVICETRANSACTION deviceTransaction in updateDeviceTransaction)
                {
                    deviceTransaction.USERID = userDAL.FindUserID(requestSetDeviceTransaction.userName);
                    deviceTransaction.CURRENTSTATUS = false;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();

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

        public bool InsertDeviceTransaction(MIM.DeviceTransaction requestSetDeviceTransaction, string taskName = "")
        {
            logger.Info("InsertDeviceTransaction");

            EntityConnection entityConnection = new EntityConnection();
            DEVICETRANSACTION deviceTransaction = new DEVICETRANSACTION();
            MIM.Device requestSetDevice = new MIM.Device();
            List<MIM.DeviceTransaction> requestDeviceTransaction = new List<MIM.DeviceTransaction>();
            bool result = false;
            string deviceDetails = string.Empty;

            try
            {
                if (taskName != "NewDevice")
                    ChangeCurrentStatus(requestSetDeviceTransaction);

                deviceTransaction.DEVICEID = requestSetDeviceTransaction.deviceID;
                deviceTransaction.DAMAGEDOLDDEVICE = requestSetDeviceTransaction.damagedOldDevice;

                if (requestSetDeviceTransaction.userID <= 0)
                    deviceTransaction.USERID = userDAL.FindUserID(requestSetDeviceTransaction.userName);
                else
                    deviceTransaction.USERID = requestSetDeviceTransaction.userID;

                deviceTransaction.DC = requestSetDeviceTransaction.dc;
                deviceTransaction.DCDATE = requestSetDeviceTransaction.dcDate;
                deviceTransaction.DELIVERYDATE = requestSetDeviceTransaction.deliveryDate;
                deviceTransaction.DEVICEID = requestSetDeviceTransaction.deviceID;
                deviceTransaction.HIC = requestSetDeviceTransaction.hic;
                deviceTransaction.HICDATE = requestSetDeviceTransaction.hicDate;
                deviceTransaction.INSURANCECLAIM = requestSetDeviceTransaction.insuranceClaim;
                deviceTransaction.INSURED = requestSetDeviceTransaction.insured;

                if (requestSetDeviceTransaction.outletID <= 0)
                    deviceTransaction.OUTLETID = outletDAL.FindOutletID(requestSetDeviceTransaction.outletName);
                else
                    deviceTransaction.OUTLETID = requestSetDeviceTransaction.outletID;

                deviceTransaction.RDC = requestSetDeviceTransaction.rdc;
                deviceTransaction.RDCDATE = requestSetDeviceTransaction.rdcDate;
                deviceTransaction.REMARKS = requestSetDeviceTransaction.remarks;
                deviceTransaction.TRANSFEROWNERSHIPDATE = requestSetDeviceTransaction.transferOwnershipDate;
                deviceTransaction.CREATEDDATE = DateTime.Now;

                if (deviceTransaction.STATUSID <= 0)
                    deviceTransaction.STATUSID = statusDAL.FindStatusID(requestSetDeviceTransaction.status);
                else
                    deviceTransaction.STATUSID = requestSetDeviceTransaction.statusID;

                deviceTransaction.DCFILE = requestSetDeviceTransaction.dcFile;
                deviceTransaction.CURRENTSTATUS = true;

                // Add the new object to the devices collection.
                entityConnection.dbMobikonIMSDataContext.DEVICETRANSACTIONs.InsertOnSubmit(deviceTransaction);
                // Submit the change to the database.            
                entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                result = true;

                if (result == true)
                {
                    //Editing device status when create new device inventory.
                    if (taskName != "NewDevice")
                    {
                        if (string.IsNullOrEmpty(requestSetDeviceTransaction.deviceDetails))
                            deviceDetails = FindDeviceDetails(requestSetDeviceTransaction.productSerial);
                        else
                            deviceDetails = requestSetDeviceTransaction.deviceDetails;

                        requestSetDevice.status = requestSetDeviceTransaction.status;
                        requestSetDevice.statusID = statusDAL.FindStatusID(requestSetDeviceTransaction.status);
                        requestSetDevice.deviceDetails = deviceDetails;
                        requestSetDevice.deviceID = requestSetDeviceTransaction.deviceID;
                        requestSetDevice.productSerial = requestSetDeviceTransaction.productSerial;
                        requestSetDevice.userID = userDAL.FindUserID(requestSetDeviceTransaction.userName);
                        requestSetDevice.createdDate = DateTime.Now;

                        result = EditDeviceStatus(requestSetDevice);
                    }

                    //if (requestSetDeviceTransaction.status == "Blocked" || requestSetDeviceTransaction.status == "In Stock" || requestSetDeviceTransaction.status == "Deployed-Active")
                    //{
                    //    //Converting requestSetDeviceTransaction object requestDeviceTransaction List Object for SendEmail
                    //    requestDeviceTransaction.Add(new MIM.DeviceTransaction
                    //    {
                    //        status = requestSetDeviceTransaction.status,
                    //        statusID = statusDAL.FindStatusID(requestSetDeviceTransaction.status),
                    //        deviceDetails = deviceDetails,
                    //        deviceID = requestSetDeviceTransaction.deviceID,
                    //        productSerial = requestSetDeviceTransaction.productSerial,
                    //        userID = userDAL.FindUserID(requestSetDeviceTransaction.userName),
                    //        userName = requestSetDeviceTransaction.userName,
                    //        deviceType = requestSetDeviceTransaction.deviceType,
                    //        createdDate = requestSetDeviceTransaction.createdDate
                    //    });

                    //    SendEmail(requestDeviceTransaction);
                    //}
                    return result;
                }                           
                else
                {
                    return result;
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
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return result;
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

        private bool EditDeviceStatus(MIM.Device requestSetDevice)
        {
            logger.Info("EditDeviceStatus");

            EntityConnection entityConnection = new EntityConnection();

            try
            {
                if (requestSetDevice.status == "Blocked")
                {

                    var updateDeviceStatus = from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                             where device.DEVICEID == requestSetDevice.deviceID
                                             select device;
                    foreach (DEVICE device in updateDeviceStatus)
                    {
                        device.STATUSID = requestSetDevice.statusID;
                        device.USERID = requestSetDevice.userID;
                        device.BLOCKEDDATE = DateTime.Now;
                        device.CREATEDDATE = requestSetDevice.createdDate;

                        entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                    }
                    entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                }
                else
                {
                    Nullable<DateTime> blockedDate = null;

                    var updateDeviceStatus = from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                             where device.DEVICEID == requestSetDevice.deviceID
                                             select device;
                    foreach (DEVICE device in updateDeviceStatus)
                    {
                        device.STATUSID = requestSetDevice.statusID;
                        device.USERID = requestSetDevice.userID;
                        device.CREATEDDATE = requestSetDevice.createdDate;
                        device.BLOCKEDDATE = blockedDate;

                        entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                    }
                    entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                }

                return true;
            }
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                //Console.WriteLine(ex.Message);
                foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                {
                    // All database values overwrite current values.
                    occ.Resolve(RefreshMode.OverwriteCurrentValues);
                }
                logger.Error(ex.Message);
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

        public int BlockedDevicesCount(string userName)
        {

            logger.Info("BlockedDevicesCount");
            EntityConnection entityConnection = new EntityConnection();
            int count = 0;

            try
            {
                count = (from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                         join status in entityConnection.dbMobikonIMSDataContext.STATUS on device.STATUSID equals status.STATUSID
                         join user in entityConnection.dbMobikonIMSDataContext.USERs on device.USERID equals user.USERID
                         where status.STATUSNAME == "Blocked" && user.USERNAME == userName
                         select new
                         {
                             device.DEVICEID,
                             device.PRODUCTSERIAL,
                             device.DEVICEDETAILS,
                             device.STATUSID,
                             status.STATUSNAME,
                             user.USERNAME
                         }).Count();

                return count;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return count;
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

        public string CheckBlockDeviceByUser(long deviceID = 0,string productSerial = "", string userName = "")
        {
            logger.Info("EditDeviceTransaction");

            EntityConnection entityConnection = new EntityConnection();
            MIM.Device requestSetDevice = new MIM.Device();
            string blockedDeviceUserName = string.Empty;

            try
            {
                var checkBlockedDevice = from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                         join status in entityConnection.dbMobikonIMSDataContext.STATUS on device.STATUSID equals status.STATUSID
                                         join user in entityConnection.dbMobikonIMSDataContext.USERs on device.USERID equals user.USERID
                                         where status.STATUSNAME == "Blocked" && user.USERNAME == userName && device.DEVICEID == deviceID && device.PRODUCTSERIAL == productSerial
                                         select new
                                         {
                                             device.DEVICEID,
                                             device.PRODUCTSERIAL,
                                             device.DEVICEDETAILS,
                                             device.STATUSID,
                                             status.STATUSNAME,
                                             user.USERNAME
                                         };

               foreach (var response in checkBlockedDevice)
                {
                    blockedDeviceUserName = response.USERNAME;
                }
                return blockedDeviceUserName;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return blockedDeviceUserName;
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

        public bool EditDeviceTransaction(MIM.DeviceTransaction requestSetDeviceTransaction)
        {
            logger.Info("EditDeviceTransaction");

            EntityConnection entityConnection = new EntityConnection();
            MIM.Device requestSetDevice = new MIM.Device();
            List<MIM.DeviceTransaction> requestDeviceTransaction = new List<MIM.DeviceTransaction>();
            bool result = false;
            try
            {
                var updateDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.DEVICETRANSACTIONs
                                              where deviceTransaction.SERIALNO == requestSetDeviceTransaction.serialNo
                                              select deviceTransaction;

                foreach (DEVICETRANSACTION deviceTransaction in updateDeviceTransaction)
                {
                    deviceTransaction.DEVICEID = requestSetDeviceTransaction.deviceID;
                    deviceTransaction.DAMAGEDOLDDEVICE = requestSetDeviceTransaction.damagedOldDevice;

                    deviceTransaction.USERID = userDAL.FindUserID(requestSetDeviceTransaction.userName);
                    deviceTransaction.DC = requestSetDeviceTransaction.dc;
                    deviceTransaction.DCDATE = requestSetDeviceTransaction.dcDate;
                    deviceTransaction.DELIVERYDATE = requestSetDeviceTransaction.deliveryDate;
                    deviceTransaction.DEVICEID = requestSetDeviceTransaction.deviceID;
                    deviceTransaction.HIC = requestSetDeviceTransaction.hic;
                    deviceTransaction.HICDATE = requestSetDeviceTransaction.hicDate;
                    deviceTransaction.INSURANCECLAIM = requestSetDeviceTransaction.insuranceClaim;
                    deviceTransaction.INSURED = requestSetDeviceTransaction.insured;
                    deviceTransaction.OUTLETID = outletDAL.FindOutletID(requestSetDeviceTransaction.outletName);
                    deviceTransaction.RDC = requestSetDeviceTransaction.rdc;
                    deviceTransaction.RDCDATE = requestSetDeviceTransaction.rdcDate;
                    deviceTransaction.REMARKS = requestSetDeviceTransaction.remarks;
                    deviceTransaction.TRANSFEROWNERSHIPDATE = requestSetDeviceTransaction.transferOwnershipDate;
                    deviceTransaction.CREATEDDATE = requestSetDeviceTransaction.createdDate;
                    deviceTransaction.STATUSID = statusDAL.FindStatusID(requestSetDeviceTransaction.status);
                    deviceTransaction.DCFILE = requestSetDeviceTransaction.dcFile;
                    deviceTransaction.CURRENTSTATUS = true;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                result = true;

                if (result == true)
                {
                    result = false;
                    //Editing device status
                    string deviceDetails = FindDeviceDetails(requestSetDeviceTransaction.productSerial);
                    requestSetDevice.status = requestSetDeviceTransaction.status;
                    requestSetDevice.statusID = statusDAL.FindStatusID(requestSetDeviceTransaction.status);
                    requestSetDevice.deviceDetails = deviceDetails;
                    requestSetDevice.deviceID = requestSetDeviceTransaction.deviceID;
                    requestSetDevice.productSerial = requestSetDeviceTransaction.productSerial;
                    requestSetDevice.userID = userDAL.FindUserID(requestSetDeviceTransaction.userName);
                    requestSetDevice.createdDate = requestSetDeviceTransaction.createdDate;

                    result = EditDeviceStatus(requestSetDevice);
                    if (result == true )
                    {
                        if (requestSetDeviceTransaction.status == "Blocked" || requestSetDeviceTransaction.status == "In Stock")
                        {
                            //Converting requestSetDevice object to requestSetDeviceTransaction List Object
                            requestDeviceTransaction.Add(new MIM.DeviceTransaction
                            {
                                status = requestSetDeviceTransaction.status,
                                statusID = statusDAL.FindStatusID(requestSetDeviceTransaction.status),
                                deviceDetails = deviceDetails,
                                deviceID = requestSetDeviceTransaction.deviceID,
                                productSerial = requestSetDeviceTransaction.productSerial,
                                userID = userDAL.FindUserID(requestSetDeviceTransaction.userName),
                                userName = requestSetDeviceTransaction.userName,
                                deviceType = requestSetDeviceTransaction.deviceType,
                                createdDate = requestSetDeviceTransaction.createdDate
                            });

                            SendEmail(requestDeviceTransaction);
                        }
                    }
                }
                return result;
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
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                return result;
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

        public bool InsertDevice(MIM.Device requestSetDevice)
        {
            logger.Info("InsertDevice");
            EntityConnection entityConnection = new EntityConnection();
            DEVICE device = new DEVICE();
            MIM.DeviceTransaction requestSetDeviceTransaction = new MIM.DeviceTransaction();

            try
            {
                //if (requestSetDevice.status == "Blocked")
                //    requestSetDevice.blockedDate = DateTime.Now;

                //device.deviceID = requestSetDevice.deviceID;
                device.PRODUCTSERIAL = requestSetDevice.productSerial;
                device.DEVICEDETAILS = requestSetDevice.deviceDetails;
                device.NOTE = requestSetDevice.note;
                device.USERID = userDAL.FindUserID(requestSetDevice.userName);
                device.STATUSID = statusDAL.FindStatusID(requestSetDevice.status);
                device.COMPANYOWNER = requestSetDevice.companyOwner;
                device.CREATEDDATE = DateTime.Now;
                //device.BLOCKEDDATE = requestSetDevice.blockedDate;
                device.DEVICETAG = requestSetDevice.deviceTag;
                device.DEVICETYPE = requestSetDevice.deviceType;

                // Add the new object to the devices collection.
                entityConnection.dbMobikonIMSDataContext.DEVICEs.InsertOnSubmit(device);
                // Submit the change to the database.            
                entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();


                //Inserting new device into DeviceTransaction table
                requestSetDeviceTransaction.status = requestSetDevice.status;
                requestSetDeviceTransaction.deviceDetails = requestSetDevice.deviceDetails;
                requestSetDeviceTransaction.statusID = statusDAL.FindStatusID(requestSetDevice.status);
                requestSetDeviceTransaction.productSerial = requestSetDevice.productSerial;
                requestSetDeviceTransaction.deviceID = FindDeviceID(requestSetDevice.productSerial);
                requestSetDeviceTransaction.userID = userDAL.FindUserID(requestSetDevice.userName);
                requestSetDeviceTransaction.userName = requestSetDevice.userName;
                requestSetDeviceTransaction.outletID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["OutletID"]);             
                requestSetDeviceTransaction.createdDate = DateTime.Now;
                requestSetDeviceTransaction.insured = true;
                requestSetDeviceTransaction.currentStatus = true;

                if (true == InsertDeviceTransaction(requestSetDeviceTransaction, "NewDevice"))
                    return true;
                else
                    return false;
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

        private long FindDeviceOutlet(long deviceID)
        {
            logger.Info("FindDeviceOutlet");

            EntityConnection entityConnection = new EntityConnection();
            MIM.DeviceTransaction requestSetDeviceTransaction = new MIM.DeviceTransaction();
            string outletID = string.Empty;

            try
            {
                var deviceTransactionList = (from deviceTransaction in entityConnection.dbMobikonIMSDataContext.DEVICETRANSACTIONs
                                             where deviceTransaction.DEVICEID == deviceID
                                             orderby deviceTransaction.DEVICEID descending
                                             select deviceTransaction).Take(1);

                foreach (var response in deviceTransactionList)
                {
                    outletID = Convert.ToString(response.OUTLETID);
                }

                return Convert.ToInt64(outletID);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return 0;
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

        public bool DeleteDevice(long deviceID, string productSerial)
        {
            logger.Info("DeleteDevice");

            EntityConnection entityConnection = new EntityConnection();
            MIM.DeviceTransaction requestSetDeviceTransaction = new MIM.DeviceTransaction();
            //int result = 0;

            try
            {
                //var deleteDevice = from device in entityConnection.dbMobikonIMSDataContext.DELETEDEVICE(deviceID, productSerial)
                //            select new
                //            {
                //                device.RESULT
                //            };
                //            foreach (var response in deleteDevice)
                //            {
                //                result = response.RESULT;
                //            }
                //if (result == 1)
                //    return true;
                //else
                //    return false;

                var deleteDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.DEVICETRANSACTIONs
                                   where deviceTransaction.DEVICEID == deviceID
                                   select deviceTransaction;

                 entityConnection.dbMobikonIMSDataContext.DEVICETRANSACTIONs.DeleteAllOnSubmit(deleteDeviceTransaction);

                var deleteDevice = from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                   where device.DEVICEID == deviceID && device.PRODUCTSERIAL == productSerial 
                                   select device;

                entityConnection.dbMobikonIMSDataContext.DEVICEs.DeleteAllOnSubmit(deleteDevice);

                entityConnection.dbMobikonIMSDataContext.SubmitChanges();
                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();

                return true;
            }
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    //Console.WriteLine(ex.Message);
                    foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                    {
                        // All database values overwrite current values.
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                logger.Error(ex.Message);
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

        public bool EditDevice(MIM.Device requestSetDevice, string role)
        {
            logger.Info("EditDevice");

            EntityConnection entityConnection = new EntityConnection();
            MIM.DeviceTransaction requestSetDeviceTransaction = new MIM.DeviceTransaction();

            try
            {
                bool result = false;
                Nullable<DateTime> blockedDate = null;

                var updateDevice = from device in entityConnection.dbMobikonIMSDataContext.DEVICEs
                                   where device.DEVICEID == requestSetDevice.deviceID
                                   select device;

                foreach (DEVICE device in updateDevice)
                {
                    device.PRODUCTSERIAL = requestSetDevice.productSerial;
                    device.DEVICEDETAILS = requestSetDevice.deviceDetails;
                    device.NOTE = requestSetDevice.note;
                    device.BLOCKEDDATE = blockedDate;

                    if (role == "Administrators" || role == "Sales" || role == "Operations")
                    {
                        device.STATUSID = statusDAL.FindStatusID(requestSetDevice.status);
                        device.CREATEDDATE = DateTime.Now;

                        if (requestSetDevice.status == "Blocked")
                            device.BLOCKEDDATE = DateTime.Now;
                    }
                    device.USERID = userDAL.FindUserID(requestSetDevice.userName);
                    device.COMPANYOWNER = requestSetDevice.companyOwner;

                    device.DEVICETAG = requestSetDevice.deviceTag;
                    device.DEVICETYPE = requestSetDevice.deviceType;

                    entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                }
                entityConnection.dbMobikonIMSDataContext.Transaction.Commit();
                result = true;

                if (role == "Administrators" || role == "Sales" || role == "Operations")
                {
                    result = false;
                    requestSetDeviceTransaction.deviceID = requestSetDevice.deviceID;
                    requestSetDeviceTransaction.deviceDetails = requestSetDevice.deviceDetails;
                    requestSetDeviceTransaction.productSerial = requestSetDevice.productSerial;
                    requestSetDeviceTransaction.createdDate = DateTime.Now;
                    requestSetDeviceTransaction.userID = userDAL.FindUserID(requestSetDevice.userName);
                    requestSetDeviceTransaction.userName = requestSetDevice.userName;
                    requestSetDeviceTransaction.outletID = FindDeviceOutlet(requestSetDevice.deviceID);
                    if (requestSetDeviceTransaction.outletID <= 0)
                    {
                        requestSetDeviceTransaction.outletID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["OutletID"]);
                    }
                    requestSetDeviceTransaction.statusID = requestSetDevice.statusID;
                    requestSetDeviceTransaction.status = requestSetDevice.status;

                    requestSetDeviceTransaction.insured = true;
                    requestSetDeviceTransaction.currentStatus = true;

                    ChangeCurrentStatus(requestSetDeviceTransaction);
                    result = InsertDeviceTransaction(requestSetDeviceTransaction);
                }

                if (result == true)
                    return true;
                else
                    return false;
            }
            //Resolve Concurrency Conflicts by Retaining Database Values (LINQ to SQL)
            catch (ChangeConflictException ex)
            {
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                    //Console.WriteLine(ex.Message);
                    foreach (ObjectChangeConflict occ in entityConnection.dbMobikonIMSDataContext.ChangeConflicts)
                    {
                        // All database values overwrite current values.
                        occ.Resolve(RefreshMode.OverwriteCurrentValues);
                    }
                    entityConnection.dbMobikonIMSDataContext.Transaction.Rollback();
                }
                logger.Error(ex.Message);
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

        public List<MIM.DeviceTransactionRpt> GetDeviceTransactionDetailListRpt(long deviceID = 0, string productSerial = "", string deviceDetails = "", long serialNo = 0, long clientID = 0, string clientName = "", long outletID = 0, string outletName = "", System.Nullable<DateTime> fromDate = null, System.Nullable<DateTime> toDate = null)
        {
            logger.Info("GetDeviceIDwiseTransactionListRpt");
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<Message.DeviceTransactionRpt>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {

                if (fromDate == Convert.ToDateTime("01/01/0001 00:00:00"))
                    fromDate = Convert.ToDateTime("01/01/1753 00:00:00");
                if (toDate == Convert.ToDateTime("01/01/0001 00:00:00"))
                    toDate = Convert.ToDateTime("01/01/1753 00:00:00");


                var selectDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.GETDEVICETRANSACTION(deviceID, productSerial, deviceDetails, serialNo, clientID, clientName, outletID, outletName, fromDate, toDate)
                                              select deviceTransaction;
                //select new
                //{
                //    deviceTransaction.DEVICEID,
                //    deviceTransaction.PRODUCTSERIAL,
                //    deviceTransaction.DEVICEDETAILS,
                //    deviceTransaction.STATUSNAME,
                //    deviceTransaction.STATUSID,                                                 
                //    deviceTransaction.COMPANYOWNER,
                //    deviceTransaction.OUTLETADDRESS,
                //    deviceTransaction.CLIENTADDRESS,
                //    deviceTransaction.CITYID,
                //    deviceTransaction.CITYNAME,
                //    deviceTransaction.CLIENTID,
                //    deviceTransaction.CLIENTNAME,
                //    deviceTransaction.COUNTRYID,
                //    deviceTransaction.COUNTRYNAME,
                //    deviceTransaction.DAMAGEDOLDDEVICE,
                //    deviceTransaction.DC,
                //    deviceTransaction.DCDATE,
                //    deviceTransaction.DCFILE,
                //    deviceTransaction.DELIVERYDATE,
                //    deviceTransaction.HIC,
                //    deviceTransaction.HICDATE,
                //    deviceTransaction.INSURANCECLAIM,
                //    deviceTransaction.INSURED,
                //    deviceTransaction.OUTLETID,
                //    deviceTransaction.OUTLETNAME,
                //    deviceTransaction.RDC,
                //    deviceTransaction.RDCDATE,
                //    deviceTransaction.REMARKS,
                //    deviceTransaction.SERIALNO,
                //    deviceTransaction.TRANSFEROWNERSHIPDATE,
                //    deviceTransaction.USERID,
                //    deviceTransaction.USERNAME,
                //    deviceTransaction.CLIENTCITY,
                //    deviceTransaction.CLIENTCOUNTRY,
                //    deviceTransaction.DEVICETYPE,
                //    deviceTransaction.DEVICETAG

                //};

                foreach (var response in selectDeviceTransaction)
                {
                    responseGetDeviceTransactionRpt.Add(new MIM.DeviceTransactionRpt()
                    {
                        DeviceSerial = response.PRODUCTSERIAL,
                        DeviceDetails = response.DEVICEDETAILS,
                        BrandName = response.CLIENTNAME,
                        OutletName = response.OUTLETNAME,
                        Address = response.OUTLETADDRESS,
                        City = response.CITYNAME,
                        CompanyOwned = response.COMPANYOWNER,
                        DC = response.DC,
                        DCDate = String.Format("{0:dd/MM/yyyy}", response.DCDATE),
                        DeploymentDate = String.Format("{0:dd/MM/yyyy}", response.DELIVERYDATE),
                        HIC = response.HIC,
                        HICDate = String.Format("{0:dd/MM/yyyy}", response.HICDATE),
                        RDC = response.RDC,
                        RDCDate = String.Format("{0:dd/MM/yyyy}", response.RDCDATE),
                        InsuranceClaim = response.INSURANCECLAIM,
                        Insured = response.INSURED == true ? "Yes" : "No",
                        DamagedOldDevice = response.DAMAGEDOLDDEVICE,
                        TransferOwnershipDate = String.Format("{0:dd/MM/yyyy}", response.TRANSFEROWNERSHIPDATE),
                        Status = response.STATUSNAME,
                        //Remarks = response.REMARKS,
                        UserName = response.USERNAME 
                    });
                }

                return responseGetDeviceTransactionRpt;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceTransactionRpt;
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
        public List<MIM.DeviceTransaction> GetStatusDeviceList(string deviceStatus)
        {
            logger.Info("GetStatusDeviceList");
            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<Message.DeviceTransaction>();
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<Message.DeviceTransactionRpt>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.GETSTATUSDEVICELIST(deviceStatus)
                                              select deviceTransaction;
                //select new
                //{
                //    deviceTransaction.DEVICEID,
                //    deviceTransaction.PRODUCTSERIAL,
                //    deviceTransaction.DEVICEDETAILS,
                //    deviceTransaction.STATUSNAME,
                //    deviceTransaction.STATUSID,
                //    deviceTransaction.COMPANYOWNER,                                                 
                //    deviceTransaction.CITYID,
                //    deviceTransaction.CITYNAME,                                                 
                //    deviceTransaction.DAMAGEDOLDDEVICE,
                //    deviceTransaction.DC,
                //    deviceTransaction.DCDATE,
                //    deviceTransaction.DCFILE,
                //    deviceTransaction.DELIVERYDATE,
                //    deviceTransaction.HIC,
                //    deviceTransaction.HICDATE,
                //    deviceTransaction.INSURANCECLAIM,
                //    deviceTransaction.INSURED,
                //    deviceTransaction.OUTLETID,
                //    deviceTransaction.OUTLETNAME,
                //    deviceTransaction.RDC,
                //    deviceTransaction.RDCDATE,
                //    deviceTransaction.REMARKS,
                //    deviceTransaction.TRANSFEROWNERSHIPDATE,
                //    deviceTransaction.USERID,
                //    deviceTransaction.USERNAME,                                                 
                //    deviceTransaction.DEVICETYPE,
                //    deviceTransaction.DEVICETAG,
                //    deviceTransaction.CLIENTNAME,
                //    deviceTransaction.SERIALNO,
                //    deviceTransaction.CURRENTSTATUS
                //};

                foreach (var response in selectDeviceTransaction)
                {
                    responseGetDeviceTransaction.Add(new MIM.DeviceTransaction()
                    {
                        //serialNo = response.SERIALNO,
                        deviceID = response.DEVICEID,
                        productSerial = response.PRODUCTSERIAL,
                        deviceDetails = response.DEVICEDETAILS,
                        status = response.STATUSNAME,
                        statusID = response.STATUSID,

                        companyOwner = response.COMPANYOWNER,
                        //address = response.OUTLETADDRESS,
                        cityID = response.CITYID,
                        cityName = response.CITYNAME,
                        //clientID = response.CLIENTID,
                        clientName = response.CLIENTNAME,
                        //countryID = response.COUNTRYID,
                        //countryName = response.COUNTRYNAME,
                        damagedOldDevice = response.DAMAGEDOLDDEVICE,
                        dc = response.DC,
                        dcDate = response.DCDATE,
                        deliveryDate = response.DELIVERYDATE,
                        hic = response.HIC,
                        hicDate = response.HICDATE,
                        insuranceClaim = response.INSURANCECLAIM,
                        insured = response.INSURED,
                        outletID = response.OUTLETID,
                        outletName = response.OUTLETNAME,
                        rdc = response.RDC,
                        rdcDate = response.RDCDATE,
                        remarks = response.REMARKS,
                        transferOwnershipDate = response.TRANSFEROWNERSHIPDATE,
                        userID = response.USERID,
                        userName = response.USERNAME,
                        //clientCityName = response.CLIENTCITY,
                        //clientCountryName = response.CLIENTCOUNTRY,
                        dcFile = response.DCFILE,
                        dcFileName = Path.GetFileName(response.DCFILE),
                        deviceTag = response.DEVICETAG,
                        deviceType = response.DEVICETYPE,
                        serialNo = response.SERIALNO,
                        currentStatus = response.CURRENTSTATUS
                    });
                }

                return responseGetDeviceTransaction;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceTransaction;
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

        public List<MIM.DeviceTransaction> GetDeviceTransactionHistory(long deviceID = 0, string productSerial = "", string deviceDetails = "", long serialNo = 0, long clientID = 0, string clientName = "", long outletID = 0, string outletName = "", System.Nullable<DateTime> fromDate = null, System.Nullable<DateTime> toDate = null)
        {
            logger.Info("GetDeviceTransactionHistory");
            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<MIM.DeviceTransactionRpt>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.GETDEVICETRANSACTIONHISTORY(deviceID, productSerial, deviceDetails, serialNo, clientID, clientName, outletID, outletName, fromDate, toDate)
                                              select deviceTransaction;
                //select new
                //{
                //    deviceTransaction.DEVICEID,
                //    deviceTransaction.PRODUCTSERIAL,
                //    deviceTransaction.DEVICEDETAILS,
                //    deviceTransaction.STATUSNAME,
                //    deviceTransaction.STATUSID,
                //    deviceTransaction.COMPANYOWNER,
                //    deviceTransaction.OUTLETADDRESS,
                //    deviceTransaction.CLIENTADDRESS,
                //    deviceTransaction.CITYID,
                //    deviceTransaction.CITYNAME,
                //    deviceTransaction.CLIENTID,
                //    deviceTransaction.CLIENTNAME,
                //    deviceTransaction.COUNTRYID,
                //    deviceTransaction.COUNTRYNAME,
                //    deviceTransaction.DAMAGEDOLDDEVICE,
                //    deviceTransaction.DC,
                //    deviceTransaction.DCDATE,
                //    deviceTransaction.DCFILE,
                //    deviceTransaction.DELIVERYDATE,
                //    deviceTransaction.HIC,
                //    deviceTransaction.HICDATE,
                //    deviceTransaction.INSURANCECLAIM,
                //    deviceTransaction.INSURED,
                //    deviceTransaction.OUTLETID,
                //    deviceTransaction.OUTLETNAME,
                //    deviceTransaction.RDC,
                //    deviceTransaction.RDCDATE,
                //    deviceTransaction.REMARKS,
                //    deviceTransaction.SERIALNO,
                //    deviceTransaction.TRANSFEROWNERSHIPDATE,
                //    deviceTransaction.USERID,
                //    deviceTransaction.USERNAME,
                //    deviceTransaction.CLIENTCITY,
                //    deviceTransaction.CLIENTCOUNTRY,
                //    deviceTransaction.DEVICETYPE,
                //    deviceTransaction.DEVICETAG,
                //    deviceTransaction.CURRENTSTATUS
                //};

                foreach (var response in selectDeviceTransaction)
                {
                    responseGetDeviceTransaction.Add(new MIM.DeviceTransaction()
                    {
                        serialNo = response.SERIALNO,
                        deviceID = response.DEVICEID,
                        productSerial = response.PRODUCTSERIAL,
                        deviceDetails = response.DEVICEDETAILS,
                        status = response.STATUSNAME,
                        statusID = response.STATUSID,

                        companyOwner = response.COMPANYOWNER,
                        address = response.OUTLETADDRESS,
                        cityID = response.CITYID,
                        cityName = response.CITYNAME,
                        clientID = response.CLIENTID,
                        clientName = response.CLIENTNAME,
                        countryID = response.COUNTRYID,
                        countryName = response.COUNTRYNAME,
                        damagedOldDevice = response.DAMAGEDOLDDEVICE,
                        dc = response.DC,
                        dcDate = response.DCDATE,
                        deliveryDate = response.DELIVERYDATE,
                        hic = response.HIC,
                        hicDate = response.HICDATE,
                        insuranceClaim = response.INSURANCECLAIM,
                        insured = response.INSURED,
                        outletID = response.OUTLETID,
                        outletName = response.OUTLETNAME,
                        rdc = response.RDC,
                        rdcDate = response.RDCDATE,
                        remarks = response.REMARKS,
                        transferOwnershipDate = response.TRANSFEROWNERSHIPDATE,
                        userID = response.USERID,
                        userName = response.USERNAME,
                        clientCityName = response.CLIENTCITY,
                        clientCountryName = response.CLIENTCOUNTRY,
                        dcFile = response.DCFILE,
                        dcFileName = Path.GetFileName(response.DCFILE),
                        deviceTag = response.DEVICETAG,
                        deviceType = response.DEVICETYPE,
                        currentStatus = response.CURRENTSTATUS
                    });
                }

                return responseGetDeviceTransaction;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceTransaction;
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

        public List<MIM.DeviceTransaction> GetDeviceTransactionDetailList(long deviceID = 0, string productSerial = "", string deviceDetails = "", long serialNo = 0, long clientID = 0, string clientName = "", long outletID = 0, string outletName = "", System.Nullable<DateTime> fromDate = null, System.Nullable<DateTime> toDate = null)
        {
            logger.Info("GetDeviceTransactionDetailList");
            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<Message.DeviceTransaction>();
            List<MIM.DeviceTransactionRpt> responseGetDeviceTransactionRpt = new List<Message.DeviceTransactionRpt>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.GETDEVICETRANSACTION(deviceID, productSerial, deviceDetails, serialNo, clientID, clientName, outletID, outletName, fromDate, toDate)
                                              select deviceTransaction;
                //select new
                //{
                //    deviceTransaction.DEVICEID,
                //    deviceTransaction.PRODUCTSERIAL,
                //    deviceTransaction.DEVICEDETAILS,
                //    deviceTransaction.STATUSNAME,
                //    deviceTransaction.STATUSID,                                                
                //    deviceTransaction.COMPANYOWNER,
                //    deviceTransaction.OUTLETADDRESS,
                //    deviceTransaction.CLIENTADDRESS,
                //    deviceTransaction.CITYID,
                //    deviceTransaction.CITYNAME,
                //    deviceTransaction.CLIENTID,
                //    deviceTransaction.CLIENTNAME,
                //    deviceTransaction.COUNTRYID,
                //    deviceTransaction.COUNTRYNAME,
                //    deviceTransaction.DAMAGEDOLDDEVICE,
                //    deviceTransaction.DC,
                //    deviceTransaction.DCDATE,
                //    deviceTransaction.DCFILE,
                //    deviceTransaction.DELIVERYDATE,
                //    deviceTransaction.HIC,
                //    deviceTransaction.HICDATE,
                //    deviceTransaction.INSURANCECLAIM,
                //    deviceTransaction.INSURED,
                //    deviceTransaction.OUTLETID,
                //    deviceTransaction.OUTLETNAME,
                //    deviceTransaction.RDC,
                //    deviceTransaction.RDCDATE,
                //    deviceTransaction.REMARKS,
                //    deviceTransaction.SERIALNO,
                //    deviceTransaction.TRANSFEROWNERSHIPDATE,
                //    deviceTransaction.USERID,
                //    deviceTransaction.USERNAME,
                //    deviceTransaction.CLIENTCITY,
                //    deviceTransaction.CLIENTCOUNTRY   ,                                                  
                //    deviceTransaction.DEVICETYPE,
                //    deviceTransaction.DEVICETAG,
                //    deviceTransaction.CURRENTSTATUS
                //};

                foreach (var response in selectDeviceTransaction)
                {
                    responseGetDeviceTransaction.Add(new MIM.DeviceTransaction()
                    {
                        serialNo = response.SERIALNO,
                        deviceID = response.DEVICEID,
                        productSerial = response.PRODUCTSERIAL,
                        deviceDetails = response.DEVICEDETAILS,
                        status = response.STATUSNAME,
                        statusID = response.STATUSID,

                        companyOwner = response.COMPANYOWNER,
                        address = response.OUTLETADDRESS,
                        cityID = response.CITYID,
                        cityName = response.CITYNAME,
                        clientID = response.CLIENTID,
                        clientName = response.CLIENTNAME,
                        countryID = response.COUNTRYID,
                        countryName = response.COUNTRYNAME,
                        damagedOldDevice = response.DAMAGEDOLDDEVICE,
                        dc = response.DC,
                        dcDate = response.DCDATE,
                        deliveryDate = response.DELIVERYDATE,
                        hic = response.HIC,
                        hicDate = response.HICDATE,
                        insuranceClaim = response.INSURANCECLAIM,
                        insured = response.INSURED,
                        outletID = response.OUTLETID,
                        outletName = response.OUTLETNAME,
                        rdc = response.RDC,
                        rdcDate = response.RDCDATE,
                        remarks = response.REMARKS,
                        transferOwnershipDate = response.TRANSFEROWNERSHIPDATE,
                        userID = response.USERID,
                        userName = response.USERNAME,
                        clientCityName = response.CLIENTCITY,
                        clientCountryName = response.CLIENTCOUNTRY,
                        dcFile = response.DCFILE,
                        dcFileName = Path.GetFileName(response.DCFILE),
                        deviceTag = response.DEVICETAG,
                        deviceType = response.DEVICETYPE,
                        currentStatus = response.CURRENTSTATUS
                    });
                }

                return responseGetDeviceTransaction;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceTransaction;
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

        public MIM.DeviceTransaction GetDeviceTransactionDetail(long deviceID = 0, string productSerial = "", string deviceDetails = "", long serialNo = 0, long clientID = 0, string clientName = "", long outletID = 0, string outletName = "", System.Nullable<DateTime> fromDate = null, System.Nullable<DateTime> toDate = null)
        {
            logger.Info("GetDeviceTransactionDetail");
            MIM.DeviceTransaction responseGetDeviceTransaction = new Message.DeviceTransaction();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var selectDeviceTransaction = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.GETDEVICETRANSACTIONHISTORY(deviceID, productSerial, deviceDetails, serialNo, clientID, clientName, outletID, outletName, fromDate, toDate)
                                              select deviceTransaction;
                //select new
                //{
                //    deviceTransaction.DEVICEID,
                //    deviceTransaction.PRODUCTSERIAL,
                //    deviceTransaction.DEVICEDETAILS,
                //    deviceTransaction.STATUSNAME,
                //    deviceTransaction.STATUSID,                                                
                //    deviceTransaction.COMPANYOWNER,
                //    deviceTransaction.OUTLETADDRESS,
                //    deviceTransaction.CLIENTADDRESS,
                //    deviceTransaction.CITYID,
                //    deviceTransaction.CITYNAME,
                //    deviceTransaction.CLIENTID,
                //    deviceTransaction.CLIENTNAME,
                //    deviceTransaction.COUNTRYID,
                //    deviceTransaction.COUNTRYNAME,
                //    deviceTransaction.DAMAGEDOLDDEVICE,
                //    deviceTransaction.DC,
                //    deviceTransaction.DCDATE,
                //    deviceTransaction.DCFILE,
                //    deviceTransaction.DELIVERYDATE,
                //    deviceTransaction.HIC,
                //    deviceTransaction.HICDATE,
                //    deviceTransaction.INSURANCECLAIM,
                //    deviceTransaction.INSURED,
                //    deviceTransaction.OUTLETID,
                //    deviceTransaction.OUTLETNAME,
                //    deviceTransaction.RDC,
                //    deviceTransaction.RDCDATE,
                //    deviceTransaction.REMARKS,
                //    deviceTransaction.SERIALNO,
                //    deviceTransaction.TRANSFEROWNERSHIPDATE,
                //    deviceTransaction.USERID,
                //    deviceTransaction.USERNAME,
                //    deviceTransaction.CLIENTCITY,
                //    deviceTransaction.CLIENTCOUNTRY,
                //    deviceTransaction.DEVICETYPE,
                //    deviceTransaction.DEVICETAG,
                //    deviceTransaction.CURRENTSTATUS
                //};

                foreach (var response in selectDeviceTransaction)
                {
                    responseGetDeviceTransaction.serialNo = response.SERIALNO;
                    responseGetDeviceTransaction.deviceID = response.DEVICEID;
                    responseGetDeviceTransaction.productSerial = response.PRODUCTSERIAL;
                    responseGetDeviceTransaction.deviceDetails = response.DEVICEDETAILS;
                    responseGetDeviceTransaction.status = response.STATUSNAME;
                    responseGetDeviceTransaction.statusID = response.STATUSID;

                    responseGetDeviceTransaction.companyOwner = response.COMPANYOWNER;
                    responseGetDeviceTransaction.address = response.OUTLETADDRESS;
                    responseGetDeviceTransaction.cityID = response.CITYID;
                    responseGetDeviceTransaction.cityName = response.CITYNAME;
                    responseGetDeviceTransaction.clientID = response.CLIENTID;
                    responseGetDeviceTransaction.clientName = response.CLIENTNAME;
                    responseGetDeviceTransaction.countryID = response.COUNTRYID;
                    responseGetDeviceTransaction.countryName = response.COUNTRYNAME;
                    responseGetDeviceTransaction.damagedOldDevice = response.DAMAGEDOLDDEVICE;
                    responseGetDeviceTransaction.dc = response.DC;
                    responseGetDeviceTransaction.dcDate = response.DCDATE;
                    responseGetDeviceTransaction.dcFile = response.DCFILE;
                    responseGetDeviceTransaction.deliveryDate = response.DELIVERYDATE;
                    responseGetDeviceTransaction.hic = response.HIC;
                    responseGetDeviceTransaction.hicDate = response.HICDATE;
                    responseGetDeviceTransaction.insuranceClaim = response.INSURANCECLAIM;
                    responseGetDeviceTransaction.insured = response.INSURED;
                    responseGetDeviceTransaction.outletID = response.OUTLETID;
                    responseGetDeviceTransaction.outletName = response.OUTLETNAME;
                    responseGetDeviceTransaction.rdc = response.RDC;
                    responseGetDeviceTransaction.rdcDate = response.RDCDATE;
                    responseGetDeviceTransaction.remarks = response.REMARKS;
                    responseGetDeviceTransaction.transferOwnershipDate = response.TRANSFEROWNERSHIPDATE;
                    responseGetDeviceTransaction.userID = response.USERID;
                    responseGetDeviceTransaction.userName = response.USERNAME;
                    responseGetDeviceTransaction.clientCityName = response.CLIENTCITY;
                    responseGetDeviceTransaction.clientCountryName = response.CLIENTCOUNTRY;
                    responseGetDeviceTransaction.dcFileName = Path.GetFileName(response.DCFILE);
                    responseGetDeviceTransaction.deviceType = response.DEVICETYPE;
                    responseGetDeviceTransaction.deviceTag = response.DEVICETAG;
                    responseGetDeviceTransaction.currentStatus = response.CURRENTSTATUS;

                }
                return responseGetDeviceTransaction;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceTransaction;
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

        public List<MIM.DeviceTransaction> ShowDashBoard(DateTime? fromDate, DateTime? toDate)
        {
            logger.Info("ShowDashBoard");
            List<MIM.DeviceTransaction> responseGetDeviceTransaction = new List<MIM.DeviceTransaction>();

            EntityConnection entityConnection = new EntityConnection();
            try
            {
                var showDashBoard = from deviceTransaction in entityConnection.dbMobikonIMSDataContext.GETDEVICEINVENTORYCOUNT(fromDate, toDate)
                                    select deviceTransaction;
                //select new
                //{
                //   deviceTransaction.STATUSNAME,
                //   deviceTransaction.DEVICECOUNT,
                //   deviceTransaction.DISPLAYORDER
                //};

                foreach (var response in showDashBoard)
                {
                    responseGetDeviceTransaction.Add(new MIM.DeviceTransaction
                    {
                        status = response.STATUSNAME,
                        deviceCount = response.DEVICECOUNT
                    });
                }

                return responseGetDeviceTransaction;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                return responseGetDeviceTransaction;
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

        public void SendEmail(List<MIM.DeviceTransaction> requestSetDeviceTransaction)
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


            //StringBuilder blockedDeviceList  = new StringBuilder("Hi,\n  This is to inform your that you <strong>" + requestSetDeviceTransaction[0].userName + "</strong> recently blocked hardware as below, the same will be blocked for 15 days from today.In case there is no action / assignment of the same hardware by you within this time period of 15 days the same would be unblocked.";);

            try
            {
                responseGetSystemSettings = systemSettingsDAL.GetAllSettings();
                responseGetUser = userDAL.GetUserEmail();

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

                mailMessage.From = new MailAddress(userName, "Konekt Marketing Systems Pvt Ltd");
                mailMessage.Subject = "Update on asset inventory.";


                smtpClient.Host = smtpServer;
                smtpClient.Port = port;
                smtpClient.UseDefaultCredentials = true;

                foreach (MIM.User response in responseGetUser)
                {
                    mailMessage.To.Add(response.email);
                }

                string body = string.Empty;

                foreach (var request in requestSetDeviceTransaction)
                {
                    if (request.status == "Blocked")
                    {
                        if (string.IsNullOrEmpty(body))
                            body = "Hi, <br/> This is to inform your that <Strong>" + request.userName + "</Strong> recently blocked hardware as below on " + String.Format("{0:dd/MM/yyyy}", DateTime.Now) + ", the same will be blocked for 15 days from today. </p> <p>In case there is no action / assignment of the same hardware by you within this time period of 15 days the same would be unblocked. <br/> <table> <tr> <td> <Strong> Device Serial </Strong> </td> <td> <Strong> Device Details </Strong> </td> </tr>  <tr> <td>" + request.productSerial + "</td> <td>" + request.deviceDetails + "</td> </tr>";                       
                        else
                            body = body + " <tr> <td>" + request.productSerial + "</td> <td>" + request.deviceDetails + "</td> </tr>";
                    }
                    else if (request.status == "In Stock")
                    {
                        if (string.IsNullOrEmpty(body))
                            body = "Hi < br/> This is to inform your that <Strong> " + request.userName + " </ Strong > recently.Added hardware on " + String.Format("{ 0:dd / MM / yyyy}", DateTime.Now) + " as below. <br/> <table> <tr> <td> <Strong> Device Serial </Strong> </td> <td> <Strong> Device Details </Strong> </td> </tr>  <tr> <td>" + request.productSerial + "</td> <td>" + request.deviceDetails + "</td> </tr> ";
                       else
                            body = "<tr> <td>" + request.productSerial + "</td> <td>" + request.deviceDetails + "</td> </tr>";
                    }
                    else if (request.status == "Deployed-Active")
                    {
                        if (string.IsNullOrEmpty(body))
                            body = "Hi, <br/> This is to inform your that <Strong>" + request.userName + "</Strong> recently blocked hardware as below on " + String.Format("{0:dd/MM/yyyy}", DateTime.Now) + ", and has now deployed the same as on " + String.Format("{0:dd/MM/yyyy}", DateTime.Now) + ". <br/> <table> <tr> <td> <Strong> Device Serial </Strong> </td> <td> <Strong> Device Details </Strong> </td> </tr>  <tr> <td>" + request.productSerial + "</td> <td>" + request.deviceDetails + "</td> </tr>";
                        else
                            body = "<tr> <td>" + request.productSerial + "</td> <td>" + request.deviceDetails + "</td> </tr>";
                    }
                }

                body = body + " </table> <br/> Please do not reply to this email.";

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

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}

