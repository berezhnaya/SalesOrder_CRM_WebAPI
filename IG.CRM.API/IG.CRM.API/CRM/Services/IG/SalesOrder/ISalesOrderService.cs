using IG.CRM.API.Models;

namespace IG.CRM.API.CRM.Services.IG
{
    public interface ISalesOrderService : IBaseService
    {
        OperationResult<AnswerSalesOrderCreateModel> Create(CreateSalesOrderModel salesOrder, string token, string function, string controller);
    }
}