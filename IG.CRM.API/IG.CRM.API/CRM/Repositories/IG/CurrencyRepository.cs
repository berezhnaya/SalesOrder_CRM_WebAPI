using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class CurrencyRepository : BaseRepository, ICurrencyRepository
    {
        public CurrencyRepository(IOrganizationService service) : base(service, "do_currency") { }

        public Entity Get(string code, ColumnSet columnSet = null)
        {
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet ?? new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_codenav", ConditionOperator.Equal, code),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault();
        }

        public List<Entity> Get(ColumnSet columnSet = null)
        {
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet ?? new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.ToList();
        }
    }
}