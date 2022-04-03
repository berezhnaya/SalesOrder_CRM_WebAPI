using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Services.IG
{
    public interface IAccountService
    {
        OperationResult<AccountModel> Get(string token, string function, string controller);
        OperationResult<List<AccountModel>> GetByPhone(string phone, string token, string function, string controller);
        OperationResult<List<BaseModel>> GetDeliveryAddresses(Guid accountId, string token, string function, string controller);
    }
}