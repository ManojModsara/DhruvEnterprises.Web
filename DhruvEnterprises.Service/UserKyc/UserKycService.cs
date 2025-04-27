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
    public class UserKycService : IUserKycService
    {

        #region "Fields"
        private IRepository<UserKYC> repoUserKYC;
        
        #endregion

        #region "Cosntructor"
        public UserKycService(IRepository<UserKYC> _repoUserKYC)
        {
            this.repoUserKYC = _repoUserKYC;
            
        }
        #endregion

        #region "Methods"
       
        public UserKYC Save(UserKYC Userkyc)
        {
            Userkyc.UpdatedDate = DateTime.Now;
            if (Userkyc.Id == 0)
            {
                Userkyc.AddedDate = DateTime.Now;
                repoUserKYC.Insert(Userkyc);
            }
            else
            {
                repoUserKYC.Update(Userkyc);
            }
            return Userkyc;
        }

        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoUserKYC != null)
            {
                repoUserKYC.Dispose();
                repoUserKYC = null;
            }
        }
        #endregion


    }
}
