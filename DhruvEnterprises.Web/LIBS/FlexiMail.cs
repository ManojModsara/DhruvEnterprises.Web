using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Net.Mail;

namespace DhruvEnterprises.Web.LIBS
{
    public class FlexiMail
    {
        #region Constructors-Destructors
        public FlexiMail()
        {
            //set defaults 

            myEmail = new System.Net.Mail.MailMessage();
            _MailBodyManualSupply = false;
        }
        //public void Dispose()
        //{
        //    base.Finalize();
        //    myEmail.Dispose();
        //}
        #endregion

        #region  Class Data
        private string _From;
        // private string _FromList;
        private string _FromName;
        private string _To;
        private string _ToList;
        private string _Subject;
        private string _CC;
        private string _CCList;
        private string _BCC;
        private string _TemplateDoc;
        private string[] _ArrValues;
        private string _BCCList;
        private bool _MailBodyManualSupply;
        private string _MailBody;
        //private string _Attachment;
        private string[] _Attachment;
        private System.Net.Mail.MailMessage myEmail;

        #endregion

        #region Propertie
        public string From
        {
            set { _From = value; }
        }
        public string FromName
        {
            set { _FromName = value; }
        }
        public string To
        {
            set { _To = value; }
        }
        public string Subject
        {
            set { _Subject = value; }
        }
        public string CC
        {
            set { _CC = value; }
        }
        public string BCC
        {

            set { _BCC = value; }
        }
        public bool MailBodyManualSupply
        {

            set { _MailBodyManualSupply = value; }
        }
        public string MailBody
        {
            set { _MailBody = value; }
        }
        public string EmailTemplateFileName
        {
            //FILE NAME OF TEMPLATE ( MUST RESIDE IN ../EMAILTEMPLAES/ FOLDER ) 
            set { _TemplateDoc = value; }
        }
        public string[] ValueArray
        {
            //ARRAY OF VALUES TO REPLACE VARS IN TEMPLATE 
            set { _ArrValues = value; }
        }

        public string[] AttachFile
        {
            set { _Attachment = value; }
        }

        #endregion

        #region SEND EMAIL

        public void Send()
        {
            myEmail.IsBodyHtml = true;

            //set mandatory properties 
            if (_FromName == "")
                _FromName = _From;
            myEmail.From = new MailAddress(_From, _FromName);
            myEmail.Subject = _Subject;


            //---Set recipients in To List 
            _ToList = _To?.Replace(";", ",");
            if (!string.IsNullOrEmpty(_ToList))
            {
                string[] arr = _ToList.Split(',');
                myEmail.To.Clear();
                if (arr.Length > 0)
                {
                    foreach (string address in arr)
                    {
                        myEmail.To.Add(new MailAddress(address));
                    }
                }
                else
                {
                    myEmail.To.Add(new MailAddress(_ToList));
                }
            }

            //---Set recipients in CC List 
            _CCList = _CC?.Replace(";", ",");
            if (!string.IsNullOrEmpty(_CCList))
            {
                string[] arr = _CCList.Split(',');
                myEmail.CC.Clear();
                if (arr.Length > 0)
                {
                    foreach (string address in arr)
                    {
                        myEmail.CC.Add(new MailAddress(address));
                    }
                }
                else
                {
                    myEmail.CC.Add(new MailAddress(_CCList));
                }
            }

            //---Set recipients in BCC List 
            _BCCList = _BCC?.Replace(";", ",");
            if (!string.IsNullOrEmpty(_BCCList))
            {
                string[] arr = _BCCList.Split(',');
                myEmail.Bcc.Clear();
                if (arr.Length > 0)
                {
                    foreach (string address in arr)
                    {
                        myEmail.Bcc.Add(new MailAddress(address));
                    }
                }
                else
                {
                    myEmail.Bcc.Add(new MailAddress(_BCCList));
                }
            }

            //set mail body 
            if (_MailBodyManualSupply)
            {
                myEmail.Body = _MailBody;
            }
            else
            {
                myEmail.Body = GetHtml(_TemplateDoc);
                //& GetHtml("EML_Footer.htm") 
            }

            // set attachment 
            if (_Attachment != null)
            {
                for (int i = 0; i < _Attachment.Length; i++)
                {
                    if (_Attachment[i] != null)
                        myEmail.Attachments.Add(new Attachment(_Attachment[i]));
                }

            }
            
            SmtpClient client = new SmtpClient();
            client.Host = SiteKey.Host;
           // client.Credentials = new System.Net.NetworkCredential("email", "password");
            client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["UserName"], ConfigurationManager.AppSettings["Password"]);
            client.Port = Convert.ToInt32(SiteKey.Port);// 587;
            client.EnableSsl = true;
            client.Send(myEmail); 

        }


        public void Send(string username, string passkey)
        {
            myEmail.IsBodyHtml = true;

            //set mandatory properties 
            if (_FromName == "")
                _FromName = _From;
            myEmail.From = new MailAddress(_From, _FromName);
            myEmail.Subject = _Subject;


            //---Set recipients in To List 
            _ToList = _To.Replace(";", ",");
            if (_ToList != "")
            {
                string[] arr = _ToList.Split(',');
                myEmail.To.Clear();
                if (arr.Length > 0)
                {
                    foreach (string address in arr)
                    {
                        myEmail.To.Add(new MailAddress(address));
                    }
                }
                else
                {
                    myEmail.To.Add(new MailAddress(_ToList));
                }
            }

            //---Set recipients in CC List 
            _CCList = _CC.Replace(";", ",");
            if (_CCList != "")
            {
                string[] arr = _CCList.Split(',');
                myEmail.CC.Clear();
                if (arr.Length > 0)
                {
                    foreach (string address in arr)
                    {
                        myEmail.CC.Add(new MailAddress(address));
                    }
                }
                else
                {
                    myEmail.CC.Add(new MailAddress(_CCList));
                }
            }

            //---Set recipients in BCC List 
            _BCCList = _BCC.Replace(";", ",");
            if (_BCCList != "")
            {
                string[] arr = _BCCList.Split(',');
                myEmail.Bcc.Clear();
                if (arr.Length > 0)
                {
                    foreach (string address in arr)
                    {
                        myEmail.Bcc.Add(new MailAddress(address));
                    }
                }
                else
                {
                    myEmail.Bcc.Add(new MailAddress(_BCCList));
                }
            }

            //set mail body 
            if (_MailBodyManualSupply)
            {
                myEmail.Body = _MailBody;
            }
            else
            {
                myEmail.Body = GetHtml(_TemplateDoc);
                //& GetHtml("EML_Footer.htm") 
            }

            // set attachment 
            if (_Attachment != null)
            {
                for (int i = 0; i < _Attachment.Length; i++)
                {
                    if (_Attachment[i] != null)
                        myEmail.Attachments.Add(new Attachment(_Attachment[i]));
                }

            }
            SmtpClient client = new SmtpClient();
            client.Host = SiteKey.Host;
            client.EnableSsl = true;
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential(username, passkey);
            client.Port = 587;
            client.Send(myEmail);  

        }
        #endregion

        #region GetHtml
        public string GetHtml(string argTemplateDocument)
        {
            int i;
            StreamReader filePtr;
            string fileData = argTemplateDocument;

            filePtr = File.OpenText(HttpContext.Current.Server.MapPath("~/EmailTemplate/") + argTemplateDocument);
            //filePtr = File.OpenText(ConfigurationSettings.AppSettings["EMLPath"] + argTemplateDocument);
            fileData = filePtr.ReadToEnd();


            filePtr.Close();
            filePtr = null;
            if ((_ArrValues == null))
            {

                return fileData;
            }
            else
            {
                //fileData = fileData.Replace("##user##", _ArrValues[0].ToString());
                //fileData = fileData.Replace("##question##", _ArrValues[1].ToString());

                for (i = _ArrValues.GetLowerBound(0); i <= _ArrValues.GetUpperBound(0); i++)
                {

                    fileData = fileData.Replace("@v" + i.ToString() + "@", (string)_ArrValues[i]);
                }
                return fileData;
            }


        }
        #endregion

    }
}
