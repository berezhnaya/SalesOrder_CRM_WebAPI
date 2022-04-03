using System.Collections.Generic;

namespace IG.CRM.API.Models
{
    public class OperationResult<T>
    {
        public List<OperationMessage> Messages { get; set; }
        public T Data { get; set; }
        public bool Success { get; set; }

        private OperationResult() { }

        public static OperationResult<T> Create(List<OperationMessage> messages, T data)
        {
            return new OperationResult<T>()
            {
                Messages = messages,
                Data = data,
                Success = !messages.Exists(x => x.Level == "ERROR")
            };
        }

        public static OperationResult<T> Create(OperationMessage message, T data)
        {
            return new OperationResult<T>()
            {
                Messages = !string.IsNullOrEmpty(message.Message) ? new List<OperationMessage>() { message } : new List<OperationMessage>() { },
                Data = data,
                Success = message.Level != "ERROR"
            };
        }
    }

    public class OperationMessage
    {
        public string Message { get; set; }
        public string Level { get; set; }
        private OperationMessage() { }

        public static OperationMessage Success()
        {
            return new OperationMessage()
            {
                Message = "",
                Level = "SUCCESS"
            };
        }

        public static OperationMessage Success(string message)
        {
            return new OperationMessage()
            {
                Message = message,
                Level = "SUCCESS"
            };
        }

        public static OperationMessage Warning(string message)
        {
            return new OperationMessage()
            {
                Message = message,
                Level = "WARNING"
            };
        }

        public static OperationMessage Error(string message)
        {
            return new OperationMessage()
            {
                Message = message,
                Level = "ERROR"
            };
        }
    }
}