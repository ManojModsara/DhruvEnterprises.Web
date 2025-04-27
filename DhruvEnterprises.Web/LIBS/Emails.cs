using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Configuration;
using DhruvEnterprises.Service;

namespace DhruvEnterprises.Web.LIBS
{
    public class Emails
    {
        private IEmailApiService emailApiService;
        public Emails(IEmailApiService _emailApiService)
        {
            
            this.emailApiService = _emailApiService;
           
        }

        public static void SendEmailTask(string EmailTo, string EmailCC, string Comment, string Ename)
        {
            try
            {
                string Subject = " Urgent Task Comment :: " + DateTime.Now.ToString("MMM, dd yyyy hh:mm tt");
                // string BodyContent = String.Format("Email ID#{0}", EmailTo);
                StringBuilder BodyContent = new StringBuilder();
                BodyContent.Append("<table style='width:100%;font-family:Arial; font-size:13px;' cellpadding='0' cellspacing='0'>");
                BodyContent.Append("<tr><td>Dear&nbsp;<b>" + Ename + "</b>,<br/><br/></td></tr>");
                BodyContent.Append("<tr><td><Br/></td></tr>");
                BodyContent.Append("<tr><td>Please accelerate this task&nbsp;" + Comment + "&nbsp;and complete in given time period.</td></tr><Br/>");
                BodyContent.Append("<tr><td><Br/><Br/></td></tr>");
                BodyContent.Append("<tr><td>Thanks & Regards<Br/>" + SiteSession.SessionUser.Name + "</td></tr>");
                BodyContent.Append("<tr><td>&nbsp;</td></tr>");
                //BodyContent.Append("<tr><td><span style='font-weight:bold; font-family:Arial; font-size:11px; font-style:italic; color:gray;'>[Note : Auto-generated email from internal system, Please do not reply to this email.]</span></td></tr>");
                BodyContent.Append("<tr><td><span style='font-weight:bold; font-family:Arial; font-size:11px; font-style:italic; color:gray;'>[Note : Please do not reply to this automated message; replies to this message are undeliverable. Contact your senior for any query.]</span></td></tr>");
                BodyContent.Append("</table>");

                FlexiMail objMail = new FlexiMail
                {
                    From = SiteKey.From,
                    // From=EmailCC,
                    To = EmailTo,
                    CC = EmailCC,
                    BCC = "",
                    Subject = Subject,
                    MailBodyManualSupply = true,
                    MailBody = BodyContent.ToString()

                };

                objMail.Send();
            }
            catch { }
        }
        public void SendEmail(string MessageBody = "", string mailto = "", string Subject = "")
        {
            try
            {
                #region "Activation Code"
                //Value Array: v1 = Name, v2 = domain , v3 = guid,
                var emails = emailApiService.EmailApiList();
                FlexiMail objMail = new FlexiMail
                {
                    From = emails.FromAddress,
                    To = mailto,
                    CC = "",
                    BCC = "",
                    Subject = MessageBody,
                    MailBodyManualSupply = true,
                    ValueArray = new string[]  {
                                                    mailto,
                                                    SiteKey.DomainName,
                                                    Subject
                                                    }
                };
                objMail.MailBody = objMail.GetHtml("Activation.html");
                objMail.Send(emails.UserName, emails.Password);
                #endregion
            }
            catch (Exception ex)
            {
            }

        }

    }
}