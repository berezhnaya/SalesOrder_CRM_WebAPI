using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models
{
    public class BaseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}