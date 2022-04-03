using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models.IG
{
    public class GetPriceModel
    {
        public Guid AccountId { get; set; }
        public List<string> ProductCodeList { get; set; }
    }
}