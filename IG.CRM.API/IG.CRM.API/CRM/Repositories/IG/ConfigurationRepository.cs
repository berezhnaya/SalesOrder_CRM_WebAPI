using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class ConfigurationRepository : BaseRepository, IConfigurationRepository
    {
        public ConfigurationRepository(IOrganizationService service) : base(service, "do_configuration") { }

        public string GetValueByName(string name)
        {
            QueryExpression query = new QueryExpression(_entityName)
            {
                ColumnSet = new ColumnSet("do_value"),
                Distinct = true,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_name", ConditionOperator.Equal, name)
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault().GetAttributeValue<string>("do_value");
        }
    }
}