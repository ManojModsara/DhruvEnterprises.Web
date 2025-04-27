using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DhruvEnterprises.Web.SmsApi;
using DhruvEnterprises.Web.LIBS;
using System.Data.SqlClient;
using System.Data;

namespace DhruvEnterprises.Web.Controllers
{

    public class PackageController : BaseController
    {
        public ActionAllowedDto action;
        private IUserService userService;
        private IPackageService packageService;
        private readonly IApiService apiService;
        private readonly IOperatorSwitchService opSwitchService;
        ActivityLogDto activityLogModel;
        public GlobalSettingAllowedDto globalSettingAllowedDto;
        SMSapiCall sMSapiCall;
        Emails emailApiCall;
        //private IEmailApiService emailApiService;
        public PackageController(IEmailApiService _emailApiService, IUserService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService, IPackageService _packageService, IApiService _apiService, IOperatorSwitchService _opSwitchService) : base(_activityLogService, _roleService)
        {
            this.userService = _adminUserService;
            this.packageService = _packageService;
            this.apiService = _apiService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
            this.opSwitchService = _opSwitchService;
            this.globalSettingAllowedDto = new GlobalSettingAllowedDto();
            this.emailApiCall = new Emails(_emailApiService);
            this.sMSapiCall = new SMSapiCall();
        }

        // GET: Package
        public ActionResult Index()
        {
            UpdateActivity("Package List VISIT", "Get:Package/Index", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId);
            return View();
        }

        [HttpPost]
        public ActionResult GetPackage(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId);
            KeyValuePair<int, List<Package>> package = packageService.GetPackageList(model, CurrentUser.UserID);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = package.Key,
                recordsFiltered = package.Key,
                data = package.Value.Select(c => new List<object> {
                    c.Id,
                    c.PackageName,
                    //c.IsUserLoss,
                   (action.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "Package",new { id = c.Id })):string.Empty )
                    +"&nbsp;"+
                     DataTableButton.SettingButton(Url.Action("PackCommRange","Package", new { id = c.Id }),"Comm Amount Range","success")
                      +"&nbsp;"+
                   (action.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","Package", new { id = c.Id }),"modal-delete-package"):string.Empty)
                   , action.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);

        }


        public bool IsUserLossPackage(int id)
        {

            UpdateActivity("IsuserLoss/!IsuserLoss Package", "GET:Package/IsuserLoss/", "userid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, 3);

            string message = string.Empty;
            try
            {
                var adminUser = packageService.GetPackage(id);
                adminUser.IsUserLoss = !adminUser.IsUserLoss;
                return packageService.Save(adminUser).IsUserLoss;

            }
            catch (Exception)
            {
                return false;
            }


        }

        public ActionResult CreateEdit(int? id)
        {
            UpdateActivity("Package Add/Update REQUEST", "GET:Package/CreateEdit", "packid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            PackageDto packageModel = new PackageDto();
            PackageComm packageComm = new PackageComm();
            var operators = packageService.GetOperatorList();

            if (id.HasValue && id.Value > 0)
            {
                Package package = packageService.GetPackage(id);
                List<PackageComm> packageComms = packageService.PackageCommList(id ?? 0, 0, 0, 0, 1).ToList();
                packageModel.PackageName = package.PackageName;
                packageModel.Id = id ?? 0;

                operators.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = id ?? 0;
                    pcomm.OpId = x.Id;
                    pcomm.OperatorName = x.Name;
                    pcomm.PackageName = package.PackageName;

                    if (packageComms.Any(p => p.PackId == id && p.OpId == x.Id))
                    {
                        var pcom = packageComms.Where(p => p.PackId == id && p.OpId == x.Id).FirstOrDefault();
                        pcomm.CommAmt = pcom.CommAmt ?? 0;
                        pcomm.AmtTypeId = pcom.AmtTypeId ?? 5;
                        pcomm.CommTypeId = pcom.CommTypeId ?? 2;
                        pcomm.IsCirclePack = pcom.IsCirclePack ?? false;
                        pcomm.DailyLimit = pcom.DailyLimit ?? 0;
                        pcomm.UsedLimit = pcom.UsedLimit ?? 0;
                        pcom.IsUserLoss = pcom.IsUserLoss;
                    }


                    packageModel.PackageCommList.Add(pcomm);
                });
            }
            else
            {
                operators.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = id ?? 0;
                    pcomm.OpId = x.Id;
                    pcomm.OperatorName = x.Name;
                    pcomm.AmtTypeId = 5;
                    pcomm.CommTypeId = 2;
                    packageModel.PackageCommList.Add(pcomm);
                });
            }
            return View("createedit", packageModel);
        }

        [HttpPost]
        public bool CreateEdit(List<PackageCommDto> data)
        {
            UpdateActivity("Package Add/Update REQUEST", "Post:Package/CreateEdit", "packid=" + data.FirstOrDefault()?.PackId);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, data.FirstOrDefault()?.PackId > 0 ? 3 : 2);
            string message = string.Empty;
            bool IsPackageChange = false;
            try
            {
                var packg = data.FirstOrDefault();
                Package package = packageService.GetPackage(packg.PackId) ?? new Package();
                package.Id = packg.PackId;
                package.PackageName = packg.PackageName;
                package.AddedById = CurrentUser.UserID;
                package.PTypeId = 1;
                packageService.Save(package);
                List<PackageComm> packagecommlist = new List<PackageComm>();
                decimal OCommAmt, ODailyLimit, OUsedLimit;
                byte OCommTypeId, OAmtTypeId;
                bool OIsCirclePack;
                foreach (var packcomm in data)
                {
                    PackageComm packageComm = packageService.GetPackageCommByOpId(packg.PackId, packcomm.OpId) ?? new PackageComm();
                    packageComm.PackId = package.Id;
                    packageComm.OpId = packcomm.OpId;
                    if (IsPackageChange == false)
                    {
                        OCommAmt = packageComm.CommAmt ?? 0;
                        OCommTypeId = packageComm.CommTypeId ?? 0;
                        OAmtTypeId = packageComm.AmtTypeId ?? 0;
                        OIsCirclePack = packageComm.IsCirclePack ?? false;
                        ODailyLimit = packageComm.DailyLimit ?? 0;
                        OUsedLimit = packageComm.UsedLimit ?? 0;
                        if (OCommAmt != packcomm.CommAmt)
                        {
                            IsPackageChange = true;
                        }
                        if (OCommTypeId != packcomm.CommTypeId)
                        {
                            IsPackageChange = true;
                        }
                        if (OAmtTypeId != packcomm.AmtTypeId)
                        {
                            IsPackageChange = true;
                        }
                        if (OIsCirclePack != packcomm.IsCirclePack)
                        {
                            IsPackageChange = true;
                        }
                        if (ODailyLimit != packcomm.DailyLimit)
                        {
                            IsPackageChange = true;
                        }
                        if (OUsedLimit != packcomm.UsedLimit)
                        {
                            IsPackageChange = true;
                        }
                    }
                    packageComm.CommAmt = packcomm.CommAmt;
                    packageComm.CommTypeId = packcomm.CommTypeId;
                    packageComm.AmtTypeId = packcomm.AmtTypeId;
                    packageComm.IsCirclePack = packcomm.IsCirclePack;
                    packageComm.DailyLimit = packcomm.DailyLimit;
                    packageComm.UsedLimit = packcomm.UsedLimit;
                    packageComm.IsUserLoss = packcomm.IsuserLoss;
                    packageService.Save(packageComm);

                }
                //if (IsPackageChange == true)
                //{
                //    sendSmsToUsers(package.Id);

                //}
                ShowSuccessMessage("Success!", "Package has been saved", false);
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Package already exist.";
                    ShowErrorMessage("Error! ", message, true);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error! ", message, true);
                }
                return false;
            }

        }
        private void sendSmsToUsers(int PackageId)
        {
            var action = "packagecommchange";
            globalSettingAllowedDto = GlobalSettingAllowed(action);
            if (globalSettingAllowedDto.AllowSms == true || globalSettingAllowedDto.AllowEmail == true)
            {
                List<User> userList = userService.GetUserListByPackageID(PackageId);
                foreach (var user in userList)
                {
                    string email = user.UserProfile.Email;
                    string mob = user.UserProfile.MobileNumber;
                    string message1 = "Your Package slabs has been changed. Please Login to see more details.\n Thank you,\n Ezytm";

                    if (globalSettingAllowedDto.AllowSms == true)
                    {
                        if (!string.IsNullOrEmpty(mob.Trim()))
                        {
                            sMSapiCall.SendSms(mob.Trim(), message1);
                        }
                    }
                    if (globalSettingAllowedDto.AllowEmail == true)
                    {
                        if (!string.IsNullOrEmpty(email))
                        {
                            emailApiCall.SendEmail("Package Change", email, message1);
                        }
                    }
                }
            }

        }

        // GET: Package
        public ActionResult PackageComm(int? u)
        {
            UpdateActivity("PackageComm List VISIT", "Get:Package/PackageComm");
            ViewBag.actionAllowed = action = ActionAllowed("PackageComm", CurrentUser.RoleId);
            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int userid = u.HasValue ? Convert.ToInt32(u) : 0;
            int uid = IsAdminRole ? userid : CurrentUser.UserID;
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u) }).ToList();
            int packid = userService.GetUser(uid)?.PackageId ?? 0;
            PackageDto packageModel = new PackageDto();
            PackageComm packageComm = new PackageComm();
            var operators = packageService.GetOperatorList();
            if (packid > 0)
            {
                Package package = packageService.GetPackage(packid);
                List<PackageComm> packageComms = packageService.PackageCommList(packid, 0, 0, 0, 1).ToList();
                packageModel.PackageName = package.PackageName;
                packageModel.Id = packid;
                operators.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    var pcom = packageComms.Where(p => p.PackId == packid && p.OpId == x.Id).FirstOrDefault();
                    pcom = pcom == null ? new PackageComm() : pcom;
                    if (pcom.CommAmt > 0 || pcom.DailyLimit > 0 || pcom.IsCirclePack == true)
                    {
                        pcomm.PackId = packid;                       
                        pcomm.OpId = x.Id;
                        pcomm.OperatorName = x.Name;
                        pcomm.PackageName = package.PackageName;
                        if (packageComms.Any(p => p.PackId == packid && p.OpId == x.Id))
                        {
                            pcomm.CommAmt = pcom.CommAmt ?? 0;
                            pcomm.AmtTypeName = pcom.AmtType?.AmtTypeName ?? "Discount";
                            pcomm.CommTypeName = pcom.CommType?.TypeName ?? "Percent";
                            pcomm.IsCirclePack = pcom.IsCirclePack ?? false;
                            pcomm.DailyLimit = pcom.DailyLimit ?? 0;
                            pcomm.UsedLimit = pcom.UsedLimit ?? 0;
                        }
                        else
                        {
                            pcomm.AmtTypeName = "Discount";
                            pcomm.CommTypeName = "Percent";
                        }
                        packageModel.PackageCommList.Add(pcomm);
                    }
                });
            }
            else
            {
                operators.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = packid;
                    pcomm.OpId = x.Id;
                    pcomm.OperatorName = x.Name;
                    pcomm.AmtTypeName = "Discount";
                    pcomm.CommTypeName = "Percent";
                    packageModel.PackageCommList.Add(pcomm);
                });
            }
            return View("packagecomm", packageModel);
        }



        public ActionResult CirclePack(int? id, int? opid)
        {
            UpdateActivity("CirclePack", "GET:Package/CirclePack", "packid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            PackageDto model = new PackageDto();
            PackageCommCircle packCommCircle = new PackageCommCircle();
            var circles = opSwitchService.circlesList();
            var optr = opSwitchService.GetOperator(opid ?? 0);
            if (id.HasValue && id.Value > 0)
            {
                Package package = packageService.GetPackage(id);


                List<PackageCommCircle> packageComms = packageService.GetPackageCommCircleByOpId(id ?? 0, opid ?? 0).ToList();
                model.PackageName = package.PackageName;
                model.Id = id ?? 0;
                model.OpId = opid ?? 0;
                model.OperatorName = optr.Name;

                circles.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = id ?? 0;
                    pcomm.CircleId = x.Id;
                    pcomm.CircleName = x.CircleName;
                    pcomm.OpId = optr.Id;
                    pcomm.OperatorName = optr.Name;
                    pcomm.PackageName = package.PackageName;
                    if (packageComms.Any(p => p.PackId == id && p.OpId == opid && p.CircleId == x.Id))
                    {
                        var cmm = packageComms.Where(p => p.PackId == id && p.OpId == opid && p.CircleId == x.Id).FirstOrDefault();

                        pcomm.CommAmt = cmm.CommAmt ?? 0;
                        pcomm.AmtTypeId = cmm.AmtTypeId ?? 5;
                        pcomm.CommTypeId = cmm.CommTypeId ?? 2;
                        pcomm.DailyLimit = cmm.DailyLimit ?? 0;
                        pcomm.UsedLimit = cmm.UsedLimit ?? 0;
                    }
                    else
                    {
                        pcomm.CommAmt = 0;
                    }
                    model.PackageCommList.Add(pcomm);
                });
            }
            else
            {
                circles.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = id ?? 0;
                    pcomm.OpId = optr.Id;
                    pcomm.OperatorName = optr.Name;
                    pcomm.CircleId = x.Id;
                    pcomm.CircleName = x.CircleName;
                    pcomm.AmtTypeId = 5;
                    pcomm.CommTypeId = 2;
                    model.PackageCommList.Add(pcomm);
                });
            }
            return View("CirclePack", model);
        }

        [HttpPost]
        public bool CirclePack(List<PackageCommDto> data)
        {
            UpdateActivity("CirclePack", "Post:Package/CirclePack", "packid=" + data.FirstOrDefault()?.PackId);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, data.FirstOrDefault()?.PackId > 0 ? 3 : 2);

            string message = string.Empty;

            try
            {
                List<PackageCommCircle> packagecommlist = new List<PackageCommCircle>();
                foreach (var item in data)
                {
                    PackageCommCircle packageComm = packageService.GetPackageCommCircleByOpId(item.PackId, item.OpId, item.CircleId) ?? new PackageCommCircle();
                    packageComm.PackId = item.PackId;
                    packageComm.OpId = item.OpId;
                    packageComm.CircleId = item.CircleId;
                    packageComm.CommAmt = item.CommAmt;
                    packageComm.CommTypeId = item.CommTypeId;
                    packageComm.AmtTypeId = item.AmtTypeId;
                    packageComm.DailyLimit = item.DailyLimit;
                    packageComm.UsedLimit = item.UsedLimit;
                    packageService.Save(packageComm);
                }

                ShowSuccessMessage("Success!", "Package has been saved", false);
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Package already exist.";
                    ShowErrorMessage("Error! ", message, true);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error! ", message, true);
                }
                return false;
            }

        }

        public ActionResult CirclePackComm(int? u, int? id, int? opid)
        {
            UpdateActivity("CirclePack", "GET:Package/CirclePackComm", "packid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("PackageComm", CurrentUser.RoleId);

            PackageDto model = new PackageDto();
            PackageCommCircle packCommCircle = new PackageCommCircle();
            var circles = opSwitchService.circlesList();
            var optr = opSwitchService.GetOperator(opid ?? 0);
            if (id.HasValue && id.Value > 0)
            {
                Package package = packageService.GetPackage(id);


                List<PackageCommCircle> packageComms = packageService.GetPackageCommCircleByOpId(id ?? 0, opid ?? 0).ToList();
                model.PackageName = package.PackageName;
                model.Id = id ?? 0;
                model.OpId = opid ?? 0;
                model.OperatorName = optr.Name;

                circles.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = id ?? 0;
                    pcomm.CircleId = x.Id;
                    pcomm.CircleName = x.CircleName;
                    pcomm.OpId = optr.Id;
                    pcomm.OperatorName = optr.Name;
                    pcomm.PackageName = package.PackageName;
                    if (packageComms.Any(p => p.PackId == id && p.OpId == opid && p.CircleId == x.Id))
                    {
                        var cmm = packageComms.Where(p => p.PackId == id && p.OpId == opid && p.CircleId == x.Id).FirstOrDefault();

                        pcomm.CommAmt = cmm.CommAmt ?? 0;
                        pcomm.AmtTypeId = cmm.AmtTypeId ?? 5;
                        pcomm.CommTypeId = cmm.CommTypeId ?? 2;
                        pcomm.AmtTypeName = cmm.AmtType?.AmtTypeName ?? "Discount";
                        pcomm.CommTypeName = cmm.CommType?.TypeName ?? "Percent";
                        pcomm.DailyLimit = cmm.DailyLimit ?? 0;
                        pcomm.UsedLimit = cmm.UsedLimit ?? 0;
                    }
                    else
                    {
                        pcomm.CommAmt = 0;
                        pcomm.AmtTypeName = "Discount";
                        pcomm.CommTypeName = "Percent";
                    }
                    model.PackageCommList.Add(pcomm);
                });
            }
            else
            {
                circles.ForEach(x =>
                {
                    PackageCommDto pcomm = new PackageCommDto();
                    pcomm.PackId = id ?? 0;
                    pcomm.OpId = optr.Id;
                    pcomm.OperatorName = optr.Name;
                    pcomm.CircleId = x.Id;
                    pcomm.CircleName = x.CircleName;
                    pcomm.CommAmt = 0;
                    pcomm.AmtTypeId = 5;
                    pcomm.CommTypeId = 2;
                    pcomm.AmtTypeName = "Discount";
                    pcomm.CommTypeName = "Percent";
                    model.PackageCommList.Add(pcomm);
                });
            }
            return View("CirclePackComm", model);
        }        

        public ActionResult VendorOpComm(int? o, int? v)
        {
            UpdateActivity("VendorOpComm List VISIT", "Get:Package/VendorOpComm");
            ViewBag.actionAllowed = action = ActionAllowed("VendorOpComm", CurrentUser.RoleId);

            int apiid = v ?? 0;
            int opid = o ?? 1;

            var userlist = userService.GetUserList(3);
            var apilist = apiService.GetApiList();
            PackageDto packageModel = new PackageDto();

            List<PackageComm> packageComms = packageService.PackageCommList(0, opid, 0, apiid, 0).ToList();

            var pcomms = packageComms.Select(x => new PackageCommDto()
            {
                Id = x.Id,
                PackId = x.PackId ?? 0,
                PackageName = x.Package?.PackageName ?? string.Empty,
                OpId = x.OpId ?? 0,
                OperatorName = x.Operator?.Name ?? string.Empty,
                CommAmt = x.CommAmt ?? 0,
                AmtTypeName = x.AmtType?.AmtTypeName ?? string.Empty,
                CommTypeName = x.CommType?.TypeName ?? string.Empty,
                IsCirclePack = x.IsCirclePack ?? false,
                UserName = string.Join(",", apilist.Where(y => y.ApiPackid == x.PackId).Select(y => y?.ApiName ?? string.Empty))

            }).ToList();

            packageModel.PackageCommList = pcomms;

            ViewBag.VendorList = apilist.Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).OrderBy(x => x.Text).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).OrderBy(x => x.Text).ToList();
            // ViewBag.UserList = userService.GetUserList(3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u) }).ToList();

            return View("vendoropcomm", packageModel);
        }

        private void UpdateActivity(string activityName, string activityPage, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = activityName;
                activityLogModel.ActivityPage = activityPage;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }

        #region "PackageComm Amount Range"
        public ActionResult PackCommRange(int? id)
        {
            UpdateActivity("Package Add/Update REQUEST", "GET:Package/CreateEdit", "packid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, id.HasValue ? 3 : 2);


            PackCommRangeDto model = new PackCommRangeDto();
            var package = packageService.GetPackage(id);
            var pcomList = packageService.GetPackageCommRangeList(id ?? 0);

            var oplist = packageService.GetOperatorList();
            var circlelist = opSwitchService.circlesList();
            var optypelist = opSwitchService.GetOperatorType();
            var commtypes = packageService.GetCommTypes();
            var amttypes = packageService.GetAmtTypes();

            model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
            model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
            model.OpTypeList = optypelist.Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.TypeName }).ToList();
            model.CommTypeList = commtypes.Where(x => x.Remark != null && x.Remark.Contains("Comm")).Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.TypeName }).OrderByDescending(x => x.TypeId).ToList();
            model.AmtTypeList = amttypes.Where(x => x.Remark != null && x.Remark.Contains("Comm")).Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.AmtTypeName }).ToList();

            model.OperatorList.Add(new OperatorDto() { OperatorId = 0, OperatorName = "All" });
            model.CircleList.Add(new CircleDto() { CircleId = 0, CircleName = "All" });
            model.OpTypeList.Add(new OperatorTypeDto() { TypeId = 0, TypeName = "All" });

            model.PackId = package?.Id ?? 0;
            model.PackName = package?.PackageName ?? string.Empty;

            var pcommlist = pcomList.Select(x => new PackCommRangeDto()
            {
                Id = x.Id,
                PackId = x.PackId ?? 0,
                PackName = package.PackageName,
                OpTypeIds = x.OpTypeFilter,
                OperatorIds = x.OpFilter,
                CircleIds = x.CircleFilter,
                AmountRange = x.MinVal + "-" + x.MaxVal,
                CommAmt = x.CommAmt ?? 0,
                CommTypeId = x.CommTypeId ?? 0,
                AmtTypeId = x.AmtTypeId ?? 0,
                CommTypeName = x.CommType.TypeName,
                AmtTypeName = x.AmtType.AmtTypeName,
                UpdatedOn = x.UpdatedDate?.ToString() ?? x.AddedDate.ToString(),
                UpdatedBy = x.User1?.Username ?? x.User?.Username ?? string.Empty

            }).ToList();

            if (pcommlist.Count > 0)
            {
                foreach (var item in pcommlist)
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
                                var opid = Convert.ToInt32(c.Trim());

                                if (opid > 0)
                                {
                                    var opname = oplist.Where(x => x.Id == opid).FirstOrDefault()?.Name;
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
                                var cid = Convert.ToInt32(c.Trim());

                                if (cid > 0)
                                {
                                    var cname = circlelist.Where(x => x.Id == cid).FirstOrDefault()?.CircleName;
                                    if (!string.IsNullOrEmpty(cname))
                                    {
                                        item.CircleNames = item.CircleNames + (string.IsNullOrEmpty(item.CircleNames) ? "" : ",") + cname;
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
                                var otid = Convert.ToInt32(c.Trim());

                                if (otid > 0)
                                {
                                    var optypename = optypelist.Where(x => x.Id == otid).FirstOrDefault()?.TypeName;
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

            model.PackCommRangeList = pcommlist;
            return View("packcommrange", model);
        }

        [HttpPost]
        public int PackCommRange(PackCommRangeDto data)
        {
            UpdateActivity("Package Add/Update REQUEST", "Post:Package/CreateEdit", "packid=" + data.PackId);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, data.PackId > 0 ? 3 : 2);
            var package = packageService.GetPackage(data.PackId);
            string message = string.Empty;
            try
            {
                if (data.PackId <= 0)
                {
                    ShowErrorMessage("Error! ", "Package Not Found.", true);
                }
                else if (string.IsNullOrEmpty(data.AmountRange) || !data.AmountRange.Contains("-"))
                {
                    ShowErrorMessage("Error! ", "Enter Amount Range", true);
                }
                else if (data.CommTypeId <= 0)
                {
                    ShowErrorMessage("Error! ", "Select Comm Type", true);
                }
                else if (data.AmtTypeId <= 0)
                {
                    ShowErrorMessage("Error! ", "Select Amt Type", true);
                }
                else
                {
                    var minval = Convert.ToInt32(data.AmountRange.Split('-')[0].Trim());
                    var maxval = Convert.ToInt32(data.AmountRange.Split('-')[1].Trim());
                    var packCommrange = new PackageCommRange();
                    if (string.IsNullOrEmpty(data.CircleIds) || data.CircleNames.ToUpper().Contains("ALL"))
                    {
                        data.CircleNames = "All";
                        data.CircleIds = "All";
                    }
                    if (string.IsNullOrEmpty(data.OperatorIds) || data.OperatorNames.ToUpper().Contains("ALL"))
                    {
                        data.OperatorNames = "All";
                        data.OperatorIds = "All";
                    }
                    if (string.IsNullOrEmpty(data.OpTypeIds) || data.OpTypeNames.ToUpper().Contains("ALL"))
                    {
                        data.OpTypeNames = "All";
                        data.OpTypeIds = "All";
                    }
                    packCommrange.PackId = package.Id;
                    packCommrange.OpTypeFilter = data.OpTypeIds;
                    packCommrange.OpFilter = data.OperatorIds;
                    packCommrange.CircleFilter = data.CircleIds;
                    packCommrange.MinVal = minval;
                    packCommrange.MaxVal = maxval;
                    packCommrange.IsUserLoss = data.IsUserLoss;
                    packCommrange.CommAmt = data.CommAmt;
                    packCommrange.CommTypeId = data.CommTypeId;
                    packCommrange.AmtTypeId = data.AmtTypeId;
                    packCommrange.AddedById = CurrentUser.UserID;
                    packageService.Save(packCommrange);
                    ShowSuccessMessage("Success!", "Data has been saved", false);
                }
                return 0;
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Package already exist.";
                    ShowErrorMessage("Error! ", message, true);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error! ", message, true);
                }
                return 1;
            }

        }

        public ActionResult PackCommRangeRetailer()
        {
            UpdateActivity("PackageComm List VISIT", "Get:Package/PackageComm");
            ViewBag.actionAllowed = action = ActionAllowed("PackageComm", CurrentUser.RoleId);
            PackCommRangeDto model = new PackCommRangeDto();
            //var package = packageService.GetPackage(id);
            int uid = CurrentUser.UserID;
           
            int packid = userService.GetUser(uid)?.PackageId ?? 0;
            var pcomList = packageService.GetPackageCommRangeList(packid);
            var oplist = packageService.GetOperatorList();
            var circlelist = opSwitchService.circlesList();
            var optypelist = opSwitchService.GetOperatorType();
            


            var pcommlist = pcomList.Select(x => new PackCommRangeDto()
            {
                //Id = x.Id,
                //PackId = x.PackId ?? 0,
                //PackName = package?.PackageName,
                OpTypeIds = x.OpTypeFilter,
                OperatorIds = x.OpFilter,
                CircleIds = x.CircleFilter,
                AmountRange = x.MinVal + "-" + x.MaxVal,
                CommAmt = x.CommAmt ?? 0,
                CommTypeId = x.CommTypeId ?? 0,
                AmtTypeId = x.AmtTypeId ?? 0,
                CommTypeName = x.CommType.TypeName,
                AmtTypeName = x.AmtType.AmtTypeName
               
            }).ToList();
            if (pcommlist.Count > 0)
            {
                foreach (var item in pcommlist)
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
                                var opid = Convert.ToInt32(c.Trim());

                                if (opid > 0)
                                {
                                    var opname = oplist.Where(x => x.Id == opid).FirstOrDefault()?.Name;
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
                                var cid = Convert.ToInt32(c.Trim());

                                if (cid > 0)
                                {
                                    var cname = circlelist.Where(x => x.Id == cid).FirstOrDefault()?.CircleName;
                                    if (!string.IsNullOrEmpty(cname))
                                    {
                                        item.CircleNames = item.CircleNames + (string.IsNullOrEmpty(item.CircleNames) ? "" : ",") + cname;
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
                                var otid = Convert.ToInt32(c.Trim());

                                if (otid > 0)
                                {
                                    var optypename = optypelist.Where(x => x.Id == otid).FirstOrDefault()?.TypeName;
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
            model.PackCommRangeList = pcommlist;
            return View("PackCommRangeRetailer", model);
           
        }




        public ActionResult EditPackCommRange(int? id)
        {
            UpdateActivity("EditPackCommRange", "GET:Package/EditPackCommRange/", "id=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            EditPackCommRangeDto model = new EditPackCommRangeDto();

            var packCommRange = packageService.GetPackageCommRange(id ?? 0);
            var package = packageService.GetPackage(packCommRange.PackId ?? 0);

            var oplist = packageService.GetOperatorList();
            var circlelist = opSwitchService.circlesList();
            var opypelist = opSwitchService.GetOperatorType();
            var commtypes = packageService.GetCommTypes();
            var amttypes = packageService.GetAmtTypes();

            model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
            model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
            model.OpTypeList = opypelist.Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.TypeName }).ToList();
            model.CommTypeList = commtypes.Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.TypeName }).ToList();
            model.AmtTypeList = amttypes.Where(x => x.Remark != null && x.Remark.Contains("Comm")).Select(x => new OperatorTypeDto { TypeId = x.Id, TypeName = x.AmtTypeName }).ToList();

            model.PackId = package?.Id ?? 0;
            model.PackName = package?.PackageName ?? string.Empty;
            model.OpTypeIds = packCommRange.OpTypeFilter.Replace(" ", "").Split(',').ToList();
            model.OperatorIds = packCommRange.OpFilter.Replace(" ", "").Split(',').ToList();
            model.CircleIds = packCommRange.CircleFilter.Replace(" ", "").Split(',').ToList();
            model.AmountRange = packCommRange.MinVal + "-" + packCommRange.MaxVal;
            model.CommAmt = packCommRange.CommAmt ?? 0;
            model.CommTypeId = packCommRange.CommTypeId ?? 0;
            model.AmtTypeId = packCommRange.AmtTypeId ?? 0;
            model.IsUserLoss = packCommRange.IsUserLoss??false;
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

            if (model.OpTypeIds.Any(x => x.ToUpper().Contains("ALL")))
            {
                model.OpTypeNames = "All";
                model.OpTypeIds.Clear();
                model.OpTypeIds.Add("0");
            }

            model.OperatorList.Add(new OperatorDto() { OperatorId = 0, OperatorName = "All" });
            model.CircleList.Add(new CircleDto() { CircleId = 0, CircleName = "All" });
            model.OpTypeList.Add(new OperatorTypeDto() { TypeId = 0, TypeName = "All" });

            return PartialView("_EditPackCommRange", model);

        }

        [HttpPost]
        public ActionResult EditPackCommRange(EditPackCommRangeDto model, FormCollection FC)
        {
            string message = string.Empty;

            UpdateActivity("EditPackCommRange", "POST:Package/EditPackCommRange/", "id=" + model.Id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);

            try
            {
                if (ModelState.IsValid)
                {
                    //var opTypeIds = model.OpTypeIds.FirstOrDefault();
                    var opIds = model.OperatorIds.FirstOrDefault();
                    //var crIds = model.CircleIds.FirstOrDefault();


                    //opTypeIds = string.IsNullOrEmpty(opTypeIds) || opTypeIds.Split(',').Any(s => s == "0") ? "All" : opTypeIds;
                    opIds = string.IsNullOrEmpty(opIds) || opIds.Split(',').Any(s => s == "0") ? "All" : opIds;
                    //crIds = string.IsNullOrEmpty(crIds) || crIds.Split(',').Any(s => s == "0") ? "All" : crIds;

                    var pcomm = packageService.GetPackageCommRange(model.Id);

                    if (pcomm != null)
                    {
                        pcomm.Id = model.Id;
                        pcomm.OpFilter = opIds;
                        //pcomm.CircleFilter = crIds;
                        //pcomm.OpTypeFilter = opTypeIds;
                        pcomm.MinVal = Convert.ToInt32(model.AmountRange.Split('-')[0].Trim());
                        pcomm.MaxVal = Convert.ToInt32(model.AmountRange.Split('-')[1].Trim());
                        pcomm.CommAmt = model.CommAmt;
                        pcomm.CommTypeId = model.CommTypeId;
                        pcomm.AmtTypeId = model.AmtTypeId;
                        pcomm.IsUserLoss = model.IsUserLoss;
                        pcomm.UpdatedById = CurrentUser.UserID;
                        packageService.Save(pcomm);
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

            return RedirectToAction("PackCommRange", new { id = model.PackId });
        }

        [HttpPost]
        public bool DeletePackCommRange(int Id)
        {
            UpdateActivity("Package", "POST:Package/DeletePackCommRange/", "Id=" + Id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, 4);

            try
            {
                return packageService.DeletePackCommRange(Id);

            }
            catch (Exception)
            {
                return false;
            }

        }

        #endregion

        #region Package Update all
        public ActionResult PackCommUpdate(int? id)
        {
            UpdateActivity("Package PackCommUpdate REQUEST", "GET:Package/PackCommUpdate", "packid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            PackCommRangeDto model = new PackCommRangeDto();
            var package = packageService.GetPackage(id);
            var pcomList = packageService.GetPackageCommRangeList(id ?? 0);
            var oplist = packageService.GetOperatorList();
            var circlelist = opSwitchService.circlesList();
            var optypelist = opSwitchService.GetOperatorType();
            var commtypes = packageService.GetCommTypes();
            var amttypes = packageService.GetAmtTypes();
            List<OperatorTypeDto> itemlist = new List<OperatorTypeDto>()
                {
                    new OperatorTypeDto() { TypeName="Package", TypeId=1 },                    
                    new OperatorTypeDto() { TypeName="Range", TypeId=3 },
            };
            model.OperatorList = oplist.Select(x => new OperatorDto { OperatorId = x.Id, OpCode = x.OpCode, OperatorName = x.Name }).ToList();
            model.CircleList = circlelist.Select(x => new CircleDto { CircleId = x.Id, CircleCode = x.CircleCode, CircleName = x.CircleName }).ToList();
            model.OpTypeList = itemlist.ToList();
            model.PackageList = packageService.GetPackageList().Where(x=>x.PTypeId==1).Select(x => new PackageDto {PackageName=x.PackageName,Id=x.Id }).ToList();
            model.PackId = package?.Id ?? 0;
            model.PackName = package?.PackageName ?? string.Empty;
            var pcommlist = pcomList.Select(x => new PackCommRangeDto()
            {
                Id = x.Id,
                PackId = x.PackId ?? 0,
                PackName = package?.PackageName,
                OpTypeIds = x.OpTypeFilter,
                OperatorIds = x.OpFilter,
                CircleIds = x.CircleFilter,
                AmountRange = x.MinVal + "-" + x.MaxVal,
                CommAmt = x.CommAmt ?? 0,
                CommTypeId = x.CommTypeId ?? 0,
                AmtTypeId = x.AmtTypeId ?? 0,
                CommTypeName = x.CommType.TypeName,
                AmtTypeName = x.AmtType.AmtTypeName,
                UpdatedOn = x.UpdatedDate?.ToString() ?? x.AddedDate.ToString(),
                UpdatedBy = x.User1?.UserProfile?.FullName ?? x.User?.UserProfile?.FullName ?? string.Empty
            }).ToList();

            if (pcommlist.Count > 0)
            {
                foreach (var item in pcommlist)
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
                                var opid = Convert.ToInt32(c.Trim());

                                if (opid > 0)
                                {
                                    var opname = oplist.Where(x => x.Id == opid).FirstOrDefault()?.Name;
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
                                var cid = Convert.ToInt32(c.Trim());

                                if (cid > 0)
                                {
                                    var cname = circlelist.Where(x => x.Id == cid).FirstOrDefault()?.CircleName;
                                    if (!string.IsNullOrEmpty(cname))
                                    {
                                        item.CircleNames = item.CircleNames + (string.IsNullOrEmpty(item.CircleNames) ? "" : ",") + cname;
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
                                var otid = Convert.ToInt32(c.Trim());

                                if (otid > 0)
                                {
                                    var optypename = optypelist.Where(x => x.Id == otid).FirstOrDefault()?.TypeName;
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

            model.PackCommRangeList = pcommlist;
            return View("PackCommUpdate", model);
        }

        [HttpPost]
        public int PackCommUpdate(PackCommRangeDto data)
        {
            UpdateActivity("Package PackCommUpdate REQUEST", "Post:Package/PackCommUpdate", "packid=" + data.PackId);
            ViewBag.actionAllowed = action = ActionAllowed("Package", CurrentUser.RoleId, data.PackId > 0 ? 3 : 2);
            var package = packageService.GetPackage(data.PackId);
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(data.AmountRange) || !data.AmountRange.Contains("-"))
                {
                    ShowErrorMessage("Error! ", "Enter Amount Range", true);
                }
                else
                {
                    var minval = Convert.ToInt32(data.AmountRange.Split('-')[0].Trim());
                    var maxval = Convert.ToInt32(data.AmountRange.Split('-')[1].Trim());
                    PackCommRangeDto packCommrange = new PackCommRangeDto();
                    
                    packCommrange.OpTypeIds = data.OpTypeIds;
                    packCommrange.OperatorIds = data.OperatorIds;
                    packCommrange.CircleIds = data.CircleIds;
                    packCommrange.MinAmt = minval;
                    packCommrange.MaxAmt = maxval;
                    packCommrange.PackName = data.PackName;
                    packCommrange.CommAmt = data.CommAmt;
                    packCommrange.CommTypeId = data.CommTypeId;
                    packCommrange.AmtTypeId = data.AmtTypeId;
                    PackageUpdate(packCommrange);
                    ShowSuccessMessage("Success!", "Data has been saved", false);
                }

                return 0;
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Package already exist.";
                    ShowErrorMessage("Error! ", message, true);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error! ", message, true);
                }
                return 1;
            }

        }

        public void PackageUpdate(PackCommRangeDto model)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("Package_Update", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PackageType", model.OpTypeIds);
                    cmd.Parameters.AddWithValue("@CircleId", model.CircleIds);
                    cmd.Parameters.AddWithValue("@CircleId", model.CircleIds);
                    cmd.Parameters.AddWithValue("@Opid", model.OperatorIds);
                    cmd.Parameters.AddWithValue("@Packid", model.PackName);
                    cmd.Parameters.AddWithValue("@MinAmt", model.MinAmt);
                    cmd.Parameters.AddWithValue("@CommAmt", model.CommAmt);
                    cmd.Parameters.AddWithValue("@MaxAmt", model.MaxAmt);
                    cmd.Parameters.AddWithValue("@UpdatebyId", CurrentUser.UserID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion
    }
}