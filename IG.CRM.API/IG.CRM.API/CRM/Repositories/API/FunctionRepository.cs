using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace IG.CRM.API.CRM.Repositories.API
{
    public class FunctionRepository : BaseRepository, IFunctionRepository
    {
        public FunctionRepository(IOrganizationService service) : base(service, "api_function") { }

        public bool IsExistForCustomer(Guid customerId, string function, string controller, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                LinkEntities =
                {
                    new LinkEntity("api_function", "api_api_role_api_function", "api_functionid", "api_functionid", JoinOperator.Inner)
                    {
                        LinkEntities =
                        {
                            new LinkEntity("api_api_role_api_function", "api_role", "api_roleid", "api_roleid", JoinOperator.Inner)
                            {
                                LinkEntities =
                                {
                                    new LinkEntity("api_role", "api_api_customer_api_role", "api_roleid", "api_roleid", JoinOperator.Inner)
                                    {
                                        LinkEntities =
                                        {
                                            new LinkEntity("api_api_customer_api_role", "api_customer", "api_customerid", "api_customerid", JoinOperator.Inner)
                                            {
                                                LinkCriteria =
                                                {
                                                    Conditions =
                                                    {
                                                        new ConditionExpression("api_customerid", ConditionOperator.Equal, customerId)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("api_function", ConditionOperator.Equal, function),
                        new ConditionExpression("api_controller", ConditionOperator.Equal, controller)
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.Count > 0;
        }
    }
}