using System;
using System.Collections.Generic;
using System.Linq;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;

namespace DhruvEnterprises.Service
{
 public   class OpSerialService : IOpSerialService
    {
        #region "Fields"
        private IRepository<OperatorSerial> repoOperatorSerial;

        #endregion

        #region "Cosntructor"
        public OpSerialService(IRepository<OperatorSerial> _repoOperatorSerial)
        {
            this.repoOperatorSerial = _repoOperatorSerial;
        }
        #endregion

        #region "Methods"

        public OperatorSerial GetOperatorSerialById(int id = 0)
        {
            return repoOperatorSerial.FindById(id);
        }

        public KeyValuePair<int, List<OperatorSerial>> GetOperatorSerials(DataTableServerSide searchModel)
        {
            var predicate = CustomPredicate.BuildPredicate<OperatorSerial>(searchModel, new Type[] { typeof(OperatorSerial) });
            int totalCount;
            totalCount = 0;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
               List<OperatorSerial> results = repoOperatorSerial
                .Query()
                .Filter(predicate)
                //.OrderBy(x=>x.)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(OperatorSerial) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();
            KeyValuePair<int, List<OperatorSerial>> resultResponse = new KeyValuePair<int, List<OperatorSerial>>(totalCount, results);
            return resultResponse;
        }


        public OperatorSerial OperatorSerialList()
        {
            return repoOperatorSerial.Query().Get().ToList().FirstOrDefault();
        }
        
        public OperatorSerial Save(OperatorSerial opSerial)
        {
            if (opSerial.Id == 0)
            {
                repoOperatorSerial.Insert(opSerial);
            }
            else
            {
                repoOperatorSerial.Update(opSerial);
            }
            return opSerial;
        }

        public void Delete(int id)

        {

            var opSerial = repoOperatorSerial.FindById(id);
            repoOperatorSerial.Delete(opSerial);


        }

        public bool SeriesExists(int opid, int circleid, string series)
        {
            ICollection<OperatorSerial> opSeries = repoOperatorSerial.Query().AsTracking()
              .Filter(c => c.OpId == opid || c.CircleId == circleid || c.Series.Trim() == series)
              .Get().ToList();

            return opSeries.Count > 0 ? true : false;

        }

        public ICollection<OperatorSerial> GetOperatorList(int CircleId = 0)
        {
            var predicate = PredicateBuilder.True<OperatorSerial>();           
            predicate = CircleId == 0 ? predicate : predicate.And(x => x.CircleId == CircleId);
            return repoOperatorSerial.Query().Filter(predicate).Get().ToList();
        }

        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoOperatorSerial != null)
            {
                repoOperatorSerial.Dispose();
                repoOperatorSerial = null;
            }
        }
        #endregion
    }
}
