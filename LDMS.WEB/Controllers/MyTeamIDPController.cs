using LDMS.Core;
using LDMS.Identity;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class MyTeamIDPController : BaseController
    {
        private readonly MyTeamIDPService _MyTeamIDPService;
        private readonly IDPService _IDPService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<MyTeamIDPController> _logger;
        public MyTeamIDPController(ILogger<MyTeamIDPController> logger, MyTeamIDPService MyTeamIDPService, IWebHostEnvironment environment)
        {
            _logger = logger;
            _MyTeamIDPService = MyTeamIDPService;
            _hostingEnvironment = environment;
        }
        //public MyTeamIDPController(ILogger<MyTeamIDPController> logger, MyTeamIDPService MyTeamIDPService, IWebHostEnvironment environment, IDPService iDPService)
        //{
        //    _logger = logger;
        //    _MyTeamIDPService = MyTeamIDPService;
        //    _hostingEnvironment = environment;
        //    _IDPService = iDPService;
        //}
        public IActionResult Index()
        {
           // var MyTeamIDP = MyTeamIDPService.GetMy_Team_IDP();
            return View();
        }
        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("MyTeamIDP/EmployeeSearch")]
        [AutoValidateAntiforgeryToken]
        public IActionResult EmployeeSearch(string EmployeeID, string EmployeeName, string DepartmentID, string DivisionID, string CenterID, string PlantID)
        {

            //string sortOrder = Request.Form["order[0][dir]"];
            //string sortIndex = Request.Form["order[0][column]"];

            //string sortColumn = "";
            //if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            ViewModels.SearchModel.LDMS_M_IDP_Employee_Search criteria = new ViewModels.SearchModel.LDMS_M_IDP_Employee_Search();
            //criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            //criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.EmployeeID = EmployeeID;
            criteria.EmployeeName = EmployeeName;
            criteria.DepartmentID = DepartmentID;
            criteria.DivisionID = DivisionID;
            criteria.CenterID = CenterID;
            criteria.PlantID = PlantID;

            var employees = _MyTeamIDPService.GetEmployee(criteria);
            //ViewData["Instructor"] = instructor.Results;
            return Json(employees);
        }
        //[AuthorizeRole(UserRole.All)]
        //[HttpGet]
        //[Route("MyTeamIDP/MyTeamDetail/{i?}")]
        //// [AutoValidateAntiforgeryToken]
        //public IActionResult MyTeamIDPDetail(string ID_Employee)
        //{

        //  //  var myIDP = _MyTeamIDPService.GetMyIDP();

        //    return View("/Views/MyTeamIDP/MyTeamIDPDetail.cshtml");

        //}
        public IActionResult MyTeamIDPDetail()
        {
            // var MyTeamIDP = MyTeamIDPService.GetMy_Team_IDP();
            return View();
        }
        public IActionResult _EmployeeSearch()
        {
            // var MyTeamIDP = MyTeamIDPService.GetMy_Team_IDP();
            return View();
        }
        public IActionResult Search(string ID_Plant, string ID_Center, string ID_Division, string ID_Department, string ID_Status, string ID_Employee)
        {
            ViewModels.Paging_Result ret = new ViewModels.Paging_Result();
            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_T_IDP_Master criteria = new LDMS_T_IDP_Master();
            
            criteria.ID_Plant = ID_Plant;
            criteria.ID_Center = ID_Center;
            criteria.ID_Division = ID_Division;
            criteria.ID_Department = ID_Department;
            criteria.ID_Status = ID_Status;
            criteria.EmployeeID = ID_Employee;
            criteria.ID_Manager = JwtManager.Instance.GetUserId(HttpContext.Request);

            var myTeamIDP = _MyTeamIDPService.GetMy_Team_IDP(criteria);

            return Json(myTeamIDP);
        }
    }
    

       

}