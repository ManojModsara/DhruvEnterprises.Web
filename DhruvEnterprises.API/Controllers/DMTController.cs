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
using System.Configuration;
using System.Text.Json;

namespace DhruvEnterprises.API.Controllers
{
    public class DMTController : ApiController
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
        SqlConnection con;
        #endregion

        #region "CONSTRUCTOR"

        public DMTController(IPackageService _packageService, IDMTReportService _reqDmtReportService, IUserService _userService, IRequestResponseService _reqResService, IApiService _apiService, IRechargeService _rechargeService, IWalletService _walletService)
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
        // GET: DMT

        #region Check sender and otp

        [Route("~/Service/SendOtp")]
        [HttpPost]
        public JObject CheckSenderandOtp(HttpRequestMessage request, FetchSenderDto model)
        {

            JObject response = new JObject();
            string log = " CheckSenderandOtp  start ";
            try
            {
                if (ModelState.IsValid)
                {

                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.MobileNo) || model.MobileNo.Length != 10)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_MOBILE,
                            ERRORCODE = ErrorCode.INVALID_MOBILE,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request CheckSenderProcess Process ";
                        model.TokenId = tokenId;
                        CheckSenderProcess(model, ref log, ref response);
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject CheckSenderProcess(FetchSenderDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Add_Fetch";

            userReqRes.CustomerNo = model.MobileNo;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 5);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);

                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.VENDOR_NOT_ACTIVE,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (string.IsNullOrEmpty(ApiUrl))
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {

                var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                ApiCall apiCall = new ApiCall(reqResService);
                string apires = string.Empty;
                log += " request Post data = " + postdata;

                apires = apiCall.Post(url, postdata , ref log);
                userReqRes.ResponseText = apires;
                log += "  Api Res " + apires;

                userReqRes = AddUpdateReqRes(userReqRes, ref log);
                dynamic res = JObject.Parse(apires);
                if (res.errorMsg == "SUCCESS")
                {
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.SUCCESS,
                        RESPONSE = apires,
                        ERRORCODE = ErrorCode.NO_ERROR,
                        HTTPCODE = HttpStatusCode.OK
                    });
                }
                else
                {
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        RESPONSE = apires,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion

        #region
        [Route("~/Service/FetchSender")]
        [HttpPost]
        public JObject FetchSenderDetails(HttpRequestMessage request, FetchSenderDto model)
        {

            JObject response = new JObject();
            string log = " FetchSender  start ";
            try
            {
                if (ModelState.IsValid)
                {

                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.MobileNo) || model.MobileNo.Length != 10)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_MOBILE,
                            ERRORCODE = ErrorCode.INVALID_MOBILE,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request FetchSender Process ";
                        model.TokenId = tokenId;
                        FetchSenderProcess(model, ref log, ref response);
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject FetchSenderProcess(FetchSenderDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Add_Fetch";

            userReqRes.CustomerNo = model.MobileNo;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 5);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);

                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.VENDOR_NOT_ACTIVE,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (string.IsNullOrEmpty(ApiUrl))
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {
                string apires = string.Empty;
                try
                {
                    var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                    var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                    userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                    ApiCall apiCall = new ApiCall(reqResService);

                    log += " request Post data = " + postdata;

                    apires = apiCall.Post(url, postdata , ref log);
                    userReqRes.ResponseText = apires;
                    log += "  Api Res " + apires;

                    userReqRes = AddUpdateReqRes(userReqRes, ref log);
                    if (string.IsNullOrEmpty(apires))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        dynamic res = JObject.Parse(apires);
                        if (res.errorMsg == "SUCCESS")
                        {
                            response = JObject.FromObject(new
                            {
                                STATUS = StatsCode.SUCCESS,
                                RESPONSE = apires,
                                ERRORCODE = ErrorCode.NO_ERROR,
                                HTTPCODE = HttpStatusCode.OK
                            });
                        }
                        else
                        {
                            response = JObject.FromObject(new
                            {
                                STATUS = StatsCode.FAILED,
                                RESPONSE = apires,
                                ERRORCODE = ErrorCode.INVALID_REQUEST,
                                HTTPCODE = HttpStatusCode.BadRequest
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogException(ex);
                }
            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion

        #region DMT API

        [Route("~/Service/AddSender")]
        [HttpPost]
        public JObject DMTAddSender(HttpRequestMessage request, AddSensderDto model)
        {
            JObject response = new JObject();
            string log = " Add Sender  start ";
            try
            {
                if (ModelState.IsValid)
                {
                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.MobileNo) || model.MobileNo.Length != 10)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_MOBILE,
                            ERRORCODE = ErrorCode.INVALID_MOBILE,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request AddSender Process ";
                        model.TokenId = tokenId;
                        AddSenderProcess(model, ref log, ref response);
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject AddSenderProcess(AddSensderDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Add_Sender";

            userReqRes.CustomerNo = model.MobileNo;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 1);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);

                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {
                string apires = string.Empty;
                try
                {
                    var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.Name, String.Empty, String.Empty, model.address, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                    var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.Name, model.address, model.dateOfBirth, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                    userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                    ApiCall apiCall = new ApiCall(reqResService);

                    log += " request Post data = " + postdata;

                    apires = apiCall.Post(url, postdata , ref log);
                    userReqRes.ResponseText = apires;
                    log += "  Api Res " + apires;

                    userReqRes = AddUpdateReqRes(userReqRes, ref log);
                    dynamic res = JObject.Parse(apires);
                    if (res.errorMsg == "SUCCESS")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.INVALID_REQUEST,
                            HTTPCODE = HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.SUCCESS,
                        RESPONSE = apires,
                        ERRORCODE = ErrorCode.NO_ERROR,
                        HTTPCODE = HttpStatusCode.OK
                    });
                }
            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion

        #region FETCH ALL RECIPIENTS 

        [Route("~/Service/FetchAllRecipients")]
        [HttpPost]
        public JObject FetchAllRecipients(HttpRequestMessage request, FetchSenderDto model)
        {

            JObject response = new JObject();
            string log = " FetchAllRecipients  start ";
            try
            {
                if (ModelState.IsValid)
                {

                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.MobileNo) || model.MobileNo.Length != 10)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_MOBILE,
                            ERRORCODE = ErrorCode.INVALID_MOBILE,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request FetchSender Process ";
                        model.TokenId = tokenId;
                        FetchAllRecipientsProcess(model, ref log, ref response);
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject FetchAllRecipientsProcess(FetchSenderDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Fetch_AllRecipients";

            userReqRes.CustomerNo = model.MobileNo;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 3);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);

                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.VENDOR_NOT_ACTIVE,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (string.IsNullOrEmpty(ApiUrl))
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {

                var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                ApiCall apiCall = new ApiCall(reqResService);
                string apires = string.Empty;
                log += " request Post data = " + postdata;

                apires = apiCall.Post(url, postdata , ref log);
                userReqRes.ResponseText = apires;
                log += "  Api Res " + apires;

                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                try
                {
                    if (string.IsNullOrEmpty(apires))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        dynamic res = JObject.Parse(apires);

                        List<Recipients> recipientsList = new List<Recipients>();
                        JsonDocument dmtServiceResponse = JsonDocument.Parse(apires);

                        JsonElement recipientList = dmtServiceResponse.RootElement.GetProperty("data").GetProperty("recipientList");

                        foreach (JsonElement recipients in recipientList.EnumerateArray())
                        {
                            Recipients recipient = new Recipients
                            {
                                RecipientId = recipients.GetProperty("recipientId").GetString(),
                                BankAccountNumber = recipients.GetProperty("udf1").GetString(),
                                BankName = recipients.GetProperty("bankName").GetString(),
                                MobileNumber = recipients.GetProperty("mobileNo").GetString(),
                                Ifsc = recipients.GetProperty("udf2").GetString(),
                                RecipientName = recipients.GetProperty("recipientName").GetString()
                            };
                            AddBeneficiaryList(recipient, model.TokenId);
                            recipientsList.Add(recipient);
                        }
                        if (res.errorMsg == "SUCCESS")
                        {
                            response = JObject.FromObject(new
                            {
                                STATUS = StatsCode.SUCCESS,
                                RESPONSE = apires,
                                ERRORCODE = ErrorCode.NO_ERROR,
                                HTTPCODE = HttpStatusCode.OK
                            });
                        }
                        else
                        {
                            response = JObject.FromObject(new
                            {
                                STATUS = StatsCode.FAILED,
                                RESPONSE = apires,
                                ERRORCODE = ErrorCode.INVALID_REQUEST,
                                HTTPCODE = HttpStatusCode.BadRequest
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogException(ex);
                }

            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion

        #region  ADD RECIPIENTS 

        [Route("~/Service/AddRecipient")]
        [HttpPost]
        public JObject AddRecipient(HttpRequestMessage request, AddBeneDto model)
        {

            JObject response = new JObject();
            string log = " AddRecipient  start ";
            try
            {
                if (ModelState.IsValid)
                {

                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.MobileNumber) || model.MobileNumber.Length != 10)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_MOBILE,
                            ERRORCODE = ErrorCode.INVALID_MOBILE,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request FetchSender Process ";
                        model.TokenId = tokenId;
                        AddRecipientProcess(model, ref log, ref response);
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject AddRecipientProcess(AddBeneDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Add_Recipient";

            userReqRes.CustomerNo = model.MobileNumber;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 6);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);

                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.VENDOR_NOT_ACTIVE,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (string.IsNullOrEmpty(ApiUrl))
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {
                string apires = string.Empty;
                try
                {
                    var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNumber, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                    var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.customerId, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.BankAccountNumber, model.Ifsc, model.MobileNumber, model.RecipientName, model.BankName, string.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                    userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                    ApiCall apiCall = new ApiCall(reqResService);

                    log += " request Post data = " + postdata;

                    apires = apiCall.Post(url, postdata , ref log);
                    userReqRes.ResponseText = apires;
                    log += "  Api Res " + apires;

                    userReqRes = AddUpdateReqRes(userReqRes, ref log);
                    dynamic res = JObject.Parse(apires);
                    if (res.errorMsg == "Success")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.INVALID_REQUEST,
                            HTTPCODE = HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.SUCCESS,
                        ERRORCODE = ErrorCode.NO_ERROR,
                        HTTPCODE = HttpStatusCode.OK,
                        RESPONSE = apires
                    });

                    Common.LogException(ex);
                    Common.LogActivity(log);
                }

            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion

        #region DELETE BENEFICERY/RECIPIENT

        [Route("~/Service/DeleteRecipient")]
        [HttpPost]
        public JObject DeleteRecipient(HttpRequestMessage request, DeleteRecipientDto model)
        {

            JObject response = new JObject();
            string log = " Delete Recipient  start ";
            try
            {
                if (ModelState.IsValid)
                {

                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.MobileNo) || model.MobileNo.Length != 10)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_MOBILE,
                            ERRORCODE = ErrorCode.INVALID_MOBILE,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request Delete Recipient Process ";
                        model.TokenId = tokenId;
                        DeleteRecipientProcess(model, ref log, ref response);
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject DeleteRecipientProcess(DeleteRecipientDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Delete_Rec";

            userReqRes.CustomerNo = model.MobileNo;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 7);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);

                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.VENDOR_NOT_ACTIVE,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (string.IsNullOrEmpty(ApiUrl))
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {

                var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, model.MobileNo, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, model.recipientId, String.Empty, String.Empty, String.Empty, string.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                ApiCall apiCall = new ApiCall(reqResService);
                string apires = string.Empty;
                log += " request Post data = " + postdata;

                apires = apiCall.Post(url, postdata , ref log);
                userReqRes.ResponseText = apires;
                log += "  Api Res " + apires;

                userReqRes = AddUpdateReqRes(userReqRes, ref log);
                if (string.IsNullOrEmpty(apires))
                {
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.SUCCESS,
                        RESPONSE = apires,
                        ERRORCODE = ErrorCode.NO_ERROR,
                        HTTPCODE = HttpStatusCode.OK
                    });
                }
                else
                {
                    dynamic res = JObject.Parse(apires);
                    if (res.errorMsg == "SUCCESS")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            RESPONSE = apires,
                            ERRORCODE = ErrorCode.INVALID_REQUEST,
                            HTTPCODE = HttpStatusCode.BadRequest
                        });
                    }
                }
            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion

        #region FETCH BANK LIST

        [Route("~/Service/FetchBankList")]
        [HttpPost]
        public JObject FetchBankList(HttpRequestMessage request, FetchBankListDto model)
        {

            JObject response = new JObject();
            string log = " Fetch BankList  start ";
            try
            {
                if (ModelState.IsValid)
                {
                    string tokenId = string.Empty;
                    string regMobileNo = string.Empty;
                    var res = GetHeaderValues(request, ref tokenId, ref regMobileNo, ref log);
                    var Data = CheckToken_RegmobileNo(tokenId, regMobileNo);
                    log += "  After Exec  Sp CheckToken_RegmobileNo = " + Data;

                    if (Data.Item2 == "No" || Data.Item2 == "NO")
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.AUTH_FAIL,
                            ERRORCODE = ErrorCode.AUTH_FAIL,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else if (string.IsNullOrEmpty(model.OpId))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.FAILED,
                            MESSAGE = StatusMsg.INVALID_OPERATOR,
                            ERRORCODE = ErrorCode.INVALID_OPERATOR,
                            HTTPCODE = HttpStatusCode.OK
                        });
                    }
                    else
                    {
                        log += " send Request Fetch BankList Process ";
                        //model.TokenId = tokenId;
                        // FetchBankListProcess(model, ref log, ref response);
                        #region Banklist
                        string Banklist = @"[{
      ""BankCode"": ""1229"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank of India"",
      ""Ifsc"": ""SBIN0013218"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1212"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Punjab National Bank"",
      ""Ifsc"": ""PUNB0038600"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1135"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of India"",
      ""Ifsc"": ""BKID0005590"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1134"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of Baroda"",
      ""Ifsc"": ""BARB0NAJDEL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1269"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Union Bank Of India"",
      ""Ifsc"": ""UBIN0534994"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1125"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Allahabad Bank"",
      ""Ifsc"": ""IDIB000N614"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1147"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Central Bank of India"",
      ""Ifsc"": ""CBIN0280658"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1268"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UCO Bank"",
      ""Ifsc"": ""UCBA0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1165"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ICICI Bank"",
      ""Ifsc"": ""ICIC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1595"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janaseva Sahakari Bank(Borivali) Ltd."",
      ""Ifsc"": ""JASB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1596"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Kodungallur Town Co-operative Bank Ltd"",
      ""Ifsc"": ""IBKL0269KTC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1597"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ellaquai Dehati Bank"",
      ""Ifsc"": ""SBIN0RRELGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1598"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ANGUL CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0ACCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1599"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ASKA CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""UTIB0SASKAC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1600"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BALASORE BHADRAK CENTRAL CO-OP BANK"",
      ""Ifsc"": ""YESB0BBCB00"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1601"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BANKI CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0BNKCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1602"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BERHAMPORE CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""IBKL0216BCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1603"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BHAWANIPATNA CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""ICIC00BHCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1604"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BOLANGIR CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0BDCB00"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1605"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BOUDH CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0BCCB00"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1606"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CUTTACK CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""IBKL0217C01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1607"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KEONJHAR CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""UTIB0SKCCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1608"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KHURDA CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0KHCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1609"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KORAPUT CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""UTIB0SKOCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1610"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MAYURBHANJ CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0MCCBHO"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1611"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NAYAGARH CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0NDB001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1612"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SAMBALPUR CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0SBPBHO"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1613"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SAMRUDDHI SAHKARI BANK(SPO-TJSB)"",
      ""Ifsc"": ""IBKL0041SCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1614"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SUNDARGARH CENTRAL CO-OP BANK LTD"",
      ""Ifsc"": ""YESB0SNGB13"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1615"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BHAVNAGAR DISTRICT CENTRAL COOPERATIVE BANK LIMITE"",
      ""Ifsc"": ""GSCB0BVN001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1616"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SABARKNTHA DCCB LTD(SPO-GSC)"",
      ""Ifsc"": ""GSCB0SKB001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1617"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UNITED PURI NIMAPARA CENTRAL CO-OP"",
      ""Ifsc"": ""YESB0UPNC01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1618"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bihar State Co-operative Bank Ltd"",
      ""Ifsc"": ""YESB0BSCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1619"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RAJARAMBAPU SAHAKARI BANK LTD"",
      ""Ifsc"": ""IBKL0116RBS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1620"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""District Co Operative Bank Ltd., Dehradun"",
      ""Ifsc"": ""YESB0DZSB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1621"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Godavari Urban Co-Op. Bank Ltd,Vazirabad"",
      ""Ifsc"": ""YESB0GUCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1622"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Junagadh Commercial Cooperative Bank LTD"",
      ""Ifsc"": ""HDFC0CJCCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1623"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Almora Zila Sahakari Bank Ltd."",
      ""Ifsc"": ""YESB0AZSB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1624"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bhagini Nivedita Sahakari Bank Ltd., Pune"",
      ""Ifsc"": ""HDFC0CBNBNK"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1625"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Unjha Nagarik Sahakari Bank Ltd., Unjha"",
      ""Ifsc"": ""GSCB0UUNJBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1626"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Chaitanya Godavari Grameena Bank"",
      ""Ifsc"": ""ANDB0CGGBHO"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1627"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""C.G. RAJYA SAHAKARI BANK MYDT. RAIPUR"",
      ""Ifsc"": ""CBIN0CGDCBN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1628"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Samata sahakari Bank Ltd"",
      ""Ifsc"": ""SRCB0SAM001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1629"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shrimant Malojiraje sahakari Bank Ltd., Phaltan"",
      ""Ifsc"": ""ICIC00SMSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1630"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pune merchants co-operative bank Ltd."",
      ""Ifsc"": ""IBKL0548PMC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1631"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rajkot District Central Cooperative Bank"",
      ""Ifsc"": ""GSCB0RJT001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1632"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sundarlal Sawaji URBAN Cooperative Bank Ltd"",
      ""Ifsc"": ""SRCB0SSB001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1633"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bhavana Rishi Co Op Urban Bank Ltd"",
      ""Ifsc"": ""YESB0BRCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1634"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sampada Sahakari Bank Ltd"",
      ""Ifsc"": ""IBKL0459SBS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1635"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE KARNAVATI CO-OPERATIVE BANK LTD."",
      ""Ifsc"": ""KKBK0TKCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1636"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Madhya Pradesh Rajya Sahakari Bank Maryadit"",
      ""Ifsc"": ""CBIN0MPABAA"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1637"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Yavatmal Urban Co-Op. Bank Ltd."",
      ""Ifsc"": ""IBKL0041Y01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1638"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Udupi town cooperative Bank ltd"",
      ""Ifsc"": ""UTIB0SUCTBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1639"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vallabh Vidyanagar Commercial Co-operative Bank Lt"",
      ""Ifsc"": ""HDFC0CVVCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1640"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Valmiki Urban Cooperative Bank, Pathri"",
      ""Ifsc"": ""UTIB0SVAUB1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1641"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vasavamba Urban Cooperative Bank Ltd"",
      ""Ifsc"": ""HDFC0CSVCBA"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1642"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Veraval Mercantile Co Operative Bank"",
      ""Ifsc"": ""HDFC0CVMCBA"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1270"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""United Bank Of India"",
      ""Ifsc"": ""PUNB0244200"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1132"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""United Bank Of India"",
      ""Ifsc"": ""PUNB0244200"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1646"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nagaland Rural Bank"",
      ""Ifsc"": ""SBIN0RRNLGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1647"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Balotra Urban Co-operative Bank Ltd"",
      ""Ifsc"": ""ICIC00BALUC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1648"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baroda Central Cooperative Bank"",
      ""Ifsc"": ""GSCB0BRD001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1649"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Abhinandan Urban Co.Op. Bank Ltd"",
      ""Ifsc"": ""HDFC0CMAN01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1650"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Latur Urban Coop Bank"",
      ""Ifsc"": ""IBKL0497LUC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1651"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bhadradri Cooperative Urban Bank Ltd"",
      ""Ifsc"": ""SRCB0BCB808"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1652"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bhatpara Naihati Co-operative Bank Ltd."",
      ""Ifsc"": ""WBSC0BUCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1653"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Belagavi Shree Basveshwar Co-op Bank Ltd"",
      ""Ifsc"": ""UTIB0S63SBC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1654"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bhilwara Urban Co-operative Bank Ltd"",
      ""Ifsc"": ""HDFC0CBHLUB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1655"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sumerpur Merchantile Urban Co-op. Bank Ltd."",
      ""Ifsc"": ""HDFC0CS1812"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1656"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Eenadu Coop Urban Bank Ltd."",
      ""Ifsc"": ""HDFC0CEENAD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1657"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Central Co-Operative Bank Ltd,Ara"",
      ""Ifsc"": ""IBKL0722CCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1658"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Darussalam Co-Operative Urban Bank Ltd."",
      ""Ifsc"": ""HDFC0CDUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1659"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""FINGROWTH CO-OPERATIVE BANK LTD."",
      ""Ifsc"": ""HDFC0CTUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1660"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Gadchirolli District Central Cooperative Bank"",
      ""Ifsc"": ""GDCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1661"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""IDUKKI DISTRICT CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""IDUK0000032"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1662"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Fincare Small Finance Bank Limited"",
      ""Ifsc"": ""FSFB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1663"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kakatiya Urban Cooperative Bank Ltd"",
      ""Ifsc"": ""UTIB0SKCUB1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1664"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SHREE WARANA SAHAKARI BANK LTD, WARANANAGAR"",
      ""Ifsc"": ""HDFC0CSWSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1665"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Khagaria District Central Co-Operative Bank Ltd"",
      ""Ifsc"": ""IBKL01077KD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1666"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kozhikode District Cooperative Bank"",
      ""Ifsc"": ""KDCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1667"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kranthi Urban Cooperative Bank Ltd"",
      ""Ifsc"": ""UTIB0SKRN01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1668"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""LAKHIMPUR URBAN CO-OPERATIVE BANK LTD., LAKHIMPUR-"",
      ""Ifsc"": ""ICIC00LKUCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1669"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Panchsheel Merc. CO. Op. Bank Ltd"",
      ""Ifsc"": ""YESB0PMCB02"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1670"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Adarniya PD Patil Bank"",
      ""Ifsc"": ""HDFC0CPDPBK"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1671"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""M S Co-Operative Bank Limited"",
      ""Ifsc"": ""IBKL0553MSC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1672"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Maharashtra State Cooperative Bank"",
      ""Ifsc"": ""MSCI0082002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1673"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mahaveer Cooperative Urban BANK ltd"",
      ""Ifsc"": ""HDFC0CMCUBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1674"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Manjeri Co-operative Urban Bank Ltd."",
      ""Ifsc"": ""ICIC00MCUBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1675"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NAINITAL DISTRICT CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""YESB0NDCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1676"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pali Urban Cooperative Bank Ltd"",
      ""Ifsc"": ""HDFC0CPUB03"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1677"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Deendayal Nagari Sahakari Bank Ltd"",
      ""Ifsc"": ""ICIC00DDNSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1678"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Peoples Urban Co-operative Bank Ltd"",
      ""Ifsc"": ""IBKL0341PUB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1679"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Prime Co-operative Bank Ltd."",
      ""Ifsc"": ""PMEC0103330"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1680"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Adinath Cooperative bank Ltd"",
      ""Ifsc"": ""HDFC0CSACBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1681"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sterling Urban Co-Operative Bank Ltd."",
      ""Ifsc"": ""HDFC0CSTUCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1682"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Textile Traders Co Operative Bank Ltd."",
      ""Ifsc"": ""HDFC0CTTCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1683"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Manorma Co-Op. Bank Ltd.,Solapur"",
      ""Ifsc"": ""HDFC0CMANCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1684"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The aurangabad district central co-operative bank "",
      ""Ifsc"": ""IBKL01192AC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1685"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Coastal Local Area Bank"",
      ""Ifsc"": ""MAHB000CB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1686"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Banaskantha District Central Co-operative Bank"",
      ""Ifsc"": ""GSCB0BKD001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1687"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Surat Mercantile Co Op Bank Ltd."",
      ""Ifsc"": ""YESB0SMCB05"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1688"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Chittorgarh Urban Co-Op Bank Ltd"",
      ""Ifsc"": ""HDFC0CCUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1689"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bhagyalaksmi Mahila Sahakari Bank Ltd  Nanded"",
      ""Ifsc"": ""HDFC0CBLMSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1690"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bhandara District Central Cooperative Bank Ltd"",
      ""Ifsc"": ""YESB0BHN001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1691"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bharat Co-Operative Bank Ltd"",
      ""Ifsc"": ""IBKL0008BCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1692"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bharuch District Central Cooperative Bank Ltd."",
      ""Ifsc"": ""GSCB0BRC001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1693"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Business Co-operative Bank"",
      ""Ifsc"": ""YESBOBCBL02"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1694"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sarvodaya Sahakari Bank Ltd"",
      ""Ifsc"": ""YESB0SSBL01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1695"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ambajogai Peoples Co-Op. Bank Ltd"",
      ""Ifsc"": ""HDFC0CAPCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1696"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Chandigarh State Cooperative Bank Ltd"",
      ""Ifsc"": ""UTIB0CSCB22"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1697"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mansing Co –op. Bank Ltd, Dudhondi"",
      ""Ifsc"": ""HDFC0CMCBLD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1698"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Gandevi People’s Co-Operative Bank Limited"",
      ""Ifsc"": ""IBKL0068GP1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1699"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Baroda City Co Op Bank Ltd"",
      ""Ifsc"": ""KKBK0BCCB04"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1700"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Janata Sahakari Bank Ltd.,Halol"",
      ""Ifsc"": ""GSCB0USJSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1701"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Jalgaon District Central Co-operative bank ltd"",
      ""Ifsc"": ""ICIC00JDCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1702"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Adarsh Mahila Nagari Sahakari Bank Ltd.,Aurangabad"",
      ""Ifsc"": ""YESB0AMSB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1703"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Kanakamahalakshmi Co-Operative Bank Ltd"",
      ""Ifsc"": ""IBKL0150KMC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1704"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Aman Sahakari Bank Ltd., Ichalkaranji"",
      ""Ifsc"": ""ICIC00AMSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1705"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Associate Co. Op. Bank Ltd"",
      ""Ifsc"": ""YESB0ASCB02"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1706"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Kanara District Central Cooperative Bank Ltd.,"",
      ""Ifsc"": ""KSCB0016001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1707"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The kangra District central cooperative Bank Ltd"",
      ""Ifsc"": ""KACE0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1708"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Kolhapur Urban Cooperative Bank Ltd., Kolhapur"",
      ""Ifsc"": ""HDFC0CKUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1709"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Emirates NBD Bank (P J S C)"",
      ""Ifsc"": ""EBIL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1710"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Madanapalle Cooperative Town bank Ltd"",
      ""Ifsc"": ""HDFC0CMPLTB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1711"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Mangalore Catholic Co-Operative Bank Ltd"",
      ""Ifsc"": ""IBKL0078MCC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1712"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Uttarkashi Zila Sahakari Bank Ltd., Uttarkashi"",
      ""Ifsc"": ""YESB0DCBU01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1713"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri  Anand Nagari Sahakari Bank Limited"",
      ""Ifsc"": ""YESB0SANB99"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1714"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Mannmandir Co-Operative Bank Ltd., Vita"",
      ""Ifsc"": ""ICIC0006405"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1715"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Meghalaya Cooperative Apex Bank Ltd."",
      ""Ifsc"": ""YESB0MCA000"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1716"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Mehsana District Central Co-operative Bank Ltd"",
      ""Ifsc"": ""GSCB0MSN001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1717"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Deola Merchant Coop Bank Ltd"",
      ""Ifsc"": ""IBKL0157001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1718"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Muslim Cooperative Bank ltd"",
      ""Ifsc"": ""HDFC0CMUSLM"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1719"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Nasik Merchants Co Operative Bank Ltd.,Nashik"",
      ""Ifsc"": ""NMCB0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1720"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Navnirman Co-Operative Bank Ltd"",
      ""Ifsc"": ""NVNM0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1721"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Nawada Central Co-operative Bank Ltd"",
      ""Ifsc"": ""YESBONCCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1722"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Nawanagar Cooperative Bank"",
      ""Ifsc"": ""IBKL0427NCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1723"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Ottapalam Co-operative Urban Bank Ltd"",
      ""Ifsc"": ""IBKL0763OCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1724"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Rohika Central Cooperative Bank Ltd, Madhubani"",
      ""Ifsc"": ""IBKL01066RC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1725"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Sangamner Merchants Co Operative Bank Ltd"",
      ""Ifsc"": ""HDFC0CTSMCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1726"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Khattri Co-op urban bank Ltd"",
      ""Ifsc"": ""YESB0KCUB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1727"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Patan Nagarik Sahakari Bank Ltd"",
      ""Ifsc"": ""GSCB0UPATAN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1728"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Nagar sahakari bank ltd. Gorakhpur"",
      ""Ifsc"": ""UTIB0NBGKP1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1729"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SARASPUR NAGRIK SAHAAKRI BANK"",
      ""Ifsc"": ""GSCB0USNCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1730"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nilambur Urban Co-op Bank Ltd"",
      ""Ifsc"": ""FDRL0NCUB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1731"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The SSK Co Operative Bank Ltd"",
      ""Ifsc"": ""UTIB0001811"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1732"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MALVIYA URBAN COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CMUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1733"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shree Panchganga  Nagari Sahakari Bank"",
      ""Ifsc"": ""IBKL0464PNS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1734"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE FINANCIAL COOP BANK LTD"",
      ""Ifsc"": ""YESB0FINCO2"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1735"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE MANIPUR STATE COOP BANK"",
      ""Ifsc"": ""YESB0MSCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1736"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KHEDA PEOPLES CO-OP BANK LTD"",
      ""Ifsc"": ""ICIC0002020"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1737"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MANVI PATTANA SOUHARDA SAHKARI BANK"",
      ""Ifsc"": ""HDFC0CMPSSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1738"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Urban Cooperative Bank Ltd.,Dharangaon"",
      ""Ifsc"": ""ICIC00TUCBD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1739"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Washim Urban Co-Operative Bank Ltd ; Washim"",
      ""Ifsc"": ""HDFC0CWUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1740"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Uttrakhand Co-operative Bank Ltd."",
      ""Ifsc"": ""HDFC0CUCOBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1741"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jana Small Finance Bank Ltd"",
      ""Ifsc"": ""JSFB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1742"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nirmal urban Co-op Bank, Nagpur"",
      ""Ifsc"": ""HDFC0CNB311"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1743"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SANDUR PATTANA SOUHARDA SAHAKARI BANK LTD"",
      ""Ifsc"": ""IBKL0776SPS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1744"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Ranuj Nagrik Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CRANUJ"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1745"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Uttarsanda People’s Co-Op. Bank Ltd"",
      ""Ifsc"": ""UTIB0SUPCB1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1746"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dr.Appashab urf Sa.Re.Patil Jasingpur Udgaon Sahak"",
      ""Ifsc"": ""ICIC00JUSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1747"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Sahakari Bank Ltd., Ajara"",
      ""Ifsc"": ""IBKL0116JSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1748"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kurla Nagrik Sahakari Bank Ltd"",
      ""Ifsc"": ""YESB0000419"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1749"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vikas  Souharda co-operative bank Ltd."",
      ""Ifsc"": ""ICIC00VSCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1750"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Wai Urban Cooperative Bank Ltd"",
      ""Ifsc"": ""SVCB0016101"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1751"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JALGAON JANATA SAHAKARI BANK LTD, JALGAON"",
      ""Ifsc"": ""JJSB0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1752"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janakalyan Sahakari Bank Ltd."",
      ""Ifsc"": ""JSBL0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1145"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Canara Bank"",
      ""Ifsc"": ""CNRB0002823"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1161"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""HDFC Bank"",
      ""Ifsc"": ""HDFC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1280"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""AXIS BANK"",
      ""Ifsc"": ""UTIB0000073"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1136"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of Maharashtra"",
      ""Ifsc"": ""MAHB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1168"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Indian Bank"",
      ""Ifsc"": ""IDIB000N120"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1169"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Indian Overseas Bank"",
      ""Ifsc"": ""IOBA0000255"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1170"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Indusind Bank"",
      ""Ifsc"": ""INDB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1171"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Integral Urban Coop Bank Ltd"",
      ""Ifsc"": ""HDFC0CIUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1172"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Irinjalakuda Town Co-operative Bank Ltd"",
      ""Ifsc"": ""HDFC0CITC01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1173"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""J&K Grameen Bank"",
      ""Ifsc"": ""JAKA0GRAMEN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1174"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jalna Merchant Cooperative Bank Ltd, Jalna"",
      ""Ifsc"": ""HDFC0CJMCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1175"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jalore Nagrik Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CJALOR"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1176"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jammu And Kashmir Bank"",
      ""Ifsc"": ""JAKA0MISHRI"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1177"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Co-operative Bank Ltd, Malegaon"",
      ""Ifsc"": ""HDFC0CJBMLG"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1178"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Sahakari Bank Ltd"",
      ""Ifsc"": ""JSBP0000057"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1179"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kallappanna Awade Ichalkaranji Janata Sahkari Bank"",
      ""Ifsc"": ""KAIJ0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1180"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karnatak vikas Gramin Bank"",
      ""Ifsc"": ""KVGB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1181"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karnataka Bank"",
      ""Ifsc"": ""KARB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1182"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karur Vysya Bank"",
      ""Ifsc"": ""KVBL0001101"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1183"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kaveri Grameena Bank"",
      ""Ifsc"": ""SBIN0RRCKGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1184"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kerala Gramin Bank"",
      ""Ifsc"": ""KLGB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1185"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kotak Mahindra Bank"",
      ""Ifsc"": ""KKBK0005033"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1186"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KOTTAYAM CO OPERATIVE URBAN BANK LTD"",
      ""Ifsc"": ""lBKL0027K01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1187"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Lakshmi Vilas Bank"",
      ""Ifsc"": ""LAVB0000661"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1188"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Langpi Dehangi Rural Bank"",
      ""Ifsc"": ""SBIN0RRLDGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1189"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Lokmangal Co-op Bank Ltd"",
      ""Ifsc"": ""IBKL0478LOK"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1190"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Madhyanchal Gramin Bank"",
      ""Ifsc"": ""SBIN0RRMBGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1191"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mahanagar Co-operative Bank"",
      ""Ifsc"": ""MCBL0960013"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1192"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Maharashtra Grameen Bank"",
      ""Ifsc"": ""MAHB0RRBMGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1193"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Maratha Co-operative Bank Ltd"",
      ""Ifsc"": ""IBKL0101MCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1194"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Meghalaya Rural Bank"",
      ""Ifsc"": ""SBIN0RRMEGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1195"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mehsana Urban Co-Operative Bank"",
      ""Ifsc"": ""MSNU0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1196"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mizoram Rural Bank"",
      ""Ifsc"": ""SBIN0RRMIGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1197"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NKGSB Co-operative Bank"",
      ""Ifsc"": ""NKGS0000088"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1198"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nutan Nagarik Sahakari Bank Ltd"",
      ""Ifsc"": ""NNSB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1199"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Odisha Gramya Bank"",
      ""Ifsc"": ""IOBA0ROGB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1200"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Oriental Bank Of Commerce"",
      ""Ifsc"": ""PUNB0172910"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1201"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pandharpur Merchant Co-operative Bank"",
      ""Ifsc"": ""ICIC00PMCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1202"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Parshwanath Co-operative Bank Ltd"",
      ""Ifsc"": ""HDFC0CPCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1203"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pavana Sahakari Bank Ltd-Pune"",
      ""Ifsc"": ""IBKL0087PSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1204"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Poornawadi Nagarik Sahakari Bank Maryadit"",
      ""Ifsc"": ""HDFC0CPNSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1205"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PRAGATHI KRISHNA GRAMIN BANK"",
      ""Ifsc"": ""PKGB0011063"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1206"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pragathi Krishna Gramin Bank"",
      ""Ifsc"": ""PKGB0010516"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1207"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Prathama Bank"",
      ""Ifsc"": ""PRTH0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1208"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pune Cantonment Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CPCSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1209"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pune Peoples Co-operative Bank Ltd"",
      ""Ifsc"": ""IBKL0548PPC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1211"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Punjab and Sind Bank"",
      ""Ifsc"": ""PSIB0000005"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1137"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Barclays Bank"",
      ""Ifsc"": ""BARC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1138"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baroda Gujarat Gramin Bank"",
      ""Ifsc"": ""BARB0BGGBXX"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1139"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baroda Rajasthan Kshetriya Gramin Bank"",
      ""Ifsc"": ""BARB0BRGBXX"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1140"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baroda Uttar Pradesh Gramin Bank"",
      ""Ifsc"": ""BARB0BUPGBX"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1141"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bassein Catholic Co-operative Bank Ltd"",
      ""Ifsc"": ""BACB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1142"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bharatiya Mahila Bank"",
      ""Ifsc"": ""BMBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1143"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BNP Paribas"",
      ""Ifsc"": ""BNPA0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1146"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Catholic Syrian Bank"",
      ""Ifsc"": ""CSBK0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1133"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bandhan Bank limited"",
      ""Ifsc"": ""BDBL0001014"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1126"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""AMBARNATH JAI HIND COOP BANK LTD"",
      ""Ifsc"": ""ICIC00AJHCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1127"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Andhra Bank"",
      ""Ifsc"": ""ANDB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1128"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Andhra Pradesh Grameena Vikas Bank"",
      ""Ifsc"": ""SBIN0RRAPGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1129"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Andhra Pragathi Grameena Bank"",
      ""Ifsc"": ""APGB0005059"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1130"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Apna Sahakari Bank Ltd"",
      ""Ifsc"": ""ASBL0000056"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1131"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Arunachal Pradesh Rural Bank"",
      ""Ifsc"": ""SBIN0RRARGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1102"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Aryavat Gramin Bank"",
      ""Ifsc"": ""BKID0ARYAGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1103"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baitarani Gramya Bank"",
      ""Ifsc"": ""BKID0BAITGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1104"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BANGIYA GRAMIN VIKASH BANK"",
      ""Ifsc"": ""UTBI0RRBBGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1105"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bihar kshetriya Gramin Bank"",
      ""Ifsc"": ""UCBA0RRBBKG"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1106"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Development Bank of Singapore"",
      ""Ifsc"": ""DBSS0IN0820"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1107"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Development Bank of Singapore"",
      ""Ifsc"": ""DBSS0000005"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1108"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ing Vysya Bank"",
      ""Ifsc"": ""VYSA0000005"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1109"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janaseva Sahakari Bank Ltd"",
      ""Ifsc"": ""JANA0000029"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1110"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janaseva Sahakari Bank Ltd"",
      ""Ifsc"": ""JANA0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1111"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kalinga Gramya Bank"",
      ""Ifsc"": ""UCBA0RRBKGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1112"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kalyan Janata Sahakari Bank"",
      ""Ifsc"": ""KJSB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1113"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Madhya Bihar Gramin Bank"",
      ""Ifsc"": ""PUNB0MBGB06"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1114"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PASCHIM BANGA GRAMIN BANK"",
      ""Ifsc"": ""UCBA0RRBPBG"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1115"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Samastipur kshetriya Gramin Bank"",
      ""Ifsc"": ""SBIN0RRSMGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1116"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SHIVALIK MERCANTILE CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""SMCB0001002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1117"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE AKOLA DISTRICT CENTRAL COOPERATIVE BANK"",
      ""Ifsc"": ""ADCC0000005"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1118"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SINDHUDURG DISTRICT CENTRAL COOP BANK"",
      ""Ifsc"": ""SIDC0001067"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1119"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE WEST BENGAL STATE COOPERATIVE BANK"",
      ""Ifsc"": ""WBSC0000023"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1120"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Uttar Bihar Gramin Bank"",
      ""Ifsc"": ""CBIN0R10001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1122"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Abhyudaya Co-operative Bank"",
      ""Ifsc"": ""ABHY0065012"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1123"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ahmedabad District Cooperative Bank"",
      ""Ifsc"": ""GSCB0ADC001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1124"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ahmednagar Merchant Co-op Bank Ltd"",
      ""Ifsc"": ""HDFC0CMBANK"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1271"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Utkal Grameen Bank"",
      ""Ifsc"": ""SBIN0RRUKGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1272"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UTTARAKHAND GRAMIN BANK"",
      ""Ifsc"": ""SBIN0RRUTGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1273"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vananchal Gramin Bank"",
      ""Ifsc"": ""SBIN0RRVCGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1274"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vijaya Bank"",
      ""Ifsc"": ""BARB0VJNOID"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1275"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Yes Bank"",
      ""Ifsc"": ""YESB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1276"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kashi Gomti Samyut Grameen Bank"",
      ""Ifsc"": ""UBIN0RRBKGS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1277"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Purvanchal Bank"",
      ""Ifsc"": ""SBIN0RRPUGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1278"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PRIMECOOPERATIVEBANKLTD"",
      ""Ifsc"": ""PMEC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1279"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JHARKHAND GRAMIN BANK"",
      ""Ifsc"": ""BKID0JHARGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1148"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Chhattisgarh Rajya Gramin Bank"",
      ""Ifsc"": ""SBIN0RRCHGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1149"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Citi Bank"",
      ""Ifsc"": ""CITI0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1150"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""City Union Bank"",
      ""Ifsc"": ""CIUB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1151"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Corporation Bank"",
      ""Ifsc"": ""CORP0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1152"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Cosmos Bank"",
      ""Ifsc"": ""COSB0000108"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1153"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DBS BANK LTD"",
      ""Ifsc"": ""DBSS0IN0811"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1154"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DCB Bank Ltd"",
      ""Ifsc"": ""DCBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1155"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dena Bank"",
      ""Ifsc"": ""BARB0DBGREA"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1156"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dhanalakshmi Bank"",
      ""Ifsc"": ""DLXB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1157"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dombivli Nagarik Sahakari Bank"",
      ""Ifsc"": ""DNSB0000050"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1158"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dr Annasaheb Chougule Urban Co-op Bank Ltd"",
      ""Ifsc"": ""HDFC0CDACUB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1159"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Federal Bank"",
      ""Ifsc"": ""FDRL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1160"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Gopinath Patil Parsik Janata Sahakari Bank Ltd"",
      ""Ifsc"": ""PJSB0000016"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1213"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Raipur Urban Merchantile Co-operative Bank Ltd"",
      ""Ifsc"": ""HDFC0CTRUMC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1214"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rajapur Urban Co-op Bank Ltd"",
      ""Ifsc"": ""ICIC00RUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1216"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rajgurunagar Sahakari Bank Ltd"",
      ""Ifsc"": ""RSBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1217"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RBL Bank Limited"",
      ""Ifsc"": ""RATN0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1218"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Saraswat Co-Operative Bank"",
      ""Ifsc"": ""SRCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1219"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Saurashtra Gramin Bank"",
      ""Ifsc"": ""SBIN0RRSRGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1220"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shivajirao Bhosale Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CSBB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1221"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Arihant Co-operative Bank Ltd"",
      ""Ifsc"": ""ICIC00ARIHT"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1222"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Basaveshwar Sahakari Bank Niyamit, Bagalkot"",
      ""Ifsc"": ""ICIC00SBSBN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1223"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Mahalaxmi Co-operative Bank"",
      ""Ifsc"": ""IBKL0116MCO"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1224"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Veershaiv Co-operative Bank Ltd"",
      ""Ifsc"": ""HDFC0CVCB02"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1225"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""South Indian Bank"",
      ""Ifsc"": ""SIBL0000153"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1226"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Standard Chartered Bank"",
      ""Ifsc"": ""SCBL0036024"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1227"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank Of Bikaner & Jaipur"",
      ""Ifsc"": ""SBIN0000006"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1228"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank of Hyderabad"",
      ""Ifsc"": ""SBIN0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1230"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank of Mysore"",
      ""Ifsc"": ""SBIN0000003"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1231"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank of Patiala"",
      ""Ifsc"": ""SBIN0000004"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1232"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank of Travancore"",
      ""Ifsc"": ""SBIN0000005"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1233"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SUCO Souharda Sahakari Bank"",
      ""Ifsc"": ""HDFC0CSUCOB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1234"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sudha Cooperative Urban Bank Ltd"",
      ""Ifsc"": ""HDFC0SUCUB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1235"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sutlej Gramin Bank"",
      ""Ifsc"": ""PSIB0SGB002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1236"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Suvarnayug Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CSUVRN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1237"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SVC Co-op Bank Limited"",
      ""Ifsc"": ""SVCB0000251"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1238"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Syndicate Bank"",
      ""Ifsc"": ""CNRB0019938"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1239"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Tamilnaad Mercantile Bank"",
      ""Ifsc"": ""TMBL0000223"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1240"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Telangana Grameena bank"",
      ""Ifsc"": ""SBHY0RRDCGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1241"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Thane Bharat Sahakari Bank Ltd"",
      ""Ifsc"": ""TBSB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1242"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Adarsh Urban Co-op Bank Ltd, Hyderabad"",
      ""Ifsc"": ""ICIC00ADRSH"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1243"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The AP Mahesh Co-operative Urban Bank Ltd"",
      ""Ifsc"": ""APMC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1244"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bharat Co-operative Bank (Mumbai) Ltd"",
      ""Ifsc"": ""BCBM0000075"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1245"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Chikhli Urban Co-op Bank Ltd"",
      ""Ifsc"": ""HDFC0CCUB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1246"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Gayatri Co-Operative Urban Bank Ltd"",
      ""Ifsc"": ""HDFC0CTGCUB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1247"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Gopalganj Central Gramin Bank"",
      ""Ifsc"": ""IKBL0000005"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1248"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Greater Bombay Co-operative Bank"",
      ""Ifsc"": ""GBCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1249"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Gujarat State Co-op Bank Ltd"",
      ""Ifsc"": ""GSCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1250"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Hasti Co-op Bank Ltd"",
      ""Ifsc"": ""HCBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1251"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Jalgaon Peoples Co-op Bank Ltd"",
      ""Ifsc"": ""JPCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1252"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Kalupur Commercial Co-Op Bank Ltd"",
      ""Ifsc"": ""KCCB0ISP019"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1253"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Mayani Urban Co-operative Bank Ltd"",
      ""Ifsc"": ""ICIC00TMUCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1254"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Municipal Co-operative Bank Ltd"",
      ""Ifsc"": ""MUBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1255"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Nainital Bank Ltd"",
      ""Ifsc"": ""NTBL0DEL038"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1256"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The National Co-Operative Bank Ltd"",
      ""Ifsc"": ""YESB0NBL002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1257"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Pandharpur Urban Co-op Bank Ltd"",
      ""Ifsc"": ""PUCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1258"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Pochampally Co-operative Urban bank Ltd"",
      ""Ifsc"": ""HDFC0CPCUBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1259"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Shirpur Peoples Co-Op Bank Ltd"",
      ""Ifsc"": ""KKBK0SPCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1260"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Surat District Co-op Bank Ltd"",
      ""Ifsc"": ""SDCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1261"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The SUTEX Co-operative Bank Ltd"",
      ""Ifsc"": ""SUTB0248001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1262"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Varachha Co-op Bank Ltd"",
      ""Ifsc"": ""VARA0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1263"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Vasai Vikas Co-op Bank Ltd"",
      ""Ifsc"": ""VVSB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1264"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Vishweshwar Sahakari Bank Ltd"",
      ""Ifsc"": ""VSBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1265"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Yadagiri Lakshmi Narasimha Swamy Co-Op Urban B"",
      ""Ifsc"": ""YESB0YLNS01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1266"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Thrissur District Co-operative Bank"",
      ""Ifsc"": ""IBKL0269TDC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1267"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""TJSB Sahakari Bank Ltd"",
      ""Ifsc"": ""TJSB0000042"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1163"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""HSBC Bank"",
      ""Ifsc"": ""HSBC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1164"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Hutatma Sahakari Bank Ltd"",
      ""Ifsc"": ""ICIC00HSBLW"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1166"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""IDBI Bank"",
      ""Ifsc"": ""IBKL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1167"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""IDFC bank"",
      ""Ifsc"": ""IDFB0041204"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1643"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Utkarsh Small Finance Bank"",
      ""Ifsc"": ""UTKS0001514"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1645"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""AKOLA MERCHANT COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CAMBLA"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1281"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SARVA UP GRAMIN BANK"",
      ""Ifsc"": ""PUNB0SUPGB5"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1282"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SANGLI URBAN CO OPERATIVE BANK LIMITED"",
      ""Ifsc"": ""HDFC0CSUCBL"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1283"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""FINO PAYMENTS BANK"",
      ""Ifsc"": ""FINO0001089"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1284"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PAYTM PAYMENTS BANK"",
      ""Ifsc"": ""PYTM0123456"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1285"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""AIRTEL PAYMENTS BANK"",
      ""Ifsc"": ""AIRP0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1286"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ASSAM GRAMIN VIKASH BANK"",
      ""Ifsc"": ""UTBI0RRBAGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1287"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SATARA DISTRICT CENTRAL CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""SDCE0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1288"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MUMBAI DISTRICT CENTRAL CO OP BANK LTD"",
      ""Ifsc"": ""MDCB0680001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1289"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE CHEMBUR NAGARIK SAHAKARI BANK"",
      ""Ifsc"": ""SRCB0CNS006"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1290"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DEUTSCHE BANK"",
      ""Ifsc"": ""DEUT0797BGL"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1291"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SARVA HARYANA GRAMIN BANK"",
      ""Ifsc"": ""PUNB0HGB001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1292"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SHUSHRUTI SOUAHRDA SAHAKRA BANK"",
      ""Ifsc"": ""HDFC0CSSSBN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1293"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SIWAN CNETRAL CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""IBKL01076SB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1294"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UTTAR BANGA KSHETRIYA GRAMIN BANK"",
      ""Ifsc"": ""CBIN0R40012"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1295"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RAJASTHAN MARUDHARA GRAMIN BANK"",
      ""Ifsc"": ""RMGB0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1296"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SATPURA NARMADA KSHETRIYA GRAMIN BANK"",
      ""Ifsc"": ""CBIN0R20002"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1297"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DIST COOP BANK LTD LAKHIMPUR KHIRI"",
      ""Ifsc"": ""ICIC00KHIRI"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1298"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RAE BARELI DISTRICT COOPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00RBDCB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1299"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""VIDHARBHA KONKAN GRAMIN BANK HO NAGPUR"",
      ""Ifsc"": ""BKID0WAINGB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1300"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RAMPUR ZILA SAHAKARI BANK LTD"",
      ""Ifsc"": ""ICIC00RAMPR"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1301"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SINDHUDURG DIST CENT COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CSINDC"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1302"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SHAHJAHANPUR DISTRICT CENTRAL CO OPERATIVE BANK LT"",
      ""Ifsc"": ""ICIC00SJDCB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1303"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ETAH DISTRICT COOPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00ETDCB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1304"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ETAWAH DISTRICT CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00ETAWH"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1305"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""VASAI JANATA SAHKARI BANK LTD"",
      ""Ifsc"": ""HDFC0CVJSBL"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1306"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE DISTRICT COOPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00PRDCB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1307"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE DISTRICT COOPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00UDCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1308"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DISTRICT CO OPERATIVE BANK"",
      ""Ifsc"": ""ICIC00MORAD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1309"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""TEHRI GARHWAL ZILA SAHKARI BANK LIMITED"",
      ""Ifsc"": ""IBKL0070T32"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1310"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE NEW URBAN CO OP BANK LTD"",
      ""Ifsc"": ""HDFC0CNUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1311"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE DISTRICT COOPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00FDCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1312"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RATNAGIRI DISTRICT CENTRAL CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SRDCC1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1313"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PUNE DISTRICT CENTRAL CO OP BANK"",
      ""Ifsc"": ""HDFC0CPDCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1314"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KRISHNA GRAMEENA BANK"",
      ""Ifsc"": ""SBIN0RRKRGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1315"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ZILA SAHKARI BANK LTD"",
      ""Ifsc"": ""ICIC00BULND"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1316"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MALAD SAHKARI BANK LTD"",
      ""Ifsc"": ""HDFC0CMALAD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1317"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE HP STATE CO OP BANK BO TUNDI"",
      ""Ifsc"": ""YESB0HPB193"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1318"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JAMIA COOPERATIVE BANK LIMITED"",
      ""Ifsc"": ""UTIB0SJCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1319"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PRAGATHI GRAMIN BANK"",
      ""Ifsc"": ""CNRB000PGB1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1320"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BICHOLIUM URBAN COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CBUCBG"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1321"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BIHAR AWAMI COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CBACOB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1322"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UDHAM SINGH NAGAR DISTRICT CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00USNDC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1323"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MAHESH SAHAKARI BANK LTD"",
      ""Ifsc"": ""SRCB0MSBLPN"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1324"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JAIPUR THAR GRAMIN BANK"",
      ""Ifsc"": ""UCBA0RRBJTG"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1325"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE HOSHIARPUR CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SHSP01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1326"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE JANATA CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""HDFC0CTJCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1327"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MODEL CO OP BANK LTD"",
      ""Ifsc"": ""HDFC0CMODEL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1328"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE VSV CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""HDFC0CVSVCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1329"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BOMBAY MERCANTILE CO OP BANK"",
      ""Ifsc"": ""HDFC0CBMC14"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1330"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE PURNIA DISTRICT CENTRAL CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00PURDC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1331"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BURDWAN CENTRAL COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CBCCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1332"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ADARSH CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""HDFC0CADARS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1333"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE GAYATRI CO OP URBAN BANK"",
      ""Ifsc"": ""HDFC0CTGB07"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1334"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CITIZEN CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""HDFC0CCBL06"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1335"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SHARAD SAHAKARI BANK LTD"",
      ""Ifsc"": ""HDFC0CSHSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1336"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE JAMSHEDPUR URBAN COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CJUCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1337"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE FAZILKA CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SFAZ01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1338"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE ASSAM COOP APEX BANK LTD"",
      ""Ifsc"": ""HDFC0CACABL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1339"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SONEPAT CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SONE01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1340"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ANDAMAN AND NICOBAR STATE COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CANSCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1341"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""AKHAND ANAND CO OP BANK LTD"",
      ""Ifsc"": ""HDFC0CAACOB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1342"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KANPUR ZILLA SAHAKARI BANK LTD"",
      ""Ifsc"": ""ICIC00KZSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1343"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MEWAR AANCHALIK GRAMIN BANK"",
      ""Ifsc"": ""ICIC00MEWAR"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1344"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BARAMATI SAHAKARI BANK LTD"",
      ""Ifsc"": ""HDFC0CBSB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1345"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE MALKAPUR URB COOP BANK LTD"",
      ""Ifsc"": ""HDFC0CTMUCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1346"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JILA SAHKARI KENDRIYA BANK"",
      ""Ifsc"": ""UTIB0SJSB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1347"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SHARDA GRAMIN BANK"",
      ""Ifsc"": ""ALLA0SG5024"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1348"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NEFT MALWA GRAMIN BANK"",
      ""Ifsc"": ""STBP0RRMLGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1349"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""HIMACHAL GRAMIN BANK"",
      ""Ifsc"": ""PUNB0HPGB04"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1350"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PARSIK JANATA SAHAKARI BANK LTD"",
      ""Ifsc"": ""PJSB0000006"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1351"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SANGLI DISTRICT CENTRAL CO OP BANK LTD"",
      ""Ifsc"": ""IBKL0487SDC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1352"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CMS NATIONAL OPERATING CENTRE MMR"",
      ""Ifsc"": ""YESB0CMSNOC"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1353"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PILIBHIT DISTRICT CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""ICIC00PDCBL"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1355"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE GOA STATE COOP BANK"",
      ""Ifsc"": ""YESBOGSCB51"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1356"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE RAJASTHAN STATE COOPERATIVE BANK LTD"",
      ""Ifsc"": ""RSCB0000001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1357"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""AU SMALL FINANCE BANK LIMITED"",
      ""Ifsc"": ""AUBL0002220"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1358"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KHALILABAD NAGAR SAH BANK"",
      ""Ifsc"": ""YESB0KHBKCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1359"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE VILLUPURAM DISTRICT CENTRAL COOPERATIVE BANK L"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1360"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE AJARA URBAN CO OP BANK LTD"",
      ""Ifsc"": ""IBKL0116AUC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1361"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ABN Amro Bank Credit Card"",
      ""Ifsc"": ""ABNA0200001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1362"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Abu Dhabi Commercial Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1363"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Adarsh Mahila Mercantile Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1364"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Aditya Birla Idea Payments Bank"",
      ""Ifsc"": ""ABPB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1365"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ahmedabad Mercantile Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1366"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Aircel Smart Money  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1367"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Akola District Central Co-Op Bank"",
      ""Ifsc"": ""ADCC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1368"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Akola Janata Commercial Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1369"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Almora Urban Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1370"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ambarnath Jai Hind Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1371"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""American Express Credit Card"",
      ""Ifsc"": ""SCBL0036020"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1372"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Andhra Bank Credit Card"",
      ""Ifsc"": ""ANDB0000001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1373"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Andhra Pradesh State Co-Op Bank"",
      ""Ifsc"": ""ALBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1374"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Annasaheb Savant Co Op Urban Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1375"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Assam Co-Op Apex Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1376"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bajaj Finance Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1377"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ballia Etawah Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1378"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of America  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1379"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of Bahrain and Kuwait"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1380"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank Of Baroda Credit Card"",
      ""Ifsc"": ""BARB0COLABA"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1381"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of Ceylon  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1383"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of Nova Scotia "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1384"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank of Tokyo-Mitsubishi UFJ "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1385"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baramati Sahakari Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1386"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Barclays Credit Card"",
      ""Ifsc"": ""BARC0INBBIR"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1387"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Baroda Rajasthan Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1388"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bhagalpur Central Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1389"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bharat Co-Op Bank, Mumbai "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1390"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BHARUCH DISTRICT CENTRAL CO-OP. BANK LTD  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1391"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bijnor Urban Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1392"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Canara Bank Credit Card"",
      ""Ifsc"": ""CNRB0001912"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1393"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Capital Small Finance Bank "",
      ""Ifsc"": ""CLBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1394"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Central madhya pradesh gramin bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1395"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Chaitanya Godavari Grameena Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1396"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CHARTERED SAHAKARI BANK NIYAMITHA "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1397"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Chickmangalur Kodagu Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1398"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Chinatrust Commercial Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1399"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Citibank Credit Card"",
      ""Ifsc"": ""CITI0000003"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1400"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Contai Co Operative Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1401"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Agricole Corp and Investment Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1402"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dapoli Urban Co-Op Bank "",
      ""Ifsc"": ""IBKL0116DPC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1403"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Deccan Merchants Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1404"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Delhi State Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1405"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dena Gujarat Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1406"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Deposit Insurance and Credit Guarantee Corporation"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1407"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Deutsche Bank AG  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1408"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Development Credit Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1409"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""District Co-Op Bank, Agra "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1410"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dombivli East"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1411"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Durg Rajnandgaon Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1412"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ellaqui Dehati Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1413"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Equitas Small Finance Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1414"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ESAF Small Finance Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1415"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Firstrand Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1416"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Gayatri Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1417"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Greater Bombay Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1418"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Gujarat State Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1419"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Gurgaon Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1420"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Hadoti Kshetriya Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1421"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Hamirpur District Co-Op Bank, Mahoba"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1422"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Hasti Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1423"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Himachal Pradesh State Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1424"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Hindusthan Co-Op Bank  "",
      ""Ifsc"": ""SVCB0001002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1425"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Home Credit Finance Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1429"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""India Post Payments Bank "",
      ""Ifsc"": ""IPOS0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1430"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Indrayani Co Op Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1432"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jalaun District Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1433"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jalgaon Peoples Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1434"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janakalyan Sahakari Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1435"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Sahakari Bank, Osmanabad "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1436"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Sahakari Bank, Pune "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1437"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jhabua Dhar Kshetriya Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1438"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jharkhand State Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1439"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jijamata Mahila Sah Bank Ltd Pune  "",
      ""Ifsc"": ""HDFC0CJMS01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1440"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jila Sahakari Kendriya Bank Mydt Tikamgarh  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1441"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jila Sahkari Kendriya Bank Maryadit"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1442"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jio Payments Bank Limited "",
      ""Ifsc"": ""JIOP0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1443"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JP Morgan Chase Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1444"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kaira District Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1445"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kalupur Commercial Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1446"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kanaka Mahalakshmi Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1447"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kangra Central Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1448"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kangra Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1449"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kapole Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1450"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karad Urban Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1451"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karnala Nagari Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1452"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karnataka State Apex Co-Op "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1453"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karnataka State Co-Op Apex Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1454"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karnataka Vikas Grameena Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1455"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Khalilabad Nagar Sah Bank,Semariawa     "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1456"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kokan Mercantile Co Op Bank Ltd  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1457"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kolhapur Mahila Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CKMB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1459"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kurmanchal Nagar Sahkari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1460"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Madhya Bharat Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1461"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Madhyanchal Gramin Bank, Sagar "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1462"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mahakaushal Kshetriya Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1463"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Maharashtra State Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1464"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Malad Sahkari Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1465"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Malda District Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1466"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Malwa Gramin Bank  "",
      ""Ifsc"": ""SBIN0RRMLGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1467"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Manipur Rural ANK  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1468"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Manvi Pattana Souharda Sahakari Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1469"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pavana Sahakari Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1470"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pithoragarh Jila Sahkari Bank "",
      ""Ifsc"": ""IBKL0768PJS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1471"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pochampally Co-Op Urban Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1472"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Prerana Co Operative Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1473"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Prime Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1474"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Priyadarshani Nagari Sahakari Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1475"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Puduvai Bharathiar Grama Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1476"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Markandey Nagari Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1477"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mashreq Bank PSC  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1478"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mayani Urban Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1479"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MEGHALAYA COOP APEX BANK "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1480"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MG Baroda Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1481"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MGCB Main   "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1482"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mizuho Corporate Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1483"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mogaveera Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1484"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Moradabad Zila Sahkari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1485"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Municipal Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1486"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Murshidabad District Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1487"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nagar Sahkari Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1488"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nainital Almora Kshetriya Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1489"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nainital Bank   "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1490"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nanded Disctrict Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1491"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Narmada Jhabua Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1492"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Narmada Malwa Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1493"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nashik Merchants Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1494"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""National Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1495"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Neelachal Gramya Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1496"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""New India Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1497"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Noble Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1498"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""North Malabar Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1499"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Odisha State Co-Op Bank "",
      ""Ifsc"": ""ORCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1500"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Oman International Bank Saog "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1501"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pachora Peoples Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1502"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pallavan Grama Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1503"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pandharpur Merchant Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1504"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pandharpur Urban Co-Op Bank "",
      ""Ifsc"": ""PUCB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1505"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Pandyan Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1506"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Panipat Urban Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1507"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Parvatiya Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1508"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shri Chhatrapati Rajarshi Shahu Urban Co-Op Bank "",
      ""Ifsc"": ""CRUB0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1509"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Siwan Central Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1510"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Societe Generale   "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1511"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Solapur Janata Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1512"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""South Malabar Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1515"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""State Bank of Mauritius "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1516"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Surat District Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1517"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Surat National Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1518"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Surguja Kshetriya Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1519"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Suryoday Small Finance Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1520"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sutex Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1521"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Telangana State Co-Op Apex Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1522"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Thane Bharat Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1523"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Thane Janata Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1524"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Bhagyodaya Co Op Bank Ltd  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1525"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Gandhinagar Nagrik Cooperative Bank Ltd  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1526"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE GANDHINAGAR URBAN CO. BANK LTD  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1527"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Khamgaon Urban Co-Operative Bank Ltd  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1528"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Muslim Co Operative Bank Ltd  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1529"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Punjab State Co Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1530"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Thane District Central Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1531"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Udaipur Mahila Samridhi Urban Cooperative Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1532"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Titwala    "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1533"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Tripura Gramin Bank  "",
      ""Ifsc"": ""UTIB0RRBTGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1534"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Triveni Kshetriya Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1535"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UBS AG   "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1536"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Udaipur Urban Co-Op Bank "",
      ""Ifsc"": ""YESB0UUCB07"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1537"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Ujjivan Small Finance Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1538"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Urban Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1539"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Urban Co-Op Bank, Siddharthanagar "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1541"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Varachha Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1542"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vasai Vikas Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1543"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vidharbha Kshetriya Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1544"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vidisha Bhopal Kshetriya Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1545"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""VIJAY COMMERCIAL CO-OPERATIVE BANK "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1546"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Zila Sahakari Bank Haridwar "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1547"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Zila Sahakari Bank Ltd,Moradabad "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1548"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Zila Sahakari Bank Ltd,Rampur "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1549"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Zila Sahkari Bank, Lucknow "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1550"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Zoroastrian Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1552"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vikas Souharda Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1553"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vishweshwar Sahakari Bank Ltd "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1554"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Visveshwaraya Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1555"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Wainganga Krishna Gramin Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1556"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""West Bengal State Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1557"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Yadagiri Lakshmi Narasimha Swamy Co-Op Urban Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1558"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Vaidyanath Urban Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1559"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""VALSAD DISTRICT CENTRAL CO-OPERATIVE BANK LTD  "",
      ""Ifsc"": ""GSCB0VDC001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1560"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Ratnakar Bank Ltd "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1561"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Co-operative Bank of Rajkot Gandhigram  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1562"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Swarna Bharat Trust Cyber Grameen"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1563"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Tamilnadu State Apex Co-Op Bank"",
      ""Ifsc"": ""TNSC0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1564"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rajarshi Shahu Sah Bank, Pune"",
      ""Ifsc"": ""HDFC0CRSSBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1565"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rajasthan Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1567"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rajkot Nagarik Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1568"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Reserve Bank of India "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1569"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rewa-Sidhi Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1570"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Royal Bank of Scotland "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1571"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Rushikulya Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1572"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sabarkantha District Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1573"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sadhana Sahakari Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1574"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sahebrao Deshmukh Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1575"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Saibaba Nagari Sahakari Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1576"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Samarth Sahakari Bank Ltd "",
      ""Ifsc"": ""SBLS0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1577"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sangamner Merchant Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1578"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sant Sopankaka Sahkari Bank, Saswad"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1579"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sapthagiri Grameena Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1580"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sardar Bhiladwala Pardi Peoples Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1581"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Seva Vikas Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1582"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shamrao Vithal Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1583"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shinhan Bank   "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1584"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shirpur Peoples Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1585"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shree Mahalaxmi Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1586"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shree Sharada Sahakari Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1587"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shree Veershaiv Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1588"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shreyas Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1589"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Saurashtra Co-Op Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1590"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Punjab Gramin Bank  "",
      ""Ifsc"": ""PUNB0PGB003"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1592"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Purvanchal Gramin Bank  "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1593"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Raigad District Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1594"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NAGAR URBAN CO OPERATIVE BANK"",
      ""Ifsc"": ""NUCB0000145"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1753"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SATARA SAHAKARI BANK"",
      ""Ifsc"": ""YESB0TSSB07"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1754"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KOLHAPUR DISTRICT CENTRAL CO OP BANK LTD"",
      ""Ifsc"": ""IBKL0463KDC"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1755"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MAHESH URBAN COP BANK LTD"",
      ""Ifsc"": ""ICIC00MUCBA"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1756"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""ANDHRA PRADESH STATE CO-OP BANK"",
      ""Ifsc"": ""APBL0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1757"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""TELANGANA STATE CO-OP APEX BANK"",
      ""Ifsc"": ""TSAB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1758"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Karad Urban Co-Operative Bank Ltd"",
      ""Ifsc"": ""KUCB0488036"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1759"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Himatnagar Nagrik Sahakari Bank Ltd.,Himatnagar"",
      ""Ifsc"": ""IBKL0218HNS"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1760"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Union Co-Operative Bank Ltd."",
      ""Ifsc"": ""HDFC0CUCBNR"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1761"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sardargunj Mercantile Co-operative Bank Limited"",
      ""Ifsc"": ""UTIB0SSMC01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1762"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Ahmednagar District Central Co-op bank ltd."",
      ""Ifsc"": ""ICIC00ADCCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1763"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shree Kadi Nagarik Sahakari Bank Ltd."",
      ""Ifsc"": ""YESB0KNB006"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1764"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Citizens Co-Operative Bank Ltd"",
      ""Ifsc"": ""IBKL01642C1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1765"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jawahar Sahakari Bank Limited., Hupari."",
      ""Ifsc"": ""IBKL0116JCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1766"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""URBAN CO-OPERATIVE BANK LTD. BAREILLY"",
      ""Ifsc"": "" IBKL0232UCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1767"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""North East Small Finance Bank Ltd"",
      ""Ifsc"": ""NESF0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1768"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Alappuzha District Co-Operative Bank Ltd"",
      ""Ifsc"": ""UTIB0SADC83"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1769"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CHAMOLI ZILA SAHKARI BANK LTD."",
      ""Ifsc"": ""IBKL070CZSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1770"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NAVSARJAN INDL CO OP BANK LTD"",
      ""Ifsc"": ""HDFC0CNICBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1771"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kankaria Maninagar Nagrik Sahakari Bank Ltd"",
      ""Ifsc"": ""HDFC0CKMNSB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1772"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Sarvodaya Nagrik Sahkari bank Ltd"",
      ""Ifsc"": ""GSCB0UTSNBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1773"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BHOPAL COOPERATIVE CENTRAL BANK LIMITED"",
      ""Ifsc"": ""CBIN0MPDCAE"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1774"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kashmir Mercantile Co-op. Bank LTd., Kashmir"",
      ""Ifsc"": ""HDFC0CKAMCO"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1775"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Sevalia Urban Coop Bank Ltd"",
      ""Ifsc"": ""ICIC00SEVUC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1776"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bapuji Cooperative Bank Ltd"",
      ""Ifsc"": ""IBKL0364BCB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1777"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Rajkot Commercial Co op Bank Ltd."",
      ""Ifsc"": ""ICIC00RCCBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1778"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Uttarakhand State Cooperative Bank Ltd"",
      ""Ifsc"": ""ICIC00USCBD"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1779"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BARDOLI NAGRIK SAHAKARI BANK"",
      ""Ifsc"": ""GSCB0UTBNBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1780"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NATIONAL URBAN COOPERATIVE BANK LIMITED"",
      ""Ifsc"": ""IBKL01870N1"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1781"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SITAMARHI CENTRAL CO OPERATIVE BANK"",
      ""Ifsc"": ""IBKL0719SCB"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1782"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE AMBALA CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0ACCB01"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1783"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Gujarat Ambuja Co-Op. Bank Ltd"",
      ""Ifsc"": ""GSCB0UGACBL"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1784"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Tiruvalla East Co-Operative Bank"",
      ""Ifsc"": ""IBKL0029T03"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1785"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Gandhi Cooperative Urban Bank Ltd"",
      ""Ifsc"": ""YESB0GCUB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1786"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Anand Mercantile Cooperative Bank Limited"",
      ""Ifsc"": ""HDFC0CAMCBK"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1787"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nagrik Sahakari Bank Maryadhit Vidisha"",
      ""Ifsc"": ""HDFC0CNSBMV"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1788"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""BANDA URBAN COOP BANK"",
      ""Ifsc"": ""YESB0BUCB01"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1789"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""MAHANAGAR CO OP BANK LTD"",
      ""Ifsc"": ""MCBL0960035"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1790"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE PALI CENTRAL CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""RSCB0029001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1791"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BHATKAL URBAN COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SBUCB1"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1792"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CENTRAL MADHYA PRADESH GRAMIN BANK"",
      ""Ifsc"": ""CBINOR20002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1793"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DMK JAOLI BANK"",
      ""Ifsc"": ""DMKJ0000003"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1794"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE TEXCO BANK"",
      ""Ifsc"": ""YESB0TCB002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1795"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BEGUSARAI DISTRICT CENTRAL CO OPERATIVE BANK"",
      ""Ifsc"": ""IBKL01077BD"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1796"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SAHKARI BANK LTD"",
      ""Ifsc"": ""ICIC00ZSBBU"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1797"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE AMRITSAR CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SASR01"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1798"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE KANGRA COOPERATIVE BANK LTD"",
      ""Ifsc"": ""KANG0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1799"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE NAVAL DOCKYARD CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""IBKL0452ND1"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1800"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""CITIZENCREDIT CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""CCBL0209033"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1801"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SOUTH CANARA DISTRICT CENTRAL CO OPERATIVE BAN"",
      ""Ifsc"": ""IBKL078SCDC"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1802"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE SURAT PEOPLES CO OP BANK LTD"",
      ""Ifsc"": ""SPCB0251016"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1803"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""PIMPRI CHINCHWAD SAHAKARI BANK"",
      ""Ifsc"": ""IBKL0087PCS"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1804"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE FATEHABAD CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0FCCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1805"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE MANSA CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SMSA01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1806"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NSDL PAYMENTS BANK LIMITED"",
      ""Ifsc"": ""NSPB0000001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1807"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JOGINDRA CENTRAL COOP BANK"",
      ""Ifsc"": ""YESB0JCCB01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1808"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UJJIVAN SMALL FINANCE BANK LIMITED"",
      ""Ifsc"": ""UJVN0004428"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1809"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SURAT NATIONAL COOPERATIVE BANK LIMITED"",
      ""Ifsc"": ""SUNB0000017"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1810"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NARMADA JHABUA GRAMIN BANK"",
      ""Ifsc"": ""BKID0NAMRGB"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1811"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE PANCHKULA CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0SPKL01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1812"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE KRISHNA DISTRICT CO OPERATIVE CENTRAL BANK LTD"",
      ""Ifsc"": ""APBL0006004"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1813"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KRISHNA BHIMA SAMRUDDHI BANK"",
      ""Ifsc"": ""HDFC0CKBS01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1814"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE DINDIGUL CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""TNSC0011700"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1815"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE LAKSHMI VILAS BANK LTD"",
      ""Ifsc"": ""LAVB0000999"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1816"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE HISAR CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0HCCB01"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1817"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Australia And New Zealand Banking Group Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1818"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Bank Internasional Indonesia"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1819"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Cauvery Kalpatharu Grameena Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1820"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Citizen Co-Op Bank, Noida"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1821"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Citizen Credit Co-op Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1827"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - HDFC Bank"",
      ""Ifsc"": ""HDFC0000128"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1828"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - HSBC"",
      ""Ifsc"": ""HSBC0400002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1829"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - ICICI Bank"",
      ""Ifsc"": ""ICIC0000103"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1830"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - IDBI Bank"",
      ""Ifsc"": ""IBKL0NEFT01"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1831"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - Indusind Bank"",
      ""Ifsc"": ""INDB0000018"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1832"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - Kotak Mahindra Bank"",
      ""Ifsc"": ""KKBK0000958"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1833"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - PNB Bank"",
      ""Ifsc"": ""PUNB0112000"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1834"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - SBI"",
      ""Ifsc"": ""SBIN00CARDS"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1835"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - Standard Chartered"",
      ""Ifsc"": ""SCBL0000001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1836"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - UTI/ Axis Bank"",
      ""Ifsc"": ""UTIB0000400"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1837"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - Vijaya Bank"",
      ""Ifsc"": ""VIJB0000001"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1838"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Suisse Ag"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1839"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Deccan Grameena Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1840"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Deogiri Nagari Sahakari Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1841"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Dicgc"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1842"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Doha Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1843"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Durgapur Steel Peoples Cooperative Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1844"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Emirates Nbd India"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1845"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Export Import Bank Of India"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1846"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Haryana Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1847"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""HSBC"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1848"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Indusind Bank Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1849"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Industrial And Commercial Bank Of China Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1850"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Sahakari Bank Ltd (Pune)"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1851"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Janata Sahakari Bank Ltd,Ajara"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1852"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Jijamata Mahila Sah Bank Ltd Pune"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1853"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Kashi Gomati Samyut Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1854"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Krishna Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1855"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Markandey Nagari Sahakari Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1856"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Mg Baroda Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1857"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nashik Merchants Co-Op Bank "",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1858"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""National Australia Bank Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1859"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""National Bank For Agriculture And Rural Developmen"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1860"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Nkgsb Co-op Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1861"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Purvanchal Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1862"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Qatar Natoinal Bank SAQ"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1863"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""RBL Bank Credit Card"",
      ""Ifsc"": ""RATN0CRCARD"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1864"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Samarth Sahakari Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1865"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Shikshak Sahakari Bank Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1866"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Siwan Central Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1867"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Small Industries Development Bank Of India"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1868"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Solapur Janata Sahakari Bank Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1869"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Sumerpur Merchantile Urban Co-op. Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1870"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Surat National Co-op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1871"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Chembur Nagarik Sahakari Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1872"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Commercial Co-op Bank Limited,Kolhapur"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1873"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Eenadu Co-op Urban Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1874"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Ernakulam District Co-Operative Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1875"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Kurla Nagarik Sahakari Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1876"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Nainital Bank Limited"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1877"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The National Co-operative Bank Ltd."",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1878"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Saraswat Co-operative Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1879"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Shirpur peoples co-op bank ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1880"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The United Coop Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1881"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Uttarsanda Peoples Co Op Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1882"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""The Washim Urban Co-Operative Bank Ltd,Washim"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1883"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Thrissur District Central Co-op Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1884"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Tripura Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1885"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Triveni Kshetriya Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1886"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Tumkur Grain Merchants Cooperative Bank Ltd"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1887"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Unjha Nagarik Sahakari Bank Ltd."",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1888"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Urban Co-Op Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1889"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UTTAR DAUDPUR SAMABAY KRISHI UNNAYAN SAMITY LTD"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1890"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Uttaranchal Gramin Bank"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1891"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Westpac Banking Corporation"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1892"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Zila Sahakari Bank Haridwar"",
      ""Ifsc"": """",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1893"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""IDFC FIRST BANK CREDIT CARD"",
      ""Ifsc"": ""IDFB0010225"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1894"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE AHMEDABAD MERCANTILE CO OP BANK LTD"",
      ""Ifsc"": ""AMCB0660010"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1895"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NAGPUR NAGRIK SAHAKARI BANK LTD"",
      ""Ifsc"": ""NGSB0000045"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1896"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""JILA SAHAKARI KENDRIYA BANK"",
      ""Ifsc"": ""CBIN0MPDCAJ"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1897"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE BHIWANI CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UTIB0BHIW01"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1898"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""TIRUPATI URBAN CO OP BANK"",
      ""Ifsc"": ""HDFC0CTUB13"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1899"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""NAVI MUMBAI CO OP BANK LTD"",
      ""Ifsc"": ""IBKL0123NMC"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1900"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KANNUR DISTRICT CO OPERATIVE BANK"",
      ""Ifsc"": ""UTIB0SKDC01"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1901"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""SIKKIMSTATE COOPERATIVE BANK LTD"",
      ""Ifsc"": ""IBKL0108SIC"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1902"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""TAMLUK GHATAL CENTRAL CO OPERATIVE BANK LTD"",
      ""Ifsc"": ""WBSC0TCCB23"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1903"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""DAKSHIN BIHAR GRAMIN BANK"",
      ""Ifsc"": ""PUNB0MBGB06"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1904"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""UTTAR PRADESH COOPERATIVE BANK LTD"",
      ""Ifsc"": ""UPCB0000002"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1905"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""KARNATAKA GRAMIN BANK"",
      ""Ifsc"": ""PKGB0012501"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1906"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""TAMIL NADU GRAMIN BANK"",
      ""Ifsc"": ""IDIBOPLE001"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1907"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE CHENNAI CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""TNSC0010500"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1908"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE COIMBATORE DISTRICT CENTRAL COOPERATIVE BANK"",
      ""Ifsc"": ""TNSC0010000"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1909"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE CUDDALORE DISTRICT CENTRAL COOPERATIVE BANK LT"",
      ""Ifsc"": ""TNSC0011200"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1910"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE DHARMAPURI DISTRICT CENTRAL COOPERATIVE BANK L"",
      ""Ifsc"": ""TNSC0010100"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1911"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE ERODE DISTRICT CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""TNSC0010800"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1912"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE KANCHIPURAM CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""TNSC0010200"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1913"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE KANYAKUMARI DISTRICT CENTRAL COOPERATIVE BANK "",
      ""Ifsc"": ""TNSC0010300"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1914"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE KUMBAKONAM CENTRAL COOPERATIVE BANK LTD"",
      ""Ifsc"": ""TNSC0010400"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1915"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""THE PUDUKKOTTAI DISTRICT CENTRAL COOPERATIVE BANK "",
      ""Ifsc"": ""TNSC0010900"",
      ""ifscStatus"": ""N"",
      ""channelsSupported"": ""ALL""
    },
    {
      ""BankCode"": ""1916"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Credit Card - AU small finance bank"",
      ""Ifsc"": ""AUBL0CCARDS"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1917"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""YES Bank Credit Card"",
      ""Ifsc"": ""YESB0CMSNOC"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1919"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""Union Bank of India Credit Card"",
      ""Ifsc"": ""UBIN0807826"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    },
    {
      ""BankCode"": ""1920"",
      ""accVerAvailabe"": ""Y"",
      ""BankName"": ""FEDERAL BANK - CREDIT CARD"",
      ""Ifsc"": ""FDRL00CARDS"",
      ""ifscStatus"": ""Y"",
      ""channelsSupported"": ""NEFT""
    }]";
                        #endregion 
                      string  string_without_newlines = Banklist.Replace("\r\n", "");

                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            MESSAGE = StatusMsg.RECHARGE_SUCCESS,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            BANKLIST = string_without_newlines,
                            HTTPCODE = HttpStatusCode.OK

                        });
                    }
                }
                else
                {
                    log += "ModelState IsValid  ";
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.FAILED,
                        MESSAGE = StatusMsg.INVALID_REQUEST,
                        ERRORCODE = ErrorCode.INVALID_REQUEST,
                        HTTPCODE = HttpStatusCode.BadRequest
                    });
                }
            }
            catch (Exception ex)
            {

                Common.LogException(ex);
            }
            Common.LogActivity(log);
            return response;
        }
        private JObject FetchBankListProcess(FetchBankListDto model, ref string log, ref JObject response)
        {
            log += " Save Data in  ";
            RequestResponseDto userReqRes = new RequestResponseDto();
            userReqRes.Remark = "Fetch_BankList";

            userReqRes.CustomerNo = model.TokenId;
            int Error = 1;
            string ApiUrl = string.Empty;
            string PostData = string.Empty;
            string OpCode = string.Empty;
            Connection();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetApiUrl", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", model.TokenId);
                    cmd.Parameters.AddWithValue("@Opid", model.OpId);
                    cmd.Parameters.AddWithValue("@urlType", 2);
                    cmd.Parameters.Add("@PostData", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ApiUrl", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@OpCode", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@error", SqlDbType.Int);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    Error = (int)cmd.Parameters["@error"].Value;
                    ApiUrl = Convert.ToString(cmd.Parameters["@ApiUrl"].Value);
                    PostData = Convert.ToString(cmd.Parameters["@PostData"].Value);
                    OpCode = Convert.ToString(cmd.Parameters["@OpCode"].Value);
                    log += " error code after exec  SP_GetApiUrl " + Error;
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            if (Error == 10)
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.VENDOR_NOT_ACTIVE,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (string.IsNullOrEmpty(ApiUrl))
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.ERROR_CONTACT_ADMIN,
                    ERRORCODE = ErrorCode.APIURL_NOT_SET,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            else if (Error == 0)
            {
                List<BankListDto> Banklist = new List<BankListDto>();

                try
                {
                    var url = ApiUrl.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, string.Empty, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                    var postdata = !string.IsNullOrEmpty(PostData) ? PostData?.ReplaceURL(SiteKey.AgentAuthId, string.Empty, SiteKey.AgentAuthPassword, string.Empty, OpCode, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty, String.Empty, String.Empty, String.Empty, string.Empty, String.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                    userReqRes.RequestTxt = ApiUrl + "DATA" + postdata;

                    ApiCall apiCall = new ApiCall(reqResService);
                    string apires = string.Empty;
                    log += " request Post data = " + postdata;

                    apires = apiCall.Post(url, postdata , ref log);
                    userReqRes.ResponseText = apires;
                    log += "  Api Res " + apires;
                    userReqRes = AddUpdateReqRes(userReqRes, ref log);
                    if (string.IsNullOrEmpty(apires))
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = StatsCode.SUCCESS,
                            ERRORCODE = ErrorCode.NO_ERROR,
                            HTTPCODE = HttpStatusCode.OK,
                            BANKLIST = Banklist
                        });
                    }
                    else
                    {
                        dynamic res = JObject.Parse(apires);

                        JsonDocument dmtServiceResponse = JsonDocument.Parse(apires);

                        JsonElement recipientList = dmtServiceResponse.RootElement.GetProperty("data").GetProperty("bankList");

                        foreach (JsonElement recipients in recipientList.EnumerateArray())
                        {
                            BankListDto recipient = new BankListDto
                            {
                                BankCode = recipients.GetProperty("bankCode").GetString(),
                                channelsSupported = recipients.GetProperty("channelsSupported").GetString(),
                                BankName = recipients.GetProperty("bankName").GetString(),
                                accVerAvailabe = recipients.GetProperty("accVerAvailabe").GetString(),
                                Ifsc = recipients.GetProperty("ifsc").GetString(),
                                ifscStatus = recipients.GetProperty("ifscStatus").GetString(),
                            };
                            Banklist.Add(recipient);
                        }
                        if (res.errorMsg == "SUCCESS")
                        {
                            response = JObject.FromObject(new
                            {
                                STATUS = StatsCode.SUCCESS,
                                ERRORCODE = ErrorCode.NO_ERROR,
                                HTTPCODE = HttpStatusCode.OK,
                                BANKLIST = Banklist
                            });
                        }
                        else
                        {
                            response = JObject.FromObject(new
                            {
                                STATUS = StatsCode.FAILED,
                                RESPONSE = apires,
                                ERRORCODE = ErrorCode.INVALID_REQUEST,
                                HTTPCODE = HttpStatusCode.BadRequest
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = JObject.FromObject(new
                    {
                        STATUS = StatsCode.SUCCESS,
                        ERRORCODE = ErrorCode.NO_ERROR,
                        HTTPCODE = HttpStatusCode.OK,
                        BANKLIST = Banklist
                    });

                    Common.LogException(ex);
                    Common.LogActivity(log);
                }
            }
            else
            {
                response = JObject.FromObject(new
                {
                    STATUS = StatsCode.FAILED,
                    MESSAGE = StatusMsg.INVALID_TOKEN,
                    ERRORCODE = ErrorCode.INVALID_TOKEN,
                    HTTPCODE = HttpStatusCode.OK
                });
            }
            return response;
        }
        #endregion
        #region Sp
        private void Connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;
            con = new SqlConnection(constr);
        }
        private string GetHeaderValues(HttpRequestMessage request, ref string tokenId, ref string regMobileNo, ref string log)
        {
            if (request.Headers.Contains("TokenID"))
            {
                tokenId = request.Headers.GetValues("TokenID").First();
            }
            log += " Request  TokenID " + tokenId;
            if (request.Headers.Contains("regMobileNumber"))
            {
                regMobileNo = request.Headers.GetValues("regMobileNumber").First();
            }
            log += "  MobileNumber " + regMobileNo;
            return log;
        }
        private Tuple<DataTable, string> CheckToken_RegmobileNo(string Tokenid, string RegMobileNo)
        {
            try
            {
                Connection();
                using (SqlCommand cmd = new SqlCommand("CheckToken_RegmobileNo", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", Tokenid);
                    cmd.Parameters.AddWithValue("@RegMobileNo", RegMobileNo);
                    cmd.Parameters.Add("@Tokenvalid", SqlDbType.VarChar, 100);
                    cmd.Parameters["@Tokenvalid"].Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@error", SqlDbType.VarChar, 100);
                    cmd.Parameters["@error"].Direction = ParameterDirection.Output;
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    con.Close();
                    string Tokenvalid = (String)cmd.Parameters["@Tokenvalid"].Value;
                    string Error = (String)cmd.Parameters["@error"].Value;
                    return new Tuple<DataTable, string>(dt, Tokenvalid);
                }
            }

            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

                return new Tuple<DataTable, string>(null, "");
            }
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
        private bool AddBeneficiaryList(Recipients recipients, string Tokenid)
        {
            try
            {
                Connection();
                using (SqlCommand cmd = new SqlCommand("SP_AddBeneficiaryList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TokenId", Tokenid);
                    cmd.Parameters.AddWithValue("@RecipientId", recipients.RecipientId);
                    cmd.Parameters.AddWithValue("@BankAccountNumber", recipients.BankAccountNumber);
                    cmd.Parameters.AddWithValue("@BankName", recipients.BankName);
                    cmd.Parameters.AddWithValue("@MobileNumber", recipients.MobileNumber);
                    cmd.Parameters.AddWithValue("@Ifsc", recipients.Ifsc);
                    cmd.Parameters.AddWithValue("@RecipientName", recipients.RecipientName);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    return true;
                }
            }

            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

                return false;
            }
        }
        #endregion
    }
}