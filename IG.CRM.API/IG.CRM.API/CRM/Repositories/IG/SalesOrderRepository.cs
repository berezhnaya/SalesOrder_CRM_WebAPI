using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class SalesOrderRepository : BaseRepository, ISalesOrderRepository
    {
        public SalesOrderRepository(IOrganizationService service) : base(service, "salesorder") { }

        public List<Entity> GetByExternalCodeClient(string externalCode, Guid accountId, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                NoLock = true,
                ColumnSet = columnSet,
                Criteria = new FilterExpression()
                {
                    FilterOperator = LogicalOperator.And,
                    Filters =
                    {
                        new FilterExpression()
                        {
                            FilterOperator = LogicalOperator.Or,
                            Conditions =
                            {
                                new ConditionExpression("api_externalcode", ConditionOperator.Equal, externalCode),
                                new ConditionExpression("api_externalcode", ConditionOperator.Like, externalCode + "-%")
                            }
                        },
                        new FilterExpression()
                        {
                            Conditions =
                            {
                                new ConditionExpression("customerid", ConditionOperator.Equal, accountId)
                            }
                        }
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.ToList();
        }

        public List<Entity> GetByExternalCode(string externalCode, Guid accountId, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                NoLock = true,
                ColumnSet = columnSet,
                Criteria = new FilterExpression()
                {
                    Conditions =
                    {
                        new ConditionExpression("api_externalcode", ConditionOperator.Equal, externalCode),
                        new ConditionExpression("customerid", ConditionOperator.Equal, accountId)
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.ToList();
        }
    }
}

