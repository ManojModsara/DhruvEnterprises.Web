using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class Lapumargindto
    {
        public Lapumargindto()
        {
            this.Lapumarginvaluedtos = new List<Lapumarginvaluedto>();
        }
        public int DealerId { get; set; }
        public List<Lapumarginvaluedto> Lapumarginvaluedtos { get; set; }
    }

    public class Lapumarginvaluedto
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public int OpId { get; set; }
        public string OperatorName { get; set; }

        public decimal Margin { get; set; }
        public int AddedById { get; set; }
        public DateTime AddedDate { get; set; }
        public int UpdateById { get; set; }
        public DateTime UpdatedDate { get; set; }

    }
}
