using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace IG.CRM.API.Helpers
{
    public class LogHelper
    {
        public static void Create(string controller, string function, string logType, string logtxt)
        {
            var fileName = "";
            switch(controller)
            {
                case "account":
                    {
                        fileName = "AccountLog";
                        break;
                    }
                case "salesorder":
                    {
                        fileName = "SalesOrderLog";
                        break;
                    }
                case "marketing":
                    {
                        fileName = "MarketingLog";
                        break;
                    }
                case "itemuom":
                    {
                        fileName = "ItemUomLog";
                        break;
                    }
                case "product":
                    {
                        fileName = "ProductLog";
                        break;
                    }
                default:
                    {
                        return;
                    }

            }
            var isTest = Convert.ToBoolean(ConfigurationManager.AppSettings["isTest"]);
            var directory = (isTest) ? "IG.CRM.API.TEST" : "IG.CRM.API";
            var path = $"C:\\inetpub\\wwwroot\\{directory}\\Logs\\{fileName}.txt";
            using (var sw = File.AppendText(path))
            {
                sw.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} - {function} - {logType}{Environment.NewLine}{logtxt}{Environment.NewLine}");
            }
        }
    }
}