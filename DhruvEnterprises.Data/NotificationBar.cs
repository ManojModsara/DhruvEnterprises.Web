//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DhruvEnterprises.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class NotificationBar
    {
        public int Id { get; set; }
        public Nullable<byte> RoleId { get; set; }
        public string NotificationMsg { get; set; }
        public string Remark { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> AddedDate { get; set; }
        public Nullable<int> AddedById { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> UpdatedById { get; set; }
    
        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
