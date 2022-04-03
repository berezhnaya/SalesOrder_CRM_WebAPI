using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Dapper;
using IG.CRM.API.Models.IG;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class AccountRepository : BaseRepository, IAccountRepository
    {
        public AccountRepository(IOrganizationService service) : base(service, "account") { }

        public DataCollection<Entity> GetChilds(Guid parentAccountId, int statecode, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                Criteria =
                     {
                         Conditions =
                         {
                            new ConditionExpression("parentaccountid", ConditionOperator.Equal, parentAccountId),
                            new ConditionExpression("statecode", ConditionOperator.Equal, statecode)
                         }
                     }
            };
            return _service.RetrieveMultiple(query).Entities;
        }

        public List<AccountModel> GetByPhone(string phone)
        {
            var query = @"select acc.accountid as Id,
                        acc.name as Name
                        from account acc
                        inner join contact cont on cont.parentcustomerid = acc.accountid 
                        where acc.statecode = 0 and cont.statecode = 0
                        and (cont.mobilephone like ('%' + @phone) or cont.telephone1 like ('%' + @phone)) 
                        and api_isaccesstelegrambot = 1";
            var dictionary = new Dictionary<string, object>
            {
                { "@phone",  phone}
            };

            using (_sqlConnection = CreateSqlConnection())
            {
                return _sqlConnection.Query<AccountModel>(query, new DynamicParameters(dictionary)).AsList();
            }
        }

        public DataCollection<Entity> GetByMarketingStockId(Guid stockId, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            QueryExpression query = new QueryExpression("account")
            {
                ColumnSet = columnSet,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)

                    }
                 },
                LinkEntities =
                {
                    new LinkEntity("account", "do_marketingstock_account", "accountid", "accountid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("do_marketingstockid", ConditionOperator.Equal, stockId)
                            }
                        }
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities;
        }
    }
}
