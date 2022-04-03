using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace IG.CRM.API.CRM.Repositories.API
{
    public interface ICustomerRepository
    {
        Entity GetByToken(string token, ColumnSet columnSet = null);
    }
}