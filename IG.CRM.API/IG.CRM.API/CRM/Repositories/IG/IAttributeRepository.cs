namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IAttributeRepository : IBaseRepository
    {
        string RetrieveAttributeDisplayName(string EntitySchemaName, string AttributeSchemaName);
    }
}