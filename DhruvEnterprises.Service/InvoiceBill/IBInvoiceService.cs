using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IBInvoiceService
    {
        KeyValuePair<int, List<InvoiceDetail>> GetInvoices(DataTableServerSide searchModel, int userId = 0);
        InvoiceDetail GetInvoicesDetails(string invoiceNo);
        InvoiceDetail Save(InvoiceDetail userInvoice);
        KeyValuePair<int, List<InvoiceDetail>> GetUserInvoices(DataTableServerSide searchModel, int userId = 0);
    }
}
