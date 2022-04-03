using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models
{
    public class SendToNavModel
    {
        public Guid SalesOrderId { get; set; }
        public bool TryAsyncIsError { get; set; } = true;
        public bool IsSendError { get; set; } = true;
    }
}