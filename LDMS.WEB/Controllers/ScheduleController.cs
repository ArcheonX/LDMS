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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class ScheduleController : Controller
    {
        private string registerStatus_Approve = "70";
        private string registerStatus_Reject = "99";
        

        private readonly ScheduleService _ScheduleService;
        private readonly ClassRegisterService _ClassRegisterService;
        private readonly CourseRegisterService _CourseRegisterService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(ILogger<ScheduleController> logger, ScheduleService scheduleService, ClassRegisterService classRegisterService, CourseRegisterService CourseRegisterService, IWebHostEnvironment environment)
        {
            _logger = logger;
            _ScheduleService = scheduleService;
            _ClassRegisterService = classRegisterService;
            _CourseRegisterService = CourseRegisterService;
            _hostingEnvironment = environment;
        }

        public IActionResult Schedule()
        {
            return View("/Views/Schedule/Schedule.cshtml");
        }

        public IActionResult TeamSchedule()
        {
            return View("/Views/Schedule/TeamSchedule.cshtml");
        }

        public IActionResult TeamClassRegister()
        {
            return View("/Views/Schedule/TeamClassRegister.cshtml");
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetSchedule/")]
        public IActionResult GetSchedule(string ID_Employee)
        {
            List<LDMS_T_ScheduleClass> schedule = _ScheduleService.GetScheduleClass();
            List<LDMS_T_ScheduleClass> lsSchedule = new List<LDMS_T_ScheduleClass>();
            List<string> lsCount = new List<string>();
            string id_course = "";
            int i = 0;
            int count = 1;
            foreach(LDMS_T_ScheduleClass sch in schedule)
            {
                if ( sch.ID_Course != id_course)
                {
                    if (count > 1)
                    {
                        lsCount.Add(count.ToString());
                        count = 1;
                    }
                  
                    sch.Apr = convertDate(sch.Apr);
                    sch.May = convertDate(sch.May);
                    sch.Jun = convertDate(sch.Jun);
                    sch.Jul = convertDate(sch.Jul);
                    sch.Aug = convertDate(sch.Aug);
                    sch.Sep = convertDate(sch.Sep);
                    sch.Oct = convertDate(sch.Oct);
                    sch.Nov = convertDate(sch.Nov);
                    sch.Dec = convertDate(sch.Dec);
                    sch.Jan = convertDate(sch.Jan);
                    sch.Feb = convertDate(sch.Feb);
                    sch.Mar = convertDate(sch.Mar);

                    lsSchedule.Add(sch);
                    
                    i++;
                    
                }
                else
                {
                    count++;
                }

                id_course = sch.ID_Course;
               
            }
            lsCount.Add(count.ToString());
            int index = 0;
            foreach (LDMS_T_ScheduleClass sch in lsSchedule)
            {
                if (sch.Apr != "") sch.Apr += "(" + lsCount[index] + ")";
                if (sch.May != "") sch.May += "(" + lsCount[index] + ")";
                if (sch.Jun != "") sch.Jun += "(" + lsCount[index] + ")";
                if (sch.Jul != "") sch.Jul += "(" + lsCount[index] + ")";
                if (sch.Aug != "") sch.Aug += "(" + lsCount[index] + ")";
                if (sch.Sep != "") sch.Sep += "(" + lsCount[index] + ")";
                if (sch.Oct != "") sch.Oct += "(" + lsCount[index] + ")";
                if (sch.Nov != "") sch.Nov += "(" + lsCount[index] + ")";
                if (sch.Dec != "") sch.Dec += "(" + lsCount[index] + ")";
                if (sch.Jan != "") sch.Jan += "(" + lsCount[index] + ")";
                if (sch.Feb != "") sch.Feb += "(" + lsCount[index] + ")";
                if (sch.Mar != "") sch.Mar += "(" + lsCount[index] + ")";
                index++;
            }

            return Json(lsSchedule);
        }

        private string convertDate(string d)
        {
            string dt = "";
            if (d == null)
            {
                dt = "";
            }
            else
            {
                var date = Convert.ToDateTime(d);
               dt = date.ToString("dd/MM/yyyy");
            }

            return dt;
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetTeamSchedule/")]
        public IActionResult GetTeamSchedule(string ID_Department)
        {
            List<LDMS_T_ScheduleClass> schedule = _ScheduleService.GetTeamScheduleClass(ID_Department);
            List<LDMS_T_ScheduleClass> lsSchedule = new List<LDMS_T_ScheduleClass>();
            List<string> lsCount = new List<string>();
            string id_course = "";
            int i = 0;
            int count = 1;
            foreach (LDMS_T_ScheduleClass sch in schedule)
            {
                if (sch.ID_Course != id_course)
                {
                    if (count > 1)
                    {
                        lsCount.Add(count.ToString());
                        count = 1;
                    }

                    sch.Apr = convertDate(sch.Apr);
                    sch.May = convertDate(sch.May);
                    sch.Jun = convertDate(sch.Jun);
                    sch.Jul = convertDate(sch.Jul);
                    sch.Aug = convertDate(sch.Aug);
                    sch.Sep = convertDate(sch.Sep);
                    sch.Oct = convertDate(sch.Oct);
                    sch.Nov = convertDate(sch.Nov);
                    sch.Dec = convertDate(sch.Dec);
                    sch.Jan = convertDate(sch.Jan);
                    sch.Feb = convertDate(sch.Feb);
                    sch.Mar = convertDate(sch.Mar);

                    lsSchedule.Add(sch);

                    i++;

                }
                else
                {
                    count++;
                }

                id_course = sch.ID_Course;

            }
            //lsCount.Add(count.ToString());
            //int index = 0;
            /*foreach (LDMS_T_ScheduleClass sch in lsSchedule)
            {
                if (sch.Apr != "") sch.Apr += "(" + lsCount[index] + ")";
                if (sch.May != "") sch.May += "(" + lsCount[index] + ")";
                if (sch.Jun != "") sch.Jun += "(" + lsCount[index] + ")";
                if (sch.Jul != "") sch.Jul += "(" + lsCount[index] + ")";
                if (sch.Aug != "") sch.Aug += "(" + lsCount[index] + ")";
                if (sch.Sep != "") sch.Sep += "(" + lsCount[index] + ")";
                if (sch.Oct != "") sch.Oct += "(" + lsCount[index] + ")";
                if (sch.Nov != "") sch.Nov += "(" + lsCount[index] + ")";
                if (sch.Dec != "") sch.Dec += "(" + lsCount[index] + ")";
                if (sch.Jan != "") sch.Jan += "(" + lsCount[index] + ")";
                if (sch.Feb != "") sch.Feb += "(" + lsCount[index] + ")";
                if (sch.Mar != "") sch.Mar += "(" + lsCount[index] + ")";
                index++;
            }*/

            return Json(lsSchedule);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetScheduleClass/")]
        public IActionResult GetScheduleClass(string ID_Course)
        {
            List<LDMS_T_ScheduleClass_Detail> schedule = _ScheduleService.GetScheduleClassDetail(ID_Course);
            List<LDMS_T_ScheduleClass_Detail> lsSchedule = new List<LDMS_T_ScheduleClass_Detail>();
            foreach (LDMS_T_ScheduleClass_Detail sch in schedule)
            {
                sch.LearnDateStart = convertDate(sch.LearnDateStart);

                lsSchedule.Add(sch);
            }

            return Json(lsSchedule);
        }


        [Route("Schedule/ClassRegister/{ID}")]
        public IActionResult ClassRegister(string ID)
        {
            //List<LDMS_T_ScheduleClass_Reg> classList = _ScheduleService.GetClassReg(ID);

            LDMS_M_Course course = new LDMS_M_Course();
            course.ID = int.Parse(ID);

            return View("/Views/Schedule/ClassRegister.cshtml", course);
        }

        [Route("Schedule/ClassRegisterAll")]
        public IActionResult ClassRegisterAll()
        {
            return View("/Views/Schedule/ClassRegister.cshtml");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetClassRegister/")]
        public IActionResult GetClassRegister(string ID_Course)
        {
            List<LDMS_T_ScheduleClass_Reg> classList = _ScheduleService.GetClassReg(ID_Course);
            foreach (LDMS_T_ScheduleClass_Reg cl in classList)
            {
                cl.LearnDateStart = convertDate(cl.LearnDateStart);
                cl.Time = cl.LearnTimeStart + " - " + cl.LearnTimeEnd;
            }


           return Json(classList);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetCourseInfo/")]
        public IActionResult GetCourseInfo(string ID_Course)
        {
            LDMS_T_ScheduleCourseInfo courseInfo = _ScheduleService.GetCourseInfo(ID_Course);
            

            return Json(courseInfo);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/ClassRegisterInsert")]
        public IActionResult ClassRegisterInsert(string model)
        {
            bool res = _ClassRegisterService.CreateClassRegister(model);

            return Json(res);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetTeamClassRegister/")]
        public IActionResult GetTeamClassRegister(string ID_Department, string Keyword)
        {
            List<LDMS_T_ScheduleClass_Reg> classList = _ScheduleService.GetTeamClassReg(ID_Department, Keyword);
            foreach (LDMS_T_ScheduleClass_Reg cl in classList)
            {
                cl.LearnDateStart = convertDate(cl.LearnDateStart);
                cl.Time = cl.LearnTimeStart + " - " + cl.LearnTimeEnd;
                if (cl.RemarkManager == null) cl.RemarkManager = "";
                if (cl.RemarkAdmin == null) cl.RemarkAdmin = "";
            }


            return Json(classList);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/UpdateRegisStatusA")]
        public IActionResult UpdateRegisStatusA(string model)
        {
            UpdateRegisStatus(model, registerStatus_Approve);

            return Ok();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/UpdateRegisStatusR")]
        public IActionResult UpdateRegisStatusR(string model)
        {
            UpdateRegisStatus(model, registerStatus_Reject);

            return Ok();
        }

        private void UpdateRegisStatus(string model, string status)
        {
            DataTable dt = (DataTable)JsonConvert.DeserializeObject(model, (typeof(DataTable)));
            if( status == registerStatus_Reject)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _ScheduleService.UpdateStatusReject(dt.Rows[i]["ID_Class"].ToString(),
                                                        dt.Rows[i]["ID_Course"].ToString(),
                                                        dt.Rows[i]["ID_Employee"].ToString(),
                                                        status, dt.Rows[i]["Remark"].ToString(), JwtManager.Instance.GetUserId(HttpContext.Request));
                }
            }
            else // Approve
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _CourseRegisterService.UpdateStatus(dt.Rows[i]["ID_Class"].ToString(),
                                                        dt.Rows[i]["ID_Course"].ToString(),
                                                        dt.Rows[i]["ID_Employee"].ToString(),
                                                        status, JwtManager.Instance.GetUserId(HttpContext.Request));
                }
            }
           
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/SetParamToClassDetail/")]
        public IActionResult SetParamToClassDetail(string ID_Department, string ID_Course, string Month)
        {
            ModelSearchClass obj = new ModelSearchClass();
            obj.ID_Department = ID_Department;
            obj.ID_Course = ID_Course;
            obj.Month = Month;

            //return View("/Views/Schedule/TeamClassDetail.cshtml", obj);
            return Json(obj);
        }

        [HttpGet]
        [Route("Schedule/TeamClassDetail/")]
        public IActionResult TeamClassDetail(string ID_Department, string ID_Course, string Month)
        {
  
            ModelSearchClass obj = new ModelSearchClass();
            obj.ID_Department = ID_Department;
            obj.ID_Course = ID_Course;
            obj.Month = Month;

            return View("/Views/Schedule/TeamClassDetail.cshtml", obj);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Schedule/GetClassEmployee/")]
        public IActionResult GetClassEmployee(string ID_Department, string ID_Course, string Month)
        {
            List<LDMS_T_ScheduleClass_EmployeeInfo> classList = _ScheduleService.GetClassRegEmployee(ID_Department, ID_Course, Month);
            List<LDMS_T_ScheduleClass_Reg> listObj = new List<LDMS_T_ScheduleClass_Reg>();
            string TmpCourseID = "";
            int count = 0;
            foreach (LDMS_T_ScheduleClass_EmployeeInfo cl in classList)
            {
                LDMS_T_ScheduleClass_EmployeeInfo emp = new LDMS_T_ScheduleClass_EmployeeInfo();
                emp.ClassID = cl.ClassID;
                emp.Capacity = cl.Capacity;
                emp.Registered = cl.Registered;
                emp.Waiting = cl.Waiting;
                emp.Approved = cl.Approved;
                emp.EmployeeID = cl.EmployeeID;
                emp.EmployeeName = cl.EmployeeName;
                emp.JobGradeName_EN = cl.JobGradeName_EN;
                emp.JobTitleName_EN = cl.JobTitleName_EN;
                emp.LearnDateStart = convertDate(cl.LearnDateStart);

                if (cl.CourseID != TmpCourseID)
                {
                    LDMS_T_ScheduleClass_EmployeeInfo obj = new LDMS_T_ScheduleClass_EmployeeInfo();
                    //obj.ClassID = cl.ClassID;
                    obj.lsEmployee = new List<LDMS_T_ScheduleClass_EmployeeInfo>();
                    obj.CourseName = cl.CourseName;
                    obj.ClassID = cl.ClassID;
                    obj.ClassID = cl.ClassID;
                    obj.Capacity = cl.Capacity;
                    obj.Registered = cl.Registered;
                    obj.Waiting = cl.Waiting;
                    obj.Approved = cl.Approved;
                    obj.EmployeeID = cl.EmployeeID;
                    obj.EmployeeName = cl.EmployeeName;
                    obj.JobGradeName_EN = cl.JobGradeName_EN;
                    obj.JobTitleName_EN = cl.JobTitleName_EN;
                    obj.LearnDateStart = convertDate(cl.LearnDateStart);

                    obj.lsEmployee.Add(emp);

                    listObj.Add(obj);
                    TmpCourseID = cl.CourseID;
                    count++;
                }
                else
                {
                    listObj[count - 1].lsEmployee.Add(emp);
                }
            }

            return Json(listObj);
        }
    }
}