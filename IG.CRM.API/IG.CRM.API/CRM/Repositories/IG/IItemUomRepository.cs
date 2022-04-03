using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IItemUomRepository : IBaseRepository
    {
        Entity Get(Guid uomId, Guid productId, ColumnSet columnSet = null);
        DataCollection<Entity> Get(string productCode);
        Entity Get(Guid productId, string uomCode);
    }
}
