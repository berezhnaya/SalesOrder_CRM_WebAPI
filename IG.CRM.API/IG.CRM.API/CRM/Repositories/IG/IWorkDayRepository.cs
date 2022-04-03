using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace IG.CRM.API.CRM.Repositories.IG
{
    public interface IWorkDayRepository: IBaseRepository
    {
        Entity GetByDate(DateTime date, ColumnSet columnSet = null);
    }
}
