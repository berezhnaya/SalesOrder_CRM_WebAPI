using System;
using System.Collections.Generic;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.Models;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Text.RegularExpressions;
using IG.CRM.API.Models.IG;
using IG.CRM.API.Helpers;
using Newtonsoft.Json;

namespace IG.CRM.API.CRM.Services.IG
{
    public class AccountService : BaseService, IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountDeliveryAddressRepository _accountDeliveryAddressRepository;

        public AccountService(ICustomerRepository customerRepository,
                              IFunctionRepository functionRepository,
                              IAccountRepository accountRepository,
                              IAccountDeliveryAddressRepository accountDeliveryAddressRepository) : base(customerRepository, functionRepository)
        {
            _accountRepository = accountRepository;
            _accountDeliveryAddressRepository = accountDeliveryAddressRepository;
        }

        public OperationResult<AccountModel> Get(string token, string function, string controller)
        {
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<AccountModel>.Create(accessResult.Messages, null);
            }

            try
            {
                var customer = _customerRepository.GetByToken(token, new ColumnSet("do_accountid"));
                var account = _accountRepository.Get(customer.GetAttributeValue<EntityReference>("do_accountid").Id, new ColumnSet("name"));
                var accountModel = new AccountModel()
                {
                    Id = account.Id,
                    Name = account.GetAttributeValue<string>("name")
                };

                return OperationResult<AccountModel>.Create(OperationMessage.Success(), accountModel);
            }
            catch (Exception ex)
            {
                return OperationResult<AccountModel>.Create(OperationMessage.Error($"Ошибка получения клиента. Exception: {ex.ToString()}"), null);
            }
        }

        //метод для тегерамм-бота
        public OperationResult<List<AccountModel>> GetByPhone(string phone, string token, string function, string controller)
        {
            LogHelper.Create("account", "Получение списка клиентов по номеру телефона", "Входящие данные", phone);
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<AccountModel>>.Create(accessResult.Messages, null);
            }

            if (phone == null || phone == "")
                return OperationResult<List<AccountModel>>.Create(OperationMessage.Error($"Не указан номер телефона"), null);

            Regex regex = new Regex("[^0-9]");
            phone = regex.Replace(phone, "");

            if (phone.Length < 9 || phone.Length > 12)
                return OperationResult<List<AccountModel>>.Create(OperationMessage.Error($"Номер телефона {phone} должен содержать от 9 до 12 цифр."), null);
            try
            {
                var correctPhone = FormatPhone.FormatingPhone(phone);
                var accounts = _accountRepository.GetByPhone(correctPhone);

                if (accounts.Count == 0)
                {
                    LogHelper.Create("account", "Получение списка клиентов по номеру телефона", "Ошибка", $"Клиенты не найдены по номеру телефона {phone}");
                    return OperationResult<List<AccountModel>>.Create(OperationMessage.Error($"Клиенты не найдены по номеру телефона {phone}"), null);
                }
                LogHelper.Create("account", "Получение списка клиентов по номеру телефона", "Исходящие данные", JsonConvert.SerializeObject(accounts));
                return OperationResult<List<AccountModel>>.Create(OperationMessage.Success(), accounts);
            }
            catch (Exception ex)
            {
                return OperationResult<List<AccountModel>>.Create(OperationMessage.Error($"Ошибка получения клиента по номеру телефона. Exception: {ex.ToString()}"), null);
            }
        }

        public OperationResult<List<BaseModel>> GetDeliveryAddresses(Guid accountId, string token, string function, string controller)
        {
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<BaseModel>>.Create(accessResult.Messages, null);
            }

            if (accountId == Guid.Empty)
            {
                return OperationResult<List<BaseModel>>.Create(OperationMessage.Error($"Не указан обязательный параметр accountId."), null);
            }

            try
            {
                var account = _accountRepository.Get(accountId, new ColumnSet("do_accountdeliveryaddressid"));
                if (account == null)
                {
                    return OperationResult<List<BaseModel>>.Create(OperationMessage.Error($"Не найден клиент с ID - {accountId}."), null);
                }

                var addresses = _accountDeliveryAddressRepository.GetByAccountId(accountId);
                var defaultAddress = account.GetAttributeValue<EntityReference>("do_accountdeliveryaddressid");

                if (addresses != null)
                {
                    if (defaultAddress != null)
                    {
                        foreach (var address in addresses)
                        {
                            if (address.Id == defaultAddress.Id)
                            {
                                address.IsDefault = true;
                                break;
                            }
                        }
                    }
                    return OperationResult<List<BaseModel>>.Create(OperationMessage.Success(), addresses);
                }
                return OperationResult<List<BaseModel>>.Create(OperationMessage.Warning($"Не найдены адреса доставки для клиента."), null);
            }
            catch (Exception ex)
            {
                return OperationResult<List<BaseModel>>.Create(OperationMessage.Error($"Ошибка получения адресов доставки по клиенту. Exception: {ex.ToString()}"), null);
            }
        }
    }
}