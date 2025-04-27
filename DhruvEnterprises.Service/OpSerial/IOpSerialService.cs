using System;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System.Collections.Generic;

namespace DhruvEnterprises.Service
{
    public  interface IOpSerialService : IDisposable
    {
        KeyValuePair<int, List<OperatorSerial>> GetOperatorSerials(DataTableServerSide searchModel);
        OperatorSerial OperatorSerialList();
        OperatorSerial Save(OperatorSerial opSerial);
        OperatorSerial GetOperatorSerialById(int id = 0);
        void Delete(int id);
        bool SeriesExists(int opid, int circleid, string series);
        ICollection<OperatorSerial> GetOperatorList( int CircleId = 0);

    }
}
