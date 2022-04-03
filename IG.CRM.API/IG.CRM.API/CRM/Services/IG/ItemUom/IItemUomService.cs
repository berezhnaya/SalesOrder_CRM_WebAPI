using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Services.IG
{
    public interface IItemUomService
    {
        OperationResult<List<ItemUomModel>> GetByProduct(string productCode, string token, string function, string controller);
    }
}