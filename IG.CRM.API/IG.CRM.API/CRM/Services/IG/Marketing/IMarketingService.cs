using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using IG.CRM.API.Models.IG.Marketing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Services.IG.Marketing
{
    public interface IMarketingService : IBaseService
    {
        OperationResult<List<MarketingStockModel>> GetMarketingStocks(Guid accountId, string token, string function, string controller);
        OperationResult<MarketingStockDetailModel> GetMarketingStockDetails(Guid accountId, Guid stockId, string token, string function, string controller);
    }
}