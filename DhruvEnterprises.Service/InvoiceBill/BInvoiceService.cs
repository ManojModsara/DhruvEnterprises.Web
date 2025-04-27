using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class BInvoiceService : IBInvoiceService
    {
        private IRepository<InvoiceDetail> repoInvoiceDetail;
        public BInvoiceService(IRepository<InvoiceDetail> _repoInvoiceDetail)
        {
            this.repoInvoiceDetail = _repoInvoiceDetail;
        }
        public KeyValuePair<int, List<InvoiceDetail>> GetInvoices(DataTableServerSide searchModel, int userId = 0)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<InvoiceDetail>(searchModel, new Type[] { typeof(InvoiceDetail) });
            if (fdata != null)
            {       predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);               
               
            }
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<InvoiceDetail> results = repoInvoiceDetail
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(User), typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<InvoiceDetail>> resultResponse = new KeyValuePair<int, List<InvoiceDetail>>(totalCount, results);

            return resultResponse;
        }


        public KeyValuePair<int, List<InvoiceDetail>> GetUserInvoices(DataTableServerSide searchModel, int userId = 0)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<InvoiceDetail>(searchModel, new Type[] { typeof(InvoiceDetail) });
            if (fdata != null)
            {
                predicate = x => x.UserId == userId;
            }
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<InvoiceDetail> results = repoInvoiceDetail
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(User), typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<InvoiceDetail>> resultResponse = new KeyValuePair<int, List<InvoiceDetail>>(totalCount, results);

            return resultResponse;
        }
        public InvoiceDetail GetInvoicesDetails(string invoiceNo)
        {
            return repoInvoiceDetail.Query().Filter(x => x.InvoiceNo == invoiceNo).Get().FirstOrDefault();
        }
        public InvoiceDetail Save(InvoiceDetail userInvoice)
        {
            userInvoice.UpdatedDate = DateTime.Now;
            if (userInvoice.Id == 0)
            {
                userInvoice.AddedDate = DateTime.Now;
                repoInvoiceDetail.Insert(userInvoice);
            }
            else
            {
                repoInvoiceDetail.Update(userInvoice);
            }
            return userInvoice;
        }
        public void Dispose()
        {
            if (repoInvoiceDetail != null)
            {
                repoInvoiceDetail.Dispose();
                repoInvoiceDetail = null;
            }
        }
    }
}
