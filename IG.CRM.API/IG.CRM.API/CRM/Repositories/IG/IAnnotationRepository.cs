using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IAnnotationRepository : IBaseRepository
    {
        Entity GetLastFileByObjectId(Guid objectId, ColumnSet columnSet = null);
        DataCollection<Entity> GetFilesByObjectId(Guid objectId, ColumnSet columnSet = null);
    }
}