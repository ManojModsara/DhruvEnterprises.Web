using System;
using System.IO;
using System.Net;
using System.Text;
using DhruvEnterprises.Service;
using System.Web;
using DhruvEnterprises.API.Models;
using RestSharp;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace DhruvEnterprises.API.LIBS
{
    public class ApiCall
    {
        #region Reference Variabless

        private readonly IRequestResponseService reqResService;

        #endregion

        #region Constructor

        public ApiCall(IRequestResponseService _reqResService)
        {
            this.reqResService = _reqResService;

        }
        #endregion
        public string Get(string url, ref RequestResponseDto requestResponse, int apiid, string contenttype = "", string accept = "", string userkey = "", string pass = "")
        {
            string response = string.Empty;
            try
            {
                if (apiid == 3)
                {
                    // response = GetSteFront(url, ref requestResponse, contenttype, accept, userkey, pass);
                }
                else
                {
                    response = Get(url, ref requestResponse);
                }
            }
            catch (Exception ex)
            {

                string comment = ", url=" + url;
                Common.LogException(ex, comment);
                Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
            }
            return response;
        }
        public string Post(string url, string postData, string contenttype, string accept, ref RequestResponseDto requestResponse, int apiid = 0, string userkey = "", string pass = "")
        {
            string apires = "";

            try
            {
                apires = Post(url, postData, contenttype, accept, ref requestResponse);
            }
            catch (Exception ex)
            {
                string comment = ", url=" + url + ", data=" + postData;
                Common.LogException(ex, comment);
                Common.LogActivity("|ApiCalExcp=" + ex.Message + ", url=" + url + ", data=" + postData + "|");
            }


            return apires;
        }
        #region  dmt
        public string Post(string Apiurl, string PostData ,ref string log)
        {
            try
            {
                #region old
                log += "\r\n post method call";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var client = new RestClient();
                client.Timeout = -1;
                log += " \r\n AgentAuthId = " + SiteKey.AgentAuthId + " AgentAuthPassword = " + SiteKey.AgentAuthPassword;
                var request = new RestRequest(Apiurl, Method.POST);
                string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(SiteKey.AgentAuthId + ":" + SiteKey.AgentAuthPassword));
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Basic " + svcCredentials);
                request.AddJsonBody(PostData);
                //api Execute 
                log += " \r\n befor Execute api";
                IRestResponse response = client.Execute(request);
                log += " \r\n after Execute api res " + response.Content;
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound  
                   || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized)

                return response.Content;
                #endregion
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        #endregion
        #region Api id =11
        public string Post2(string Apiurl, string PostData)
        {
            try
            {
                var log = "post method call";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    | SecurityProtocolType.Tls;
                var handler = new HttpClientHandler();
                var client = new RestClient();
                client.Timeout = -1;

                var request = new RestRequest(Apiurl, Method.POST);
                request.AddHeader("appId", "099cc451827e4fc380ea70bfd4ea72f7");
                request.AddHeader("secretKey", "6428077aa7ab4c67b1e007bd31ce9271");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", PostData ,ParameterType.RequestBody);
                //api Execute 
                log += "befor Execute api";
                IRestResponse response = client.Execute(request);
                log += "after Execute api res " + response.Content;
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound
                    || response.StatusCode == HttpStatusCode.OK)

                    Common.LogActivity(log);
                return response.Content;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        public string GetBingPay(string url, ref RequestResponseDto requestResponse)
        {
            try
            {
                var log = "post method call";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    | SecurityProtocolType.Tls;
                var handler = new HttpClientHandler();
                var client = new RestClient();
                client.Timeout = -1;

                var request = new RestRequest(url, Method.GET);
                //api Execute 
                log += "befor Execute api";
                IRestResponse response = client.Execute(request);
                log += "after Execute api res " + response.Content;
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound
                    || response.StatusCode == HttpStatusCode.OK)

                    Common.LogActivity(log);
                return response.Content;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        public string Get(string Apiurl, string PostData)
        {
            try
            {
                var log = "post method call";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    | SecurityProtocolType.Tls;
                var handler = new HttpClientHandler();
                var client = new RestClient();
                client.Timeout = -1;

                var request = new RestRequest(Apiurl, Method.GET);
                request.AddHeader("appId", "099cc451827e4fc380ea70bfd4ea72f7");
                request.AddHeader("secretKey", "6428077aa7ab4c67b1e007bd31ce9271");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", PostData, ParameterType.RequestBody);
                //api Execute 
                log += "befor Execute api";
                IRestResponse response = client.Execute(request);
                log += "after Execute api res " + response.Content;
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound
                    || response.StatusCode == HttpStatusCode.OK)

                    Common.LogActivity(log);
                return response.Content;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        #endregion
        public string Post(string ApiUrl, string Postdata, string ContentType, string ResType, int apiId, string ApiUserId, string ApiPassword)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var handler = new HttpClientHandler();
                //handler.Proxy = new WebProxy("136.243.46.243", 808)
                //{
                //    Credentials = new NetworkCredential("admin", "admin")
                //};
               // string test = ApiUrl + Postdata;
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl + Postdata);
                //api Execute 
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound
                    || response.StatusCode == HttpStatusCode.OK)
                    return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        #region apiid 9
        public string Post(string ApiUrl, string Postdata, ref MoneyTransferModel model, string ContentType, string ResType, string ApiUserId, string ApiPassword)
        {
            try
            {
                var log = "post method call";
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var handler = new HttpClientHandler();
                var client = new RestClient();
                client.Timeout = -1;
                log += " UNK id =" + model.ourtxnid;
                var request = new RestRequest(ApiUrl,Method.POST);
                string sha512Hash = ""+SiteKey.Payoutkey+"|" + model.accountno + "|" + model.ifsc + "|" + "|" + model.ourtxnid + "|" + model.amount + "|"+SiteKey.payoutSalt+"";
                var Authorization = CalculateSHA512(sha512Hash);
                request.AddHeader("Content-Type", "application/Json");
                request.AddHeader("Authorization", Authorization);
                request.AddParameter("application/json", Postdata, ParameterType.RequestBody);
                //api Execute 
                log += "befor Execute api";
                IRestResponse response = client.Execute(request);
                log += "after Execute api";
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound
                    || response.StatusCode == HttpStatusCode.OK)
                    Common.LogActivity(log);
                    return response.Content;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        public string Post2(string ApiUrl, string Postdata, string RefTxnId, string ResType, int apiId, string ApiUserId, string ApiPassword)
        {
            string responseBody = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string sha512Hash = "" + SiteKey.Payoutkey + "|" + RefTxnId + "|" + SiteKey.payoutSalt + "";
                var Authorization = CalculateSHA512(sha512Hash);
               
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", Authorization);
                    try
                    {
                        // Send the GET request and get the response
                        HttpResponseMessage response = client.GetAsync(ApiUrl).Result;
                        // Check if the request was successful
                        if (response.IsSuccessStatusCode)
                        {
                            // Read and display the response body as a string
                            responseBody = response.Content.ReadAsStringAsync().Result;
                            return responseBody;
                        }
                        else
                        {
                            return responseBody;
                        }
                    }
                    catch (Exception ex)
                    {
                        return responseBody;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return responseBody;
        }
        static string CalculateSHA512(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha512.ComputeHash(inputBytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion
        public string Post(string Url, ref string log, ref RequestResponseDto requestResponseDto)
        {
            string reponce = string.Empty;
            log += "Call SenderRegistration Api";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = new StringContent("", null, "text/plain").ToString();
            Stream dataStream = request.GetRequestStream();

            using (WebResponse response = request.GetResponse())
            {
                dataStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(dataStream);
                reponce = streamReader.ReadToEnd();
                requestResponseDto.ResponseText = reponce;
                log += "ResponseText" + reponce;
                response.Close();
            }
            return reponce;
        }
        public string Post(string url, string postData, string contenttype, string accept, ref RequestResponseDto requestResponse)
        {
            requestResponse.RequestTxt = "URL: " + url + ", DATA: " + postData;
            string responseFromServer = string.Empty;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contenttype;
            request.Accept = accept;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            using (WebResponse response = request.GetResponse())
            {
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                requestResponse.ResponseText = responseFromServer; //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");

                response.Close();
            }

            return responseFromServer;

        }
        public string Get(string url, ref RequestResponseDto requestResponse, string userkey = "", string pass = "")
        {
            string response = string.Empty;
            try
            {
                response = Get(url, userkey, pass, ref requestResponse);
            }
            catch (Exception ex)
            {
                string comment = ", url=" + url;
                Common.LogException(ex, comment);
                Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
            }
            return response;
        }
        public string Get(string url, string username, string password, ref RequestResponseDto requestResponse)
        {
            {
                string response = string.Empty;
                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    requestResponse.RequestTxt = "URL: " + url;
                    var client = new RestClient("https://dashboard.xettle.net");
                    var request = new RestRequest(url, Method.GET);

                    request.AddHeader("Content-Type", "application/json");
                    string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
                    request.AddHeader("Authorization", "Basic " + svcCredentials);
                    var response1 = client.Get(request);
                    response = response1.Content;
                    requestResponse.ResponseText = response;
                    //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");
                }
                catch (Exception ex)
                {
                    response = "CONN_ERROR-" + ex.Message;
                    string comment = ", url=" + url;
                    Common.LogException(ex, comment);
                    Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
                }
                return response;
            }
        }
        public string PostUpiCall(string uri, string postData, string username, string password, ref RequestResponseDto requestResponse)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Common.LogActivity("Request=" + postData);
            requestResponse.RequestTxt = "URL: " + uri + ", DATA: " + postData;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = postData.Length;
            webRequest.Accept = "application/json";
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            webRequest.Headers.Add("Authorization", "Basic" + svcCredentials);
            try
            {
                using (StreamWriter requestWriter2 = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter2.Write(postData);
                }
                string responseData = string.Empty;
                StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
                Common.LogActivity("responseData=" + responseData);
                requestResponse.ResponseText = responseData;
                return responseData;
            }
            catch (WebException web)
            {
                HttpWebResponse res = web.Response as HttpWebResponse;
                Stream s = res.GetResponseStream();
                string message;
                StreamReader sr = new StreamReader(s);
                message = sr.ReadToEnd();
                requestResponse.ResponseText = message;
                return message;
            }
        }
        public string PostUpi(string uri, string postData, string username, string password, ref RequestResponseDto requestResponse)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var handler = new HttpClientHandler();
            var client = new RestClient();
            client.Timeout = -1;
            string log = "";
            var request = new RestRequest(uri, Method.POST);
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.AddHeader("Authorization", "Basic " + svcCredentials);
            request.AddHeader("accept", "application/json");
  
            request.AddParameter("application/json", postData, ParameterType.RequestBody);
            //api Execute 
      
            IRestResponse response = client.Execute(request);
       
            if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NotFound
                || response.StatusCode == HttpStatusCode.OK)
                Common.LogActivity(log);
            requestResponse.ResponseText = response.Content; 
            return response.Content;
        }

    }
}