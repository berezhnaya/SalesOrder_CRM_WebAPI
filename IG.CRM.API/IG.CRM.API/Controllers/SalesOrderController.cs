using IG.CRM.API.CRM.Services.IG;
using IG.CRM.API.Models;
using System.Web;
using System.Web.Http;

namespace IG.CRM.API.Controllers
{
    public class SalesOrderController : ApiController
    {
        private readonly ISalesOrderService _salesOrderService;
        public SalesOrderController(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        [Route("api/salesorder/create")]
        [HttpPost]
        public OperationResult<AnswerSalesOrderCreateModel> Create(CreateSalesOrderModel salesOrder)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _salesOrderService.Create(salesOrder, token, "create", "salesorder");
        }
    }
}