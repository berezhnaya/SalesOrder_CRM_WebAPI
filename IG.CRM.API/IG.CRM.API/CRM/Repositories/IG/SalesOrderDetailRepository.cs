using Microsoft.Xrm.Sdk;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class SalesOrderDetailRepository : BaseRepository, ISalesOrderDetailRepository
    {
        public SalesOrderDetailRepository(IOrganizationService service) : base(service, "salesorderdetail") { }
    }
}