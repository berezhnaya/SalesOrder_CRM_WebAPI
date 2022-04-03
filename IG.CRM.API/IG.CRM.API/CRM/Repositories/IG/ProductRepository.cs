using Microsoft.Xrm.Sdk;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(IOrganizationService service) : base(service, "product") { }
    }
}