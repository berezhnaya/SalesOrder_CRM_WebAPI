using System.Collections.Generic;
using System.Configuration;

namespace IG.CRM.API.CRM.Models
{
    public class MailSenderModel
    {
        public List<string> To { get; set; } = new List<string> { ConfigurationManager.AppSettings["emailForError"] };
        public string Subject { get; set; }
        public string Description { get; set; }
    }
}