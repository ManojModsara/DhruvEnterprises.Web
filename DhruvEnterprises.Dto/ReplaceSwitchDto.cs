using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class ReplaceSwitchDto
    {

        public ReplaceSwitchDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.SwitchTypeList = new List<SwitchTypeDto>();
            this.ApiSourceList = new List<ApiSourceDto>();
        }

        [DisplayName("Service")]
        public int OperatorId { get; set; } 

        [DisplayName("Switch Type")]
        public int SwitchTypeId { get; set; }

        [DisplayName("Current Vendor")]
        public int CurrentApiId { get; set; }

        [DisplayName("New Vendor")]
        public int NewApiId { get; set; }

        [DisplayName("Amount")]
        public string Amount { get; set; } 

        public List<OperatorDto>   OperatorList  { get; set; } 
        public List<SwitchTypeDto> SwitchTypeList { get; set; } 
        public List<ApiSourceDto>  ApiSourceList  { get; set; }
       
    }

    public class SwitchTypeDto 
    {
        public byte Id { get; set; }
        public string TypeName { get; set; }
    }
}
