using Microsoft.Xrm.Sdk.Query;
using System;

namespace IG.CRM.API.CRM.Repositories.API
{
    public interface IFunctionRepository
    {
        bool IsExistForCustomer(Guid customerId, string function, string controller, ColumnSet columnSet = null);
    }
}
