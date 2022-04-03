using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Models
{
    public class MailSenderModel
    {
        public List<string> To { get; set; } = new List<string> { "berezhnaya@ig.ua"/*, "erofeev@ig.ua", "bevzenko@.ig.ua", "pomazan@ig.ua" */};
        public string Subject { get; set; }
        public string Description { get; set; }
    }
}