using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class RoleService : IRoleService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Role> repoAdminRole;
        private IRepository<Menu> repoMenu;
        private IRepository<MapMenuToRole> repoMapMenuToRole;
        private IRepository<GlobalSetting> repoSmsEmailPermission;
        #endregion

        #region "Cosntructor"
        public RoleService(IRepository<GlobalSetting> _repoSmsEmailPermission, IRepository<User> _repoUserMaster, IRepository<Role> _repoAdminRole, IRepository<Menu> _repoMenu, IRepository<MapMenuToRole> _repoMapMenuToRole)
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoAdminRole = _repoAdminRole;
            this.repoMenu = _repoMenu;
            this.repoMapMenuToRole = _repoMapMenuToRole;
            this.repoSmsEmailPermission = _repoSmsEmailPermission;

        }
        #endregion

        #region "Action"

        public List<Menu> GetMenusByRoleId(int roleid)
        {
            return Cache.Get<List<Menu>>(CacheKey.AdminMenu + roleid) ??
                Cache.AddOrReplace<List<Menu>>(CacheKey.AdminMenu + roleid, repoMenu.Query().Filter(m => m.MapMenuToRoles.Any(a => a.RoleId == roleid)).Get().ToList(),
                DateTimeOffset.Now.AddDays(30));
        }

        public List<MapMenuToRole> GetRolePermission(int roleid)
        {
            return Cache.Get<List<MapMenuToRole>>(CacheKey.RolePermission + roleid) ??
                Cache.AddOrReplace<List<MapMenuToRole>>(CacheKey.RolePermission + roleid, repoMapMenuToRole.Query().Filter(a => a.RoleId == roleid).Get().ToList(),
                DateTimeOffset.Now.AddDays(30));
        }
        
        public List<Menu> GetMenu()
        {
            return repoMenu.Query().Get().ToList();
        }

        public Menu GetMenu(int id)
        {
            return repoMenu.FindById(id);
        }

        public Role Save(Role adminRole)
        {
            if (adminRole.Id == 0)
            {
                repoAdminRole.Insert(adminRole);
            }
            else
            {
                repoAdminRole.Update(adminRole);
            }
            return adminRole;
        }

        public Role GetAdminRole(int id)
        {
            return repoAdminRole.FindById(id);
        }

        //public Role GetAdminRole(int id)
        //{
        //    return repoAdminRole.Query().Filter(x=>x.Id== Convert.ToByte(id)).Get().FirstOrDefault();
        //}

        public KeyValuePair<int, List<Role>> GetAdminRoles(Core.DataTableServerSide searchModel)
        {
            var predicate = Core.CustomPredicate.BuildPredicate<Role>(searchModel, new Type[] { typeof(Role) });

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Role> results = repoAdminRole
                .Query()
               .Filter(predicate)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Role) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Role>> resultResponse = new KeyValuePair<int, List<Role>>(totalCount, results);

            return resultResponse;
        }

        public bool CheckCurrentMenu(string url, int roleID)
        {

            var parentIds = repoMenu.Query().Filter(M => M.MapMenuToRoles.Any(a => a.RoleId == roleID)).Get()
                            .Select(x => x.ParentId);

            var pages = repoMenu.Query().Filter(M => M.MapMenuToRoles.Any(a => a.RoleId == roleID) ||
             (M.ParentId == 0)).Get().Where(M => parentIds.Contains(M.ParentId) || parentIds.Contains(M.Id))
                 .Select(P => P.Name.ToLower() + (!String.IsNullOrEmpty(P.ChildMenus) ? ("," + P.ChildMenus) : ""))
                .ToList();


            List<string> pageList = new List<string>();
            if (pages.Count > 0)
            {
                foreach (var page in pages)
                {
                    string[] str = page.Trim().Split(',');
                    if (str.Length > 1)
                    {
                        foreach (string value in str)
                        {
                            pageList.Add(value);
                        }
                    }
                    else
                    {
                        pageList.Add(page);
                    }
                }
            }
            return pageList.Any(P => url.ToLower().EndsWith(P));
           
        }

        public bool DeleteRolePermission(int id, List<int> editableMenus)
        {
            var allowedmenus = repoMapMenuToRole.Query().Filter(m => m.RoleId == id).Get().ToList();

            foreach (var menuallowed in allowedmenus)
            {
                if (editableMenus.Any(x => x == menuallowed.MenuId))
                {
                    repoMapMenuToRole.Delete(menuallowed);
                }

            }
            return true;

        }

        public bool AddRolePermission(List<MapMenuToRole> mapMenuToRoles, int userid)
        {

            foreach (var menuallowed in mapMenuToRoles)
            {
                menuallowed.AddedDate = DateTime.Now;
                menuallowed.AddedById = userid;
                repoMapMenuToRole.Insert(menuallowed);
            }
            return true;


        }

        public List<GlobalSetting> GetActionNames()
        {
            return repoSmsEmailPermission.Query().Get().ToList();
        }

        public List<GlobalSetting> GetSMSEmailPermission(string actionName)
        {
            return Cache.Get<List<GlobalSetting>>(CacheKey.RolePermission + actionName) ??
                Cache.AddOrReplace<List<GlobalSetting>>(CacheKey.RolePermission + actionName, repoSmsEmailPermission.Query().Filter(a => a.Actionname == actionName).Get().ToList(),
                DateTimeOffset.Now.AddDays(30));
        }

        public bool DeleteGlobalPermission( List<int> editableActions)
        {
            var allowedmenus = repoSmsEmailPermission.Query().Get().ToList();

            foreach (var menuallowed in allowedmenus)
            {
                if (editableActions.Any(x => x == menuallowed.id))
                {
                    repoSmsEmailPermission.Delete(menuallowed);
                }

            }
            return true;

        }

        public bool AddGlobalPermission(List<GlobalSetting> globalSetting, int userid)
        {

            foreach (var menuallowed in globalSetting)
            {
                menuallowed.UpdatedDate = DateTime.Now;
                menuallowed.UpdatedById = userid;
                repoSmsEmailPermission.Update(menuallowed);
            }
            return true;


        }
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }
            if (repoSmsEmailPermission != null)
            {
                repoSmsEmailPermission.Dispose();
                repoSmsEmailPermission = null;
            }
        }
        #endregion
    }
}
