using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models.IG.Marketing
{
    public class MarketingStockByParentModel
    {
        public Guid? StockId { get; set; }
        public Guid? ParentStockId { get; set; }
    }
}