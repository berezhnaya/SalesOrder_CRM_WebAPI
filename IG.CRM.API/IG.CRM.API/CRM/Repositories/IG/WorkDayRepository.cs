using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class WorkDayRepository : BaseRepository, IWorkDayRepository
    {
        public WorkDayRepository(IOrganizationService service) : base(service, "do_isworkday") { }

        public Entity GetByDate(DateTime date, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                NoLock = true,
                ColumnSet = columnSet,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_date", ConditionOperator.Equal, date)
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault();
        }
    }
}