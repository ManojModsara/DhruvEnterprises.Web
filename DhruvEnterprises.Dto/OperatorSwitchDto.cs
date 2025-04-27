using System.Collections.Generic;
using System.ComponentModel;

namespace DhruvEnterprises.Dto
{
    public class OperatorSwitchDto
    {
        public OperatorSwitchDto()
        {
            this.OpcodeLists = new List<OpcodeListDTO>();
            this.apiSourceDTOs = new List<ApiSourceDTO>();
            this.ValidTypeList = new List<ValidTypeDto>();
                        
        }
        public int Opid { get; set; }
        public string OperatorName { get; set; }
        [DisplayName("Vendor1_Id")]
        public int ApiID1 { get; set; }
        [DisplayName("Vendor2_Id")]
        public int ApiID2 { get; set; }
        [DisplayName("Vendor3_Id")]
        public int ApiID3 { get; set; }
        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }
        public int SwitchTypeId { get; set; }
        public List<OpcodeListDTO> OpcodeLists { get; set; }
        public List<ApiSourceDTO> apiSourceDTOs { get; set; }
        public List<ValidTypeDto> ValidTypeList { get; set; }
    }
    

   



    public class OpcodeListDTO
    {
        public int Id { get; set; }
        [DisplayName("Vendor1")]
        public int ApiID1 { get; set; }
        [DisplayName("Vendor2")]
        public int ApiID2 { get; set; }
        [DisplayName("Vendor3")]
        public int ApiID3 { get; set; }
        public int Opid { get; set; }
        public int SwitchTypeId { get; set; }
        public string OperatorName { get; set; }
        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }
        
        public int FetchApiId { get; set; } 
        public int ValidTypeId { get; set; }
        public bool IsFetch { get; set; }
        public bool IsPartial { get; set; }

        public string AddededBy { get; set; }
        public string AddedOn { get; set; } 

        public string UpdatedBy { get; set; }
        public string UpdatedOn { get; set; }


    }
    public class ApiSourceDTO
    {
        [DisplayName("Vendor Id")]
        public int Apiid { get; set; }
        public string Name { get; set; }
        public int Opid { get; set; }
    }

    public class ValidTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OpValidationDto
    {
        public int OpId { get; set; }

        public string NumberLength { get; set; }
        public string NumberRange { get; set; }
        public string NumberStart { get; set; }
        public bool   NumberIsNumeric { get; set; }
        public string NumberErrorMessage { get; set; }

        public string AccountLength { get; set; }
        public string AccountRange { get; set; }
        public string AccountStart { get; set; }
        public bool   AccountIsNumeric { get; set; }
        public string AccountErrorMessage { get; set; }

        public string Auth3Length { get; set; }
        public string Auth3Range { get; set; }
        public string Auth3Start { get; set; }
        public bool   Auth3IsNumeric { get; set; }
        public string Auth3ErrorMessage { get; set; }

        public string AmountLength { get; set; }
        public string AmountRange { get; set; }
        public string AmountStart { get; set; }
        public bool   AmountIsNumeric { get; set; }
        public string AmountErrorMessage { get; set; }

    }
}
