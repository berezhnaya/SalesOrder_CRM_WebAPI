using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;


namespace IG.CRM.API.CRM.Repositories.IG
{
    public class UomRepository : BaseRepository, IUomRepository
    {
        public UomRepository(IOrganizationService service) : base(service, "do_uom") { }

        public Guid Get(string code)
        {
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = new ColumnSet(new string[] { "do_uomid" }),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_code", ConditionOperator.Equal, code),
                        new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault().Id;
        }
    }
}