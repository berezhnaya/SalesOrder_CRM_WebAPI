using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Helpers;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Services.IG
{
    public class ItemUomService : BaseService, IItemUomService
    {
        private readonly IItemUomRepository _itemUomRepository;

        public ItemUomService(ICustomerRepository customerRepository,
                              IFunctionRepository functionRepository,
                              IItemUomRepository itemUomRepository) : base(customerRepository, functionRepository)
        {
            _itemUomRepository = itemUomRepository;
        }

        public OperationResult<List<ItemUomModel>> GetByProduct(string productCode, string token, string function, string controller)
        {
            LogHelper.Create("itemuom", "Получение ед. изм. по продукту", "Входящие данные", productCode);

            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<ItemUomModel>>.Create(accessResult.Messages, null);
            }  

            var itemUoms = _itemUomRepository.Get(productCode);
            if(itemUoms.Count < 1)
            {
                LogHelper.Create("itemuom", "Получение ед. изм. по продукту", "Ошибка", $"Ед. измерения не найдены");
                return OperationResult<List<ItemUomModel>>.Create(OperationMessage.Error($"Ед. измерения не найдены для продукта {productCode}."), null);
            }

            var itemUomList = new List<ItemUomModel>();
            foreach(var itemUom in itemUoms)
            {                
                itemUomList.Add(new ItemUomModel()
                {
                    Id = itemUom.Id,
                    UomId = itemUom.GetAttributeValue<EntityReference>("do_uom_itemuomid").Id,
                    Name = itemUom.GetAttributeValue<string>("do_name")
                });
            }
            LogHelper.Create("itemuom", "Получение ед. изм. по продукту", "Исходящие данные", JsonConvert.SerializeObject(itemUomList));
            return OperationResult<List<ItemUomModel>>.Create(OperationMessage.Success(), itemUomList);
        }
    }
}