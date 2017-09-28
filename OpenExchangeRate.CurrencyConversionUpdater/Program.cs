using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenExchangeRate.CurrencyConversionUpdater.Business;


namespace OpenExchangeRate.CurrencyConversionUpdater
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            string syncDays = "1";
            string runmode = "";
            //Prepare parameters
            try
            {

                foreach (string arg in args)
                {

                    int commandIndex = arg.IndexOf(":");
                    string command = arg.Substring(0, commandIndex).ToUpper();

                    switch (command)
                    {
                        case "RUNMODE":
                            runmode = arg.Substring(commandIndex + 1, arg.Length - commandIndex - 1).ToUpper();
                            break;
                        case "SYNCDAYS":
                            syncDays = arg.Substring(commandIndex + 1, arg.Length - commandIndex - 1).ToUpper();
                            break;
                        default:
                            // do other stuff...
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                logger.ErrorException("[" + DateTime.Now.ToString() + "]", ex);
            }

            //Start Syncronizing
            try
            {
                logger.Info(DateTime.Now.ToString() + " Foreign Exchange Started");
                Configuration config = new Configuration();
                OpenExchange openexchangerate = new OpenExchange();
                openexchangerate.AppID = config.AppID;
                openexchangerate.SyncDays = Convert.ToInt32('-' + syncDays);
                openexchangerate.ConnectionString = config.ConnectionString;
                openexchangerate.MsSqlConnectionString = config.MSSQLConnectionString;
                openexchangerate.AppName = config.AppName;
                openexchangerate.AppVersion = config.AppVersion;

                switch (runmode)
                {
                    case "EXCHANGERATELOCALUPDATE":
                        openexchangerate.UpdateLocalExchangeRates();
                        break;
                    case "EXCHANGERATEDATABASEUPDATE":
                        openexchangerate.UpdateExchangeRates();
                        break;
                    default:
                        // do other stuff...
                        break;
                }
                

                logger.Info(DateTime.Now.ToString() + " Foreign Exchange Ended");
                return 1;
            }
            catch (Exception ex)
            {
                logger.Info(DateTime.Now.ToString() + " Foreign Exchange Stoped");
                logger.ErrorException("Exception: ", ex);
                return 0;
            }
        }
    }
}
