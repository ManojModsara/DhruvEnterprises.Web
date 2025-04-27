using DhruvEnterprises.API.LIBS;
using DhruvEnterprises.API.MobiPactApi;
using DhruvEnterprises.API.Models;
using DhruvEnterprises.Data;
using DhruvEnterprises.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Media.Media3D;
using System.Runtime.Remoting.Messaging;

namespace DhruvEnterprises.API.Controllers
{

    public class UpiController : ApiController
    {
        #region "REFERENCE VARIABLES"

        public static bool IsActive = false;
        private readonly IUserService userService;
        private readonly IRequestResponseService reqResService;
        private readonly IDMTReportService reqDmtReportService;
        private readonly IApiService apiService;
        private readonly IPackageService packageService;

        private readonly IRechargeService rechargeService;
        private readonly IWalletService walletService;
        private readonly string path;
        #endregion

        #region "CONSTRUCTOR"

        public UpiController(IPackageService _packageService, IDMTReportService _reqDmtReportService, IUserService _userService, IRequestResponseService _reqResService, IApiService _apiService, IRechargeService _rechargeService, IWalletService _walletService)
        {
            this.userService = _userService;
            this.reqResService = _reqResService;
            this.apiService = _apiService;
            this.packageService = _packageService;

            this.reqDmtReportService = _reqDmtReportService;
            this.rechargeService = _rechargeService;
            this.walletService = _walletService;
            this.path = HttpContext.Current.Server.MapPath("~/ApiActivityLog/" + DateTime.Now.ToString("ddMMyy") + "/");
        }
        #endregion

        #region "VALIDATIONS"
        public string GetUniqueNumber(int length = 6)
        {
            string dt = DateTime.Now.ToString("yyMMddHHmmss");
            string randomstr = GetUniqueDigit();
            string autorefno = "U" + dt + Guid.NewGuid().ToString().ToUpper().Replace("-", "").Substring(10, 6);
            if (!IsOK(autorefno))
            {
                do
                {
                    autorefno = "U" + dt + Guid.NewGuid().ToString().ToUpper().Replace("-", "").Substring(10, 6);
                }
                while (!IsOK(autorefno));
            }


            return autorefno;

        }
        public bool IsOK(string s)
        {
            if (s.Length < 3) return true;

            return !s.Where((c, i) => i >= 2 && s[i - 1] == c && s[i - 2] == c && Char.IsLetter(c)).Any();
        }
        public string Fetch_UserIP()
        {
            string VisitorsIPAddress = string.Empty;
            try
            {
                if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    VisitorsIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
                else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
                {
                    VisitorsIPAddress = HttpContext.Current.Request.UserHostAddress;
                }
            }
            catch (Exception)
            {

                //Handle Exceptions  
            }
            return VisitorsIPAddress;
        }
        private bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
            else
            {

                try
                {
                    var guid = new Guid(token);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }

        }

        public string GetUniqueDigit(int min = 0, int max = 10)
        {
            return string.Join("", Enumerable.Range(min, max).OrderBy(g => Guid.NewGuid()).Take(10).ToArray());
        }

        //SqlConnection con;
        private UpiResponse Validationcheck()
        {
            UpiResponse signupResponse = new UpiResponse();
            if (!ModelState.IsValid)
            {
                signupResponse.Error = ErrorCode.AUTH_FAIL;
                signupResponse.Status = StatusName.FAILED;
                var data = ModelState.Values.SelectMany(x => x.Errors).ToList();
                foreach (var value in data)
                {
                    signupResponse.Message = value.ErrorMessage;
                    return signupResponse;
                }
            }
            return signupResponse;
        }
        #endregion



        #region  DMT
        //[Route("~/Service/MoneyTransfer")]
        //[HttpPost]
        //public JObject DMTMoneyTransfer(MoneyTransferDto data)
        //{
        //    var request = HttpContext.Current.Request;
        //    string log = "start- MoneyTransfer,";

        //    int statusId = 4;
        //    RequestResponseDto userReqRes = new RequestResponseDto();
        //    JObject response = new JObject();
        //    MoneyTransferModel model = new MoneyTransferModel();
        //    bool IsBefore = true;
        //    try
        //    {
        //        model.ipaddress = Fetch_UserIP();
        //        model.ourtxnid = GetUniqueNumber();
        //        model.transfermode = data.Mode;
        //        string post_data = JsonConvert.SerializeObject(data);

        //        userReqRes.RequestTxt = "URL: " + request.Url.AbsoluteUri + ", DATA: " + post_data;
        //        userReqRes.OpId = data.OpId;
        //        userReqRes.RefId = model.ourtxnid;
        //        userReqRes.UserReqId = data.OrderID;
        //        userReqRes.Remark = "Request_PayoutTransfer";
        //        userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);
        //        model.token = data.ApiToken;
        //        model.amount = data.Amount;
        //        model.opid = data.OpId; // AEPSOperators.MoneyTransfer;
        //        model.reftxnid = data.OrderID;
        //        model.Name = data.Name;
        //        model.customerId = data.customerId;
        //        model.address = data.address;
        //        model.mobileno = data.Mobileno;
        //        model.accountno = data.Account;
        //        model.ifsc = data.IFSC;
        //        model.BankName = data.BankName;
        //        model.beneficiaryName = data.recipientName;
        //        model.BCID = data.recipientId;
        //        model.AgentId = model.AgentId;

        //        string ecode = ErrorCode.UNKNOWN_ERROR;
        //        string emsg = StatusMsg.UNKNOWN_ERROR;
        //        if (!IsValidateParameters(model, ref ecode, ref emsg))
        //        {
        //            log += "\r\n , " + StatusMsg.INVALID_REQUEST_ID;
        //            statusId = StatsCode.FAILED;
        //            response = JObject.FromObject(new
        //            {
        //                STATUS = StatsCode.FAILED,
        //                MESSAGE = emsg,
        //                ERRORCODE = ecode,
        //                EZREFTXNID = model.ourtxnid,
        //                HTTPCODE = HttpStatusCode.BadRequest
        //            });
        //        }
        //        else
        //        {
        //            IsBefore = false;
        //            MoneyTransferRequest(model, ref log, ref response, ref userReqRes, ref statusId);
        //        }
        //        userReqRes.Remark = (userReqRes.RecId > 0) || userReqRes.Remark == "Request_MoneyTransfer" ? userReqRes.Remark : "Request_ERR";
        //        userReqRes.RefId = model.ourtxnid;
        //        userReqRes.ResponseText = response.ToString();
        //        userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);
        //        try
        //        {
        //            dynamic resp = response;
        //        }
        //        catch (Exception ex)
        //        {
        //            Common.LogException(ex, " mobileno=" + model.mobileno + ", ourref=" + model.ourtxnid + ", reqid=" + model.reftxnid + ",log=" + log);
        //        }
        //        log += "\r\n -end";
        //        Common.LogActivity(log);
        //        model = null;
        //        log = string.Empty;
        //        userReqRes = null;
        //    }
        //    catch (Exception exc)
        //    {
        //        if (IsBefore)
        //        {
        //            statusId = StatsCode.FAILED;
        //            response = JObject.FromObject(new
        //            {
        //                STATUS = StatsCode.FAILED,
        //                MESSAGE = StatusMsg.RECHARGE_FAILED,
        //                ERRORCODE = ErrorCode.UNKNOWN_ERROR,
        //                HTTPCODE = HttpStatusCode.BadRequest
        //            });
        //        }
        //        Common.LogException(exc, " mobileno=" + model.mobileno + ", ourref=" + model.ourtxnid + ", reqid=" + model.reftxnid + ",log=" + log);
        //    }
        //    return response;
        //}
        #endregion


        #region   Payout
        [Route("~/Prod/Create_PayOut")]
        [HttpPost]
        public JObject Create_PayOut(MoneyTransferDto data)
        {
            var request = HttpContext.Current.Request;
            string log = "start- MoneyTransfer,";
            //Common.LogActivity(log);
            int statusId = 4;
            string status = StatusName.PROCESSING;
            RequestResponseDto userReqRes = new RequestResponseDto();
            JObject response = new JObject();
            MoneyTransferModel model = new MoneyTransferModel();
            bool IsBefore = true;
            try
            {
                model.ipaddress = Fetch_UserIP();
                model.ourtxnid = GetUniqueNumber();
                string ApiToken = Convert.ToString(request.Headers["authKey"]);
                model.transfermode = data.transferMode;
                string post_data = JsonConvert.SerializeObject(data);
                userReqRes.RequestTxt = "URL: " + request.Url.AbsoluteUri + ", DATA: " + post_data;
                //userReqRes.OpId = data.OpId;
                userReqRes.RefId = model.ourtxnid;
                userReqRes.UserReqId = data.requestId;
                userReqRes.Remark = "PayoutRequest";
                userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);
                model.token = ApiToken;
                model.amount = data.amount;
                model.firstname = data.firstName;
                model.lastname = data.lastName;
                model.accountno = data.accountNumber;
                model.ifsc = data.IFSC;
                //model.opid = data.OpId; 
                model.reftxnid = data.requestId;
                model.beneficiaryName = data.accountHolderName;
                model.mobileno = data.mobileNumber;
                //model.Pin = data.Pin;
                model.BankName = data.bankName;
                model.bankid = Convert.ToInt32(data.bankID);
                //model.BCID = data.recipientId;
                //model.AgentId = model.agentId;

                string ecode = ErrorCode.UNKNOWN_ERROR;
                string emsg = StatusMsg.UNKNOWN_ERROR;
                if (!IsValidateParameters(model, ref ecode, ref emsg))
                {
                    log += "\r\n , " + StatusMsg.INVALID_REQUEST_ID;
                    status = StatusName.FAILURE;
                    response = JObject.FromObject(new
                    {
                        status = status,
                        errorCode = ecode,
                        message = emsg,
                        data = new
                        {
                            utrNumber = "",
                            apiRecordId = "",
                            referenceId = "",
                            requestId = model.reftxnid
                        }
                    });
                }
                else
                {
                    IsBefore = false;
                    MoneyTransferRequest(model, ref log, ref response, ref userReqRes, ref statusId);
                }
                userReqRes.Remark = (userReqRes.RecId > 0) || userReqRes.Remark == "PayoutRequest" ? userReqRes.Remark : "Error";
                userReqRes.RefId = model.ourtxnid;
                userReqRes.ResponseText = response.ToString();
                userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);
                try
                {
                    dynamic resp = response;
                }
                catch (Exception ex)
                {
                    Common.LogException(ex, " mobileno=" + model.mobileno + ", ourref=" + model.ourtxnid + ", reqid=" + model.reftxnid + ",log=" + log);
                }
                log += "\r\n -end";
                Common.LogActivity(log);
                model = null;
                log = string.Empty;
                userReqRes = null;
            }
            catch (Exception exc)
            {
                if (IsBefore)
                {
                    status = StatusName.FAILURE;
                    response = JObject.FromObject(new
                    {
                        status = status,
                        errorCode = ErrorCode.UNKNOWN_ERROR,
                        message = StatusMsg.RECHARGE_FAILED,
                        data = new
                        {
                            utrNumber = "",
                            apiRecordId = "",
                            referenceId = "",
                            requestId = model.reftxnid
                        }
                    });
                    
                }
                Common.LogException(exc, " mobileno=" + model.mobileno + ", ourref=" + model.ourtxnid + ", reqid=" + model.reftxnid + ",log=" + log);
            }
            return response;
        }
        private JObject MoneyTransferRequest(MoneyTransferModel model, ref string log, ref JObject response, ref RequestResponseDto userReqRes, ref int statusId)
        {
            log += "\r\n MTreq v2 start ";
            RechargeHelperDto helper = new RechargeHelperDto();
            bool IsOpMatch = true;
            string status = StatusName.PROCESSING;
            try
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")(ApiToken=" + model.token + ",MobileNo=" + model.mobileno + ",Amount=" + model.amount + ",OpId=" + model.opid + ",RefTxnId=" + model.reftxnid + " IpAddress=" + model.ipaddress + ",OurTxnId=" + model.ourtxnid + "), ";
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SP_DMTRequestValidation", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiToken", model.token);
                    cmd.Parameters.AddWithValue("@AccountNO", model.accountno);
                    cmd.Parameters.AddWithValue("@Amount", model.amount);
                    cmd.Parameters.AddWithValue("@OpId", model.opid);
                    cmd.Parameters.AddWithValue("@UserTxnId", model.reftxnid);
                    cmd.Parameters.AddWithValue("@IPAddress", model.ipaddress);
                    cmd.Parameters.AddWithValue("@IFSC", model.ifsc);
                    //cmd.Parameters.AddWithValue("@PinCheck","0");
                    // Pin 1 Ka Matlab No Pin Requirement 0 means Pin Requirement
                    cmd.Parameters.Add("@Bankcode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.AddWithValue("@UserPin", EncryptDecrypt.Encrypt(model.Pin ?? ""));
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@SwitchTypeId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@DebitAmount", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmd.Parameters["@DebitAmount"].Precision = 20;
                    cmd.Parameters["@DebitAmount"].Scale = 4;
                    cmd.Parameters.Add("@UserComm", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmd.Parameters["@UserComm"].Precision = 18;
                    cmd.Parameters["@UserComm"].Scale = 2;
                    cmd.Parameters.Add("@Api1", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Api2", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Api3", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@AmtTypeId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    log += "\r\n , before exec SP_DMTRequestValidation userid=" + model.userid;
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n , after exec";
                    string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                    string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    string Log = Convert.ToString(cmd.Parameters["@Log"].Value);
                    log += "\r\n , error=" + error + ", errordesc=" + ErrorDesc;
                    if (error == ErrorCode.DEFAULT_OFFLINE)//100-default-offline
                    {
                        statusId = StatsCode.FAILED;
                        status = StatusName.FAILURE;
                        log += "\r\n , default offline ";
                        response = JObject.FromObject(new
                        {
                            status = status,
                            errorCode = ErrorCode.DEFAULT_OFFLINE,
                            message = StatusMsg.DEFAULT_OFFLINE,
                            data = new
                            {
                                utrNumber = "",
                                apiRecordId = "",
                                referenceId = model.ourtxnid,
                                requestId = model.reftxnid
                            }
                        });
                    }
                    else if (error == ErrorCode.ROUTE_BLOCKED)//100-default-offline
                    {
                        statusId = StatsCode.FAILED;
                        status = StatusName.FAILURE;

                        log += "\r\n , default offline ";
                        response = JObject.FromObject(new
                        {
                            statusCode = status,
                            errorCode = ErrorCode.ROUTE_BLOCKED,
                            message = StatusMsg.ROUTE_BLOCKED,
                            data = new
                            {
                                utrNumber = "",
                                apiRecordId = "",
                                referenceId = model.ourtxnid,
                                requestId = model.reftxnid
                            }
                        });
                    }
                    else if (error == ErrorCode.NO_ERROR || error == ErrorCode.DEFAULT_FAILED || error == ErrorCode.DEFAULT_PROCESSING || error == ErrorCode.CALLBACK_RECHARGE_THREAD) //0 -No error, 101 -default failed
                    {
                        // retreive sp parameter values
                        int Amttype = Convert.ToInt32(cmd.Parameters["@AmtTypeId"].Value);
                        model.userid = Convert.ToString(cmd.Parameters["@UserId"].Value);
                        userReqRes.UserId = !string.IsNullOrEmpty(model.userid) ? Convert.ToInt32(model.userid) : userReqRes.UserId;
                        helper.SwitchId = Convert.ToInt32(cmd.Parameters["@SwitchTypeId"]?.Value);
                        helper.DebitAmount = Convert.ToDecimal(cmd.Parameters["@DebitAmount"]?.Value);
                        helper.CommAmount = Convert.ToDecimal(cmd.Parameters["@UserComm"]?.Value);
                        model.BankName = Convert.ToString(cmd.Parameters["@Bankcode"].Value);
                        helper.CommAmount = helper.DebitAmount - Convert.ToDecimal(model.amount);
                        helper.AmtTypeId = Amttype;
                        string api1 = Convert.ToString(cmd.Parameters["@Api1"].Value);
                        string api2 = Convert.ToString(cmd.Parameters["@Api2"].Value);
                        string api3 = Convert.ToString(cmd.Parameters["@Api3"].Value);
                        long amtint = 0;
                        decimal amtdecimal = 0;
                        amtdecimal = Convert.ToDecimal(model.amount);
                        amtint = Convert.ToInt64(amtdecimal);
                        model.amount = amtdecimal == amtint ? amtint.ToString() : model.amount;
                        List<ApiPriorityDto> oproutes = new List<ApiPriorityDto>();
                        if (api1 != "" && api1 != "0")
                            oproutes.Add(new ApiPriorityDto { ApiId = Convert.ToInt32(api1), PriorityId = 1 });
                        if (api2 != "" && api2 != "0")
                            oproutes.Add(new ApiPriorityDto { ApiId = Convert.ToInt32(api2), PriorityId = 2 });
                        if (api3 != "" && api3 != "0")
                            oproutes.Add(new ApiPriorityDto { ApiId = Convert.ToInt32(api3), PriorityId = 3 });
                        log += "\r\n , op route set done ";
                        if (helper.SwitchId == RouteType.AMOUNT_WISE)//amount routing
                        {
                            log += "\r\n c8(" + DateTime.Now.TimeOfDay.ToString() + ")(switype=4-amt),";
                            var routelist = rechargeService.GetAmountRouts(Convert.ToInt32(0), Convert.ToInt32(model.opid), Convert.ToDecimal(model.amount));
                            if (routelist != null && routelist.Count > 0)
                            {
                                int i = 1;
                                helper.ApiRouteList.Clear();
                                foreach (var route in routelist.OrderBy(x => x.Priority).ToList())
                                {
                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") ROUTE(Id=" + route.Id + ",Priority=" + route.Priority + ",rtype=" + route.FTypeId + ",amouts/range=" + route.AmountFilter + "RO=" + (route.MinRO ?? 0) + ", userfilter=" + route.UserFilter + ", BlockUser=" + route.BlockUser + ")";

                                    if (string.IsNullOrEmpty(route.BlockUser) && string.IsNullOrEmpty(route.UserFilter))
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")UserFilter";
                                        helper.ApiRouteList.Add(new ApiPriorityDto() { ApiId = route.ApiId ?? 0, PriorityId = i++, LapuFilter = route.LapuFilter, CircleFilter = route.CircleFilter, BlockUser = route.BlockUser, UserFilter = route.UserFilter, MinRO = route.MinRO ?? 0, RouteId = route.Id, FTypeId = route.FTypeId ?? 0, RouteOP1 = route.RouteOP1, RoutePriorityId = route.Priority ?? 0 });
                                    }
                                    else if (!string.IsNullOrEmpty(route.BlockUser) && !string.IsNullOrEmpty(route.UserFilter))
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") BlockUser And UserFilter";
                                        if (!route.BlockUser.Replace(" ", "").Split(',').Any(s => s == model.userid) && (route.UserFilter.ToLower().Contains("all") || route.UserFilter.Replace(" ", "").Split(',').Any(s => s == model.userid)))
                                        {
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")AddedRouteId=" + route.Id + ",routepriority=" + route.Priority;
                                            helper.ApiRouteList.Add(new ApiPriorityDto() { ApiId = route.ApiId ?? 0, PriorityId = i++, LapuFilter = route.LapuFilter, CircleFilter = route.CircleFilter, BlockUser = route.BlockUser, UserFilter = route.UserFilter, MinRO = route.MinRO ?? 0, RouteId = route.Id, FTypeId = route.FTypeId ?? 0, RouteOP1 = route.RouteOP1, RoutePriorityId = route.Priority ?? 0 });
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(route.BlockUser))
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") BlockUser";
                                        if (!route.BlockUser.Replace(" ", "").Split(',').Any(s => s == model.userid))
                                        {
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")AddedRouteId=" + route.Id + ",routepriority=" + route.Priority;
                                            helper.ApiRouteList.Add(new ApiPriorityDto() { ApiId = route.ApiId ?? 0, PriorityId = i++, LapuFilter = route.LapuFilter, CircleFilter = route.CircleFilter, BlockUser = route.BlockUser, UserFilter = route.UserFilter, MinRO = route.MinRO ?? 0, RouteId = route.Id, FTypeId = route.FTypeId ?? 0, RouteOP1 = route.RouteOP1, RoutePriorityId = route.Priority ?? 0 });
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(route.UserFilter))
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") UserFilter not null";
                                        if (route.UserFilter.ToLower().Contains("all") || route.UserFilter.Replace(" ", "").Split(',').Any(s => s == model.userid))
                                        {
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")AddedRouteId=" + route.Id + ",routepriority=" + route.Priority;
                                            helper.ApiRouteList.Add(new ApiPriorityDto() { ApiId = route.ApiId ?? 0, PriorityId = i++, LapuFilter = route.LapuFilter, CircleFilter = route.CircleFilter, BlockUser = route.BlockUser, UserFilter = route.UserFilter, MinRO = route.MinRO ?? 0, RouteId = route.Id, FTypeId = route.FTypeId ?? 0, RouteOP1 = route.RouteOP1, RoutePriorityId = route.Priority ?? 0 });
                                        }
                                    }
                                    else
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")NoRouteMatch RouteId=" + route.Id + ",routepriority=" + route.Priority;
                                    }
                                }
                            }
                            else
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")c8(switype=2-co-op),";
                                helper.SwitchId = RouteType.OPERATOR_WISE;
                            }
                        }
                        if (!IsOpMatch)
                        {
                            response = JObject.FromObject(new
                            {
                                status = StatusName.FAILURE,
                                errorCode = ErrorCode.INVALID_OPERATOR,
                                message = StatusMsg.INVALID_OPERATOR,
                                data = new
                                {
                                    utrNumber = "",
                                    apiRecordId = "",
                                    referenceId = model.ourtxnid,
                                    requestId = model.reftxnid
                                }
                            });
                        }
                        else
                        {
                            List<ApiPriorityDto> apiroutes = new List<ApiPriorityDto>();
                            int p = 1;
                            if (helper.ApiRouteList.Count > 0)
                            {
                                foreach (var route in helper.ApiRouteList)
                                {
                                    if (!apiroutes.Any(x => x.ApiId == route.ApiId && x.MinRO == route.MinRO))
                                    {
                                        route.PriorityId = p;
                                        apiroutes.Add(route);
                                        p++;
                                    }
                                }
                            }
                            if (oproutes.Count > 0)
                            {
                                foreach (var route in oproutes)
                                {
                                    if (!apiroutes.Any(x => x.ApiId == route.ApiId))
                                    {
                                        route.PriorityId = p;
                                        apiroutes.Add(route);
                                        p++;
                                    }
                                }
                            }
                            helper.ApiRouteList = apiroutes;
                            if (helper.ApiRouteList.Count > 0)
                            {

                                response = MoneyTransferProcess(model, response, helper, ref userReqRes, ref log, ref statusId, false);

                            }
                            else
                            {
                                log += "\r\n ,route not available= " + ErrorDesc; response = JObject.FromObject(new
                                {
                                    status = StatusName.FAILURE,
                                    errorCode = ErrorCode.ROUTE_NOT_FOUND,
                                    message = StatusMsg.ERROR_CONTACT_ADMIN,
                                    data = new
                                    {
                                        utrNumber = "",
                                        apiRecordId = "",
                                        referenceId = model.ourtxnid,
                                        requestId = model.reftxnid
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        log += "\r\n , error desc= " + ErrorDesc;
                        response = JObject.FromObject(new
                        {
                            status = StatusName.FAILURE,
                            errorCode = error,
                            message = ErrorDesc,
                            data = new
                            {
                                utrNumber = "",
                                apiRecordId = "",
                                referenceId = model.ourtxnid,
                                requestId = model.reftxnid
                            }
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp= " + ex.ToString();
                statusId = StatsCode.FAILED;
                status = StatusName.FAILURE;

                response = JObject.FromObject(new
                {
                    status = status,
                    errorCode = ErrorCode.UNKNOWN_ERROR,
                    message = StatusMsg.RECHARGE_FAILED,
                    data = new
                    {
                        utrNumber = "",
                        apiRecordId = "",
                        referenceId = model.ourtxnid,
                        requestId = model.reftxnid
                    }
                });
                Common.LogException(ex, "RechargeRequest log=" + log);
            }

            if (helper.RecId > 0)
            {
                userReqRes.RecId = helper.RecId;
            }
            try
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")UpdateStatus-RechargeRequest-FINAL";
                bool IsDownline = false, IsRefund = false;
                UpdateStatusWithDMTCheck(helper.RecId, 0, statusId, string.Empty, string.Empty, string.Empty, "Auto", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(model.opid), string.Empty);
            }
            catch (Exception ex)
            {
                Common.LogException(ex, "UpdateStatus-RechargeRequest-FINAL,log=" + log);
            }
            return response;
        }
        private JObject MoneyTransferProcess(MoneyTransferModel model, JObject response, RechargeHelperDto helper, ref RequestResponseDto userReqRes, ref string log, ref int statusId, bool IsSwitched = false, string path = "")
        {
            // int minp = rech.ApiRouteList.Max(x => x.PriorityId);
            int maxp = helper.ApiRouteList.Max(x => x.PriorityId);
            if (!string.IsNullOrEmpty(path))
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")rcprocess-thread,";
            if (helper.RecId > 0)
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")rcprocess v2 switched  recid=" + helper.RecId.ToString();
            }
            else
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")rcprocess v2,";
            }
            if (helper.CurrentPriorityId == maxp)
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")all api consumed,CurrentPriorityId = maxp=" + maxp;
                statusId = StatsCode.FAILED;
                bool IsDownline = false, IsRefund = false;
                UpdateStatusWithDMTCheck(helper.RecId, Convert.ToInt32(model.userid), StatsCode.FAILED, string.Empty, string.Empty, string.Empty, "Auto", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(model.opid), string.Empty);
            }
            else
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") api current route set start ";

                #region "Api Route"
                decimal apibal = 0;
                bool IsApiAvailable = false;
                var route = helper.ApiRouteList.Where(x => x.PriorityId > helper.CurrentPriorityId && (x.ApiId != helper.CurrentApiId || x.MinRO != helper.MinRO)).OrderBy(x => x.PriorityId).FirstOrDefault();
                if (route != null)
                {
                    IsApiAvailable = true;
                    helper.CurrentPriorityId = route.PriorityId;
                    helper.CurrentApiId = route.ApiId;
                    helper.LapuFilter = route.LapuFilter;
                    helper.CircleFilter = route.CircleFilter;
                    helper.BlockUser = route.BlockUser;
                    helper.UserFilter = route.UserFilter;
                    helper.MinRO = route.MinRO;
                    helper.RouteId = route.RouteId;
                    helper.FTypeId = route.FTypeId;
                    helper.RouteOP1 = route.RouteOP1;
                    helper.RoutePriorityId = route.RoutePriorityId;
                }
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")CurrentRoute Details-  apino=" + helper.CurrentPriorityId + " apiid=" + helper.CurrentApiId + " apibalc=" + apibal + " routeid=" + helper.RouteId + ", routePriority=" + helper.RoutePriorityId;
                #endregion
                if (!IsApiAvailable)
                {
                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")all next api same or low bal,";
                    if (helper.RecId > 0)
                    {
                        bool IsDownline = false, IsRefund = false;
                        try
                        {
                            statusId = StatsCode.FAILED;
                            UpdateStatusWithDMTCheck(helper.RecId, Convert.ToInt32(model.userid), StatsCode.FAILED, string.Empty, string.Empty, string.Empty, "Auto", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(model.opid), string.Empty);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") !IsApiAvailable  exce update status=" + ex.Message;
                            Common.LogException(ex, "!IsApiAvailable  exce update status, RecId=" + helper.RecId + ", mobileno=" + model.mobileno + ", ourref=" + model.ourtxnid);
                        }
                    }
                }
                else
                {
                    if (helper.RecId == 0)
                        statusId = StatsCode.HOLD;
                    string status = "";
                    try
                    {
                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")param setting usp_MoneyTransferCreate, ";
                        using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                        {
                            //set sp parameters
                            SqlCommand cmd = new SqlCommand("Api_DMTCreate", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ApiId", helper.CurrentApiId);
                            cmd.Parameters.AddWithValue("@UserId", model.userid);
                            cmd.Parameters.AddWithValue("@IPAddress", model.ipaddress);
                            cmd.Parameters.AddWithValue("@AccountNo", model.accountno);
                            cmd.Parameters.AddWithValue("@Amount", model.amount);
                            cmd.Parameters.AddWithValue("@OpId", model.opid);
                            cmd.Parameters.AddWithValue("@CommAmount", helper.CommAmount);
                            cmd.Parameters.AddWithValue("@AmtTypeId", helper.AmtTypeId);
                            cmd.Parameters.AddWithValue("@OurRef", model.ourtxnid);
                            cmd.Parameters.AddWithValue("@DebitAmount", helper.DebitAmount);
                            cmd.Parameters.AddWithValue("@MediumId", model.MediumId);
                            cmd.Parameters.AddWithValue("@BeneName", model.beneficiaryName);
                            cmd.Parameters.AddWithValue("@IFSC", model.ifsc);
                            cmd.Parameters.AddWithValue("@SwitchedRecId", helper.RecId);
                            cmd.Parameters.AddWithValue("@BeneMobileNo", model.mobileno);
                            cmd.Parameters.AddWithValue("@transferMode", model.transfermode);
                            cmd.Parameters.AddWithValue("@UserTxnId", model.reftxnid);

                            cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@Method", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ContentType", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ResType", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@UrlId", SqlDbType.Int).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ApiUserId", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ApiPassword", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ApiOptional", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@RecId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                            //cmd.Parameters.Add("@RechargeId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@TxnId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ApiTypeId", SqlDbType.Int).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ExtraUrl", SqlDbType.NVarChar, 500).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ExtraUrlData", SqlDbType.NVarChar, 500).Direction = ParameterDirection.Output;
                            log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")before exec  Api_DMTCreate";
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                            log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") after exec  usp_MoneyTransferCreate";
                            string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                            string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                            string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);
                            log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") spLog= (" + spLog + ")";
                            log += "\r\n ,  error=" + error;
                            if (error == ErrorCode.DEFAULT_OFFLINE)
                            {
                                statusId = StatsCode.FAILED;
                                status = StatusName.FAILURE;

                                log += "\r\n default offline";
                                response = JObject.FromObject(new
                                {
                                    status= status,
                                    errorCode = error,
                                    message = StatusMsg.RECHARGE_FAILED,
                                    data = new
                                    {
                                        utrNumber = "",
                                        apiRecordId = "",
                                        referenceId = model.ourtxnid,
                                        requestId = model.reftxnid
                                    }
                                });
                            }
                            else if (error == ErrorCode.NO_ERROR || error == ErrorCode.DEFAULT_FAILED || error == ErrorCode.DEFAULT_PROCESSING || error == ErrorCode.REQUEST_GAP)
                            {
                                statusId = StatsCode.FAILED;
                                string ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                                string Method = Convert.ToString(cmd.Parameters["@Method"].Value);
                                string ContentType = Convert.ToString(cmd.Parameters["@ContentType"].Value);
                                string ResType = Convert.ToString(cmd.Parameters["@ResType"].Value);
                                string PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                                string UrlId = Convert.ToString(cmd.Parameters["@UrlId"].Value);
                                string ApiUserId = Convert.ToString(cmd.Parameters["@ApiUserId"].Value);
                                string ApiPassword = Convert.ToString(cmd.Parameters["@ApiPassword"].Value);
                                string ApiOptional = Convert.ToString(cmd.Parameters["@ApiOptional"].Value);
                                string OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);
                                string RecId = Convert.ToString(cmd.Parameters["@RecId"].Value);
                                //string RechargeId = Convert.ToString(cmd.Parameters["@RechargeId"].Value);
                                string TxnId = Convert.ToString(cmd.Parameters["@TxnId"].Value);
                                string ApiTypeId = Convert.ToString(cmd.Parameters["@ApiTypeId"].Value);
                                string ExtraUrl = Convert.ToString(cmd.Parameters["@ExtraUrl"].Value);
                                string ExtraUrlData = Convert.ToString(cmd.Parameters["@ExtraUrlData"].Value);
                                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")value retrieved usp_WalletTransferCreate" + error;
                                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") RecId=" + RecId + ", TxnId=" + TxnId + ", Opcode=" + OpCode + ", ApiTypeId" + ApiTypeId;
                                helper.RecId = !string.IsNullOrWhiteSpace(RecId) ? Convert.ToInt64(RecId) : 0;
                                //helper.RechargeId = !string.IsNullOrWhiteSpace(RechargeId) ? Convert.ToInt64(RechargeId) : 0;
                                userReqRes.RecId = helper.RecId;
                                userReqRes.RechargeId = helper.RechargeId;
                                model.OpCode = OpCode;
                                userReqRes.UrlId = !string.IsNullOrWhiteSpace(UrlId) ? Convert.ToInt32(UrlId) : 0;
                                helper.TxnId = !string.IsNullOrWhiteSpace(TxnId) ? Convert.ToInt64(TxnId) : 0;
                                //  model.OpCode = OpCode;
                                helper.ApiTypeId = !string.IsNullOrWhiteSpace(ApiTypeId) ? Convert.ToInt32(ApiTypeId) : 2;
                                if (error == ErrorCode.DEFAULT_FAILED)
                                {
                                    statusId = StatsCode.FAILED;
                                    status = StatusName.FAILURE;

                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")default failed";

                                    response = JObject.FromObject(new
                                    {
                                        status = status,
                                        errorCode = ErrorCode.DEFAULT_FAILED,
                                        message = StatusMsg.DEFAULT_FAILED,
                                        data = new
                                        {
                                            utrNumber = "",
                                            apiRecordId = "",
                                            referenceId = model.ourtxnid,
                                            requestId = model.reftxnid
                                        }
                                    });
                                }
                                else if (error == ErrorCode.DEFAULT_PROCESSING)
                                {
                                    statusId = StatsCode.PROCESSING;
                                    status = StatusName.PROCESSING;

                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")default processing";
                                    response = JObject.FromObject(new
                                    {
                                        status = status,
                                        errorCode = ErrorCode.DEFAULT_PROCESSING,
                                        message = StatusMsg.DEFAULT_PROCESSING,
                                        data = new
                                        {
                                            utrNumber = "",
                                            apiRecordId = "",
                                            referenceId = model.ourtxnid,
                                            requestId = model.reftxnid
                                        }
                                    });

                                }
                                else if (string.IsNullOrEmpty(OpCode))
                                {
                                    statusId = StatsCode.FAILED;
                                    status = StatusName.FAILURE;

                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")api opcode not set apiid=" + helper.CurrentApiId + ", ";
                                    response = JObject.FromObject(new
                                    {
                                        status = status,
                                        errorCode = ErrorCode.INVALID_OPERATOR,
                                        message = StatusMsg.INVALID_OPERATOR,
                                        data = new
                                        {
                                            utrNumber = "",
                                            apiRecordId = "",
                                            referenceId = model.ourtxnid,
                                            requestId = model.reftxnid
                                        }
                                    });
                                    response = MoneyTransferProcess(model, response, helper, ref userReqRes, ref log, ref statusId, true, path);
                                }
                                else if (string.IsNullOrEmpty(ApiUrl))
                                {
                                    statusId = StatsCode.FAILED;
                                    status = StatusName.FAILURE;

                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apiurl not set, ";
                                    response = JObject.FromObject(new
                                    {
                                        status = status,
                                        errorCode = ErrorCode.APIURL_NOT_SET,
                                        message = StatusMsg.ERROR_CONTACT_ADMIN,
                                        data = new
                                        {
                                            utrNumber = "",
                                            apiRecordId = "",
                                            referenceId = model.ourtxnid,
                                            requestId = model.reftxnid
                                        }
                                    });

                                    response = MoneyTransferProcess(model, response, helper, ref userReqRes, ref log, ref statusId, true, path);
                                }
                                else if (error == ErrorCode.REQUEST_GAP)
                                {
                                    statusId = StatsCode.PROCESSING;
                                    status = StatusName.PROCESSING;

                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")ErrorCode.REQUEST_GAP(" + ErrorCode.REQUEST_GAP + ")";
                                    response = JObject.FromObject(new
                                    {
                                        status = status,
                                        errorCode = ErrorCode.NO_ERROR,
                                        message = StatusMsg.RECHARGE_PROCESSING,
                                        data = new
                                        {
                                            utrNumber = "",
                                            apiRecordId = "",
                                            referenceId = model.ourtxnid,
                                            requestId = model.reftxnid
                                        }
                                    });
                                }
                                else
                                {
                                    #region Instant Money Transfer APi Call
                                    statusId = StatsCode.FAILED;

                                    int unitid = 0, len = 0, reflen = 0, IsNumeric = 0;
                                    string randomkey = "", dtformat = "", refpadding = "", apiamount = "", ourref = "", datetime = "";
                                    // GetApiExtDetails(helper.CurrentApiId, ref unitid, ref len, ref randomkey, ref dtformat, ref refpadding, ref log, ref reflen, ref IsNumeric);
                                    apiamount = unitid == 2 ? (Convert.ToInt64(Convert.ToDecimal(model.amount) * 100)).ToString("D" + len) : model.amount;
                                    ourref = IsNumeric == 1 ? helper.TxnId.ToString() : model.ourtxnid;
                                    ourref = reflen > 0 ? (reflen < ourref.Length ? ourref.Remove(0, ourref.Length - reflen) : refpadding + ourref) : ourref;
                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + "),  apiamount=" + apiamount + " ourref=" + ourref;
                                    datetime = DateTime.Now.ToString(dtformat);
                                    #region "APi Call"
                                    RequestResponseDto requestResponse = new RequestResponseDto();
                                    try
                                    {
                                        if (Convert.ToInt32(model.userid) > 0)
                                            requestResponse.UserId = Convert.ToInt32(model.userid);
                                        if (Convert.ToInt32(UrlId) > 0)
                                            requestResponse.UrlId = Convert.ToInt32(UrlId);
                                    }
                                    catch (Exception exx)
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + "), excp: (set reqres) userid=" + model.userid + " urlid=" + UrlId;
                                        LIBS.Common.LogException(exx);
                                    }
                                    bool IsDownline = false, IsRefund = false;

                                    requestResponse.Remark = model.MediumId == 1 ? "Payout App" : "Payout";
                                    ApiCall apiCall = new ApiCall(reqResService);
                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";
                                    statusId = StatsCode.PROCESSING;
                                    status = StatusName.PROCESSING;
                                    string apires = string.Empty;
                                    requestResponse.RecId = Convert.ToInt64(RecId);
                                    //requestResponse.RechargeId = Convert.ToInt64(RechargeId);
                                    requestResponse.RefId = model.ourtxnid;
                                    requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);

                                    if (helper.CurrentApiId == 8)
                                    {
                                        var PaymentMode = string.Empty;
                                        if (model.transfermode == "IMPS")
                                        {
                                            PaymentMode = "IMPSIFSC"; // only this api use 
                                        }
                                        var url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(model.firstname, model.lastname, ApiPassword, model.mobileno, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, "", "", model.accountno, model.BankName, model.ifsc, model.ourtxnid, model.beneficiaryName, PaymentMode, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";
                                        requestResponse.RequestTxt = url + " DATA " + postdata;
                                        requestResponse.Remark = "payout";
                                        apires = Method == "POST" ? apiCall.Post(url, postdata, ContentType, ResType, helper.CurrentApiId, ApiUserId, ApiPassword)
                                                                                                               : apiCall.Get(url, ref requestResponse, helper.CurrentApiId, ContentType, ResType, ApiUserId, ApiPassword);
                                        requestResponse.ResponseText = apires;
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")Payout Transaction Response :  (" + apires + ")";
                                    }
                                    else if (helper.CurrentApiId == 9)
                                    {
                                        var url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(model.firstname, model.lastname, ApiPassword, model.mobileno, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, "", "", model.accountno, model.BankName, model.ifsc, model.ourtxnid, model.beneficiaryName, model.transfermode, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";
                                        url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(model.firstname, model.lastname, ApiPassword, model.mobileno, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, "", "", model.accountno, model.BankName, model.ifsc, model.ourtxnid, model.beneficiaryName, model.transfermode, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";

                                        requestResponse.RequestTxt = url + " DATA " + postdata;
                                        requestResponse.Remark = "payout";
                                        apires = Method == "POST" ? apiCall.Post(url, postdata, ref model, ContentType, ResType, ApiUserId, ApiPassword)
                                                                                                               : apiCall.Get(url, ref requestResponse, helper.CurrentApiId, ContentType, ResType, ApiUserId, ApiPassword);

                                        requestResponse.ResponseText = apires;
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")Payout Transaction Response :  (" + apires + ")";

                                    }
                                    else if (helper.CurrentApiId == 11)
                                    {
                                        var url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(model.firstname, model.lastname, ApiPassword, model.mobileno, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, "", "", model.accountno, model.BankName, model.ifsc, model.ourtxnid, model.beneficiaryName, model.transfermode, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";
                                        url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(model.firstname, model.lastname, ApiPassword, model.mobileno, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, "", "", model.accountno, model.BankName, model.ifsc, model.ourtxnid, model.beneficiaryName, model.transfermode, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";

                                        requestResponse.RequestTxt = url + " DATA " + postdata;
                                        requestResponse.Remark = "payout";
                                        apires = Method == "POST" ? apiCall.Post2(url, postdata) : apiCall.Get(url, ref requestResponse, helper.CurrentApiId, ContentType, ResType, ApiUserId, ApiPassword);
                                        requestResponse.ResponseText = apires;
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")Payout Transaction Response :  (" + apires + ")";

                                    }
                                    else if (helper.CurrentApiId == 13) //bankit api for  DMT
                                    {
                                        //if(model.transfermode == "NEFT")
                                        //{
                                        //    ApiUrl = "https://services.bankit.in:8443/DMR/transact/NEFT/v1/remit";
                                        //}
                                        var url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.customerId, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, model.ifsc, "", "", model.accountno, model.mobileno, model.Name, model.address, model.BankName, model.beneficiaryName, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";
                                        requestResponse.RequestTxt = url + " DATA " + postdata;
                                        requestResponse.Remark = "DMT";
                                        apires = Method == "POST" ? apiCall.Post(url, postdata, ref log) : apiCall.Get(url, ref requestResponse, helper.CurrentApiId, ContentType, ResType, ApiUserId, ApiPassword);
                                        requestResponse.ResponseText = apires;
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")DMT Transaction Response :  (" + apires + ")";
                                    }
                                    else if (helper.CurrentApiId == 14) //Bing Pay  Payout
                                    {
                                        var url = ApiUrl.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.accountno, model.OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, string.Empty, ExtraUrl, "", model.accountno, model.ifsc, string.Empty, string.Empty, string.Empty, model.firstname + model.lastname, "", "", "", "", "", "", "", string.Empty, "", model.userid);
                                        var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(ApiUserId, ApiPassword, ApiOptional, model.customerId, OpCode, Convert.ToString(model.bankid), Convert.ToString(model.amount), ourref, string.Empty, model.ifsc, "", "", model.accountno, model.mobileno, model.Name, model.address, model.BankName, model.beneficiaryName, "", "", "", "", "", "", "", string.Empty, "", model.userid) : "";
                                        requestResponse.RequestTxt = url + " DATA " + postdata;
                                        requestResponse.Remark = "Payout";
                                        apires = Method == "POST" ? apiCall.Post(url, postdata, ref log) : apiCall.GetBingPay(url, ref requestResponse);
                                        requestResponse.ResponseText = apires;
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")Payout Transaction Response :  (" + apires + ")";
                                    }

                                    string apires1 = apires;
                                    try
                                    {
                                        requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);
                                    }
                                    catch (Exception e1)
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")nullresp excp= " + e1.Message;
                                        LIBS.Common.LogException(e1);
                                    }
                                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")resp saved to rec, ";
                                    #endregion

                                    #region "Handle Response"
                                    FilterResponseModel fResp = new FilterResponseModel();
                                    if (string.IsNullOrEmpty(apires1))
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")null resp, ";
                                    }
                                    else
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")handle by tagvalue resp, ";
                                        try
                                        {
                                            // FilterResponseModel fResp = new FilterResponseModel();
                                            FilterRespTagValue(helper.CurrentApiId, Convert.ToInt32(UrlId), ResType, apires1, ref statusId, ref log, ref fResp);
                                            fResp.ApiTxnID = !string.IsNullOrEmpty(fResp.ApiTxnID) ? fResp.ApiTxnID : string.Empty;
                                            fResp.CustomerName = !string.IsNullOrEmpty(fResp.CustomerName) ? fResp.CustomerName : string.Empty;
                                            fResp.OperatorTxnID = !string.IsNullOrEmpty(fResp.OperatorTxnID) ? fResp.OperatorTxnID : string.Empty;
                                            fResp.Message = !string.IsNullOrEmpty(fResp.Message) ? fResp.Message : StatusMsg.VALID_REQUEST;
                                            fResp.Message = "p" + helper.CurrentPriorityId + "-v" + helper.CurrentApiId + "-" + fResp.Message;
                                            status = statusId == StatsCode.SUCCESS ? StatusMsg.RECHARGE_SUCCESS : statusId == StatsCode.PROCESSING ? StatusMsg.RECHARGE_PROCESSING : statusId == StatsCode.FAILED ? StatusMsg.RECHARGE_FAILED : StatusMsg.RECHARGE_PROCESSING;
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")status";
                                            statusId = statusId == StatsCode.HOLD ? StatsCode.PROCESSING : statusId;
                                            status = statusId == 2 ? "processing" : statusId == 1 || statusId == 6 ? "processed" : statusId == 3 || statusId == 7 ? "failure" : "processing";

                                            response = JObject.FromObject(new
                                            {
                                                status = status,
                                                errorCode = ErrorCode.NO_ERROR,
                                                message = status,
                                                data = new
                                                {
                                                    utrNumber = fResp.OperatorTxnID,
                                                    apiRecordId = Convert.ToString(helper.RecId),
                                                    referenceId = model.ourtxnid,
                                                    requestId = model.reftxnid
                                                }
                                            });
                                        }
                                        catch (Exception e2)
                                        {
                                            statusId = StatsCode.PROCESSING;
                                            status = StatusName.PROCESSING;

                                            response = JObject.FromObject(new
                                            {
                                                status = status,
                                                errorCode = error,
                                                message = StatusMsg.RECHARGE_PROCESSING,
                                                data = new
                                                {
                                                    utrNumber = "",
                                                    apiRecordId = Convert.ToString(helper.RecId),
                                                    referenceId = model.ourtxnid,
                                                    requestId = model.reftxnid
                                                }
                                            });
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")handle tagvalue excp= " + e2.Message;
                                            LIBS.Common.LogException(e2);
                                        }
                                    }
                                    if (IsSwitched)
                                    {
                                        log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")switch end rec=" + helper.RecId;
                                    }
                                    #endregion
                                    if (statusId == 3)
                                    {
                                        //  response = RechargeProcess(model, response, helper, ref userReqRes, ref log, ref statusId, true, path);
                                        try
                                        {
                                            UpdateStatusWithDMTCheck(helper.RecId, Convert.ToInt32(model.userid), StatsCode.FAILED, fResp.ApiTxnID, fResp.OperatorTxnID, fResp.Message, "Auto", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(model.opid), string.Empty);
                                            response = MoneyTransferProcess(model, response, helper, ref userReqRes, ref log, ref statusId, true, path);
                                        }
                                        catch (Exception exx)
                                        {
                                            statusId = StatsCode.FAILED;
                                            status = StatusName.FAILURE;

                                            response = JObject.FromObject(new
                                            {
                                                status = status,
                                                errorCode = error,
                                                message = StatusMsg.RECHARGE_PROCESSING,
                                                data = new
                                                {
                                                    utrNumber = "",
                                                    apiRecordId = Convert.ToString(helper.RecId),
                                                    referenceId = model.ourtxnid,
                                                    requestId = model.reftxnid
                                                }
                                            });
                                            Common.LogException(exx);
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + "), status updateexcp exx=" + exx.Message;
                                        }
                                    }
                                    //api pack & txn
                                    else if (statusId == StatsCode.SUCCESS || statusId == StatsCode.PROCESSING)
                                    {
                                        try
                                        {
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")UpdateStatusWithCheck-start";
                                            UpdateStatusWithDMTCheck(helper.RecId, Convert.ToInt32(model.userid), statusId, fResp.ApiTxnID, fResp.OperatorTxnID, fResp.Message, "Auto", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(model.opid), fResp.CustomerName);
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")UpdateStatusWithCheck-start";
                                        }
                                        catch (Exception e3)
                                        {
                                            statusId = StatsCode.PROCESSING;
                                            status = StatusName.PROCESSING;

                                            response = JObject.FromObject(new
                                            {
                                                status = status,
                                                errorCode = error,
                                                message = StatusMsg.RECHARGE_PROCESSING,
                                                data = new
                                                {
                                                    utrNumber = "",
                                                    apiRecordId = Convert.ToString(helper.RecId),
                                                    referenceId = model.ourtxnid,
                                                    requestId = model.reftxnid
                                                }
                                            });
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") update status excp e3= " + e3.Message;
                                            Common.LogException(e3);
                                        }
                                        try
                                        {
                                            if (statusId == StatsCode.SUCCESS)
                                            {
                                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")GenerateApiTxn-start";
                                                //GenerateApiTxn(helper.CurrentApiId, model.opid, model.amount, TxnId, RecId, fResp.Vendor_CL_Bal, ref log, helper.LapuId, helper.LapuNo);
                                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")GenerateApiTxn-end";
                                            }

                                        }
                                        catch (Exception exc)
                                        {
                                            statusId = StatsCode.PROCESSING;
                                            status = StatusName.PROCESSING;

                                            response = JObject.FromObject(new
                                            {
                                                status = status,
                                                errorCode = error,
                                                message = StatusMsg.RECHARGE_PROCESSING,
                                                data = new
                                                {
                                                    utrNumber = "",
                                                    apiRecordId = Convert.ToString(helper.RecId),
                                                    referenceId = model.ourtxnid,
                                                    requestId = model.reftxnid
                                                }
                                            });
                                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")api txn save excp exc= " + exc.Message;
                                            LIBS.Common.LogException(exc);
                                        }
                                    }
                                    #endregion
                                }
                            }
                            else if (error == ErrorCode.LOW_API_BALANCE || error == ErrorCode.OPERATOR_NOT_EXIST || error == ErrorCode.APIURL_NOT_SET || error == ErrorCode.LAPU_NOT_SET || error == ErrorCode.LAPU_LOW_BA || error == ErrorCode.INVALID_COMMISSION_AMT || error == ErrorCode.PROCESSING_QUEUE_SIZE_FULL)
                            {
                                string RecId = Convert.ToString(cmd.Parameters["@RecId"].Value);
                                helper.RecId = !string.IsNullOrWhiteSpace(RecId) ? Convert.ToInt64(RecId) : 0;

                                statusId = StatsCode.FAILED;
                                status = StatusName.FAILURE;

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")sp error= " + error + "-" + ErrorDesc + " recid=" + RecId;
                                response = JObject.FromObject(new
                                {
                                    status = status,
                                    errorCode = error,
                                    message = ErrorDesc,
                                    data = new
                                    {
                                        utrNumber = "",
                                        apiRecordId = RecId,
                                        referenceId = model.ourtxnid,
                                        requestId = model.reftxnid
                                    }
                                });
                                response = MoneyTransferProcess(model, response, helper, ref userReqRes, ref log, ref statusId, false, path);
                            }
                            else
                            {
                                statusId = StatsCode.FAILED;
                                status = StatusName.FAILURE;

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")sp error= " + error + "-" + ErrorDesc;
                                response = JObject.FromObject(new
                                {
                                    status = status,
                                    errorCode = error,
                                    message = ErrorDesc,
                                    data = new
                                    {
                                        utrNumber = "",
                                        apiRecordId = Convert.ToString(helper.RecId),
                                        referenceId = model.ourtxnid,
                                        requestId = model.reftxnid
                                    }
                                });
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        log += "\r\n excp= " + ex.Message;
                        Common.LogException(ex, "log=" + log);

                        status = statusId == StatsCode.SUCCESS ? StatusMsg.RECHARGE_SUCCESS :
                                 statusId == StatsCode.PROCESSING ? StatusMsg.RECHARGE_PROCESSING :
                                 statusId == StatsCode.FAILED ? StatusMsg.RECHARGE_FAILED :
                                 StatusMsg.RECHARGE_PROCESSING;

                        statusId = statusId == StatsCode.HOLD ? StatsCode.PROCESSING : statusId;
                        string statusName = statusId == 2 ? "processing" : statusId == 1 || statusId == 6 ? "processed" : statusId == 3 || statusId == 7 ? "failure" : "processing";

                        response = JObject.FromObject(new
                        {
                            status = statusName,
                            errorCode = ErrorCode.UNKNOWN_ERROR,
                            message = status,
                            data = new
                            {
                                utrNumber = "",
                                apiRecordId = Convert.ToString(helper.RecId),
                                referenceId = model.ourtxnid,
                                requestId = model.reftxnid
                            }
                        });
                    }
                }
            }
            return response;
        }

        private void CalculateComm(string rcamt, int packid, string OpId, ref decimal commAmt, ref decimal debitamount, ref string log)
        {

            log += "\r\n user pack=" + packid + ",";
            decimal comm = 0;

            var packcomm = packageService.GetPackageCommByOpId(packid, Convert.ToInt32(OpId));
            log += "\r\n user comm=" + packcomm?.Id ?? 0 + ",";
            if (packcomm != null)
            {
                comm = packcomm.CommAmt ?? 0;

                if (packcomm.AmtTypeId == 5)//discount
                {
                    if (packcomm.CommTypeId == 1)//flat
                    {
                        commAmt = comm;
                        debitamount = debitamount - commAmt;
                    }
                    else if (packcomm.CommTypeId == 2)//percent
                    {
                        commAmt = comm * (Convert.ToDecimal(rcamt)) / 100;
                        debitamount = debitamount - commAmt;
                    }
                    else if (packcomm.CommTypeId == 3)//range
                    {
                        commAmt = comm * (Convert.ToDecimal(rcamt)) / 100;
                        debitamount = debitamount - commAmt;
                    }

                }
                else if (packcomm.AmtTypeId == 4)//surcharge
                {
                    if (packcomm.CommTypeId == 1)//flat
                    {
                        commAmt = comm;
                        debitamount = debitamount + commAmt;
                    }
                    else if (packcomm.CommTypeId == 2)//percent
                    {
                        commAmt = comm * Convert.ToDecimal(rcamt) / 100;
                        debitamount = debitamount + commAmt;
                    }
                    else if (packcomm.CommTypeId == 3)//range
                    {
                        commAmt = comm * Convert.ToDecimal(rcamt) / 100;
                        debitamount = debitamount + commAmt;
                    }
                }

            }

            log += "\r\n user comm amt=" + commAmt + ",";
        }


        [Route("~/prod/getTransactionStatus")]
        [HttpGet]
        public JObject StatusCheck(string requestId = "")
        {
            JObject response = new JObject();

            string log = "Start Status Check";

            var request = HttpContext.Current.Request;
            string ApiToken = Convert.ToString(request.Headers["authKey"]);
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.RequestTxt = request.Url.AbsoluteUri;
            userReqRes.Remark = "GetStatus";


            try
            {
                log += "\r\n ApiToken = " + ApiToken + ", RefTxnId = " + requestId;


                if (!ValidateToken(ApiToken))
                {
                    log += "\r\n c1(" + DateTime.Now.TimeOfDay.ToString() + "),";
                    response = JObject.FromObject(new
                    {
                        message = "provided authKey is wrong."

                    });
                }
                else
                {
                    User user = userService.GetUserByApiToken(ApiToken);


                    if (user == null)
                    {
                        log += "\r\n c2(" + DateTime.Now.TimeOfDay.ToString() + "),";
                        response = JObject.FromObject(new
                        {
                            message = "provided authKey is wrong."

                        });
                    }
                    else if (!user.IsActive)
                    {
                        log += "\r\n c3(" + DateTime.Now.TimeOfDay.ToString() + "),";
                        response = JObject.FromObject(new
                        {
                            message = "you are an Inactive User.Contact to Support/Administrator!"
                        });
                    }
                    else
                    {

                        log += "\r\n userid=" + user.Id;
                        AddDMTUpdateReqRes(userReqRes, ref log);
                        DMT recharge = reqDmtReportService.GetDmtStatusCheck(0, string.Empty, requestId, user.Id);

                        if (recharge == null)
                        {
                            log += "\r\n , rc not found";
                            response = JObject.FromObject(new
                            {
                                message = "provided requestId is wrong."
                            });
                        }
                        else
                        {
                            userReqRes.UserId = user.Id;
                            userReqRes.RecId = recharge.Id;
                            userReqRes.RefId = recharge.OurRefTxnId;

                            //response = JObject.FromObject(new
                            //{
                            //    CUSTOMERNO = recharge.AccountNo,
                            //    OPERATOR = recharge.Operator.Name,
                            //    AMOUNT = recharge.Amount,
                            //    STATUS = recharge.StatusId == 4 ? 2 : recharge.StatusId,
                            //    MESSAGE = recharge.StatusId == 4 ? "Processing" : recharge.StatusType.TypeName,
                            //    ERRORCODE = ErrorCode.NO_ERROR,
                            //    TXNNO = recharge.TxnId,
                            //    OPTXNID = recharge.OptTxnId,
                            //    REQUESTTXNID = recharge.UserTxnId,
                            //    HTTPCODE = HttpStatusCode.OK
                            //});
                            int statusId = recharge?.StatusId == 4 ? 2 : recharge?.StatusId ?? 0;
                            string status = statusId == 2 ? "processing" : statusId == 1 || statusId == 6 ? "processed" : statusId == 3 || statusId == 7 ? "failure" : "processing";
                            response = JObject.FromObject(new
                            {
                                message = "succeed",
                                data = new
                                {
                                    status = status,
                                    utrNumber = recharge.OptTxnId,
                                    apiRecordId = Convert.ToString(recharge.Id),
                                    requestId = requestId
                                }
                            });
                        }
                    }
                }
                log += "\r\n , end Status Check";
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                log += "\r\n , expc=" + ex.ToString();
                response = JObject.FromObject(new
                {
                    message = "Internal Server Error."
                });
            }
            userReqRes.ResponseText = response.ToString();

            userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);

            Common.LogActivity(log);
            log = string.Empty;
            userReqRes = null;
            return response;

        }


        [Route("~/Service/VendorStatusCheck")]
        [HttpGet]
        public string VendorStatusCheck(string token = "", string recid = "", int uid = 0)
        {
            string respStr = string.Empty;

            JObject response = new JObject();
            //StatusCheckForVendor
            string log = "start-VendorStatusCheck token=" + token + " recid=" + recid;

            Dto.RechargeDetail recharge = new Dto.RechargeDetail();

            recharge.RecId = !string.IsNullOrEmpty(recid) ? Convert.ToInt64(recid) : 0;
            recharge = GetDMTDetail(recharge.RecId, "status");
            #region  dmt get data  using Service
            //var data = reqDmtReportService.GetRecharge(recharge.RecId);
            //var apidata = reqDmtReportService.GetApiUrlById(Convert.ToInt32(data.ApiId), 3);
            //recharge.OurRefTxnId = data.OurRefTxnId;
            //recharge.StatusId = (int)data.StatusId;
            //recharge.Method = apidata.Method;
            //recharge.ApiUrl = apidata.URL;
            //recharge.ApiId = (int)data.ApiId;
            //recharge.UrlId = apidata.Id;
            //recharge.ResType = apidata.ResType;
            //recharge.PostData = apidata.PostData;
            //recharge.UserId = (int)uid;
            #endregion
            if (string.IsNullOrEmpty(recid) || string.IsNullOrEmpty(token) || Fetch_UserIP() != SiteKey.DomainIPAddress)
                return "failed";
            try
            {
                log += "\r\n, (recid=" + recharge.RecId + ") GetRechargeDetail";

                log += " (model.RecId=" + recharge.RecId + ", old model.statusId=" + recharge.StatusId + ") ourref=" + recharge.OurRefTxnId + " customerno=" + recharge.CustomerNo + ", apiId=" + recharge.ApiId + ", apiname=" + recharge.ApiName;

                int statusId = 0;
                string optxnid = "";
                string apitxnid = "";
                string statusmsg = "";
                string remark = "AutoStatusCheck";
                bool IsDownline = false;
                bool IsRefund = false;


                log += "\r\n, CheckStatus";
                CheckStatus(recharge, remark, ref IsDownline, ref apitxnid, ref statusmsg, ref optxnid, ref statusId, ref log);
                log += " (new statusId=" + statusId + ")";

                //recharge = GetRechargeDetail(recharge.RecId, "status");

                TimeSpan reqtime = DateTime.Now - Convert.ToDateTime(recharge.RequestTime);
                TimeSpan rsndtime = reqtime;


                if (!string.IsNullOrEmpty(recharge.ResendTime))
                    rsndtime = DateTime.Now - Convert.ToDateTime(recharge.ResendTime);

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") RequestTime=" + recharge.RequestTime + " reqtime.TotalMinutes=" + reqtime.TotalMinutes + " rsndtime.TotalMinutes=" + rsndtime.TotalMinutes + " resendtime=" + recharge.ResendTime + " resendcount=" + recharge.ResendCount + " ResendWaitTime=" + recharge.ResendWaitTime;


                if ((uid == 0 && reqtime.TotalMinutes > 2 && recharge.ResendWaitTime > 0 && recharge.ResendWaitTime > recharge.WaitTime && reqtime.TotalMinutes > recharge.ResendWaitTime && reqtime.TotalMinutes > recharge.WaitTime && recharge.StatusId == 2 && statusId == 3 && recharge.ResendCount < 3) &&
                    ((recharge.ResendCount == 0) || ((recharge.ResendCount == 1 || recharge.ResendCount == 2) && rsndtime.TotalMinutes > 2)))
                {

                    RequestResponseDto requestResponse = new RequestResponseDto();
                    requestResponse.Remark = "AutoResend";
                    requestResponse.RecId = recharge.RecId;
                    requestResponse.UserId = recharge.UserId;
                    requestResponse.RefId = recharge.OurRefTxnId;
                    requestResponse.CustomerNo = recharge.CustomerNo;
                    requestResponse.UserReqId = recharge.UserTxnId;
                    requestResponse.RequestTxt = "rec-status=" + recharge.StatusId + " resp-statusid=" + statusId;
                    //SetRechargeUpdatedBy("Resend", recharge.RecId, uid, string.Empty, ref log);

                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")AutoResend start, ";
                    RequestResponseDto userReqRes = new RequestResponseDto();

                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")recharge.TxnRemark=" + recharge.TxnRemark;

                    //response = (recharge.TxnRemark.ToLower().Contains("billpay")) ?
                    //           CallbackBillPayRequest(recharge, recharge.ApiId, ref log, ref response, ref userReqRes, ref statusId) :
                    //           CallbackRechargeRequest(recharge, recharge.ApiId, ref log, ref response, ref userReqRes, ref statusId);

                    //requestResponse.ResponseText = response.ToString();
                    //requestResponse = AddUpdateReqRes(requestResponse, ref log);

                }
                else
                {
                    //UpdateStatusWithCheck(recharge.RecId, uid, statusId, apitxnid, optxnid, string.Empty, remark, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, recharge.OpId, 0, 0);
                    UpdateStatusWithDMTCheck(recharge.RecId, Convert.ToInt32(recharge.UserId), statusId, apitxnid, optxnid, "", "Auto", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(recharge.OpId), recharge.BeneficiaryName);
                    apitxnid = string.IsNullOrEmpty(apitxnid) ? recharge.ApiTxnId : apitxnid;
                    optxnid = string.IsNullOrEmpty(optxnid) ? recharge.OptTxnId : optxnid;
                    statusmsg = string.IsNullOrEmpty(statusmsg) ? recharge.StatusMsg : statusmsg;
                }
                statusId = statusId == 4 || statusId == 0 ? 2 : statusId;

                if (statusId != 2)
                {
                    #region "Send Callback to User"
                    log += "\r\n, get user callback url ";

                    if (recharge.UserId > 0)
                    {
                        SendCallBack(recharge.RecId, ref log);
                    }
                    else
                    {
                        log += "\r\n, usernotfound";
                    }
                    log += "\r\n, callback sent to user ";
                    #endregion
                }

                respStr = "success";
            }
            catch (Exception ex)
            {
                respStr = "failed";
                Common.LogException(ex, "statuscheck recid=" + recharge.RecId);
                log += "\r\n, excp(recid=" + recharge.RecId + ") excp=" + ex.Message;

            }

            Common.LogActivity(log);

            return respStr = "success";
        }

        [Route("~/Prod/Wallet")]
        [HttpGet]
        public JObject ApiUserBalanceCheck(string ApiToken = "")
        {
            string log = "Balance -start";
            JObject response = new JObject();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {

                    SqlCommand cmd = new SqlCommand("usp_GetUserBalance", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiToken", ApiToken);
                    cmd.Parameters.Add("@Balance", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmd.Parameters["@Balance"].Precision = 20;
                    cmd.Parameters["@Balance"].Scale = 4;
                    cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Error", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    log += "\r\n ,  before execute -usp_GetUserBalance";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after exec-sp";

                    string balance = Convert.ToString(cmd.Parameters["@Balance"].Value);
                    string Error = Convert.ToString(cmd.Parameters["@Error"].Value);
                    string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;

                    if (Error == "0")
                    {
                        response = JObject.FromObject(new
                        {
                            status = "succeed",
                            walletMoney = balance
                        });
                    }
                    else
                    {
                        response = JObject.FromObject(new
                        {
                            status = "failure",
                            walletMoney = string.Empty
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp Balance=" + ex.Message;
                Common.LogException(ex);

                response = JObject.FromObject(new
                {
                    status = "failure",
                    walletMoney = string.Empty
                });
            }

            log += "\r\n ,  Balance-end";

            Common.LogActivity(log);
            return response;

        }

        #region call back 
        [Route("~/Service/RecCallback/{eApiID}")]
        [HttpGet]
        [HttpPost]
        public string RecCallback(string eApiID = "")
        {
            string res = "failed";
            string log = " (" + DateTime.Now.TimeOfDay.ToString() + ")callback start-,eApiID=" + eApiID;
            //  JObject response = new JObject();
            try
            {
                long recid = 0, lapuid = 0;
                int opId = 0, statusId = 2, userid = 0, recapiid = 0, rcstatusid = 0;
                string host = Request.RequestUri.AbsoluteUri.ToString();
                string Post = Request.Method.ToString();
                string apires = HttpUtility.UrlDecode(host);

                if (Post == "POST")
                {
                    StreamReader reader = new StreamReader(HttpContext.Current.Request.InputStream);
                    apires = reader.ReadToEnd();
                }

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")url=" + host + ", ";

                var apisource = apiService.GetCallbackApi(string.IsNullOrEmpty(eApiID) ? "NotAvailable" : eApiID);
                var apiid = apisource != null ? apisource.Id : 0;
                var apiurl = apiService.GetApiurl(apiid, 4);

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")apiurl-id=" + apiurl.Id + ", ";

                RequestResponseDto requestResponse = new RequestResponseDto();
                ApiCall apiCall = new ApiCall(reqResService);
                requestResponse.RequestTxt = apires;
                requestResponse.UrlId = apiurl?.Id ?? 0;
                requestResponse.Remark = "CallBack";

                log += "\r\n reqres saved, ";
                FilterResponseModel fResp = new FilterResponseModel();
                FilterRespTagValue(apiid, apiurl.Id, apiurl.ResType, apires, ref statusId, ref log, ref fResp);

                if (string.IsNullOrEmpty(fResp.Status))
                {
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")blank status,";
                    statusId = StatsCode.PROCESSING;
                }

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")tavlues retrieved, reqtxnid=" + fResp.RequestTxnId + ", apitxnid=" + fResp.ApiTxnID + ", ";

                try
                {
                    using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("usp_GetDMTByRef", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Remark", "Callback");
                        cmd.Parameters.AddWithValue("@OurRefTxnId", fResp.RequestTxnId);
                        cmd.Parameters.AddWithValue("@ApiTxnId", fResp.ApiTxnID);
                        cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@RecId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@StatusId", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@ApiId", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.Parameters.AddWithValue("@LapuNo", fResp.LapuNo);
                        cmd.Parameters.Add("@OpId", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@LapuId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                        log += "\r\n ,  before exec = usp_GetRechargeByRef";
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);
                        string rid = Convert.ToString(cmd.Parameters["@RecId"].Value);
                        string uid = Convert.ToString(cmd.Parameters["@UserId"].Value);
                        string sid = Convert.ToString(cmd.Parameters["@StatusId"].Value);
                        string apid = Convert.ToString(cmd.Parameters["@ApiId"].Value);
                        string Lpid = Convert.ToString(cmd.Parameters["@LapuId"].Value);
                        string oid = Convert.ToString(cmd.Parameters["@OpId"].Value);

                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog + ", apid=" + apid + ",uid=" + uid + ",sid=" + sid + ",rid=" + rid;

                        recid = !string.IsNullOrEmpty(rid) ? Convert.ToInt64(rid) : 0;
                        userid = !string.IsNullOrEmpty(uid) ? Convert.ToInt32(uid) : 0;
                        rcstatusid = !string.IsNullOrEmpty(sid) ? Convert.ToInt32(sid) : 0;
                        recapiid = !string.IsNullOrEmpty(apid) ? Convert.ToInt32(apid) : 0;

                        lapuid = !string.IsNullOrEmpty(Lpid) ? Convert.ToInt64(Lpid) : 0;
                        opId = !string.IsNullOrEmpty(oid) ? Convert.ToInt32(oid) : 0;

                        requestResponse.RecId = recid;
                        requestResponse.RefId = fResp.RequestTxnId;
                        requestResponse.ResponseText = "recharge-statusId=" + rcstatusid + ", callback statusId=" + statusId;
                        if (userid > 0)
                        {
                            requestResponse.UserId = userid;
                        }
                        requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);
                    }
                }
                catch (Exception cex)
                {
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")sp-excp=" + cex.Message;
                    Common.LogException(cex);
                }

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")rcstatus=" + rcstatusid + " callback status=" + statusId + ", ";

                if (recid == 0)
                {
                    log += "\r\n recharge not found, ";
                }
                else if (recapiid != apiid)
                {
                    log += "\r\n recharge api and callback api not same, ";
                    string remark = "(rcapi=" + recapiid + " and cbkApi=" + apiid + ",rsid=" + rcstatusid + "cbsid=" + statusId + ")";
                    SetDMTUpdatedBy("InvalidCallbackApi", recid, 0, remark, ref log);
                }
                else if ((rcstatusid != StatsCode.HOLD) && (statusId == StatsCode.FAILED || statusId == StatsCode.SUCCESS))
                {
                    // Callback resend work
                    // check not already resend
                    // Dto.RechargeDetail recharge = GetDMTDetail(recid, string.Empty);
                    Dto.RechargeDetail recharge = GetDMTDetail(recid, string.Empty);
                    // resend only processing recharge
                    // check wait time before resend

                    TimeSpan ts = DateTime.Now - Convert.ToDateTime(recharge.RequestTime);
                    TimeSpan ts1 = ts;
                    var wtime = apisource.WaitTime ?? 0;
                    var rswtime = apisource.ResendWaitTime ?? 0;

                    if ((recharge.ApiId == apisource.Id) && ((recharge.StatusId == 2 && statusId == 1) ||
                                                              (recharge.StatusId == 1 && statusId == 1) ||
                                                              (recharge.StatusId == 1 && statusId == 3) ||
                                                              (recharge.StatusId == 3 && statusId == 1)) ||
                                                              (IsStopRouteMessageExists(apires, (int)recharge.ApiId, (int)recharge.OpId))
                                                              )
                    {

                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") both status are success call status change, ";
                        string remark = "CallBack";
                        string sclog = string.Empty;

                        sclog = DMTStatusChange(sclog, fResp.ApiTxnID, fResp.OperatorTxnID, fResp.Message, statusId, userid, recid, requestResponse, fResp.Vendor_CL_Bal, remark, lapuid, fResp.LapuNo, opId, fResp.R_Offer, string.Empty);
                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")" + sclog;
                    }

                    else if ((wtime == 0 && rswtime > 0) ||
                       (rswtime > 0 && wtime < ts.TotalMinutes) ||
                       (recharge.ApiId != apisource.Id) ||
                       (recharge.StatusId == statusId)
                       )
                    {
                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") do nothing, ";
                    }
                    else if ((wtime > 0 && wtime > ts.TotalMinutes) &&
                        (recharge.StatusId == 2 && statusId == 3) &&
                        (apisource.Id == recharge.ApiId))
                    {
                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")callback resend start, ";
                        requestResponse.Remark = "CallbackRoute";
                        JObject response = new JObject();
                        RequestResponseDto userReqRes = new RequestResponseDto();

                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")recharge.TxnRemark=" + recharge.TxnRemark;

                        requestResponse.ResponseText = response.ToString();
                        requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);

                        #region "Send Callback to User"
                        statusId = statusId == 0 || statusId == 4 ? 2 : statusId;

                        if (recharge.StatusId == 2 && statusId != 2)
                        {

                            log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")userid=" + recharge.UserId;
                            SendCallBack(recid, ref log);
                        }

                        #endregion

                    }
                    else if (recharge.StatusId == 2 && statusId != 2 && apisource.Id == recharge.ApiId && ((wtime > 0 && wtime < ts.TotalMinutes && rswtime == 0) || (wtime == 0 && rswtime == 0)))
                    {
                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")call status change, ";
                        string remark = "CallBack";
                        string sclog = string.Empty;
                        ///StatusChange(model.StatusId, recharge, model.OpTxnId, model.ApiTxnId);

                        sclog = DMTStatusChange(sclog, fResp.ApiTxnID, fResp.OperatorTxnID, fResp.Message, statusId, userid, recid, requestResponse, fResp.Vendor_CL_Bal, remark, lapuid, fResp.LapuNo, opId, fResp.R_Offer, string.Empty);
                        log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")" + sclog;

                    }
                    else
                    {
                        log += "\r\n cannot call status change, ";
                    }

                }

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")callback -end,";
                res = "success";

            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

                res = "failed";
            }

            Common.LogActivity(log);
            log = string.Empty;

            return res;
        }


        private string StatusChange(string log, string apitxnid, string optxnid, string statusmsg, int statusId, int userid, long recid, RequestResponseDto requestResponse, decimal VendorBalc, string remark, long lapuid, string lapuno, int opid, decimal roffer = 0, string comment = "")
        {
            try
            {
                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")StatusChange-start, ";

                //apitxnid = !string.IsNullOrEmpty(apitxnid) ? apitxnid : "NA";
                //optxnid = !string.IsNullOrEmpty(optxnid) ? optxnid : "NA";
                //statusmsg = !string.IsNullOrEmpty(statusmsg) ? statusmsg : StatusMsg.VALID_REQUEST;
                bool IsDownline = false;
                bool IsRefund = false;
                try
                {
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")sp-statusupdate-start";
                    UpdateStatusWithCheck(recid, 0, statusId, apitxnid, optxnid, statusmsg, remark, ref IsDownline, ref IsRefund, ref log, lapuid, lapuno, opid, VendorBalc, roffer);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")sp-statusupdate-end";
                }
                catch (Exception ee)
                {

                    LIBS.Common.LogException(ee);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")api txn save excp= " + ee.Message + ", ";
                }

                #region "Send Callback to User"
                log += ", (" + DateTime.Now.TimeOfDay.ToString() + ")getuserstart";
                User user = userService.GetUser(userid);
                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")user-end";
                if (user != null && (IsDownline || (statusId == 1 && !string.IsNullOrEmpty(optxnid))))
                {
                    // recharge= rechargeService.GetRecharge(recharge.Id);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")userid=" + user.Id;
                    SendCallBack(recid, ref log);
                }

                #endregion


            }
            catch (Exception e3)
            {
                log += "\r\n api txn save excp= " + e3.Message + ", ";
                LIBS.Common.LogException(e3);
            }
            log += "\r\n statuschange -end, ";
            return log;
        }
        private void UpdateStatusWithCheck(long RecId, int UserId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, decimal apibal, decimal roffer, string updatetype = "StatusWithCheck", string comment = "")
        {
            remark = string.IsNullOrEmpty(remark) ? "StatusWithCheck" : remark;
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateRechargeStatusPayin", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);

                if (UserId > 0)
                    cmd.Parameters.AddWithValue("@UserId", UserId);

                if (!string.IsNullOrWhiteSpace(apitxnid))
                    cmd.Parameters.AddWithValue("@ApiTxnId", apitxnid);

                if (!string.IsNullOrWhiteSpace(optxnid))
                    cmd.Parameters.AddWithValue("@OptTxnId", optxnid);

                if (!string.IsNullOrWhiteSpace(statusmsg))
                    cmd.Parameters.AddWithValue("@StatusMsg", statusmsg);

                if (!string.IsNullOrWhiteSpace(remark))
                    cmd.Parameters.AddWithValue("@Remark", remark);

                if (lapuid > 0)
                    cmd.Parameters.AddWithValue("@LapuId", lapuid);

                if (!string.IsNullOrWhiteSpace(lapuno))
                    cmd.Parameters.AddWithValue("@LapuNo", lapuno);

                if (opid > 0)
                    cmd.Parameters.AddWithValue("@OpId", opid);

                if (apibal > 0)
                    cmd.Parameters.AddWithValue("@ApiBal", apibal);
                if (roffer > 0)
                    cmd.Parameters.AddWithValue("@ROfferAmt", roffer);

                if (!string.IsNullOrWhiteSpace(comment))
                    cmd.Parameters.AddWithValue("@Comment", comment);

                cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 4000).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsRefund", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsDownline", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log += "\r\n ,  before exec = usp_UpdateRechargeStatus";

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                log += "\r\n ,  after exec = usp_UpdateRechargeStatus";
                string Refund = Convert.ToString(cmd.Parameters["@IsRefund"].Value);
                string Downline = Convert.ToString(cmd.Parameters["@IsDownline"].Value);
                string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);


                IsRefund = !string.IsNullOrEmpty(Refund) ? Convert.ToBoolean(Refund) : false;
                IsDownline = !string.IsNullOrEmpty(Downline) ? Convert.ToBoolean(Downline) : false;

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;
            }

        }

        private string DMTStatusChange(string log, string apitxnid, string optxnid, string statusmsg, int statusId, int userid, long recid, RequestResponseDto requestResponse, decimal VendorBalc, string remark, long lapuid, string lapuno, int opid, decimal roffer = 0, string comment = "")
        {
            try
            {
                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")StatusChange-start, ";

                //apitxnid = !string.IsNullOrEmpty(apitxnid) ? apitxnid : "NA";
                //optxnid = !string.IsNullOrEmpty(optxnid) ? optxnid : "NA";
                //statusmsg = !string.IsNullOrEmpty(statusmsg) ? statusmsg : StatusMsg.VALID_REQUEST;
                bool IsDownline = false;
                bool IsRefund = false;
                try
                {
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")sp-statusupdate-start";
                    UpdateDMTStatusWithCheck(recid, 0, statusId, apitxnid, optxnid, statusmsg, remark, ref IsDownline, ref IsRefund, ref log, lapuid, lapuno, opid, VendorBalc, roffer);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")sp-statusupdate-end";
                }
                catch (Exception ee)
                {

                    LIBS.Common.LogException(ee);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")api txn save excp= " + ee.Message + ", ";
                }

                #region "Send Callback to User"
                log += ", (" + DateTime.Now.TimeOfDay.ToString() + ")getuserstart";
                User user = userService.GetUser(userid);
                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")user-end";
                if (user != null && (IsDownline || (statusId == 1 && !string.IsNullOrEmpty(optxnid))))
                {
                    // recharge= rechargeService.GetRecharge(recharge.Id);
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")userid=" + user.Id;
                    SendDMTCallBack(recid, ref log);
                }

                #endregion


            }
            catch (Exception e3)
            {
                log += "\r\n api txn save excp= " + e3.Message + ", ";
                LIBS.Common.LogException(e3);
            }
            log += "\r\n statuschange -end, ";
            return log;
        }
        private void SendDMTCallBack(long recid, ref string log)
        {
            Dto.RechargeDetail recharge = GetDMTDetail(recid, string.Empty);
            int statusId = recharge.StatusId;
            // var recharge = rechargeService.GetRecharge(recid);
            log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")downline callback-start, ";
            if (string.IsNullOrEmpty(recharge.CallbackURL))
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") callback not exists, ";
            }
            else
            {
                recharge.StatusId = statusId == 4 ? Convert.ToInt16(recharge.StatusId) : statusId;
                recharge.CallbackURL = recharge.CallbackURL.ReplaceURL(string.Empty, string.Empty, recharge.UserTxnId, recharge.CustomerNo, recharge.OptTxnId, recharge.CircleId.ToString(), recharge.Amount.ToString(), recharge.TxnId.ToString(), statusId.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.UserId.ToString());
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")callback url, " + recharge.CallbackURL + " recid=" + recharge.RecId + ", ";
                RequestResponseDto requestResponse = new RequestResponseDto();
                //requestResponse.RecId = recharge.RecId;
                requestResponse.UserId = recharge.UserId;
                requestResponse.RefId = recharge.OurRefTxnId;
                requestResponse.Remark = "downline";
                log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")callapi, ";
                ApiCall apiCall = new ApiCall(reqResService);
                apiCall.Get(recharge.CallbackURL, ref requestResponse);
                log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")resp= " + requestResponse.ResponseText;
                requestResponse.ResponseText = requestResponse.ResponseText + ", sent statusId=" + statusId;

                requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);

                log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")resp saved";

            }
            log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")downline callback-end, ";
        }
        private void UpdateDMTStatusWithCheck(long RecId, int UserId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, decimal apibal, decimal roffer, string updatetype = "StatusWithCheck", string comment = "")
        {
            try
            {
                remark = string.IsNullOrEmpty(remark) ? "StatusWithCheck" : remark;
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_UpdateDMTStatus", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                    cmd.Parameters.AddWithValue("@RecId", RecId);
                    cmd.Parameters.AddWithValue("@StatusId", statusId);

                    if (UserId > 0)
                        cmd.Parameters.AddWithValue("@UserId", UserId);

                    if (!string.IsNullOrWhiteSpace(apitxnid))
                        cmd.Parameters.AddWithValue("@ApiTxnId", apitxnid);

                    if (!string.IsNullOrWhiteSpace(optxnid))
                        cmd.Parameters.AddWithValue("@OptTxnId", optxnid);

                    if (!string.IsNullOrWhiteSpace(statusmsg))
                        cmd.Parameters.AddWithValue("@StatusMsg", statusmsg);

                    if (!string.IsNullOrWhiteSpace(remark))
                        cmd.Parameters.AddWithValue("@Remark", remark);

                    if (lapuid > 0)
                        cmd.Parameters.AddWithValue("@LapuId", lapuid);

                    if (!string.IsNullOrWhiteSpace(lapuno))
                        cmd.Parameters.AddWithValue("@LapuNo", lapuno);

                    if (opid > 0)
                        cmd.Parameters.AddWithValue("@OpId", opid);

                    if (apibal > 0)
                        cmd.Parameters.AddWithValue("@ApiBal", apibal);
                    if (roffer > 0)
                        cmd.Parameters.AddWithValue("@ROfferAmt", roffer);

                    if (!string.IsNullOrWhiteSpace(comment))
                        cmd.Parameters.AddWithValue("@Comment", comment);

                    cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@IsRefund", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@IsDownline", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    log += "\r\n ,  before exec = usp_UpdateRechargeStatus";

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    log += "\r\n ,  after exec = usp_UpdateRechargeStatus";
                    string Refund = Convert.ToString(cmd.Parameters["@IsRefund"].Value);
                    string Downline = Convert.ToString(cmd.Parameters["@IsDownline"].Value);
                    string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);


                    IsRefund = !string.IsNullOrEmpty(Refund) ? Convert.ToBoolean(Refund) : false;
                    IsDownline = !string.IsNullOrEmpty(Downline) ? Convert.ToBoolean(Downline) : false;

                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;
                }
            }
            catch (Exception)
            {
            }
        }
        private void SetDMTUpdatedBy(string filter, long recid, int userid, string remark, ref string log)
        {
            log += "\r\n , SetRechargeUpdatedBy, recid=" + recid + " userid=" + userid + " filter=" + filter + " remark=" + remark;
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_SetDMTUpdatedBy", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FilterType", filter);
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@Remark", remark);


                    log += "\r\n ,  before execute = usp_SetRechargeUpdatedBy";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after execute";

                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp=" + ex.Message;
                Common.LogException(ex);
            }

        }
        private bool IsStopRouteMessageExists(string respstring, int apiid = 0, int opid = 0)
        {
            bool IsStop = false;

            try
            {

                var routemsg = rechargeService.GetStopRouteMessages(apiid, opid);
                if (string.IsNullOrEmpty(respstring) || routemsg == null)
                {
                    IsStop = false;
                }
                else if (routemsg.Count == 0)
                {
                    IsStop = false;
                }
                else
                {
                    foreach (var route in routemsg)
                    {
                        if (route.MsgFilter.Split(';').Any(s => respstring.ToLower().Contains(s.ToLower())))
                            IsStop = true;
                    }

                }


            }
            catch (Exception ex)
            {
                IsStop = false;
            }

            return IsStop;
        }
        #endregion
        private Dto.RechargeDetail GetDMTDetail(long recid, string remark)
        {
            DataTable dt = new DataTable();
            Dto.RechargeDetail model = new Dto.RechargeDetail();


            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetDMTDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@FilterType", remark);
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {

                        var row = dt.Rows[0];

                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(row["Id"])) ? Convert.ToInt64(row["Id"]) : 0;
                        model.UserId = !string.IsNullOrEmpty(Convert.ToString(row["UserId"])) ? Convert.ToInt32(row["UserId"]) : 0;
                        model.ApiId = !string.IsNullOrEmpty(Convert.ToString(row["ApiId"])) ? Convert.ToInt32(row["ApiId"]) : 0;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(row["TxnId"])) ? Convert.ToInt64(row["TxnId"]) : 0;
                        model.CustomerNo = Convert.ToString(row["CustomerNo"]);
                        model.OpId = !string.IsNullOrEmpty(Convert.ToString(row["OpId"])) ? Convert.ToInt32(row["OpId"]) : 0;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(row["Amount"])) ? Convert.ToDecimal(row["Amount"]) : 0;
                        model.StatusId = !string.IsNullOrEmpty(Convert.ToString(row["StatusId"])) ? Convert.ToInt32(row["StatusId"]) : 0;
                        model.BeneficiaryName = !string.IsNullOrEmpty(Convert.ToString(row["BeneficiaryName"])) ? Convert.ToString(row["BeneficiaryName"]) : "";

                        model.StatusMsg = Convert.ToString(row["StatusMsg"]);
                        model.UserTxnId = Convert.ToString(row["UserTxnId"]);
                        model.OurRefTxnId = Convert.ToString(row["OurRefTxnId"]);
                        model.OptTxnId = Convert.ToString(row["OptTxnId"]);
                        model.AccountNo = Convert.ToString(row["AccountNo"]);
                        model.UrlId = !string.IsNullOrEmpty(Convert.ToString(row["UrlId"])) ? Convert.ToInt32(row["UrlId"]) : 0;
                        model.ApiUrl = Convert.ToString(row["ApiUrl"]);
                        model.Method = Convert.ToString(row["Method"]);
                        model.ContentType = Convert.ToString(row["ContentType"]);
                        model.ResType = Convert.ToString(row["ResType"]);
                        model.PostData = Convert.ToString(row["PostData"]);
                        model.OpCode = Convert.ToString(row["OpCode"]);
                        model.ExtraUrl = Convert.ToString(row["ExtraUrl"]);
                        model.ExtraUrlData = Convert.ToString(row["ExtraUrlData"]);
                        model.ApiUserId = Convert.ToString(row["ApiUserId"]);
                        model.ApiPassword = Convert.ToString(row["ApiPassword"]);
                        model.ApiOptional = Convert.ToString(row["ApiOptional"]);
                        model.ApiTypeId = !string.IsNullOrEmpty(Convert.ToString(row["ApiTypeId"])) ? Convert.ToInt32(row["ApiTypeId"]) : 0;
                        model.CallbackURL = Convert.ToString(row["CallbackURL"]);
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(row["DB_Amt"])) ? Convert.ToDecimal(row["DB_Amt"]) : 0;
                        model.TxnRemark = Convert.ToString(row["TxnRemark"]);
                        model.ApiName = Convert.ToString(row["ApiName"]);

                        model.RequestTime = Convert.ToString(row["RequestTime"]);
                        model.CallbackId = Convert.ToString(row["CallbackId"]);


                    }

                }
            }
            catch (Exception ex)
            {
                // log += "\r\n , excp=" + ex.Message;
                Common.LogException(ex, "GetRechargeDetail RecId=" + recid);
            }

            return model;

        }
        private Dto.RechargeDetail GetRechargeDetail(long recid, string remark)
        {
            DataTable dt = new DataTable();
            Dto.RechargeDetail model = new Dto.RechargeDetail();


            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetRechargeDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@FilterType", remark);
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {

                        var row = dt.Rows[0];

                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(row["Id"])) ? Convert.ToInt64(row["Id"]) : 0;
                        model.UserId = !string.IsNullOrEmpty(Convert.ToString(row["UserId"])) ? Convert.ToInt32(row["UserId"]) : 0;
                        model.ApiId = !string.IsNullOrEmpty(Convert.ToString(row["ApiId"])) ? Convert.ToInt32(row["ApiId"]) : 0;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(row["TxnId"])) ? Convert.ToInt64(row["TxnId"]) : 0;
                        model.CustomerNo = Convert.ToString(row["CustomerNo"]);
                        model.OpId = !string.IsNullOrEmpty(Convert.ToString(row["OpId"])) ? Convert.ToInt32(row["OpId"]) : 0;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(row["Amount"])) ? Convert.ToDecimal(row["Amount"]) : 0;
                        model.StatusId = !string.IsNullOrEmpty(Convert.ToString(row["StatusId"])) ? Convert.ToInt32(row["StatusId"]) : 0;
                        model.StatusMsg = Convert.ToString(row["StatusMsg"]);
                        model.CircleId = !string.IsNullOrEmpty(Convert.ToString(row["CircleId"])) ? Convert.ToInt32(row["CircleId"]) : 0;
                        model.UserTxnId = Convert.ToString(row["UserTxnId"]);
                        model.OurRefTxnId = Convert.ToString(row["OurRefTxnId"]);
                        model.ApiTxnId = Convert.ToString(row["ApiTxnId"]);
                        model.OptTxnId = Convert.ToString(row["OptTxnId"]);
                        model.AccountNo = Convert.ToString(row["AccountNo"]);
                        model.AccountOther = Convert.ToString(row["AccountOther"]);
                        model.Optional1 = Convert.ToString(row["Optional1"]);
                        model.Optional2 = Convert.ToString(row["Optional2"]);
                        model.Optional3 = Convert.ToString(row["Optional3"]);
                        model.Optional4 = Convert.ToString(row["Optional4"]);
                        model.LapuId = !string.IsNullOrEmpty(Convert.ToString(row["LapuId"])) ? Convert.ToInt64(row["LapuId"]) : 0;
                        model.LapuNo = Convert.ToString(row["LapuNo"]);
                        model.AmountUnitId = !string.IsNullOrEmpty(Convert.ToString(row["AmountUnitId"])) ? Convert.ToInt32(row["AmountUnitId"]) : 0;
                        model.AmountLength = !string.IsNullOrEmpty(Convert.ToString(row["AmountLength"])) ? Convert.ToInt32(row["AmountLength"]) : 0;
                        model.DateTimeFormat = Convert.ToString(row["DateTimeFormat"]);
                        model.RefPadding = Convert.ToString(row["RefPadding"]);
                        model.RandomKey = Convert.ToString(row["RandomKey"]);
                        model.IsNumericOnly = !string.IsNullOrEmpty(Convert.ToString(row["IsNumericOnly"])) ? Convert.ToInt32(row["IsNumericOnly"]) : 0;
                        model.UrlId = !string.IsNullOrEmpty(Convert.ToString(row["UrlId"])) ? Convert.ToInt32(row["UrlId"]) : 0;
                        model.ApiUrl = Convert.ToString(row["ApiUrl"]);
                        model.Method = Convert.ToString(row["Method"]);
                        model.ContentType = Convert.ToString(row["ContentType"]);
                        model.ResType = Convert.ToString(row["ResType"]);
                        model.PostData = Convert.ToString(row["PostData"]);
                        model.OpCode = Convert.ToString(row["OpCode"]);
                        model.ExtraUrl = Convert.ToString(row["ExtraUrl"]);
                        model.ExtraUrlData = Convert.ToString(row["ExtraUrlData"]);
                        model.ApiUserId = Convert.ToString(row["ApiUserId"]);
                        model.ApiPassword = Convert.ToString(row["ApiPassword"]);
                        model.ApiOptional = Convert.ToString(row["ApiOptional"]);
                        model.ApiTypeId = !string.IsNullOrEmpty(Convert.ToString(row["ApiTypeId"])) ? Convert.ToInt32(row["ApiTypeId"]) : 0;
                        model.LapuPass = Convert.ToString(row["LapuPass"]);
                        model.LapuPIN = Convert.ToString(row["LapuPIN"]);
                        model.LapuOP1 = Convert.ToString(row["LapuOP1"]);
                        model.LapuOP2 = Convert.ToString(row["LapuOP2"]);
                        model.CallbackURL = Convert.ToString(row["CallbackURL"]);
                        model.CircleCode = Convert.ToString(row["CircleCode"]);
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(row["DB_Amt"])) ? Convert.ToDecimal(row["DB_Amt"]) : 0;
                        model.TxnRemark = Convert.ToString(row["TxnRemark"]);
                        model.ApiName = Convert.ToString(row["ApiName"]);
                        model.RequestTime = Convert.ToString(row["RequestTime"]);
                        model.ResendById = !string.IsNullOrEmpty(Convert.ToString(row["ResendById"])) ? Convert.ToInt32(row["ResendById"]) : 0;
                        model.ResendTime = Convert.ToString(row["ResendTime"]);
                        model.CircleCode = string.IsNullOrEmpty(model.CircleCode) ? "10" : model.CircleCode;
                        model.ResendWaitTime = Convert.ToInt32(row["ResendWaitTime"]);
                        model.WaitTime = Convert.ToInt32(row["WaitTime"]);
                        model.StatusCheckTime = Convert.ToInt32(row["StatusCheckTime"]);
                        model.IsAutoStatusCheck = Convert.ToBoolean(row["IsAutoStatusCheck"]);
                        model.CallbackId = Convert.ToString(row["CallbackId"]);
                        model.ApiBal = Convert.ToDecimal(row["ApiBal"]);
                        model.UserBal = Convert.ToDecimal(row["UserBal"]);
                        model.ResendCount = Convert.ToInt32(row["ResendCount"]);
                        model.IsROChecked = Convert.ToBoolean(row["IsROChecked"]);
                        model.IsValidRO = Convert.ToBoolean(row["IsValidRO"]);
                        model.Comment = Convert.ToString(row["Comment"]);
                        model.ApiComm = Convert.ToDecimal(row["ApiComm"]);
                        model.UserComm = Convert.ToDecimal(row["UserComm"]);
                        model.UserName = Convert.ToString(row["UserName"]);



                    }

                }
            }
            catch (Exception ex)
            {
                // log += "\r\n , excp=" + ex.Message;
                Common.LogException(ex, "GetRechargeDetail RecId=" + recid);
            }

            return model;

        }


        private void CheckStatus(Dto.RechargeDetail model, string remark, ref bool IsDownline, ref string apitxnid, ref string statusmsg, ref string optxnid, ref int statusId, ref string log)
        {
            log += "\r\n, CheckStatus- start";
            string status = "";


            #region "APi Call"

            RequestResponseDto requestResponse = new RequestResponseDto();
            ApiCall apiCall = new ApiCall(reqResService);

            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

            int unitid = model.AmountUnitId;
            int len = model.AmountLength;
            int reflen = model.RefLength;
            int isnumeric = model.IsNumericOnly;
            string randomkey = "";
            string refpadding = "";
            string apiamount = "";
            string ourref = "";
            string datetime = "";

            //  apiamount = unitid == 2 ? (Convert.ToDecimal(model.Amount)* 100).ToString("D" + len) : model.Amount.ToString();

            ourref = isnumeric == 1 ? model.TxnId.ToString() : model.OurRefTxnId;
            ourref = reflen > 0 ? (reflen < ourref.Length ? ourref.Remove(0, ourref.Length - reflen) : refpadding + ourref) : ourref;
            var url = string.Empty;
            var postdata = string.Empty;

            url = model.ApiUrl?.ReplaceURL(model.ApiUserId, model.ApiPassword, model.ApiOptional, model.CustomerNo, model.OpCode, model.CircleCode, apiamount, ourref, "", model.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, model.OptTxnId, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.UserId.ToString());
            postdata = !string.IsNullOrEmpty(model.PostData) ? model.PostData?.ReplaceURL(model.ApiUserId, model.ApiPassword, model.ApiOptional, model.CustomerNo, model.OpCode, model.CircleCode, apiamount, ourref, "", model.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.OurRefTxnId, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.UserId.ToString()) : "";

            string apires = string.Empty;
            requestResponse.RequestTxt = url + " DATA" + postdata;
            requestResponse.Remark = remark;
            requestResponse.UserId = model.UserId;
            requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);
            if (model.ApiId == 8)
            {
                apires = model.Method == "POST" ? apiCall.Post(url, postdata, model.ContentType, model.ResType, model.ApiId, model.ApiUserId, model.ApiPassword)
                                                                         : apiCall.Get(url, ref requestResponse, model.ApiId);
            }
            else if (model.ApiId == 9)
            {
                apires = model.Method == "POST" ? apiCall.Post2(url, postdata, model.OurRefTxnId, model.ResType, model.ApiId, model.ApiUserId, model.ApiPassword)
                                                         : apiCall.Get(url, ref requestResponse, model.ApiId);
            }
            else if (model.ApiId == 11)
            {
                apires = model.Method == "POST" ? apiCall.Post2(url, postdata)
                                                         : apiCall.Get(url, postdata);
            }
            else if (model.ApiId == 12)
            {
                apires = model.Method == "POST" ? apiCall.Post2(url, postdata)
                                                         : apiCall.Get(url + model.OurRefTxnId, ref requestResponse, SiteKey.UpiClientKey, SiteKey.UpiClientsecret);
            }
            else if (model.ApiId == 13)
            {
                apires = model.Method == "POST" ? apiCall.Post(url, postdata, ref log)
                                                         : apiCall.Get(url, postdata);
            }
            else if (model.ApiId == 14)
            {
                apires = model.Method == "POST" ? apiCall.Post(url, postdata, ref log)
                                                         : apiCall.GetBingPay(url, ref requestResponse);
            }
            else
            {
                apires = model.Method == "POST" ? apiCall.Post(url, postdata, model.ContentType, model.ResType, model.ApiId, model.ApiUserId, model.ApiPassword)
                                                                  : apiCall.Get(url, ref requestResponse, model.ApiId);
            }
            log += "\r\n, update req-response";
            requestResponse.ResponseText = apires;
            requestResponse.RecId = Convert.ToInt64(model.RecId);
            requestResponse.RefId = model.OurRefTxnId;
            requestResponse.Remark = remark;
            requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);
            #endregion

            FilterResponseModel fResp = new FilterResponseModel();
            FilterRespTagValue(model.ApiId, model.UrlId, model.ResType, apires, ref statusId, ref log, ref fResp);

            optxnid = fResp.OperatorTxnID;
            apitxnid = fResp.ApiTxnID;
            optxnid = fResp.OperatorTxnID;
            statusmsg = fResp.Message;
            statusId = fResp.StatusId;
            status = fResp.Status;

        }
        private void SendCallBack(long recid, ref string log)
        {
            //Dto.RechargeDetail recharge = GetRechargeDetail(recid, string.Empty);
            Dto.RechargeDetail recharge = GetDMTDetail(recid, string.Empty);

            int statusId = recharge.StatusId;
            // var recharge = rechargeService.GetRecharge(recid);
            log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")downline callback-start, ";
            if (string.IsNullOrEmpty(recharge.CallbackURL))
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ") callback not exists, ";
            }
            else
            {
                recharge.StatusId = statusId == 4 ? Convert.ToInt16(recharge.StatusId) : statusId;
                recharge.CallbackURL = recharge.CallbackURL.ReplaceURL(string.Empty, string.Empty, recharge.UserTxnId, recharge.CustomerNo, recharge.OptTxnId, recharge.CircleId.ToString(), recharge.Amount.ToString(), recharge.TxnId.ToString(), statusId.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.UserId.ToString());

                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")callback url, " + recharge.CallbackURL + " recid=" + recharge.RecId + ", ";
                RequestResponseDto requestResponse = new RequestResponseDto();
                requestResponse.RecId = recharge.RecId;
                requestResponse.UserId = recharge.UserId;
                requestResponse.RefId = recharge.OurRefTxnId;
                requestResponse.Remark = "downline";
                log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")callapi, ";
                ApiCall apiCall = new ApiCall(reqResService);
                apiCall.Get(recharge.CallbackURL, ref requestResponse);
                log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")resp= " + requestResponse.ResponseText;
                requestResponse.ResponseText = requestResponse.ResponseText + ", sent statusId=" + statusId;

                requestResponse = AddDMTUpdateReqRes(requestResponse, ref log);

                log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")resp saved";

            }
            log += "\r\n  (" + DateTime.Now.TimeOfDay.ToString() + ")downline callback-end, ";
        }

        private string FilterRespTagValue(int apiid, int UrlId, string resType, string apires, ref int statusId, ref string log, ref FilterResponseModel filterResponse)
        {
            if (apiid == 67000)
            {
                apires = apires.Remove(apires.IndexOf("BEGIN SIGNATURE"))?.ToPlainText();
            }
            // var tagvalues = tagValueService.GetTagValuesByUrlId(apiid, UrlId);
            var tagvalues = GetTagValueListOfApiUrl(apiid, UrlId);
            //  var tags = tagValueService.GetTagValuesByUrlId(apiid, apiurl.Id);
            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")ResType=" + resType + ",";

            if (resType.ToLower().Contains("split"))
            {
                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();
                    int index = tg.TagIndex - 1;
                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Replace(" ", string.Empty).Split(',').Where(x => x != string.Empty).ToList();
                    }
                    if (index >= 0)
                    {
                        log += "\r\n index=" + index + ",";
                        if (tg.TagId == TAGName.SUCCESS) //status-success
                        {
                            try
                            {
                                string sval = filterResponse.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = StatsCode.SUCCESS;
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
                                string sval = filterResponse.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = StatsCode.FAILED;
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
                                string sval = filterResponse.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = StatsCode.PROCESSING;
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
                                string sval = filterResponse.Status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                sval = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = 5;
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
                                filterResponse.ApiTxnID = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);


                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + filterResponse.ApiTxnID + ", ";
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
                                filterResponse.OperatorTxnID = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")optxnid=" + filterResponse.OperatorTxnID + ", ";
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
                                filterResponse.Message = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + filterResponse.Message + ", ";
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
                                filterResponse.RequestTxnId = sval.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + filterResponse.RequestTxnId + ", ";
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
                                filterResponse.Vendor_CL_Bal = Convert.ToDecimal(clbal);
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
                                filterResponse.Vendor_OP_Bal = Convert.ToDecimal(opbal);
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
                                filterResponse.LapuNo = lapuno.Length > 50 ? lapuno.Substring(0, 45) : lapuno;
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
                                filterResponse.Complaint_Id = cid.Length > 50 ? cid.Substring(0, 45) : cid;
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
                                filterResponse.R_Offer = Convert.ToDecimal(ro);

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.BILL_PRICE)
                        {

                            try
                            {
                                string ro = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                ro = ro.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer=" + ro + ", ";
                                ro = ro.Length > 50 ? ro.Substring(0, 45) : ro;
                                filterResponse.R_Offer = Convert.ToDecimal(ro);

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.CUSTOMER_NAME) //Customer Name
                        {

                            try
                            {
                                string CustomerName = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                CustomerName = CustomerName.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CustomerName=" + CustomerName + ", ";
                                filterResponse.LapuNo = CustomerName.Length > 50 ? CustomerName.Substring(0, 45) : CustomerName;
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno excp= " + ex.Message;
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
                        cmpList = tg.CompareTxt.Replace(" ", string.Empty).Split(',').Where(x => x != string.Empty).ToList();
                    }
                    if (tg.TagId == TAGName.SUCCESS) //status-success
                    {
                        try
                        {
                            string sval = filterResponse.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS=" + sval + ", ";

                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = StatsCode.SUCCESS;
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
                            string sval = filterResponse.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED=" + sval + ", ";
                            //string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = StatsCode.FAILED;
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
                            string sval = filterResponse.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING=" + sval + ", ";

                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = StatsCode.PROCESSING;
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
                            string sval = filterResponse.Status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING=" + sval + ", ";
                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = 5;
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

                            filterResponse.ApiTxnID = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + filterResponse.ApiTxnID + ", ";
                            filterResponse.ApiTxnID = filterResponse.ApiTxnID.Length > 50 ? filterResponse.ApiTxnID.Substring(0, 45) : filterResponse.ApiTxnID;
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
                            filterResponse.OperatorTxnID = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                            filterResponse.OperatorTxnID = HttpUtility.UrlDecode(filterResponse.OperatorTxnID);
                            filterResponse.OperatorTxnID = filterResponse.OperatorTxnID.Trim();

                            log += "\r\n optxnid=" + filterResponse.OperatorTxnID.Trim() + ", ";
                            filterResponse.OperatorTxnID = filterResponse.OperatorTxnID.Length > 50 ? filterResponse.OperatorTxnID.Substring(0, 45) : filterResponse.OperatorTxnID;
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
                            filterResponse.Message = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + filterResponse.Message + ", ";
                            filterResponse.Message = filterResponse.Message.Length > 199 ? filterResponse.Message.Substring(0, 198) : filterResponse.Message;
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
                            filterResponse.RequestTxnId = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + filterResponse.RequestTxnId + ", ";
                            filterResponse.RequestTxnId = filterResponse.RequestTxnId.Length > 50 ? filterResponse.RequestTxnId.Substring(0, 45) : filterResponse.RequestTxnId;
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
                            filterResponse.Vendor_CL_Bal = Convert.ToDecimal(clbal);
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
                            filterResponse.Vendor_OP_Bal = Convert.ToDecimal(opbal);
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
                            filterResponse.LapuNo = lapuno.Length > 50 ? lapuno.Substring(0, 45) : lapuno;
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
                            filterResponse.Complaint_Id = cid.Length > 50 ? cid.Substring(0, 45) : cid;
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
                            filterResponse.R_Offer = Convert.ToDecimal(ro);

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.CUSTOMER_NAME) //Customer Name
                    {
                        try
                        {
                            string CustomerName = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CustomerName=" + CustomerName + ", ";
                            filterResponse.CustomerName = CustomerName.Length > 50 ? CustomerName.Substring(0, 45) : CustomerName;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")CustomerName excp= " + ex.Message;
                            //LIBS.Common.LogException(ex);
                        }

                    }
                }
            }
            filterResponse.Message = "status=" + filterResponse.Status + " and msg=" + filterResponse.Message;
            return filterResponse.RequestTxnId;
        }
        #endregion

        #region Wallet Request Callback
        [Route("~/Service/WalletCallback")]
        [HttpGet]
        [HttpPost]
        public string WalletCallback(string TokenId, string BankRefId = "", string Amount = "", string CustomerName = "")
        {
            string res = "failed";
            string host = Request.RequestUri.AbsoluteUri.ToString();
            int userid = 0;
            string apires = HttpUtility.UrlDecode(host);
            RequestResponseDto requestResponse = new RequestResponseDto();
            requestResponse.RequestTxt = apires;
            requestResponse.Remark = "WalletCallBack";

            string log = " (" + DateTime.Now.TimeOfDay.ToString() + ")callback start-,WalletCallback=" + BankRefId + " Amount=" + Amount;
            try
            {
                int UserId = 0;
                User user = userService.GetUserByApiToken(TokenId);
                if (user != null)
                {
                    decimal Amt = Convert.ToDecimal(Amount);
                    WalletRequest walletRequest = walletService.GetPendingWalletRequestList(BankRefId, Amt, 5);
                    if (walletRequest != null && walletRequest?.BankAccountId != 2)
                    {
                        Dto.WalletRequestDto walletRequestDto = new Dto.WalletRequestDto();
                        walletRequestDto.Id = walletRequest.Id;
                        walletRequestDto.Comment = "Auto Approved Callback";
                        walletRequestDto.AddedById = 1;
                        userid = walletRequest.UserId ?? 0;
                        AutoApproveWalletRequestSMS(walletRequestDto, ref res);
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(CustomerName))
                        {
                            //int Error = UsernameCheck(CustomerName, ref UserId);
                            int Error = 0;
                            if (Error == 0)
                            {
                                if (walletService.IsChequeNoExists(BankRefId.Trim()))
                                {
                                    res = "Duplicate cheque/ref number!";
                                }
                                else
                                {
                                    Dto.WalletRequestDto walletRequestDto = new Dto.WalletRequestDto();
                                    walletRequestDto.Comment = "Auto Approved Callback With Name";
                                    walletRequestDto.AddedById = 1;
                                    walletRequestDto.UserId = UserId;
                                    walletRequestDto.PaymentRemark = "Auto Name Add";
                                    walletRequestDto.Chequeno = BankRefId;
                                    walletRequestDto.BankAccountId = 1;
                                    walletRequestDto.TrTypeId = 3;
                                    walletRequestDto.StatusId = 5;
                                    walletRequestDto.Amount = Convert.ToDecimal(Amount);
                                    AutoCallbackWalletRequestSMS(walletRequestDto, ref res);
                                }
                            }
                            else if (Error == 3)// No User
                            {
                                res = "No User Call Back";
                            }
                            else if (Error == 4)// Multi User
                            {
                                res = "Multi User Call Back";
                            }
                            else if (Error == 5)// No Record Found
                            {
                                res = "No Record Found Call Back";
                            }
                            else if (Error == 6)// Exception Error
                            {
                                res = "Exception Error Call Back";
                            }
                        }
                        else if (walletRequest != null && walletRequest?.BankAccountId == 2)
                        {
                            res = "Credit Bank Hit Call Back";
                        }
                        else
                        {
                            res = "failed";
                        }
                    }
                    //ApproveAllPendingRequests(5);
                    //res = "SUCCESS";
                }
                else
                {
                    res = "Authentication FAILED";
                }
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);
                res = "failed";
            }
            requestResponse.ResponseText = res;// "recharge-statusId=" + rcstatusid + ", callback statusId=" + statusId;
            if (userid > 0)
            {
                requestResponse.UserId = userid;
            }
            AddDMTUpdateReqRes(requestResponse, ref log);
            Common.LogActivity(log);
            log = string.Empty;
            return res;
        }
        private void ApproveAllPendingRequests(int userid)
        {
            try
            {
                string s = "";
                var reqList = walletService.GetWalletRequestList(5);
                foreach (var walletRequest in reqList)
                {
                    var ret = WalletRefrenceNO(walletRequest.Amount.ToString(), walletRequest.Chequeno, SiteKey.SMSDBWalletUserId);
                    if (ret == 0)
                    {
                        Dto.WalletRequestDto wrmodel = new Dto.WalletRequestDto();
                        wrmodel.Id = walletRequest.Id;
                        wrmodel.Comment = "Auto Approved CallBack";
                        wrmodel.AddedById = userid;
                        AutoApproveWalletRequestSMS(wrmodel, ref s);
                    }
                }
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex, "ApproveAllPendingRequests");
            }

        }
        private int WalletRefrenceNO(string amount, string txnrefno, string WalletUserID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SMSDBConn))
                {
                    using (SqlCommand cmd = new SqlCommand("AutoWalletMessageread", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.Parameters.AddWithValue("@refid", txnrefno);
                        cmd.Parameters.AddWithValue("@WalletUserID", WalletUserID);
                        cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        int error = Convert.ToInt32(cmd.Parameters["@result"].Value);
                        return error;
                    }
                }
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex, "Auto read walllet SMS txnrefno=" + txnrefno + ", WalletUserID" + WalletUserID + ", amount=" + amount);
                return 5;
            }
        }
        private int UsernameCheck(string Name, ref int UserId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                {
                    using (SqlCommand cmd = new SqlCommand("AddPaymentNameMatch", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.Add("@UserId", SqlDbType.Int);
                        cmd.Parameters["@UserId"].Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Error", SqlDbType.Int);
                        cmd.Parameters["@Error"].Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        int error = Convert.ToInt32(cmd.Parameters["@Error"].Value);
                        UserId = Convert.ToInt32(cmd.Parameters["@UserId"].Value);
                        return error;
                    }
                }
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex, "Auto read Name Table check Name =" + Name);
                return 6;
            }
        }
        private void AutoApproveWalletRequestSMS(Dto.WalletRequestDto model, ref string Response)
        {
            string log = "AutoApproveRequest start";
            string message = string.Empty;
            string error = string.Empty;
            try
            {
                WalletRequest walletRequest = model.Id > 0 ? walletService.GetWalletRequest(model.Id) : new WalletRequest();
                model.StatusId = model.Id == 0 ? Convert.ToByte(5) : model.StatusId;
                if (walletRequest.StatusId == 5)
                {
                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("sp_AddUserWallet", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IsPullOut", "Approve");
                        cmd.Parameters.AddWithValue("@UserId", walletRequest.UserId);
                        cmd.Parameters.AddWithValue("@Amount", walletRequest.Amount);
                        cmd.Parameters.AddWithValue("@Remark", model.Comment);
                        cmd.Parameters.AddWithValue("@AddedById", model.AddedById);
                        cmd.Parameters.AddWithValue("@IsCreditClear", model.IsClearCredit ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@TrTypeId", walletRequest.TrTypeId);
                        cmd.Parameters.AddWithValue("@BankAccountId", walletRequest.BankAccountId);
                        cmd.Parameters.AddWithValue("@ChequeNo", walletRequest.Chequeno);
                        cmd.Parameters.AddWithValue("@WRID", walletRequest.Id);
                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        error = Convert.ToString(cmd.Parameters["@error"].Value);
                        var splog = cmd.Parameters["@Log"].Value;
                        log += "\r\n, splog=" + splog;
                        if (error == "0")
                        {
                            Response = "SUCCESS";
                        }
                    }
                }
                else
                {
                    log += "\r\n, already status=" + walletRequest.StatusId;
                }
            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                //ShowErrorMessage("Error!", message, false);
                log += "\r\n, excp=" + Ex.Message;
                LIBS.Common.LogException(Ex);
            }
            log += "AutowalletUpdate-End";
            LIBS.Common.LogActivity(log);
        }
        private void AutoCallbackWalletRequestSMS(Dto.WalletRequestDto model, ref string Response)
        {
            string log = "AutoApproveRequest start";
            string message = string.Empty;
            string error = string.Empty;
            try
            {
                WalletRequest walletRequest = model.Id > 0 ? walletService.GetWalletRequest(model.Id) : new WalletRequest();
                model.StatusId = model.Id == 0 ? Convert.ToByte(5) : model.StatusId;
                if (model.StatusId == 5)
                {
                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("sp_AddUserWallet", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IsPullOut", "No");
                        cmd.Parameters.AddWithValue("@UserId", model.UserId);
                        cmd.Parameters.AddWithValue("@Amount", model.Amount);
                        cmd.Parameters.AddWithValue("@Remark", model.Comment);
                        cmd.Parameters.AddWithValue("@AddedById", model.AddedById);
                        cmd.Parameters.AddWithValue("@IsCreditClear", model.IsClearCredit ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@TrTypeId", model.TrTypeId);
                        cmd.Parameters.AddWithValue("@BankAccountId", model.BankAccountId);
                        cmd.Parameters.AddWithValue("@ChequeNo", model.Chequeno);
                        cmd.Parameters.AddWithValue("@WRID", walletRequest.Id);
                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        error = Convert.ToString(cmd.Parameters["@error"].Value);
                        var splog = cmd.Parameters["@Log"].Value;
                        log += "\r\n, splog=" + splog;
                        if (error == "0")
                        {
                            Response = "SUCCESS";
                        }
                    }
                }
                else
                {
                    log += "\r\n, already status=" + walletRequest.StatusId;
                }
            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                //ShowErrorMessage("Error!", message, false);
                log += "\r\n, excp=" + Ex.Message;
                LIBS.Common.LogException(Ex);
            }
            log += "AutowalletUpdate-End";
            LIBS.Common.LogActivity(log);
        }
        #endregion

        #region SP
        private bool IsValidateParameters(MoneyTransferModel model, ref string errCode, ref string errMsg)
        {
            bool Isvalid = false;
            errCode = ErrorCode.UNKNOWN_ERROR;
            errMsg = StatusMsg.UNKNOWN_ERROR;
            if (model.opid == "6")
            {
                if (string.IsNullOrEmpty(model.amount))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_AMOUNT;
                    errMsg = StatusMsg.INVALID_AMOUNT;
                }
                else if (string.IsNullOrEmpty(model.customerId))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_MOBILE;
                    errMsg = StatusMsg.INVALID_MOBILE;
                }
                else if (string.IsNullOrEmpty(model.Name))
                {
                    Isvalid = false;
                    errCode = "121";
                    errMsg = "provide Name";
                }
                //else if (string.IsNullOrEmpty(model.BankName))
                //{
                //    Isvalid = false;
                //    errCode = "120";
                //    errMsg = "Invalid Bank code";
                //}
                else if (string.IsNullOrEmpty(model.accountno))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_ACCOUNTNO;
                    errMsg = "provide accountNumber.";
                }
                else if (string.IsNullOrEmpty(model.ifsc))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_IFSCCODE;
                    errMsg = "provide IFSC.";
                }
                else if (string.IsNullOrEmpty(model.mobileno))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_MOBILE;
                    errMsg = StatusMsg.INVALID_MOBILE;
                }
                else if (string.IsNullOrEmpty(model.beneficiaryName))
                {
                    Isvalid = false;
                    errCode = "119";
                    errMsg = "provide accountHolderName.";
                }
                else if (string.IsNullOrEmpty(model.token))
                {
                    Isvalid = false;
                    errCode = "1";
                    errMsg = "provided authKey is wrong.";
                }
                else if (string.IsNullOrEmpty(model.reftxnid))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_REQUEST_ID;
                    errMsg = StatusMsg.INVALID_REQTXNID;
                }
                else
                {
                    Isvalid = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(model.amount))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_AMOUNT;
                    errMsg = StatusMsg.INVALID_AMOUNT;
                }
                else if (string.IsNullOrEmpty(model.token))
                {
                    Isvalid = false;
                    errCode = "1";
                    errMsg = "provided authKey is wrong.";
                }
                else if (string.IsNullOrEmpty(model.firstname))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_firstname;
                    errMsg = StatusMsg.INVALID_firstname;
                }
                else if (string.IsNullOrEmpty(model.lastname))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_lastname;
                    errMsg = StatusMsg.INVALID_lastname;
                }
                else if (string.IsNullOrEmpty(model.reftxnid))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_REQUEST_ID;
                    errMsg = StatusMsg.INVALID_REQTXNID;
                }
                else if (model?.transfermode?.ToUpper() != "NEFT" && model?.transfermode?.ToUpper() != "IMPS" && model?.transfermode?.ToUpper() != "RTGS")
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_TXNTYPE;
                    errMsg = StatusMsg.INVALID_TXNTYPE;
                }
                else if (model.reftxnid.Length > 65)
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_REQUEST_ID;
                    errMsg = StatusMsg.INVALID_REQUEST_ID;
                }
                else if (string.IsNullOrEmpty(model.accountno))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_ACCOUNTNO;
                    errMsg = "provide a valid accountNumber.";
                }
                else if (string.IsNullOrEmpty(model.ifsc))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_IFSCCODE;
                    errMsg = "provide a valid IFSC.";
                }
                else if (string.IsNullOrEmpty(model.transfermode))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_TRANSFERMODE;
                    errMsg = "provide a valid transferMode.";
                }
                else if (string.IsNullOrEmpty(model?.beneficiaryName))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_BENEFICIERNAME;
                    errMsg = "provide a valid accountHolderName.";
                }
                else if (string.IsNullOrEmpty(model.mobileno))
                {
                    Isvalid = false;
                    errCode = ErrorCode.INVALID_MOBILE;
                    errMsg = StatusMsg.INVALID_MOBILE;
                }
                else
                {
                    Isvalid = true;
                }

            }
            return Isvalid;
        }
        private void UpdateStatusWithDMTCheck(long RecId, int UserId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, string fsssion, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, string BeneName, string updatetype = "StatusWithCheck")
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateDMTStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                if (!string.IsNullOrWhiteSpace(apitxnid))
                    cmd.Parameters.AddWithValue("@ApiTxnId", apitxnid);

                if (!string.IsNullOrWhiteSpace(optxnid))
                    cmd.Parameters.AddWithValue("@OptTxnId", optxnid);

                if (!string.IsNullOrWhiteSpace(statusmsg))
                    cmd.Parameters.AddWithValue("@StatusMsg", statusmsg);

                if (!string.IsNullOrWhiteSpace(BeneName))
                    cmd.Parameters.AddWithValue("@BeneName", BeneName);

                if (!string.IsNullOrWhiteSpace(remark))
                    cmd.Parameters.AddWithValue("@Remark", remark);

                if (lapuid > 0)
                    cmd.Parameters.AddWithValue("@LapuId", lapuid);

                if (!string.IsNullOrWhiteSpace(lapuno))
                    cmd.Parameters.AddWithValue("@LapuNo", lapuno);
                if (!string.IsNullOrWhiteSpace(fsssion))
                    cmd.Parameters.AddWithValue("@FessionNo", fsssion);
                if (opid > 0)
                    cmd.Parameters.AddWithValue("@OpId", opid);

                cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsRefund", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsDownline", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log += "\r\n ,  before exec = usp_UpdateDMTStatus";
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                log += "\r\n ,  after exec = usp_UpdateDMTStatus";
                string Refund = Convert.ToString(cmd.Parameters["@IsRefund"].Value);
                string Downline = Convert.ToString(cmd.Parameters["@IsDownline"].Value);
                string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);

                IsRefund = !string.IsNullOrEmpty(Refund) ? Convert.ToBoolean(Refund) : false;
                IsDownline = !string.IsNullOrEmpty(Downline) ? Convert.ToBoolean(Downline) : false;

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;
            }
        }
        private List<Dto.TagValueDto> GetTagValueListOfApiUrl(int apiid, int urlid)
        {
            List<Dto.TagValueDto> tvlist = new List<Dto.TagValueDto>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetTagValueByUrl", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiId", apiid);
                    cmd.Parameters.AddWithValue("@UrlId", urlid);
                    cmd.Parameters.AddWithValue("@Remark", "All");


                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Dto.TagValueDto model = new Dto.TagValueDto();

                        model.UrlId = Convert.ToInt32(dt.Rows[i]["UrlId"]);
                        model.ApiId = Convert.ToInt32(dt.Rows[i]["ApiId"]);
                        model.TagId = Convert.ToInt32(dt.Rows[i]["TagId"]);
                        model.Name = Convert.ToString(dt.Rows[i]["Name"]);
                        model.PreTxt = Convert.ToString(dt.Rows[i]["PreTxt"]);
                        model.PostText = Convert.ToString(dt.Rows[i]["PostText"]);
                        model.PreMargin = Convert.ToInt32(dt.Rows[i]["PreMargin"]);
                        model.PostMargin = Convert.ToInt32(dt.Rows[i]["PostMargin"]);
                        model.TagMsg = Convert.ToString(dt.Rows[i]["TagMsg"]);
                        model.CompareTxt = Convert.ToString(dt.Rows[i]["CompareTxt"]);
                        model.ResSeparator = Convert.ToString(dt.Rows[i]["ResSeparator"]);
                        model.TagIndex = Convert.ToInt32(dt.Rows[i]["TagIndex"]);
                        tvlist.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }

            return tvlist;
        }
        private RequestResponseDto AddDMTUpdateReqRes(RequestResponseDto model, ref string log, string filter = "NA")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_UpdateDMTDetailToReqRes", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@UrlId", model.UrlId);
                    cmd.Parameters.AddWithValue("@RecId", model.RecId);
                    cmd.Parameters.AddWithValue("@RefId", model.RefId);
                    cmd.Parameters.AddWithValue("@Remark", model.Remark);
                    cmd.Parameters.AddWithValue("@RequestTxt", model.RequestTxt);
                    cmd.Parameters.AddWithValue("@ResponseText", model.ResponseText);
                    cmd.Parameters.AddWithValue("@CustomerNo", model.CustomerNo);
                    cmd.Parameters.AddWithValue("@OpId", model.OpId);
                    cmd.Parameters.AddWithValue("@UserTxnId", model.UserReqId);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
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
                Common.LogException(ex);
            }

            return model;

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
                    cmd.Parameters.AddWithValue("@CustomerNo", model.CustomerNo);
                    cmd.Parameters.AddWithValue("@OpId", model.OpId);
                    cmd.Parameters.AddWithValue("@UserTxnId", model.UserReqId);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
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
                Common.LogException(ex);
            }

            return model;

        }
        private UpiResponse UpiRequestSp(UpiRequestModel model, ref string response, ref string log, ref RequestResponseDto userReqRes)
        {
            UpiResponse upiResponse = new UpiResponse();
            try
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")(ApiToken=" + model.ApiToken + ",MobileNo=" + model.CustomerMobile + ",Amount=" + model.Amount + ",RefTxnId=" + model.ClientId + " IpAddress=" + model.IpAddress + ",OurTxnId=" + model.OurTxnId + "), ";

                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    //set sp parameters
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SP_UpiRequestValidation", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiToken", model.ApiToken);
                    cmd.Parameters.AddWithValue("@UserName", model.UserName);
                    cmd.Parameters.AddWithValue("@IPAddress", model.IpAddress);
                    cmd.Parameters.AddWithValue("@UserTxnId", model.OurTxnId);
                    cmd.Parameters.AddWithValue("@CustomerNo", model.CustomerMobile);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    log += "\r\n , before exec SP_UpiRequestValidation";
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n , after exec";
                    string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                    string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    string Log = Convert.ToString(cmd.Parameters["@Log"].Value);


                    log += "\r\n , error=" + error + ", errordesc=" + ErrorDesc;

                    if (error == ErrorCode.NO_ERROR) //0 -No error, 101 -default failed
                    {
                        // retreive sp parameter values
                        model.UserId = Convert.ToString(cmd.Parameters["@UserId"].Value);
                        userReqRes.UserId = !string.IsNullOrEmpty(model.UserId) ? Convert.ToInt32(model.UserId) : userReqRes.UserId;
                        UpiProcess(model, ref log, ref upiResponse, ref response);
                    }
                    else
                    {
                        log += "\r\n , error desc= " + ErrorDesc;
                        upiResponse.Error = error;
                        upiResponse.Status = StatsCode.FAILED.ToString();
                        upiResponse.Message = StatusMsg.RECHARGE_FAILED;
                        upiResponse.ClientId = model.ClientId;
                    }

                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp= " + ex.ToString();
                upiResponse.Error = ErrorCode.UNKNOWN_ERROR;
                upiResponse.Status = StatsCode.PROCESSING.ToString();
                upiResponse.Message = StatusMsg.RECHARGE_PROCESSING;
                upiResponse.ClientId = model.ClientId;
                Common.LogException(ex, "RechargeRequest log=" + log);
            }

            return upiResponse;
        }
        private void UpiProcess(UpiRequestModel model, ref string log, ref UpiResponse upiResponse, ref string response)
        {
            RequestResponseDto requestResponse = new RequestResponseDto();
            try
            {
                string Recid;
                int opcode = 3;

                if (Convert.ToInt32(model.UserId) > 0)
                    requestResponse.UserId = Convert.ToInt32(model.UserId);
                requestResponse.Remark = "Upi Request Api Call";
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";
                requestResponse.RefId = model.OurTxnId;
                requestResponse = AddUpdateReqRes(requestResponse, ref log);
                ApiCall apiCall = new ApiCall(reqResService);
                string postdata = "{\"amount\":\"" + model.Amount + "\",\"referenceId\":\"" + model.OurTxnId + "\",\"note\":\"\",\"customer_name\":\"" + model.CustomerName + "\",\"customer_email\":\"" + model.CustomerEamil + "\",\"customer_mobile\":\"" + model.CustomerMobile + "\",\"redirect_url\":\"" + model.RedirectUrl + "\"}";
                log += "\r\n , Api Call Postdata =" + postdata;
                response = apiCall.PostUpi("https://dashboard.xettle.net/v1/service/upi/dynamic/qr", postdata, SiteKey.UpiClientKey, SiteKey.UpiClientsecret, ref requestResponse);
                log += "\r\n , Api Response =" + response;
                requestResponse.Remark = "Payment Request Api Response";
                requestResponse = AddUpdateReqRes(requestResponse, ref log);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    dynamic res = JObject.Parse(response);
                    if (res.code == "0x0200" || res.statuscode == "TXN")
                    {
                        UpiUrlResponse data = new UpiUrlResponse
                        {
                            QrIntent = res.data?.qr_intent ?? "",
                            PaymentUrl = res.data?.payment_url ?? "",
                        };
                        upiResponse.Error = "0";
                        upiResponse.Status = "1";
                        upiResponse.ApiTxnId = res.data?.extTransactionId ?? "";
                        upiResponse.ClientId = model.ClientId;
                        upiResponse.Data = data;
                        upiResponse.Message = res.message ?? "SUCCESS";

                        using (SqlConnection connection = new SqlConnection(SiteKey.SqlConn))
                        {
                            SqlCommand sqlCommand = new SqlCommand("Sp_PayinTxnCreate", connection);
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.Parameters.AddWithValue("@UserId", int.Parse(model.UserId));
                            sqlCommand.Parameters.AddWithValue("@IpAddress", model.IpAddress);
                            sqlCommand.Parameters.AddWithValue("@CustomerNo", model.CustomerMobile);
                            sqlCommand.Parameters.AddWithValue("@Amount", model.Amount);
                            sqlCommand.Parameters.AddWithValue("@StatusId", 2);
                            sqlCommand.Parameters.AddWithValue("@StatusMessage", "PROCESSING");
                            sqlCommand.Parameters.AddWithValue("@OpId", opcode);
                            sqlCommand.Parameters.AddWithValue("@UserTxnId", model.ClientId);
                            sqlCommand.Parameters.AddWithValue("@OurTxnId", model.OurTxnId);
                            sqlCommand.Parameters.Add("@Rid", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                            connection.Open();
                            sqlCommand.ExecuteNonQuery();
                            connection.Close();
                            Recid = Convert.ToString(sqlCommand.Parameters["@Rid"].Value);
                        }

                    }
                    else if (res.code == "0x0204" || res.code == "0x0206")
                    {
                        upiResponse.Error = "2";
                        upiResponse.Status = "2";
                        upiResponse.ClientId = model.ClientId;
                        upiResponse.Message = res["message"]?.ToString(Formatting.None) ?? "PROCESSING";

                    }
                    else
                    {
                        upiResponse.Error = "3";
                        upiResponse.Status = "3";
                        upiResponse.ClientId = model.ClientId;
                        upiResponse.Message = res["message"]?.ToString(Formatting.None) ?? "FAILED";
                    }
                    //using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                    //{
                    //    SqlCommand Cmd = new SqlCommand("Sp_PayinTxnCreate", con);
                    //    Cmd.CommandType = CommandType.StoredProcedure;
                    //    Cmd.Parameters.AddWithValue("@qr_intent", upiResponse.Data.QrIntent ?? "0");
                    //    Cmd.Parameters.AddWithValue("@payment_url", upiResponse.Data.PaymentUrl ?? "0");
                    //    Cmd.Parameters.AddWithValue("@RecId", int.Parse(Recid));
                    //    Cmd.Parameters.Add("@Rid", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                    //    con.Open();
                    //    Cmd.ExecuteNonQuery();
                    //    con.Close();
                    //    Recid = Convert.ToString(Cmd.Parameters["@Rid"].Value);
                    //}
                }
            }
            catch (Exception exx)
            {
                var e = exx.InnerException;
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + "), excp: (set reqres) userid=" + model.UserId;
                LIBS.Common.LogException(exx);
            }
        }
        private JObject CheckRegistrationProsess(CheckRegistration checkRegistration, ref string log, string IpAddress, ref RequestResponseDto reqReseDto)
        {
            JObject responce = new JObject();
            try
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")(ApiToken=" + checkRegistration.ApiToken + ", IpAddress=" + IpAddress + ",UserName=" + checkRegistration.UserName + "), ";
                using (SqlConnection conn = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("SP_SenderRegistrationValidation", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiToken", checkRegistration.ApiToken);
                    cmd.Parameters.AddWithValue("@UserName", checkRegistration.UserName);
                    cmd.Parameters.AddWithValue("@IPAddress", IpAddress);
                    cmd.Parameters.AddWithValue("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    log += "\r\n,before exec SP_SenderRegistrationValidation";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    log += "\r\n, after exec";
                    string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                    string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    string Log = Convert.ToString(cmd.Parameters["@Log"].Value);
                    string userid = Convert.ToString(cmd.Parameters["@UserId"].Value);
                    log += "\r\n , error=" + error + ", errordesc=" + ErrorDesc;

                    if (error == ErrorCode.NO_ERROR)
                    {
                        var tt = string.Empty;
                        ApiCall apiCall = new ApiCall(reqResService);
                        reqReseDto.UserId = !string.IsNullOrEmpty(userid) ? Convert.ToInt32(userid) : reqReseDto.UserId;
                        var a = "mobileno";
                        var data = "{" + a + ":" + checkRegistration.Mobileno + "}";
                        string Url = "http://zpluscash.com/apis/v1/dmr?action=checkregistration&authKey=A92dQxWSkj49XCaj34jfkjSHsdeY_global&clientId=API_CLIENT107&userId=48&data=" + data;
                        tt = apiCall.Post(Url, ref log, ref reqReseDto);
                        dynamic dynamic = JObject.Parse(tt);
                        var DATA = dynamic.DATA.isregistered == false ? 0 : 1;
                        if (dynamic.STATUS == 1 || dynamic.STATUS == "1")
                        {
                            responce = JObject.FromObject(new
                            {
                                ERROR = 1,
                                Status = 1,
                                MESSAGE = DATA == 0 ? "Sorry, The Provided Mobile Number is Not Registered" : "Congratulations! Your mobile number is registered"
                            });
                        }
                        else
                        {
                            responce = JObject.FromObject(new
                            {
                                ERROR = 3,
                                Status = 3,
                                MESSAGE = dynamic.MESSAGE
                            });
                        }

                    }
                    else
                    {
                        log += "\r\n , error desc= " + ErrorDesc;
                        responce = JObject.FromObject(new
                        {
                            Error = 3,
                            Status = StatsCode.FAILED,
                            Message = StatusMsg.RECHARGE_FAILED
                        });
                    }
                }
            }
            catch (Exception ex)
            {

                log += "\r\n , excp= " + ex.ToString();
                responce = JObject.FromObject(new
                {
                    Error = ErrorCode.UNKNOWN_ERROR,
                    Status = StatsCode.FAILED,
                    Message = StatusMsg.RECHARGE_FAILED
                });
                Common.LogException(ex, "RegistrationRequest log=" + log);
            }
            return responce;
        }
        private JObject RegistrationKYCProsess(RegistrationKYC registrationKYC, ref string log, ref RequestResponseDto reqReseDto, string IpAddress)
        {
            JObject responce = new JObject();
            try
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")(ApiToken=" + registrationKYC.ApiToken + ", IpAddress=" + IpAddress + ",UserName=" + registrationKYC.UserName + "), ";
                using (SqlConnection conn = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("SP_SenderRegistrationValidation", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiToken", registrationKYC.ApiToken);
                    cmd.Parameters.AddWithValue("@UserName", registrationKYC.UserName);
                    cmd.Parameters.AddWithValue("@IPAddress", IpAddress);
                    cmd.Parameters.AddWithValue("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    log += "\r\n,before exec SP_SenderRegistrationValidation";
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    log += "\r\n, after exec";
                    string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                    string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    string Log = Convert.ToString(cmd.Parameters["@Log"].Value);
                    string userid = Convert.ToString(cmd.Parameters["@UserId"].Value);
                    log += "\r\n , error=" + error + ", errordesc=" + ErrorDesc;

                    if (error == ErrorCode.NO_ERROR)
                    {
                        var res = string.Empty;
                        ApiCall apiCall = new ApiCall(reqResService);
                        reqReseDto.UserId = !string.IsNullOrEmpty(userid) ? Convert.ToInt32(userid) : reqReseDto.UserId;
                        var Urldata = "{" + "firstname" + ":" + registrationKYC.FirstName + "," + "lastname" + ":" + registrationKYC.LastName + "," + "registeras" + ":" + "NONKYC" + "," + "mobileno" + ":" + registrationKYC.MobileNo + "}";
                        string Url = "http://zpluscash.com/apis/v1/dmr?action=senderregistration&authKey=A92dQxWSkj49XCaj34jfkjSHsdeY_global&clientId=API_CLIENT107&userId=48&data=" + Urldata;
                        res = apiCall.Post(Url, ref log, ref reqReseDto);
                        dynamic dynamic = JObject.Parse(res);

                        if (dynamic.STATUS == 1 || dynamic.STATUS == "1")
                        {
                            responce = JObject.FromObject(new
                            {
                                ERROR = 1,
                                Status = 1,
                                MESSAGE = "Registered Successfully!"

                            }); ;
                        }
                        else if (dynamic.STATUS == 4 || dynamic.STATUS == "4")
                        {
                            responce = JObject.FromObject(new
                            {
                                ERROR = 4,
                                Status = 4,
                                Otp = dynamic?.DATA?.otp,
                                MESSAGE = "Verification Pending!",
                            });
                        }
                        else
                        {
                            responce = JObject.FromObject(new
                            {
                                ERROR = 3,
                                Status = 3,
                                MESSAGE = dynamic.MESSAGE
                            });
                        }
                    }
                    else
                    {
                        log += "\r\n , error desc= " + ErrorDesc;
                        responce = JObject.FromObject(new
                        {
                            Error = 3,
                            Status = StatsCode.FAILED,
                            Message = StatusMsg.RECHARGE_FAILED
                        });
                    }
                }
            }
            catch (Exception ex)
            {

                log += "\r\n , excp= " + ex.ToString();
                responce = JObject.FromObject(new
                {
                    Error = ErrorCode.UNKNOWN_ERROR,
                    Status = StatsCode.FAILED,
                    Message = StatusMsg.RECHARGE_FAILED
                });
                Common.LogException(ex, "RegistrationRequest log=" + log);
            }
            return responce;
        }
        #endregion


        #region Resend Money Transfer
        [Route("~/Service/ResendDmt")]
        [HttpGet]
        [HttpPost]
        public string ResendRechargeRequest(string token = "", long recid = 0, int apiid = 0, int chk = 0, int uid = 0, int rsnd = 0)
        {
            string log = string.Empty;
            bool IsRefund = false;
            bool IsDownline = false;
            string spLog = "";
            string remark = "resend";
            int statusId = 0;
            string apitxnid = "NA";
            string optxnid = "NA";
            string statusmsg = "resend";
            Dto.RechargeDetail recharge = new Dto.RechargeDetail();
            recharge.RecId = recid;

            log += "\r\n, recid=" + recharge.RecId;
            try
            {
                recharge = GetDMTDetail(recharge.RecId, chk > 0 ? "status" : string.Empty);
                // testing ke leyi service se ifsc leya hai
                var data = reqDmtReportService.GetRecharge(recid);
                recharge.IFSCCode = data.IFSCCode;
                recharge.AmtTypeId = (byte)data.AmtTypeId;
                recharge.Mode = data.transfermode;
                chk = recharge.ApiName.ToLower().Contains("default processing") ? 0 : chk;

                recharge.ApiId = apiid > 0 ? apiid : recharge.ApiId;

                TimeSpan reqtime = DateTime.Now - Convert.ToDateTime(recharge.RequestTime);
                TimeSpan rsndtime = reqtime;

                if (!string.IsNullOrEmpty(recharge.ResendTime))
                    rsndtime = DateTime.Now - Convert.ToDateTime(recharge.ResendTime);

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") RequestTime=" + recharge.RequestTime + " reqtime.TotalMinutes=" + reqtime.TotalMinutes + " rsndtime.TotalMinutes=" + rsndtime.TotalMinutes + " resendtime=" + recharge.ResendTime + " resendcount=" + recharge.ResendCount + " ResendWaitTime=" + recharge.ResendWaitTime;


                if ((recharge.ResendCount < 3 && recharge.StatusId == 2) &&
                    (recharge.ResendCount == 0 || (recharge.ResendCount == 1 && reqtime.TotalMinutes > 2) || (recharge.ResendCount == 2 && reqtime.TotalMinutes > 2 && recharge.ResendById == uid)) &&
                    (statusId == 3 || recharge.ApiName.ToLower().Contains("default processing") || chk != 1))
                {

                    #region "APi Call"
                    ApiCall apiCall = new ApiCall(reqResService);

                    var resp = ResendRechargeProcess(recharge, uid);
                    #endregion

                    if (string.IsNullOrEmpty(resp?.ToString()))
                    {
                        log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")blank reponse, ";
                        statusId = 2;
                    }
                    else
                    {
                        dynamic jObj = resp;

                        if ((jObj.STATUS == "1" || jObj.STATUS == "3") && jObj.ERRORCODE == "0")
                        {
                            #region "Send Callback to User"
                            try
                            {
                                statusId = Convert.ToInt32(jObj.STATUS);

                                log += "\r\n, get user callback url ";

                                if (recharge.UserId > 0)
                                {
                                    SendCallBack(recharge.RecId, ref log);
                                }
                                else
                                {
                                    log += "\r\n, usernotfound";
                                }
                                log += "\r\n, callback sent to user ";
                            }
                            catch (Exception)
                            {

                                log += "\r\n , downline_callback_excp jObj.STATUS=" + Convert.ToString(jObj.STATUS) + " ,statusId=" + statusId;
                            }
                            #endregion

                        }
                    }

                }

            }
            catch (Exception ex)
            {

                Common.LogException(ex);
                log += "\r\n, excp(recid=" + recid + "): " + ex.Message;
            }
            Common.LogActivity(log);
            return "success";
        }

        public JObject ResendRechargeProcess(Dto.RechargeDetail recharge, int userid)
        {
            string log = "";
            JObject response = new JObject();
            try
            {
                var ipaddress = LIBS.Common.Fetch_UserIP();
                var request = HttpContext.Current.Request;

                RequestResponseDto userReqRes = new RequestResponseDto();
                userReqRes.RequestTxt = request.Url.AbsoluteUri;
                userReqRes.Remark = "Resend_R";
                userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);
                log += "\r\n url=" + request.Url.AbsoluteUri;
                MoneyTransferModel model = new MoneyTransferModel();
                int statusId = StatsCode.FAILED;

                log += "\r\n ,get-recharge,txn,user";

                long rcamt = Convert.ToInt64(recharge.Amount);

                model.userid = recharge.UserId.ToString();
                model.mobileno = recharge.CustomerNo;
                model.accountno = recharge.AccountNo;
                model.ifsc = recharge.IFSCCode;
                model.beneficiaryName = recharge.BeneficiaryName;
                model.transfermode = recharge.Mode;
                model.firstname = recharge.BeneficiaryName;
                model.lastname = recharge.BeneficiaryName;
                model.AmtTypeId = recharge.AmtTypeId;
                model.amount = (rcamt == recharge.Amount ? rcamt : recharge.Amount).ToString();
                model.opid = recharge.OpId.ToString();
                model.reftxnid = recharge.UserTxnId;

                model.ipaddress = Fetch_UserIP();
                model.ourtxnid = GetUniqueNumber();
                model.transfermode = recharge.Mode;
                userReqRes.UserReqId = recharge.UserId.ToString();
                userReqRes.CustomerNo = recharge.CustomerNo;
                userReqRes.UserReqId = recharge.UserTxnId;


                log += "\r\n , recid=" + recharge.RecId +
                            ", userid=" + recharge.UserId +
                            ", txnid=" + recharge.TxnId +
                            ", CustomerNo=" + recharge.CustomerNo +
                            ", UserReqId=" + recharge.UserTxnId +
                            ", OurRefTxnId=" + model.ourtxnid;
                log += "\r\n  ,set helper";
                RechargeHelperDto rech = new RechargeHelperDto();

                rech.SwitchId = 2;
                rech.DebitAmount = recharge.DB_Amt;
                rech.CommAmount = recharge.DB_Amt - recharge.Amount;
                rech.RecId = recharge.RecId;
                rech.TxnId = recharge.TxnId;

                rech.ApiRouteList.Add(new ApiPriorityDto { ApiId = recharge.ApiId, PriorityId = 1 });
                log += "\r\n  ,route set";
                if (!string.IsNullOrEmpty(recharge.ResendTime))
                {
                    statusId = StatsCode.FAILED;
                    log += "\r\n ,already resend=" + recharge.ResendTime;
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.RECHARGE_FAILED,
                        ERRORCODE = ErrorCode.DUPLICATE_RESEND_REQUEST,
                        HTTPCODE = HttpStatusCode.OK
                    });
                }
                else
                {
                    SetDMTUpdatedBy("Resend", recharge.RecId, userid, string.Empty, ref log);
                    response = MoneyTransferProcess(model, response, rech, ref userReqRes, ref log, ref statusId, true, path);

                }

                log += "\r\n -end";

                userReqRes.RefId = model.ourtxnid;
                userReqRes.ResponseText = response.ToString();
                userReqRes = AddDMTUpdateReqRes(userReqRes, ref log);
            }
            catch (Exception ex)
            {
                log += "\r\n excp=" + ex.Message;
                Common.LogException(ex);
            }

            Common.LogActivity(log);
            log = string.Empty;
            return response;
        }

        private void SetRechargeUpdatedBy(string filter, long recid, int userid, string remark, ref string log)
        {
            log += "\r\n , SetRechargeUpdatedBy, recid=" + recid + " userid=" + userid + " filter=" + filter + " remark=" + remark;
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_SetRechargeUpdatedBy", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FilterType", filter);
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@Remark", remark);


                    log += "\r\n ,  before execute = usp_SetRechargeUpdatedBy";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after execute";

                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp=" + ex.Message;
                Common.LogException(ex);
            }

        }

        #endregion


        #region Rec Status check
        [Route("~/Service/VendorStatusCheckRec")]
        [HttpGet]
        public string VendorStatusCheckRec(string token = "", string recid = "", int uid = 0)
        {
            string respStr = string.Empty;
            JObject response = new JObject();
            //StatusCheckForVendor
            string log = "start-VendorStatusCheck token=" + token + " recid=" + recid;
            Dto.RechargeDetail recharge = new Dto.RechargeDetail();
            recharge.RecId = !string.IsNullOrEmpty(recid) ? Convert.ToInt64(recid) : 0;

            if (string.IsNullOrEmpty(recid) || string.IsNullOrEmpty(token) || Fetch_UserIP() != SiteKey.DomainIPAddress)
                return "failed";
            try
            {
                log += "\r\n, (recid=" + recharge.RecId + ") GetRechargeDetail";
                recharge = GetRechargeDetail(recharge.RecId, "status");
                log += " (model.RecId=" + recharge.RecId + ", old model.statusId=" + recharge.StatusId + ") ourref=" + recharge.OurRefTxnId + " customerno=" + recharge.CustomerNo + ", apiId=" + recharge.ApiId + ", apiname=" + recharge.ApiName;

                int statusId = 0;
                string optxnid = "";
                string apitxnid = "";
                string statusmsg = "";
                string remark = "AutoStatusCheck";
                bool IsDownline = false;
                bool IsRefund = false;


                log += "\r\n, CheckStatus";
                CheckStatus(recharge, remark, ref IsDownline, ref apitxnid, ref statusmsg, ref optxnid, ref statusId, ref log);
                log += " (new statusId=" + statusId + ")";

                recharge = GetRechargeDetail(recharge.RecId, "status");

                TimeSpan reqtime = DateTime.Now - Convert.ToDateTime(recharge.RequestTime);
                TimeSpan rsndtime = reqtime;


                if (!string.IsNullOrEmpty(recharge.ResendTime))
                    rsndtime = DateTime.Now - Convert.ToDateTime(recharge.ResendTime);

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") RequestTime=" + recharge.RequestTime + " reqtime.TotalMinutes=" + reqtime.TotalMinutes + " rsndtime.TotalMinutes=" + rsndtime.TotalMinutes + " resendtime=" + recharge.ResendTime + " resendcount=" + recharge.ResendCount + " ResendWaitTime=" + recharge.ResendWaitTime;

                if ((uid == 0 && reqtime.TotalMinutes > 2 && recharge.ResendWaitTime > 0 && recharge.ResendWaitTime > recharge.WaitTime && reqtime.TotalMinutes > recharge.ResendWaitTime && reqtime.TotalMinutes > recharge.WaitTime && recharge.StatusId == 2 && statusId == 3 && recharge.ResendCount < 3) &&
                    ((recharge.ResendCount == 0) || ((recharge.ResendCount == 1 || recharge.ResendCount == 2) && rsndtime.TotalMinutes > 2)))
                {
                    RequestResponseDto requestResponse = new RequestResponseDto();
                    requestResponse.Remark = "AutoResend";
                    requestResponse.RecId = recharge.RecId;
                    requestResponse.UserId = recharge.UserId;
                    requestResponse.RefId = recharge.OurRefTxnId;
                    requestResponse.CustomerNo = recharge.CustomerNo;
                    requestResponse.UserReqId = recharge.UserTxnId;
                    requestResponse.RequestTxt = "rec-status=" + recharge.StatusId + " resp-statusid=" + statusId;

                    SetRechargeUpdatedBy("Resend", recharge.RecId, uid, string.Empty, ref log);

                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")AutoResend start, ";
                    RequestResponseDto userReqRes = new RequestResponseDto();

                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")recharge.TxnRemark=" + recharge.TxnRemark;

                    requestResponse.ResponseText = response.ToString();
                    requestResponse = AddUpdateReqRes(requestResponse, ref log);

                }
                else
                {
                    UpdateStatusWithCheck(recharge.RecId, uid, statusId, apitxnid, optxnid, string.Empty, remark, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, recharge.OpId, 0, 0);

                    apitxnid = string.IsNullOrEmpty(apitxnid) ? recharge.ApiTxnId : apitxnid;
                    optxnid = string.IsNullOrEmpty(optxnid) ? recharge.OptTxnId : optxnid;
                    statusmsg = string.IsNullOrEmpty(statusmsg) ? recharge.StatusMsg : statusmsg;
                }
                statusId = statusId == 4 || statusId == 0 ? 2 : statusId;

                if (statusId != 2)
                {
                    #region "Send Callback to User"
                    log += "\r\n, get user callback url ";

                    if (recharge.UserId > 0)
                    {
                        SendCallBack(recharge.RecId, ref log);
                    }
                    else
                    {
                        log += "\r\n, usernotfound";
                    }
                    log += "\r\n, callback sent to user ";
                    #endregion
                }

                respStr = "success";
            }
            catch (Exception ex)
            {
                respStr = "failed";
                Common.LogException(ex, "statuscheck recid=" + recharge.RecId);
                log += "\r\n, excp(recid=" + recharge.RecId + ") excp=" + ex.Message;

            }

            Common.LogActivity(log);

            return respStr = "success";
        }
        #endregion
    }


}