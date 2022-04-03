using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.Models.IG.Action
{
    public class OperationResultSendToNav<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public OperationLevel OperationLevel { get; set; }
    }

    public enum OperationLevel
    {
        Success = 0,
        Warning = 1,
        Error = 2
    }
}