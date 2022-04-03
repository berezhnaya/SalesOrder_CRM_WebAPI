using IG.CRM.API.CRM.Services.IG;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace IG.CRM.API.Controllers
{
    public class AccountController : ApiController
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [Route("api/account/get")]
        [HttpGet]
        public OperationResult<AccountModel> Get()
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _accountService.Get(token, "get", "account");
        }

        [Route("api/account/getbyphone")]
        [HttpGet]
        public OperationResult<List<AccountModel>> GetByPhone(string phone)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _accountService.GetByPhone(phone, token, "getbyphone", "account");
        }

        [Route("api/account/getdeliveryaddresses")]
        [HttpGet]
        public OperationResult<List<BaseModel>> GetDeliveryAddresses(Guid accountId)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _accountService.GetDeliveryAddresses(accountId, token, "getdeliveryaddresses", "account");
        }
    }
}
