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
    public class EzyTMPayService : IEzyTMPayService
    {
        #region "Fields"

        private IRepository<EzyTMPayRequest> repoEzyTMPayRequest;
        IRepository<StatusType> repoStatusType;
        IRepository<GatewayTransferType> repoTransferType;
       
        #endregion

        #region "Cosntructor"
        public EzyTMPayService( IRepository<GatewayTransferType> _repoTransferType, IRepository<StatusType> _repoStatusType, IRepository<EzyTMPayRequest> _repoEzyTMPayRequest)
        {
            this.repoEzyTMPayRequest = _repoEzyTMPayRequest;
            this.repoStatusType = _repoStatusType;
            this.repoTransferType = _repoTransferType;
        }
        #endregion

        #region "Methods"
        public EzyTMPayRequest Save(EzyTMPayRequest ezyTMPayRequest)
        {

            if (ezyTMPayRequest.Id == 0)
            {
                ezyTMPayRequest.AddedDate = DateTime.Now;
                repoEzyTMPayRequest.Insert(ezyTMPayRequest);
            }
            else
            {
                ezyTMPayRequest.UpdatedDate = DateTime.Now;
                repoEzyTMPayRequest.Update(ezyTMPayRequest);
            }
            return ezyTMPayRequest;
        }
        public EzyTMPayRequest GetTxnRecordById(int Id)
        {
            return repoEzyTMPayRequest.Query().AsTracking().Filter(x => x.Id == Id).Get().SingleOrDefault();
        }
        
        public KeyValuePair<int, List<EzyTMPayRequest>> GetGatewaytxnReport(DataTableServerSide searchModel, int userId=0)
        {
            var fdata = searchModel.filterdata;
            #region "Filter"
            fdata.ToDate = fdata.EndDate;
            var predicate = CustomPredicate.BuildPredicate<EzyTMPayRequest>(searchModel, new Type[] { typeof(EzyTMPayRequest) });
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            if(userId!=1)
            {
                predicate = predicate.And(x => x.UserId == userId);
            }
            predicate = fdata.tid == 0 ? predicate : predicate.And(x => x.Id == fdata.tid);
            predicate = fdata.StatusId==0?predicate: predicate.And(x => x.StatusId==fdata.StatusId);
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = fdata.Paymentmode == 0 ? predicate : predicate.And(x => x.PaymentMode == fdata.Paymentmode);
            predicate = string.IsNullOrEmpty(fdata.GtxnId) ? predicate : predicate.And(x => x.TxnId == fdata.GtxnId);
            predicate = string.IsNullOrEmpty(fdata.BankName) ? predicate : predicate.And(x => x.BankName == fdata.BankName);
            predicate = string.IsNullOrEmpty(fdata.OrderId) ? predicate : predicate.And(x => x.OrderId == fdata.OrderId );
            predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => (x.UpdatedDate==null?x.AddedDate:x.UpdatedDate )>= fdate );
            predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => (x.UpdatedDate == null ? x.AddedDate : x.UpdatedDate) <= tdate);
            #endregion

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<EzyTMPayRequest> results = repoEzyTMPayRequest
                .Query().Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(EzyTMPayRequest), typeof(User) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();
            KeyValuePair<int, List<EzyTMPayRequest>> resultResponse = new KeyValuePair<int, List<EzyTMPayRequest>>(totalCount, results);

            return resultResponse;
        }
        public ICollection<StatusType> GetStatusType()
        {
            return repoStatusType.Query().Filter(x => x.Id == 1 || x.Id == 2 || x.Id == 3 || x.Id == 4 || x.Id == 5).Get().ToList();
        }
        public ICollection<GatewayTransferType> GetTransfer()
        {
            return repoTransferType.Query().Get().ToList();
        }
        #endregion
    }
}
