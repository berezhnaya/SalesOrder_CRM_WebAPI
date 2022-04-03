using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models.IG
{
    public class CurrencyModel
    {
        public Guid CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public string  Code { get; set; }
    }
}