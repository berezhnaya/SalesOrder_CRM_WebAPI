using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Helpers;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IG.CRM.API.CRM.Services.IG
{
    public class CompanyService : BaseService, ICompanyService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ISystemUserRepository _systemUserRepository;
        private readonly IFirmRepository _firmRepository;
        private readonly IAttributeRepository _attributeRepository;

        public CompanyService(ICustomerRepository customerRepository,
                             IFunctionRepository functionRepository,
                             ISystemUserRepository systemUserRepository,
                             IAccountRepository accountRepository,
                             IFirmRepository firmRepository,
                             IAttributeRepository attributeRepository) : base(customerRepository, functionRepository)
        {
            _accountRepository = accountRepository;
            _systemUserRepository = systemUserRepository;
            _firmRepository = firmRepository;
            _attributeRepository = attributeRepository;
        }

        public OperationResult<List<Contact>> GetContacts(Guid accountId, string token, string function, string controller)
        {
            var accessResult = RoleValidation(token, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<List<Contact>>.Create(accessResult.Messages, null);
            }
            if (accountId == Guid.Empty)
            {
                return OperationResult<List<Contact>>.Create(OperationMessage.Error($"Не указан обязательный параметр accountId."), null);
            }
            try
            {
                //список контактов
                var contactsList = new List<Contact>();

                var account = _accountRepository.Get(accountId, new ColumnSet("ownerid"));
                if (account == null)
                {
                    return OperationResult<List<Contact>>.Create(OperationMessage.Error($"Не найден клиент с ID - {accountId}."), null);
                }

                //фирма
                var customerapi = _customerRepository.GetByToken(token);
                if (!customerapi.Contains("do_firmid"))
                {
                    return OperationResult<List<Contact>>.Create(OperationMessage.Error($"Не найден token."), null);
                }

                var firmId = customerapi.GetAttributeValue<EntityReference>("do_firmid").Id;
                var firm = _firmRepository.Get(firmId, new ColumnSet("do_clientsdeptphone", "do_telephone1", "do_telephone2", "do_telephone3", "do_telephone4"));
                Regex regex = new Regex("[^0-9]");

                //ответственный менеджер
                if (account.Contains("ownerid"))
                {
                    var manager = _systemUserRepository.Get(account.GetAttributeValue<EntityReference>("ownerid").Id, new ColumnSet("do_officemanagerid", "fullname", "mobilephone"));
                    if (manager.Contains("mobilephone"))
                    {
                        var phone = regex.Replace(manager.GetAttributeValue<string>("mobilephone"), "");

                        if (phone.Length >= 9 && phone.Length <= 12)
                        {
                            contactsList.Add(new Contact()
                            {
                                Name = manager.GetAttributeValue<string>("fullname"),
                                Description = "Менеджер",
                                Phone = FormatPhone.FormatingPhone(phone)
                            });
                        }
                    }

                    //отдел по работе с клиентами
                    if (firm.Contains("do_clientsdeptphone"))
                    {
                        var phone = regex.Replace(firm.GetAttributeValue<string>("do_clientsdeptphone"), "");
                        if (phone.Length >= 9 && phone.Length <= 12)
                        {
                            var clientsDept = new Contact()
                            {
                                Description = "Отдел по работе с клиентами",
                                Phone = FormatPhone.FormatingPhone(phone)
                            };
                            //ответственный офис-менеджер
                            if (manager.Contains("do_officemanagerid"))
                            {
                                clientsDept.Name = manager.GetAttributeValue<EntityReference>("do_officemanagerid").Name;
                            }
                            contactsList.Add(clientsDept);
                        }
                    }
                }
                if (firm.Contains("do_telephone1"))
                {
                    var phone = regex.Replace(firm.GetAttributeValue<string>("do_telephone1"), "");
                    if (phone.Length >= 9 && phone.Length <= 12)
                    {
                        contactsList.Add(new Contact()
                        {
                            Name = _attributeRepository.RetrieveAttributeDisplayName(firm.LogicalName, "do_telephone1"),
                            Description = "Основные телефоны",
                            Phone = FormatPhone.FormatingPhone(phone)
                        });
                    }
                }

                if (firm.Contains("do_telephone2"))
                {
                    var phone = regex.Replace(firm.GetAttributeValue<string>("do_telephone2"), "");
                    if (phone.Length >= 9 && phone.Length <= 12)
                    {
                        contactsList.Add(new Contact()
                        {
                            Name = _attributeRepository.RetrieveAttributeDisplayName(firm.LogicalName, "do_telephone2"),
                            Description = "Основные телефоны",
                            Phone = FormatPhone.FormatingPhone(phone)
                        });
                    }
                }

                if (firm.Contains("do_telephone3"))
                {
                    var phone = regex.Replace(firm.GetAttributeValue<string>("do_telephone3"), "");
                    if (phone.Length >= 9 && phone.Length <= 12)
                    {
                        contactsList.Add(new Contact()
                        {
                            Name = _attributeRepository.RetrieveAttributeDisplayName(firm.LogicalName, "do_telephone3"),
                            Description = "Основные телефоны",
                            Phone = FormatPhone.FormatingPhone(phone)
                        });
                    }
                }

                if (firm.Contains("do_telephone4"))
                {
                    var phone = regex.Replace(firm.GetAttributeValue<string>("do_telephone4"), "");
                    if (phone.Length >= 9 && phone.Length <= 12)
                    {
                        contactsList.Add(new Contact()
                        {
                            Name = _attributeRepository.RetrieveAttributeDisplayName(firm.LogicalName, "do_telephone4"),
                            Description = "Основные телефоны",
                            Phone = FormatPhone.FormatingPhone(phone)
                        });
                    }
                }
                return OperationResult<List<Contact>>.Create(OperationMessage.Success(), contactsList);
            }
            catch (Exception ex)
            {
                return OperationResult<List<Contact>>.Create(OperationMessage.Error($"Ошибка получения контактной информации: {ex.Message}"), null);
            }
        }
    }
}