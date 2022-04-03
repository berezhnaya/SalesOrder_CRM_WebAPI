using IG.CRM.API.CRM.Repositories.IG;
using IG.CRM.API.Helpers;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security;

namespace IG.CRM.API.CRM.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        protected readonly IOrganizationService _service;
        protected readonly string _entityName;
        protected SqlConnection _sqlConnection;

        public BaseRepository(IOrganizationService service, string entityName)
        {
            _service = service;
            _entityName = entityName;
            _sqlConnection = CreateSqlConnection();
        }

        public BaseRepository(IOrganizationService service)
        {
            _service = service;
            _sqlConnection = CreateSqlConnection();
        }

        protected SqlConnection CreateSqlConnection()
        {
            return new SqlConnection(SqlHelper.GetSqlCrmString());
        }

        public void Dispose()
        {
            _sqlConnection?.Dispose();
        }

        public Entity Get(Guid id, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            return _service.Retrieve(_entityName, id, columnSet);
        }

        public List<Entity> GetAll(bool isNolock = false, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var pageNumber = 1;
            var pageInfo = new PagingInfo()
            {
                Count = 5000,
                PageNumber = pageNumber
            };
            var query = new QueryExpression(_entityName)
            {
                ColumnSet = columnSet,
                NoLock = isNolock,
                PageInfo = pageInfo
            };
            var allEntities = new List<Entity>();
            var isMoreRecords = false;
            do
            {
                var result = _service.RetrieveMultiple(query);
                allEntities.AddRange(result.Entities);
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;
                isMoreRecords = result.MoreRecords;
            }
            while (isMoreRecords);
            return allEntities;
        }

        public List<Entity> GetAllByFetch(string fetch)
        {
            var moreRecords = false;
            int page = 1;
            var cookie = "";

            var entities = new List<Entity>();
            do
            {
                var xml = fetch.Replace("<fetch", $"<fetch {cookie}");
                var collection = _service.RetrieveMultiple(new FetchExpression(xml));
                if (collection.Entities.Count > 0)
                {
                    entities.AddRange(collection.Entities);
                }
                moreRecords = collection.MoreRecords;

                if (moreRecords)
                {
                    page++;
                    cookie = $"page='{page}' paging-cookie='{SecurityElement.Escape(collection.PagingCookie)}'";
                }
            } while (moreRecords);

            return entities;
        }

        public Entity GetSingleByFetch(string fetch)
        {
            return _service.RetrieveMultiple(new FetchExpression(fetch)).Entities.SingleOrDefault();
        }

        public DataCollection<Entity> GetByField(string field, object value, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var queryByAttribute = new QueryByAttribute(_entityName)
            {
                ColumnSet = columnSet,
                Attributes = { field },
                Values = { value },
            };
            return _service.RetrieveMultiple(queryByAttribute).Entities;
        }

        public Guid Create(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.LogicalName))
            {
                entity.LogicalName = _entityName;
            }
            return _service.Create(entity);
        }

        public void Update(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.LogicalName))
            {
                entity.LogicalName = _entityName;
            }
            _service.Update(entity);
        }

        public Entity GetSingleByField(string field, object value, ColumnSet columnSet = null)
        {
            columnSet = columnSet ?? new ColumnSet(true);
            var queryByAttribute = new QueryByAttribute(_entityName)
            {
                ColumnSet = columnSet,
                Attributes = { field },
                Values = { value }
            };
            return _service.RetrieveMultiple(queryByAttribute).Entities.Single();
        }

        public OrganizationResponse SetState(Guid id, int state, int status)
        {
            var request = new SetStateRequest()
            {
                State = new OptionSetValue(state),
                Status = new OptionSetValue(status),
                EntityMoniker = new EntityReference(_entityName, id)
            };
            return _service.Execute(request);
        }
    }
}