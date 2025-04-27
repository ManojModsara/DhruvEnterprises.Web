using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IUserKycService
    {
        UserKYC Save(UserKYC userKyc);
        
    }
}
