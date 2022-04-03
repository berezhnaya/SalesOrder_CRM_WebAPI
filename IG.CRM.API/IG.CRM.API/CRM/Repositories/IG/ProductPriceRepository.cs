using Dapper;
using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class ProductPriceRepository : BaseRepository, IProductPriceRepository
    {
        private readonly IItemUomRepository _itemUomRepository;
        private readonly IMinfincourceRepository _minfincourceRepository;

        public ProductPriceRepository(IOrganizationService service) : base(service, "do_productpricelevel")
        {

            _itemUomRepository = new ItemUomRepository(service);
            _minfincourceRepository = new MinfincourceRepository(service);
        }

        /// <summary>
        /// Получить максимальную цену продажи
        /// </summary>
        /// <param name="sqlConn">sql connection</param>
        /// <param name="productCode">код продукта</param>
        /// <param name="accountId">id организации</param>
        /// <param name="firmId">id компании</param>
        /// <param name="defaultCurrencyRate">курс валюты по умолчанию</param>
        /// <param name="defaultUomId">ед. изм. в которую перевести</param>
        /// <returns>цена</returns>
        public double GetPrice(string productCode, Guid accountId, Guid firmId, double defaultCurrencyRate, Guid defaultUomId, List<CurrencyModel> currencyList)
        {
            //Запрос для связи с индивидуальной ценой
            var queryIndividualPrice = @"--declare @productCode varchar(36) set @productCode = 'ТОВ-У000049' 
                                        --declare @firmId varchar(36) set @firmId = '67802E36-49AB-DC11-94ED-000423AF733E'
                                        --declare @defaultUomId varchar(36) set @defaultUomId = '2003BFA0-2E1C-E011-9C41-00155D036605' --ШТ
                                        --declare @accountId varchar(36) set @accountId = 'F1FD00EA-608B-E011-A441-00155D010B02' 

                                        select                        
                                        (indPrice.Do_pricenotnds * 6.0 / 5.0) as [Price], --добавляем НДС					    
                                        product.productid as [ProductId],
                                        indPrice.Do_currencyindividualpricelistid as [CurrencyId],
                                        indPrice.Do_itemuomindividualpricelistid as [ItemUomId],
                                        itemUom.Do_coefficient as [Coefficient],
                                        defaultUom.Do_coefficient as [DefaultCoefficient],
                                        defaultUom.do_uom_itemuomidName as  [DefaultUomName]
                                        from Do_individualpricelist indPrice
                                        join Do_itemuom itemUom on itemUom.Do_itemuomId = indPrice.Do_itemuomindividualpricelistid
                                        join product on product.productid =  indPrice.Do_productindividualpricelistid
                                        left join do_itemuom defaultUom on defaultUom.do_uom_itemuomid = @defaultUomId and defaultUom.do_product_itemuomid = productid
                                        where indPrice.Do_accountindividualpricelistid = @accountId
                                        and  product.productNumber = @productCode
                                        and indPrice.do_firmid = @firmId
                                        and indPrice.statecode = 0
                                        group by indPrice.Do_pricenotnds, indPrice.Do_currencyindividualpricelistid, indPrice.Do_itemuomindividualpricelistid, itemUom.Do_coefficient, product.productid, defaultUom.Do_coefficient, defaultUom.do_uom_itemuomidName";

            //Запрос для связи N:N и основного прайса
            var queryOthers = @"--declare @productCode varchar(36) set @productCode = 'ТОВ-У000049' 
                            --declare @firmId varchar(36) set @firmId = '67802E36-49AB-DC11-94ED-000423AF733E'
                            --declare @defaultUomId varchar(36) set @defaultUomId = '2003BFA0-2E1C-E011-9C41-00155D036605' --ШТ
                            --declare @accountId varchar(36) set @accountId = 'F1FD00EA-608B-E011-A441-00155D010B02' 

                             select distinct *
                             from 
                             (
	                            select distinct
	                            (prodPriceLevel.Do_amount * 6.0 / 5.0) as [Price], --добавляем НДС						
	                            product.productid as [ProductId],
	                            price.Do_currencypriceid as [CurrencyId],
	                            itemUom.Do_itemuomId as [ItemUomId],
	                            itemUom.Do_coefficient as [Coefficient],
	                            defaultUom.Do_coefficient as [DefaultCoefficient],
	                            defaultUom.do_uom_itemuomidName as  [DefaultUomName]
	                            from do_account_do_price accNtNprice
	                            join Account acc on acc.AccountId = accNtNprice.accountId
	                            join Do_price price on accNtNprice.do_priceId = price.Do_priceId  
	                            join Do_productpricelevel prodPriceLevel on price.Do_priceId = prodPriceLevel.Do_pricelevelpriceid
	                            join product on productid = prodPriceLevel.do_pricelevelproductid
	                            join Do_itemuom itemUom with(nolock) on prodPriceLevel.Do_itemuomprice = itemUom.Do_itemuomId
	                            left join do_itemuom defaultUom on defaultUom.do_uom_itemuomid = @defaultUomId and defaultUom.do_product_itemuomid = productid
	                            where acc.AccountId = @accountId
	                            and price.do_firmid = @firmId
	                            and product.productnumber = @productCode
	                            and prodPriceLevel.statecode = 0
	                            group by prodPriceLevel.Do_amount, price.Do_currencypriceid, itemUom.Do_itemuomId, itemUom.Do_coefficient, product.productid, defaultUom.Do_coefficient, defaultUom.do_uom_itemuomidName

	                            union all

	                            select 
	                            (prodPriceLevel.Do_amount * 6.0 / 5.0) as [Price], --добавляем НДС							
	                            product.productid as [ProductId],
	                            price.Do_currencypriceid as [CurrencyId],
	                            itemUom.Do_itemuomId as [ItemUomId],
	                            itemUom.Do_coefficient as [Coefficient],
	                            defaultUom.Do_coefficient as [DefaultCoefficient],
	                            defaultUom.do_uom_itemuomidName as  [DefaultUomName]
	                            from Account acc with(nolock)
	                            join Do_price price on acc.Do_priceaccountid = price.Do_priceId 
	                            join Do_productpricelevel prodPriceLevel on price.Do_priceId = prodPriceLevel.Do_pricelevelpriceid
	                            join Do_itemuom itemUom with(nolock) on prodPriceLevel.Do_itemuomprice = itemUom.Do_itemuomId
	                            join product on product.productid = prodPriceLevel.Do_pricelevelproductid
	                            left join do_itemuom defaultUom on defaultUom.do_uom_itemuomid = @defaultUomId and defaultUom.do_product_itemuomid = productid
	                            where acc.AccountId = @accountId
	                            and price.do_firmid = @firmId
	                            and product.productnumber = @productCode
	                            and prodPriceLevel.statecode = 0
                            ) as prices";

            using (var command = new SqlCommand(queryIndividualPrice, _sqlConnection))
            {
                command.Parameters.Add(new SqlParameter("@accountId", accountId));
                command.Parameters.Add(new SqlParameter("@productCode", productCode));
                command.Parameters.Add(new SqlParameter("@firmId", firmId));
                command.Parameters.Add(new SqlParameter("@defaultUomId", defaultUomId));
                _sqlConnection.Open();
                var reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    reader.Close();
                    command.CommandText = queryOthers;
                    reader = command.ExecuteReader();
                }
                if (reader.HasRows)
                {
                    List<ProductPriceModel> prices = new List<ProductPriceModel>();
                    while (reader.Read())
                    {
                        var productPriceLevel = new ProductPriceModel()
                        {
                            //ProductId = reader["ProductId"] != DBNull.Value ? (Guid?)(new Guid(reader["ProductId"].ToString())) : null,
                            Price = reader["Price"] != DBNull.Value ? Convert.ToDouble(reader["Price"]) : 0,
                            CurrencyId = reader["CurrencyId"] != DBNull.Value ? (Guid?)(new Guid(reader["CurrencyId"].ToString())) : null,
                            ItemUomId = reader["ItemUomId"] != DBNull.Value ? (Guid?)(new Guid(reader["ItemUomId"].ToString())) : null,
                            ItemUomCoefficient = reader["Coefficient"] != DBNull.Value ? Convert.ToDecimal(reader["Coefficient"]) : 0,
                            DefaultUomCoefficient = reader["DefaultCoefficient"] != DBNull.Value ? Convert.ToDecimal(reader["DefaultCoefficient"]) : 0,
                            DefaultUomName = reader["DefaultUomName"] != DBNull.Value ? reader["DefaultUomName"].ToString() : ""
                        };
                        if (productPriceLevel.Price != 0)
                        {
                            var rate = (double)currencyList.Where(x => x.CurrencyId == productPriceLevel.CurrencyId).Select(x => x.Rate).Single();
                            productPriceLevel.Price = CalculatePrice(productPriceLevel, defaultCurrencyRate, rate);
                        }
                        prices.Add(productPriceLevel);
                    }
                    reader.Close();
                    return prices.Max(x => x.Price);
                }
                reader.Close();
                _sqlConnection.Close();
            }
            return 0;
        }

        public double GetRecommendedPrice(string productCode, double defaultCurrencyRate, Guid defaultUomId, List<CurrencyModel> currencyList)
        {
            var query = @"  --declare @productCode varchar(36) set @productCode = 'ТОВ-У000049'                            
                            --declare @defaultUomId varchar(36) set @defaultUomId = '2003BFA0-2E1C-E011-9C41-00155D036605' --ШТ

                            select 
                            (prodPriceLevel.Do_amount * 6.0 / 5.0) as [Price], --добавляем НДС
                            product.productid as [ProductId],
                            price.Do_currencypriceid as [CurrencyId],
                            itemUom.Do_itemuomId as [ItemUomId],
                            itemUom.Do_coefficient as [Coefficient],
                            defaultUom.Do_coefficient as [DefaultCoefficient],
                            defaultUom.do_uom_itemuomidName as  [DefaultUomName]
                            from do_productpricelevel prodPriceLevel with(nolock)
                            join Do_price price on price.Do_priceId = prodPriceLevel.do_pricelevelpriceid
                            join product on product.productid = prodPriceLevel.do_pricelevelproductid
                            inner join do_itemuom itemUom on prodPriceLevel.do_itemuomprice = itemUom.do_itemuomid
                            left join do_itemuom defaultUom on defaultUom.do_uom_itemuomid = @defaultUomId and defaultUom.do_product_itemuomid = productid
                            where prodPriceLevel.statecode = 0
                            and price.api_isusingbot = 1                            
                            and product.productnumber = @productCode";
            var dictionary = new Dictionary<string, object>
            {
                {"@productCode",  productCode},
                {"@defaultUomId", defaultUomId}
            };

            using (_sqlConnection = CreateSqlConnection())
            {
                var productPriceLevelList = _sqlConnection.Query<ProductPriceModel>(query, new DynamicParameters(dictionary)).AsList();
                if (productPriceLevelList != null && productPriceLevelList.Count > 0)
                {
                    for (var i = 0; i < productPriceLevelList.Count; i++)
                    {
                        if (productPriceLevelList[i].Price != 0)
                        {
                            var rate = currencyList.Where(x => x.CurrencyId == productPriceLevelList[i].CurrencyId).Select(x => x.Rate).Single();
                            productPriceLevelList[i].Price = CalculatePrice(productPriceLevelList[i], defaultCurrencyRate, (double)rate);
                        }
                    }
                    return productPriceLevelList.Max(x => x.Price);
                }
            }
            return 0.0;
        }

        public double CalculatePrice(ProductPriceModel productPriceLevel, double defaultCurrencyRate, double priceCurrencyRate)
        {
            var price = 0.0;

            if (productPriceLevel.DefaultUomCoefficient == productPriceLevel.ItemUomCoefficient)
            {
                price = productPriceLevel.Price;
            }
            //ИСКЛЮЧЕНИЕ ДЛЯ ПЕРЕВОДА В КГ 
            else if (productPriceLevel.DefaultUomName == "КГ" && productPriceLevel.DefaultUomCoefficient > 1)
            {
                price = productPriceLevel.Price / Convert.ToDouble(productPriceLevel.ItemUomCoefficient) * Convert.ToDouble(productPriceLevel.DefaultUomCoefficient);
            }
            else if (productPriceLevel.DefaultUomCoefficient != 1)
            {
                price = productPriceLevel.Price * Convert.ToDouble(productPriceLevel.DefaultUomCoefficient);
            }
            else
            {
                price = productPriceLevel.Price / Convert.ToDouble(productPriceLevel.ItemUomCoefficient);
            }
            if (price != 0)
            {
                price = Math.Round((price * (double)priceCurrencyRate) / (double)defaultCurrencyRate, 2);
            }
            return price;
        }
    }
}
