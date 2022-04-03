using IG.CRM.API.Models;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IAccountDeliveryAddressRepository : IBaseRepository
    {
        List<BaseModel> GetByAccountId(Guid accountId);
    }
}