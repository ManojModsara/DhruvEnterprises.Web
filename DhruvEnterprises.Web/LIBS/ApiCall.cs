using CCA.Util;
using CyberPlatOpenSSL;
//using DhruvEnterprises.API.LIBS;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DhruvEnterprises.Web.LIBS
{
    public class ApiCall
    {
        #region Reference Variables

        private readonly IRequestResponseService reqResService;

        #endregion

        #region Constructor

        public ApiCall(IRequestResponseService _reqResService)
        {
            this.reqResService = _reqResService;

        }
        #endregion
        
        public string Post(string url, string postData, string contenttype, string accept, ref RequestResponseDto requestResponse)
        {
            requestResponse.RequestTxt = "URL: " + url + ", DATA: " + postData;
            string responseFromServer = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

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

        public string Post(string url, string postData, string contenttype, string accept, ref RequestResponseDto requestResponse, int apiid = 0, string userkey = "", string pass = "")
        {
            string apires = "";

            try
            {
                    apires= apires = Post(url, postData, contenttype, accept, ref requestResponse);
            }
            catch (Exception ex)
            {
                string comment = ", url=" + url + ", data=" + postData;
                Core.Common.LogException(ex, comment);
                Core.Common.LogActivity("|ApiCalExcp=" + ex.Message + ", url=" + url + ", data=" + postData + "|");
            }


            return apires;
        }
        #region apiid 11
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

                 
                return response.Content;
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
            return "";
        }
        #endregion
        public string Get(string url, ref RequestResponseDto requestResponse, int apiid, string contenttype = "", string accept = "", string userkey = "", string pass = "")
        {
            string response = string.Empty;
            try
            {
                    response= Get(url, ref requestResponse, contenttype);
            }
            catch (Exception ex)
            {

                string comment = ", url=" + url;
                Core.Common.LogException(ex, comment);
                Core.Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
            }


            return response;

        }

        public string PostSpaceXPay(string Data, string Uid, ref RequestResponseDto requestResponse)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new RestClient("https://payout.safexpay.com/agWalletAPI/v2/agg");
                string PostData = "{\r\n\"payload\": \"" + Data + "\",\r\n\"uId\": \"" + Uid + "\"\r\n}";
                requestResponse.RequestTxt = "URL: https://payout.safexpay.com/agWalletAPI/v2/agg  DATA: text/plain " + PostData;
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "text/plain");
                request.AddParameter("text/plain", "{\r\n\"payload\": \"" + Data + "\",\r\n\"uId\": \"" + Uid + "\"\r\n}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);
                return response.Content;
            }
            catch (Exception)
            {

                return "";
            }
        }
        public string Get(string url, ref RequestResponseDto requestResponse, string contenttype = "")
        {
            string response = string.Empty;
            try
            {
                requestResponse.RequestTxt = "URL: " + url;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);
                if (url.Contains("planapi"))
                {
                    httpreq.Timeout = 2000;
                    httpreq.ReadWriteTimeout = 2000;
                }

                httpreq.ContentType = contenttype;
                using (HttpWebResponse httpres = (HttpWebResponse)httpreq.GetResponse())
                {
                    StreamReader sr = new StreamReader(httpres.GetResponseStream());
                    response = sr.ReadToEnd();
                    sr.Close();
                }

                requestResponse.ResponseText = response; //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");

            }
            catch (Exception ex)
            {
                response = "CONN_ERROR-" + ex.Message;
                string comment = ", url=" + url;
                Core.Common.LogException(ex, comment);
                Core.Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
            }
              
            return response;

        }

        //#region payout callback
        //public string Get(string url, ref RequestResponseDto requestResponse, string userkey = "", string pass = "")
        //{
        //    string response = string.Empty;
        //    try
        //    {
        //        response = Get(url, userkey,pass, ref requestResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        string comment = ", url=" + url;
        //        Core.Common.LogException(ex, comment);
        //        Core.Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
        //    }
        //    return response;
        //}

        //public string Get(string url, string username, string password, ref RequestResponseDto requestResponse)
        //{
        //    {
        //        string response = string.Empty;
        //        try
        //        {
        //            ServicePointManager.Expect100Continue = true;
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //            requestResponse.RequestTxt = "URL: " + url;
        //            var client = new RestClient("https://dashboard.xettle.net");
        //            var request = new RestRequest(url, Method.GET);

        //            request.AddHeader("Content-Type", "application/json");
        //            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
        //            request.AddHeader("Authorization", "Basic " + svcCredentials);
        //            var response1 = client.Get(request);
        //            response = response1.Content;
        //            requestResponse.ResponseText = response; //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");
        //        }
        //        catch (Exception ex)
        //        {
        //            response = "CONN_ERROR-" + ex.Message;
        //            string comment = ", url=" + url;
        //            Core.Common.LogException(ex);
        //            Core.Common.LogActivity("|ApiCallExcp=" + ex.Message + ", url=" + url);
        //        }
        //        return response;
        //    }
        //}

        //#endregion

        public string PostCyberPlat(string url, string postData, string contenttype, string accept, string cert, string pass, ref RequestResponseDto requestResponse)
        {
            requestResponse.RequestTxt = "cyberplat URL: " + url + ", DATA: " + postData;

            string resp = string.Empty;

            OpenSSL ssl = new OpenSSL();

            ssl.CERTNo = cert;
            string keyPath = HttpContext.Current.Server.MapPath("~/Certificates/myprivatekey.pfx");
            ssl.message = ssl.Sign_With_PFX(postData, keyPath, pass);
            ssl.htmlText = ssl.CallCryptoAPI(ssl.message, url);

            resp = ssl.htmlText;
            Core.Common.LogActivity("cyberplathit : cert=" + cert + ", keyPath=" + keyPath + ", pass=" + pass + "RequestTxt=" + requestResponse.RequestTxt + ", ssl.message=" + ssl.message + ", response:" + resp);

            requestResponse.ResponseText = resp; //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");
            return resp;

        }

        public string PostBillAvenues(string url, string postdata, string contenttype, string accept, string cert, string pass, ref RequestResponseDto requestResponse)
        {
            requestResponse.RequestTxt = "URL: " + url + ", DATA: " + postdata;

            string postenc = string.Empty;

            string xmlStr = postdata.GetSubstring("encRequest=", "none")?.Trim();

            int idx = postdata.IndexOf("encRequest=") + ("encRequest=").Length;
            postenc = postdata.Remove(idx);

            CCACrypto crypto = new CCACrypto();
            var encStr = crypto.Encrypt(xmlStr, pass);
            postenc = postenc + encStr;

            WebRequest wReq = WebRequest.Create(url);
            HttpWebRequest httpReq = (HttpWebRequest)wReq;
            httpReq.Method = "POST";
            httpReq.ContentType = "application/x-www-form-urlencoded";

            //ONLY TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Stream sendStream = httpReq.GetRequestStream();

            StreamWriter strmWrtr = new StreamWriter(sendStream);
            strmWrtr.Write(postenc);
            strmWrtr.Close();
            WebResponse wResp = null;
            StreamReader strmRdr = null;
            string responseFromServer = string.Empty;

            try
            {
                wResp = httpReq.GetResponse();
                Stream respStrm = wResp.GetResponseStream();
                strmRdr = new StreamReader(respStrm);
                responseFromServer = strmRdr.ReadToEnd();
                requestResponse.ResponseText = responseFromServer;
            }
            catch (Exception ex)
            {
                Core.Common.LogException(ex);
                Core.Common.LogActivity("|ApiCalExcp=" + ex.Message + ", url=" + url + ", data=" + postdata + "|" + ", encDATA=" + encStr + "|");
            }
            string decStr = crypto.Decrypt(responseFromServer, pass);
            Core.Common.LogActivity("\r\n url=" + url + ", \r\n data=" + postdata + "|" + ",\r\n xmlStr=" + xmlStr + "|" + ",\r\n encDATA=" + encStr + "|" + ",\r\n encResponse=" + responseFromServer + ",\r\n encResponse=" + responseFromServer + ",\r\n decResponse=" + decStr);
            requestResponse.ResponseText = decStr; //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");

            return decStr;


        }

        public string PostSteFront(string url, string postData, string contenttype, string accept, ref RequestResponseDto requestResponse, string userkey = "", string pass = "")
        {
            requestResponse.RequestTxt = "URL: " + url + ", DATA: " + postData;
            string responseFromServer = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add("api-key", userkey);
           
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
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

        public string GetSteFront(string url, ref RequestResponseDto requestResponse, string contenttype = "", string accept = "", string userkey = "", string pass = "")
        {
            string response = string.Empty;
          
                requestResponse.RequestTxt = "URL: " + url;

                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);
                if (url.Contains("planapi"))
                {
                    httpreq.Timeout = 2000;
                    httpreq.ReadWriteTimeout = 2000;

                }

                httpreq.Headers.Add("api-key", userkey);
               // httpreq.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                httpreq.ContentType = "application/x-www-form-urlencoded";
                using (HttpWebResponse httpres = (HttpWebResponse)httpreq.GetResponse())
                {
                    StreamReader sr = new StreamReader(httpres.GetResponseStream());
                    response = sr.ReadToEnd();
                    sr.Close();
                }

            requestResponse.ResponseText = response; //?.Replace("\n", "[N]")?.Replace("\r", "[R]")?.Replace("\t", "[T]");
            
            return response;

        }

        public string GetURL(string url)
        { 
            string response = string.Empty;
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);
               
                using (HttpWebResponse httpres = (HttpWebResponse)httpreq.GetResponse())
                {
                    StreamReader sr = new StreamReader(httpres.GetResponseStream());
                    response = sr.ReadToEnd();
                    sr.Close();
                }
                
            }
            catch (Exception ex)
            {
               
            }

            return response;

        }


        public string GetCyberPlatCallback(string url, ref RequestResponseDto requestResponse)
        {
            string resp = string.Empty;

            OpenSSL ssl = new OpenSSL();

            ssl.CERTNo = SiteKey.CyberPlatCert;
            string keyPath = HttpContext.Current.Server.MapPath("~/Certificates/myprivatekey.pfx");
            ssl.message = ssl.Sign_With_PFX(string.Empty, keyPath, SiteKey.CyberPlatPass);

            string sign = ssl.message;
            sign = sign.GetSubstring("BEGIN+SIGNATURE", "END+SIGNATURE");

            url = url.Replace("[SIGN]", sign);

            resp = Get(url, ref requestResponse);

            Core.Common.LogActivity("cyberplathit : cert=" + SiteKey.CyberPlatCert + ", keyPath=" + keyPath + ", pass=" + SiteKey.CyberPlatPass + "RequestTxt=" + requestResponse.RequestTxt + ", ssl.message=" + ssl.message + ", response:" + resp + ", signature:" + sign);


            return resp;

        }

      
        //public async Task<string> CashDepositPostApi(string url, string trantime, string imei, string requestFromPost)
        //{

        //    string responseFromServer = string.Empty;
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(url);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        string secKey = "d3b40b73a283db128f42a5eac50c1fe61c8d4ae91930e23922b741b15eadea21";                
        //        string hash = GenerateSha256Hash(Encoding.UTF8.GetBytes(requestFromPost + secKey));
        //        string sessionKey = GenerateSessionKey();
        //        string eskey = EncryptUsingPublicKey(Encoding.UTF8.GetBytes(sessionKey));
        //        string body = EncryptUsingSessionKey(Encoding.UTF8.GetBytes(eskey), Encoding.UTF8.GetBytes(requestFromPost));
        //        client.DefaultRequestHeaders.Add("trnTimestamp", trantime);
        //        client.DefaultRequestHeaders.Add("hash", hash);
        //        client.DefaultRequestHeaders.Add("deviceIMEI", imei);
        //        client.DefaultRequestHeaders.Add("eskey", eskey);
        //        var content = new StringContent(body, Encoding.UTF8, "application/json");
        //        var response = await client.PostAsync(url, content);
        //        string result = response.Content.ReadAsStringAsync().Result;
        //        return result;
        //    }
        //}
        //public string GenerateSha256Hash(byte[] message)
        //{
        //    try
        //    {
        //        IDigest digest = new Sha256Digest();
        //        digest.Reset();
        //        byte[] buffer = new byte[digest.GetDigestSize()];
        //        digest.BlockUpdate(message, 0, message.Length);
        //        digest.DoFinal(buffer, 0);
        //        return Convert.ToBase64String(buffer);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}
        //public string GenerateSessionKey()
        //{
        //    try
        //    {
        //        SecureRandom random = new SecureRandom();
        //        KeyGenerationParameters parameters = new KeyGenerationParameters(random, SYMMETRIC_KEY_SIZE);
        //        CipherKeyGenerator keyGenerator = GeneratorUtilities.GetKeyGenerator("AES");
        //        keyGenerator.Init(parameters);
        //        return Convert.ToBase64String(keyGenerator.GenerateKey());
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}
        //public void EncrypterReg(string publicKeyFileName)
        //{
        //    FileStream stream = null;
        //    try
        //    {
        //        byte[] fileBytes = File.ReadAllBytes(publicKeyFileName);
        //        PublicKey = new byte[fileBytes.Length];
        //        Array.Copy(fileBytes, PublicKey, fileBytes.Length);
        //        Org.BouncyCastle.X509.X509Certificate certificate = new X509CertificateParser().ReadCertificate(fileBytes);
        //        this.publicKey = (RsaKeyParameters)certificate.GetPublicKey();
        //        this.certExpiryDate = certificate.NotAfter;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Could not initialize encryption module", ex);
        //    }
        //    finally
        //    {
        //        if (stream != null)
        //        {
        //            try
        //            {
        //                stream.Close();
        //            }
        //            catch
        //            {
        //            }
        //        }
        //    }
        //}
        //public string EncryptUsingPublicKey(byte[] data)
        //{
        //    try
        //    {
        //        string Certificate = HttpContext.Current.Server.MapPath("~/Certificates/fingpay_public_production.cer");
        //        EncrypterReg(Certificate);
        //        IBufferedCipher cipher = CipherUtilities.GetCipher(ASYMMETRIC_ALGO);
        //        cipher.Init(true, this.publicKey);
        //        return Convert.ToBase64String(cipher.DoFinal(data));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}
        //public string EncryptUsingSessionKey(byte[] skey, byte[] data)
        //{
        //    try
        //    {

        //        PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(cipherEngine, padding);
        //        //AesEngine aese = new AesEngine();
        //        //PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(aese);
        //        cipher.Init(true, new KeyParameter(skey));
        //        byte[] sourceArray = new byte[cipher.GetOutputSize(data.Length)];
        //        int num2 = cipher.ProcessBytes(data, 0, data.Length, sourceArray, 0);
        //        int num3 = cipher.DoFinal(sourceArray, num2);
        //        byte[] destinationArray = new byte[num2 + num3];
        //        Array.Copy(sourceArray, 0, destinationArray, 0, destinationArray.Length);
        //        return Convert.ToBase64String(destinationArray);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}
    }
}