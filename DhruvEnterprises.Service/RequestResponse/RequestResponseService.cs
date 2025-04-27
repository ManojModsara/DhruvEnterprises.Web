using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using DhruvEnterprises.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DhruvEnterprises.Service
{
    public class RequestResponseService: IRequestResponseService
    {

        #region "Fields"
        private IRepository<User> repoUser; 
        private IRepository<ApiUrl> repoApiUrl;
        private IRepository<Recharge> repoRecharge; 
        private IRepository<RequestResponse> repoRequestResponse;
        #endregion
         
        #region "Cosntructor"
        public RequestResponseService(IRepository<User> _repoUser, IRepository<ApiUrl> _repoApiUrl, IRepository<Recharge> _repoRecharge, IRepository<RequestResponse> _repoRequestResponse)
        {
            this.repoUser = _repoUser;
            this.repoApiUrl = _repoApiUrl;
            this.repoRecharge = _repoRecharge;
            this.repoRequestResponse = _repoRequestResponse;

          

        }
        #endregion

        #region "Methods"

        public ICollection<RequestResponse> GetRequestResponse(ReqResFilterDto ft)
        {

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(ft.Sdate) ? ft.Sdate : ft.SdateNow, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(ft.Edate) ? ft.Edate : ft.EdateNow, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            tdate = !string.IsNullOrEmpty(ft.Edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            return repoRequestResponse.Query()
                .Filter
                (x => (ft.RecId == 0 ? true : x.RecId == ft.RecId) &&
                      (ft.RefId == "" ? true : x.RefId == ft.RefId.Trim()) &&
                      (ft.Remark == "" ? true : x.Remark == ft.Remark.Trim())&&
                      (ft.CustomerNo == "" ? true : x.CustomerNo == ft.CustomerNo.Trim())&&
                      (ft.UserTxnId == "" ? true : x.UserTxnId == ft.UserTxnId.Trim()) &&
                       (ft.UserId == 0 ? true : x.UserId == ft.UserId) &&
                        (ft.ApiId == 0 ? true : (x.ApiUrl != null && x.ApiUrl.ApiSource.Id == ft.ApiId)) &&
                         (ft.OpId == 0 ? true : (x.Recharge != null && x.Recharge.OpId == ft.OpId)) &&
                          (ft.StatusId == 0 ? true : (x.Recharge != null && x.Recharge.StatusId == ft.StatusId)) &&
                           (ft.Sdate == "" ? true : x.AddedDate >= fdate) &&
                            (ft.Edate == "" ? true : x.AddedDate <= tdate) 


                ).AsTracking() .Get().ToList();
        }

        public RequestResponse GetRequestResponse(int id)
        {
            return repoRequestResponse.FindById(id);
        }
        
        /// <summary>
        /// Get all RequestResponses by DataTable SearchModel
        /// </summary>
        /// <param name="searchModel" type="DataTableServerSide"></param>
        /// <returns></returns>
        public KeyValuePair<int, List<RequestResponse>> GetRequestResponses(DataTableServerSide searchModel, ReqResFilterDto ft)
        {
            var fdata = searchModel.filterdata;
            fdata.FromDateNow = !string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.ToString("dd/MM/yyy");
            fdata.ToDateNow = !string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyy");            
            var predicate = CustomPredicate.BuildPredicate<RequestResponse>(searchModel, new Type[] { typeof(RequestResponse) });
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.ToString("dd/MM/yyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            predicate = fdata.RecId == 0 ? predicate : predicate.And(x =>x.RecId == fdata.RecId);
            predicate = string.IsNullOrEmpty(fdata.RefId) ? predicate: predicate.And(x =>  x.RefId == fdata.RefId.Trim());
            predicate = string.IsNullOrEmpty(fdata.Remark) ? predicate : predicate.And(x => x.Remark == fdata.Remark.Trim());
            predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.CustomerNo== fdata.CustomerNo.Trim());
            predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId== fdata.UserTxnId.Trim());
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiUrl != null).And(x => x.ApiUrl.ApiSource.Id == fdata.ApiId);
            predicate = (fdata.OpId == 0 && fdata.StatusId == 0) ? predicate : predicate.And(x => x.Recharge != null);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.Recharge.OpId == fdata.OpId);
            predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.Recharge.StatusId == fdata.StatusId);

            predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x =>  x.AddedDate >= fdate);
            predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.AddedDate <= tdate);
            
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<RequestResponse> results = repoRequestResponse
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderByDescending(x=>x.Id))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<RequestResponse>> resultResponse = new KeyValuePair<int, List<RequestResponse>>(totalCount, results);

            return resultResponse;
        }

        /// <summary>
        /// Save/update RequestResponse 
        /// </summary>
        /// <param name="RequestResponse" type="RequestResponse"></param>
        /// <returns></returns>
        /// 


        public KeyValuePair<int, List<RequestResponse>> GetRequestResponses1(DataTableServerSide searchModel, ReqResFilterDto ft)
        {
            var fdata = searchModel.filterdata;
            //fdata.FromDateNow = !string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.ToString("dd/MM/yyy");
            //fdata.ToDateNow = !string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyy");
            var predicate = CustomPredicate.BuildPredicate<RequestResponse>(searchModel, new Type[] { typeof(RequestResponse) });
            //DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.ToString("dd/MM/yyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
           // DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
           // tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            predicate =  predicate.And(x => x.RecId == ft.RecId);
            predicate = string.IsNullOrEmpty(ft.RefId) ? predicate : predicate.And(x => x.RefId == ft.RefId.Trim());
            //predicate = string.IsNullOrEmpty(fdata.Remark) ? predicate : predicate.And(x => x.Remark == fdata.Remark.Trim());
            //predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.CustomerNo == fdata.CustomerNo.Trim());
            //predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId == fdata.UserTxnId.Trim());
            //predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            //predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiUrl != null).And(x => x.ApiUrl.ApiSource.Id == fdata.ApiId);
            //predicate = (fdata.OpId == 0 && fdata.StatusId == 0) ? predicate : predicate.And(x => x.Recharge != null);
            //predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.Recharge.OpId == fdata.OpId);
            //predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.Recharge.StatusId == fdata.StatusId);
            //predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.AddedDate >= fdate);
            //predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.AddedDate <= tdate);
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
              List<RequestResponse> results = repoRequestResponse
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderByDescending(x => x.Id))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();
            KeyValuePair<int, List<RequestResponse>> resultResponse = new KeyValuePair<int, List<RequestResponse>>(totalCount, results);

            return resultResponse;
        }

        public RequestResponse Save(RequestResponse RequestResponse)
        {
           
            if (RequestResponse.Id == 0)
            {
                RequestResponse.AddedDate = DateTime.Now;
                repoRequestResponse.Insert(RequestResponse);
            }
            else
            {
                RequestResponse.UpdatedDate = DateTime.Now;
                repoRequestResponse.Update(RequestResponse);
            }
            return RequestResponse;
        }

        public List<RequestResponse> GetRequestResponse(long recid)
        {          
            return repoRequestResponse.Query()
                .Filter
                (x => (x.RecId == recid) 
                ).AsTracking().Get().ToList();
        }
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoUser != null)
            {
                repoUser.Dispose();
                repoUser = null;
            }
            if (repoApiUrl != null)
            {
                repoApiUrl.Dispose();
                repoApiUrl = null;
            }
            if (repoRecharge != null)
            {
                repoRecharge.Dispose();
                repoRecharge = null;
            }
            if (repoRequestResponse != null)
            {
                repoRequestResponse.Dispose();
                repoRequestResponse = null;
            }
        }
        #endregion

    }
}
