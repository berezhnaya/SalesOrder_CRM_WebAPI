using IG.CRM.API.CRM.Services.IG;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace IG.CRM.API.Controllers
{
    public class ProductController : ApiController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [Route("api/product/getprice")]
        [HttpPost]
        public OperationResult<List<ProductModel>> GetByProduct(GetPriceModel getPriceModel)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _productService.GetPrice(getPriceModel, token, "getprice", "product");
        }

        [Route("api/product/getrecommendedprice")]
        [HttpPost]
        public OperationResult<List<ProductRecommendPriceModel>> GetRecommendedPriceByProduct(GetPriceModel getPriceModel)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _productService.GetRecommendedPrice(getPriceModel, token, "getprice", "product");
        }
    }
}
