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
    public class OperatorSwitchService : IOperatorSwitchService
    {
        #region "Fields"
        private IRepository<User> repoUser;
        private IRepository<Operator> repoOperator;
        private IRepository<OperatorCode> repoOperatorCode;
        private IRepository<ApiSource> repoApiSource;
        private IRepository<SwitchType> repoSwitchType;
        private IRepository<Circle> repoCircle;
        private IRepository<CircleRouting> repoCircleRouting;
        private IRepository<OperatorType> repoOperatorType;
        private IRepository<OperatorValidType> repoOperatorValidType;
        private IRepository<OperatorValidation> repoOperatorValidation;

        #endregion

        #region "Cosntructor"
        public OperatorSwitchService(
            IRepository<User> _repoUser,
            IRepository<CircleRouting> _repoCircleRouting,
            IRepository<Circle> _repoCircle,
            IRepository<SwitchType> _repoSwitchType,
            IRepository<OperatorCode> _repoOperatorCode,
            IRepository<ApiSource> _repoApiSource,
            IRepository<Operator> _repoOperator,
            IRepository<OperatorType> _repoOperatorType,
            IRepository<OperatorValidType> _repoOperatorValidType,
            IRepository<OperatorValidation> _repoOperatorValidation)
        {
            this.repoUser = _repoUser;
            this.repoOperatorCode = _repoOperatorCode;
            this.repoApiSource = _repoApiSource;
            this.repoOperator = _repoOperator;
            this.repoCircleRouting = _repoCircleRouting;
            this.repoSwitchType = _repoSwitchType;
            this.repoCircle = _repoCircle;
            this.repoOperatorType = _repoOperatorType;
            this.repoOperatorValidType = _repoOperatorValidType;
            this.repoOperatorValidation = _repoOperatorValidation;
        }
        #endregion

        #region Method
        public KeyValuePair<int, List<Operator>> GetOperatorSwitch(DataTableServerSide searchModel)
        {
            var predicate = CustomPredicate.BuildPredicate<Operator>(searchModel, new Type[] { typeof(Operator) });

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Operator> results = repoOperator
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Operator>> resultResponse = new KeyValuePair<int, List<Operator>>(totalCount, results);

            return resultResponse;
        }

        public KeyValuePair<int, List<Circle>> GetCircleList(DataTableServerSide searchModel)
        {
            var predicate = CustomPredicate.BuildPredicate<Circle>(searchModel);

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Circle> results = repoCircle
                .Query()
                .CustomOrderBy(u => u.OrderBy(searchModel))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Circle>> resultResponse = new KeyValuePair<int, List<Circle>>(totalCount, results);

            return resultResponse;
        }

        public List<Circle> circlesList()
        {
            return repoCircle.Query().Get().OrderBy(x=>x.CircleName).ToList();
        }

        public Operator GetOperator(int opid)
        {
            return repoOperator.FindById(opid);
        }

        public Operator GetOperatorByName(string opname)
        {
            var oplist = repoOperator.Query().Get().ToList();
            return oplist.Where(x => x.Name.ToLower() == opname).FirstOrDefault();
        }

        public List<SwitchType> SwitchTypes()
        {
            return repoSwitchType.Query().Get().ToList();
        }

        public List<OperatorType> GetOperatorType()
        {
            return repoOperatorType.Query().Get().ToList();
        }

        public List<ApiSource> GetApiSourceList()
        {
            return repoApiSource.Query().Get().ToList();
        }

        public List<OperatorCode> OpcodeApiList(int Opid)
        {
            return repoOperatorCode.Query().Filter(x => x.OpId == Opid && x.OpCode != null).Get().ToList();
        }

        public List<CircleRouting> CircleApiRouteList(int Opid, int CircleId)
        {
            return repoCircleRouting.Query().Filter(x => x.OpId == Opid && x.CircleId == CircleId).Get().ToList();
        }

        public List<CircleRouting> CircleApiRouteList(int CircleId)
        {
            return repoCircleRouting.Query().Filter(x => x.CircleId != CircleId).Get().ToList();
        }

        public bool Save(List<Operator> opList)
        {
            foreach (var optr in opList)
            {
                if (optr.Id > 0)
                {
                    optr.UpdatedDate = DateTime.Now;
                    repoOperator.Update(optr);
                }
            }
            return false;
        }

        public void Save(Operator entity)
        {
            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
                repoOperator.Update(entity);
            }
            else
            {
                entity.AddedDate = DateTime.Now;
                repoOperator.Insert(entity);
            }

        }

        public bool Save(CircleRouting entity, bool IsExists = false)
        {
            

            if (!IsExists)
            {
                entity.AddedDate = DateTime.Now;
                repoCircleRouting.Insert(entity);
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                repoCircleRouting.Update(entity);
            }


            return false;
        }

        public List<OperatorValidType> GetOperatorValidTypes()
        {
            return repoOperatorValidType.Query().Get().ToList();
        }

        public List<OperatorValidation> GetOperatorValidation(int opid = 0)
        {
            return repoOperatorValidation.Query().Filter(x => opid == 0 || x.OpId == opid).Get().ToList();
        }

        public void Save(OperatorValidation entity)
        {
            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
                repoOperatorValidation.Update(entity);
            }
            else
            {
                entity.AddedDate = DateTime.Now;
                repoOperatorValidation.Insert(entity);
            }

        }

        public void Delete(OperatorValidation entity)
        {
            repoOperatorValidation.Delete(entity);
        }
       
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoUser != null)
            {
                repoUser.Dispose();
                repoUser = null;
            }

            if (repoOperator != null)
            {
                repoOperator.Dispose();
                repoOperator = null;
            }

            if (repoCircleRouting != null)
            {
                repoCircleRouting.Dispose();
                repoCircleRouting = null;
            }

            if (repoOperatorCode != null)
            {
                repoOperatorCode.Dispose();
                repoOperatorCode = null;
            }

            if (repoApiSource != null)
            {
                repoApiSource.Dispose();
                repoApiSource = null;
            }

            if (repoSwitchType != null)
            {
                repoSwitchType.Dispose();
                repoSwitchType = null;
            }

            if (repoCircle != null)
            {
                repoCircle.Dispose();
                repoCircle = null;
            }

        }
        #endregion
    }
}
