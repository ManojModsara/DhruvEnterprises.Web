using DhruvEnterprises.Data;
using DhruvEnterprises.Core;
using System;
using System.Collections.Generic;

namespace DhruvEnterprises.Service
{
  public  interface IEmailApiService: IDisposable
    {
        KeyValuePair<int, List<EmailAPI>> GetEmailApi(DataTableServerSide searchModel);
        EmailAPI EmailApiList();
        EmailAPI Save(EmailAPI smsAPI);
        EmailAPI GetEmailById(int id = 0);
    }
}
