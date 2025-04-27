using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class PackageDto
    {
        public PackageDto()
        {
            this.PackageCommList = new List<PackageCommDto>();
        }
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string Range { get; set; }

        public decimal DefaultComm { get; set; }
        public System.DateTime AddedDate { get; set; }
        public Nullable<int> AddedById { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> UpdatedById { get; set; }
        public List<PackageCommDto> PackageCommList { get; set; }

        public int OpId { get; set; }
        public string OperatorName { get; set; }
    }

    public class PackageCommDto
    {
        public long Id { get; set; }
        public int PackId { get; set; }
        public string Range { get; set; }

        public int OpId { get; set; }

        [DisplayName("Vendor Id")]
        public int ApiID { get; set; }

        [DisplayName("Vendor Opcode")]
        public string ApiOpcode { get; set; }
        public string OperatorName { get; set; }
        public string PackageName { get; set; }
        public decimal CommAmt { get; set; }
        public byte AmtTypeId { get; set; }
        public byte CommTypeId { get; set; }

        public string AmtTypeName { get; set; }
        public string CommTypeName { get; set; }

        [DisplayName("Extra Url")]
        public string ExtraUrl { get; set; }

        [DisplayName("Extra Data")]
        public string ExtraData { get; set; }

        public int CircleId { get; set; }
        public string CircleName { get; set; }
        public bool IsCirclePack { get; set; }
        public bool IsuserLoss { get; set; }


        [DisplayName("Daily Limit")]
        public decimal DailyLimit { get; set; }
        [DisplayName("Used Limit")]
        public decimal UsedLimit { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }

        [DisplayName("Max-Q-Size")]
        public int MaxQSize { get; set; }

        [DisplayName("IsAmt?")]
        public bool IsAmtWiseComm { get; set; }


    }

    public class CircleCodeDto
    {
        public long Id { get; set; }
        public int CircleId { get; set; }
        [DisplayName("Circle Name")]
        public string CircleName { get; set; }
        [DisplayName("Vendor Id")]
        public int ApiId { get; set; }
        [DisplayName("Vendor Name")]
        public string CircleCode { get; set; }
        [DisplayName("Circle Code")]
        public string ApiName { get; set; }
        [DisplayName("Package")]
        public int PackId { get; set; }
        [DisplayName("Package Name")]
        public string PackageName { get; set; }
        [DisplayName("Comm Amt")]
        public decimal CommAmt { get; set; }
        [DisplayName("Amt Type")]
        public byte AmtTypeId { get; set; }
        [DisplayName("Comm Type")]
        public byte CommTypeId { get; set; }
        [DisplayName("Amt Type")]
        public string AmtTypeName { get; set; }
        [DisplayName("Comm Type")]
        public string CommTypeName { get; set; }
        [DisplayName("Extra Url")]
        public string ExtraUrl { get; set; }
        [DisplayName("Extra Data")]
        public string ExtraData { get; set; }
        [DisplayName("Daily Limit")]
        public decimal DailyLimit { get; set; }
        [DisplayName("Used Limit")]
        public decimal UsedLimit { get; set; }

    }

    public class CircleCodeModel
    {
        public CircleCodeModel()
        {
            this.CircleCodes = new List<CircleCodeDto>();
        }

        public int ApiId { get; set; }

        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }

        [DisplayName("Package")]
        public int PackId { get; set; }

        [DisplayName("Package Name")]
        public string PackageName { get; set; }

        public List<CircleCodeDto> CircleCodes { get; set; }

    }
}
