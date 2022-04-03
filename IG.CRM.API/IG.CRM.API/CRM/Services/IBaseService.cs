using IG.CRM.API.Models;

namespace IG.CRM.API.CRM.Services
{
    public interface IBaseService
    {
        OperationResult<string> RoleValidation(string token, string entityName, string methodName);
    }
}
