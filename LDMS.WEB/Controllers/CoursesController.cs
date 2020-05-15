using LDMS.Core;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.ViewModels.SearchModel;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly CourseService _CourseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ILogger<CoursesController> logger, CourseService courseService)
        {
            _logger = logger;
            _CourseService = courseService;
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Courses")]
        [Route("Courses/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Courses/Add")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult Detail()
        {
            return View("/Views/Courses/Detail.cshtml");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Courses/ClassDetail")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult ClassDetail()
        {
            return View("/Views/Courses/ClassDetail.cshtml");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Courses/Detail/{ID}")]
        public IActionResult Detail(string ID)
        {
            if (ID == null) ViewData["ID"] = "";
            else ViewData["ID"] = ID;

            if (ID == null) { return View("/Views/Courses/Detail.cshtml", null); }


            LDMS_M_Course p = _CourseService.GetCourseByID(ID);

            return View("/Views/Courses/Detail.cshtml", p);
        }

        [Route("Courses/ClassDetail/{ID}")]
        public IActionResult ClassDetail(string ID)
        {
            if (ID == null) ViewData["ID"] = "";
            else ViewData["ID"] = ID;

            LDMS_T_Class p = _CourseService.GetClassByID(ID);
            p.LearnDateStart_Show = p.LearnDateStart.ToString("MM/dd/yyyy");
            p.LearnDateEnd_Show = p.LearnDateEnd.ToString("MM/dd/yyyy");

            p.RegisterDateStart_Show = p.RegisterDateStart.ToString("MM/dd/yyyy");
            p.RegisterDateEnd_Show = p.RegisterDateEnd.ToString("MM/dd/yyyy");

            return View("/Views/Courses/ClassDetail.cshtml", p);
        }

        [Route("Courses/AddClass/{ID_Course}")]
        public IActionResult AddClass(string ID_Course)
        {
            LDMS_M_Course p = _CourseService.GetCourseByID(ID_Course);

            LDMS_T_Class cls = new LDMS_T_Class();
            cls.ID = 0;
            cls.ID_Course = p.ID;
            cls.CourseName = p.CourseName;
            cls.ID_LearnMethod = p.ID_LearnMethod;

            return View("/Views/Courses/ClassDetail.cshtml", cls);
        }

        [HttpGet]
        [Route("Courses/NewClass")]
        public IActionResult NewClass( string ID_Course )
        {
            return Json(ID_Course);
        }


        [HttpGet]
        [Route("Courses/GetAllCourseType")]
        public IActionResult GetAllCourseType()
        {
            return Json(_CourseService.GetAllCourseType().Result);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Courses/GetAllCourseLearnMethod")]
        public IActionResult GetAllCourseLearnMethod()
        {
            return Json(_CourseService.GetAllCourseLearnMethod().Result);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Courses/Courses")]
        public async Task<IActionResult> Courses(SearchCourseModel model)
        {
            var platforms = await _CourseService.GetAll(model.CourseID, model.CourseName,  model.CourseStatus, model.LearnMethod);
            return Response(new ServiceResult(platforms));
        }

        [HttpGet]
        [Route("Courses/GetVenuRoom")]
        public IActionResult GetVenuRoom()
        {
            return Json(_CourseService.GetVenuRoom().Result);
        }

        [HttpGet]
        [Route("Courses/GetVenuRoomByPlantID")]
        public IActionResult GetVenuRoomByPlantID(string ID_Plant)
        {
            return Json(_CourseService.GetVenuRoomByPlantID(ID_Plant).Result);
        }


        [HttpPost]
        [Route("Courses/Search")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult Search(string CourseID, string CourseName, string CourseStatus, string LearnMethod)
        {

            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_M_Course_Search criteria = new LDMS_M_Course_Search();
            criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.SortField = sortColumn;
            criteria.SortOrder = sortOrder;
            criteria.CourseID = CourseID;
            criteria.CourseName = CourseName;
            criteria.CourseStatus = CourseStatus;
            criteria.LearnMethod = LearnMethod;
            var courses = _CourseService.GetCourse(criteria);
            //ViewData["Instructor"] = instructor.Results;
            return Json(courses);
        }

        private bool ValidateList(List<vEmployeeResult> list,  ref string message )
        {
            List<vEmployeeResult> listTemp = list;

            bool isCheck = false;
            for (int k = 0; k < list.Count; k++)
            {
                int x = 0;
                for (int m = 0; m < listTemp.Count; m++)
                {
                    if (list[k].EmployeeId == listTemp[m].EmployeeId)
                    {
                        if (x > 0)
                        {
                            message += listTemp[m].EmployeeId + ",";
                            isCheck = true;
                        }
                        x++;
                    }
                }
            }

            return isCheck;
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Courses/InsertCourse")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InsertCourse(string ID_Course, string CourseID, string CourseName, string ID_LearnMethod,
                                            string ID_CourseType, string Objective, string Description, string OutLine,
                                            string IsRefreshment, string RefreshmentPeriod, string RefreshmentUnit,
                                            string TargetEmployeeID, string ID_PlantTarget, string ID_CenterTarget, 
                                            string ID_DivisionTarget, string ID_DepartmentTarget, string ID_SectionTarget,
                                            string JobGradeTargetID, string JobTitleTargetID, string IsActive)
        {

            ResultMessage res = new ResultMessage();
            List<vEmployeeResult> list = _CourseService.CheckEmployee(TargetEmployeeID);
            List<vEmployeeResult> listTemp = list;
            string alert = "มีรหัสพนักงานซ้ำกัน : ";
            
            /// Validate รหัสพนักงาน ซ้ำ
            if (ValidateList(list, ref alert)) {
                res.message = alert;
                res.result = false;
                return Json(res);
            }
            
            res.message = " รหัสพนักงาน ";
            //string[] emps = TargetEmployeeID.Split(',');
            int index = 0;
            for( int i = 0; i<list.Count; i++)
            {
                if(list[i].Result == "0")
                {
                    res.message += list[i].EmployeeId + ",";
                    index++;
                }
            }

            if(index > 0)
            {
                res.message += " ไม่มีอยู่ในระบบ";
                res.result = false;
                res.data = null;
                return Json(res);
            }
            

            LDMS_M_Course course = new LDMS_M_Course();
            if (ID_Course == "null")
            {
                course = _CourseService.CreateCourse(CourseID, CourseName, ID_LearnMethod, ID_CourseType, Objective, Description, OutLine,
                                                  IsRefreshment, RefreshmentPeriod, RefreshmentUnit, TargetEmployeeID, ID_PlantTarget,
                                                  ID_CenterTarget, ID_DivisionTarget, ID_DepartmentTarget, ID_SectionTarget, JobGradeTargetID,
                                                  JobTitleTargetID, IsActive );
                //course.ID = _id;

            }
            else
            {
                course = _CourseService.UpdateCourse(ID_Course, CourseID, CourseName, ID_LearnMethod, ID_CourseType, Objective, Description, OutLine,
                                                  IsRefreshment, RefreshmentPeriod, RefreshmentUnit, TargetEmployeeID, ID_PlantTarget,
                                                  ID_CenterTarget, ID_DivisionTarget, ID_DepartmentTarget, ID_SectionTarget, JobGradeTargetID,
                                                  JobTitleTargetID, IsActive);
                
            }

            res.data = course;
            res.result = true;
            res.message = "Save Course Complete";

            return Json(res);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Courses/EmployeeSearch")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult EmployeeSearch(string EmployeeID, string EmployeeName, string DepartmentID, string SectionID, string JobGradeID, string JobTitleID)
        {

            //string sortOrder = Request.Form["order[0][dir]"];
            //string sortIndex = Request.Form["order[0][column]"];

            //string sortColumn = "";
            //if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            LDMS_M_Employee_Search criteria = new LDMS_M_Employee_Search();
            //criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            //criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.EmployeeID = EmployeeID;
            criteria.EmployeeName = EmployeeName;
            criteria.DepartmentID = DepartmentID;
            criteria.SectionID = SectionID;
            criteria.JobGradeID = JobGradeID;
            criteria.JobTitleID = JobTitleID;

            var employees = _CourseService.GetEmployee(criteria);
            //ViewData["Instructor"] = instructor.Results;
            return Json(employees);
        }
        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Courses/InstructorSearch")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InstructorSearch(string InstructorID, string InstructorName, string Type, string Organization, string Experience )
        {
            LDMS_M_Instructor_Search criteria = new LDMS_M_Instructor_Search();
            criteria.Instructor_ID = InstructorID;
            criteria.Instructor_Name = InstructorName;
            criteria.Type = Type;
            criteria.Organization = Organization;
            criteria.Course_Experience = Experience;

            var employees = _CourseService.GetInstructor(criteria);
          
            return Json(employees);
        }

        [HttpGet]
        [Route("Courses/GetTime")]
        public IActionResult GetTime()
        {
            List<LDMS_M_Time> lstime = new List<LDMS_M_Time>();


            string ktr, mtr;
            for (int k = 0; k < 24; k++)
            {
                for (int m = 0; m < 2; m++)
                {
                    if (k < 10)
                    {
                        ktr = "0" + k.ToString();
                    }
                    else
                    {
                        ktr = k.ToString();
                    }
                    if (m < 1)
                    {
                        mtr = "0" + m.ToString();
                    }
                    else
                    {
                        mtr = "30";
                    }

                    LDMS_M_Time t = new LDMS_M_Time();
                    t.key = ktr + ":" + mtr;
                    t.value = ktr + ":" + mtr;

                    lstime.Add(t);
                }
            }

            return Json(lstime);
        }




        [HttpPost]
        [Route("Courses/InsertClass")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InsertClass( string ID, string LMS_PACD_courseID, string ID_Course, string ID_Instructor, string ClassCapacity, string ClassFee,
                                            string LearnDateStart, string LearnTimeStart, string LearnDateEnd, string LearnTimeEnd,
                                            string RegisterDateStart, string RegisterDateEnd, string ID_PlantVenue,
                                            string ID_VenueRoom, string PlaceAndLocation, string ClassStatus,
                                            string IsAttend, string AttendNum, string IsTest,
                                            string TestFullScore, string TestPercentage, string IsSkill, string SkillFullScore,
                                            string SkillPercentage, string IsCoaching, string IsCertificate, string IsAttachCert,
                                            string CertificationRemark, string ReminderDurationDay, string IsActive )
        {

            string statusClass = "0"; // 0 = Plan, 10 = Not Start 
            if(LearnDateStart != "" && RegisterDateStart == "" && RegisterDateEnd == "")
            {
                statusClass = "0"; //
            }
            
            if(LearnDateStart != "" && RegisterDateStart != "" && RegisterDateEnd != "")
            {
                statusClass = "10"; // Not Start 
            }
     
            LDMS_T_Class t_class = new LDMS_T_Class();
            if (ID == "0")
            {
                t_class = _CourseService.CreateClass(LMS_PACD_courseID, ID_Course,  ID_Instructor,  ClassCapacity,  ClassFee,
                                                     LearnDateStart,  LearnTimeStart,  LearnDateEnd,  LearnTimeEnd,
                                                     RegisterDateStart,  RegisterDateEnd,  ID_PlantVenue,
                                                     ID_VenueRoom,  PlaceAndLocation, statusClass,
                                                     IsAttend,  AttendNum,  IsTest,
                                                     TestFullScore,  TestPercentage,  IsSkill,  SkillFullScore,
                                                     SkillPercentage,  IsCoaching,  IsCertificate,  IsAttachCert,
                                                     CertificationRemark,  ReminderDurationDay,  IsActive );
                        //course.ID = _id;

            }
            else
            {
                t_class =  _CourseService.UpdateClass( ID, LMS_PACD_courseID, ID_Course, ID_Instructor, ClassCapacity, ClassFee,
                                                     LearnDateStart, LearnTimeStart, LearnDateEnd, LearnTimeEnd,
                                                     RegisterDateStart, RegisterDateEnd, ID_PlantVenue,
                                                     ID_VenueRoom, PlaceAndLocation, ClassStatus,
                                                     IsAttend, AttendNum, IsTest,
                                                     TestFullScore, TestPercentage, IsSkill, SkillFullScore,
                                                     SkillPercentage, IsCoaching, IsCertificate, IsAttachCert,
                                                     CertificationRemark, ReminderDurationDay, IsActive );

            }

            return Json(t_class);
        }

        [HttpPost]
        [Route("Courses/LoadClass")]
        public IActionResult LoadClass(string ID)
        {
            var cla = _CourseService.GetClass(ID);
           
            return Json(cla);
        }

        [HttpGet]
        [Route("Courses/BacktoCourse")]
        public IActionResult BacktoCourse(string ID)
        {
            return Json(ID);
        }


        [HttpGet]
        [Route("Courses/GetClassRemain")]
        public IActionResult GetClassRemain(string ID_Course, string ID_Plant,
                                              string ID_Center, string ID_Division,
                                              string ID_Department, string ID_Section)
        {
            var cap = _CourseService.GetClassRemain(ID_Course, ID_Plant, ID_Center, 
                                                    ID_Division, ID_Department, ID_Section);

            //LDMS_M_Course p = _CourseService.GetCourseByID(ID_Course);

            //cap.ID_LearnMethod = p.ID_LearnMethod;

            return Json(cap);
        }

        [HttpPost]
        [Route("Courses/DeleteCourse")]
        public IActionResult DeleteCourse(string ID_Course)
        {

            return Json(_CourseService.DeleteCourse(ID_Course));
        }


        [HttpPost]
        [Route("Courses/DeleteClass")]
        public IActionResult DeleteClass(string ID_Class)
        {
            /*StringBuilder sb = new StringBuilder();
            string[] list = ID_Class.Split(',');
            foreach (var r in list)
            {
                sb.Append(r + ",");
            }

            string classID = sb.ToString();
            classID = classID.TrimEnd(new char[] { ',' });*/

            return Json(_CourseService.DeleteClass(ID_Class));
        }
    }
}