using MYRC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MYRC.Service
{
    public interface IRechargeCallService : IDisposable
    {
        User CheckUserToken(string TokenId);
    }
}
