namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IConfigurationRepository : IBaseRepository
    {
        string GetValueByName(string name);
    }
}
