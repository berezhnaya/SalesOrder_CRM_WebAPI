using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Helpers;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using IG.CRM.API.Models.IG.Marketing;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace IG.CRM.API.CRM.Services.IG.Marketing
{
    public class MarketingService : BaseService, IMarketingService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMarketingStockRepository _marketingStockRepository;
        private readonly IMarketingStockProductRepository _marketingStockProductRepository;
        public readonly IUomRepository _uomRepository;
        public readonly IMinfincourceRepository _minfincourceRepository;
        public readonly IProductPriceRepository _productPriceRepository;

        private readonly IAnnotationRepository _annotationRepository;


        public MarketingService(ICustomerRepository customerRepository,
                           IFunctionRepository functionRepository,
                           IAccountRepository accountRepository,
                           IMarketingStockRepository marketingStockRepository,
                           IMarketingStockProductRepository marketingStockProductRepository,
                           IAnnotationRepository annotationRepository,
                           IUomRepository uomRepository,
                           IMinfincourceRepository minfincourceRepository,
                           IProductPriceRepository productPriceRepository) : base(customerRepository, functionRepository)
        {
            _accountRepository = accountRepository;
            _marketingStockRepository = marketingStockRepository;
            _marketingStockProductRepository = marketingStockProductRepository;
            _annotationRepository = annotationRepository;
            _uomRepository = uomRepository;
            _minfincourceRepository = minfincourceRepository;
            _productPriceRepository = productPriceRepository;
        }

        public OperationResult<List<MarketingStockModel>> GetMarketingStocks(Guid accountId, string token, string function, string controller)
        {
            LogHelper.Create("marketing", "Получение списка акций", "Входящие данные", accountId.ToString());
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<MarketingStockModel>>.Create(accessResult.Messages, null);
            }
            //настройки api
            var customerapi = _customerRepository.GetByToken(token);
            var stocks = _marketingStockRepository.GetActiveMarketingStocks(customerapi.GetAttributeValue<EntityReference>("do_firmid").Id, accountId);

            if (stocks == null)
            {
                return OperationResult<List<MarketingStockModel>>.Create(OperationMessage.Success(), null);
            }
            //изображения пока не отправляем
            //foreach (var stock in stocks)
            //{ 
            //    var images = _annotationRepository.GetFilesByObjectId(stock.Id);                   
            //    if(images != null)
            //    {
            //        stock.Images = new List<string>();
            //        foreach(var img in images)
            //        {
            //            stock.Images.Add(img.GetAttributeValue<string>("documentbody"));
            //        }
            //    } 
            //}
            LogHelper.Create("marketing", "Получение списка акций", "Исходящие данные", JsonConvert.SerializeObject(stocks));
            return OperationResult<List<MarketingStockModel>>.Create(OperationMessage.Success(), stocks);
        }

        public OperationResult<MarketingStockDetailModel> GetMarketingStockDetails(Guid accountId, Guid stockId, string token, string function, string controller)
        {
            LogHelper.Create("marketing", "Получение деталей акции", "Входящие данные", $"accountId : {accountId }, stockId : {stockId}");
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<MarketingStockDetailModel>.Create(accessResult.Messages, null);
            }
            //настройки api
            var customerapi = _customerRepository.GetByToken(token);
            var stock = _marketingStockRepository.Get(stockId);

            if (!_marketingStockRepository.IsAccessClientToStock(accountId, stock.Contains("do_parentmarketingstockid") ? stock.GetAttributeValue<EntityReference>("do_parentmarketingstockid").Id : stockId))
            {
                LogHelper.Create("marketing", "Получение деталей акции", "Ошибка", $"У клиента нет доступа к акции! accountId : {accountId }, stockId : {stockId}");
                return OperationResult<MarketingStockDetailModel>.Create(OperationMessage.Error("У клиента нет доступа к акции!"), null);
            }

            //валюта и ед. измерения по умолчанию           
            var defaultCurrencyRate = _minfincourceRepository.Get(DefaultSettings.CurrencyCode, true).GetAttributeValue<decimal>("do_rate");
            var defaultUomId = _uomRepository.Get(DefaultSettings.UomCode);

            //список курс валют
            var currencyCourse = _minfincourceRepository.GetAll(true);
            var customerApiSettings = _customerRepository.GetByToken(token);
            var firmId = customerApiSettings.GetAttributeValue<EntityReference>("do_firmid");

            var productPrice = _marketingStockProductRepository.GetByStockId(stockId, defaultUomId, (double)defaultCurrencyRate, currencyCourse);

            var detailModel = new MarketingStockDetailModel()
            {
                StockId = stockId,
                Name = stock.GetAttributeValue<string>("do_name"),
                IsSet = (stock.Contains("do_isset")) ? stock.GetAttributeValue<bool>("do_isset") : false,
                ProductList = new List<ProductModel>()
            };

            if (stock.Contains("do_fulldescription"))
            {
                detailModel.DetailDescription = stock.GetAttributeValue<string>("do_fulldescription");
            }

            if (productPrice == null)
            {
                accessResult.Messages.Add(OperationMessage.Success("В акции нет продуктов"));
                return OperationResult<MarketingStockDetailModel>.Create(accessResult.Messages, detailModel);
            }

            for (var i = 0; i < productPrice.Count; i++)
            {
                var productModel = new ProductModel()
                {
                    ProductCode = productPrice[i].ProductCode,
                    ItemUomCode = DefaultSettings.UomCode,
                    CurrencyCode = DefaultSettings.CurrencyCode,
                    Price = productPrice[i].Price,
                    RecommendedPrice = _productPriceRepository.GetRecommendedPrice(productPrice[i].ProductCode, (double)defaultCurrencyRate, defaultUomId, currencyCourse)
                };

                if (productPrice[i].Price == 0)
                {
                    productModel.Price = _productPriceRepository.GetPrice(productPrice[i].ProductCode, accountId, firmId.Id, (double)defaultCurrencyRate, defaultUomId, currencyCourse);
                }
                detailModel.ProductList.Add(productModel);
            }
            LogHelper.Create("marketing", "Получение списка акций", "Исходящие данные", JsonConvert.SerializeObject(detailModel));
            return OperationResult<MarketingStockDetailModel>.Create(accessResult.Messages, detailModel);
        }
    }
}