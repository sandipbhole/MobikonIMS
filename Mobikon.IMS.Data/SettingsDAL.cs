using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Microsoft.Win32;

using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;


namespace Mobikon.IMS.Data
{
    public class SettingsDAL
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SettingsDAL));
        public SettingsDAL()
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
		
		public bool UpdateSettings(MIM.Settings requestSetSystemSettings)
		{
			logger.Info("UpdateSettings");			
			
            EntityConnection entityConnection = new EntityConnection();
			SETTING settings = new SETTING();
			
			try
			{
				settings.SETTINGSNAME = requestSetSystemSettings.settingsName;
                settings.SETTINGSVALUE = requestSetSystemSettings.settingsValue;	

				// Add the new object to the settings collection.
                entityConnection.dbMobikonIMSDataContext.SETTINGs.InsertOnSubmit(settings);
                // Submit the change to the database.            
                entityConnection.dbMobikonIMSDataContext.SubmitChanges(ConflictMode.ContinueOnConflict);
                 entityConnection.dbMobikonIMSDataContext.Transaction.Commit();				
				
				return true;
			}
			catch (Exception ex)
            {
                logger.Error(ex.Message);
                if (entityConnection.dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                {
                     entityConnection.dbMobikonIMSDataContext.Transaction.Dispose();
                    entityConnection.dbMobikonIMSDataContext.Connection.Close();
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

        public List<MIM.Settings> GetAllSettings()
        {
            logger.Info("GetAllSettings");

            List<MIM.Settings> responseGetSystemSettings = new List<MIM.Settings>();
            EntityConnection entityConnection = new EntityConnection();

            try
            {
                var selectSettings = from settings in entityConnection.dbMobikonIMSDataContext.SETTINGs
                                     select settings;

                foreach (var response in selectSettings)
                {
                    responseGetSystemSettings.Add(new MIM.Settings()
                    {
                        settingsName = response.SETTINGSNAME,
                        settingsValue = response.SETTINGSVALUE                       
                    });
                }
                return responseGetSystemSettings;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);               
                return responseGetSystemSettings;
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
