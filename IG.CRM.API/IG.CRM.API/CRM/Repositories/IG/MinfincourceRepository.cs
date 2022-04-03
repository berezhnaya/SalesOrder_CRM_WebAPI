using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class MinfincourceRepository : BaseRepository, IMinfincourceRepository
    {
        public ICurrencyRepository _currencyRepository;

        public MinfincourceRepository(IOrganizationService service) : base(service, "do_minfincource")
        {
            _currencyRepository = new CurrencyRepository(service);
        }

        public Entity Get(Guid currencyId, bool ismilitary)
        {
            var query = new QueryExpression(_entityName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(true),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_currency", ConditionOperator.Equal, currencyId),
                        new ConditionExpression("do_ismilitary",  ConditionOperator.Equal, ismilitary)
                    }
                }
            };
            query.Orders.Add(new OrderExpression() { AttributeName = "do_date", OrderType = OrderType.Descending });
            return _service.RetrieveMultiple(query).Entities.FirstOrDefault();
        }

        public List<CurrencyModel> GetAll(bool ismilitary)
        {
            var currencyCourse = new List<CurrencyModel>();

            var currencies = _currencyRepository.Get(new ColumnSet("do_currencyid", "do_codenav"));
            foreach (var currency in currencies)
            {
                var query = new QueryExpression(_entityName)
                {
                    NoLock = true,
                    ColumnSet = new ColumnSet(true),
                    Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression("do_currency", ConditionOperator.Equal, currency.Id),
                            new ConditionExpression("do_ismilitary",  ConditionOperator.Equal, ismilitary)
                        }
                    }
                };
                query.Orders.Add(new OrderExpression() { AttributeName = "do_date", OrderType = OrderType.Descending });
                var minfin = _service.RetrieveMultiple(query).Entities.FirstOrDefault();
                currencyCourse.Add(new CurrencyModel()
                {
                    CurrencyId = currency.Id,
                    Code = currency.GetAttributeValue<string>("do_codenav"),
                    Rate = minfin.GetAttributeValue<decimal>("do_rate")
                });
            }
            return currencyCourse;
        }

        public Entity Get(string currencyCode, bool ismilitary)
        {
            var query = new QueryExpression(_entityName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(true),
                LinkEntities =
                {
                    new LinkEntity(_entityName, "do_currency", "do_currency", "do_currencyid", JoinOperator.Inner)
                    {
                        LinkCriteria =
                        {
                            Conditions =
                            {
                                new ConditionExpression("do_codenav", ConditionOperator.Equal, currencyCode),
                                new ConditionExpression("do_ismilitary",  ConditionOperator.Equal, ismilitary)
                            }
                        }
                    }
                }
            };
            query.Orders.Add(new OrderExpression() { AttributeName = "do_date", OrderType = OrderType.Descending });
            return _service.RetrieveMultiple(query).Entities.FirstOrDefault();
        }
    }
}