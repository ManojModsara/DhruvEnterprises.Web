using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.LIBS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{

    public class OperatorSwitchController : BaseController
    {
        #region "Fields"

        private readonly IOperatorSwitchService operatorswitchService;
        private readonly IRoleService roleService;
        private readonly IPackageService packageService;
        ActivityLogDto activityLogModel;

        private readonly IApiService apiService;
        private readonly ICommonSwitchService commonSwitchService;
        public ActionAllowedDto action;
        #endregion
        #region "Constructor"
        public OperatorSwitchController(IOperatorSwitchService _operatorswitchService,
            IRoleService _userroleService,
            IApiService _apiService,
            ICommonSwitchService _commonSwitchService,
            IActivityLogService _activityLogService,
            IPackageService _packageService
            ) : base(_activityLogService, _userroleService)
        {

            this.operatorswitchService = _operatorswitchService;
            this.roleService = _userroleService;
            this.apiService = _apiService;
            this.commonSwitchService = _commonSwitchService;
            this.activityLogModel = new ActivityLogDto();
            this.packageService = _packageService;
            this.action = new ActionAllowedDto();

        }
        #endregion
        // GET: OperatorSwitch

        #region "Method"
        [HttpGet]
        public ActionResult Index()
        {

            UpdateActivity("OperatorSwitch REQUEST", "GET:OperatorSwitch/Index/");
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId);

            OperatorSwitchDto model = new OperatorSwitchDto();
            try
            {
                var operators = packageService.GetOperatorList();
                List<ApiSourceDTO> apiSources = new List<ApiSourceDTO>();
                 List<OpcodeListDTO> opcodeListDTOlist = new List<OpcodeListDTO>();

                foreach (var operatr in operators)
                {

                    //  OperatorSwitchDto operatorSwitchDtomodel = new OperatorSwitchDto();
                    OpcodeListDTO opcodeDto = new OpcodeListDTO();
                    var ApiList = operatorswitchService.OpcodeApiList(operatr.Id);
                    foreach (var OperatorApiList in ApiList)
                    {
                        ApiSourceDTO apiSource = new ApiSourceDTO();
                        apiSource.Apiid = OperatorApiList.ApiId ?? 0;
                        apiSource.Name = OperatorApiList.ApiSource.ApiName;
                        apiSource.Opid = operatr.Id;
                        model.apiSourceDTOs.Add(apiSource);
                    }
                    opcodeDto.ApiName = operatr.Name;
                    opcodeDto.ApiID1 = operatr.API1_Id ?? 0;
                    opcodeDto.ApiID2 = operatr.API2_Id ?? 0;
                    opcodeDto.ApiID3 = operatr.API3_Id ?? 0;

                    opcodeDto.Opid = operatr.Id;
                    opcodeDto.OperatorName = operatr.Name;
                    opcodeDto.SwitchTypeId = operatr.SwitchTypeId ?? 0;

                    opcodeDto.ValidTypeId = operatr.ValidationLevel ?? 0;
                    opcodeDto.FetchApiId = operatr.Validate_ApiId ?? 0;
                    opcodeDto.IsFetch = operatr.IsFetch ?? false;
                    opcodeDto.IsPartial = operatr.IsPartial ?? false;

                    opcodeDto.AddedOn = operatr?.AddedDate.ToString() ?? string.Empty;
                    opcodeDto.AddededBy = operatr.User?.UserProfile?.FullName ?? string.Empty;

                    opcodeDto.UpdatedOn = operatr?.UpdatedDate?.ToString() ?? opcodeDto.AddedOn;
                    opcodeDto.UpdatedBy = operatr.User1?.UserProfile?.FullName ?? opcodeDto.AddededBy;

                    model.OpcodeLists.Add(opcodeDto);
                }

                ViewBag.ApiType = operators;
                ViewBag.SwitchList = operatorswitchService.SwitchTypes();
                ViewBag.ApiList = operatorswitchService.GetApiSourceList();
                model.ValidTypeList = operatorswitchService.GetOperatorValidTypes().Select(x => new ValidTypeDto { Id = x.Id, Name = x.Name }).ToList();
            }
            catch (Exception e)
            {
                LogException(e);
            }


            return View(model);
        }

        public ActionResult GetOperatorSwitch(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId);

            var operators = packageService.GetOperatorList();
            KeyValuePair<int, List<Operator>> requestResponses = operatorswitchService.GetOperatorSwitch(model);
            List<ApiSourceDTO> apiSources = new List<ApiSourceDTO>();
            List<OperatorSwitchDto> OperatorSwitchDtos = new List<OperatorSwitchDto>();
            foreach (var operatr in operators)
            {
                ApiSourceDTO apidto = new ApiSourceDTO();
                OperatorSwitchDto opswitchdto = new OperatorSwitchDto();

                var opcodelist = operatorswitchService.OpcodeApiList(operatr.Id);

                foreach (var opcode in opcodelist)
                {
                    apidto.Apiid = opcode.ApiId ?? 0;
                    apidto.Name = opcode.ApiSource.ApiName;
                    apidto.Opid = operatr.Id;
                    opswitchdto.apiSourceDTOs.Add(apidto);
                }
                opswitchdto.ApiName = operatr.Name;
                opswitchdto.ApiID1 = operatr.API1_Id ?? 0;
                opswitchdto.ApiID2 = operatr.API2_Id ?? 0;
                opswitchdto.ApiID3 = operatr.API3_Id ?? 0;
                opswitchdto.Opid = operatr.Id;
                opswitchdto.OperatorName = operatr.Name;
                OperatorSwitchDtos.Add(opswitchdto);
            }

            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = OperatorSwitchDtos.Select(c => new List<object> {
                    c.Opid,
                    c.OperatorName,
                    c.ApiID1,
                    c.ApiID2,
                    c.ApiID3,
                    c.apiSourceDTOs.AsQueryable().ToList()
                    })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public bool Index(List<OpcodeListDTO> data)
        {
            UpdateActivity("OperatorSwitch REQUEST", "POST:OperatorSwitch/Index", "datacount=" + data.Count);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, data.Count > 0 ? 3 : 2);

            try
            {

                foreach (var item in data)
                {

                    int api1 = item.ApiID1 > 0 ? item.ApiID1 :
                                    item.ApiID2 > 0 ? item.ApiID2 :
                                    item.ApiID3 > 0 ? item.ApiID3 : 14;
                    int api2 = item.ApiID2 > 0 ? item.ApiID2 :
                                        item.ApiID3 > 0 ? item.ApiID3 :
                                        item.ApiID1 > 0 ? item.ApiID1 : 14;
                    int api3 = item.ApiID3 > 0 ? item.ApiID3 :
                                    item.ApiID1 > 0 ? item.ApiID1 :
                                    item.ApiID2 > 0 ? item.ApiID2 : 14;


                    if (api1 > 0 || api2 > 0 || api3 > 0)
                    {
                        Operator entity = operatorswitchService.GetOperator(item.Opid);
                        if (IsUpdatedOpSwitch(entity, item))
                        {
                            entity.SwitchTypeId = item.SwitchTypeId > 0 ? (byte)item.SwitchTypeId : Convert.ToByte(2);
                            entity.API1_Id = api1;
                            entity.API2_Id = api2;
                            entity.API3_Id = api3;
                            if (item.FetchApiId > 0)
                            {
                                entity.Validate_ApiId = item.FetchApiId;
                            }
                            entity.IsFetch = item.IsFetch;
                            entity.IsPartial = item.IsPartial;
                            if (item.ValidTypeId > 0)
                            {
                                entity.ValidationLevel = item.ValidTypeId;
                            }

                            if (entity.Id > 0)
                                entity.UpdatedById = CurrentUser.UserID;
                            else
                                entity.AddedById = CurrentUser.UserID;

                            operatorswitchService.Save(entity);
                        }

                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        [HttpPost]
        public bool UpdateSwitch(OpcodeListDTO model)
        {
            UpdateActivity("OperatorSwitch Update REQUEST", "POST:OperatorSwitch/UpdateSwitch/", "opid=" + model.Opid);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, 3);


            try
            {

                int api1 = model.ApiID1 > 0 ? model.ApiID1 :
                                model.ApiID2 > 0 ? model.ApiID2 :
                                model.ApiID3 > 0 ? model.ApiID3 : 14;
                int api2 = model.ApiID2 > 0 ? model.ApiID2 :
                                    model.ApiID3 > 0 ? model.ApiID3 :
                                    model.ApiID1 > 0 ? model.ApiID1 : 14;
                int api3 = model.ApiID3 > 0 ? model.ApiID3 :
                                model.ApiID1 > 0 ? model.ApiID1 :
                                model.ApiID2 > 0 ? model.ApiID2 : 14;


                if (api1 > 0 || api2 > 0 || api3 > 0)
                {
                    Operator entity = operatorswitchService.GetOperator(model.Opid);
                    if (IsUpdatedOpSwitch(entity, model))
                    {
                        entity.SwitchTypeId = model.SwitchTypeId > 0 ? (byte)model.SwitchTypeId : Convert.ToByte(2);
                        entity.API1_Id = api1;
                        entity.API2_Id = api2;
                        entity.API3_Id = api3;
                        if (model.FetchApiId > 0)
                        {
                            entity.Validate_ApiId = model.FetchApiId;
                        }
                        entity.IsFetch = model.IsFetch;
                        entity.IsPartial = model.IsPartial;
                        if (model.ValidTypeId > 0)
                        {
                            entity.ValidationLevel = model.ValidTypeId;
                        }

                        if (entity.Id > 0)
                            entity.UpdatedById = CurrentUser.UserID;
                        else
                            entity.AddedById = CurrentUser.UserID;

                        operatorswitchService.Save(entity);
                    }

                }



                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }
        }

        [HttpPost]
        public ActionResult GetCricleList(DataTableServerSide model)
        {

            ViewBag.actionAllowed = action = ActionAllowed("Circle Switch", CurrentUser.Roles.FirstOrDefault());

            KeyValuePair<int, List<Circle>> requestResponses = operatorswitchService.GetCircleList(model);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    c.Id,
                    c.CircleName,
                     (action.AllowCreate?  DataTableButton.EditButton(Url.Action( "CircleSwitch", "OperatorSwitch",new { id = c.Id }),"Circle Operator"):string.Empty )
                    +"&nbsp;"+
                   (action.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","OperatorSwitch", new { id = c.Id }),"modal-delete-api"):string.Empty)
                   +"&nbsp;"+
                     DataTableButton.SettingButton(Url.Action("CircleSwitch","OperatorSwitch", new { id = c.Id }),"Circle Operator")
                   , action.AllowEdit?true:false
                    })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CircleList()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CircleSwitch(int id)
        {
            UpdateActivity("CircleSwitch REQUEST", "GET:OperatorSwitch/CircleSwitch/", "opid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, 3);


            var circlelist = operatorswitchService.circlesList();

            CircleAmtDto model = new CircleAmtDto();


            var ApiList = operatorswitchService.OpcodeApiList(id);

            foreach (var opcode in ApiList)
            {
                CircleApiSourceDTO apiSource = new CircleApiSourceDTO();

                apiSource.Apiid = opcode.ApiId ?? 0;
                apiSource.Name = opcode.ApiSource.ApiName;
                apiSource.Opid = id;
                model.CircleApiSourceList.Add(apiSource);
            }

            foreach (var circle in circlelist)
            {
                CircleAmtList circledto = new CircleAmtList();

                var route = operatorswitchService.CircleApiRouteList(id, circle.Id).FirstOrDefault();

                circledto.OpId = id;
                circledto.CircleName = circle.CircleName;
                circledto.CircleId = circle.Id;
                circledto.API1_Id = route == null ? 0 : route.API1_Id ?? 0;
                circledto.API2_Id = route == null ? 0 : route.API2_Id ?? 0;
                circledto.API3_Id = route == null ? 0 : route.API3_Id ?? 0;
                circledto.IsRoffer = route == null ? false : route.IsROffer ?? false;
                circledto.AddedOn = route == null ? string.Empty : route.AddedDate.ToString() ?? string.Empty;
                circledto.AddededBy = route == null ? string.Empty : route.User?.UserProfile?.FullName ?? string.Empty;

                circledto.UpdatedOn = route == null ? string.Empty : route.UpdatedDate?.ToString() ?? circledto.AddedOn;
                circledto.UpdatedBy = route == null ? string.Empty : route.User1?.UserProfile?.FullName ?? circledto.AddededBy;

                model.CircleAmtLists.Add(circledto);
            }
            ViewBag.ApiType = circlelist;
            return View(model);
        }

        [HttpPost]
        public bool CircleSwitch(List<CircleAmtList> data)
        {
            UpdateActivity("CircleSwitch REQUEST", "POST:OperatorSwitch/CircleSwitch/", "opid=" + data?.FirstOrDefault()?.OpId + ", data.count=" + data.Count);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, 3);

            if (data == null)
                return false;


            try
            {
                List<CircleRouting> cRouteList = new List<CircleRouting>();
                foreach (var routedata in data)
                {
                    if (routedata.API1_Id > 0 || routedata.API2_Id > 0 || routedata.API3_Id > 0)
                    {
                        CircleRouting route = operatorswitchService.CircleApiRouteList(routedata.OpId, routedata.CircleId).FirstOrDefault();

                        if (route != null)
                        {
                            if (IsUpdatedCRSwitch(route, routedata))
                            {
                                route.API1_Id = routedata.API1_Id > 0 ? routedata.API1_Id :
                                        routedata.API2_Id > 0 ? routedata.API2_Id :
                                        routedata.API3_Id > 0 ? routedata.API3_Id : 0;

                                route.API2_Id = routedata.API2_Id > 0 ? routedata.API2_Id :
                                                 routedata.API3_Id > 0 ? routedata.API3_Id :
                                                 routedata.API1_Id > 0 ? routedata.API1_Id : 0;

                                route.API3_Id = routedata.API3_Id > 0 ? routedata.API3_Id :
                                                 routedata.API1_Id > 0 ? routedata.API1_Id :
                                                 routedata.API2_Id > 0 ? routedata.API2_Id : 0;

                                route.IsROffer = routedata.IsRoffer;

                                route.UpdatedById = CurrentUser.UserID;
                                operatorswitchService.Save(route, true);
                            }


                        }
                        else
                        {
                            CircleRouting croute = new CircleRouting();

                            croute.CircleId = routedata.CircleId;
                            croute.OpId = routedata.OpId;
                            croute.API1_Id = routedata.API1_Id > 0 ? routedata.API1_Id :
                                             routedata.API2_Id > 0 ? routedata.API2_Id :
                                             routedata.API3_Id > 0 ? routedata.API3_Id : 0;

                            croute.API2_Id = routedata.API2_Id > 0 ? routedata.API2_Id :
                                             routedata.API3_Id > 0 ? routedata.API3_Id :
                                             routedata.API1_Id > 0 ? routedata.API1_Id : 0;

                            croute.API3_Id = routedata.API3_Id > 0 ? routedata.API3_Id :
                                             routedata.API1_Id > 0 ? routedata.API1_Id :
                                             routedata.API2_Id > 0 ? routedata.API2_Id : 0;

                            croute.IsROffer = routedata.IsRoffer;

                            route.AddedById = CurrentUser.UserID;
                            operatorswitchService.Save(croute, false);
                        }
                    }

                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        public bool UpdateCRoute(CircleAmtList data)
        {
            UpdateActivity("CircleSwitch Update REQUEST", "POST:OperatorSwitch/UpdateCRoute/", "opid=" + data?.OpId + "cid=" + data?.CircleId);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, 3);

            if (data == null)
                return false;

            try
            {
                if (data.API1_Id > 0 || data.API2_Id > 0 || data.API3_Id > 0)
                {
                    CircleRouting route = operatorswitchService.CircleApiRouteList(data.OpId, data.CircleId).FirstOrDefault();

                    if (route != null)
                    {
                        if (IsUpdatedCRSwitch(route, data))
                        {
                            route.API1_Id = data.API1_Id > 0 ? data.API1_Id :
                                    data.API2_Id > 0 ? data.API2_Id :
                                    data.API3_Id > 0 ? data.API3_Id : 0;

                            route.API2_Id = data.API2_Id > 0 ? data.API2_Id :
                                             data.API3_Id > 0 ? data.API3_Id :
                                             data.API1_Id > 0 ? data.API1_Id : 0;

                            route.API3_Id = data.API3_Id > 0 ? data.API3_Id :
                                             data.API1_Id > 0 ? data.API1_Id :
                                             data.API2_Id > 0 ? data.API2_Id : 0;

                            route.IsROffer = data.IsRoffer;

                            route.UpdatedById = CurrentUser.UserID;
                            operatorswitchService.Save(route, true);
                        }

                    }
                    else
                    {
                        CircleRouting croute = new CircleRouting();

                        croute.CircleId = data.CircleId;
                        croute.OpId = data.OpId;
                        croute.API1_Id = data.API1_Id > 0 ? data.API1_Id :
                                         data.API2_Id > 0 ? data.API2_Id :
                                         data.API3_Id > 0 ? data.API3_Id : 0;

                        croute.API2_Id = data.API2_Id > 0 ? data.API2_Id :
                                         data.API3_Id > 0 ? data.API3_Id :
                                         data.API1_Id > 0 ? data.API1_Id : 0;

                        croute.API3_Id = data.API3_Id > 0 ? data.API3_Id :
                                         data.API1_Id > 0 ? data.API1_Id :
                                         data.API2_Id > 0 ? data.API2_Id : 0;

                        croute.IsROffer = data.IsRoffer;

                        croute.AddedById = CurrentUser.UserID;
                        operatorswitchService.Save(croute, false);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public ActionResult CreateEdit(int? id)
        {
            UpdateActivity("OperatorSwitch CreateEdit REQUEST", "GET:OperatorSwitch/CreateEdit/" + id, "opid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            OperatorDto operatorDto = new OperatorDto();

            var optypes = operatorswitchService.GetOperatorType().Select(x => new OperatorTypeDto() { TypeId = x.Id, TypeName = x.TypeName }).ToList();
            operatorDto.OpTypeList = optypes;

            if (id.HasValue && id.Value > 0)
            {
                //Operator oprator = id.HasValue? operatorswitchService.GetOperator(id.Value) : new Operator();

                //roleDto.Id = oprator.Id;
                //roleDto.RoleName = oprator.RoleName;
            }

            return PartialView("_createedit", operatorDto);

        }

        [HttpPost]
        public ActionResult CreateEdit(OperatorDto model, FormCollection FC)
        {
            string message = string.Empty;
            UpdateActivity("OperatorSwitch CreateEdit REQUEST", "POST:OperatorSwitch/CreateEdit/" + model.OperatorId, "opid=" + model.OperatorId);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, model.OperatorId > 0 ? 3 : 2);

            try
            {

                if (ModelState.IsValid)
                {
                    var optr = operatorswitchService.GetOperatorByName(model.OperatorName.ToLower());
                    if (optr != null)
                    {
                        ShowErrorMessage("Error!", "Operator name already exists. Please enter a unique name.", false);
                    }
                    else
                    {

                        Operator oprator = model.OperatorId > 0 ? operatorswitchService.GetOperator(model.OperatorId) : new Operator();

                        oprator.Name = model.OperatorName;
                        oprator.OpCode = "NA";
                        oprator.OpTypeId = model.OpTypeId;
                        oprator.IsActive = true;
                        oprator.SwitchTypeId = 2;
                        //oprator.API1_Id = 14;
                        //oprator.API2_Id = 14;
                        //oprator.API3_Id = 14;
                        oprator.AddedDate = DateTime.Now;
                        oprator.AddedById = CurrentUser.UserID;
                        oprator.IsSwitch = true;
                        operatorswitchService.Save(oprator);
                        oprator.AddedById = CurrentUser.UserID;
                        oprator.OpCode = oprator.Id.ToString();
                        operatorswitchService.Save(oprator);

                        ShowSuccessMessage("Success!", "Operator has been saved", false);
                    }
                }
            }
            catch (Exception Ex)
            {
                LogException(Ex);
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, false);

            }
            // return CreateModelStateErrors();

            return RedirectToAction("Index");
        }

        public ActionResult ReplaceSwitch(int? id)
        {
            UpdateActivity("OperatorSwitch ReplaceSwitch REQUEST", "GET:OperatorSwitch/ReplaceSwitch/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ReplaceSwitch", CurrentUser.RoleId, 3);

            ReplaceSwitchDto replaceSwitchDto = new ReplaceSwitchDto();

            try
            {
                replaceSwitchDto.OperatorList = commonSwitchService.GetOperatorList().Select(x => new OperatorDto() { OperatorId = x.Id, OperatorName = x.Name }).ToList();
                replaceSwitchDto.ApiSourceList = apiService.GetApiList().Select(x => new ApiSourceDto() { ApiId = x.Id, ApiName = x.ApiName }).ToList();
                replaceSwitchDto.SwitchTypeList = operatorswitchService.SwitchTypes().Select(x => new SwitchTypeDto() { Id = x.Id, TypeName = x.TypeName }).ToList();


            }
            catch (Exception e)
            {
                LogException(e);
            }


            return View("replaceswitch", replaceSwitchDto);

        }

        [HttpPost]
        public ActionResult ReplaceSwitch(ReplaceSwitchDto model, FormCollection FC)
        {
            string message = string.Empty;

            UpdateActivity("OperatorSwitch ReplaceSwitch REQUEST", "POST:OperatorSwitch/ReplaceSwitch/", " from apiid=" + model.CurrentApiId + " to apiid=" + model.NewApiId);
            ViewBag.actionAllowed = action = ActionAllowed("ReplaceSwitch", CurrentUser.RoleId, 3);

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.CurrentApiId == model.NewApiId)
                    {
                        ShowErrorMessage("Error!", "Current vendor and new vendor must not be the same.", false);

                    }
                    else
                    {
                        using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                        {
                            SqlCommand cmd = new SqlCommand("SP_ReplaceSwitch", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@OpId", model.OperatorId);
                            cmd.Parameters.AddWithValue("@SwitchId", model.SwitchTypeId);
                            cmd.Parameters.AddWithValue("@CurrentApiId", model.CurrentApiId);
                            cmd.Parameters.AddWithValue("@NewApiId", model.NewApiId);
                            cmd.Parameters.AddWithValue("@Amount", "0");
                            cmd.Parameters.AddWithValue("@UserId", CurrentUser.UserID);

                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                            ShowSuccessMessage("Success!", "Updated Successfully", false);
                        }
                    }

                }
            }
            catch (Exception Ex)
            {
                LogException(Ex);
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, false);


            }


            return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("ReplaceSwitch") });
        }

        public ActionResult OpValidation(int? id)
        {
            UpdateActivity("OperatorSwitch OpValidation REQUEST", "GET:OperatorSwitch/OpValidation/", "opid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, 3);

            OpValidationDto model = new OpValidationDto();
            model.OpId = id ?? 0;
            try
            {
                var opval = operatorswitchService.GetOperatorValidation(id ?? 0);

                if (opval != null)
                {
                    var numval = opval.Where(x => x.ColumnName == "Number").FirstOrDefault();
                    var Accval = opval.Where(x => x.ColumnName == "Account").FirstOrDefault();
                    var Auth3val = opval.Where(x => x.ColumnName == "Auth3").FirstOrDefault();
                    var Amtval = opval.Where(x => x.ColumnName == "Amount").FirstOrDefault();

                    if (numval != null)
                    {
                        model.NumberLength = numval.MinLenth + (numval.MaxLenth != null ? ("-" + numval.MaxLenth) : string.Empty);
                        model.NumberRange = numval.MinVal + (numval.MaxVal != null ? "-" + numval.MaxVal : string.Empty);
                        model.NumberStart = numval.StartWith;
                        model.NumberErrorMessage = numval.ErrorMsg;
                        model.NumberIsNumeric = numval.IsNum ?? false;

                    }
                    if (Accval != null)
                    {
                        model.AccountLength = Accval.MinLenth + (Accval.MaxLenth != null ? ("-" + Accval.MaxLenth) : string.Empty);
                        model.AccountRange = Accval.MinVal + (Accval.MaxVal != null ? "-" + Accval.MaxVal : string.Empty);
                        model.AccountStart = Accval.StartWith;
                        model.AccountErrorMessage = Accval.ErrorMsg;
                        model.AccountIsNumeric = Accval.IsNum ?? false;

                    }
                    if (Auth3val != null)
                    {
                        model.Auth3Length = Auth3val.MinLenth + Auth3val.MaxLenth > 0 ? ("-" + Auth3val.MaxLenth) : string.Empty;
                        model.Auth3Range = Auth3val.MinVal + (Auth3val.MaxVal != null ? "-" + Auth3val.MaxVal : string.Empty);
                        model.Auth3Start = Auth3val.StartWith;
                        model.Auth3ErrorMessage = Auth3val.ErrorMsg;
                        model.Auth3IsNumeric = Auth3val.IsNum ?? false;

                    }
                    if (Amtval != null)
                    {
                        model.AmountLength = Amtval.MinLenth + (Amtval.MaxLenth > 0 ? ("-" + Amtval.MaxLenth?.ToString()) : string.Empty);
                        model.AmountRange = Amtval.MinVal + (Amtval.MaxVal != null ? "-" + Amtval.MaxVal : string.Empty);
                        model.AmountStart = Amtval.StartWith;
                        model.AmountErrorMessage = Amtval.ErrorMsg;
                        model.AmountIsNumeric = Amtval.IsNum ?? false;

                    }
                }


            }
            catch (Exception e)
            {
                LogException(e);
            }


            return View("OpValidation", model);

        }

        [HttpPost]
        public ActionResult OpValidation(OpValidationDto model, FormCollection FC)
        {
            UpdateActivity("OperatorSwitch OpValidation REQUEST", "POST:OperatorSwitch/OpValidation/", "opid=" + model.OpId);
            ViewBag.actionAllowed = action = ActionAllowed("OperatorSwitch", CurrentUser.RoleId, 3);
            string message = string.Empty;

            try
            {
                var opval = operatorswitchService.GetOperatorValidation(model.OpId);

                OperatorValidation numval = opval?.Where(x => x.ColumnName == "Number")?.FirstOrDefault() ?? new OperatorValidation();
                OperatorValidation Accval = opval?.Where(x => x.ColumnName == "Account")?.FirstOrDefault() ?? new OperatorValidation();
                OperatorValidation Auth3val = opval?.Where(x => x.ColumnName == "Auth3")?.FirstOrDefault() ?? new OperatorValidation();
                OperatorValidation Amtval = opval?.Where(x => x.ColumnName == "Amount")?.FirstOrDefault() ?? new OperatorValidation();

                if (numval != null)
                {
                    try
                    {
                        if (model.NumberLength?.Split('-')?.Length == 2 || model.NumberRange?.Split('-')?.Length == 2 || !string.IsNullOrEmpty(model.NumberStart) || model.NumberIsNumeric)
                        {

                            if (model.NumberLength?.Split('-')?.Length == 2)
                            {
                                numval.MinLenth = Convert.ToInt32(model.NumberLength?.Split('-')[0].Trim());
                                numval.MaxLenth = Convert.ToInt32(model.NumberLength?.Split('-')[1].Trim());
                            }
                            if (model.NumberRange?.Split('-')?.Length == 2)
                            {
                                numval.MinVal = Convert.ToInt32(model.NumberRange?.Split('-')[0].Trim());
                                numval.MaxVal = Convert.ToInt32(model.NumberRange?.Split('-')[1].Trim());
                            }

                            numval.StartWith = model.NumberStart;
                            numval.IsNum = model.NumberIsNumeric;
                            numval.ColumnName = "Number";
                            numval.ErrorMsg = model.NumberErrorMessage;
                            numval.OpId = model.OpId;
                            operatorswitchService.Save(numval);
                        }
                        else if (numval.Id > 0)
                        {
                            operatorswitchService.Delete(numval);
                        }
                    }
                    catch (Exception ex1)
                    {
                        LogException(ex1);
                    }

                }
                if (Accval != null)
                {
                    try
                    {
                        if (model.AccountLength?.Split('-')?.Length == 2 || model.AccountRange?.Split('-')?.Length == 2 || !string.IsNullOrEmpty(model.AccountStart) || model.AccountIsNumeric)
                        {

                            if (model.AccountLength?.Split('-')?.Length == 2)
                            {
                                Accval.MinLenth = Convert.ToInt32(model.AccountLength?.Split('-')[0].Trim());
                                Accval.MaxLenth = Convert.ToInt32(model.AccountLength?.Split('-')[1]);
                            }
                            if (model.AccountRange?.Split('-')?.Length == 2)
                            {
                                Accval.MinVal = Convert.ToInt32(model.AccountRange?.Split('-')[0]);
                                Accval.MaxVal = Convert.ToInt32(model.AccountRange?.Split('-')[1]);
                            }

                            Accval.StartWith = model.AccountStart;
                            Accval.IsNum = model.AccountIsNumeric;
                            Accval.ColumnName = "Account";
                            Accval.ErrorMsg = model.AccountErrorMessage;
                            Accval.OpId = model.OpId;
                            operatorswitchService.Save(Accval);
                        }
                        else if (Accval.Id > 0)
                        {
                            operatorswitchService.Delete(Accval);
                        }
                    }
                    catch (Exception ex1)
                    {
                        LogException(ex1);
                    }
                }
                if (Auth3val != null)
                {
                    try
                    {
                        if (model.Auth3Length?.Split('-')?.Length == 2 || model.Auth3Range?.Split('-').Length == 2 || !string.IsNullOrEmpty(model.Auth3Start) || model.Auth3IsNumeric)
                        {

                            if (model.Auth3Length?.Split('-')?.Length == 2)
                            {
                                Auth3val.MinLenth = Convert.ToInt32(model.Auth3Length?.Split('-')[0].Trim());
                                Auth3val.MaxLenth = Convert.ToInt32(model.Auth3Length?.Split('-')[1]);
                            }
                            if (model.Auth3Range?.Split('-')?.Length == 2)
                            {
                                Auth3val.MinVal = Convert.ToInt32(model.Auth3Range?.Split('-')[0]);
                                Auth3val.MaxVal = Convert.ToInt32(model.Auth3Range?.Split('-')[1]);
                            }

                            Auth3val.StartWith = model.Auth3Start;
                            Auth3val.IsNum = model.Auth3IsNumeric;
                            Auth3val.ColumnName = "Auth3";
                            Auth3val.ErrorMsg = model.Auth3ErrorMessage;
                            Auth3val.OpId = model.OpId;
                            operatorswitchService.Save(Auth3val);
                        }
                        else if (Auth3val.Id > 0)
                        {
                            operatorswitchService.Delete(Auth3val);
                        }
                    }
                    catch (Exception ex1)
                    {
                        LogException(ex1);
                    }
                }
                if (Amtval != null)
                {
                    try
                    {
                        if (model.AmountLength?.Split('-')?.Length == 2 || model.AmountRange?.Split('-')?.Length == 2 || !string.IsNullOrEmpty(model.AmountStart) || model.AmountIsNumeric)
                        {

                            if (model.AmountLength?.Split('-')?.Length == 2)
                            {
                                Amtval.MinLenth = Convert.ToInt32(model.AmountLength?.Split('-')[0].Trim());
                                Amtval.MaxLenth = Convert.ToInt32(model.AmountLength?.Split('-')[1]);
                            }
                            if (model.AmountRange?.Split('-')?.Length == 2)
                            {
                                Amtval.MinVal = Convert.ToInt32(model.AmountRange?.Split('-')[0]);
                                Amtval.MaxVal = Convert.ToInt32(model.AmountRange?.Split('-')[1]);
                            }

                            Amtval.StartWith = model.AmountStart;
                            Amtval.IsNum = model.AmountIsNumeric;
                            Amtval.ColumnName = "Amount";
                            Amtval.ErrorMsg = model.AmountErrorMessage;
                            Amtval.OpId = model.OpId;
                            operatorswitchService.Save(Amtval);
                        }
                        else if (Amtval.Id > 0)
                        {
                            operatorswitchService.Delete(Amtval);
                        }
                    }
                    catch (Exception ex1)
                    {
                        LogException(ex1);
                    }
                }

            }
            catch (Exception Ex)
            {
                LogException(Ex);
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, false);

            }


            return RedirectToAction("OpValidation", new { id = model.OpId });
        }

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

        private bool IsUpdatedOpSwitch(Operator entity, OpcodeListDTO model)
        {

            bool IsUpdated = true;

            if (entity != null)
            {
                if ((entity.SwitchTypeId != model.SwitchTypeId && model.SwitchTypeId > 0) ||
                      (entity.API1_Id != model.ApiID1 && model.ApiID1 > 0) ||
                      (entity.API2_Id != model.ApiID2 && model.ApiID2 > 0) ||
                      (entity.API3_Id != model.ApiID3 && model.ApiID3 > 0) ||
                     (entity.Validate_ApiId != model.FetchApiId && model.FetchApiId > 0) ||
                      entity.IsPartial != model.IsPartial ||
                      entity.IsFetch != model.IsFetch
                      )
                {
                    IsUpdated = true;
                }
                else
                    IsUpdated = false;


            }
            else
            {
                IsUpdated = true;
            }

            return IsUpdated;
        }

        private bool IsUpdatedCRSwitch(CircleRouting entity, CircleAmtList model)
        {

            bool IsUpdated = true;

            if (entity != null)
            {
                if (
                      (entity.API1_Id != model.API1_Id && model.API1_Id > 0) ||
                      (entity.API2_Id != model.API2_Id && model.API2_Id > 0) ||
                      (entity.API3_Id != model.API3_Id && model.API3_Id > 0) ||
                      entity.IsROffer != model.IsRoffer
                      )
                {
                    IsUpdated = true;
                }
                else
                    IsUpdated = false;


            }
            else
            {
                IsUpdated = true;
            }

            return IsUpdated;
        }

        #endregion
    }
}