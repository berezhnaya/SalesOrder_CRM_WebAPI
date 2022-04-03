using IG.CRM.API.CRM.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IMailSenderRepository
    {
        OrganizationResponse SendMail(MailSenderModel mail);
    }
}