using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;

namespace DhruvEnterprises.Service
{
    public interface IActivityLogService
    {
        ICollection<ActivityLog> GetActivityLog(ActiVityLogFilterDto ftr);
        ActivityLog GetActivityLog(long id);
        KeyValuePair<int, List<ActivityLog>> GetActivityLogs(DataTableServerSide searchModel, ActiVityLogFilterDto ftr);
        ActivityLog Save(ActivityLog activityLog);
    }
}
