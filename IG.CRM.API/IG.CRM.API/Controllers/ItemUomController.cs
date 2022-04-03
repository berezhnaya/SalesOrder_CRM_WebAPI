using IG.CRM.API.CRM.Services.IG;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace IG.CRM.API.Controllers
{
    public class ItemUomController : ApiController
    {
        private readonly IItemUomService _itemUomService;

        public ItemUomController(IItemUomService itemUomService)
        {
            _itemUomService = itemUomService;
        }        

        [Route("api/itemuom/getbyproduct")]
        [HttpGet]
        public OperationResult<List<ItemUomModel>> GetByProduct(string productCode)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _itemUomService.GetByProduct(productCode, token, "getbyproduct", "itemuom");
        }
    }
}
