using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Configuration;
using System.Data.Common;
using System.Transactions;

using log4net.Appender;
using log4net;
using log4net.Core;
using MIM = Mobikon.IMS.Message;

using System.Runtime.CompilerServices;

[assembly: SuppressIldasmAttribute]
namespace Mobikon.IMS.Data
{
    public class EntityConnection
    {
        public MobikonIMSDataContext dbMobikonIMSDataContext;
        //public DbTransaction transaction;       

        private static readonly ILog logger = LogManager.GetLogger(typeof(EntityConnection));

        public EntityConnection()
        {
            InitializeLog4Net();
            //logger.Info("EntityConnection");
            try
            {
                string mobikonIMSConnectionString = Base64Decode(System.Configuration.ConfigurationManager.ConnectionStrings["MobikonIMSConnectionString"].ConnectionString);
                //string mobikonIMSConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MobikonIMSConnectionString"].ConnectionString;

                dbMobikonIMSDataContext = new MobikonIMSDataContext(mobikonIMSConnectionString);

                if (dbMobikonIMSDataContext.Connection.State == ConnectionState.Open)
                    dbMobikonIMSDataContext.Connection.Close();

                dbMobikonIMSDataContext.Connection.Open();
                //dbMobikonIMSDataContext.CommandTimeout = 60* 60 ; // 60* 60 - 1 Hour  
                dbMobikonIMSDataContext.CommandTimeout = 0;   // 0 - indefinite wait period

                //transaction = CreateEntityTransaction();
                CreateEntityTransaction();

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
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

        //private DbTransaction CreateEntityTransaction()
        private void CreateEntityTransaction()
        {
            DbTransaction dbTransaction = null;

            try
            {
                dbTransaction = dbMobikonIMSDataContext.Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                //dbTransaction = dbMobikonIMSDataContext.Connection.BeginTransaction();
                dbMobikonIMSDataContext.Transaction = dbTransaction;

                //return dbTransaction;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                //return dbTransaction;
            }
        }
    }
}
