using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IAccountRepository: IBaseRepository
    {
        DataCollection<Entity> GetChilds(Guid parentAccountId, int statecode, ColumnSet columnSet = null);
        List<AccountModel> GetByPhone(string phone);
        DataCollection<Entity> GetByMarketingStockId(Guid stockId, ColumnSet columnSet = null);
    }
}
