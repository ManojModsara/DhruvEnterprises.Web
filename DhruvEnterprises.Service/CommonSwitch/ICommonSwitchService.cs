using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
namespace DhruvEnterprises.Service
{
   public interface ICommonSwitchService: IDisposable
    {
        ICollection<CommanRouting> GetCommanRoutings();
        KeyValuePair<int, List<CommanRouting>> GetCommanRouting(DataTableServerSide searchModel, int apiId = 0);
        void Save(List<CommanRouting> cList);
        CommanRouting Save(CommanRouting cRoute);
        ICollection<Operator> GetOperatorList();
        ICollection<Circle> GetCircleList();
        ICollection<FilterType> GetFilterTypeList();
        bool Delete(int Id);
        CommanRouting GetCommanRouting(int id);

        ICollection<BlockRoute> GetBlockRoutes();
        void Save(BlockRoute blockRoute);
        bool DeleteBlockRoute(int Id);
        BlockRoute GetBlockRouteById(int id);
        
        ICollection<StopRouteMessage> GetStopRouteMessages();
        void Save(StopRouteMessage StopRouteMessage);
        bool DeleteStopRouteMessage(int Id);
        StopRouteMessage GetStopRouteMessageById(int id);

        ICollection<UserFilterRule> GetUserFilterRules();
        void Save(UserFilterRule userFilterRule);
        bool DeleteUserFilterRule(int Id);
        UserFilterRule GetUserFilterRuleById(int id);
        KeyValuePair<int, List<UserFilterRuleReport>> GetUserFilterRuleReport(DataTableServerSide searchModel);
    }
}
