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
   public  class PackageService : IPackageService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Operator> repoOperator;
        private IRepository<Package> repoPackage;
        private IRepository<PackageComm> repoPackagecomm;
        private IRepository<AmtType> repoAmtType;
        private IRepository<CommType> repoCommType;
        private IRepository<ApiSource> repoApiSource;
        private IRepository<OperatorCode> repoOperatorCode;
        private IRepository<PackageCommCircle> repoPackageCommCircle;
        private IRepository<PackageCommRange> repoPackageCommRange;
        private IRepository<TxnLedger> repoTxnLedger;

        #endregion

        #region "Cosntructor"
        public PackageService(IRepository<TxnLedger> _repotxnLedger, IRepository<PackageCommRange> _repoPackageCommRange, IRepository<User> _repoUserMaster, IRepository<ApiSource> _repoApiSource,IRepository<OperatorCode> _repoOperatorCode, IRepository<Operator> _repoOperator, IRepository<Package> _repoPackage, IRepository<PackageComm> _repoPackagecomm, IRepository<AmtType> _repoAmtType, IRepository<CommType> _repoCommType, IRepository<PackageCommCircle> _repoPackageCommCircle)
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoOperator = _repoOperator;
            this.repoPackage = _repoPackage;
            this.repoPackagecomm = _repoPackagecomm;
            this.repoAmtType = _repoAmtType;
            this.repoCommType = _repoCommType;
            this.repoApiSource = _repoApiSource;
            this.repoOperatorCode = _repoOperatorCode;
            this.repoPackageCommCircle = _repoPackageCommCircle;
            this.repoPackageCommRange = _repoPackageCommRange;
            this.repoTxnLedger = _repotxnLedger;
        }
        #endregion

        public ICollection<Package> GetPackageList()
        {
            return repoPackage.Query().AsTracking().Get().ToList();
        }

        public ICollection<Operator> GetOperatorList()
        {
            return repoOperator.Query().AsTracking().Get().ToList();
        }

        public Package GetPackage(int? id)
        {
            return repoPackage.FindById(id);
        }

        public PackageComm GetPackageCommByOpId(int pid, int opid)  
        {
            return repoPackagecomm.Query().Filter(x=>x.PackId==pid && x.OpId==opid).Get().ToList().FirstOrDefault();
        }

       
        public KeyValuePair<int, List<Package>> GetPackageList(DataTableServerSide searchModel, int userId = 0)
        {
            var predicate = CustomPredicate.BuildPredicate<Package>(searchModel, new Type[] { typeof(PackageComm) });

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Package> results = repoPackage
                .Query()
                .Filter(predicate.And(a => a.PTypeId != 0)) //a.IsDeleted == false &&
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(PackageComm) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Package>> resultResponse = new KeyValuePair<int, List<Package>>(totalCount, results);

            return resultResponse;
        }
        
        public Package Save(Package package)
        {
            package.UpdatedDate = DateTime.Now;
            if (package.Id == 0)
            {
                package.AddedDate = DateTime.Now;
                repoPackage.Insert(package);
            }
            else
            {
                package.UpdatedDate = DateTime.Now;
                repoPackage.Update(package);
            }
            return package;
        }

        public bool Save(PackageComm packageComm)
        {
            if (packageComm.Id > 0)
            {
                packageComm.UpdatedDate = DateTime.Now;
                repoPackagecomm.Update(packageComm);
            }
            else
            {
                packageComm.AddedDate = DateTime.Now;
                repoPackagecomm.Insert(packageComm);
            }
           
            return true;
        }
        
        public ICollection<PackageComm> PackageCommList(int pid=0, int opid=0,int uid=0, int apid=0, int typeid = 0)
        {
            var user = repoAdminUser.FindById(uid);
            var vendor = repoApiSource.FindById(apid);
            pid = uid > 0? (user.PackageId ?? 0) : apid > 0? (vendor.ApiPackid ?? 0): pid;
            return repoPackagecomm.Query().Filter(x => (pid == 0 ? true : x.PackId == pid) && (opid == 0 ? true : x.OpId == opid) && (x.Package.PTypeId == typeid)).Get().ToList(); ;
        }
        public ICollection<PackageCommCircle> PackageCommCircleList(int pid = 0, int opid = 0, int uid = 0, int apid = 0, int typeid = 0,int Circleid=0)
        {
            var user = repoAdminUser.FindById(uid);
            var vendor = repoApiSource.FindById(apid);
            pid = uid > 0 ? (user.PackageId ?? 0) : apid > 0 ? (vendor.ApiPackid ?? 0) : pid;
            return repoPackageCommCircle.Query().Filter(x => (pid == 0 ? true : x.PackId == pid) && (Circleid == 0 ? true : x.CircleId == Circleid) && (opid == 0 ? true : x.OpId == opid) && (x.Package.PTypeId == typeid)).Get().ToList(); ;
        }
        public ICollection<Package> PackageUserList()
        {
            List<Package> packages = new List<Package>();
                packages = repoPackage.Query().Filter(x => x.PTypeId == 1).Get().ToList();
            return packages;
        }

        public ICollection<OperatorCode> ApioperatorCodes(int id)
        {
            List<OperatorCode> OperatorCode = new List<OperatorCode>();
            var apiSource = repoApiSource.FindById(id);

            if (apiSource.Id != 0)
            {
                OperatorCode = repoOperatorCode.Query().Filter(x => x.ApiId == apiSource.Id).Get().ToList();
            }

            return OperatorCode;
        }

        public bool Save(OperatorCode operatorCode)
        {
            if (operatorCode.Id > 0)
            {
                operatorCode.UpdatedDate = DateTime.Now;
                repoOperatorCode.Update(operatorCode);
            }
            else
            {
                operatorCode.AddedDate = DateTime.Now;
                repoOperatorCode.Insert(operatorCode);
            }
          
            return true;
        }
        
        public OperatorCode GetOperatorCode(int apiid, int opid)
        {
            return repoOperatorCode.Query().Filter(x => x.ApiId == apiid && x.OpId == opid).Get().ToList().FirstOrDefault();
        }

        public PackageCommCircle GetPackageCommCircle(int id)
        {
            return repoPackageCommCircle.FindById(id);
        }

        public ICollection<PackageCommCircle> GetPackageCommCircleByOpId(int pid, int opid)
        {
            return repoPackageCommCircle.Query().Filter(x => x.PackId == pid && x.OpId == opid).Get().ToList();
        }

        public PackageCommCircle GetPackageCommCircleByOpId(int pid, int opid, int cid)
        {
            return repoPackageCommCircle.Query().Filter(x => x.PackId == pid && x.OpId == opid && x.CircleId == cid).Get().ToList().FirstOrDefault();
        }
        
        public bool Save(PackageCommCircle packageCommCircle)
        {
            if (packageCommCircle.Id > 0)
            {
                packageCommCircle.UpdatedDate = DateTime.Now;
                repoPackageCommCircle.Update(packageCommCircle);
            }
            else
            {
                packageCommCircle.AddedDate = DateTime.Now;
                repoPackageCommCircle.Insert(packageCommCircle);
            }

            return true;
        }

        public ICollection<CommType> GetCommTypes()
        {
            return repoCommType.Query().Get().ToList();
        }

        public ICollection<AmtType> GetAmtTypes()
        {
            return repoAmtType.Query().Get().ToList();
        }

        public PackageCommRange GetPackageCommRange(int id=0)
        {
            return repoPackageCommRange.FindById(id);
        }

        public PackageCommRange Save(PackageCommRange packCommRange)
        {
            packCommRange.UpdatedDate = DateTime.Now;
            if (packCommRange.Id == 0)
            {
                packCommRange.AddedDate = DateTime.Now;
                repoPackageCommRange.Insert(packCommRange);
            }
            else
            {
                packCommRange.UpdatedDate = DateTime.Now;
                repoPackageCommRange.Update(packCommRange);
            }
            return packCommRange;
        }

        public TxnLedger GetuserIdCl_Bal(int userid)
        {
            return repoTxnLedger.Query().Filter(x => x.UserId == userid).Get().FirstOrDefault();
        }
        public bool DeletePackCommRange(int Id)
        {
            repoPackageCommRange.Delete(Id);
            return true;
        }

        public ICollection<PackageCommRange> GetPackageCommRangeList(int pid = 0)
        {
            return repoPackageCommRange.Query().Filter(x => (pid == 0 ? true : x.PackId == pid) ).Get().ToList(); 
        }


        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }

            if (repoOperator != null)
            {
                repoOperator.Dispose();
                repoOperator = null;
            }

            if (repoPackage != null)
            {
                repoPackage.Dispose();
                repoPackage = null;
            }

            if (repoPackagecomm != null)
            {
                repoPackagecomm.Dispose();
                repoPackagecomm = null;
            }

            if (repoAmtType != null)
            {
                repoAmtType.Dispose();
                repoAmtType = null;
            }

            if (repoCommType != null)
            {
                repoCommType.Dispose();
                repoCommType = null;
            }

            if (repoApiSource != null)
            {
                repoApiSource.Dispose();
                repoApiSource = null;
            }

            if (repoOperatorCode != null)
            {
                repoOperatorCode.Dispose();
                repoOperatorCode = null;
            }
        }
        #endregion
        
    }
}
