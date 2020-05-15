using LDMS.Core;
using LDMS.Identity;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class MyTeamIDPController : Controller
    {
        private readonly MyTeamIDPService _MyTeamIDPService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<MyTeamIDPController> _logger;
        public MyTeamIDPController(ILogger<MyTeamIDPController> logger, MyTeamIDPService MyTeamIDPService, IWebHostEnvironment environment)
        {
            _logger = logger;
            _MyTeamIDPService = MyTeamIDPService;
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
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