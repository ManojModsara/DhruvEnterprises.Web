using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using DhruvEnterprises.Data;

namespace DhruvEnterprises.Web.SmsApi
{
    public class SMSapiCall
    {
        #region "Method"
        public static DataTable SmsData()
        {
            DataTable dsData = new DataTable();
            SqlConnection conn;
            SqlDataAdapter sqlCmd;
            try
            {
                using (conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString))
                {
                    conn.Open();
                    sqlCmd = new SqlDataAdapter("smsapiactive", conn);
                    sqlCmd.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Fill(dsData);
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

                //Callmethod.LogExceptions(ex);
                dsData = null;
            }
            return dsData;
        }
        public static void SmsDataInsert(string smsid, string mobileno, string msg, string Response)
        {
            SqlConnection conn;
            try
            {
                using (conn = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand sqlCmd = new SqlCommand("Smsinsert", conn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@smsid", smsid);
                        sqlCmd.Parameters.AddWithValue("@Mobileno", mobileno);
                        sqlCmd.Parameters.AddWithValue("@message", msg);
                        sqlCmd.Parameters.AddWithValue("@Response", Response);
                        sqlCmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

                //Callmethod.LogExceptions(ex);
            }
        }
        public string SendSms(string Mobileno, string message)
        {
            string result = "";
            try
            {
                var dt = SmsData();
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int smsapi_id = Convert.ToInt32(dt.Rows[i]["Id"].ToString());
                        string Method = dt.Rows[i]["Method"].ToString();
                        string Smsurl = dt.Rows[i]["Url"].ToString();
                        string Parameter = dt.Rows[i]["Parameter"].ToString();
                        //status = dt.status
                        if (Method == "GET")
                        {
                            string apiurl = Smsurl.ToString();
                            apiurl = apiurl.Replace("[MMM]", Mobileno.Trim()).Replace("[TO]", Mobileno.Trim()).
                                            Replace("[MSG]", message).Replace("[MSG]", message);
                            if (Mobileno.Length == 10)
                            {
                                result = ApiCall(apiurl);
                            }
                            SMSData sMSData = new SMSData()
                            {
                                Mobileno = Mobileno,
                                Message = message,
                                Smsid = smsapi_id,
                                Response = result
                            };
                            SmsDataInsert(Convert.ToString(smsapi_id), Mobileno.Trim(), message, result);
                        }
                        else if (Method == "POST")
                        {
                            string apiurl = Smsurl.ToString();
                            apiurl = apiurl.Replace("[TO]", Mobileno.Trim()).Replace("[TO]", Mobileno.Trim()).
                                             Replace("[MS]", message).Replace("[MS]", message);
                            if (Mobileno.Length == 10)
                            {
                                result = PostApiCall(apiurl, Parameter);
                            }
                            SMSData sMSData = new SMSData()
                            {
                                Mobileno = Mobileno,
                                Message = message,
                                Smsid = smsapi_id,
                                Response = result
                            };
                            SmsDataInsert(Convert.ToString(sMSData.Smsid), Mobileno.Trim(), message, result);

                        }
                        else
                        {

                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

                return "FAILED";
            }
        }
        #endregion

        #region Get And Post Call Methods
        public static string ApiCall(string url)
        {
            HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                HttpWebResponse httpres = (HttpWebResponse)httpreq.GetResponse();
                StreamReader sr = new StreamReader(httpres.GetResponseStream());
                string results = sr.ReadToEnd();
                sr.Close();
                return results;
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);
                return "Processing";
            }

        }
        public static string PostApiCall(string url, string postData)
        {

            string json = "";

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                response.Close();
                return responseFromServer;
            }
            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);

            }
            return json;
        }
        #endregion
    }
}