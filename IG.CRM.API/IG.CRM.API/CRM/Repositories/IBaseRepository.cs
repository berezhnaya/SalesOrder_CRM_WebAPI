using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace IG.CRM.API.CRM.Repositories
{
    public interface IBaseRepository
    {
        Entity Get(Guid id, ColumnSet columnSet = null);
        List<Entity> GetAll(bool isNolock = false, ColumnSet columnSet = null);
        List<Entity> GetAllByFetch(string fetch);
        Entity GetSingleByFetch(string fetch);     
        DataCollection<Entity> GetByField(string field, object value, ColumnSet columnSet = null);
        Guid Create(Entity entity);
        void Update(Entity entity);
        Entity GetSingleByField(string field, object value, ColumnSet columnSet = null);
        OrganizationResponse SetState(Guid id, int state, int status);
    }
}