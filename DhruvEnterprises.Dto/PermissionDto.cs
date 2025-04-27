using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class PermissionDto
    {
        public PermissionDto()
        {
            this.MenuList = new List<MenuDto>();
            this.MenuMapList = new List<MenuMapDto>();
        }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int CurrentRoleId { get; set; }
        public List<int> MenuIds { get; set; } 
        public List<MenuDto> MenuList { get; set; }
        public List<MenuMapDto> MenuMapList { get; set; } 
    }


   public  class MenuMapDto
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public bool IsMenuAllow { get; set; }
        public string MenuName { get; set; }
        public bool IsCreate { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }

    }


    public class MenuDto 
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string MenuId { get; set; }
        public string ChildMenus { get; set; }

    }
}
