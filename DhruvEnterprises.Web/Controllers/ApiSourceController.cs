using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.LIBS;
using DhruvEnterprises.Web.MobipactRechargeService;
using DhruvEnterprises.Web.Models.Others;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static DhruvEnterprises.Core.Enums;
namespace DhruvEnterprises.Web.Controllers
{

    public class ApiSourceController : BaseController
    {
        public ActionAllowedDto action;
        private readonly IUserService adminUserService;
        private readonly IApiService apiService;
        private readonly IPackageService packageService;
        private readonly IApiWalletService apiWalletService;
        private readonly IRequestResponseService reqResService;
        private readonly IOperatorSwitchService operatorSwitchService;

        private ITagValueService tagValueService;
        ActivityLogDto activityLogModel;
        public ApiSourceController
            (IUserService _adminUserService,
               IPackageService _packageService,
               ITagValueService _tagValueService,
               IActivityLogService _activityLogService,
               IRoleService _roleService,
               IApiService _apiService,
               IApiWalletService _apiWalletService,
               IRequestResponseService _reqResService,
               IOperatorSwitchService _operatorSwitchService) : base(_activityLogService, _roleService
            )
        {
            this.adminUserService = _adminUserService;
            this.apiService = _apiService;
            this.tagValueService = _tagValueService;
            this.packageService = _packageService;
            this.apiWalletService = _apiWalletService;
            this.reqResService = _reqResService;
            this.operatorSwitchService = _operatorSwitchService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
        }
        // GET: Api
        public ActionResult Index(int? i)
        {
            UpdateActivity("ApiSource List REQUEST", "Get:ApiSource/Index", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId);

            TempData["FetchAll"] = i.HasValue ? Convert.ToInt32(i) : 0;

            return View();
        }

        [HttpPost]
        public ActionResult GetApiSourceList(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.Roles.FirstOrDefault());
            var action2 = ActionAllowed("ApiWallet", CurrentUser.RoleId, 2);
            int i = TempData["FetchAll"] != null ? Convert.ToInt32(TempData["FetchAll"]) : 0;
            TempData["FetchAll"] = i;
            KeyValuePair<int, List<ApiSource>> apilist = apiService.GetApiSourceList(model, CurrentUser.UserID);

            // var apibals = apiWalletService.GetApiListWithBalance();

            var retlist = Json(new
            {
                draw = model.draw,

                recordsTotal = apilist.Key,
                recordsFiltered = apilist.Key,
                data = apilist.Value.Select(c => new List<object> {
                    c.Id,
                    c.ApiName,
                    c.Remark,
                    c.IsActive??false,
                    c.ApiBal??0 //(apibals.Where(x=>x.Id==c.Id).FirstOrDefault()?.cl_bal??0).ToString()
                    ,
                    (i>0 ?GetApiBalance(c.Id):0 )
                    ,
                    0,
                    c.CreditBal,
                    //c.IsAutoStatusCheck==true?"Yes":"No",
                    //c.StatusCheckTime??0,
                    //c.RequestGap??0,
                    (DataTableButton.RefreshVbalButton(Url.Action("index", "ApiSource",new { v = c.Id })))
                    +"&nbsp;"+
                   (action.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "ApiSource",new { id = c.Id })):string.Empty )
                    +"&nbsp;"+
                     (action2.AllowCreate?  DataTableButton.PlusButton2(Url.Action( "AddEdit", "ApiWallet",new { id = c.Id }),"modal-add-edit-addapiwallet","Add Wallet"):string.Empty )
                    +"&nbsp;"+
                   (action.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","ApiSource", new { id = c.Id }),"modal-delete-api"):string.Empty)
                   +"&nbsp;"+
                     DataTableButton.SettingButton(Url.Action("ApiOpcode","ApiSource", new { id = c.Id }),"Service Code")
                     // +"&nbsp;"+
                     //DataTableButton.SettingButton(Url.Action("CircleCode","ApiSource", new { id = c.Id }),"Circle Code","primary") 
                     +"&nbsp;"+
                    DataTableButton.SettingButton(Url.Action("PackCommRange","Package", new { id = c.ApiPackid??0 }),"Comm Amount Range","success")
                   , action.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);

            // apibals.Clear();

            return retlist;
        }

        [HttpGet]
        public ActionResult CreateEdit(int? id)
        { //aId=1=view, 2=create,3=edit,4=delete
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            ApiDto apiDtoModel = new ApiDto();
            UpdateActivity("ApiSource Add/Edit REQUEST", "Get:ApiSource/CreateEdit/" + id, "vendorid=" + id);

            try
            {
                ViewBag.ApiType = apiService.GetApiType(true);
                var apiUrlTypes = apiService.GetApiUrlType();
                if (id.HasValue && id.Value > 0)
                {
                    ApiSource apiSource = apiService.GetApiSource(id);
                    List<ApiUrl> apiURLs = apiService.ApiUrlList(id).ToList();
                    apiDtoModel.Id = apiSource.Id;
                    apiDtoModel.ApiName = apiSource.ApiName;
                    apiDtoModel.ApiUserId = apiSource.ApiUserId;
                    apiDtoModel.ApiTypeId = (byte)apiSource.ApiTypeId;
                    apiDtoModel.ApiPassword = apiSource.ApiPassword;
                    apiDtoModel.IsAutoStatusCheck = apiSource.StatusCheckTime > 0 ? true : false;
                    apiDtoModel.CheckTime = apiSource.StatusCheckTime ?? 0;
                    apiDtoModel.RequestGap = apiSource.RequestGap ?? 0;
                    apiDtoModel.BlockAmount = apiSource.BlockAmount ?? 0;
                    apiDtoModel.CallbackWaitTime = apiSource.WaitTime ?? 0;
                    apiDtoModel.Remark = apiSource.Remark;
                    apiDtoModel.ResendWaitTime = apiSource.ResendWaitTime ?? 0;
                    apiDtoModel.ContactPerson = apiSource.ContactPerson;
                    apiDtoModel.ContactNo = apiSource.ContactNo;
                    apiDtoModel.EmailId = apiSource.Email;
                    apiDtoModel.FullAddress = apiSource.FullAddress;
                    apiDtoModel.RequestDate = apiSource.ARequestTime;
                    if (apiUrlTypes != null && apiUrlTypes.Count > 0)
                    {
                        apiUrlTypes.ForEach(x =>
                        {
                            ApiURlDto apiURlDto = new ApiURlDto();
                            if (apiURLs.Any(p => p.ApiId == id && p.UrlTypeId == x.Id))
                            {
                                var url = apiURLs.Where(p => p.ApiId == id && p.UrlTypeId == x.Id).FirstOrDefault();
                                apiURlDto.ApiUrl = url.URL;
                                apiURlDto.apiurlid = url.Id;
                                apiURlDto.Method = url.Method;
                                apiURlDto.UrlTypeId = url.UrlTypeId ?? 0;
                                apiURlDto.ResType = url.ResType;
                                apiURlDto.PostData = url.PostData;
                            }
                            else
                            {
                                apiURlDto.Apiid = id ?? 0;
                                apiURlDto.Id = x.Id;
                                apiURlDto.ApiUrl = "";
                                apiURlDto.UrlTypeId = x.Id;
                                apiURlDto.TypeName = x.TypeName;
                                apiURlDto.ResType = "JSON";
                                apiURlDto.PostData = "";
                            }
                            apiURlDto.TypeName = apiUrlTypes.Where(y => y.Id == apiURlDto.UrlTypeId).FirstOrDefault().TypeName;
                            apiDtoModel.apiURlDtos.Add(apiURlDto);
                        });
                    }
                }
                else
                {
                    if (apiUrlTypes != null && apiUrlTypes.Count > 0)
                    {
                        apiUrlTypes.ForEach(x =>
                        {
                            ApiURlDto apiURlDto = new ApiURlDto();
                            apiURlDto.Apiid = id ?? 0;
                            apiURlDto.Id = x.Id;
                            apiURlDto.ApiUrl = "";
                            apiURlDto.UrlTypeId = x.Id;
                            apiURlDto.TypeName = x.TypeName;
                            apiURlDto.ResType = "JSON";
                            apiDtoModel.apiURlDtos.Add(apiURlDto);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return View(apiDtoModel);
        }

        [HttpPost]
        public ActionResult CreateEdit(List<ApiURlDto> model)
        {
            UpdateActivity("ApiSource Add/Edit REQUEST", "POST:ApiSource/CreateEdit/", "vendorid=" + model.FirstOrDefault()?.Id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, model.FirstOrDefault().Id > 0 ? 3 : 2);
            string message = string.Empty;
            try
            {
                var apidto = model.FirstOrDefault();
                if (ModelState.IsValid)
                {
                    ApiSource apiSource = apiService.GetApiSource(apidto.Id) ?? new ApiSource();
                    apiSource.Id = apidto.Id;
                    apiSource.ApiName = apidto.ApiName;
                    apiSource.ApiTypeId = (byte)apidto.ApiTypeId;
                    apiSource.ApiUserId = apidto.ApiUserId;
                    apiSource.ApiPassword = apidto.ApiPassword;
                    apiSource.AddedById = CurrentUser.UserID;
                    apiSource.Remark = apidto.Remark;
                    apiSource.IsAutoStatusCheck = apidto.CheckTime > 0 ? true : false;
                    apiSource.StatusCheckTime = apidto.CheckTime;
                    apiSource.RequestGap = apidto.RequestGap;
                    apiSource.BlockAmount = apidto.BlockAmount;
                    apiSource.WaitTime = apidto.CallbackWaitTime;
                    apiSource.ResendWaitTime = apidto.ResendWaitTime;
                    apiSource.ContactPerson = apidto.ContactPerson;
                    apiSource.ContactNo = apidto.ContactNo;
                    apiSource.Email = apidto.EmailId;
                    apiSource.FullAddress = apidto.FullAddress;
                    apiSource.ARequestTime = apidto.RequestDate;

                    apiService.Save(apiSource);
                    List<ApiUrl> apiUrls = new List<ApiUrl>();
                    foreach (var item in model)
                    {
                        ApiUrl apiUrl = apiService.GetApiurl(apiSource.Id, item.UrlTypeId) ?? new ApiUrl();
                        apiUrl.ApiId = apiSource.Id;
                        if (item.UrlTypeId == 4 && string.IsNullOrEmpty(apiUrl.URL))
                        {
                            var callbackid = Core.Common.GetUniqueAlphaNumeric();
                            apiUrl.URL = SiteKey.ApiDomainName + "service/reccallback/" + callbackid;
                            apiSource.CallbackId = callbackid;
                            apiService.Save(apiSource);

                        }
                        else if (item.UrlTypeId == 8 && string.IsNullOrEmpty(apiUrl.URL))
                        {
                            apiUrl.URL = SiteKey.ApiDomainName + "service/callbackcomplaint/" + apiSource.CallbackId;
                            apiService.Save(apiSource);

                        }
                        else if (item.UrlTypeId != 4 && item.UrlTypeId != 8)
                        {
                            apiUrl.URL = item.ApiUrl;
                        }

                        apiUrl.Id = apiUrl.Id;
                        apiUrl.AddedById = CurrentUser.UserID;
                        apiUrl.UrlTypeId = (byte)item.UrlTypeId;
                        apiUrl.Method = item.Method;
                        apiUrl.PostData = item.PostData;
                        apiUrl.ResType = item.ResType;
                        apiUrl.ContentType = item.ResType;

                        apiUrls.Add(apiUrl);

                    }
                    apiService.Save(apiUrls, apiSource.Id);
                    ShowSuccessMessage("Success!", "Vendor has been saved", false);
                }
                else
                {
                    ShowErrorMessage("Sorry!", "Invalid Parameters", false);
                }
                

            }
            catch (Exception ex)
            {
                LogException(ex);
                var msg = ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Api Name already exist.";
                    ModelState.AddModelError("error", message);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ModelState.AddModelError("error", message);
                }
            }
            return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("Index") });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            UpdateActivity("ApiSource Delete REQUEST", "GET:ApiSource/Delete/" + id, "vendorid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, 4);

            return PartialView("_ModalDelete", new Modal
            {
                Message = "Are you sure you want to delete this Api?",
                Size = ModalSize.Small,
                Header = new ModalHeader { Heading = "Delete Vendor" },
                Footer = new ModalFooter { SubmitButtonText = "Yes", CancelButtonText = "No" }
            });
        }

        public bool Active(int id)
        {
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id > 0 ? 3 : 2);

            UpdateActivity("ApiSource Active/Inactive REQUEST", "GET:ApiSource/Active/", "apiid=" + id);

            if (!action.AllowEdit)
            {
                return false;
            }
            else
            {
                string message = string.Empty;
                try
                {
                    var apisource = apiService.GetApiSource(id);
                    apisource.IsActive = !(apisource.IsActive ?? false);
                    return apiService.Save(apisource)?.IsActive ?? false;

                }
                catch (Exception)
                {
                    return false;
                }
            }

        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteApi(int id)
        {
            UpdateActivity("ApiSource Delete REQUEST", "POST:ApiSource/Delete/" + id, "vendorid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, 4);

            try
            {
                apiService.DeleteApiTag(id);
                apiService.DeleteApiUrl(id);
                bool IsValue = apiService.DeleteApiSource(id);
                if (IsValue == true)
                {
                    ShowSuccessMessage("Success", "Vendor has been deleted.", false);
                }
                else
                {
                    ShowSuccessMessage("Success", "Vendor cann't be deleted.", false);
                }
                //ShowSuccessMessage("Success", "hola", false);
            }
            catch (Exception)
            {
                //ShowErrorMessage("Error Occurred", "", false);
            }
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult TagValueSetting(int? id)
        {
            var apiurl = apiService.GetApiurl(id ?? 0);

            UpdateActivity("ApiSource TagValueSetting REQUEST", "GET:ApiSource/TagValueSetting/" + id, "urlid=" + id + "apiid=" + apiurl?.ApiId);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            TagValueSettingDto tagValueSettingDto = new TagValueSettingDto();


            List<TagValue> TagValue = tagValueService.GetApiWithUrlList(id).ToList();
            tagValueSettingDto.ApiId = apiurl?.ApiId ?? 0;
            tagValueSettingDto.UrlId = apiurl.Id;

            foreach (var item in TagValue)
            {
                TagValueDto tagValueDto = new TagValueDto();
                tagValueDto.TagId = item.TagId ?? 0;
                tagValueDto.PreTxt = item.PreTxt;
                tagValueDto.PostText = item.PostText;
                tagValueDto.PreMargin = item.PreMargin;
                tagValueDto.PostMargin = item.PostMargin;
                tagValueDto.PreTxt = item.PreTxt;
                tagValueDto.TagMsg = item.TagMsg;
                tagValueDto.Name = item.Tag.Name;
                tagValueDto.CompareTxt = item.CompareTxt;

                tagValueDto.ResSeparator = item.ResSeparator;
                tagValueDto.TagIndex = item.TagIndex ?? 0;

                tagValueSettingDto.tagValueDtos.Add(tagValueDto);
            }

            ViewBag.GetTagList = tagValueService.GetTagList().Where(x => !x.Name.ToLower().Contains("pending"));
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName }).ToList();

            return View(tagValueSettingDto);
        }
        [HttpPost]
        public bool TagValueSetting(List<TagValueDto> data)
        {
            UpdateActivity("ApiSource TagValueSetting REQUEST", "POST:ApiSource/TagValueSetting/", "data.count=" + data.Count);
            //aId=1=view, 2=create,3=edit,4=delete
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, data.FirstOrDefault().UrlId > 0 ? 3 : 2);

            try
            {
                if (data != null)
                {
                    List<TagValue> tagValues = new List<TagValue>();
                    var data1 = data.FirstOrDefault();
                    foreach (var item in data)
                    {
                        TagValue tagValue = new TagValue();
                        tagValue.TagId = item.TagId;
                        tagValue.ApiId = item.ApiId;
                        tagValue.UrlId = item.UrlId;
                        tagValue.PreTxt = item.PreTxt.HtmlDecode();
                        tagValue.PostText = item.PostText.HtmlDecode();
                        tagValue.PreMargin = item.PreMargin;
                        tagValue.PostMargin = item.PostMargin;
                        tagValue.TagMsg = item.TagMsg;
                        tagValue.CompareTxt = item.CompareTxt;
                        tagValue.ResSeparator = item.ResSeparator;
                        tagValue.TagIndex = item.TagIndex;
                        tagValue.AddedById = CurrentUser.UserID;
                        tagValues.Add(tagValue);
                    }
                    tagValueService.Save(tagValues, data1.UrlId, data1.ApiId);

                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }

        }

        [HttpGet]
        public ActionResult ApiOpcode(int? id)
        {
            UpdateActivity("ApiOpcode REQUEST", "GET:ApiSource/ApiOpcode/" + id, "vendorid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id.HasValue ? 3 : 2);


            List<OperatorCode> operatorCodes = new List<OperatorCode>();
            operatorCodes = packageService.ApioperatorCodes(id ?? 0).ToList();
            List<Operator> operators = packageService.GetOperatorList().ToList();
            ApiSource apiSource = apiService.GetApiSource(id);
            Package package = packageService.GetPackage(apiSource.ApiPackid);
            List<PackageComm> packageComms = new List<PackageComm>();
            List<PackageCommDto> ApiPackageCommDtos = new List<PackageCommDto>();
            PackageDto packageModel = new PackageDto();
            if (package != null)
            {
                packageModel.Id = package.Id;
                packageComms = packageService.PackageCommList(package.Id, 0, 0, id ?? 0, 0).ToList();
                foreach (var operatorr in operators)
                {
                    var pcomm = packageComms?.Where(p => p.PackId == apiSource.ApiPackid && p.OpId == operatorr.Id)?.FirstOrDefault();
                    var opcode = operatorCodes?.Where(p => p.ApiId == id && p.OpId == operatorr.Id)?.FirstOrDefault();


                    PackageCommDto apiPackageCommDto = new PackageCommDto();
                    apiPackageCommDto.ApiID = id ?? 0;
                    apiPackageCommDto.OpId = operatorr.Id;
                    apiPackageCommDto.OperatorName = operatorr.Name;
                  

                    if (opcode != null)
                    {
                        apiPackageCommDto.ApiOpcode = opcode.OpCode;
                        apiPackageCommDto.ExtraUrl = opcode.ExtraUrl;
                        apiPackageCommDto.ExtraData = opcode.ExtraUrlData;
                        apiPackageCommDto.MaxQSize = opcode.MaxQSize ?? 0;
                      
                    }
                    else
                    {
                        apiPackageCommDto.ApiOpcode = string.Empty;
                        apiPackageCommDto.ExtraUrl = string.Empty;
                        apiPackageCommDto.ExtraData = string.Empty;

                    }
                    if (pcomm != null)
                    {
                        apiPackageCommDto.CommTypeId = pcomm.CommTypeId ?? 0;
                        apiPackageCommDto.AmtTypeId = pcomm.AmtTypeId ?? 0;
                        apiPackageCommDto.CommAmt = pcomm.CommAmt ?? 0;
                        apiPackageCommDto.IsAmtWiseComm = pcomm.IsAmtWiseComm ?? false;
                    }
                    else
                    {
                        apiPackageCommDto.CommAmt = 0;
                        apiPackageCommDto.CommTypeId = 0;
                        apiPackageCommDto.AmtTypeId = 0;
                    }
                    packageModel.PackageCommList.Add(apiPackageCommDto);
                }
            }
            else
            {
                packageModel.Id = 0;
                foreach (var operatorr in operators)
                {
                    PackageCommDto apiPackageCommDto = new PackageCommDto();
                    apiPackageCommDto.ApiID = id ?? 0;
                    apiPackageCommDto.OpId = operatorr.Id;
                    apiPackageCommDto.OperatorName = operatorr.Name;
                    if (operatorCodes.Any(p => p.ApiId == id && p.OpId == operatorr.Id))
                    {
                        apiPackageCommDto.ApiOpcode = operatorCodes.Where(p => p.ApiId == id && p.OpId == operatorr.Id).FirstOrDefault().OpCode;
                    }
                    else
                    {
                        apiPackageCommDto.ApiOpcode = "";
                    }
                    apiPackageCommDto.CommAmt = 0;
                    packageModel.PackageCommList.Add(apiPackageCommDto);
                }
            }

            return View(packageModel);
        }

        [HttpPost]
        public bool ApiOpcode(List<PackageCommDto> data)
        {
            UpdateActivity("ApiOpcode REQUEST", "POST:ApiSource/ApiOpcode/", "apiid=" + data?.FirstOrDefault()?.ApiID ?? string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, data.FirstOrDefault().ApiID > 0 ? 3 : 2);

            string message = string.Empty;

            try
            {
                var packg = data.FirstOrDefault();
                ApiSource apiSource = apiService.GetApiSource(packg.ApiID);

                Package package = packageService.GetPackage(packg.PackId) ?? new Package();

                package.Id = packg.PackId;
                package.PackageName = apiSource.ApiName + "-" + apiSource.Id;

                if (package.Id > 0)
                    package.UpdatedById = CurrentUser.UserID;
                else
                    package.AddedById = CurrentUser.UserID;

                packageService.Save(package);
                apiSource.ApiPackid = package.Id;
                apiService.Save(apiSource);

                foreach (var packcomm in data)
                {
                    PackageComm packageComm = packageService.GetPackageCommByOpId(package.Id, packcomm.OpId) ?? new PackageComm();
                    packageComm.PackId = package.Id;
                    packageComm.OpId = packcomm.OpId;
                    packageComm.CommAmt = packcomm.CommAmt;
                    packageComm.CommTypeId = packcomm.CommTypeId;
                    packageComm.AmtTypeId = packcomm.AmtTypeId;
                    packageComm.IsAmtWiseComm = packcomm.IsAmtWiseComm;

                    if (packageComm.Id > 0)
                        packageComm.UpdatedById = CurrentUser.UserID;
                    else
                        packageComm.AddedById = CurrentUser.UserID;

                    packageService.Save(packageComm);
                    OperatorCode operatorCode = packageService.GetOperatorCode(packg.ApiID, packcomm.OpId) ?? new OperatorCode();
                    operatorCode.OpId = packcomm.OpId;
                    operatorCode.OpCode = packcomm.ApiOpcode;
                    operatorCode.ApiId = packg.ApiID;

                    operatorCode.ExtraUrl = packcomm.ExtraUrl?.Trim();
                    operatorCode.ExtraUrlData = packcomm.ExtraData?.Trim();
                    operatorCode.MaxQSize = packcomm.MaxQSize;
                  

                    if (operatorCode.Id > 0)
                        operatorCode.UpdatedById = CurrentUser.UserID;
                    else
                        operatorCode.AddedById = CurrentUser.UserID;

                    packageService.Save(operatorCode);
                }

                //ShowSuccessMessage("Success!", "Package has been saved", false);
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

        private long UpdateActivity(string title, string action, string remark = "", long id = 0)
        {
            try
            {
                activityLogModel.Id = id;
                activityLogModel.ActivityName = title;
                activityLogModel.ActivityPage = action;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                activityLogModel = LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }
            return activityLogModel.Id;
        }

        [HttpPost]
        public decimal FetchApiBalance(int? id)
        {
            UpdateActivity("ApiSource FetchApiBalance REQUEST", "GET:ApiSource/FetchApiBalance/" + id, string.Empty);

            return GetApiBalance(id ?? 0);
        }

        [HttpGet]
        public ActionResult VendorBalance()
        {
            return View();
        }

        [HttpPost]
        public JObject GetApiBalanceList()
        {

            JObject response = new JObject();
            var apilist = VenderBalance();

            // var apibals = apiWalletService.GetApiListWithBalance();
            response = JObject.FromObject(new
            {
                apilist
            });

            return response;
        }

        public DataTable VenderBalance()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("VenderBalanceList", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);
                    con.Close();
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        private decimal GetApiBalance(int id)
        {
            string status = string.Empty;
            decimal Vendor_CL_Bal = 0;
            decimal Vendor_OP_Bal = 0;

            int statusId = 4;
            string apitxnid = string.Empty;
            string statusmsg = string.Empty;
            string optxnid = string.Empty;
            string log = "Fetch Vendor Balance-start";
            try
            {
                var apiurl = apiService.GetApiurl(id, 2);
                var apisource = apiService.GetApiSource(id);

                #region "APi Call"

                RequestResponseDto reqres = new RequestResponseDto();

                ApiCall apiCall = new ApiCall(reqResService);

                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

                statusId = 3;
                int unitid = 0, len = 0, reflen = 0;
                string randomkey = "", dtformat = "", refpadding = "", apiamount = "", ourref = "", datetime = "";

                GetApiExtDetails(apisource.Id, ref unitid, ref len, ref randomkey, ref dtformat, ref refpadding, ref log, ref reflen);

                datetime = DateTime.Now.ToString(dtformat);

                string apires = string.Empty;
                string balcheckurl = string.Empty;
                string postdata = string.Empty;

                balcheckurl = apiurl.URL?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                postdata = apiurl.PostData?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

                balcheckurl = balcheckurl?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                postdata = postdata?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

                if (apisource.Id == 12)
                {
                    RechargeAPI mobiPactservice = new RechargeAPI();
                    apires = mobiPactservice.Balance(apisource.ApiUserId, apisource.ApiPassword);
                    reqres.ResponseText = apires;

                }
                else if (apisource.Id == 11)
                {
                    apires = apiurl.Method == "POST" ? apiCall.Post(balcheckurl, postdata, apiurl.ContentType, apiurl.ResType, ref reqres, id, apisource.ApiUserId, apisource.ApiPassword)
                                         : apiCall.Get(balcheckurl, postdata);

                }
                else
                {

                    apires = apiurl.Method == "POST" ? apiCall.Post(balcheckurl, postdata, apiurl.ContentType, apiurl.ResType, ref reqres, id, apisource.ApiUserId, apisource.ApiPassword)
                                                              : apiCall.Get(balcheckurl, ref reqres, id, apiurl.ContentType, apiurl.ResType, apisource.ApiUserId, apisource.ApiPassword);

                }

                reqres.UrlId = apiurl.Id;
                reqres.UserId = CurrentUser.UserID;
                reqres.RequestTxt = "URL: " + balcheckurl + "DATA: " + postdata;
                reqres.Remark = "Balance_V";
                log += "\r\n request.Url=" + balcheckurl;
                log += "\r\n PostData=" + postdata;
                reqres = AddUpdateReqRes(reqres, ref log);

                // var resp2 = mobiPactservice.Recharge("Username=8386900044", "555959", "7976155877", 9, string.Empty, "RJ", "test231219170001");

                #endregion

                string reqtxnid = FilterRespTagValue(id, apiurl.Id, apiurl.ResType, apires, ref status, ref statusId, ref apitxnid, ref statusmsg, ref optxnid, ref log, ref Vendor_CL_Bal, ref Vendor_OP_Bal);

            }
            catch (Exception ex)
            {
                log += "\r\n excp=" + ex.Message;
                LogException(ex);
            }

            log += "\r\n end";
            LogActivity(log);

            log = string.Empty;

            return Vendor_CL_Bal;
        }

        private string FilterRespTagValue(int apiid, int UrlId, string resType, string apires, ref string status, ref int statusId, ref string apitxnid, ref string statusmsg, ref string optxnid, ref string log, ref decimal Vendor_CL_Bal, ref decimal Vendor_OP_Bal)
        {
            string reqtxnid = "";
            var tagvalues = tagValueService.GetTagValuesByUrlId(apiid, UrlId);

            //  var tags = tagValueService.GetTagValuesByUrlId(apiid, apiurl.Id);
            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")ResType=" + resType + ",";

            if (resType.ToLower().Contains("split"))
            {

                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();

                    int index = Convert.ToInt16((tg.TagIndex ?? 0)) - 1;

                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Replace(" ", string.Empty).Split(',').Where(x => x != string.Empty).ToList();
                    }

                    if (index >= 0)
                    {
                        log += "\r\nindex=" + index + ",";
                        if (tg.TagId == 1) //status-success
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);

                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-1=" + status + ", ";

                                string sval = status.ToLower();

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 1;
                                }

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")sucess status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 4)//status-failed
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-3=" + status + ", ";
                                string sval = status.ToLower();
                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 3;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")failed status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == 2) //status-processing
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-2=" + status + ", ";

                                string sval = status.ToLower();
                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 2;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")processing status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 3)//status-pending
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-5=" + status + ", ";
                                string sval = status.ToLower();
                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 5;
                                }
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")pending status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 5)//api txn id
                        {
                            try
                            {
                                apitxnid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + apitxnid + ", ";
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")api txnid expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == 6) //operator txn id
                        {
                            try
                            {
                                optxnid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")optxnid=" + optxnid + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\noptr txnid expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == 7) //status message
                        {

                            try
                            {
                                statusmsg = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + statusmsg + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 8) //request txn id
                        {

                            try
                            {
                                reqtxnid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + reqtxnid + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 9) //Vendor_CL_Bal
                        {

                            try
                            {
                                string clbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                                clbal = clbal.Length > 199 ? statusmsg.Substring(0, 198) : clbal;
                                Vendor_CL_Bal = Convert.ToDecimal(clbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 10) //Vendor_OP_Bal
                        {

                            try
                            {
                                string opbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                                opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                                Vendor_OP_Bal = Convert.ToDecimal(opbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                    }

                }
            }
            else
            {
                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")tagvalue retrival,";
                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();
                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Replace(" ", string.Empty).Split(',').Where(x => x != string.Empty).ToList();
                    }
                    if (tg.TagId == 1) //status-success
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-1=" + status + ", ";

                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")sucess status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 4)//status-failed
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-3=" + status + ", ";
                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 3;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")failed status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == 2) //status-processing
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-2=" + status + ", ";
                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 2;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")processing status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 3)//status-pending
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-5=" + status + ", ";
                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 5;
                            }
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")pending status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 5)//api txn id
                    {
                        try
                        {

                            apitxnid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + apitxnid + ", ";
                            apitxnid = apitxnid.Length > 50 ? apitxnid.Substring(0, 45) : apitxnid;
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")api txnid expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == 6) //operator txn id
                    {
                        try
                        {
                            optxnid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\noptxnid=" + optxnid + ", ";
                            optxnid = optxnid.Length > 50 ? optxnid.Substring(0, 45) : optxnid;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")optr txnid expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 7) //status message
                    {

                        try
                        {
                            statusmsg = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + statusmsg + ", ";
                            statusmsg = statusmsg.Length > 199 ? statusmsg.Substring(0, 198) : statusmsg;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 8) //request txn id
                    {

                        try
                        {
                            reqtxnid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + reqtxnid + ", ";
                            reqtxnid = reqtxnid.Length > 50 ? reqtxnid.Substring(0, 45) : reqtxnid;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 9) //Vendor_CL_Bal
                    {

                        try
                        {
                            string clbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                            clbal = clbal.Length > 199 ? statusmsg.Substring(0, 198) : clbal;
                            Vendor_CL_Bal = Convert.ToDecimal(clbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 10) //Vendor_OP_Bal
                    {

                        try
                        {
                            string opbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                            opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                            Vendor_OP_Bal = Convert.ToDecimal(opbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                }
            }

            statusmsg = "status=" + status + " and message=" + statusmsg;

            return reqtxnid;
        }

        private RequestResponseDto AddUpdateReqRes(RequestResponseDto model, ref string log, string filter = "NA")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_UpdateRecDetailToReqRes", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@UrlId", model.UrlId);
                    cmd.Parameters.AddWithValue("@RecId", model.RecId);
                    cmd.Parameters.AddWithValue("@RefId", model.RefId);
                    cmd.Parameters.AddWithValue("@Remark", model.Remark);
                    cmd.Parameters.AddWithValue("@RequestTxt", model.RequestTxt);
                    cmd.Parameters.AddWithValue("@ResponseText", model.ResponseText);
                    cmd.Parameters.AddWithValue("@FilterType", filter);
                    cmd.Parameters.Add("@CurrentId", SqlDbType.BigInt).Direction = ParameterDirection.Output;

                    log += "\r\n ,  before execute = usp_UpdateRecDetailToReqRes";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after execute";
                    string CurrentId = Convert.ToString(cmd.Parameters["@CurrentId"].Value);
                    if (model.Id == 0)
                        model.Id = !string.IsNullOrWhiteSpace(CurrentId) ? Convert.ToInt64(CurrentId) : 0;

                    log += "\r\n ,  reqres Id=" + model.Id;
                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp=" + ex.Message;
                LogException(ex);
            }

            return model;

        }

        private void GetApiExtDetails(int apiid, ref int unitid, ref int len, ref string randomkey, ref string dtformat, ref string refpadding, ref string log, ref int reflen)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetApiValidation", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiId", apiid);
                    cmd.Parameters.Add("@AmountUnitId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@AmountLength", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RandomKey", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@DateTimeFormat", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RefPadding", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RefLength", SqlDbType.Int).Direction = ParameterDirection.Output;

                    // log += "\r\n ,  before execute = usp_UpdateRecDetailToReqRes";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    // log += "\r\n ,  after execute";
                    string AmountUnitId = Convert.ToString(cmd.Parameters["@AmountUnitId"].Value);
                    string AmountLength = Convert.ToString(cmd.Parameters["@AmountLength"].Value);
                    randomkey = Convert.ToString(cmd.Parameters["@RandomKey"].Value);
                    string DateTimeFormat = Convert.ToString(cmd.Parameters["@DateTimeFormat"].Value);
                    refpadding = Convert.ToString(cmd.Parameters["@RefPadding"].Value);
                    string RefLength = Convert.ToString(cmd.Parameters["@RefLength"].Value);

                    unitid = !string.IsNullOrWhiteSpace(AmountUnitId) ? Convert.ToInt32(AmountUnitId) : 0;
                    len = !string.IsNullOrWhiteSpace(AmountLength) ? Convert.ToInt32(AmountLength) : 0;
                    reflen = !string.IsNullOrWhiteSpace(RefLength) ? Convert.ToInt32(RefLength) : 0;
                    dtformat = !string.IsNullOrWhiteSpace(DateTimeFormat) ? DateTimeFormat : "yyyyMMddHHmmss";


                    // log += "\r\n ,  reqres Id=" + model.Id;
                }
            }
            catch (Exception ex)
            {
                // log += "\r\n , excp=" + ex.Message;
                LogException(ex);
            }

        }

        // GET: Api
        public ActionResult LowVendor()
        {
            UpdateActivity("ApiSource List REQUEST", "Get:ApiSource/Index", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId);

            // TempData["FetchAll"] = i.HasValue ? Convert.ToInt32(i) : 0;

            return View();
        }

        [HttpPost]
        public ActionResult GetLowVendors(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId);

            //int i = TempData["FetchAll"] != null ? Convert.ToInt32(TempData["FetchAll"]) : 0;
            //TempData["FetchAll"] = i;
            KeyValuePair<int, List<ApiSource>> apilist = apiService.GetApiSourceList(model, CurrentUser.UserID, true);

            // var apibals = apiWalletService.GetApiListWithBalance();

            var retlist = Json(new
            {
                draw = model.draw,

                recordsTotal = apilist.Key,
                recordsFiltered = apilist.Key,
                data = apilist.Value.Select(c => new List<object> {
                    c.Id,
                    c.ApiName,
                    c.Remark,
                    c.BlockAmount??0,
                    c.ApiBal??0
                })
            }, JsonRequestBehavior.AllowGet);

            return retlist;
        }

        [HttpGet]
        public ActionResult CircleCode(int? id)
        {
            UpdateActivity("ApiOpcode REQUEST", "GET:ApiSource/Circlecode/" + id, "vendorid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            CircleCodeModel model = new CircleCodeModel();
            var circleCodes = apiService.GetCircleCodeList(id ?? 0);
            var circlelist = operatorSwitchService.circlesList();
            var apiSource = apiService.GetApiSource(id);
            var package = packageService.GetPackage(apiSource.ApiPackid);

            model.PackId = package?.Id ?? 0;
            model.ApiId = apiSource.Id;
            model.PackageName = package?.PackageName ?? string.Empty;
            model.ApiName = apiSource.ApiName;

            foreach (var circle in circlelist)
            {
                CircleCodeDto codeDto = new CircleCodeDto();
                if (circleCodes.Any(x => x.ApiId == id && x.CircleId == circle.Id))
                {
                    var circlecode = circleCodes.Where(x => x.ApiId == id && x.CircleId == circle.Id).FirstOrDefault();

                    codeDto.Id = circlecode.Id;
                    codeDto.CircleId = circlecode.CircleId ?? 0;
                    codeDto.CircleName = circle.CircleName;
                    codeDto.CircleCode = !string.IsNullOrEmpty(circlecode.CircleCode1) ? circlecode.CircleCode1 : circle.CircleCode;
                    codeDto.ApiId = apiSource.Id;
                    codeDto.ApiName = apiSource.ApiName;
                    codeDto.PackId = package?.Id ?? 0;
                    codeDto.PackageName = package?.PackageName ?? string.Empty;
                    codeDto.ExtraUrl = circlecode.ExtraUrl;
                    codeDto.ExtraData = circlecode.ExtraUrlData;
                    model.CircleCodes.Add(codeDto);
                }
                else
                {
                    codeDto.Id = 0;
                    codeDto.CircleId = circle.Id;
                    codeDto.CircleName = circle.CircleName;
                    codeDto.CircleCode = string.Empty;
                    codeDto.ApiId = apiSource.Id;
                    codeDto.ApiName = apiSource.ApiName;
                    codeDto.PackId = package?.Id ?? 0;
                    codeDto.PackageName = package?.PackageName ?? string.Empty;
                    codeDto.ExtraUrl = string.Empty;
                    codeDto.ExtraData = string.Empty;
                    model.CircleCodes.Add(codeDto);
                }
            }

            return View(model);
        }

        [HttpPost]
        public bool CircleCode(List<CircleCodeDto> data)
        {
            UpdateActivity("ApiOpcode REQUEST", "POST:ApiSource/Circlecode/", "apiid=" + data?.FirstOrDefault()?.ApiId ?? string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, data.FirstOrDefault().ApiId > 0 ? 3 : 2);

            string message = string.Empty;

            try
            {
                var packg = data.FirstOrDefault();
                ApiSource apiSource = apiService.GetApiSource(packg.ApiId);

                //  var circlelist = operatorSwitchService.circlesList();

                foreach (var item in data)
                {
                    var circlecode = apiService.GetCircleCodeList(item.ApiId, item.CircleId).FirstOrDefault();
                    circlecode = circlecode != null ? circlecode : new CircleCode();
                    circlecode.CircleId = item.CircleId;
                    circlecode.ApiId = item.ApiId;
                    circlecode.CircleCode1 = item.CircleCode;
                    circlecode.ExtraUrl = item.ExtraUrl;
                    circlecode.ExtraUrlData = item.ExtraData;
                    circlecode.UpdatedById = CurrentUser.UserID;
                    apiService.Save(circlecode);
                }

                ShowSuccessMessage("Success!", "Data has been saved", false);
                return true;
            }
            catch (Exception ex)
            {
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error! ", message, true);
                LogException(ex, "save circle code");
                return false;
            }
        }

        [HttpGet]
        public ActionResult TestResponse(int? id, string apires)
        {
            UpdateActivity("TestResponse", "GET:ApiSource/TestResponse/" + id, "url=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            var apiurl = apiService.GetApiurl(id ?? 0);

            FilterResponseModel model = new FilterResponseModel();
            int statusId = 0;
            string log = string.Empty;

            FilterRespTagValue(apiurl.ApiId ?? 0, apiurl.Id, apiurl.ResType, apires, ref statusId, ref log, ref model);

            return PartialView("_TestResponse", model);
        }

        [HttpGet]
        public int CopyResponse(int? id, int? apiid)
        {
            UpdateActivity("TestResponse", "GET:ApiSource/TestResponse/" + id, "url=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("ApiSource", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            try
            {
                var apiurl = apiService.GetApiurl(id ?? 0);
                var newulr = apiService.GetApiurl(apiid ?? 0, apiurl.UrlTypeId ?? 0);

                if (newulr == null)
                {
                    return 0;
                }
                else
                {
                    var data = tagValueService.GetTagValuesByUrlId(apiurl.ApiId ?? 0, id ?? 0).Select(x => new TagValueDto()
                    {
                        TagId = x.TagId ?? 0,
                        ApiId = newulr.ApiId ?? 0,
                        UrlId = newulr.Id,
                        PreTxt = x.PreTxt,
                        PostText = x.PostText,
                        PreMargin = x.PreMargin,
                        PostMargin = x.PostMargin,
                        TagMsg = x.TagMsg,
                        CompareTxt = x.CompareTxt,
                        ResSeparator = x.ResSeparator,
                        TagIndex = x.TagIndex ?? 0

                    }).ToList();

                    if (data != null)
                    {
                        List<TagValue> tagValues = new List<TagValue>();
                        var data1 = data.FirstOrDefault();
                        foreach (var item in data)
                        {
                            TagValue tagValue = new TagValue();
                            tagValue.TagId = item.TagId;
                            tagValue.ApiId = item.ApiId;
                            tagValue.UrlId = item.UrlId;
                            tagValue.PreTxt = item.PreTxt;
                            tagValue.PostText = item.PostText;
                            tagValue.PreMargin = item.PreMargin;
                            tagValue.PostMargin = item.PostMargin;
                            tagValue.TagMsg = item.TagMsg;
                            tagValue.CompareTxt = item.CompareTxt;
                            tagValue.ResSeparator = item.ResSeparator;
                            tagValue.TagIndex = item.TagIndex;
                            tagValue.AddedById = CurrentUser.UserID;
                            tagValues.Add(tagValue);
                        }
                        tagValueService.Save(tagValues, data1.UrlId, data1.ApiId);

                    }

                    return newulr.Id;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "copy response settings");

                return -1;
            }


        }

        private string FilterRespTagValue(int apiid, int UrlId, string resType, string apires, ref int statusId, ref string log, ref FilterResponseModel resF)
        {
            if (apiid == 67000)
            {
                apires = apires.Remove(apires.IndexOf("BEGIN SIGNATURE"));
            }
            var tagvalues = tagValueService.GetTagValuesByUrlId(apiid, UrlId);
            // var tagvalues = GetTagValueListOfApiUrl(apiid, UrlId);
            //  var tags = tagValueService.GetTagValuesByUrlId(apiid, apiurl.Id);
            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")ResType=" + resType + ",";

            if (resType.ToLower().Contains("split"))
            {

                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();

                    int index = tg.TagIndex ?? 0 - 1;

                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Split(',').Where(x => x != string.Empty).ToList();
                    }

                    if (index >= 0)
                    {
                        log += "\r\n index=" + index + ",";
                        if (tg.TagId == TAGName.SUCCESS) //status-success
                        {
                            try
                            {
                                string sval = resF.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    resF.StatusId = statusId = StatsCode.SUCCESS;
                                }

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS expc= " + ex.Message;
                                //   LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.FAILED)//status-failed
                        {
                            try
                            {
                                string sval = resF.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    resF.StatusId = statusId = StatsCode.FAILED;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED expc= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == TAGName.PROCESSING) //status-processing
                        {
                            try
                            {
                                string sval = resF.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    resF.StatusId = statusId = StatsCode.PROCESSING;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING expc= " + ex.Message;
                                //LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.PENDING)//status-pending
                        {
                            try
                            {
                                string sval = resF.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    resF.StatusId = statusId = 5;
                                }
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING expc= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.APITXNID)//api txn id
                        {
                            try
                            {
                                var sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                resF.ApiTxnID = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);


                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + resF.ApiTxnID + ", ";
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")api-txnid expc= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == TAGName.OPERATORTXNID) //operator txn id
                        {
                            try
                            {
                                var sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = HttpUtility.UrlDecode(sval);
                                sval = sval.Trim();
                                resF.OperatorTxnID = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);


                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")optxnid=" + resF.OperatorTxnID + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n optr txnid expc= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == TAGName.MESSAGE) //status message
                        {

                            try
                            {
                                var sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                resF.Message = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + resF.Message + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.REQUESTTXNID) //request txn id
                        {

                            try
                            {
                                var sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                resF.RequestTxnId = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + resF.RequestTxnId + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.VENDOR_CL_BAL) //Vendor_CL_Bal
                        {

                            try
                            {
                                string clbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                clbal = clbal.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                                clbal = clbal.Length > 199 ? clbal.Substring(0, 198) : clbal;
                                resF.Vendor_CL_Bal = Convert.ToDecimal(clbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.VENDOR_OP_BAL) //Vendor_OP_Bal
                        {
                            try
                            {
                                string opbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                opbal = opbal.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                                opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                                resF.Vendor_OP_Bal = Convert.ToDecimal(opbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.LAPUNO) //Lapu Number
                        {

                            try
                            {
                                string lapuno = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                lapuno = lapuno.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno=" + lapuno + ", ";
                                resF.LapuNo = lapuno.Length > 50 ? lapuno.Substring(0, 45) : lapuno;
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.COMPLAINT_ID)
                        {

                            try
                            {
                                string cid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                cid = cid.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID=" + cid + ", ";
                                resF.Complaint_Id = cid.Length > 50 ? cid.Substring(0, 45) : cid;
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.R_OFFER)
                        {

                            try
                            {
                                string ro = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                ro = ro.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer=" + ro + ", ";
                                ro = ro.Length > 50 ? ro.Substring(0, 45) : ro;
                                resF.R_Offer = Convert.ToDecimal(ro);

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.CUSTOMER_NUMBER)
                        {

                            try
                            {
                                string cno = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                cno = cno.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NUMBER=" + cno + ", ";
                                cno = cno.Length > 100 ? cno.Substring(0, 99) : cno;
                                resF.CustomerNo = cno;

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NUMBER excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.CUSTOMER_NAME)
                        {

                            try
                            {
                                string cname = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                cname = cname.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NAME=" + cname + ", ";
                                cname = cname.Length > 50 ? cname.Substring(0, 49) : cname;
                                resF.CustomerName = cname;

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NAME excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.BILL_NUMBER)
                        {

                            try
                            {
                                string billno = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                billno = billno.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_NUMBER=" + billno + ", ";
                                billno = billno.Length > 50 ? billno.Substring(0, 49) : billno;
                                resF.BillNo = billno;

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_NUMBER excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.BILL_PERIOD)
                        {

                            try
                            {
                                string billperiod = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                billperiod = billperiod.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PERIOD=" + billperiod + ", ";
                                billperiod = billperiod.Length > 50 ? billperiod.Substring(0, 49) : billperiod;
                                resF.BillPeriod = billperiod;

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PERIOD excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.BILL_PRICE)
                        {

                            try
                            {
                                string price = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                price = price.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PRICE=" + price + ", ";
                                price = price.Length > 50 ? price.Substring(0, 49) : price;
                                resF.BillPrice = Convert.ToDecimal(price);

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PRICE excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.BILL_DATE)
                        {

                            try
                            {
                                string billdate = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                billdate = billdate.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_DATE=" + billdate + ", ";
                                billdate = billdate.Length > 50 ? billdate.Substring(0, 49) : billdate;
                                resF.BillDate = billdate;

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_DATE excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.BILL_DUE_DATE)
                        {

                            try
                            {
                                string duedate = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                duedate = duedate.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")DUE_DATE=" + duedate + ", ";
                                duedate = duedate.Length > 50 ? duedate.Substring(0, 49) : duedate;
                                resF.BillDueDate = duedate;

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")DUE_DATE excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }

                    }

                }
            }
            else
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")tagvalue retrival,";
                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();
                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Split(',').Where(x => x != string.Empty).ToList();
                    }
                    if (tg.TagId == TAGName.SUCCESS) //status-success
                    {
                        try
                        {
                            string sval = resF.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS=" + sval + ", ";

                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                resF.StatusId = statusId = StatsCode.SUCCESS;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS expc= " + ex.Message;
                            //LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.FAILED)//status-failed
                    {
                        try
                        {
                            string sval = resF.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED=" + sval + ", ";
                            //string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                resF.StatusId = statusId = StatsCode.FAILED;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED expc= " + ex.Message;
                            /// LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == TAGName.PROCESSING) //status-processing
                    {
                        try
                        {
                            string sval = resF.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING=" + sval + ", ";

                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                resF.StatusId = statusId = StatsCode.PROCESSING;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.PENDING)//status-pending
                    {
                        try
                        {
                            string sval = resF.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING=" + sval + ", ";
                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                resF.StatusId = statusId = 5;
                            }
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.APITXNID)//api txn id
                    {
                        try
                        {

                            resF.ApiTxnID = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + resF.ApiTxnID + ", ";
                            resF.ApiTxnID = resF.ApiTxnID.Length > 50 ? resF.ApiTxnID.Substring(0, 45) : resF.ApiTxnID;
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")api txnid expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == TAGName.OPERATORTXNID) //operator txn id
                    {
                        try
                        {
                            resF.OperatorTxnID = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                            resF.OperatorTxnID = HttpUtility.UrlDecode(resF.OperatorTxnID);
                            resF.OperatorTxnID = resF.OperatorTxnID.Trim();

                            log += "\r\n optxnid=" + resF.OperatorTxnID.Trim() + ", ";
                            resF.OperatorTxnID = resF.OperatorTxnID.Length > 50 ? resF.OperatorTxnID.Substring(0, 45) : resF.OperatorTxnID;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")optr txnid expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.MESSAGE) //status message
                    {

                        try
                        {
                            resF.Message = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + resF.Message + ", ";
                            resF.Message = resF.Message.Length > 199 ? resF.Message.Substring(0, 198) : resF.Message;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.REQUESTTXNID) //request txn id
                    {

                        try
                        {
                            resF.RequestTxnId = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + resF.RequestTxnId + ", ";
                            resF.RequestTxnId = resF.RequestTxnId.Length > 50 ? resF.RequestTxnId.Substring(0, 45) : resF.RequestTxnId;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.VENDOR_CL_BAL) //Vendor_CL_Bal
                    {

                        try
                        {
                            string clbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                            clbal = clbal.Length > 199 ? clbal.Substring(0, 198) : clbal;
                            resF.Vendor_CL_Bal = Convert.ToDecimal(clbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.VENDOR_OP_BAL) //Vendor_OP_Bal
                    {

                        try
                        {
                            string opbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                            opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                            resF.Vendor_OP_Bal = Convert.ToDecimal(opbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.LAPUNO) //Lapu Number
                    {
                        try
                        {
                            string lapuno = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno=" + lapuno + ", ";
                            resF.LapuNo = lapuno.Length > 50 ? lapuno.Substring(0, 45) : lapuno;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno excp= " + ex.Message;
                            //LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.COMPLAINT_ID)
                    {

                        try
                        {
                            var cid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID=" + cid + ", ";
                            resF.Complaint_Id = cid.Length > 50 ? cid.Substring(0, 45) : cid;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }



                    }
                    else if (tg.TagId == TAGName.R_OFFER)
                    {

                        try
                        {
                            string ro = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer=" + ro + ", ";
                            ro = ro.Length > 50 ? ro.Substring(0, 45) : ro;
                            resF.R_Offer = Convert.ToDecimal(ro);

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.CUSTOMER_NUMBER)
                    {

                        try
                        {
                            string cno = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NUMBER=" + cno + ", ";
                            cno = cno.Length > 100 ? cno.Substring(0, 99) : cno;
                            resF.CustomerNo = cno;

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NUMBER excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.CUSTOMER_NAME)
                    {

                        try
                        {
                            string cname = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NAME=" + cname + ", ";
                            cname = cname.Length > 100 ? cname.Substring(0, 99) : cname;
                            resF.CustomerName = cname;

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CUSTOMER_NAME excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.BILL_NUMBER)
                    {

                        try
                        {
                            string billno = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_NUMBER=" + billno + ", ";
                            billno = billno.Length > 50 ? billno.Substring(0, 49) : billno;
                            resF.BillNo = billno;

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_NUMBER excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.BILL_PERIOD)
                    {

                        try
                        {
                            string billperiod = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PERIOD=" + billperiod + ", ";
                            billperiod = billperiod.Length > 50 ? billperiod.Substring(0, 49) : billperiod;
                            resF.BillPeriod = billperiod;

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PERIOD excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.BILL_PRICE)
                    {

                        try
                        {
                            string price = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PRICE=" + price + ", ";
                            price = price.Length > 50 ? price.Substring(0, 49) : price;
                            resF.BillPrice = Convert.ToDecimal(price);

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_PRICE excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.BILL_DATE)
                    {

                        try
                        {
                            string billdate = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_DATE=" + billdate + ", ";
                            billdate = billdate.Length > 50 ? billdate.Substring(0, 49) : billdate;
                            resF.BillDate = billdate;

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")BILL_DATE excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.BILL_DUE_DATE)
                    {

                        try
                        {
                            string duedate = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")DUE_DATE=" + duedate + ", ";
                            duedate = duedate.Length > 50 ? duedate.Substring(0, 49) : duedate;
                            resF.BillDueDate = duedate;

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")DUE_DATE excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }

                }
            }

            resF.Message = "status=" + resF.Status + " and msg=" + resF.Message;

            return resF.RequestTxnId;
        }

    }
}