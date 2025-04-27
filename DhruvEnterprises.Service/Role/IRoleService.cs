using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DhruvEnterprises.Core;
namespace DhruvEnterprises.Service
{
    public interface IRoleService
    {
        List<Menu> GetMenu();
        List<Menu> GetMenusByRoleId(int roleid);
        List<MapMenuToRole> GetRolePermission(int roleid);
        Menu GetMenu(int id);
        Role Save(Role adminRole);
        Role GetAdminRole(int id);
        bool DeleteRolePermission(int roleid, List<int> editableMenus);
        KeyValuePair<int, List<Role>> GetAdminRoles(DataTableServerSide searchModel);
        bool CheckCurrentMenu(string url, int roleID);
        bool AddRolePermission(List<MapMenuToRole> mapMenuToRoles, int userid);

        List<GlobalSetting> GetActionNames();
        List<GlobalSetting> GetSMSEmailPermission(string actionName);
        bool DeleteGlobalPermission(List<int> editableActions);

        bool AddGlobalPermission(List<GlobalSetting> globalSetting, int userid);
    }
}
