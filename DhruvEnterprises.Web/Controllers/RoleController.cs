using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.Models.Others;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static DhruvEnterprises.Core.Enums;

namespace DhruvEnterprises.Web.Controllers
{
   
    
    public class RoleController : BaseController
    {
        public ActionAllowedDto action;
        private IRoleService roleService;
        ActivityLogDto activityLogModel;

        public RoleController(IRoleService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService) : base(_activityLogService, _roleService)
        {
            this.roleService = _adminUserService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();

        }

        // GET: Admin/AdminUser
        public ActionResult Index()
        {
            UpdateActivity("Roles List REQUEST", "GET:Role/Index/", string.Empty);
            action = ActionAllowed("Role", CurrentUser.RoleId);
            return View();
        }

        [HttpPost]
        public ActionResult GetRoles(DataTableServerSide model)
        {
            ViewBag.actionAllowed= action =ActionAllowed("Role", CurrentUser.RoleId);

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;
            
            KeyValuePair<int, List<Role>> roles = roleService.GetAdminRoles(model);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = roles.Key,
                recordsFiltered = roles.Key,
                data = roles.Value.Where(x=>(userrole==1?true: (x.Id != userrole && x.Id!=1))).Select(c => new List<object> {
                    c.Id,
                    c.RoleName,

                   (action.AllowEdit? DataTableButton.EditButton(Url.Action( "createedit", "role",new { id = c.Id }),"modal-add-edit-adminrole"):string.Empty )
                    +"&nbsp;"+
                   (action.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","role", new { id = c.Id }),"modal-delete-adminrole"):string.Empty)
                   +"&nbsp;"+
                     (action.AllowDelete? DataTableButton.SettingButton(Url.Action( "permission","role", new { id = c.Id }),"Permission"):string.Empty)
                })
            }, JsonRequestBehavior.AllowGet);

        }
       
        public ActionResult CreateEdit(int? id)
        {
            UpdateActivity("Role Create/Edit REQUEST", "GET:Role/CreateEdit/" + id, "RoleId="+id);
            action = ActionAllowed("Role", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            RoleDto roleDto = new RoleDto();
            
            if (id.HasValue && id.Value > 0)
            {
                Role role = roleService.GetAdminRole(id.Value);
                roleDto.Id = role.Id;
                roleDto.RoleName = role.RoleName;
            }

            return PartialView("_createedit", roleDto);

        }

        [HttpPost]
        public ActionResult CreateEdit(RoleDto model, FormCollection FC)
        {
            string message = string.Empty;

            UpdateActivity("Role Create/Edit REQUEST", "POST:Role/CreateEdit/", "RoleId=" + model.Id);
            action = ActionAllowed("Role", CurrentUser.RoleId, model.Id>0 ? 3 : 2);

            try
            {
                if (ModelState.IsValid)
                {
                    Role role = roleService.GetAdminRole(model.Id) ?? new Role();

                    role.Id = (byte)model.Id;
                    role.RoleName = model.RoleName;
                    roleService.Save(role);

                    ShowSuccessMessage("Success!", "Role has been saved", false);

                   // return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("Index") });


                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Role already exist.";
                    ShowErrorMessage("Error!", message, false);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error!", message, false);
                }
            }
            // return CreateModelStateErrors();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            UpdateActivity("Role DELETE REQUEST", "GET:Role/DELETE/" + id, "roleid="+id);
            action = ActionAllowed("Role", CurrentUser.RoleId,4);

            return PartialView("_ModalDelete", new Modal
            {
                Message = "Are you sure you want to delete this role?",
                Size = ModalSize.Small,
                Header = new ModalHeader { Heading = "Delete Role" },
                Footer = new ModalFooter { SubmitButtonText = "Yes", CancelButtonText = "No" }
            });
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteAdminUser(int id)
        {
            UpdateActivity("Role DELETE REQUEST", "POST:Role/DELETE/" + id, "roleid="+id);
            action = ActionAllowed("Role", CurrentUser.RoleId, 4);
            try
            {
                //var adminUser = adminUserService.GetAdminUser(id);
                //adminUser.IsActive = false;
                //adminUserService.Save(adminUser);
                ShowSuccessMessage("Success","Role cann't be deleted.", false);
                //ShowSuccessMessage("Success", "hola", false);
            }
            catch (Exception)
            {
                //ShowErrorMessage("Error Occurred", "", false);
            }
            return RedirectToAction("index");
        }

        public bool Active(int id)
        {
            UpdateActivity("Role Active/Inactive REQUEST", "GET:Role/Active/" + id, string.Empty);
            action = ActionAllowed("Role", CurrentUser.RoleId, 3);

            //if (!actionAllowedDto.AllowEdit)
            //{
            //    return false;
            //}
            //else
            //{
            //    string message = string.Empty;
            //    try
            //    {
            //        var adminUser = RoleUserService.GetAdminUser(id);
            //        adminUser.IsActive = !adminUser.IsActive;
            //        return RoleUserService.Save(adminUser).IsActive;

            //    }
            //    catch (Exception)
            //    {
            //        return false;
            //    }
            //}
            return false;
        }

        [HttpGet]
        public ActionResult Permission(int id)
        {
            UpdateActivity("Role Permission REQUEST", "GET:Role/Permission/", "roleid="+id);
            action = ActionAllowed("Role", CurrentUser.RoleId, id > 0 ? 3 : 2);

            int userroleid = CurrentUser.RoleId; 
             
            PermissionDto permissionDto = new PermissionDto();
            MenuDto menuDto = new MenuDto();

            Role role = id > 0 ? roleService.GetAdminRole(id) : new Role();

            List<Menu> menuList;

            if (userroleid == 1)
            {
                menuList = roleService.GetMenu().OrderBy(x => x.Priority).Where(x => x.IsActive.Equals(true)).ToList();
            }
            else
            {
                menuList= roleService.GetMenusByRoleId(userroleid).OrderBy(x=>x.Priority).ToList();
            }
           
            
            MenuMapDto menuMapDto;

            if (role != null && menuList!=null)
            {
                try
                {
                    List<MapMenuToRole> allowedmenus = role.MapMenuToRoles.Where(x => x.RoleId == id).ToList();
                    List<int> menuids = new List<int>();

                    foreach (var menu in menuList)
                    {
                        menuDto = new MenuDto();
                        menuDto.Id = menu.Id;
                        menuDto.Name = menu.Name;
                        menuDto.MenuId = menu.MenuId;
                        menuDto.ChildMenus = menu.ChildMenus;
                        menuDto.ParentId = menu.ParentId ?? 0;
                        permissionDto.MenuList.Add(menuDto);

                        menuMapDto = new MenuMapDto();
                        menuMapDto.RoleId = id;
                        menuMapDto.MenuId = menu.Id;
                        menuMapDto.IsCreate = false;
                        menuMapDto.IsEdit = false;
                        menuMapDto.IsDelete = false;

                        foreach (var allowedmenu in allowedmenus)
                        {
                            if (menu.Id == allowedmenu.MenuId)
                            {
                                menuids.Add(allowedmenu.MenuId);

                                menuMapDto.IsCreate = allowedmenu.AllowCreate;
                                menuMapDto.IsEdit = allowedmenu.AllowUpdate;
                                menuMapDto.IsDelete = allowedmenu.AllowDelete;
                            }
                        }

                        permissionDto.MenuMapList.Add(menuMapDto);
                    }
                    permissionDto.RoleName = role.RoleName;
                    permissionDto.MenuIds = menuids;
                    permissionDto.CurrentRoleId = id;
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            return View(permissionDto);
        }

        [HttpPost]
        public bool Permission(List<MenuMapDto> data)
        {

            UpdateActivity("Role Permission REQUEST", "POST:Role/Permission/", "roleid="+data.FirstOrDefault()?.RoleId);
            action = ActionAllowed("Role", CurrentUser.RoleId, 3);
            string message = string.Empty;
            var Roleid=0;
            try
            {
                List<int> editableMenus = data.Select(x => x.MenuId).ToList();
                Roleid = data.FirstOrDefault().RoleId;
                roleService.DeleteRolePermission(Roleid, editableMenus);
                List<MapMenuToRole> menulist = new List<MapMenuToRole>();
                foreach (var menuallwed in data.Where(x => x.IsMenuAllow).ToList())
                {
                    MapMenuToRole menu = new MapMenuToRole();
                    menu.RoleId = (byte)menuallwed.RoleId;
                    menu.MenuId = menuallwed.MenuId;
                    menu.AllowCreate = menuallwed.IsCreate;
                    menu.AllowDelete = menuallwed.IsDelete;
                    menu.AllowUpdate = menuallwed.IsEdit;
                    menulist.Add(menu);
                }
                roleService.AddRolePermission(menulist, CurrentUser.UserID);
                ShowSuccessMessage("Success!", "Menu Permission has been saved", true);
                Cache.RemoveAll();
                return true; // RedirectToAction("Permission", new { id = Roleid });
            }
            catch (Exception ex)
            {
                LogException(ex);
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, true);
                return false;
            }
            //RedirectToAction("Permission", new { id = Roleid });
        }

        private void UpdateActivity(string activityName, string ativityPage,  string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = activityName;
                activityLogModel.ActivityPage = ativityPage;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }

    }
}