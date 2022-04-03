using Microsoft.Xrm.Sdk;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class ExecuteActionRepository: BaseRepository, IExecuteActionRepository
    {
        public  ExecuteActionRepository(IOrganizationService service) : base(service) { }

        public OrganizationResponse Execute(string action, string method, string jsonObject)
        {
            var request = new OrganizationRequest(action);
            request["MethodName"] = method;
            request["JsonObject"] = jsonObject;
            return _service.Execute(request);
        }        
    }
}