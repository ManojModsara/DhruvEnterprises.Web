using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DhruvEnterprises.Web.Models.Others;
using static DhruvEnterprises.Core.Enums;
using System.Globalization;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;

namespace DhruvEnterprises.Web.Controllers
{
    public class OperatorSerialController : BaseController
    {
        // GET: OperatorSerial
        public ActionAllowedDto actionAllowedDto;
        private IOpSerialService repoOperatorSerial;
        ActivityLogDto activityLogModel;
        private IOperatorSwitchService operatorSwitchService;
        private IPackageService packageService;
        public OperatorSerialController(IPackageService _packageService, IOperatorSwitchService _operatorSwitchService, IActivityLogService _activityLogService, IOpSerialService _repoOperatorSerial, IRoleService _roleService) : base(_activityLogService, _roleService)
        {            
            this.repoOperatorSerial = _repoOperatorSerial;
            this.actionAllowedDto = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
            this.operatorSwitchService = _operatorSwitchService;
            this.packageService = _packageService;
        }
        public ActionResult Index()
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.Roles.FirstOrDefault());
            try
            {
                activityLogModel.ActivityName = "OperatorSerial Index REQUEST";
                activityLogModel.ActivityPage = "GET:OperatorSerial/Index/";
                activityLogModel.Remark = "";
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception e)
            {
                LogException(e);
            }
            return View();
        }
        [HttpPost]
        public ActionResult GetOpSerialList(DataTableServerSide model)
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.Roles.FirstOrDefault());

            int roleid = CurrentUser.Roles.FirstOrDefault();
            int userid = CurrentUser?.UserID ?? 0;
            KeyValuePair<int, List<OperatorSerial>> emails = repoOperatorSerial.GetOperatorSerials(model);
            return Json(new
            {
                model.draw,
                recordsTotal = emails.Key,
                recordsFiltered = emails.Key,
                data = emails.Value.Select(c => new List<object> {
                    c.Id,
                    c.Operator?.Name,
                    c.Circle?.CircleName,
                    c.Series,                   
                   (actionAllowedDto.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "OperatorSerial",new { id = c.Id })):string.Empty )
                    +"&nbsp;"+
                   (actionAllowedDto.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "deleteopserial","OperatorSerial", new { id = c.Id }),"modal-delete-OperatorSerial"):string.Empty)
                   , actionAllowedDto.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult CreateEdit(int? id)
        {
            UpdateActivity("OperatorSerial CreateEdit REQUEST", "GET:OperatorSerial/CreateEdit/", "ID=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            OperatorSerialDto operatorSerialdto = new OperatorSerialDto();
            ViewBag.Range10 = Enumerable.Range(1, 10).Select(x => new { Id = x, Name = x }).ToList();
            if (id.HasValue && id.Value > 0)
            {
                OperatorSerial opSerial = repoOperatorSerial.GetOperatorSerialById(id.Value);
                operatorSerialdto.Id = opSerial.Id;
                operatorSerialdto.CircleId = opSerial.CircleId??0;
                operatorSerialdto.OpId = opSerial.OpId??0;
                operatorSerialdto.Series = opSerial.Series;                
            }
            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name }).ToList();
            return View("createedit", operatorSerialdto);
        }

        [HttpPost]
        public ActionResult CreateEdit(OperatorSerialDto model, FormCollection FC)
        {
            UpdateActivity("OperatorSerial CreateEdit REQUEST", "POST:OperatorSerial/CreateEdit/", "ID=" + model.Id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);
            string message = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    bool CheckUserExists = repoOperatorSerial.SeriesExists(model.OpId, model.CircleId, model.Series);// check records in user table 
                    if (CheckUserExists == true)
                    {
                        ShowErrorMessage("Error!", "This Series Already Exists!",false);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        int roleid = CurrentUser.Roles.FirstOrDefault();
                        OperatorSerial opSerial = repoOperatorSerial.GetOperatorSerialById(model.Id) ?? new OperatorSerial();
                        opSerial.Id = model.Id;
                        opSerial.OpId = model.OpId;
                        opSerial.CircleId = model.CircleId;
                        opSerial.Series = model.Series;
                        repoOperatorSerial.Save(opSerial);
                        ShowSuccessMessage("Success!", "Series has been saved", false);
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Series already exist.";
                    ModelState.AddModelError("error", message);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ModelState.AddModelError("error", message);
                }
            }
    return View();
        }




       
        public ActionResult CreateEditByExcel(int? id)
        {
            UpdateActivity("OperatorSerial CreateEditByExcel REQUEST", "GET:OperatorSerial/CreateEditByExcel/", "ID=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            return View("CreateEditByExcel");
        }

        [HttpPost]
        public ActionResult CreateEditByExcel(OperatorSerialExcelDto model, FormCollection FC)
        {
            UpdateActivity("OperatorSerial CreateEditByExcel REQUEST", "POST:OperatorSerial/CreateEditByExcel/", "ID=" + model.Id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);
            string message = string.Empty;
            string error = "";
            string errordesc = "";
            string log = "UploadOperatorSerialExcel start";
            string response = ",";
            int rowno = 0, success = 0, failed = 0;
            try
            {
                string expfilepath = System.Web.HttpContext.Current.Server.MapPath("~/ExceptionLog/");  //Text File Path
                    string uploadfolder = System.Configuration.ConfigurationManager.AppSettings["UploadPath"] + "OperatorSerialLoadExcel/";
                    string extname = model.UploadedFile.FileName.EndsWith(".xls") ? ".xls" :
                                    model.UploadedFile.FileName.EndsWith(".xlsx") ? ".xlsx" :
                                   model.UploadedFile.FileName.EndsWith(".ods") ? ".ods" : string.Empty;
                    if (string.IsNullOrEmpty(extname))
                    {
                        ShowErrorMessage("Error!", "Invalid File Format.(Allowed Only .xls, .xlsx, .ods Format)", false);
                    }
                    else
                    {
                        DateTime purdate = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        string fileName = "UploadOperatorSerialExcel" + "_D" + purdate.ToString("ddMMyyyy") + "_U" + CurrentUser.UserID + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + extname;
                        string path = Server.MapPath(uploadfolder + fileName);
                        if (!Directory.Exists(Server.MapPath(uploadfolder)))
                        {
                            Directory.CreateDirectory(Server.MapPath(uploadfolder));
                        }
                        model.UploadedFile.SaveAs(path);
                        string excelConStr = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
                        OleDbConnection excelCon = new OleDbConnection(excelConStr);
                        //Sheet Name
                        excelCon.Open();
                        var table = excelCon.GetSchema("Tables");
                        string tableName = excelCon.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
                        excelCon.Close();
                        //End
                        string sqlquery = "select  * FROM [" + tableName + "] "; 
                        OleDbCommand olecmd = new OleDbCommand(sqlquery, excelCon);
                        excelCon.Open();
                        OleDbDataReader dReader;
                        dReader = olecmd.ExecuteReader();
                        while (dReader.Read())
                        {
                            rowno++;
                            using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                            {

                            SqlCommand cmd = new SqlCommand("sp_UploadOperatorSeries", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@OpId", dReader.GetValue(0));
                            cmd.Parameters.AddWithValue("@CircleId", dReader.GetValue(1));
                            cmd.Parameters.AddWithValue("@Series", dReader.GetValue(2));                            
                            cmd.Parameters.Add("@Error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                            cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                            error = Convert.ToString(cmd.Parameters["@Error"].Value);
                                errordesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                                var splog = cmd.Parameters["@Log"].Value;
                                log += "\r\n, splog=" + splog;
                                if (error == "0")
                                {
                                    success++;
                                    response = response + "-Success),";
                                }
                                else
                                {
                                    failed++;
                                    response = response + "-Failed),";
                                }
                            }

                        }

                        ShowSuccessMessage("Success!", "Successfully Imported (" + success + "/" + rowno + "), DETAILS=" + response, false);

                    }
                }
            catch (Exception ex)
            {
                ShowErrorMessage("OOPS!", "Something Went Wrong. RESPONSE=" + response, false);
                LogException(ex, "Import Operator Series statement Excel response=" + response);
            }

            LogActivity(log);
                 
            return View();
        }


        public ActionResult DownLoadInExcel(int? id)
        {
            UpdateActivity("OperatorSerial CreateEditByExcel REQUEST", "GET:OperatorSerial/CreateEditByExcel/", "ID=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName }).ToList();
            return View("DownLoadInExcel");
        }




        [HttpGet]
        public ActionResult DeleteOpSerial(int id)
        {
            UpdateActivity("Delete OperatorSerial", "GET:OperatorSerial/DeleteOpSerial/", "id=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, id > 0 ? 3 : 2);           
            var opseries = repoOperatorSerial.GetOperatorSerialById(id);
            return PartialView("_ModalDelete", new Modal
            {
                Message = "Are You Sure To Delete?",
                Size = ModalSize.Small,
                Header = new ModalHeader { Heading = "Delete" },
                Footer = new ModalFooter { SubmitButtonText = "Yes", CancelButtonText = "No" }
            });

        }

        [HttpPost]
        [ActionName("DeleteOpSerial")]
        public ActionResult DeleteOpserial(int id)
        {
            UpdateActivity("Delete DeleteOpserial", "POST:OperatorSerial/DeleteOpserial/", "id=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("OperatorSerial", CurrentUser.RoleId, id > 0 ? 3 : 2);
            try
            {
                repoOperatorSerial.Delete(id);
                ShowSuccessMessage("Success", "Deleted", false);
            }
            catch (Exception)
            {
                ShowErrorMessage("Error Occurred", "", false);
            }
            return RedirectToAction("Index");
        }
        private void UpdateActivity(string name, string page, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = name;
                activityLogModel.ActivityPage = page;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }


    }
}