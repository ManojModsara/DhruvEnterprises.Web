using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
 public class CommonSwitchService: ICommonSwitchService
    {
        #region "Fields"
        private IRepository<CommanRouting> repoCommanRouting;
        private IRepository<Operator> repoOperator;
        private IRepository<Circle> repoCircle;
        private IRepository<FilterType> repoFilterType;
        private IRepository<BlockRoute> repoBlockRoute;
        private IRepository<StopRouteMessage> repoStopRouteMessage;
        private IRepository<UserFilterRule> repoUserFilterRule;
        private IRepository<UserFilterRuleReport> repoUserFilterRuleReport;
        #endregion 

        #region "Cosntructor"
        public CommonSwitchService(
            IRepository<CommanRouting> _repoCommanRouting,
            IRepository<Operator> _repoOperator,
            IRepository<Circle> _repoCircle ,
            IRepository<FilterType> _repoFilterType,
            IRepository<BlockRoute> _repoBlockRoute,
            IRepository<StopRouteMessage> _repoStopRouteMessage,
            IRepository<UserFilterRule> _repoUserFilterRule,
            IRepository<UserFilterRuleReport> _repoUserFilterRuleReport
            )
        {
            this.repoCommanRouting = _repoCommanRouting;
            this.repoOperator = _repoOperator;
            this.repoCircle = _repoCircle;
            this.repoFilterType = _repoFilterType;
            this.repoBlockRoute = _repoBlockRoute;
            this.repoStopRouteMessage = _repoStopRouteMessage;
            this.repoUserFilterRule = _repoUserFilterRule;
            this.repoUserFilterRuleReport = _repoUserFilterRuleReport;
        }
        #endregion

        #region "Methods"

        public ICollection<CommanRouting> GetCommanRoutings()
        {
            return repoCommanRouting.Query().Get().ToList();
        }
        
        public KeyValuePair<int, List<CommanRouting>> GetCommanRouting(DataTableServerSide searchModel, int apiId = 0)
        {
            var predicate = CustomPredicate.BuildPredicate<CommanRouting>(searchModel, new Type[] { typeof(ApiSource), typeof(Operator), typeof(Circle) });

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<CommanRouting> results = repoCommanRouting
                .Query()
                .Filter(x => (x.ApiId == apiId || apiId == 0))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(ApiSource), typeof(Operator), typeof(Circle) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<CommanRouting>> resultResponse = new KeyValuePair<int, List<CommanRouting>>(totalCount, results);

            return resultResponse;
        }

        public void Save(List<CommanRouting> cList)
        {
       
            //add entities
            foreach (var item in cList)
            {
                if (item.Id == 0)
                {
                    item.AddedDate = DateTime.Now;
                    repoCommanRouting.Insert(item);
                }
               
              
            }

            //var croutings = GetCommanRoutings();

            //foreach (var item in croutings) 
            //{
            //    if(!cList.Any(x=>x.Id>0 && x.Id == item.Id))
            //    {
            //        repoCommanRouting.Delete(item.Id);
            //    }
            //}
            
        }

        public CommanRouting Save(CommanRouting cRoute)
        {
            if (cRoute.Id > 0)
            {
                cRoute.UpdatedDate = DateTime.Now;
                repoCommanRouting.Update(cRoute);
            }
            else 
                {
                  cRoute.AddedDate = DateTime.Now;
                    repoCommanRouting.Insert(cRoute); 
                }
            return cRoute;
        }

        public ICollection<Operator> GetOperatorList() {
            return repoOperator.Query().Get().ToList();
        }

        public ICollection<Circle> GetCircleList()
        {
            return repoCircle.Query().Get().ToList();
        }

        public bool Delete(int Id)
        {
            repoCommanRouting.Delete(Id);
            return true;
        }

        public ICollection<FilterType> GetFilterTypeList()
        {
            return repoFilterType.Query().Get().ToList();
        }

        public CommanRouting GetCommanRouting(int id)
        {
            return repoCommanRouting.FindById(id);
        }

        public ICollection<BlockRoute> GetBlockRoutes()
        {
            return repoBlockRoute.Query().Get().ToList();
        }

        public BlockRoute GetBlockRouteById(int id)
        { 
            return repoBlockRoute.FindById(id);
        }

        public void Save(BlockRoute blockRoute)
        {
            if (blockRoute.Id > 0)
            {
                blockRoute.UpdatedDate = DateTime.Now;
                repoBlockRoute.Update(blockRoute);
            }
            else
            {
                blockRoute.AddedDate = DateTime.Now;
                repoBlockRoute.Insert(blockRoute);
            }


        }

        public bool DeleteBlockRoute(int Id)
        {
            repoBlockRoute.Delete(Id);
            return true;
        }

        
        public ICollection<StopRouteMessage> GetStopRouteMessages()
        {
            return repoStopRouteMessage.Query().Get().ToList();
        }

        public StopRouteMessage GetStopRouteMessageById(int id)
        {
            return repoStopRouteMessage.FindById(id);
        }

        public void Save(StopRouteMessage StopRouteMessage)
        {
            if (StopRouteMessage.Id > 0)
            {
                StopRouteMessage.UpdatedDate = DateTime.Now;
                repoStopRouteMessage.Update(StopRouteMessage);
            }
            else
            {
                StopRouteMessage.AddedDate = DateTime.Now;
                repoStopRouteMessage.Insert(StopRouteMessage);
            }


        }

        public bool DeleteStopRouteMessage(int Id)
        {
            repoStopRouteMessage.Delete(Id);
            return true;
        }
        
        public ICollection<UserFilterRule> GetUserFilterRules()
        {
            return repoUserFilterRule.Query().Get().ToList();
        }

        public UserFilterRule GetUserFilterRuleById(int id)
        {
            return repoUserFilterRule.FindById(id);
        }

        public void Save(UserFilterRule userFilterRule)
        {
            if (userFilterRule.Id > 0)
            {
                userFilterRule.UpdatedDate = DateTime.Now;
                repoUserFilterRule.Update(userFilterRule);
            }
            else
            {
                userFilterRule.AddedDate = DateTime.Now;
                repoUserFilterRule.Insert(userFilterRule);
            }


        }

        public bool DeleteUserFilterRule(int Id)
        {
            repoUserFilterRule.Delete(Id);
            return true;
        }


        public KeyValuePair<int, List<UserFilterRuleReport>> GetUserFilterRuleReport(DataTableServerSide searchModel)
        {
            var predicate = CustomPredicate.BuildPredicate<UserFilterRuleReport>(searchModel, new Type[] { typeof(UserFilterRuleReport), typeof(User), typeof(Circle), typeof(Operator), typeof(ApiSource) });
            
            var fdata = searchModel.filterdata;

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

           
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiId == fdata.ApiId);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.OpId == fdata.OpId);
            predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.CircleId == fdata.CircleId);
            predicate = fdata.Amount == 0 ? predicate : predicate.And(x => x.Amount == fdata.Amount);
            
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<UserFilterRuleReport> results = repoUserFilterRuleReport
                .Query()
               .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(UserFilterRuleReport), typeof(User), typeof(Circle), typeof(Operator), typeof(ApiSource) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<UserFilterRuleReport>> resultResponse = new KeyValuePair<int, List<UserFilterRuleReport>>(totalCount, results);

            return resultResponse;
        }


        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoCommanRouting != null)
            {
                repoCommanRouting.Dispose();
                repoCommanRouting = null;
            }

            if (repoOperator != null)
            {
                repoOperator.Dispose();
                repoOperator = null;
            }

            if (repoCircle != null)
            {
                repoCircle.Dispose();
                repoCircle = null;
            }

            if (repoFilterType != null)
            {
                repoFilterType.Dispose();
                repoFilterType = null;
            }
        }
        #endregion
    }
}
