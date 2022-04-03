using Microsoft.Xrm.Sdk;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class FirmRepository : BaseRepository, IFirmRepository
    {
        public FirmRepository(IOrganizationService service) : base(service, "do_firms") { }
    }
}