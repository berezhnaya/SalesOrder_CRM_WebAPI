using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public class AttributeRepository : BaseRepository, IAttributeRepository
    {
        public AttributeRepository(IOrganizationService service) : base(service) { }

        public string RetrieveAttributeDisplayName(string EntitySchemaName, string AttributeSchemaName)
        {
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = EntitySchemaName,
                LogicalName = AttributeSchemaName,
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)_service.Execute(retrieveAttributeRequest);
            AttributeMetadata retrievedAttributeMetadata = (AttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
            return retrievedAttributeMetadata.DisplayName.UserLocalizedLabel.Label;
        }
    }
}