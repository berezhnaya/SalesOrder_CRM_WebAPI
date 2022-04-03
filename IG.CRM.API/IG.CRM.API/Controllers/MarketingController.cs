using IG.CRM.API.CRM.Services.IG.Marketing;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using IG.CRM.API.Models.IG.Marketing;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace IG.CRM.API.Controllers
{
    public class MarketingController : ApiController
    {
        private readonly IMarketingService _marketingService;

        public MarketingController(IMarketingService marketingService)
        {
            _marketingService = marketingService;
        }

        [Route("api/marketing/getstocks")]
        [HttpGet]
        public OperationResult<List<MarketingStockModel>> GetMarketingStock(Guid accountId, string phone)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _marketingService.GetMarketingStocks(accountId, token, "getstocks", "marketing");
        }

        [Route("api/marketing/getstockdetails")]
        [HttpGet]
        public OperationResult<MarketingStockDetailModel> GetMarketingStockDetails(Guid accountId, Guid stockId)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _marketingService.GetMarketingStockDetails(accountId, stockId, token, "getstockdetails", "marketing");
        }

    }
}
