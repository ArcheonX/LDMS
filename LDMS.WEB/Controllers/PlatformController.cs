using LDMS.Core;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.ViewModels.SearchModel;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class PlatformController : BaseController
    {
        private readonly PlatformService _PlatFormService;
        private readonly ILogger<PlatformController> _logger;

        public PlatformController(ILogger<PlatformController> logger, PlatformService platformService)
        {
            _logger = logger;
            _PlatFormService = platformService;
        }

        [AuthorizeRole(UserRole.All)]
        public IActionResult PlatformManagement()
        {
            return View();
        }


        [AuthorizeRole(UserRole.All)]
        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeRole(UserRole.All)]
        [Route("Platform/EditPlatform/{ID}")]
        public IActionResult EditPlatform(string ID)
        {
            if (ID == null) ViewData["ID"] = "";
            else ViewData["ID"] = ID;

            if (ID == null) { return View("/Views/Platform/_EditorPlatform.cshtml", null ); }
                

            LDMS_M_Platform p = _PlatFormService.GetPlatformByID(ID);
 
            return View("/Views/Platform/_EditorPlatform.cshtml", p);
            //return PartialView("_EditorPlatform", p);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/AddPlatform")]
        public IActionResult AddPlatform()
        {
            return View("/Views/Platform/_EditorPlatform.cshtml", null);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Platform/SearchPlatform")]
        public async Task<ActionResult> SearchPlatform(SearchPlatformModel model)
        {
            var platforms = await _PlatFormService.GetAll(model.PlatformId, model.PlatformName, 
                                                         model.PlatformType, model.PlatformStatus);
            return PartialView("_ViewAllPlatform", platforms);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/Platforms")]
        public async Task<IActionResult> Platforms(SearchPlatformModel model)
        {
            var platforms = await _PlatFormService.GetAll(model.PlatformId, model.PlatformName,
                                                         model.PlatformType, model.PlatformStatus);
            return Response(new ServiceResult(platforms));
        }
        [AuthorizeRole(UserRole.All)] 
        [HttpPost]
        [Route("Platform/Search")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Search(string PlatformID, string PlatformName, string PlatformType, string PlatformStatus)
        {

            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_M_Platform_Search criteria = new LDMS_M_Platform_Search();
            criteria.PageNum = int.Parse(Request.Form["start"])/ int.Parse(Request.Form["length"]) + 1;
            criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.SortField = sortColumn;
            criteria.SortOrder = sortOrder;
            criteria.PlatformId = PlatformID;
            criteria.PlatformName = PlatformName;
            criteria.PlatformType = PlatformType;
            criteria.PlatformStatus = PlatformStatus;
            var platform = _PlatFormService.GetPlatform(criteria);
            //ViewData["Instructor"] = instructor.Results;
            return Json(platform);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/GetAllPlatformType")]
        public IActionResult GetAllPlatformType()
        {
            return Json(_PlatFormService.GetAllPlatformType().Result);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/InsertPlatform")]
        public IActionResult InsertPlatform( string ID_Platform, string PlatformID, string PlatformName_EN, string PlatformName_TH,
                                             string ID_PlatformType, string PlatformDescription, string ID_Department_Create, string PlatformStatus)
        {

            ResultMessage result = new ResultMessage();

            int count = _PlatFormService.SelectPlatform(PlatformID/*, PlatformName_EN*/);
            if(count > 0)
            {
                result.message = "มีรายการ Platform ID : " + PlatformID /* + ", Platform Name : " + PlatformName_EN*/ + " ในระบบแล้ว กรุณาเปลี่ยนแปลงข้อมูล";
                return Json(result);
            }


            int _id = -1;
            LDMS_M_Platform platform = new LDMS_M_Platform();
            if (ID_Platform == "null")
            {
                _id = _PlatFormService.CreatePlatform(PlatformID, PlatformName_EN, PlatformName_TH,
                                                          ID_PlatformType, PlatformDescription, ID_Department_Create, PlatformStatus);
                platform.ID = _id;
               
            }
            else
            {
                _PlatFormService.UpdatePlatform(ID_Platform, PlatformID, PlatformName_EN, PlatformName_TH,
                                                          ID_PlatformType, PlatformDescription, ID_Department_Create, PlatformStatus);
                platform.ID = int.Parse(ID_Platform);
            }

            platform.PlatformID = PlatformID;
            platform.PlatformName_EN = PlatformName_EN;
            platform.PlatformName_TH = PlatformName_TH;
            platform.PlatformDescription = PlatformDescription;
            platform.ID_Department_Create = int.Parse(ID_Department_Create);


            return Json(platform);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/SearchCourse")]
        public async Task<ActionResult> SearchCourse(SearchCourseModel model)
        {
            var course = await _PlatFormService.GetCourseAll(model.CourseID, model.CourseName,
                                                         model.CourseStatus, model.LearnMethod);

            
            //return PartialView("_ViewAllCourse", course); 
            return Json(course);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/GetAllCourseLearnMethod")]
        public IActionResult GetAllCourseLearnMethod()
        {
            return Json(_PlatFormService.GetAllCourseLearnMethod().Result);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/InsertSubPlatform")]
        public IActionResult InsertSubPlatform(string ID_Platform, string SubPlatformName_EN, string SubPlatformName_TH)
        {
            int _id = -1;
            int count = _PlatFormService.GetSubPlatform(ID_Platform, SubPlatformName_EN); // Validate
            if (count > 0) {
                _id = -1;
            }
            else
            {
                _id = _PlatFormService.CreateSubPlatform(ID_Platform, SubPlatformName_EN, SubPlatformName_TH);
            }

            return Json(_id);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/EditSubPlatform/{ID_SubPlatform}")]
        [AutoValidateAntiforgeryToken]
        public IActionResult EditSubPlatform(string ID_SubPlatform)
        {
            if (ID_SubPlatform == null) { return View("/Views/Platform/_EditorSubPlatform.cshtml", null); }

            LDMS_M_SubPlatform p = _PlatFormService.LoadSubPlatformByID(ID_SubPlatform);

            return View("/Views/Platform/_EditorSubPlatform.cshtml", p);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/UpdateSubPlatform")]
        [AutoValidateAntiforgeryToken]
        public IActionResult UpdateSubPlatform(string ID_Platform,  string ID_SubPlatform, string SubPlatformName_EN, string SubPlatformName_TH)
        {
            int _id = -1;
            int count = _PlatFormService.GetSubPlatform(ID_Platform, SubPlatformName_EN); // Validate
            if (count > 0)
            {
                _id = -1;
            }
            else
            {
                _id = _PlatFormService.UpdateSubPlatform(ID_SubPlatform, SubPlatformName_EN, SubPlatformName_TH);
            }

            return Json(ID_Platform);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/DeleteSubPlatform")]
        [AutoValidateAntiforgeryToken]
        public IActionResult DeleteSubPlatform(string ID_SubPlatform)
        {
            int _id = _PlatFormService.DeleteSubPlatform(ID_SubPlatform);

            return Json(_id);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Platform/SelectSubPlatform")]
        [AutoValidateAntiforgeryToken]
        public IActionResult SelectSubPlatform(string ID_Platform, string Keyword)
        {
            if (ID_Platform == "null")
                return null;

            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_M_SubPlatform_Search criteria = new LDMS_M_SubPlatform_Search();
            criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.ID_Platform = ID_Platform;
            criteria.Keyword = Keyword;

            var subplatform = _PlatFormService.GetSubPlatform(criteria);

            return Json(subplatform);



            //return Json(_PlatFormService.GetSubPlatformAll(ID_Platform));
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/InsertSubPlatformCourse")]
        [AutoValidateAntiforgeryToken]
        public IActionResult InsertSubPlatformCourse(string model, string ID_SubPlatform, string ID_Platform)
        {
            ResultMessage result = new ResultMessage();

            if (model == "[]") {
                result.message = "Please Select Course !!!";
                result.result = false;
                return Json(result);
            }


            List<LDMS_M_SubPlatformCourse> ls = _PlatFormService.SelectLDMS_M_SubPlatformCourse(ID_Platform);
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(model);
            string message = " มีรายการ Subplatform และ Course ซ้ำดังนี้ ";
            int checkCount = 0;
            for( int i=0; i < dt.Rows.Count; i++ )
            {
                foreach(LDMS_M_SubPlatformCourse s in ls)
                {
                    //if( dt.Rows[i]["ID_Subplatform"].ToString() == s.ID_SubPlatform.ToString() 
                    //    && dt.Rows[i]["ID_Course"].ToString() == s.ID_Course.ToString() )
                    if (dt.Rows[i]["ID_Course"].ToString() == s.ID_Course.ToString())
                    {
                        message +=   "\n " + (i + 1).ToString() + " : Subplatform " + s.SubPlatformName_EN + "  Course : " + s.CourseName;
                        checkCount++;
                    }
                }
            }

            if(checkCount > 0)
            {
                result.message = message;
                result.result = false;
            }
            else
            {
                bool res = _PlatFormService.CreateSubPlatformCourse(model, ID_SubPlatform);
                result.message = "Save Course Complete";
                result.result = true;
            }

            //bool res = _PlatFormService.CreateSubPlatformCourse(model, ID_SubPlatform);

            return Json(result);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/LoadSubPlatformByID")]
        public IActionResult LoadSubPlatformByID(string ID_SubPlatform)
        {
            return Json(ID_SubPlatform);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/BacktoPlatform")]
        public IActionResult BacktoPlatform(string ID)
        {
            return Json(ID);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/LoadSubPlatformCourseByID")]
        [AutoValidateAntiforgeryToken]
        public IActionResult LoadSubPlatformCourseByID(string ID_SubPlatform)
        {
            List<LDMS_M_Course> p = _PlatFormService.LoadSubPlatformCourseByID(ID_SubPlatform);
            return Json(p);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/DeleteSubPlatformCourse")]
        public IActionResult DeleteSubPlatformCourse(string ID_SubPlatform, string ID_Course)
        {
            int ret = _PlatFormService.DeleteSubPlatformCourse(ID_SubPlatform, ID_Course);

            return Json(ID_SubPlatform);
            //return PartialView("_EditorPlatform", p);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Platform/LoadSubPlatformCourse")]
        [AutoValidateAntiforgeryToken]
        public IActionResult LoadSubPlatformCourse(string ID_SubPlatform, string Keyword)
        {
            //List<LDMS_M_Course> p = _PlatFormService.LoadSubPlatformCourseByID(ID_SubPlatform);
            //return Json(p);

            if (ID_SubPlatform == "null")
                return null;

            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_M_SubPlatformCourse_Search criteria = new LDMS_M_SubPlatformCourse_Search();
            criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.ID_SubPlatform = ID_SubPlatform;
            criteria.Keyword = Keyword;

            var course = _PlatFormService.LoadSubPlatformCourse(criteria);

            return Json(course);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Platform/GetPlatformByDep")]
        public IActionResult GetPlatformByDep()
        {
            List<LDMS_M_Platform> p = _PlatFormService.GetPlatformByDep();

            return Json(p);
        }
    }
}