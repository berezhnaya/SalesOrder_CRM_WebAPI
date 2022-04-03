using System;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IUomRepository : IBaseRepository
    {
        Guid Get(string code);
    }
}
