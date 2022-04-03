using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Helpers;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Services.IG
{
    public class ProductService : BaseService, IProductService
    {
        public readonly IProductRepository _productRepository;
        public readonly IProductPriceRepository _productPriceRepository;
        public readonly IUomRepository _uomRepository;
        public readonly IMinfincourceRepository _minfincourceRepository;

        public ProductService(ICustomerRepository customerRepository,
                               IFunctionRepository functionRepository,
                               IProductRepository productRepository,
                               IProductPriceRepository productPriceLevelRepository,
                               IUomRepository uomRepository,
                               IMinfincourceRepository minfincourceRepository) : base(customerRepository, functionRepository)
        {
            _productRepository = productRepository;
            _productPriceRepository = productPriceLevelRepository;
            _uomRepository = uomRepository;
            _minfincourceRepository = minfincourceRepository;
        }

        public OperationResult<List<ProductModel>> GetPrice(GetPriceModel getPriceModel, string token, string function, string controller)
        {
            LogHelper.Create("product", "Получение цен продуктов", "Входящие данные", JsonConvert.SerializeObject(getPriceModel));
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<ProductModel>>.Create(accessResult.Messages, null);
            }

            var customerApiSettings = _customerRepository.GetByToken(token);
            var firmId = customerApiSettings.GetAttributeValue<EntityReference>("do_firmid");

            //валюта и еденица измерения по-умолчанию
            var defaultCurrency = _minfincourceRepository.Get(DefaultSettings.CurrencyCode, true);
            var defaultUomId = _uomRepository.Get(DefaultSettings.UomCode);

            //список курс валют
            var currencyCourse = _minfincourceRepository.GetAll(true);

            var prices = new List<ProductModel>();
            //using (var sqlConn = new SqlConnection(_productPriceRepository.SqlConnectionString))
            {
                //sqlConn.Open();
                for (int i = 0; i < getPriceModel.ProductCodeList.Count; i++)
                {
                    var priceModel = new ProductModel()
                    {
                        ProductCode = getPriceModel.ProductCodeList[i],
                        Price = 0,
                        RecommendedPrice = 0,
                        CurrencyCode = DefaultSettings.CurrencyCode,
                        ItemUomCode = DefaultSettings.UomCode
                    };

                    //Цена продажи  
                    var price = _productPriceRepository.GetPrice(getPriceModel.ProductCodeList[i], getPriceModel.AccountId, firmId.Id, (double)defaultCurrency.GetAttributeValue<decimal>("do_rate"), defaultUomId, currencyCourse);
                    priceModel.Price = price;

                    //Рекоммендованная цена
                    var recommendedPrice = _productPriceRepository.GetRecommendedPrice(getPriceModel.ProductCodeList[i], (double)defaultCurrency.GetAttributeValue<decimal>("do_rate"), defaultUomId, currencyCourse);

                    priceModel.RecommendedPrice = recommendedPrice;
                    prices.Add(priceModel);
                }
            }
            LogHelper.Create("product", "Получение цен продуктов", "Исходящие данные", JsonConvert.SerializeObject(prices));
            return OperationResult<List<ProductModel>>.Create(accessResult.Messages, prices);
        }

        public OperationResult<List<ProductRecommendPriceModel>> GetRecommendedPrice(GetPriceModel getPriceModel, string token, string function, string controller)
        {
            LogHelper.Create("product", "Получение рекомендованных цен продуктов", "Входящие данные", JsonConvert.SerializeObject(getPriceModel));
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<ProductRecommendPriceModel>>.Create(accessResult.Messages, null);
            }

            var customerApiSettings = _customerRepository.GetByToken(token);
            var firmId = customerApiSettings.GetAttributeValue<EntityReference>("do_firmid");

            //валюта и еденица измерения по-умолчанию
            var defaultCurrency = _minfincourceRepository.Get(DefaultSettings.CurrencyCode, true);
            var defaultUomId = _uomRepository.Get(DefaultSettings.UomCode);

            //список курс валют
            var currencyCourse = _minfincourceRepository.GetAll(true);

            var prices = new List<ProductRecommendPriceModel>();
            // using (var sqlConn = new SqlConnection(_productPriceRepository.SqlConnectionString))
            {
                //sqlConn.Open();
                for (int i = 0; i < getPriceModel.ProductCodeList.Count; i++)
                {
                    var priceModel = new ProductRecommendPriceModel()
                    {
                        ProductCode = getPriceModel.ProductCodeList[i],
                        RecommendedPrice = 0,
                        CurrencyCode = DefaultSettings.CurrencyCode,
                        ItemUomCode = DefaultSettings.UomCode
                    };
                    //Рекоммендованная цена
                    var recommendedPrice = _productPriceRepository.GetRecommendedPrice(getPriceModel.ProductCodeList[i], (double)defaultCurrency.GetAttributeValue<decimal>("do_rate"), defaultUomId, currencyCourse);
                    priceModel.RecommendedPrice = recommendedPrice;
                    prices.Add(priceModel);

                }
            }
            LogHelper.Create("product", "Получение цен продуктов", "Исходящие данные", JsonConvert.SerializeObject(prices));
            return OperationResult<List<ProductRecommendPriceModel>>.Create(accessResult.Messages, prices);
        }
    }
}