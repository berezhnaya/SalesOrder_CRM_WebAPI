using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class AnnotationRepository : BaseRepository, IAnnotationRepository
    {
        public AnnotationRepository(IOrganizationService service) : base(service, "annotation") { }

        public Entity GetLastFileByObjectId(Guid objectId, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("objectid", ConditionOperator.Equal, objectId),
                        new ConditionExpression("isdocument", ConditionOperator.Equal, true)
                    }
                },
                Orders =
                {
                    new OrderExpression("createdon", OrderType.Descending)
                }
            };
            return _service.RetrieveMultiple(query).Entities.FirstOrDefault();
        }


        public DataCollection<Entity> GetFilesByObjectId(Guid objectId, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("objectid", ConditionOperator.Equal, objectId),
                        new ConditionExpression("isdocument", ConditionOperator.Equal, true),
                        new ConditionExpression("mimetype", ConditionOperator.In, new string[] {"image/jpeg", "image/jpg", "image/png" })
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities;
        }
    }
}