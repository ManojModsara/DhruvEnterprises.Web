using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public  interface IPackageService : IDisposable
    {
        ICollection<Package> GetPackageList();
        ICollection<Package> PackageUserList();
        Package GetPackage(int? id);
        KeyValuePair<int, List<Package>> GetPackageList(DataTableServerSide searchModel, int userId = 0);
        Package Save(Package package);
        TxnLedger GetuserIdCl_Bal(int userid);
        ICollection<Operator> GetOperatorList();
        bool Save(PackageComm packageComm);
        ICollection<PackageCommCircle> PackageCommCircleList(int pid = 0, int opid = 0, int uid = 0, int apid = 0, int typeid = 0, int Circleid = 0);
        ICollection<PackageComm>  PackageCommList(int pid = 0, int opid = 0, int uid = 0, int apid = 0, int typeid=0);
        PackageComm GetPackageCommByOpId(int pid, int opid);
        OperatorCode GetOperatorCode(int apiid, int opid);
        ICollection<OperatorCode> ApioperatorCodes(int id);
        bool Save(OperatorCode operatorCode);
        PackageCommCircle GetPackageCommCircle(int id);
        bool Save(PackageCommCircle packageCommCircle);
        ICollection<PackageCommCircle> GetPackageCommCircleByOpId(int pid, int opid);
        PackageCommCircle GetPackageCommCircleByOpId(int pid, int opid, int cid);
        ICollection<CommType> GetCommTypes();
        ICollection<AmtType> GetAmtTypes();
        PackageCommRange GetPackageCommRange(int id = 0);
        PackageCommRange Save(PackageCommRange packCommRange);
        bool DeletePackCommRange(int Id);
        ICollection<PackageCommRange> GetPackageCommRangeList(int pid = 0);
    }
}
