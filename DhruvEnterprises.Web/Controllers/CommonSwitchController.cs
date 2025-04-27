using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{

    public class CommonSwitchController : BaseController
    {

        #region "Properties"
        public ActionAllowedDto action;
        private IOperatorSwitchService operatorSwitchService;
        private IApiService apiService;
        private IUserService userService;
        ActivityLogDto activityLogModel;
        private ICommonSwitchService commonSwitchService;
        #endregion

        #region "Constructor" 
        public CommonSwitchController
            (
             IOperatorSwitchService _operatorSwitchService,
             IApiService _apiService,
             IActivityLogService _activityLogService,
             IRoleService _roleService,
             ICommonSwitchService _commonSwitchService,
             IUserService _userService
            ) : base(_activityLogService, _roleService)
        {
            this.operatorSwitchService = _operatorSwitchService;
            this.apiService = _apiService;
            this.commonSwitchService = _commonSwitchService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
            this.userService = _userService;
        }
        #endregion

        #region "COMMON SWITCH"
        public ActionResult Index()
        {
            UpdateActivity("CommonSwitch REQUEST", "GET:CommonSwitch/Index/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId);

            CommonSwitchDto commonSwitchDto = new CommonSwitchDto();
            try
            {

                var oplist = commonSwitchService.GetOperatorList();
                var circlelist = commonSwitchService.GetCircleList();
                var apilist = apiService.GetApiList();
                var clist = commonSwitchService.GetCommanRoutings();
                var flist = commonSwitchService.GetFilterTypeList();
                var userlist = userService.GetUserList(3);

                if (clist.Count > 0)
                {

                    var routelist = clist.Select(x => new SwitchDto
                    {
                        AmountFilter = x.AmountFilter,
                        ApiId = x.ApiId ?? 0,
                        ApiName = x.ApiSource.ApiName,
                        CircleId = x.CircleFilter,
                        CircleName = "",
                        Id = x.Id,
                        FilterTypeId = x.FTypeId ?? 0,
                        FilterTypeName = x.FilterType?.TypeName,
                        OperatorId = x.OpId ?? 0,
                        OperatorName = x.Operator.Name,
                        Priority = x.Priority ?? 0,
                        BlockUser = x.BlockUser,
                        MinRO = x.MinRO ?? 0,
                        UpdatedOn = x.UpdatedDate?.ToString() ?? x.AddedDate.ToString(),
                        UpdatedBy = x.User1?.UserProfile?.FullName ?? x.User?.UserProfile?.FullName,
                        LapuIds = !string.IsNullOrWhiteSpace(x.LapuFilter) ? x.LapuFilter.Split(',').Select(s => Convert.ToInt64(s.Trim())).ToArray() : new long[] { 0 },
                        UserId = x.UserFilter,
                        UserName = "",
                        RouteOP1 = x.RouteOP1,
                        RouteOP2 = x.RouteOP2,
                        IsActive = x.IsActive ?? false,
                        ActiveUpdatedOn = x.ActiveUpdatedDate?.ToString() ?? string.Empty,
                        ActiveUpdatedBy = x.User2?.UserProfile?.FullName ?? string.Empty
                    }).ToList();

                    if (routelist.Count > 0)
                    {

                        foreach (var item in routelist.Where(x => x.CircleId != null))
                        {
                            if (item.CircleId.ToUpper().Contains("ALL"))
                            {
                                item.CircleName = "All";
                                item.CircleId = "All";
                            }
                            else
                            {
                                string[] arr = item.CircleId.Split(',');

                                foreach (var c in arr)
                                {
                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        var id = Convert.ToInt32(c.Trim());

                                        if (id > 0)
                                        {
                                            var cname = circlelist.Where(x => x.Id == id).FirstOrDefault()?.CircleName;
                                            if (!string.IsNullOrEmpty(cname))
                                            {
                                                item.CircleName = item.CircleName + (string.IsNullOrEmpty(item.CircleName) ? "" : ",") + cname;
                                            }
                                        }
                                    }

                                }

                            }
                        }

                    }

                    if (routelist.Count > 0)
                    {

                        foreach (var item in routelist.Where(x => x.UserId != null))
                        {
                            if (item.UserId.ToUpper().Contains("ALL"))
                            {
                                item.UserName = "All";
                                item.UserId = "All";
                            }
                            else
                            {
                                string[] arr = item.UserId.Split(',');

                                foreach (var u in arr)
                                {
                                    if (!string.IsNullOrEmpty(u))
                                    {
                                        var id = Convert.ToInt32(u.Trim());

                                        if (id > 0)
                                        {
                                            var uname = userlist.Where(x => x.Id == id).FirstOrDefault()?.UserProfile?.FullName ?? string.Empty;
                                            if (!string.IsNullOrEmpty(uname))
                                            {
                                                item.UserName = item.UserName + (string.IsNullOrEmpty(item.UserName) ? "" : ",") + uname;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    commonSwitchDto.CommonSwitchList = routelist.OrderBy(x => x.OperatorId).ThenBy(x => x.Priority).ToList();

                }

                commonSwitchDto.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
                commonSwitchDto.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
                commonSwitchDto.ApiList = apilist.Select(x => new ApiSourceDto { ApiId = x.Id, ApiName = x.ApiName }).ToList();
                commonSwitchDto.FilterTypeList = flist.Where(f => f.Id > 2).Select(x => new FilterTypeDto { Id = x.Id, TypeName = x.TypeName }).ToList();
                commonSwitchDto.UserList = userlist.Select(x => new UserDto { Id = x.Id, Uid = x.Id, Username = x.Username, Name = x.UserProfile?.FullName ?? string.Empty }).ToList();

                List<SelectListItem> lapulist = new List<SelectListItem>();
                lapulist.Add(new SelectListItem() { Value = "0", Text = "Select Lapu" });
                ViewBag.LapuList = lapulist;
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return View(commonSwitchDto);
        }

        [HttpPost]
        public string CommonSwitchSetting(List<SwitchDto> data)
        {
            UpdateActivity("CommonSwitchSetting REQUEST", "GET:CommonSwitch/CommonSwitchSetting/", "data.count=" + data.Count);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, data.Count > 0 ? 3 : 2);

            try
            {
                List<CommanRouting> cList = new List<CommanRouting>();

                var item = data.FirstOrDefault();

                //bool isexists = commonSwitchService.GetCommanRoutings().Any(x => x.OpId.Value == item.OperatorId && x.Priority.Value == item.Priority);

                //if (isexists)
                //{
                //    return "3";
                //}
                //else 

                if (!ValidateRange(item.AmountFilter, item.FilterTypeId))
                {
                    return "4";
                }
                else
                {
                    if (string.IsNullOrEmpty(item.CircleName) || item.CircleName.ToUpper().Contains("ALL"))
                    {
                        item.CircleName = "All";
                        item.CircleId = "All";
                    }

                    if (string.IsNullOrEmpty(item.UserName) || item.UserName.ToUpper().Contains("ALL"))
                    {
                        item.UserName = "All";
                        item.UserId = "All";
                    }

                    if (!string.IsNullOrEmpty(item.CircleId))
                    {
                        CommanRouting cRouting = new CommanRouting();
                        cRouting.Id = item.Id;
                        cRouting.OpId = item.OperatorId;
                        cRouting.CircleFilter = item.CircleId;
                        cRouting.ApiId = item.ApiId;
                        cRouting.Priority = item.Priority;
                        cRouting.FTypeId = item.FilterTypeId;
                        cRouting.AmountFilter = item.AmountFilter;
                        cRouting.BlockUser = item.BlockUser;
                        cRouting.MinRO = item.MinRO;
                        cRouting.AddedById = CurrentUser.UserID;
                        //cRouting.LapuFilter = item.LapuIds.Count() > 0 ? string.Join(",", item.LapuIds.Where(s => s > 0)) : string.Empty;
                        cRouting.UserFilter = item.UserId;
                        cRouting.RouteOP1 = item.RouteOP1;
                        cRouting.RouteOP2 = item.RouteOP2;
                        cRouting.IsActive = true;
                        cList.Add(cRouting);
                    }

                    commonSwitchService.Save(cList);
                    return "0";
                }

            }
            catch (Exception ex)
            {
                LogException(ex);
                return "2";
            }

        }

        [HttpPost]
        public bool DeleteRoute(int Id)
        {
            UpdateActivity("DeleteRoute REQUEST", "POST:CommonSwitch/DeleteRoute/", "Id=" + Id);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, 4);
            try
            {
                return commonSwitchService.Delete(Id);

            }
            catch (Exception)
            {
                return false;
            }

        }
        public ActionResult EditSwitchSetting(int? id)
        {
            UpdateActivity("EditSwitchSetting REQUEST", "GET:CommonSwitch/EditSwitchSetting/", "id=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            EditSwitchDto model = new EditSwitchDto();

            try
            {
                ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.Roles.FirstOrDefault());

                var oplist = commonSwitchService.GetOperatorList();
                var circlelist = commonSwitchService.GetCircleList();
                var apilist = apiService.GetApiList();
                var cRoute = commonSwitchService.GetCommanRouting(id ?? 0);
                var userlist = userService.GetUserList(3);

                model = new EditSwitchDto
                {
                    AmountFilter = cRoute.AmountFilter,
                    apiId = cRoute.ApiId ?? 0,
                    ApiName = cRoute.ApiSource.ApiName,
                    CircleIds = cRoute.CircleFilter.Replace(" ", "").Split(',').ToList(),
                    CircleName = "",
                    Id = cRoute.Id,
                    FilterTypeId = cRoute.FTypeId ?? 0,
                    FilterTypeName = cRoute.FilterType?.TypeName,
                    OperatorId = cRoute.OpId ?? 0,
                    OperatorName = cRoute.Operator.Name,
                    Priority = cRoute.Priority ?? 0,
                    BlockUser = cRoute.BlockUser,
                    UserIds = cRoute.UserFilter.Replace(" ", "").Split(',').ToList(),
                    UserName = "",
                    MinRO = cRoute.MinRO ?? 0,
                    LapuIds = !string.IsNullOrWhiteSpace(cRoute.LapuFilter) ? cRoute.LapuFilter.Split(',').Select(s => Convert.ToInt64(s.Trim())).ToArray() : new long[] { 0 },
                    RouteOP1 = cRoute.RouteOP1,
                    RouteOP2 = cRoute.RouteOP2

                };

                var ftList = commonSwitchService.GetFilterTypeList();

                if (model.CircleIds.Any(x => x.ToUpper().Contains("ALL")))
                {
                    model.CircleName = "All";
                    model.CircleIds.Clear();
                    model.CircleIds.Add("0");
                }
                else
                {
                    string[] arr = model.CircleIds.ToArray();

                    foreach (var c in arr)
                    {
                        if (!string.IsNullOrEmpty(c))
                        {
                            var id1 = Convert.ToInt32(c.Trim());

                            if (id1 > 0)
                            {
                                var cname = circlelist.Where(x => x.Id == id1).FirstOrDefault()?.CircleName;
                                if (!string.IsNullOrEmpty(cname))
                                {
                                    model.CircleName = model.CircleName + (string.IsNullOrEmpty(model.CircleName) ? "" : ",") + cname;
                                }
                            }
                        }
                    }
                }


                if (model.UserIds.Any(x => x.ToUpper().Contains("ALL")))
                {
                    model.UserName = "All";
                    model.UserIds.Clear();
                    model.UserIds.Add("0");
                }
                else
                {
                    string[] arr = model.UserIds.ToArray();

                    foreach (var u in arr)
                    {
                        if (!string.IsNullOrEmpty(u))
                        {
                            var uid = Convert.ToInt32(u.Trim());

                            if (uid > 0)
                            {
                                var uname = userlist.Where(x => x.Id == uid).FirstOrDefault()?.UserProfile?.FullName ?? string.Empty;
                                if (!string.IsNullOrEmpty(uname))
                                {
                                    model.UserName = model.UserName + (string.IsNullOrEmpty(model.UserName) ? "" : ",") + uname;
                                }
                            }
                        }
                    }
                }

                model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
                model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
                model.CircleList.Add(new CircleDto() { CircleId = 0, CircleName = "All" });
                model.CircleList = model.CircleList.OrderBy(x => x.CircleName).ToList();
                model.ApiList = apilist.Select(x => new ApiSourceDto { ApiId = x.Id, ApiName = x.ApiName }).OrderBy(y => y.ApiName).ToList();
                model.FilterTypeList = ftList.Where(f => f.Id > 2).Select(x => new FilterTypeDto { Id = x.Id, TypeName = x.TypeName }).OrderBy(y => y.TypeName).ToList();

                var cIds = !string.IsNullOrEmpty(cRoute.CircleFilter) && cRoute.CircleFilter?.Split(',')?.Count() > 0 ? cRoute.CircleFilter.Split(',').Where(s => !s.ToLower().Contains("all") && !string.IsNullOrEmpty(s)).Select(x => Convert.ToInt32(x)).ToArray() : new int[0];

                model.UserList = userlist.Select(x => new UserDto { Id = x.Id, Uid = x.Id, Username = x.Username, Name = x.UserProfile?.FullName ?? string.Empty }).ToList();
                model.UserList.Add(new UserDto() { Id = 0, Uid = 0, Username = "All", Name = "All" });
                model.UserList = model.UserList.OrderBy(x => x.Name).ToList();


                //var lapulist = lapuDealerService.GetLapuListByApi(model.apiId, model.OperatorId, cIds).Select(x => new SelectListItem()
                //{
                //    Value = x.Id.ToString(),
                //    Text = x.Id.ToString() + " -" + x.Number.ToString(),
                //    Selected = model.LapuIds.Any(p => p == x.Id)
                //});

                //ViewBag.LapuList = lapulist;
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return PartialView("_EditSwitchSetting", model);

        }

        [HttpPost]
        public ActionResult EditSwitchSetting(EditSwitchDto model, FormCollection FC)
        {
            string message = string.Empty;
            UpdateActivity("EditSwitchSetting REQUEST", "POST:CommonSwitch/EditSwitchSetting/", "model.id=" + model.Id);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);
            try
            {
                if (ModelState.IsValid)
                {
                    var selectedCircles = model.CircleIds.FirstOrDefault();
                    var selectedUsers = model.UserIds.FirstOrDefault();

                    var cRoute = commonSwitchService.GetCommanRoutings().Where(x => x.Id == model.Id).FirstOrDefault();
                    if (cRoute != null)
                    {
                        cRoute.ApiId = model.apiId;
                        cRoute.OpId = model.OperatorId;
                        cRoute.ApiId = model.apiId;
                        cRoute.Priority = model.Priority;
                        cRoute.FTypeId = model.FilterTypeId;
                        cRoute.AmountFilter = model.AmountFilter;
                        cRoute.BlockUser = model.BlockUser;
                        cRoute.MinRO = model.MinRO;
                        cRoute.UpdatedById = CurrentUser.UserID;
                        cRoute.CircleFilter = selectedCircles.Split(',').Any(s => s == "0") ? "All" : selectedCircles;
                        cRoute.UserFilter = selectedUsers.Split(',').Any(s => s == "0") ? "All" : selectedUsers;
                        if (model.LapuIds != null)
                            cRoute.LapuFilter = model.LapuIds.Count() > 0 ? string.Join(",", model.LapuIds.Where(s => s > 0)) : string.Empty;

                        cRoute.RouteOP1 = model.RouteOP1;
                        cRoute.RouteOP2 = model.RouteOP2;

                        commonSwitchService.Save(cRoute);
                    }
                }

                ShowSuccessMessage("Error!", "Data has been saved successfully!", false);

            }
            catch (Exception Ex)
            {
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, false);
            }
            // return CreateModelStateErrors();

            return RedirectToAction("Index");
        }

        #endregion

        #region "BLOCK ROUTE"

        public ActionResult BlockRoute()
        {
            UpdateActivity("BlockRoute", "GET:CommonSwitch/BlockRoute/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId);

            BlockRouteModel model = new BlockRouteModel();
            var oplist = commonSwitchService.GetOperatorList();
            var circlelist = commonSwitchService.GetCircleList();
            var userlist = userService.GetUserList(3);
            var optypelist = operatorSwitchService.GetOperatorType();


            try
            {
                var blockroutelist = commonSwitchService.GetBlockRoutes();

                if (blockroutelist.Count > 0)
                {
                    var clist1 = blockroutelist.Select(x => new BlockRouteDto
                    {
                        Id = x.Id,
                        OperatorIds = x.OpFilter,
                        CircleIds = x.CircleFilter,
                        UserIds = x.UserFilter,
                        Amounts = x.Amounts + (!string.IsNullOrEmpty(x.Amounts) && !string.IsNullOrEmpty(x.AmountRanges) ? ("," + x.AmountRanges) : x.AmountRanges),
                        TypeId = x.SettingTypeId ?? 0,
                        StatusId = x.SendStatusId ?? 0,
                        UpdatedOn = x.UpdatedDate?.ToString() ?? x.AddedDate.ToString(),
                        UpdatedBy = x.User1?.UserProfile?.FullName ?? x.User?.UserProfile?.FullName,
                        OpTypeIds = x.OpTypeFilter
                    }).ToList();
                    if (clist1.Count > 0)
                    {
                        foreach (var item in clist1)
                        {
                            if (item.OperatorIds?.ToUpper()?.Contains("ALL") ?? true)
                            {
                                item.OperatorNames = "All";
                                item.OperatorIds = "All";
                            }
                            else
                            {
                                string[] arr = item.OperatorIds.Split(',');

                                foreach (var c in arr)
                                {
                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        var id = Convert.ToInt32(c.Trim());

                                        if (id > 0)
                                        {
                                            var opname = oplist.Where(x => x.Id == id).FirstOrDefault()?.Name;
                                            if (!string.IsNullOrEmpty(opname))
                                            {
                                                item.OperatorNames = item.OperatorNames + (string.IsNullOrEmpty(item.OperatorNames) ? "" : ",") + opname;
                                            }
                                        }
                                    }

                                }

                            }

                            if (item.CircleIds?.ToUpper()?.Contains("ALL") ?? true)
                            {
                                item.CircleNames = "All";
                                item.CircleIds = "All";
                            }
                            else
                            {
                                string[] arr = item.CircleIds.Split(',');

                                foreach (var c in arr)
                                {
                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        var id = Convert.ToInt32(c.Trim());

                                        if (id > 0)
                                        {
                                            var cname = circlelist.Where(x => x.Id == id).FirstOrDefault()?.CircleName;
                                            if (!string.IsNullOrEmpty(cname))
                                            {
                                                item.CircleNames = item.CircleNames + (string.IsNullOrEmpty(item.CircleNames) ? "" : ",") + cname;
                                            }
                                        }
                                    }

                                }

                            }

                            if (item.UserIds?.ToUpper()?.Contains("ALL") ?? true)
                            {
                                item.UserNames = "All";
                                item.UserNames = "All";
                            }
                            else
                            {
                                string[] arr = item.UserIds.Split(',');

                                foreach (var c in arr)
                                {
                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        var id = Convert.ToInt32(c.Trim());

                                        if (id > 0)
                                        {
                                            var uname = userlist.Where(x => x.Id == id).FirstOrDefault()?.UserProfile.FullName;
                                            if (!string.IsNullOrEmpty(uname))
                                            {
                                                item.UserNames = item.UserNames + (string.IsNullOrEmpty(item.UserNames) ? "" : ",") + uname;
                                            }
                                        }
                                    }

                                }

                            }

                            if (item.OpTypeIds?.ToUpper()?.Contains("ALL") ?? true)
                            {
                                item.OpTypeNames = "All";
                                item.OpTypeIds = "All";
                            }
                            else
                            {
                                string[] arr = item.OpTypeIds.Split(',');

                                foreach (var c in arr)
                                {
                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        var id = Convert.ToInt32(c.Trim());

                                        if (id > 0)
                                        {
                                            var optypename = optypelist.Where(x => x.Id == id).FirstOrDefault()?.TypeName;
                                            if (!string.IsNullOrEmpty(optypename))
                                            {
                                                item.OpTypeNames = item.OpTypeNames + (string.IsNullOrEmpty(item.OpTypeNames) ? "" : ",") + optypename;
                                            }
                                        }
                                    }

                                }

                            }
                        }

                    }
                    model.BlockRouteList = clist1;
                }

                model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OperatorName = x.Name }).ToList();
                //model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
                model.UserList = userlist.Select(x => new UserDto { Uid = x.Id, Name = x.UserProfile?.FullName ?? x.Username }).ToList();
                /*model.OpTypeList*//* = optypelist.Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.TypeName }).ToList();*/

            }
            catch (Exception e)
            {
                LogException(e);
            }

            return View(model);
        }

        [HttpPost]
        public string BlockRoute(List<BlockRouteDto> data)
        {
            int id = 0;
            try
            {
                var amts = "";
                var amtRanges = "";
                List<BlockRoute> cList = new List<BlockRoute>();

                var item = data.FirstOrDefault();

                if (string.IsNullOrEmpty(item.CircleNames) || item.CircleNames.ToUpper().Contains("ALL"))
                {
                    item.CircleNames = "All";
                    item.CircleIds = "All";
                }

                if (string.IsNullOrEmpty(item.OperatorNames) || item.OperatorNames.ToUpper().Contains("ALL"))
                {
                    item.OperatorNames = "All";
                    item.OperatorIds = "All";
                }

                if (string.IsNullOrEmpty(item.UserNames) || item.UserNames.ToUpper().Contains("ALL"))
                {
                    item.UserNames = "All";
                    item.UserIds = "All";
                }

                if (string.IsNullOrEmpty(item.OpTypeNames) || item.OpTypeNames.ToUpper().Contains("ALL"))
                {
                    item.OpTypeNames = "All";
                    item.OpTypeIds = "All";
                }

                BlockRoute blockroute = new BlockRoute();
                blockroute.Id = item.Id;
                blockroute.OpFilter = item.OperatorIds;
                blockroute.CircleFilter = item.CircleIds;
                blockroute.UserFilter = item.UserIds;
                blockroute.SettingTypeId = item.TypeId == 0 ? 1 : item.TypeId;
                blockroute.SendStatusId = blockroute.SettingTypeId == 1 ? 3 : item.StatusId;
                blockroute.OpTypeFilter = item.OpTypeIds;

                if (!string.IsNullOrEmpty(item.Amounts))
                {
                    var amountslist = item.Amounts.Split(',').ToList();
                    if (item.Amounts.Contains("-"))
                    {
                        foreach (var amt in amountslist)
                        {
                            var amount = amt.Trim();
                            if (!string.IsNullOrEmpty(amount))
                            {
                                if (amount.Contains("-"))
                                {
                                    amtRanges = amtRanges + (!string.IsNullOrEmpty(amtRanges) ? "," : "") + amount;
                                }
                                else
                                {
                                    amts = amts + (!string.IsNullOrEmpty(amts) ? "," : "") + amount;
                                }
                            }

                        }
                    }

                    blockroute.Amounts = !string.IsNullOrEmpty(amts) ? amts : blockroute.Amounts;
                    blockroute.AmountRanges = !string.IsNullOrEmpty(amtRanges) ? amtRanges : blockroute.AmountRanges;

                }

                blockroute.AddedById = CurrentUser.UserID;
                commonSwitchService.Save(blockroute);
                id = blockroute.Id;

                return id.ToString();

            }
            catch (Exception ex)
            {
                LogException(ex);
                return "0";
            }

        }

        [HttpPost]
        public bool DeleteBlockRoute(int Id)
        {
            UpdateActivity("BlockRoute", "POST:CommonSwitch/DeleteBlockRoute/", "Id=" + Id);
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, 4);

            try
            {
                return commonSwitchService.DeleteBlockRoute(Id);

            }
            catch (Exception)
            {
                return false;
            }

        }

        public ActionResult EditBlockRoute(int? id)
        {
            //UpdateActivity("EditSwitchSetting REQUEST", "GET:CommonSwitch/EditSwitchSetting/", "id=" + id);
            //ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            EditBlockRouteDto model = new EditBlockRouteDto();

            try
            {
                ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.Roles.FirstOrDefault());

                var oplist = commonSwitchService.GetOperatorList();
                var circlelist = commonSwitchService.GetCircleList();
                var userlist = userService.GetUserList(3).ToList();

                var bRoute = commonSwitchService.GetBlockRouteById(id ?? 0);

                model = new EditBlockRouteDto
                {
                    Id = bRoute.Id,
                    OperatorIds = bRoute.OpFilter.Replace(" ", "").Split(',').ToList(),
                    CircleIds = bRoute.CircleFilter.Replace(" ", "").Split(',').ToList(),
                    UserIds = bRoute.UserFilter.Replace(" ", "").Split(',').ToList(),
                    Amounts = bRoute.Amounts + (!string.IsNullOrEmpty(bRoute.Amounts) && !string.IsNullOrEmpty(bRoute.AmountRanges) ? ("," + bRoute.AmountRanges) : bRoute.AmountRanges),
                    TypeId = bRoute.SettingTypeId ?? 0,
                    StatusId = bRoute.SendStatusId ?? 0,
                    UpdatedOn = bRoute.UpdatedDate?.ToString() ?? bRoute.AddedDate.ToString(),
                    UpdatedBy = bRoute.User1?.UserProfile?.FullName ?? bRoute.User?.UserProfile?.FullName,
                    OpTypeIds = (bRoute?.OpTypeFilter ?? "All").Replace(" ", "").Split(',').ToList()
                };


                if (model.OperatorIds.Any(x => x.ToUpper().Contains("ALL")))
                {
                    model.OperatorNames = "All";
                    model.OperatorIds.Clear();
                    model.OperatorIds.Add("0");
                }


                if (model.CircleIds.Any(x => x.ToUpper().Contains("ALL")))
                {
                    model.CircleNames = "All";
                    model.CircleIds.Clear();
                    model.CircleIds.Add("0");
                }


                if (model.UserIds.Any(x => x.ToUpper().Contains("ALL")))
                {
                    model.UserNames = "All";
                    model.UserIds.Clear();
                    model.UserIds.Add("0");
                }

                if (model.OpTypeIds.Any(x => x.ToUpper().Contains("ALL")))
                {
                    model.OpTypeNames = "All";
                    model.OpTypeIds.Clear();
                    model.OpTypeIds.Add("0");
                }

                model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
                model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
                model.UserList = userlist.Select(x => new UserDto { Uid = x.Id, Name = x.UserProfile?.FullName ?? x.Username }).ToList();

                model.OperatorList.Add(new OperatorDto() { OperatorId = 0, OperatorName = "All" });
                model.CircleList.Add(new CircleDto() { CircleId = 0, CircleName = "All" });
                model.UserList.Add(new UserDto() { Uid = 0, Name = "All" });

                //  var cIds = !string.IsNullOrEmpty(bRoute.CircleFilter) && bRoute.CircleFilter?.Split(',')?.Count() > 0 ? bRoute.CircleFilter.Split(',').Where(s => !s.ToLower().Contains("all") && !string.IsNullOrEmpty(s)).Select(x => Convert.ToInt32(x)).ToArray() : new int[0];
                model.OpTypeList = operatorSwitchService.GetOperatorType().Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.TypeName }).ToList();
                model.OpTypeList.Add(new OperatorTypeDto() { TypeId = 0, TypeName = "All" });

            }
            catch (Exception e)
            {
                LogException(e);
            }

            return PartialView("_EditBlockRoute", model);

        }

        [HttpPost]
        public ActionResult EditBlockRoute(EditBlockRouteDto model, FormCollection FC)
        {
            string message = string.Empty;
            var amts = "";
            var amtRanges = "";
            //UpdateActivity("EditSwitchSetting REQUEST", "POST:CommonSwitch/EditSwitchSetting/", "model.id=" + model.Id);
            //ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);

            try
            {
                if (ModelState.IsValid)
                {
                    var opIds = model.OperatorIds.FirstOrDefault();
                    var crIds = model.CircleIds.FirstOrDefault();
                    var usrIds = model.UserIds.FirstOrDefault();
                    var opTypeIds = model.OpTypeIds.FirstOrDefault();

                    opIds = string.IsNullOrEmpty(opIds) || opIds.Split(',').Any(s => s == "0") ? "All" : opIds;
                    crIds = string.IsNullOrEmpty(crIds) || crIds.Split(',').Any(s => s == "0") ? "All" : crIds;
                    usrIds = string.IsNullOrEmpty(usrIds) || usrIds.Split(',').Any(s => s == "0") ? "All" : usrIds;
                    opTypeIds = string.IsNullOrEmpty(opTypeIds) || opTypeIds.Split(',').Any(s => s == "0") ? "All" : opTypeIds;

                    var bRoute = commonSwitchService.GetBlockRouteById(model.Id);

                    if (bRoute != null)
                    {
                        bRoute.Id = model.Id;
                        bRoute.OpFilter = opIds;
                        bRoute.CircleFilter = crIds;
                        bRoute.UserFilter = usrIds;
                        bRoute.OpTypeFilter = opTypeIds;
                        bRoute.SettingTypeId = model.TypeId == 0 ? 1 : model.TypeId;
                        bRoute.SendStatusId = bRoute.SettingTypeId == 1 ? 3 : model.StatusId;

                        if (!string.IsNullOrEmpty(model.Amounts))
                        {
                            var amountslist = model.Amounts.Split(',').ToList();
                            if (model.Amounts.Contains("-"))
                            {
                                foreach (var amt in amountslist)
                                {
                                    var amount = amt.Trim();
                                    if (!string.IsNullOrEmpty(amount))
                                    {
                                        if (amount.Contains("-"))
                                        {
                                            amtRanges = amtRanges + (!string.IsNullOrEmpty(amtRanges) ? "," : "") + amount;
                                        }
                                        else
                                        {
                                            amts = amts + (!string.IsNullOrEmpty(amts) ? "," : "") + amount;
                                        }
                                    }

                                }
                                bRoute.Amounts = !string.IsNullOrEmpty(amts) ? amts : bRoute.Amounts;
                                bRoute.AmountRanges = !string.IsNullOrEmpty(amtRanges) ? amtRanges : bRoute.AmountRanges;

                            }
                        }

                        bRoute.UpdatedById = CurrentUser.UserID;
                        commonSwitchService.Save(bRoute);
                    }
                }

                ShowSuccessMessage("Error!", "Data has been saved successfully!", false);

            }
            catch (Exception Ex)
            {
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, false);
            }
            // return CreateModelStateErrors();

            return RedirectToAction("BlockRoute");
        }

        #endregion

        private void UpdateActivity(string title, string action, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = title;
                activityLogModel.ActivityPage = action;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }

        private bool ValidateRange(string amountfilter, int ftypeid)
        {
            try
            {
                if (ftypeid == 4)//range
                {
                    var ranges = amountfilter.Split(',').ToList();
                    ranges.ForEach(x =>
                    {

                        var min = Convert.ToInt32(x.Split('-')[0]);
                        var max = Convert.ToInt32(x.Split('-')[1]);
                    });
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        public JsonResult GetOperatorByType(string OpTypeIds)
        {
            var ids = OpTypeIds.Split(',');
            var oplist = commonSwitchService.GetOperatorList().Select(x => new OperatorDto() { OperatorId = x.Id, OperatorName = x.Name, OpTypeId = x.OpTypeId ?? 0 }).ToList();

            if (!string.IsNullOrEmpty(OpTypeIds))
            {
                if (OpTypeIds != "0" && !OpTypeIds.ToLower().Contains("all"))
                    oplist = oplist.Where(x => ids.Any(y => y == x.OpTypeId.ToString())).ToList();
            }

            return Json(oplist);

        }

        public bool Active(int id)
        {
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, id > 0 ? 3 : 2);

            UpdateActivity("CommonSwitch Active/Inactive REQUEST", "GET:CommonSwitch/Active/", "id=" + id);

            if (!action.AllowEdit)
            {
                return false;
            }
            else
            {
                string message = string.Empty;
                try
                {
                    var cRoute = commonSwitchService.GetCommanRouting(id);
                    cRoute.IsActive = !(cRoute.IsActive ?? false);
                    cRoute.ActiveUpdatedDate = DateTime.Now;
                    cRoute.ActiveUpdatedById = CurrentUser.UserID;
                    return commonSwitchService.Save(cRoute).IsActive ?? false;

                }
                catch (Exception)
                {
                    return false;
                }
            }

        }

        #region "USER AMOUNT FILTER RULE"

        //public ActionResult UserFilterRule()
        //{
        //    UpdateActivity("UserFilterRule", "GET:CommonSwitch/UserFilterRule/", string.Empty);
        //    ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId);

        //    UserFilterRuleModel model = new UserFilterRuleModel();
        //    var oplist = commonSwitchService.GetOperatorList();
        //    var circlelist = commonSwitchService.GetCircleList();
        //    var userlist = userService.GetUserList(3);
        //    var apilist = apiService.GetApiList();

        //    try
        //    {
        //        var userFilterRules = commonSwitchService.GetUserFilterRules();

        //        if (userFilterRules.Count > 0)
        //        {
        //            var clist1 = userFilterRules.Select(x => new UserFilterRuleDto
        //            {
        //                Id =x.Id,
        //                UserIds = x.UserFilter,
        //                OperatorIds = x.OpFilter,
        //                CircleIds = x.CircleFilter,
        //                VendorIds = x.ApiFilter,
        //                Percent = x.AmtPercent??0,
        //                Amount = x.Amount??0,
        //                Roffer = x.Roffer??0,
        //                IsActive = x.IsActive ?? false,
        //                IsAutoBlock = x.IsAutoBlock ?? false,
        //                UpdatedOn = x.UpdatedDate?.ToString() ?? x.AddedDate.ToString(),
        //                UpdatedBy = x.User1?.UserProfile?.FullName ?? x.User?.UserProfile?.FullName
        //            }).ToList();

        //            if (clist1.Count > 0)
        //            {
        //                foreach (var item in clist1)
        //                {
        //                    if (item.OperatorIds?.ToUpper()?.Contains("ALL") ?? true)
        //                    {
        //                        item.OperatorNames = "All";
        //                        item.OperatorIds = "All";
        //                    }
        //                    else
        //                    {
        //                        string[] arr = item.OperatorIds.Split(',');

        //                        foreach (var c in arr)
        //                        {
        //                            if (!string.IsNullOrEmpty(c))
        //                            {
        //                                var id = Convert.ToInt32(c.Trim());

        //                                if (id > 0)
        //                                {
        //                                    var opname = oplist.Where(x => x.Id == id).FirstOrDefault()?.Name;
        //                                    if (!string.IsNullOrEmpty(opname))
        //                                    {
        //                                        item.OperatorNames = item.OperatorNames + (string.IsNullOrEmpty(item.OperatorNames) ? "" : ",") + opname;
        //                                    }
        //                                }
        //                            }

        //                        }

        //                    }

        //                    if (item.CircleIds?.ToUpper()?.Contains("ALL") ?? true)
        //                    {
        //                        item.CircleNames = "All";
        //                        item.CircleIds = "All";
        //                    }
        //                    else
        //                    {
        //                        string[] arr = item.CircleIds.Split(',');

        //                        foreach (var c in arr)
        //                        {
        //                            if (!string.IsNullOrEmpty(c))
        //                            {
        //                                var id = Convert.ToInt32(c.Trim());

        //                                if (id > 0)
        //                                {
        //                                    var cname = circlelist.Where(x => x.Id == id).FirstOrDefault()?.CircleName;
        //                                    if (!string.IsNullOrEmpty(cname))
        //                                    {
        //                                        item.CircleNames = item.CircleNames + (string.IsNullOrEmpty(item.CircleNames) ? "" : ",") + cname;
        //                                    }
        //                                }
        //                            }

        //                        }

        //                    }

        //                    if (item.UserIds?.ToUpper()?.Contains("ALL") ?? true)
        //                    {
        //                        item.UserNames = "All";
        //                        item.UserNames = "All";
        //                    }
        //                    else
        //                    {
        //                        string[] arr = item.UserIds.Split(',');

        //                        foreach (var c in arr)
        //                        {
        //                            if (!string.IsNullOrEmpty(c))
        //                            {
        //                                var id = Convert.ToInt32(c.Trim());

        //                                if (id > 0)
        //                                {
        //                                    var uname = userlist.Where(x => x.Id == id).FirstOrDefault()?.UserProfile.FullName;
        //                                    if (!string.IsNullOrEmpty(uname))
        //                                    {
        //                                        item.UserNames = item.UserNames + (string.IsNullOrEmpty(item.UserNames) ? "" : ",") + uname;
        //                                    }
        //                                }
        //                            }

        //                        }

        //                    }

        //                    if (item.VendorIds?.ToUpper()?.Contains("ALL") ?? true)
        //                    {
        //                        item.VendorNames = "All";
        //                        item.VendorIds = "All";
        //                    }
        //                    else
        //                    {
        //                        string[] arr = item.VendorIds.Split(',');

        //                        foreach (var c in arr)
        //                        {
        //                            if (!string.IsNullOrEmpty(c))
        //                            {
        //                                var id = Convert.ToInt32(c.Trim());

        //                                if (id > 0)
        //                                {
        //                                    var vname = apilist.Where(x => x.Id == id).FirstOrDefault()?.ApiName;
        //                                    if (!string.IsNullOrEmpty(vname))
        //                                    {
        //                                        item.VendorNames = item.VendorNames + (string.IsNullOrEmpty(item.VendorNames) ? "" : ",") + vname;
        //                                    }
        //                                }
        //                            }

        //                        }

        //                    }
        //                }

        //            }
        //            model.UserFilterRuleList = clist1;
        //        }

        //        model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
        //        model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
        //        model.UserList = userlist.Select(x => new UserDto { Uid = x.Id, Name = x.UserProfile?.FullName ?? x.Username }).ToList();
        //        model.ApiList = apilist.Select(x => new ApiDto { Id = x.Id, ApiName = x.ApiName }).ToList();

        //    }
        //    catch (Exception e)
        //    {
        //        LogException(e);
        //    }

        //    return View(model);
        //}

        //[HttpPost]
        //public string UserFilterRule(List<UserFilterRuleDto> data)
        //{
        //    int id = 0;
        //    try
        //    {
        //        var amts = "";
        //        var amtRanges = "";
        //        List<UserFilterRule> cList = new List<UserFilterRule>();

        //        var item = data.FirstOrDefault();

        //        if (string.IsNullOrEmpty(item.CircleNames) || item.CircleNames.ToUpper().Contains("ALL"))
        //        {
        //            item.CircleNames = "All";
        //            item.CircleIds = "All";
        //        }

        //        if (string.IsNullOrEmpty(item.OperatorNames) || item.OperatorNames.ToUpper().Contains("ALL"))
        //        {
        //            item.OperatorNames = "All";
        //            item.OperatorIds = "All";
        //        }

        //        if (string.IsNullOrEmpty(item.UserNames) || item.UserNames.ToUpper().Contains("ALL"))
        //        {
        //            item.UserNames = "All";
        //            item.UserIds = "All";
        //        }

        //        if (string.IsNullOrEmpty(item.VendorNames) || item.VendorNames.ToUpper().Contains("ALL"))
        //        {
        //            item.VendorNames = "All";
        //            item.VendorIds = "All";
        //        }

        //        UserFilterRule userFilterRule = new UserFilterRule();
        //        userFilterRule.Id = item.Id;
        //        userFilterRule.OpFilter = item.OperatorIds;
        //        userFilterRule.CircleFilter = item.CircleIds;
        //        userFilterRule.UserFilter = item.UserIds;
        //        userFilterRule.ApiFilter = item.VendorIds;


        //        userFilterRule.AmtPercent = item.Percent;
        //        userFilterRule.Amount = item.Amount;
        //        userFilterRule.Roffer = item.Roffer;

        //        userFilterRule.IsActive = item.IsActive;
        //        userFilterRule.IsAutoBlock = item.IsAutoBlock;
        //        userFilterRule.AddedById = CurrentUser.UserID;
        //        commonSwitchService.Save(userFilterRule);
        //        id = userFilterRule.Id;

        //        return id.ToString();

        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(ex);
        //        return "0";
        //    }

        //}

        //[HttpPost]
        //public bool DeleteUserFilterRule(int Id)
        //{
        //    UpdateActivity("UserFilterRule", "POST:CommonSwitch/DeleteUserFilterRule/", "Id=" + Id);
        //    ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, 4);

        //    try
        //    {
        //        return commonSwitchService.DeleteUserFilterRule(Id);

        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //}

        //public ActionResult EditUserFilterRule(int? id)
        //{
        //    UpdateActivity("EditSwitchSetting REQUEST", "GET:CommonSwitch/EditSwitchSetting/", "id=" + id);
        //    ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, id.HasValue ? 3 : 2);

        //    EditUserFilterRuleDto model = new EditUserFilterRuleDto();

        //    try
        //    {
        //        ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.Roles.FirstOrDefault());

        //        var oplist = commonSwitchService.GetOperatorList();
        //        var circlelist = commonSwitchService.GetCircleList();
        //        var userlist = userService.GetUserList(3).ToList();

        //        var bRoute = commonSwitchService.GetUserFilterRuleById(id ?? 0);

        //        model = new EditUserFilterRuleDto
        //        {
        //            Id = bRoute.Id,
        //            OperatorIds = bRoute.OpFilter.Replace(" ", "").Split(',').ToList(),
        //            CircleIds = bRoute.CircleFilter.Replace(" ", "").Split(',').ToList(),
        //            UserIds = bRoute.UserFilter.Replace(" ", "").Split(',').ToList(),
        //            Amount = bRoute.Amount??0,
        //            Roffer = bRoute.Roffer ?? 0,
        //            Percent = bRoute.AmtPercent ?? 0,
        //            IsActive= bRoute.IsActive??false,
        //            IsAutoBlock=bRoute.IsAutoBlock??false,
        //            UpdatedOn = bRoute.UpdatedDate?.ToString() ?? bRoute.AddedDate.ToString(),
        //            UpdatedBy = bRoute.User1?.UserProfile?.FullName ?? bRoute.User?.UserProfile?.FullName,
        //            VendorIds = (bRoute?.ApiFilter ?? "All").Replace(" ", "").Split(',').ToList()
        //        };


        //        if (model.OperatorIds.Any(x => x.ToUpper().Contains("ALL")))
        //        {
        //            model.OperatorNames = "All";
        //            model.OperatorIds.Clear();
        //            model.OperatorIds.Add("0");
        //        }


        //        if (model.CircleIds.Any(x => x.ToUpper().Contains("ALL")))
        //        {
        //            model.CircleNames = "All";
        //            model.CircleIds.Clear();
        //            model.CircleIds.Add("0");
        //        }


        //        if (model.UserIds.Any(x => x.ToUpper().Contains("ALL")))
        //        {
        //            model.UserNames = "All";
        //            model.UserIds.Clear();
        //            model.UserIds.Add("0");
        //        }

        //        if (model.VendorIds.Any(x => x.ToUpper().Contains("ALL")))
        //        {
        //            model.VendorNames = "All";
        //            model.VendorIds.Clear();
        //            model.VendorIds.Add("0");
        //        }

        //        model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
        //        model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
        //        model.UserList = userlist.Select(x => new UserDto { Uid = x.Id, Name = x.UserProfile?.FullName ?? x.Username }).ToList();

        //        model.OperatorList.Add(new OperatorDto() { OperatorId = 0, OperatorName = "All" });
        //        model.CircleList.Add(new CircleDto() { CircleId = 0, CircleName = "All" });
        //        model.UserList.Add(new UserDto() { Uid = 0, Name = "All" });

        //        model.ApiList = apiService.GetApiList().Select(x => new ApiDto { Id = x.Id, ApiName = x.ApiName }).ToList();
        //        model.ApiList.Add(new ApiDto() { Id = 0, ApiName = "All" });

        //    }
        //    catch (Exception e)
        //    {
        //        LogException(e);
        //    }

        //    return PartialView("_EditUserFilterRule", model);
        //}

        //[HttpPost]
        //public ActionResult EditUserFilterRule(EditUserFilterRuleDto model, FormCollection FC)
        //{
        //    string message = string.Empty;
        //    var amts = "";
        //    var amtRanges = "";
        //    UpdateActivity("EditSwitchSetting REQUEST", "POST:CommonSwitch/EditSwitchSetting/", "model.id=" + model.Id);
        //    ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);

        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var opIds = model.OperatorIds.FirstOrDefault();
        //            var crIds = model.CircleIds.FirstOrDefault();
        //            var usrIds = model.UserIds.FirstOrDefault();
        //            var vendorIds = model.VendorIds.FirstOrDefault();

        //            opIds = string.IsNullOrEmpty(opIds) || opIds.Split(',').Any(s => s == "0") ? "All" : opIds;
        //            crIds = string.IsNullOrEmpty(crIds) || crIds.Split(',').Any(s => s == "0") ? "All" : crIds;
        //            usrIds = string.IsNullOrEmpty(usrIds) || usrIds.Split(',').Any(s => s == "0") ? "All" : usrIds;
        //            vendorIds = string.IsNullOrEmpty(vendorIds) || vendorIds.Split(',').Any(s => s == "0") ? "All" : vendorIds;

        //            var userFilterRule = commonSwitchService.GetUserFilterRuleById(model.Id);

        //            if (userFilterRule != null)
        //            {
        //                userFilterRule.Id = model.Id;
        //                userFilterRule.OpFilter = opIds;
        //                userFilterRule.CircleFilter = crIds;
        //                userFilterRule.UserFilter = usrIds;
        //                userFilterRule.ApiFilter = vendorIds;
        //                userFilterRule.AmtPercent = model.Percent; 
        //                userFilterRule.Amount = model.Amount;
        //                userFilterRule.Roffer = model.Roffer;
        //                userFilterRule.IsActive = model.IsActive;
        //                userFilterRule.IsAutoBlock = model.IsAutoBlock;
        //                userFilterRule.UpdatedById = CurrentUser.UserID;
        //                userFilterRule.UpdatedDate = DateTime.Now;
        //                commonSwitchService.Save(userFilterRule);
        //            }
        //        }

        //        ShowSuccessMessage("Error!", "Data has been saved successfully!", false);

        //    }
        //    catch (Exception Ex)
        //    {
        //        message = "An internal error found during to process your requested data!";
        //        ShowErrorMessage("Error!", message, false);
        //    }
        //    return RedirectToAction("UserFilterRule");
        //}

        public bool ActiveUserFilterRule(int id)
        {
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, id > 0 ? 3 : 2);

            UpdateActivity("CommonSwitch Active/Inactive REQUEST", "GET:CommonSwitch/Active/", "id=" + id);

            if (!action.AllowEdit)
            {
                return false;
            }
            else
            {
                string message = string.Empty;
                try
                {
                    var cRoute = commonSwitchService.GetUserFilterRuleById(id);
                    cRoute.IsActive = !(cRoute.IsActive ?? false);
                    cRoute.UpdatedDate = DateTime.Now;
                    cRoute.UpdatedById = CurrentUser.UserID;
                    commonSwitchService.Save(cRoute);
                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }

        }

        public bool AutoBlockUserFilterRule(int id)
        {
            ViewBag.actionAllowed = action = ActionAllowed("CommonSwitch", CurrentUser.RoleId, id > 0 ? 3 : 2);

            UpdateActivity("CommonSwitch Active/Inactive REQUEST", "GET:CommonSwitch/Active/", "id=" + id);

            if (!action.AllowEdit)
            {
                return false;
            }
            else
            {
                string message = string.Empty;
                try
                {
                    var cRoute = commonSwitchService.GetUserFilterRuleById(id);
                    cRoute.IsAutoBlock = !(cRoute.IsAutoBlock ?? false);
                    cRoute.UpdatedDate = DateTime.Now;
                    cRoute.UpdatedById = CurrentUser.UserID;
                    commonSwitchService.Save(cRoute);
                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }

        }

        #endregion

    }

}