using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.API
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(IOrganizationService service) : base(service, "api_customer") { }

        public Entity GetByToken(string token, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression("api_token", ConditionOperator.Equal, token)
                        }
                    }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault();
        }
    }
}