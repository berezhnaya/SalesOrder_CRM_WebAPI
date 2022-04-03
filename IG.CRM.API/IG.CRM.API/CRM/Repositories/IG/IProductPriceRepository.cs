using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;


namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IProductPriceRepository : IBaseRepository
    { 
        double GetPrice(string productCode, Guid accountId, Guid firmId, double defaultCurrencyRate, Guid defaultUomId, List<CurrencyModel> currencyList);
        double GetRecommendedPrice(string productCode, double defaultCurrencyRate, Guid defaultUomId, List<CurrencyModel> currencyList);
    }
}