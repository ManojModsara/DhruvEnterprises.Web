using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IOperatorSwitchService
    {
        KeyValuePair<int, List<Operator>> GetOperatorSwitch(DataTableServerSide searchModel);
        List<OperatorCode> OpcodeApiList(int Opid);
        KeyValuePair<int, List<Circle>> GetCircleList(DataTableServerSide searchModel);
        List<CircleRouting> CircleApiRouteList(int Opid, int CircleId);
        List<CircleRouting> CircleApiRouteList(int CircleId);
        bool Save(CircleRouting model, bool IsExists = false);
        bool Save(List<Operator> model);
        void Save(Operator entity); 
        List<SwitchType> SwitchTypes();
        List<OperatorType> GetOperatorType(); 
        List<Circle> circlesList();
        Operator GetOperator(int opid);
        Operator GetOperatorByName(string opname); 
        List<ApiSource> GetApiSourceList();
        List<OperatorValidType> GetOperatorValidTypes();
        List<OperatorValidation> GetOperatorValidation(int opid = 0);
        void Save(OperatorValidation entity);
        void Delete(OperatorValidation entity);
    }
}
