using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class DeveloperController : BaseController
    {
        #region "Fields"

        private readonly IOperatorSwitchService operatorswitchService;
        private readonly IRoleService roleService;
        #endregion
        #region "Constructor"
        public DeveloperController(IOperatorSwitchService _operatorswitchService,
            IRoleService _userroleService,
            IActivityLogService _activityLogService
            ) : base(_activityLogService, _userroleService)
        {
            this.operatorswitchService = _operatorswitchService;
            this.roleService = _userroleService;
        }
        #endregion
        // GET: Developer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RechargeApiDOC()
        {
            return View();
        }

        public ActionResult OperatorList()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetOperatorSwitch(DataTableServerSide model)
        {
            KeyValuePair<int, List<Operator>> operatorl = operatorswitchService.GetOperatorSwitch(model);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = operatorl.Key,
                recordsFiltered = operatorl.Key,
                data = operatorl.Value.Select(c => new List<object> {
                    c.Id,
                    c.Name
                    })
            }, JsonRequestBehavior.AllowGet);
        }

        
    }
}