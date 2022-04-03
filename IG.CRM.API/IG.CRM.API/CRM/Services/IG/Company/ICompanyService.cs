using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Services.IG
{
    public interface ICompanyService
    {
        OperationResult<List<Contact>> GetContacts(Guid accountId, string token, string function, string controller);
    }
}