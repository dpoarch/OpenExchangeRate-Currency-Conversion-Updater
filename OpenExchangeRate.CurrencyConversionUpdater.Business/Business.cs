using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenExchangeRate.CurrencyConversionUpdater;
using OpenExchangeRate.CurrencyConversionUpdater.Data;
using System.Net;
using System.Configuration;
using System.IO;
using NLog;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace OpenExchangeRate.CurrencyConversionUpdater.Business
{
    /// <summary>
    /// 
    /// </summary>
    public class OpenExchange
    {

        #region Public Properties

            private int intSyncDays;
            public int SyncDays
            {
                get { return intSyncDays; }
                set { intSyncDays = value; }
            }

            private string strAppID;
            public string AppID
            {
                get { return strAppID; }
                set { strAppID = value; }
            }

            private string strConnectionString;
            public string ConnectionString
            {
                get { return strConnectionString; }
                set { strConnectionString = value; }
            }

            private string strMsSqlConnectionString;
            public string MsSqlConnectionString
            {
                get { return strMsSqlConnectionString; }
                set { strMsSqlConnectionString = value; }
            }

            private string strAppName;
            public string AppName
            {
                get { return strAppName; }
                set { strAppName = value; }
            }

            private string strAppVersion;
            public string AppVersion
            {
                get { return strAppVersion; }
                set { strAppVersion = value; }
            }
        
        #endregion  Public Properties


        #region Public Methods

            public static Logger logger = LogManager.GetCurrentClassLogger();
            
            /// <summary>
            /// Downloads and Saves the meta's of all created  and modified Images on the database based on sycdays. 
            /// Also sets the imageupdate to 1 if it needs to be udpated. 
            /// </summary>
            public void UpdateLocalExchangeRates()
            {
                logger.Info("Update of Local Exchange Rates Started");
                var webServiceUrl = "https://openexchangerates.org/api/latest.json?app_id=" + AppID;
                var request = (HttpWebRequest)WebRequest.Create(webServiceUrl);
                var response = (HttpWebResponse)request.GetResponse();
                var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();

                var json = JObject.Parse(rawJson);  //Turns your raw string into a key value lookup
                var rates = json["rates"];

                JToken USDrate;
                JToken CADrate;
                string stringUSD = null;
                string stringCAD = null;
                foreach (var data in rates)
                {
                    
                    var country = ((Newtonsoft.Json.Linq.JProperty)(data)).Name;
                    var rate = ((Newtonsoft.Json.Linq.JProperty)(data)).Value;
                    if (country == "CAD")
                    {
                        CADrate = rate;
                        stringCAD = CADrate.ToString();
                    }
                    if (country == "USD")
                    {
                        USDrate = rate;
                        stringUSD = USDrate.ToString();
                    }
                }
                
                ExchangeRateDataContext ctx = new ExchangeRateDataContext(ConnectionString);

                ctx.ExchangeRate_Insert(null, stringUSD, stringCAD, DateTime.Now);
                logger.Info("Latest Exchange Rate has been saved");
                logger.Info("Update of Exchange Rates Ended");
            }

            public void UpdateExchangeRates()
            {
                logger.Info("Update of Exchange Rates Started");

                //var xml = XDocument.Load("database.xml");
                //var query = ((System.Xml.Linq.XElement)(((System.Xml.Linq.XContainer)(xml.Root)).FirstNode)).FirstAttribute;
                //if (query.Value == "MSSQLSERVER")
                //{
                //    var test = ((System.Xml.Linq.XContainer)(((System.Xml.Linq.XObject)(query)).Parent)).FirstNode;
                //}
               
                ExchangeRateDataContext ctx = new ExchangeRateDataContext(ConnectionString);
                ExchangeRateServerDataContext ers = new ExchangeRateServerDataContext(MsSqlConnectionString);
                List<ExchangeRate> exchangeratedata = ctx.ExchangeRates.ToList();
                
                foreach(var data in exchangeratedata)
                {
                   int datacount = ers.ExchangeRatesServers.Where(r => r.date == data.date).Count();
                    if(datacount != 0)
                    {
                           ExchangeRatesServer exchangerateserverdata  = ers.ExchangeRatesServers.FirstOrDefault(r => r.date == data.date);
                           if(exchangerateserverdata.USD != data.USD || exchangerateserverdata.CAD != data.CAD){ 
                               ers.ExchangeRate_UPDATE(data.Id,data.USD,data.CAD,data.date); 
                           }
                    }
                    else
                    {
                        ers.ExchangeRate_Insert(data.Id,data.USD,data.CAD,data.date); 
                    }
                }
                logger.Info("Latest Exchange Rate has been saved");
                logger.Info("Update of Exchange Rates Ended");
            }

        #endregion Public Methods
      
    }

    /// <summary>
    /// Configuration Settings
    /// </summary>
    public class Configuration
        {
                public string AppID
                {
                    get { return ConfigurationManager.AppSettings.Get("AppID"); ; }
                }

                public string AppName
                {
                    get { return ConfigurationManager.AppSettings.Get("appName"); ; }
                }

                public string AppVersion
                {
                    get { return ConfigurationManager.AppSettings.Get("appVersion"); ; }
                }

                public string ConnectionString
                {
                    get { return ConfigurationManager.AppSettings.Get("connection"); ; }
                }

                public string MSSQLConnectionString
                {
                    get { return ConfigurationManager.AppSettings.Get("MSSQLSERVER"); ; }
                }
        }

}
