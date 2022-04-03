using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IMinfincourceRepository : IBaseRepository
    {
        Entity Get(Guid currencyId, bool ismilitary);
        Entity Get(string currencyCode, bool ismilitary);
        List<CurrencyModel> GetAll(bool ismilitary);
    }
}
