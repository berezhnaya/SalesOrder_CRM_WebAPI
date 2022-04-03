using Microsoft.Xrm.Sdk;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class SystemUserRepository : BaseRepository, ISystemUserRepository
    {
        public SystemUserRepository(IOrganizationService service) : base(service, "systemuser") { }
    }
}