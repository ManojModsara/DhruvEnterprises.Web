using System.Collections.Generic;

namespace DhruvEnterprises.Dto
{
    public class CircleAmtDto
    {
        public CircleAmtDto()
        {
            this.CircleAmtLists = new List<CircleAmtList>();
            this.CircleApiSourceList = new List<CircleApiSourceDTO>();
        }
        public List<CircleAmtList> CircleAmtLists { get; set; }
        public List<CircleApiSourceDTO> CircleApiSourceList { get; set; }
    }
    public class CircleAmtList
    {
        public int Id { get; set; }
        public int API1_Id { get; set; }
        public int API2_Id { get; set; }
        public int API3_Id { get; set; }
        public int OpId { get; set; }
        public int CircleId { get; set; }
        public string CircleName { get; set; }
        public bool IsRoffer { get; set; }
        public string OperatorName { get; set; }
        public string ApiName { get; set; }
        public string AddedOn { get; set; }
        public string AddededBy { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class CircleApiSourceDTO
    {
        public int Apiid { get; set; }
        public string Name { get; set; }
        public int Opid { get; set; }
    }

}
