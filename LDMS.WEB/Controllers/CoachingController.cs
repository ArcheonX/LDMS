using LDMS.Core;
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
    public class CoachingController : Controller
    {
        private readonly CoachingService _CoachingService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<CoachingController> _logger;

        public CoachingController(ILogger<CoachingController> logger, CoachingService coachingService, IWebHostEnvironment environment)
        {
            _logger = logger;
            _CoachingService = coachingService;
            _hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EIndex()
        {
            return View("/Views/Coaching/E_Index.cshtml");
        }

        public IActionResult Detail()
        {
            return View("/Views/Coaching/Detail.cshtml");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Coaching/EDetail/{ID}")]
        public IActionResult EDetail(string ID)
        {
            var coaching = _CoachingService.GetCoachingByID(ID);

            return View("/Views/Coaching/E_Detail.cshtml", coaching);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Coaching/Detail/{ID}")]
        public IActionResult Detail(string ID)
        {
            var coaching = _CoachingService.GetCoachingByID(ID);

            return View("/Views/Coaching/Detail.cshtml", coaching);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Coaching/UpdateCoaching")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult UpdateCoaching(string ID_Coaching, string Topic, string CoachingStatus)
        {
            LDMS_T_Coaching coaching = new LDMS_T_Coaching();
            coaching = _CoachingService.UpdateCoachingDetail(ID_Coaching, Topic, CoachingStatus);

            return Json(coaching);
        }


        [HttpPost]
        [Route("Coaching/Search")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Search(string ID_Plant, string ID_Center, string ID_Division, string ID_Department, string ID_Status, string ID_Employee, string ID_Platform, bool IS_Employee)
        {

            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_T_CoachingSearch criteria = new LDMS_T_CoachingSearch();
            criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.SortField = sortColumn;
            criteria.SortOrder = sortOrder;
            criteria.ID_Plant = ID_Plant;
            criteria.ID_Center = ID_Center;
            criteria.ID_Division = ID_Division;
            criteria.ID_Department = ID_Department;
            criteria.ID_Status = ID_Status;
            criteria.ID_Employee = ID_Employee;
            criteria.ID_Platform = ID_Platform;

            var coaching = _CoachingService.GetCoaching(criteria, IS_Employee);
           
            return Json(coaching);
        }



        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Coaching/InsertReviewHead")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InsertReviewHead(string ID_Coaching, string EmployeeReport, string Topic, IFormFile fileAttach)
        {

            string attachFile = "";
            string attachName = "";
            if (fileAttach != null)
            {
                var fileName = ContentDispositionHeaderValue.Parse(fileAttach.ContentDisposition).FileName.Replace("\"", "");
                string filesPath = _hostingEnvironment.WebRootPath + "\\Uploads\\Coaching\\" + ID_Coaching;

                fileName = fileName.Contains("\\")
                    ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName.Trim('"');
                fileName = ID_Coaching + "_" + fileName;

                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

                var fullFilePath = Path.Combine(filesPath, fileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    fileAttach.CopyTo(stream);
                }

                attachFile = Path.Combine("\\Uploads\\Coaching\\" + ID_Coaching, fileName);
                attachName = fileName;
            }
           

            LDMS_T_Coaching c = new LDMS_T_Coaching();
            c = _CoachingService.UpdateCoachingDetail(ID_Coaching, Topic, "30"); // On Progress

            LDMS_T_CoachingReviewHead coaching = new LDMS_T_CoachingReviewHead();
           
            int ret = _CoachingService.CreateCoachingHead( ID_Coaching,  EmployeeReport, attachFile, attachName);
            coaching.ID = ret;
            coaching.ID_Coaching = int.Parse(ID_Coaching);
            coaching.EmployeeReport = EmployeeReport;
            coaching.AttachFilePath = attachFile;
            coaching.AttachFileName = attachName;

            return Json(coaching);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Coaching/InsertReviewDetail")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InsertReviewDetail(string ID_CoachingReviewHead, string PostDetail, string ID_Coaching, IFormFile fileAttach)
        {

            //LDMS_T_CoachingReviewDetail coaching = new LDMS_T_CoachingReviewDetail();

            string attachFile = "";
            string attachName = "";
            if (fileAttach != null)
            {
                var fileName = ContentDispositionHeaderValue.Parse(fileAttach.ContentDisposition).FileName.Replace("\"", "");
                string filesPath = _hostingEnvironment.WebRootPath + "\\Uploads\\Coaching\\" + ID_Coaching + "\\" + ID_CoachingReviewHead;

                fileName = fileName.Contains("\\")
                    ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName.Trim('"');
                fileName = ID_Coaching + "_" + ID_CoachingReviewHead + "_" + fileName;

                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

                var fullFilePath = Path.Combine(filesPath, fileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    fileAttach.CopyTo(stream);
                }

                attachFile = Path.Combine("\\Uploads\\Coaching\\" + ID_Coaching + "\\" + ID_CoachingReviewHead, fileName);
                attachName = fileName;
            }
            

            int coaching = _CoachingService.CreateCoachingDetail(ID_CoachingReviewHead, PostDetail, attachFile, attachName);

            return Json(ID_Coaching);
        }

        [HttpPost]
        [Route("Coaching/GetCoachingHead")]
        public IActionResult GetCoachingHead(string ID_Coaching)
        {
            var coachingHead = _CoachingService.GetCoachingReviewHead(ID_Coaching);

            return Json(coachingHead);
        }

        [HttpPost]
        [Route("Coaching/GetReviewDetail")]
        public IActionResult GetReviewDetail(string ID_CoachingReviewHead)
        {
            List<LDMS_T_CoachingReviewDetail> list = _CoachingService.GetCoachingReviewDetail(ID_CoachingReviewHead);

            return Json(list);
        }
    }
}