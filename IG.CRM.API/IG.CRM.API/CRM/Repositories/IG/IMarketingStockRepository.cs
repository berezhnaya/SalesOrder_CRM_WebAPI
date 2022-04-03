using IG.CRM.API.Models.IG.Marketing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IMarketingStockRepository : IBaseRepository
    {
        List<MarketingStockModel> GetActiveMarketingStocks(Guid firmId, Guid accountId);
        bool IsAccessClientToStock(Guid accountId, Guid stockId);
        List<MarketingStockByParentModel> GetWithParentMarketingStock(List<Guid> stockIds);
    }
}