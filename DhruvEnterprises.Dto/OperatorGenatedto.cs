using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class OperatorGenatedto
    {
        public int Id { get; set; }
        public int OpId { get; set; }
        public int NoLength { get; set; }
        public int KeyTypeId { get; set; }
        public string TextLength { get; set; }
        public int AddedById { get; set; }
        public Nullable<System.DateTime> AddedByDate { get; set; }
        public Nullable<int> UpdateById { get; set; }
        public Nullable<System.DateTime> UpdatedByDate { get; set; }
    }

    public class OperatorGenrateListdto
    {
        public OperatorGenrateListdto()
        {
            this.OperatorGenateList = new List<OperatorGenatedto>();
        }

        public List<OperatorGenatedto> OperatorGenateList { get; set; }
    }
}
