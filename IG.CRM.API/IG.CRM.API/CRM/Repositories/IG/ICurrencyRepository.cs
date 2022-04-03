using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface ICurrencyRepository : IBaseRepository
    {
        Entity Get(string code, ColumnSet columnSet = null);
        List<Entity> Get(ColumnSet columnSet = null);
    }
}
