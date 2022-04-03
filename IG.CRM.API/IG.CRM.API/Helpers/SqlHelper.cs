using IG.CRM.API.CRM.Enums.IG;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Helpers
{
    static class SqlHelper
    {
        public static string GetSqlCrmString()
        {           
           return ConfigurationManager.ConnectionStrings["CrmSqlConnectionTest"].ConnectionString; 
        }        
    }
}