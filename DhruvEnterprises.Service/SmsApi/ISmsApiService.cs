using DhruvEnterprises.Data;
using DhruvEnterprises.Core;
using System;
using System.Collections.Generic;

namespace DhruvEnterprises.Service
{
    public interface ISmsApiService : IDisposable
    {
        KeyValuePair<int, List<SmsAPI>> GetSmsApi(DataTableServerSide searchModel);
        SmsAPI SmsApiList();
        SMSData Save(SMSData sMSData);
        SmsAPI Save(SmsAPI smsAPI);
        SmsAPI GetSMSById(int id = 0);
    }
}
