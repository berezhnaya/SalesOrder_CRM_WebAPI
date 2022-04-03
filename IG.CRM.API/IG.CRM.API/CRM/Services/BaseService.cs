using IG.CRM.API.CRM.Repositories.API;
using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace IG.CRM.API.CRM.Services
{
    public class BaseService
    {
        protected readonly ICustomerRepository _customerRepository;
        protected readonly IFunctionRepository _functionRepository;       
        
        public BaseService(ICustomerRepository customerRepository, IFunctionRepository functionRepository)
        {
            _customerRepository = customerRepository;
            _functionRepository = functionRepository;           
        }

        public OperationResult<string> RoleValidation(string token, string function, string controller)
        {
            if (token == null || token == "")
            {
                return OperationResult<string>.Create(OperationMessage.Error("Headers запроса должен содержать token"), "");
            }
            var customer = _customerRepository.GetByToken(token);
            if (customer == null)
            {
                return OperationResult<string>.Create(OperationMessage.Error($"Не найден клиент с текущим TOKEN ключём : {token}"), "");
            }
            if(!_functionRepository.IsExistForCustomer(customer.Id, function, controller, new ColumnSet(false)))
            {
                return OperationResult<string>.Create(OperationMessage.Error($"У вас нет доступа к функции '{function}' контроллера '{controller}'"), "");
            }
            return OperationResult<string>.Create(OperationMessage.Success(), "");
        }

        public OperationResult<string> RoleValidation(string token, Guid accountid, string function, string controller)
        {
            if (token == null)
            {
                return OperationResult<string>.Create(OperationMessage.Error("Headers запроса должен содержать token"), "");
            }
            var customer = _customerRepository.GetByToken(token);
            if (customer == null)
            {
                return OperationResult<string>.Create(OperationMessage.Error($"Не найден клиент с текущим TOKEN ключём : {token}"), "");
            }
            if (!_functionRepository.IsExistForCustomer(customer.Id, function, controller, new ColumnSet(false)))
            {
                return OperationResult<string>.Create(OperationMessage.Error($"У вас нет доступа к функции '{function}' контроллера '{controller}'"), "");
            }
          
            if(customer.GetAttributeValue<EntityReference>("do_accountid").Id != accountid)
            {
                return OperationResult<string>.Create(OperationMessage.Error("У вас нет доступа к этому клиенту"), "");
            }

            return OperationResult<string>.Create(OperationMessage.Success(), "");
        }
    }
}