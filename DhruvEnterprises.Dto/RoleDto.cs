using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class RoleDto
    {
        public int Id { get; set; }

        [DisplayName("Role Name")]
        public string RoleName { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }
    }

    public class ActionAllowedDto 
    { 
        public int RoleId { get; set; }

        public bool AllowView { get; set; }
         
        public bool AllowCreate { get; set; }
        
        public bool AllowEdit { get; set; }

        public bool AllowDelete { get; set; } 


    }

    public class GlobalSettingAllowedDto
    {
       

        public bool AllowEmail { get; set; }

        public bool AllowSms { get; set; }

       


    }
}
