using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class CommonSwitchDto
    {
        public CommonSwitchDto()
        {
            this.ApiList = new List<ApiSourceDto>();
            this.CircleList = new List<CircleDto>();
            this.CommonSwitchList = new List<SwitchDto>();
            this.OperatorList = new List<OperatorDto>();
            this.FilterTypeList = new List<FilterTypeDto>();//1=R-offer, 2=non R-offer, 3=Both, 4=Range
            this.UserList = new List<UserDto>();

        }
        public List<SwitchDto> CommonSwitchList { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<ApiSourceDto> ApiList { get; set; }
        public List<FilterTypeDto> FilterTypeList { get; set; }
        public List<UserDto> UserList { get; set; }
    }

    public class SwitchDto
    {
        public string AmountFilter { get; set; }
        public int ApiId { get; set; }
        public string ApiName { get; set; }
        public string CircleId { get; set; }
        public string CircleName { get; set; }
        public int Id { get; set; }
        public int FilterTypeId { get; set; }
        public string FilterTypeName { get; set; }
        public int OperatorId { get; set; }
        public string OperatorName { get; set; }
        public int Priority { get; set; }
        public string BlockUser { get; set; }
        public decimal MinRO { get; set; }

        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public long[] LapuIds { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public string RouteOP1 { get; set; }
        public string RouteOP2 { get; set; }
        public bool IsActive { get; set; }
        public string ActiveUpdatedOn { get; set; }
        public string ActiveUpdatedBy { get; set; }
    }

    public class OperatorDto
    {
        public OperatorDto()
        {
            this.OpTypeList = new List<OperatorTypeDto>();
        }
        public int OperatorId { get; set; }
        public string OpCode { get; set; }
        [DisplayName("Operator Name")]
        public string OperatorName { get; set; }
        [DisplayName("Operator Type")]
        public int OpTypeId { get; set; }
        public List<OperatorTypeDto> OpTypeList { get; set; }


    }

    public class OperatorTypeDto
    {

        public int TypeId { get; set; }
        public string TypeName { get; set; }

    }

    public class CircleDto
    {
        public int CircleId { get; set; }
        public string CircleCode { get; set; }
        public string CircleName { get; set; }
    }

    public class ApiSourceDto
    {
        public int ApiId { get; set; }
        public string ApiName { get; set; }
    }

    public class FilterTypeDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
    }

    public class EditSwitchDto
    {
        public EditSwitchDto()
        {
            this.ApiList = new List<ApiSourceDto>();
            this.CircleList = new List<CircleDto>();
            this.OperatorList = new List<OperatorDto>();
            this.FilterTypeList = new List<FilterTypeDto>();//1=R-offer, 2=non R-offer, 3=Both, 4=Range
            this.UserList = new List<UserDto>();

        }

        [DisplayName("Amount To Filter")]
        public string AmountFilter { get; set; }

        [DisplayName("Vendor")]
        public int apiId { get; set; }
        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }
        [DisplayName("Circle(s)")]
        public List<string> CircleIds { get; set; }
        public string CircleName { get; set; }
        [DisplayName("Switch Id")]
        public int Id { get; set; }
        [DisplayName("Filter-Type")]
        public int FilterTypeId { get; set; }
        public string FilterTypeName { get; set; }
        [DisplayName("Operator")]
        public int OperatorId { get; set; }
        public string OperatorName { get; set; }
        public int Priority { get; set; }
        [DisplayName("User(s)")]
        public List<string> UserIds { get; set; }
        public string UserName { get; set; }
        [DisplayName("ROffer(%)")]
        public decimal MinRO { get; set; }
        public long[] LapuIds { get; set; }
        [DisplayName("Block User")]
        public string BlockUser { get; set; }
        public string RouteOP1 { get; set; }
        public string RouteOP2 { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<ApiSourceDto> ApiList { get; set; }
        public List<FilterTypeDto> FilterTypeList { get; set; }
        public List<UserDto> UserList { get; set; }
    }

    public class BlockRouteDto
    {
        public BlockRouteDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.UserList = new List<UserDto>();
            this.ApiList = new List<ApiDto>();
            this.TypeList = new List<StatusTypeDto>()
            {
                new StatusTypeDto(){StatusId=1, StatusName="Block"},
                new StatusTypeDto(){StatusId=2, StatusName="Callback"}
            };
            this.StatusList = new List<StatusTypeDto>()
            {
                new StatusTypeDto(){StatusId=1, StatusName="Success"},
                new StatusTypeDto(){StatusId=2, StatusName="Processing"}
            };
            this.OpTypeList = new List<OperatorTypeDto>();
        }

        public int Id { get; set; }
        public string OperatorIds { get; set; }
        public string OperatorNames { get; set; }
        public string CircleIds { get; set; }
        public string CircleNames { get; set; }
        public string UserIds { get; set; }
        public string UserNames { get; set; }
        public string Amounts { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public string VendorIds { get; set; }
        public string VendorNames { get; set; }
        public string MsgFilter { get; set; }

        public int TypeId { get; set; }
        public int StatusId { get; set; }
        public string OpTypeIds { get; set; }
        public string OpTypeNames { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<UserDto> UserList { get; set; }
        public List<ApiDto> ApiList { get; set; }

        public List<StatusTypeDto> StatusList { get; set; }
        public List<StatusTypeDto> TypeList { get; set; }
        public List<OperatorTypeDto> OpTypeList { get; set; }
    }

    public class BlockRouteModel
    {
        public BlockRouteModel()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.UserList = new List<UserDto>();
            this.BlockRouteList = new List<BlockRouteDto>();
            this.ApiList = new List<ApiDto>();
            this.TypeList = new List<StatusTypeDto>()
            {
                new StatusTypeDto(){StatusId=1, StatusName="Block"},
                new StatusTypeDto(){StatusId=2, StatusName="Callback"}
            };
            this.StatusList = new List<StatusTypeDto>()
            {
                new StatusTypeDto(){StatusId=1, StatusName="Success"},
                new StatusTypeDto(){StatusId=2, StatusName="Processing"}
            };
            this.OpTypeList = new List<OperatorTypeDto>();
        }

        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<UserDto> UserList { get; set; }
        public List<BlockRouteDto> BlockRouteList { get; set; }
        public List<ApiDto> ApiList { get; set; }

        public List<StatusTypeDto> StatusList { get; set; }
        public List<StatusTypeDto> TypeList { get; set; }
        public List<OperatorTypeDto> OpTypeList { get; set; } 
    }

    public class EditBlockRouteDto
    {
        public EditBlockRouteDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.UserList = new List<UserDto>();
            this.ApiList = new List<ApiDto>();

            this.TypeList = new List<StatusTypeDto>()
            {
                new StatusTypeDto(){StatusId=1, StatusName="Block"},
                new StatusTypeDto(){StatusId=2, StatusName="Callback"}
            };
            this.StatusList = new List<StatusTypeDto>()
            {
                new StatusTypeDto(){StatusId=1, StatusName="Success"},
                new StatusTypeDto(){StatusId=2, StatusName="Processing"}
            };
            this.OpTypeList = new List<OperatorTypeDto>();
        }
        public int Id { get; set; }
        public List<string> OperatorIds { get; set; }
        public string OperatorNames { get; set; }
        public List<string> CircleIds { get; set; }
        public string CircleNames { get; set; }
        public List<string> UserIds { get; set; }
        public string UserNames { get; set; }
        public string Amounts { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public List<string> VendorIds { get; set; }
        public string VendorNames { get; set; }
        public string MsgFilter { get; set; }

        [DisplayName("Route Type")]
        public int TypeId { get; set; }

        public int StatusId { get; set; }
        public List<string> OpTypeIds { get; set; }
        public string OpTypeNames { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<UserDto> UserList { get; set; }
        public List<ApiDto> ApiList { get; set; }
        public List<StatusTypeDto> StatusList { get; set; }
        public List<StatusTypeDto> TypeList { get; set; }
        public List<OperatorTypeDto> OpTypeList { get; set; } 
    }

    public class PackCommRangeDto
    {
        public PackCommRangeDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.OpTypeList = new List<OperatorTypeDto>();
            this.CommTypeList = new List<OperatorTypeDto>();
            this.AmtTypeList = new List<OperatorTypeDto>();
            this.PackCommRangeList = new List<PackCommRangeDto>();
            this.PackageList = new List<PackageDto>();


        }

        public int Id { get; set; }
        public int PackId { get; set; }
        public int PackTypeId { get; set; }
        public bool IsUserLoss { get; set; }

        public string PackName { get; set; }
        public string OpTypeIds { get; set; }
        public string OpTypeNames { get; set; }
        public string OperatorIds { get; set; }
        public string OperatorNames { get; set; }
        public string CircleIds { get; set; }
        public string CircleNames { get; set; }
        public string AmountRange { get; set; } 
        public decimal CommAmt { get; set; }
        public int MinAmt { get; set; }
        public int MaxAmt { get; set; }
        public byte AmtTypeId { get; set; }
        public byte CommTypeId { get; set; }
        public string AmtTypeName { get; set; }
        public string CommTypeName { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; } 
        
        public List<OperatorTypeDto> OpTypeList { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<OperatorTypeDto> CommTypeList { get; set; }
        public List<OperatorTypeDto> AmtTypeList { get; set; }
        public List<PackCommRangeDto> PackCommRangeList { get; set; }
        public List<PackageDto> PackageList { get; set; }


    }

    public class EditPackCommRangeDto
    {
        public EditPackCommRangeDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.OpTypeList = new List<OperatorTypeDto>();
            this.CommTypeList = new List<OperatorTypeDto>();
            this.AmtTypeList = new List<OperatorTypeDto>();
            this.PackCommRangeList = new List<PackCommRangeDto>();
        }

        public int Id { get; set; }
        public int PackId { get; set; }
        public string PackName { get; set; }
        public bool IsUserLoss { get; set; }

        public List<string> OpTypeIds { get; set; }
        public string OpTypeNames { get; set; }
        public List<string> OperatorIds { get; set; }
        public string OperatorNames { get; set; }
        public List<string> CircleIds { get; set; }
        public string CircleNames { get; set; }
        public string AmountRange { get; set; }
        public decimal CommAmt { get; set; }
        public byte AmtTypeId { get; set; }
        public byte CommTypeId { get; set; }
        public string AmtTypeName { get; set; }
        public string CommTypeName { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

        public List<OperatorTypeDto> OpTypeList { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<OperatorTypeDto> CommTypeList { get; set; }
        public List<OperatorTypeDto> AmtTypeList { get; set; }
        public List<PackCommRangeDto> PackCommRangeList { get; set; }

    }


    public class UserFilterRuleModel
    {
        public UserFilterRuleModel()
        { 
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.UserList = new List<UserDto>();
            this.UserFilterRuleList = new List<UserFilterRuleDto>();
            this.ApiList = new List<ApiDto>();
        }


        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<UserDto> UserList { get; set; }
        public List<UserFilterRuleDto> UserFilterRuleList { get; set; }
        public List<ApiDto> ApiList { get; set; }

    }


    public class UserFilterRuleDto
    {
        public UserFilterRuleDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.UserList = new List<UserDto>();
            this.ApiList = new List<ApiDto>();

        }
        public int Id { get; set; }
        public string OperatorIds { get; set; }
        public string OperatorNames { get; set; }
        public string CircleIds { get; set; }
        public string CircleNames { get; set; }
        public string UserIds { get; set; }
        public string UserNames { get; set; }
        public string VendorIds { get; set; }
        public string VendorNames { get; set; }
        public decimal Amount { get; set; }
        public decimal Roffer { get; set; }
        public decimal Percent { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoBlock { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<UserDto> UserList { get; set; }
        public List<ApiDto> ApiList { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

    }

    public class EditUserFilterRuleDto
    {
        public EditUserFilterRuleDto()
        {
            this.OperatorList = new List<OperatorDto>();
            this.CircleList = new List<CircleDto>();
            this.UserList = new List<UserDto>();
            this.ApiList = new List<ApiDto>();
            
        }
        public int Id { get; set; }
        public List<string> OperatorIds { get; set; }
        public string OperatorNames { get; set; }
        public List<string> CircleIds { get; set; }
        public string CircleNames { get; set; }
        public List<string> UserIds { get; set; }
        public string UserNames { get; set; }
        public List<string> VendorIds { get; set; }
        public string VendorNames { get; set; }
        public decimal Amount { get; set; }
        public decimal Roffer { get; set; }
        public decimal Percent { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoBlock { get; set; }
        public List<OperatorDto> OperatorList { get; set; }
        public List<CircleDto> CircleList { get; set; }
        public List<UserDto> UserList { get; set; }
        public List<ApiDto> ApiList { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }

    }

}
