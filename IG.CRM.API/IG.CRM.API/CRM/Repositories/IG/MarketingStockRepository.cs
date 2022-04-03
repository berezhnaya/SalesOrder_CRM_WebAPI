using Dapper;
using IG.CRM.API.Models.IG.Marketing;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class MarketingStockRepository : BaseRepository, IMarketingStockRepository
    {
        public MarketingStockRepository(IOrganizationService service) : base(service, "do_marketingstock") { }

        public List<MarketingStockModel> GetActiveMarketingStocks(Guid firmId, Guid accountId)
        {
            var query = @"--обычные акции
                        select distinct
                        stock.do_marketingstockid as Id,
                        stock.do_name as Name,
                        stock.do_description as Description,
                        case when stock.do_isset is null then 0
                        else stock.do_isset end as IsSet,
                        stock.do_isgrouping,
                        stock.statuscode
                        from do_marketingstock stock
                        inner join do_marketingstock_do_firms firms on stock.do_marketingstockid = firms.do_marketingstockid and firms.do_firmsid = @firmid
                        inner join do_marketingstock_account accounts on stock.do_marketingstockid = accounts.do_marketingstockid and accounts.accountid = @accountid
                        where stock.statuscode = 1 and(stock.do_isgrouping is null or stock.do_isgrouping = 0)
                        and(convert(date, stock.do_start) <= convert(date, getDate()) and convert(date, stock.do_end) >= convert(date, getDate()))

                        union all
                        --Дочерние
                        select distinct
                        stock.do_marketingstockid as Id,
                        stock.do_name as Name,
                        stock.do_description as Description,
                        case when stock.do_isset is null then 0
                        else stock.do_isset end as IsSet,
                        stock.do_isgrouping,
                        stock.statuscode
                        from do_marketingstock stock
                        inner join do_marketingstock_do_firms firms on stock.do_parentmarketingstockid = firms.do_marketingstockid and firms.do_firmsid = @firmid
                        inner join do_marketingstock parentStock on parentStock.do_marketingstockid = stock.do_parentmarketingstockid
                        where parentStock.do_isgrouping = 1 and parentStock.statuscode = 1
                        and(convert(date, parentStock.do_start) <= convert(date, getDate()) and convert(date, parentStock.do_end) >= convert(date, getDate()))";
            using (_sqlConnection = CreateSqlConnection())
            {

                var dictionary = new Dictionary<string, object>
                {
                    {"@firmid",  firmId},
                    {"@accountid", accountId}
                };

                using (_sqlConnection = CreateSqlConnection())
                {
                    return _sqlConnection.Query<MarketingStockModel>(query, new DynamicParameters(dictionary)).AsList();
                }
            }
        }

        public bool IsAccessClientToStock(Guid accountId, Guid stockId)
        {
            var query = @"--обычные акции
                        select distinct
                        stock.do_marketingstockid as Id,
                        stock.do_name as Name,
                        stock.do_description as Description,
                        case when stock.do_isset is null then 0
                        else stock.do_isset end as IsSet,
                        stock.do_isgrouping,
                        stock.statuscode
                        from do_marketingstock stock                     
                        inner join do_marketingstock_account accounts on stock.do_marketingstockid = accounts.do_marketingstockid and accounts.accountid = @accountid
                        where stock.statuscode = 1 and(stock.do_isgrouping is null or stock.do_isgrouping = 0)
                        and(convert(date, stock.do_start) <= convert(date, getDate()) and convert(date, stock.do_end) >= convert(date, getDate()))
                        and stock.do_marketingstockid = @stockId

                        union all
                        --Дочерние
                        select distinct
                        stock.do_marketingstockid as Id,
                        stock.do_name as Name,
                        stock.do_description as Description,
                        case when stock.do_isset is null then 0
                        else stock.do_isset end as IsSet,
                        stock.do_isgrouping,
                        stock.statuscode
                        from do_marketingstock stock                       
                        inner join do_marketingstock parentStock on parentStock.do_marketingstockid = stock.do_parentmarketingstockid
                        where parentStock.do_isgrouping = 1 and parentStock.statuscode = 1
                        and(convert(date, parentStock.do_start) <= convert(date, getDate()) and convert(date, parentStock.do_end) >= convert(date, getDate()))
                        and parentStock.do_marketingstockid = @stockId";
            using (_sqlConnection = CreateSqlConnection())
            {

                var dictionary = new Dictionary<string, object>
                {
                    {"@stockId",  stockId},
                    {"@accountid", accountId}
                };

                using (_sqlConnection = CreateSqlConnection())
                {
                    var result = _sqlConnection.Query<MarketingStockModel>(query, new DynamicParameters(dictionary)).AsList();
                    if (result != null && result.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<MarketingStockByParentModel> GetWithParentMarketingStock(List<Guid> stockIds)
        {
            QueryExpression query = new QueryExpression(_entityName)
            {
                ColumnSet = new ColumnSet("do_marketingstockid", "do_parentmarketingstockid"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("do_marketingstockid", ConditionOperator.In, stockIds)
                    }
                 }
            };
            var result = _service.RetrieveMultiple(query).Entities.ToList();

            if (result == null)
                return null;

            var list = new List<MarketingStockByParentModel>();
            foreach (var stock in result)
            {
                list.Add(new MarketingStockByParentModel()
                {
                    StockId = stock.GetAttributeValue<Guid>("do_marketingstockid"),
                    ParentStockId = (stock.GetAttributeValue<EntityReference>("do_parentmarketingstockid") != null) ? (Guid?)stock.GetAttributeValue<EntityReference>("do_parentmarketingstockid").Id : null
                });
            }
            return list;
        }
    }
}