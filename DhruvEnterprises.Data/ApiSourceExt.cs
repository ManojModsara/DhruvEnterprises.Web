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
    
    public partial class ApiSourceExt
    {
        public int Id { get; set; }
        public Nullable<int> ApiId { get; set; }
        public Nullable<int> AmountUnitId { get; set; }
        public Nullable<int> AmountLength { get; set; }
        public string DateTimeFormat { get; set; }
        public string RefPadding { get; set; }
        public Nullable<int> RefLength { get; set; }
        public Nullable<int> IsNumericOnly { get; set; }
    
        public virtual ApiSource ApiSource { get; set; }
    }
}
