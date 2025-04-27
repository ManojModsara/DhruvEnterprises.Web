using System.Web.Mvc;

namespace DhruvEnterprises.Web
{
    public static class DataTableButton
    {

        /// <summary>
        /// Edit button to pass additional parameters in url and open popup in same window.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <param name="targetModalId"></param>
        /// <returns></returns>
        public static string EditButton(string actionUrl, string targetModalId)
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='Edit'> <i class='fa fa-edit'></i></a>";
        }

        public static string LinkButton(string actionUrl, string targetModalId, string linkTxt, string title)
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "'  class='btn btn-link' title='"+title+"' ></a>";
        }

        public static string HyperLink(string actionUrl, string targetModalId, string linkTxt, string title, string color="")
        {
            if (string.IsNullOrEmpty(color))
            {
                return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' title='" + title + "' >"+(!string.IsNullOrEmpty(linkTxt)? linkTxt:string.Empty) + "</a>";
            }
            else
            {
                return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' title='" + title + "' style='color:"+color+ ";font-weight:bold;' >" + (!string.IsNullOrEmpty(linkTxt) ? linkTxt : string.Empty) + "</a>";
            }
           
        }

        /// <summary>
        /// Edit button which will redirect to different page.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <returns></returns>
        public static string EditButton(string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='Edit'> <i class='fa fa-edit'></i></a>";
        }
        public static string ImgButton(string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='Image'> <i class='fa fa-picture-o'></i></a>";
        }
        public static string ValidateButton(string actionUrl, string targetModalId,string displayName="Update")
        { 
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-bitbucket grid-btn btn-sm' title='" + displayName + "'  > <i class='fa fa-check'></i></a>";
        }

        /// <summary>
        /// Edit button which will redirect to different page.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <returns></returns>
        public static string ViewButton(string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' target='_blank' title='View'><i class='fa fa-eye'></i></a>";
        }

        /// <summary>
        /// Delete button to pass additional parameters in url and open popup in same window.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <param name="targetModalId"></param>
        /// <returns></returns>
        public static string DeleteButton(string actionUrl, string targetModalId, string displayName = "Delete")
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-danger grid-btn btn-sm' title='" + displayName + "'><i class='fa fa-trash-o'></i></a>";
        }
        public static string View(string actionUrl, string targetModalId, string displayName = "Delete")
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='" + displayName + "'><i class='fa fa-eye'></i></a>";
        }

        public static string ComplaintButton(string actionUrl, string targetModalId, string title = "Generate Complaint")
        { 
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-success grid-btn btn-sm' title='" + title + "'><i class='fa fa-plus'></i></a>";
        }
        public static string ComplaintReOpenButton(string actionUrl, string targetModalId, string title = "Generate Complaint")
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='" + title + "'><i class='fa fa-plus'></i></a>";
        }

        public static string ExtLinkRedButton(string actionUrl, string tiltle = "Look-Up")
        { 
            return "<a href='" + actionUrl + "' class='btn btn-danger grid-btn btn-sm' target='_blank' title='"+ tiltle + "'><i class='fa fa-external-link'></i></a>";
            
        }

        /// <summary>
        /// Insert button to pass additional parameters in url and open popup in same window.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <param name="targetModalId"></param>
        /// <returns></returns>
        public static string InsertButton(this HtmlHelper htmlHelper, string actionUrl, string targetModalId)
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-success'>Add new <i class='fa fa-plus'></i></a>";
        }
        
        /// <summary>
        /// Insert button to pass additional parameters in url and open popup in same window.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <param name="targetModalId"></param>
        /// <returns></returns>
        public static string AddWalletButton(this HtmlHelper htmlHelper, string actionUrl, string targetModalId)
        { 
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-success'>Add Wallet <i class='fa fa-plus'></i></a>";
        }
        
        public static string ViewButton(string actionUrl, string targetModalId)
        {
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='View'><i class='fa fa-eye'></i></a>";
        }
        
        public static string ListButton(string actionUrl, string title, string displayText="")
        { 
            return "<a href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='" + title + "'>"+ displayText + "<i class='fa fa-eye'></i></a>";
        }

        public static string CheckBox(string Id, string Val, string Class)  
        {
            return "<input type='checkbox' id='" + Id + "' name='" + Id+"' value='"+ Val + "' class='"+ Class + "' />";
        }
        
        /// <summary>
        /// Insert button to pass additional parameters in url and open page in new window.
        /// </summary>
        /// <param name="actionUrl"></param>
        /// <param name="targetModalId"></param>
        /// <returns></returns>
        public static string InsertButton(this HtmlHelper htmlHelper, string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-success'>Add new <i class='fa fa-plus'></i></a>";
        }
        public static string UploadExcel(this HtmlHelper htmlHelper, string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-default'>Upload By Excel <i class='fa fa-file-excel-o'></i></a>";
        }
        public static string DownloadExcel(this HtmlHelper htmlHelper, string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-default'>Download Excel <i class='fa fa-file-excel-o'></i></a>";
        }
        public static string AddImageButton(string actionUrl)
        {
            return "<a href='" + actionUrl + "' class='btn btn-success grid-btn btn-sm'>Add/Edit Images <i class='fa fa-file-picture'></i></a>";
        }

        public static string SettingButton(string actionUrl, string text, string cls= "success")
        {
            return "<a href='" + actionUrl + "' class='btn btn-"+ cls + " grid-btn btn-sm' title='" + text + "'>  <i class='fa fa-cog'></i></a>";
        }
        
        public static string RefreshVbalButton(string actionUrl, string title = "Refresh")
        {
            return "<a class='btn btn-success' title='" + title + "' onclick='GetBalance(this)' > <i class='fa fa-refresh'></i></a>";
        }

        public static string RefreshAllButton(this HtmlHelper htmlHelper, string actionUrl, string title = "Refresh All")
        {
            return "<a href='" + actionUrl + "' title='" + title + "' class='btn btn-success'> Refresh All <i class='fa fa-refresh'></i></a>";
        }

        public static string RefreshButton(string actionUrl, string title = "Refresh")
        { 
            return "<a href='" + actionUrl + "' class='btn btn-success' title='" + title + "' > <i class='fa fa-refresh'></i></a>";
        }

        public static string AddButton(string actionUrl, string id, string text = "Add", string title="Add")
        { 
            return "<a id='"+id+"' href='" + actionUrl + "' class='btn btn-primary grid-btn btn-sm' title='"+title+"'> "+ text + "<i class='fa fa-plus'></i></a>";
        }

        public static string PlusButton(string actionUrl, string id, string title = "Add", string displayText = "") 
        { 
            return "<a id='"+id+"' href='" + actionUrl + "' class='btn btn-success grid-btn btn-sm' title='" + title+"'>"+ displayText + "<i class='fa fa-plus'></i></a>";
        }

        public static string PlusButton2(string actionUrl, string targetModalId, string title = "Add", string displayText = "")
        { 
            return "<a data-toggle='modal' data-target='#" + targetModalId + "' href='" + actionUrl + "' class='btn btn-success grid-btn btn-sm' title='" + title + "'>"+ displayText + "<i class='fa fa-plus'></i></a>";
        }

        public static string ButtonJS(string onclick, string icon, string btnType, string title = "", string displayName = "")
        {  
            
            return "<a class='btn btn-"+btnType+"' title='" + title + "' "+(!string.IsNullOrEmpty(onclick)? " onclick='" + onclick + "'" : "")+" > "+displayName+"<i class='fa fa-"+icon+"'></i></a>";
        }

        public static string ButtonDownload(string path, string displayName = "", string title = "Download")
        {
            return "<a  title='" + title + "' href='"+ path + "' download=''> " + displayName + "</a>";
        }
        public static string Label(string color, string text, string url = "#")
        {
            return "<a  href='" + url + "' title='Status' style='color:" + color + ";font-weight:bold;' >" + text + " </a>";
        }
    }
}