using Dapper;
using IG.CRM.API.Helpers;
using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class MarketingStockProductRepository : BaseRepository, IMarketingStockProductRepository
    {
        private readonly IItemUomRepository _itemUomRepository;
        private readonly IMinfincourceRepository _minfincourceRepository;

        public MarketingStockProductRepository(IOrganizationService service) : base(service, "do_marketingstockproduct")
        {
            _itemUomRepository = new ItemUomRepository(service);
            _minfincourceRepository = new MinfincourceRepository(service);
        }

        public List<ProductPriceModel> GetByStockId(Guid stockId, Guid defaultUomId, double defaultCurrencyRate, List<CurrencyModel> currencyList)
        {
            var query = @"select 
                        (mproduct.do_amount * 6.0 / 5.0) as [Price], --добавляем НДС
                        --mproduct.do_productid as [ProductId],
                        prod.productnumber as [ProductCode],
                        prod.name as [Name],
                        mstock.do_currencyid as [CurrencyId],
                        itemUom.Do_itemuomId as [ItemUomId],
                        itemUom.Do_coefficient as [Coefficient],
                        defaultUom.Do_coefficient as [DefaultCoefficient],
                        defaultUom.do_uom_itemuomidName as  [DefaultUomName]
                        from do_marketingstockproduct mproduct with(nolock)
                        inner join do_marketingstock mstock on mproduct.do_marketingstockid = mstock.do_marketingstockid
                        inner join product prod on prod.productid = mproduct.do_productid
                        left join do_itemuom itemUom on mproduct.do_itemuomid = itemUom.do_itemuomid
                        left join do_itemuom defaultUom on defaultUom.do_uom_itemuomid = @defaultUomId and defaultUom.do_product_itemuomid = mproduct.do_productid
                        where mproduct.do_marketingstockid = @mStockId";

            var dictionary = new Dictionary<string, object>
            {
                {"@mStockId",  stockId},
                {"@defaultUomId", defaultUomId}
            };
            var productList = new List<ProductPriceModel>();
            using (_sqlConnection = CreateSqlConnection())
            {
                productList = _sqlConnection.Query<ProductPriceModel>(query, new DynamicParameters(dictionary)).AsList();
            }
            if (productList.Count == 0)
            {
                return null;
            }
            for (var i = 0; i < productList.Count; i++)
            {
                var rate = currencyList.Where(x => x.CurrencyId == productList[i].CurrencyId).Select(x => x.Rate).Single();
                productList[i].Price = ConverterPrice.Calculate(productList[i], defaultCurrencyRate, (double)rate);
            }
            return productList;
        }
    }
}