using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
   public  interface IRequestResponseService : IDisposable
    {
        ICollection<RequestResponse> GetRequestResponse(ReqResFilterDto ft);
        RequestResponse GetRequestResponse(int id);
        KeyValuePair<int, List<RequestResponse>> GetRequestResponses(DataTableServerSide searchModel, ReqResFilterDto ft); 
        RequestResponse Save(RequestResponse requestResponse);
        List<RequestResponse> GetRequestResponse(long recid);
        KeyValuePair<int, List<RequestResponse>> GetRequestResponses1(DataTableServerSide searchModel, ReqResFilterDto ft);
    }
}
