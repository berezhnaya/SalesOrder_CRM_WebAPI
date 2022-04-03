using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface ISalesOrderRepository : IBaseRepository
    {
        List<Entity> GetByExternalCodeClient(string externalCode, Guid accountId, ColumnSet columnSet = null);
        List<Entity> GetByExternalCode(string externalCode, Guid accountId, ColumnSet columnSet = null);
    }
}
