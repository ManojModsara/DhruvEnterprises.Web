using MYRC.Data;
using MYRC.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MYRC.Service
{
    public class RechargeCallService : IRechargeCallService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Operator> repoOperator;
        private IRepository<Package> repoPackage;
        private IRepository<PackageComm> repoPackagecomm;
        private IRepository<ApiSource> repoApiSource;
        #endregion

        #region "Cosntructor"
        public RechargeCallService(IRepository<User> _repoUserMaster, IRepository<ApiSource> _repoApiSource,  IRepository<Operator> _repoOperator, IRepository<Package> _repoPackage, IRepository<PackageComm> _repoPackagecomm)
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoOperator = _repoOperator;
            this.repoPackage = _repoPackage;
            this.repoPackagecomm = _repoPackagecomm;
            this.repoApiSource = _repoApiSource;
        }
        #endregion

        #region "Method"

        public User CheckUserToken(string TokenId)
        {
            return repoAdminUser.Query().Filter(x => x.TokenAPI == TokenId).Get().FirstOrDefault();
        }

        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }
        }
        #endregion
    }
}
