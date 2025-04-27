using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.Web.LIBS
{
    public class TraceError
    {
        public static void ReportError(System.Exception exception, string ReportToEmail = "er.params@rediffmail.com", string ApplicationTitle = "")
        {
            if ((exception != null))
            {
                try
                {
                    FlexiMail objSendMail = new FlexiMail();

                    //Dim myEmail As System.Web.Mail.MailMessage = New System.Web.Mail.MailMessage()
                    string ErrorDesc = "";

                    var _with1 = exception;

                    ErrorDesc += "<H3>Error Occured " + (SiteKey.DomainName.Contains("localhost")|| SiteKey.DomainName.Contains("www.dev.") ? "Test" : "Live") + "</h3><hr>";
                    ErrorDesc += "<b>Date Time:</b><br>" + DateTime.Now.ToString() + "<br><br>";
                    ErrorDesc += "<b>Error Code: </b><br>" + exception.GetHashCode().ToString() + "<br><br>";
                    ErrorDesc += "<b>Base Exception: </b><br>" + exception.GetBaseException().ToString() + "<br><br>";
                    ErrorDesc += "<b>Type: </b><br>" + exception.GetType().ToString() + "<br><br>";
                    ErrorDesc += "<b>Inner Exception: </b><br>" + exception.InnerException + "<br><br>";
                    ErrorDesc += "<b>Message: </b><br>" + exception.Message + "<br><br>";
                    ErrorDesc += "<b>Source: </b><br>" + exception.Source + "<br><br>";
                    ErrorDesc += "<b>Stack Trace: </b><br>" + exception.StackTrace.ToString() + "<br><br>";
                    ErrorDesc += "<b>Generic Info: </b><br>" + exception.ToString() + "<br><br><hr>";


                    string strExtentedInfo = null;

                    //collect server variables
                    object x = null;
                    strExtentedInfo = "<b>Server variables ...</b><br>";
                    foreach (object x_loopVariable in System.Web.HttpContext.Current.Request.ServerVariables)
                    {
                        x = x_loopVariable;
                        try
                        {
                            strExtentedInfo += x + " = " + System.Web.HttpContext.Current.Request.ServerVariables[Convert.ToString(x)] + "<BR>";
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    strExtentedInfo += "<hr>";

                    //collect form collection
                    strExtentedInfo += "<b>Form collection ...</b><br>";
                    foreach (object x_loopVariable in System.Web.HttpContext.Current.Request.Form)
                    {
                        x = x_loopVariable;
                        try
                        {
                            strExtentedInfo += x + " = " + System.Web.HttpContext.Current.Request.Form[Convert.ToString(x)] + "<BR>";
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    strExtentedInfo += "<hr>";

                    strExtentedInfo += "<b>Session collection ...</b><br>";
                    foreach (object x_loopVariable in System.Web.HttpContext.Current.Session.Contents)
                    {
                        x = x_loopVariable;
                        try
                        {
                            strExtentedInfo += x + " = " + System.Web.HttpContext.Current.Session[Convert.ToString(x)] + "<BR>";
                        }
                        catch
                        {
                            continue;
                        }

                    }
                    strExtentedInfo += "<hr>";

                    //dump extended error info to error description
                    ErrorDesc += strExtentedInfo;


                    //report the error here
                    try
                    {
                        ErrorDesc = ErrorDesc + "<hr>" + exception.ToString();
                    }
                    catch
                    {
                    }

                    objSendMail.From = SiteKey.CompanyEmail;
                    objSendMail.To = ReportToEmail;
                    objSendMail.CC = "";
                    objSendMail.BCC = "";
                    objSendMail.Subject = "Exception in application - " + ApplicationTitle;
                    objSendMail.MailBodyManualSupply = true;
                    objSendMail.MailBody = ErrorDesc;
                  
                    objSendMail.Send();




                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}