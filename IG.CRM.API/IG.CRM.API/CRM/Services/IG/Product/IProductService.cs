using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Services.IG
{
    public interface IProductService : IBaseService
    {     
        OperationResult<List<ProductModel>> GetPrice(GetPriceModel getPriceModel, string token, string function, string controller);
        OperationResult<List<ProductRecommendPriceModel>> GetRecommendedPrice(GetPriceModel getPriceModel, string token, string function, string controller);
    }
}