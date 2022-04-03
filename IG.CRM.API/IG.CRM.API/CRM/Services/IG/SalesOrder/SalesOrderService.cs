using IG.CRM.API.CRM.Enums.IG;
using IG.CRM.API.CRM.Models;
using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG.Action;
using IG.CRM.API.Models.IG;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using IG.CRM.API.CRM.Enums.API;
using System.Linq;
using IG.CRM.API.Helpers;
using IG.CRM.API.Models.IG.Marketing;
using System.Configuration;

namespace IG.CRM.API.CRM.Services.IG
{
    public class SalesOrderService : BaseService, ISalesOrderService
    {
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IAccountDeliveryAddressRepository _accountDeliveryAddressRepository;
        private readonly IProductRepository _productRepository;
        private readonly IItemUomRepository _itemUomRepository;
        private readonly IUomRepository _uomRepository;
        private readonly IFirmRepository _firmRepository;
        private readonly ISalesOrderDetailRepository _salesOrderDetailRepository;
        private readonly IWorkDayRepository _workDayRepository;
        private readonly ISystemUserRepository _systemUserRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IExecuteActionRepository _executeActionRepository;
        private readonly IMarketingStockRepository _marketingStockRepository;

        public SalesOrderService(ICustomerRepository customerRepository,
                                 IFunctionRepository functionRepository,
                                 ISalesOrderRepository salesOrderRepository,
                                 IAccountRepository accountRepository,
                                 ICurrencyRepository currencyRepository,
                                 IAccountDeliveryAddressRepository accountDeliveryAddressRepository,
                                 IProductRepository productRepository,
                                 IItemUomRepository itemUomRepository,
                                 IUomRepository uomRepository,
                                 IFirmRepository firmRepository,
                                 ISalesOrderDetailRepository salesOrderDetailRepository,
                                 IWorkDayRepository workDayRepository,
                                 ISystemUserRepository systemUserRepository,
                                 IConfigurationRepository configurationRepository,
                                 IExecuteActionRepository executeActionRepository,
                                 IMarketingStockRepository marketingStockRepository) : base(customerRepository, functionRepository)
        {
            _salesOrderRepository = salesOrderRepository;
            _accountRepository = accountRepository;
            _currencyRepository = currencyRepository;
            _accountDeliveryAddressRepository = accountDeliveryAddressRepository;
            _productRepository = productRepository;
            _itemUomRepository = itemUomRepository;
            _uomRepository = uomRepository;
            _firmRepository = firmRepository;
            _salesOrderDetailRepository = salesOrderDetailRepository;
            _workDayRepository = workDayRepository;
            _systemUserRepository = systemUserRepository;
            _configurationRepository = configurationRepository;
            _executeActionRepository = executeActionRepository;
            _marketingStockRepository = marketingStockRepository;
        }

        new public OperationResult<string> RoleValidation(string token, Guid accountid, string function, string controller)
        {
            if (token == null)
            {
                return OperationResult<string>.Create(OperationMessage.Error("Headers запроса должен содержать token"), "");
            }
            var customer = _customerRepository.GetByToken(token);
            if (customer == null)
            {
                return OperationResult<string>.Create(OperationMessage.Error("Не найден клиент с текущим token ключём"), "");
            }
            //принадлежит ли токен именно этому клиенту 
            if (customer.GetAttributeValue<OptionSetValue>("api_accesstypeoption").Value == (int)AccessType.Client && customer.GetAttributeValue<EntityReference>("do_accountid").Id != accountid)
            {
                return OperationResult<string>.Create(OperationMessage.Error($"Токен не принадлежит этому клиенту!"), "");
            }
            if (!_functionRepository.IsExistForCustomer(customer.Id, function, controller, new ColumnSet(false)))
            {
                return OperationResult<string>.Create(OperationMessage.Error($"У вас нет доступа к функции '{function}' контроллера '{controller}'"), "");
            }
            var account = _accountRepository.Get(accountid, new ColumnSet("accountid"));
            if (account == null)
            {
                return OperationResult<string>.Create(OperationMessage.Error("У вас нет доступа к этому клиенту"), "");
            }

            return OperationResult<string>.Create(OperationMessage.Success(), "");
        }

        public OperationResult<AnswerSalesOrderCreateModel> Create(CreateSalesOrderModel salesorder, string token, string function, string controller)
        {
            var messages = new List<OperationMessage>();
            var customerapi = _customerRepository.GetByToken(token);

            //Клиент для отгрузки      
            if (!salesorder.ShipmentAccountId.HasValue)
            {
                messages.Add(OperationMessage.Error("Отсутствует ShipmentAccountId"));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //проверить не прислали ли клиентов, к которым нет доступа
            var accessResult = RoleValidation(token, (Guid)salesorder.ShipmentAccountId, function, controller);
            if (!accessResult.Success)
            {
                return OperationResult<AnswerSalesOrderCreateModel>.Create(accessResult.Messages, null);
            }

            if (salesorder.PaymentAccountId.HasValue)
            {
                accessResult = RoleValidation(token, (Guid)salesorder.PaymentAccountId, function, controller);
                if (!accessResult.Success)
                {
                    return OperationResult<AnswerSalesOrderCreateModel>.Create(accessResult.Messages, null);
                }
            }

            //Внешний номер заказа
            if (salesorder.ExternalCode == null || (salesorder.ExternalCode == ""))
            {
                messages.Add(OperationMessage.Error("Укажите внешний номер заказа!"));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //вся ли инфа есть в карточке клиента
            var shipmentAccount = _accountRepository.Get(salesorder.ShipmentAccountId.Value);
            if (!ValidateAccount(shipmentAccount))
            {
                messages.Add(OperationMessage.Error("Недостаточно заполнена карточка клиента! Просим обратиться к Вашему менеджеру!"));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //Проверка продуктов
            if (salesorder.Products.Count == 0)
            {
                messages.Add(OperationMessage.Error("Список продуктов к заказу пуст."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            foreach (var product in salesorder.Products.ToArray())
            {
                var valid = ValidationProduct(product);
                if (valid.Level != "SUCCESS")
                {
                    messages.Add(valid);
                    salesorder.Products.Remove(product);
                }
            }

            if (salesorder.Products.Count == 0)
            {
                messages.Add(OperationMessage.Error("Все продукты в списке не валидны."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //два отдельных метода для АПИ и телеграма

            if (customerapi.GetAttributeValue<OptionSetValue>("api_accesstypeoption").Value == (int)AccessType.Global)
            {
                return CreateFromTelegramBot(salesorder, customerapi.GetAttributeValue<EntityReference>("do_firmid"));
            }
            else if (customerapi.GetAttributeValue<OptionSetValue>("api_accesstypeoption").Value == (int)AccessType.Client)
            {
                return CreateFromAPI(salesorder, customerapi.GetAttributeValue<EntityReference>("do_firmid"));
            }

            messages.Add(OperationMessage.Error("Не указана настройка токена."));
            return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
        }

        public OperationResult<AnswerSalesOrderCreateModel> CreateFromTelegramBot(CreateSalesOrderModel salesorder, EntityReference firm)
        {
            //проверка данных
            LogHelper.Create("salesorder", "Создание заказа", "Входящие данные", JsonConvert.SerializeObject(salesorder));
            var checkMail = new MailSenderModel()
            {
                Subject = $"Телеграм_Бот - входящие данные",
                Description = JsonConvert.SerializeObject(salesorder),
                To = new List<string>() { ConfigurationManager.AppSettings["emailForError"] }
            };
            _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(checkMail));

            var messages = new List<OperationMessage>();
            var serverUrl = _configurationRepository.GetValueByName("ServerUrl");

            //проверить наличие заказа по ExternalCode
            var salesOrderExist = _salesOrderRepository.GetByExternalCodeClient("T-BOT" + salesorder.ExternalCode, (Guid)salesorder.ShipmentAccountId);
            if (salesOrderExist.Count > 0)
            {
                messages.Add(OperationMessage.Error($"Заказ с внешним номером заказа (ExternalCode) <{salesorder.ExternalCode}> уже существует."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            var createSalesOrder = new Entity();

            //клиент для отгрузки
            var shipmentAccount = _accountRepository.Get(salesorder.ShipmentAccountId.Value);
            createSalesOrder["customerid"] = new EntityReference("account", shipmentAccount.Id);

            //Клиент для выставления счёта
            if (salesorder.PaymentAccountId.HasValue)
            {
                var paymentAccount = _accountRepository.Get(salesorder.PaymentAccountId.Value, new ColumnSet(false));
                if (paymentAccount != null)
                {
                    createSalesOrder["do_accountinvoiceid"] = new EntityReference("account", paymentAccount.Id);
                }
                else
                {
                    messages.Add(OperationMessage.Warning("Клиент для выставления счета c таким кодом не найден. Счет будет выставлен клиенту для отгрузки"));
                    createSalesOrder["do_accountinvoiceid"] = new EntityReference("account", shipmentAccount.Id);
                }
            }
            else
            {
                messages.Add(OperationMessage.Warning("Клиент для выставления счета не указан. Счет будет выставлен клиенту для отгрузки"));
                createSalesOrder["do_accountinvoiceid"] = new EntityReference("account", shipmentAccount.Id);
            }

            //Требуемая дата доставки
            try
            {
                var deliveryDate = DateTime.Today.AddDays(1);
                if (salesorder.DeliveryDate != null && salesorder.DeliveryDate != "" && Convert.ToDateTime(salesorder.DeliveryDate) > DateTime.Today.Date)
                {
                    deliveryDate = Convert.ToDateTime(salesorder.DeliveryDate);
                }
                if (CheckIsWeekendDay(deliveryDate))
                {
                    while (CheckIsWeekendDay(deliveryDate))
                    {
                        deliveryDate = deliveryDate.AddDays(1);
                    }
                }
                createSalesOrder["requestdeliveryby"] = deliveryDate;
            }
            catch
            {
                messages.Add(OperationMessage.Error("Некорректная или пустая дата поставки!"));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //Ответственный
            var su = _systemUserRepository.Get(shipmentAccount.GetAttributeValue<EntityReference>("ownerid").Id, new ColumnSet(new string[] { "do_code", "fullname", "domainname", "internalemailaddress" }));
            if (!su.Contains("do_code"))
            {
                var fullname = su.Contains("fullname") ? su.GetAttributeValue<string>("fullname") : su.GetAttributeValue<string>("domainname");

                var mailModel = new MailSenderModel()
                {
                    Subject = "Телеграм-бот - Ошибка создания заказа",
                    Description = $"В карточке специалиста {fullname} не указан код менеджера!",
                    To = new List<string>() { su.GetAttributeValue<string>("internalemailaddress"), ConfigurationManager.AppSettings["emailForError"] }
                };
                _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mailModel));


                messages.Add(OperationMessage.Error($"В карточке специалиста не указан код менеджера! Просим обратиться к Вашему менеджеру."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }
            createSalesOrder["ownerid"] = shipmentAccount.GetAttributeValue<EntityReference>("ownerid");

            //Фирма из настроек АПИ           
            createSalesOrder["do_firm"] = firm;

            //Создан из бота
            createSalesOrder["do_createdfrom"] = new OptionSetValue((int)SalesOrderCreatedFrom.TelegramBot);

            //Способ доставки. По-умолчанию = Собственный транспорт
            createSalesOrder["shippingmethodcode"] = new OptionSetValue((salesorder.DeliveryType != null) ? (int)salesorder.DeliveryType : (int)ShippingMethodCode.OwnTransport);

            //Способ оплаты. По-умолчанию = Безнал
            createSalesOrder["do_accountingtype"] = new OptionSetValue((salesorder.AccountingType != null) ? (int)salesorder.AccountingType : (int)AccountingType.Cashless);

            //Условия оплаты. По-умолчанию = предоплата
            createSalesOrder["do_paymenttype"] = new OptionSetValue((salesorder.PaymentType != null) ? (int)salesorder.PaymentType : (int)PaymentType.Prepayment);

            //Валюта
            var currencyCode = (salesorder.CurrencyCode != "" && salesorder.CurrencyCode != null) ? salesorder.CurrencyCode : "UAH";
            var currency = _currencyRepository.Get(currencyCode);
            createSalesOrder["do_currency"] = new EntityReference("do_currency", currency.Id);

            //ДР
            createSalesOrder["do_bonus"] = new Money(0);

            //Основной контакт
            createSalesOrder["do_contactmainid"] = shipmentAccount.GetAttributeValue<EntityReference>("primarycontactid");

            //Контакт для отгрузки
            if (shipmentAccount.Contains("do_contact3"))
                createSalesOrder["do_contactshippingid"] = new EntityReference("contact", shipmentAccount.GetAttributeValue<EntityReference>("do_contact3").Id);

            else createSalesOrder["do_contactshippingid"] = createSalesOrder["do_contactmainid"];

            //Контакт для оплаты
            if (shipmentAccount.Contains("do_contact5"))
                createSalesOrder["do_contactpaymentid"] = new EntityReference("contact", shipmentAccount.GetAttributeValue<EntityReference>("do_contact5").Id);
            else createSalesOrder["do_contactpaymentid"] = createSalesOrder["do_contactmainid"];

            //Адрес доставки
            var deliveryAddressId = shipmentAccount.GetAttributeValue<EntityReference>("do_accountdeliveryaddressid").Id;
            if (salesorder.AccountDeliveryAddressId != null)
                deliveryAddressId = (Guid)salesorder.AccountDeliveryAddressId;

            var addressDelivery = _accountDeliveryAddressRepository.Get(deliveryAddressId);
            //Из организации
            createSalesOrder["willcall"] = true;
            //Страна
            if (addressDelivery.Contains("do_countryid"))
                createSalesOrder["do_countryid"] = addressDelivery.GetAttributeValue<EntityReference>("do_countryid");
            //Область
            if (addressDelivery.Contains("do_regionid"))
                createSalesOrder["do_regionsalesid"] = addressDelivery.GetAttributeValue<EntityReference>("do_regionid");
            //Почтовый индекс
            if (addressDelivery.Contains("do_postalcode"))
                createSalesOrder["shipto_postalcode"] = addressDelivery.GetAttributeValue<string>("do_postalcode");
            //Город
            if (addressDelivery.Contains("do_cityid"))
                createSalesOrder["do_citysalesorderid"] = addressDelivery.GetAttributeValue<EntityReference>("do_cityid");
            //Улица
            if (addressDelivery.Contains("do_streetid"))
                createSalesOrder["do_streetid"] = addressDelivery.GetAttributeValue<EntityReference>("do_streetid");
            //Дом
            if (addressDelivery.Contains("do_house"))
                createSalesOrder["shipto_line1"] = addressDelivery.GetAttributeValue<string>("do_house");

            //Примечание к заказу
            // createSalesOrder["description"] = shipmentAccount.GetAttributeValue<string>("do_noteorder") + "\n" + salesorder.NoteOrder;

            createSalesOrder["description"] = "СОЗДАН ТЕСТОВО ИЗ Телеграм-бота!!! НЕ РАЗМЕЩАТЬ В NAV";

            //Прайс-лист (СИСТЕМНОЕ ПО УМОЛЧАНИЮ!)
            createSalesOrder["pricelevelid"] = new EntityReference("pricelevel", new Guid("1B28A6A5-3630-DF11-88B1-001517817114"));

            //Валюта (СИСТЕМНОЕ ПО УМОЛЧАНИЮ!)
            createSalesOrder["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("7194A8FA-BE1B-DE11-9BD7-000E2EDB432F"));

            var ownerEmail = su.GetAttributeValue<string>("internalemailaddress");

            //разделить продукты согласно акций и не акций
            var listByStock = salesorder.Products.Select(x => x.StockId).Distinct().ToList();
            var listWithParent = _marketingStockRepository.GetWithParentMarketingStock(listByStock.Where(x => x.HasValue).Select(x => x.Value).ToList()).ToList();
            listWithParent.Add(new MarketingStockByParentModel() { StockId = null, ParentStockId = null });

            //кол-во заказов
            var orderCount = 1;
            // список заказов для размещения
            var orderIdList = new Dictionary<string, Guid>();
            //для возврата кодов созданных заказов
            var codes = "";
            //общая сумма по всем заказам
            var sum = 0.0;

            foreach (var parentStockId in listWithParent.Select(x => x.ParentStockId).Distinct())
            {
                if (parentStockId == null)
                {
                    foreach (var stockId in listByStock)
                    {
                        try
                        {
                            //внешний номер заказа
                            createSalesOrder["api_externalcode"] = (listByStock.Count > 1) ? $"T-BOT{salesorder.ExternalCode}-{orderCount}" : $"T-BOT{salesorder.ExternalCode}";
                            var newSalesOrderId = CreateSalesOrder(createSalesOrder, salesorder.Products.Where(x => x.StockId == stockId).ToList(), stockId);

                            var code = _salesOrderRepository.Get(newSalesOrderId, new ColumnSet("do_code")).GetAttributeValue<string>("do_code");
                            codes += (orderCount > 1) ? ($", {code}") : code;
                            //сумма по заказу
                            sum += _salesOrderRepository.Get(newSalesOrderId, new ColumnSet("do_sum")).GetAttributeValue<double>("do_sum");
                            orderIdList.Add(code, newSalesOrderId);
                            orderCount++;
                        }
                        catch (Exception ex)
                        {
                            var description = $"Внешний номер заказа: {salesorder.ExternalCode} <br/> Ошибка : {ex.Message} <br/> </br> Список продуктов :";
                            foreach (var product in salesorder.Products.Where(x => x.StockId == stockId).ToList())
                            {
                                description += $"<br/> - { product.ProductCode }, {product.Quantity} {DefaultSettings.UomCode}.";
                            }
                            //уведомление менеджера и прогера о не создании заказа                  
                            var mail = new MailSenderModel()
                            {
                                To = new List<string>() { ownerEmail, ConfigurationManager.AppSettings["emailForError"] },
                                Subject = $"Телеграм_Бот - Ошибка при создании заказа по клиенту {shipmentAccount.GetAttributeValue<string>("name")}",
                                Description = description
                            };
                            _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                            LogHelper.Create("salesorder", "Создание заказа", "Ошибка", description);
                        }
                    }
                }
                else
                {
                    var stockByParent = listWithParent.Where(x => x.ParentStockId == parentStockId).Select(x => x.StockId);
                    var products = new List<CreateSalesOrderDetailModel>();
                    foreach (var stockId in stockByParent)
                    {
                        products.AddRange(salesorder.Products.Where(x => x.StockId == stockId));
                    }
                    try
                    {
                        //внешний номер заказа
                        createSalesOrder["api_externalcode"] = (listByStock.Count > 1) ? $"T-BOT{salesorder.ExternalCode}-{orderCount}" : $"T-BOT{salesorder.ExternalCode}";
                        var newSalesOrderId = CreateSalesOrder(createSalesOrder, products, parentStockId);

                        var code = _salesOrderRepository.Get(newSalesOrderId, new ColumnSet("do_code")).GetAttributeValue<string>("do_code");
                        codes += (orderCount > 1) ? ($", {code}") : code;
                        //сумма по заказу
                        sum += _salesOrderRepository.Get(newSalesOrderId, new ColumnSet("do_sum")).GetAttributeValue<double>("do_sum");
                        orderIdList.Add(code, newSalesOrderId);
                        orderCount++;
                    }
                    catch (Exception ex)
                    {
                        var description = $"Внешний номер заказа: {salesorder.ExternalCode} <br/> Ошибка : {ex.Message} <br/> </br> Список продуктов :";
                        foreach (var product in products)
                        {
                            description += $"<br/> - { product.ProductCode }, {product.Quantity} {DefaultSettings.UomCode}.";
                        }
                        //уведомление менеджера и прогера о не создании заказа                  
                        var mail = new MailSenderModel()
                        {
                            To = new List<string>() { ownerEmail, ConfigurationManager.AppSettings["emailForError"] },
                            Subject = $"Телеграм_Бот - Ошибка при создании заказа по клиенту {shipmentAccount.GetAttributeValue<string>("name")}",
                            Description = description
                        };
                        _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                        LogHelper.Create("salesorder", "Создание заказа", "Ошибка", description);
                    }
                }
            }
            //если ни один заказ не создался отправляем глобальную ошибку
            if (orderCount == 0)
            {
                messages.Add(OperationMessage.Error($"Во время создания заказа произошла ошибка. Обратитесь к менеджеру."));
                OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //отправка заказов в НАВ
            foreach (var newSalesOrder in orderIdList)
            {
                var salesOrderUrl = $"<a href='{serverUrl}/main.aspx?etn=salesorder&id={newSalesOrder.Value}&pagetype=entityrecord'>{newSalesOrder.Key}</a>";
                try
                {
                    var resultSendToNav = SendToNavision(new SendToNavModel() { SalesOrderId = newSalesOrder.Value });
                    if (resultSendToNav.Level == "SUCCESS")
                    {
                        //уведомление менеджера о создании заказа     
                        var mail = new MailSenderModel()
                        {
                            Subject = $"Телеграм_Бот - Заказ {shipmentAccount.GetAttributeValue<string>("name")}",
                            Description = $"По клиенту {shipmentAccount.GetAttributeValue<string>("name")} создан и размещен заказ в NAV {salesOrderUrl}. ",
                            To = new List<string>() { ownerEmail, ConfigurationManager.AppSettings["emailForError"] }
                        };
                        _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                        messages.Add(OperationMessage.Success($"Заказ № {newSalesOrder.Key} успешно создан и размещен в NAV"));
                        LogHelper.Create("salesorder", "Размещение заказа", "Успех", mail.Description);
                    }
                    else if (resultSendToNav.Level == "ERROR")
                    {
                        var mail = new MailSenderModel()
                        {
                            Subject = $"Телеграм_Бот - Ошибка отправки заказа в НАВ",
                            Description = $"По клиенту {shipmentAccount.GetAttributeValue<string>("name")} не размещен заказ в NAV {salesOrderUrl}. Error: {resultSendToNav.Message}",
                            To = new List<string>() { ownerEmail, ConfigurationManager.AppSettings["emailForError"] }
                        };
                        _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                        //warning что бы общая ошибка была success
                        messages.Add(OperationMessage.Warning($"Заказ № {newSalesOrder.Key} не размещен в NAV"));
                        //логи
                        LogHelper.Create("salesorder", "Размещение заказа", "Ошибка", mail.Description);
                    }
                }
                catch (Exception ex)
                {
                    var mail = new MailSenderModel()
                    {
                        Subject = $"Телеграм_Бот - Ошибка размещения заказа в НАВ",
                        Description = $"По клиенту {shipmentAccount.GetAttributeValue<string>("name")} не размещен заказ в NAV {salesOrderUrl}. Error: {ex.Message}",
                        To = new List<string>() { ownerEmail, ConfigurationManager.AppSettings["emailForError"] }
                    };
                    _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                    //лог
                    LogHelper.Create("salesorder", "Размещение заказа", "Ошибка", mail.Description);
                    messages.Add(OperationMessage.Warning($"Заказ № {newSalesOrder.Key} не размещен в NAV"));
                }
            }

            var answer = new AnswerSalesOrderCreateModel()
            {
                SalesOrderCode = codes,
                DeliveryDate = salesorder.DeliveryDate,
                DeliveryAddress = addressDelivery.GetAttributeValue<string>("do_fulladdress"),
                Sum = sum
            };
            return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, answer);
        }

        public Guid CreateSalesOrder(Entity createSalesOrder, List<CreateSalesOrderDetailModel> products, Guid? stockId)
        {
            //акция
            if (stockId != null)
            {
                createSalesOrder["do_marketingstockid"] = new EntityReference("do_marketingstock", stockId.Value);
            }
            var newSalesOrderId = _salesOrderRepository.Create(createSalesOrder);
            CreateSalesOrderDetailList(products, newSalesOrderId);
            return newSalesOrderId;
        }

        public OperationResult<AnswerSalesOrderCreateModel> CreateFromAPI(CreateSalesOrderModel salesorder, EntityReference firm)
        {
            var messages = new List<OperationMessage>();
            var serverUrl = _configurationRepository.GetValueByName("ServerUrl");

            // проверить наличие заказа по ExternalCode
            var salesOrderExist = _salesOrderRepository.GetByExternalCode(salesorder.ExternalCode, (Guid)salesorder.ShipmentAccountId);
            if (salesOrderExist.Count > 0)
            {
                messages.Add(OperationMessage.Error($"Заказ с внешним номером заказа (ExternalCode) <{salesorder.ExternalCode}> уже существует. Обратитесь к менеджеру."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            var createSalesOrder = new Entity();
            var shipmentAccount = _accountRepository.Get(salesorder.ShipmentAccountId.Value);
            createSalesOrder["customerid"] = new EntityReference("account", shipmentAccount.Id);

            //Клиент для выставления счёта
            if (salesorder.PaymentAccountId.HasValue)
            {
                var paymentAccount = _accountRepository.Get(salesorder.PaymentAccountId.Value, new ColumnSet(false));
                if (paymentAccount != null)
                {
                    createSalesOrder["do_accountinvoiceid"] = new EntityReference("account", paymentAccount.Id);
                }
                else
                {
                    messages.Add(OperationMessage.Warning("Клиент для выставления счета c таким кодом не найден. Счет будет выставлен клиенту для отгрузки"));
                    createSalesOrder["do_accountinvoiceid"] = new EntityReference("account", shipmentAccount.Id);
                }
            }
            else
            {
                messages.Add(OperationMessage.Warning("Клиент для выставления счета не указан. Счет будет выставлен клиенту для отгрузки"));
                createSalesOrder["do_accountinvoiceid"] = new EntityReference("account", shipmentAccount.Id);
            }

            //Требуемая дата поставки           
            try
            {
                var deliveryDate = Convert.ToDateTime(salesorder.DeliveryDate);
                if (deliveryDate < DateTime.Today)
                {
                    messages.Add(OperationMessage.Error("Дата поставки должна быть > сегодня !"));
                    return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
                }
                if (CheckIsWeekendDay(deliveryDate))
                {
                    messages.Add(OperationMessage.Error($"Требуемая дата поставки {salesorder.DeliveryDate} приходится на выходной день."));
                    return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
                }
                createSalesOrder["requestdeliveryby"] = deliveryDate;

            }
            catch
            {
                messages.Add(OperationMessage.Error("Некорректная или пустая дата поставки!"));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }

            //Ответственный
            var su = _systemUserRepository.Get(shipmentAccount.GetAttributeValue<EntityReference>("ownerid").Id, new ColumnSet(new string[] { "do_code", "fullname", "domainname", "internalemailaddress" }));
            if (!su.Contains("do_code"))
            {
                var fullname = su.Contains("fullname") ? su.GetAttributeValue<string>("fullname") : su.GetAttributeValue<string>("domainname");

                var mailModel = new MailSenderModel()
                {
                    Subject = "API CRM - Ошибка создания заказа",
                    Description = $"В карточке специалиста {fullname} не указан код менеджера!",
                    To = new List<string>() { su.GetAttributeValue<string>("internalemailaddress"), ConfigurationManager.AppSettings["emailForError"] }
                };

                _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mailModel));

                messages.Add(OperationMessage.Error($"В карточке специалиста не указан код менеджера! Просим обратиться к Вашему менеджеру."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }
            createSalesOrder["ownerid"] = shipmentAccount.GetAttributeValue<EntityReference>("ownerid");

            //Фирма из настроек АПИ           
            createSalesOrder["do_firm"] = firm;

            //создан из апи
            createSalesOrder["do_createdfrom"] = new OptionSetValue((int)SalesOrderCreatedFrom.API);

            //внешний номер заказа
            createSalesOrder["api_externalcode"] = salesorder.ExternalCode;

            //Способ доставки. По-умолчанию = Собственный транспорт
            createSalesOrder["shippingmethodcode"] = new OptionSetValue((salesorder.DeliveryType != null) ? (int)salesorder.DeliveryType : (int)ShippingMethodCode.OwnTransport);

            //Способ оплаты. По-умолчанию = Безнал
            createSalesOrder["do_accountingtype"] = new OptionSetValue((salesorder.AccountingType != null) ? (int)salesorder.AccountingType : (int)AccountingType.Cashless);

            //Условия оплаты. По-умолчанию = предоплата
            createSalesOrder["do_paymenttype"] = new OptionSetValue((salesorder.PaymentType != null) ? (int)salesorder.PaymentType : (int)PaymentType.Prepayment);

            //Валюта
            var currencyCode = (salesorder.CurrencyCode != "" && salesorder.CurrencyCode != null) ? salesorder.CurrencyCode : "UAH";
            var currency = _currencyRepository.Get(currencyCode);
            createSalesOrder["do_currency"] = new EntityReference("do_currency", currency.Id);

            //ДР
            createSalesOrder["do_bonus"] = new Money(0);

            //Основной контакт
            createSalesOrder["do_contactmainid"] = shipmentAccount.GetAttributeValue<EntityReference>("primarycontactid");

            //Контакт для отгрузки
            if (shipmentAccount.Contains("do_contact3"))
                createSalesOrder["do_contactshippingid"] = new EntityReference("contact", shipmentAccount.GetAttributeValue<EntityReference>("do_contact3").Id);

            else createSalesOrder["do_contactshippingid"] = createSalesOrder["do_contactmainid"];

            //Контакт для оплаты
            if (shipmentAccount.Contains("do_contact5"))
                createSalesOrder["do_contactpaymentid"] = new EntityReference("contact", shipmentAccount.GetAttributeValue<EntityReference>("do_contact5").Id);
            else createSalesOrder["do_contactpaymentid"] = createSalesOrder["do_contactmainid"];

            //Адрес доставки
            var deliveryAddressId = shipmentAccount.GetAttributeValue<EntityReference>("do_accountdeliveryaddressid").Id;
            if (salesorder.AccountDeliveryAddressId != null)
                deliveryAddressId = (Guid)salesorder.AccountDeliveryAddressId;

            var addressDelivery = _accountDeliveryAddressRepository.Get(deliveryAddressId);
            //Из организации
            createSalesOrder["willcall"] = true;
            //Страна
            if (addressDelivery.Contains("do_countryid"))
                createSalesOrder["do_countryid"] = addressDelivery.GetAttributeValue<EntityReference>("do_countryid");
            //Область
            if (addressDelivery.Contains("do_regionid"))
                createSalesOrder["do_regionsalesid"] = addressDelivery.GetAttributeValue<EntityReference>("do_regionid");
            //Почтовый индекс
            if (addressDelivery.Contains("do_postalcode"))
                createSalesOrder["shipto_postalcode"] = addressDelivery.GetAttributeValue<string>("do_postalcode");
            //Город
            if (addressDelivery.Contains("do_cityid"))
                createSalesOrder["do_citysalesorderid"] = addressDelivery.GetAttributeValue<EntityReference>("do_cityid");
            //Улица
            if (addressDelivery.Contains("do_streetid"))
                createSalesOrder["do_streetid"] = addressDelivery.GetAttributeValue<EntityReference>("do_streetid");
            //Дом
            if (addressDelivery.Contains("do_house"))
                createSalesOrder["shipto_line1"] = addressDelivery.GetAttributeValue<string>("do_house");

            //Примечание к заказу
            createSalesOrder["description"] = shipmentAccount.GetAttributeValue<string>("do_noteorder") + "\n" + salesorder.NoteOrder;

            //Прайс-лист (СИСТЕМНОЕ ПО УМОЛЧАНИЮ!)
            createSalesOrder["pricelevelid"] = new EntityReference("pricelevel", new Guid("1B28A6A5-3630-DF11-88B1-001517817114"));

            //Валюта (СИСТЕМНОЕ ПО УМОЛЧАНИЮ!)
            createSalesOrder["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("7194A8FA-BE1B-DE11-9BD7-000E2EDB432F"));

            try
            {
                var salesOrderExist2 = _salesOrderRepository.GetByExternalCode(salesorder.ExternalCode, (Guid)salesorder.ShipmentAccountId);
                if (salesOrderExist2.Count > 0)
                {
                    messages.Add(OperationMessage.Error($"Заказ с внешним номером заказа (ExternalCode) <{salesorder.ExternalCode}> уже существует. Обратитесь к менеджеру."));
                    return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
                }

                //новый заказ
                var newSalesOrderId = _salesOrderRepository.Create(createSalesOrder);
                var code = _salesOrderRepository.Get(newSalesOrderId, new ColumnSet("do_code")).GetAttributeValue<string>("do_code");
                //Создание продуктов
                CreateSalesOrderDetailList(salesorder.Products, newSalesOrderId);

                //отправка заказа в НАВ
                var resultSendToNav = SendToNavision(new SendToNavModel() { SalesOrderId = newSalesOrderId });
                if (resultSendToNav.Level == "SUCCESS")
                {
                    //уведомление менеджера о создании заказа
                    var ownerEmail = su.GetAttributeValue<string>("internalemailaddress");
                    var salesOrderUrl = $"<a href='{serverUrl}/main.aspx?etn=salesorder&id={newSalesOrderId.ToString()}&pagetype=entityrecord'>{code}</a>";

                    var mail = new MailSenderModel()
                    {
                        Subject = $"API CRM - Заказ {shipmentAccount.GetAttributeValue<string>("name")}",
                        Description = $"По клиенту {shipmentAccount.GetAttributeValue<string>("name")} создан и размещен заказ в NAV {salesOrderUrl}. ",
                        To = new List<string>() { ownerEmail }
                    };
                    _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                    messages.Add(OperationMessage.Success($"Заказ № {code} успешно создан и размещен в NAV"));
                }
                else if (resultSendToNav.Level == "ERROR")
                {
                    var salesOrderUrl = $"<a href='{serverUrl}/main.aspx?etn=salesorder&id={newSalesOrderId.ToString()}&pagetype=entityrecord'>{code}</a>";
                    var mail = new MailSenderModel()
                    {
                        Subject = $"API CRM - Ошибка отправки заказа в НАВ",
                        Description = $"По клиенту {shipmentAccount.GetAttributeValue<string>("name")} не размещен заказ в NAV {salesOrderUrl}. Error: {resultSendToNav.Message}",
                        To = new List<string>() { ConfigurationManager.AppSettings["emailForError"] }
                    };
                    _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                    messages.Add(OperationMessage.Error($"Заказ № {code} не размещен в NAV"));
                }
                var answer = new AnswerSalesOrderCreateModel()
                {
                    SalesOrderCode = code,
                    DeliveryDate = salesorder.DeliveryDate,
                    DeliveryAddress = addressDelivery.GetAttributeValue<string>("do_fulladdress"),
                    Sum = _salesOrderRepository.Get(newSalesOrderId, new ColumnSet("do_sum")).GetAttributeValue<double>("do_sum")
                };
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, answer);
            }
            catch (Exception ex)
            {
                //уведомление менеджера и прогера о не создании заказа
                var ownerEmail = su.GetAttributeValue<string>("internalemailaddress");
                var mail = new MailSenderModel()
                {
                    To = new List<string>() { ownerEmail, ConfigurationManager.AppSettings["emailForError"] },
                    Subject = $"API CRM - Ошибка при создании заказа по клиенту {shipmentAccount.GetAttributeValue<string>("name")}",
                    Description = $"Внешний номер заказа: {salesorder.ExternalCode} <br/> Ошибка : {ex.Message}"
                };
                _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                messages.Add(OperationMessage.Error($"Во время создания заказа произошла ошибка. Обратитесь к менеджеру."));
                return OperationResult<AnswerSalesOrderCreateModel>.Create(messages, null);
            }
        }

        public void CreateSalesOrderDetailList(List<CreateSalesOrderDetailModel> products, Guid salesorderId)
        {
            foreach (var product in products)
            {
                var createDetail = new Entity();
                var crmProduct = _productRepository.GetSingleByField("productnumber", product.ProductCode);
                createDetail["productid"] = new EntityReference("product", crmProduct.Id);
                //Заказ (Поле для лукапа)
                createDetail["do_salesorderid"] = new EntityReference("salesorder", salesorderId);
                createDetail["salesorderid"] = new EntityReference("salesorder", salesorderId);
                //Единица измерения (СИСТЕМНОЕ ПО УМОЛЧАНИЮ!)
                createDetail["uomid"] = new EntityReference("uom", new Guid("94cabfe7-e653-418f-831c-2ad2e8d3de96"));
                //Кол-во
                createDetail["quantity"] = product.Quantity;
                //Единица измерения               
                createDetail["do_itemuom_salesorderdetailid"] = new EntityReference("do_itemuom", (Guid)product.ItemUomId);
                //цена будет проставлена из плагина на создание продукта заказа
                _salesOrderDetailRepository.Create(createDetail);
            }
        }

        private OperationMessage ValidationProduct(CreateSalesOrderDetailModel product)
        {
            if (product.ProductCode == null || product.ProductCode == "")
            {
                return OperationMessage.Error($"Не указан код продукта.");
            }
            var crmProduct = _productRepository.GetSingleByField("productnumber", product.ProductCode);
            if (crmProduct == null)
            {
                return OperationMessage.Error($"Продукт c кодом {product.ProductCode} не найден в каталоге продуктов.");
            }
            //Кол-во
            if (product.Quantity == 0 || product.Quantity == null)
            {
                return OperationMessage.Error($"Необходимо указать количество продукта {product.ProductCode}.");
            }
            //Единица измерения
            if (product.ItemUomId == null)
            {
                return OperationMessage.Error($"Необходимо указать ед. измерения для продукта {product.ProductCode}.");
            }
            var itemUom = _itemUomRepository.Get((Guid)product.ItemUomId, crmProduct.Id);
            if (itemUom == null)
            {
                return OperationMessage.Error($"Ед. измерения {product.ItemUomId} нет у продукта {product.ProductCode}.");
            }
            var salesLock = crmProduct.Contains("do_salelock") ? crmProduct.GetAttributeValue<bool>("do_salelock") : false;
            if (salesLock)
            {
                return OperationMessage.Warning($"Продукт {product.ProductCode} заблокирован для продажи.");
            }
            return OperationMessage.Success();
        }

        private bool CheckIsWeekendDay(DateTime date)
        {
            var isWorkDayEntity = _workDayRepository.GetByDate(date.Date, new ColumnSet("do_nonworking"));
            if (isWorkDayEntity != null)
            {
                if (isWorkDayEntity.Contains("do_nonworking"))
                {
                    return isWorkDayEntity.GetAttributeValue<bool>("do_nonworking");
                }
                else return false;
            }
            else return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }

        private bool ValidateAccount(Entity account)
        {
            var fields = new List<string>();
            if (!account.Contains("do_code") || account.GetAttributeValue<string>("do_code") == "")
            {
                fields.Add("Код клиента");
            }
            if (!account.Contains("primarycontactid"))
            {
                fields.Add("Основной контакт");
            }
            if (!account.Contains("do_inn"))
            {
                fields.Add("ИНН");
            }
            if (!account.Contains("do_accountdeliveryaddressid"))
            {
                fields.Add("Основной адрес доставки");
            }
            if (fields.Count > 0)
            {
                var manager = _systemUserRepository.Get(account.GetAttributeValue<EntityReference>("ownerid").Id);
                var mail = new MailSenderModel()
                {
                    Subject = "API CRM - Ошибка создания заказа",
                    Description = String.Format($"В карточке клиента {account.GetAttributeValue<string>("name")} не заполнены поля : {string.Join("&nbsp;&nbsp;&nbsp;", fields)}."),
                    To = new List<string>() { manager.GetAttributeValue<string>("internalemailaddress"), ConfigurationManager.AppSettings["emailForError"] }
                };
                _executeActionRepository.Execute("do_MailSenderAction", "SendMessage", JsonConvert.SerializeObject(mail));
                return false;
            }
            return true;
        }

        private OperationMessage SendToNavision(SendToNavModel model)
        {
            try
            {
                var response = _executeActionRepository.Execute("do_CrmNavSyncAction", "ExportSalesOrder", JsonConvert.SerializeObject(model)).Results;
                var result = JsonConvert.DeserializeObject<OperationResultSendToNav<string>>(response["OutJsonObject"].ToString());
                if (result.OperationLevel == OperationLevel.Success)
                {
                    return OperationMessage.Success("Заказ успешно создан и размещен в NAV.");
                }
                else if (result.OperationLevel == OperationLevel.Warning)
                {
                    return OperationMessage.Warning(result.Message);
                }
                else if (result.OperationLevel == OperationLevel.Error)
                {
                    return OperationMessage.Error(result.Message);
                }
            }
            catch (Exception ex)
            {
                return OperationMessage.Error($"Ошибка размещения заказа в NAV : {ex.Message}");
            }

            return OperationMessage.Success("Заказ успешно создан и размещен в NAV.");
        }
    }
}

