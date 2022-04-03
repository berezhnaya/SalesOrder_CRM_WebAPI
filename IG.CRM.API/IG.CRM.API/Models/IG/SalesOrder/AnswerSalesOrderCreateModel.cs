using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models
{
    public class AnswerSalesOrderCreateModel
    {
        public string SalesOrderCode { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public double Sum { get; set; }
    }
}