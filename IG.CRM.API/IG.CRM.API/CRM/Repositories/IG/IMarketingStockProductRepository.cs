using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface  IMarketingStockProductRepository : IBaseRepository
    {  
        List<ProductPriceModel> GetByStockId(Guid stockId, Guid defaultUomId, double defaultCurrencyRate, List<CurrencyModel> currencyList);
    }
}