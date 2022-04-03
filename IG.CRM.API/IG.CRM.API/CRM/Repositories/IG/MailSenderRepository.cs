using IG.CRM.API.CRM.Models;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class MailSenderRepository: BaseRepository, IMailSenderRepository
    {
        public  MailSenderRepository(IOrganizationService service) : base(service) { }

        public OrganizationResponse SendMail(MailSenderModel mail)
        {
            var request = new OrganizationRequest("do_MailSenderAction");
            request["MethodName"] = "SendMessage";
            request["JsonObject"] = JsonConvert.SerializeObject(mail);
            return _service.Execute(request);
        }
    }
}