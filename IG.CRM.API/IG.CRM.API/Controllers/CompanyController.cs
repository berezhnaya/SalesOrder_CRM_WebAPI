using IG.CRM.API.CRM.Services.IG;
using IG.CRM.API.Models;
using IG.CRM.API.Models.IG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace IG.CRM.API.Controllers
{
    public class CompanyController : ApiController
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [Route("api/company/getcontacts")]
        [HttpGet]
        public OperationResult<List<Contact>> GetContacts(Guid accountId)
        {
            var token = HttpContext.Current.Request.Headers["token"];
            return _companyService.GetContacts(accountId, token, "getcontacts", "company");
        }
    }
}
