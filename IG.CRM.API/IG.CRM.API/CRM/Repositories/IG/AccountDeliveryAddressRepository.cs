using IG.CRM.API.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class AccountDeliveryAddressRepository : BaseRepository, IAccountDeliveryAddressRepository
    {
        public AccountDeliveryAddressRepository(IOrganizationService service) : base(service, "do_accountdeliveryaddress") { }

        public List<BaseModel> GetByAccountId(Guid accountId)
        {
            QueryExpression query = new QueryExpression(_entityName)
            {
                ColumnSet = new ColumnSet("do_accountdeliveryaddressid", "do_fulladdress"),
                Distinct = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_accountid", ConditionOperator.Equal, accountId),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                }
            };
            var result = _service.RetrieveMultiple(query).Entities;
            if (result.Count == 0)
                return null;

            var listAddresses = new List<BaseModel>();
            foreach (var address in result)
            {
                listAddresses.Add(new BaseModel()
                {
                    Id = address.Id,
                    Name = address.GetAttributeValue<string>("do_fulladdress"),
                    IsDefault = false
                });
            }
            return listAddresses;
        }
    }
}