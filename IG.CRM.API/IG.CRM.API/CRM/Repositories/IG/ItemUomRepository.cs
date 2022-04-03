using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class ItemUomRepository : BaseRepository, IItemUomRepository
    {
        public ItemUomRepository(IOrganizationService service) : base(service, "do_itemuom") { }

        public Entity Get(Guid uomId, Guid productId, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                Criteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("do_product_itemuomid", ConditionOperator.Equal, productId),
                                new ConditionExpression("do_itemuomid", ConditionOperator.Equal, uomId),
                                new ConditionExpression("statecode", ConditionOperator.Equal, 0),
                            }
                        }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault();
        }

        public DataCollection<Entity> Get(string productCode)
        {
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = new ColumnSet(new string[] { "do_name", "do_coefficient", "do_uom_itemuomid" }),
                Criteria =
                {
                        Conditions =
                        {
                            new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                        }
                },
                LinkEntities =
                {
                    new LinkEntity("do_itemuom", "product", "do_product_itemuomid", "productid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("productnumber", ConditionOperator.Equal, productCode)
                            }
                        }
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities;
        }

        public Entity Get(Guid productId, string uomCode)
        {

            var query = new QueryExpression(_entityName)
            {
                ColumnSet = new ColumnSet(new string[] { "do_name", "do_coefficient" }),
                Criteria =
                {
                        Conditions =
                        {
                            new ConditionExpression("do_name", ConditionOperator.Equal, uomCode),
                            new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                        }
                },
                LinkEntities =
                {
                    new LinkEntity("do_itemuom", "product", "do_product_itemuomid", "productid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("productid", ConditionOperator.Equal, productId)
                            }
                        }
                    }
                }
            };
            return _service.RetrieveMultiple(query).Entities.SingleOrDefault();
        }
    }
}