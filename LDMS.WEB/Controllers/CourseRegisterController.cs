using LDMS.Core;
using LDMS.Identity;
using LDMS.Services;
//using LDMS.WEB.Models;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO.Packaging;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace LDMS.WEB.Controllers
{
    public class CourseRegisterController : BaseController
    {
        private string registerStatus_Approve = "70";
        private string registerStatus_Reject = "99";
        private string classStatus_Close = "70";
        private readonly CourseRegisterService _courseRegisterService;
        private IWebHostEnvironment _hostingEnvironment;

        public CourseRegisterController(CourseRegisterService courseRegisterService, IWebHostEnvironment environment)
        {
            _courseRegisterService = courseRegisterService;
            _hostingEnvironment = environment;
        }

        [AuthorizeRole(UserRole.All)]
        [Route("CourseRegister/Index/{c}/{l}")]
        public IActionResult Index(string c, string l)
        {
            ViewData["course"] = c;
            ViewData["class"] = l;
            return View("~/Views/CourseRegister/Index.cshtml");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/GetInfo")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetInfo(string ClassID)
        {
            var ret = _courseRegisterService.GetInfo(ClassID);

            return Json(ret);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/GetCourseRegis")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetCourseRegis(string classID, string courseID, string employeeID, string employeeName, string departmentName)
        {
            var ret = _courseRegisterService.Get(courseID, classID, employeeID, employeeName, departmentName);

            return Json(ret);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("CourseRegister/ExportExcel")]
        public FileStreamResult ExportExcel(string classID, string courseID, string employeeID, string employeeName, string departmentName)
        {
            var courseInfo = _courseRegisterService.GetInfo(classID);

            int TestFullScore = 1;
            if(courseInfo[0].TestFullScore != null)
                TestFullScore = courseInfo[0].TestFullScore;

            int SkillFullScore = 1;
            if (courseInfo[0].SkillFullScore != null)
                SkillFullScore = courseInfo[0].SkillFullScore;

            string ClassStatus = courseInfo[0].ClassStatus.ToString();

            List<dynamic> ret = _courseRegisterService.Get(courseID, classID, employeeID, employeeName, departmentName);

            MemoryStream ms = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("CourseRegister");

                worksheet.Cell(1, 1).Value = "#";
                worksheet.Cell(1, 2).Value = "Employee ID";
                worksheet.Cell(1, 3).Value = "Name";
                worksheet.Cell(1, 4).Value = "Department";
                worksheet.Cell(1, 5).Value = "Job Grade";
                worksheet.Cell(1, 6).Value = "Register";
                worksheet.Cell(1, 7).Value = "Attend";
                worksheet.Cell(1, 8).Value = "Pre-Test";
                worksheet.Cell(1, 9).Value = "Post-Test";
                worksheet.Cell(1, 10).Value = "%";
                worksheet.Cell(1, 11).Value = "Skill";
                worksheet.Cell(1, 12).Value = "%";
                worksheet.Cell(1, 13).Value = "Coaching";
                worksheet.Cell(1, 14).Value = "Certificate";
                worksheet.Cell(1, 15).Value = "Status";

                for(int i = 1; i <= 15; i++) worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.FromName("PowderBlue");

                for (int i = 0; i < ret.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = (i + 1);
                    worksheet.Cell(i + 2, 2).Value = ret[i].EmployeeID;
                    worksheet.Cell(i + 2, 3).Value = ret[i].Name+ " "+ ret[i].Surname;
                    worksheet.Cell(i + 2, 4).Value = ret[i].DepartmentName_TH;
                    worksheet.Cell(i + 2, 5).Value = ret[i].JobGradeName_TH;
                    if (ret[i].RegisterStatus == 70) worksheet.Cell(i + 2, 6).Value = "Yes"; else worksheet.Cell(i + 2, 6).Value = "No";
                    if (ret[i].Attend > 0) worksheet.Cell(i + 2, 7).Value = "Yes"; else worksheet.Cell(i + 2, 7).Value = "No";

                    if (ret[i].PreTestScore != null) worksheet.Cell(i + 2, 8).Value = ret[i].PreTestScore;

                    if (ret[i].PostTestScore != null)
                    {
                        worksheet.Cell(i + 2, 9).Value = ret[i].PostTestScore;
                        worksheet.Cell(i + 2, 10).Value = ((ret[i].PostTestScore / TestFullScore) * 100).ToString("0.00");
                    }
                    if (ret[i].SkillScore != null)
                    {
                        worksheet.Cell(i + 2, 11).Value = ret[i].SkillScore;
                        worksheet.Cell(i + 2, 12).Value = ((ret[i].SkillScore / SkillFullScore) * 100).ToString("0.00");
                    }

                    if (ret[i].CoachingStatus == 1) worksheet.Cell(i + 2, 13).Value = "Yes"; else worksheet.Cell(i + 2, 13).Value = "No";
                    if (ret[i].ExpireDate != null && ret[i].ExpireDate.Trim().Length > 0) worksheet.Cell(i + 2, 14).Value = ret[i].ExpireDate;
                    if (ClassStatus != "70") worksheet.Cell(i + 2, 15).Value = ret[i].RegisterStatus_Name; else worksheet.Cell(i + 2, 15).Value = ret[i].LearningResult_Name;
                }
                worksheet.Columns(1, 15).AdjustToContents();
                workbook.SaveAs(ms);
            }
         
            ms.Position = 0;

            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CourseRegister_Export.xlsx");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/GetEmployeeClose")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetEmployeeClose(string classID, string courseID)
        {
            var ret = _courseRegisterService.GetEmployeeForClose(courseID, classID);

            return Json(ret);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/AddRegis")]
        [AutoValidateAntiforgeryToken]
        public IActionResult AddRegis(string classID, string courseID, string[] empID)
        {
            List<dynamic> ret = _courseRegisterService.GetInfo(classID);

            for (int i = 0; i < empID.Length; i++)
            {
                string regis_id = _courseRegisterService.AddCourseRegister(classID, courseID, empID[i], "70", JwtManager.Instance.GetUserId(HttpContext.Request));

                if (ret.Count > 0)
                {
                    if (ret[0].IsAttend == 1)
                    {
                        _courseRegisterService.UpdateClassResult(classID, courseID, empID[i], "10", "30", "1", "0",
                            null, null, null, "0", "0", JwtManager.Instance.GetUserId(HttpContext.Request));
                    }

                    if (ret[0].IsCoaching == 1)
                    {
                        _courseRegisterService.AddCoaching(classID, courseID, empID[i], "10", JwtManager.Instance.GetUserId(HttpContext.Request));
                    }

                    if (ret[0].IsCertificate == 1)
                    {
                        _courseRegisterService.AddCertificate(classID, courseID, empID[i], "", "", "", JwtManager.Instance.GetUserId(HttpContext.Request));
                    }
                }
            }

            return Ok();
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/UpdateRegisStatusA")]
        [AutoValidateAntiforgeryToken]
        public IActionResult UpdateRegisStatusA(string classID, string courseID, string idList)
        {
            UpdateRegisStatus(classID, courseID, idList, registerStatus_Approve);

            return Ok();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/UpdateRegisStatusR")]
        [AutoValidateAntiforgeryToken]
        public IActionResult UpdateRegisStatusR(string classID, string courseID, string idList)
        {
            UpdateRegisStatus(classID, courseID, idList, registerStatus_Reject);

            return Ok();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/CloseClass")]
        [AutoValidateAntiforgeryToken]
        public IActionResult CloseClass(string classID, string courseID)
        {
            string idList = "";
            List<dynamic> ret = _courseRegisterService.GetEmployeeForClose(courseID, classID);

            for (int i = 0; i < ret.Count; i++)
            {
                idList += ret[i].EmployeeID + ",";
            }

            UpdateRegisStatus(classID, courseID, idList, registerStatus_Reject);
            _courseRegisterService.UpdateClass(classID, this.classStatus_Close, JwtManager.Instance.GetUserId(HttpContext.Request));

            return Ok();
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/UpdateAttendResult")]
        [AutoValidateAntiforgeryToken]
        public IActionResult UpdateAttendResult(string classID, string courseID, string lists)
        {
            List<dynamic> ret = _courseRegisterService.GetInfo(classID);
            if (ret.Count > 0)
            {
                string[] row = lists.Split("#", StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < row.Length; i++)
                {
                    //EmpID | Check Attend | pre Test | Post Test | Skill
                    string[] col = row[i].Split("|", StringSplitOptions.None);
                    string empID = col[0];
                    string preTest = "", postTest = "", skill = "";
                    string classState = "10";
                    string isAttend = col[1];

                    if (ret[0].IsTest == 1)
                    {
                        preTest = col[2];
                        if (preTest.Trim().Length > 0) classState = "20";
                        postTest = col[3];
                        if (preTest.Trim().Length > 0) classState = "30";
                    }

                    if (ret[0].IsSkill == 1)
                    {
                        skill = col[4];
                        if (skill.Trim().Length > 0) classState = "40";
                    }

                    if (isAttend == "1")
                    {
                        if (ret[0].IsAttend == 1)
                        {
                            _courseRegisterService.UpdateClassResult(classID, courseID, empID, classState, "30", "1", "0",
                                preTest, postTest, skill, "0", "0", JwtManager.Instance.GetUserId(HttpContext.Request));
                        }

                        if (ret[0].IsCoaching == 1)
                        {
                            _courseRegisterService.AddCoaching(classID, courseID, empID, "10", JwtManager.Instance.GetUserId(HttpContext.Request));
                        }

                        if (ret[0].IsCertificate == 1)
                        {
                            _courseRegisterService.AddCertificate(classID, courseID, empID, "", "", "", JwtManager.Instance.GetUserId(HttpContext.Request));
                        }
                    }
                }
            }


            return Ok();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("CourseRegister/UpdateCert")]
        [AutoValidateAntiforgeryToken]
        public IActionResult UpdateCert(string classID, string courseID, string employeeID, string expireDate, IFormFile fiCert)
        {
            string pathCert = "";
            if (fiCert != null)
            {
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" +
                    ContentDispositionHeaderValue.Parse(fiCert.ContentDisposition).FileName.Replace("\"", "");

                string filesPath = _hostingEnvironment.WebRootPath + "\\Uploads\\Certificate\\" + DateTime.Now.ToString("yyyyMM");

                fileName = fileName.Contains("\\")
                    ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName.Trim('"');
                fileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + fileName;

                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

                var fullFilePath = Path.Combine(filesPath, fileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    fiCert.CopyTo(stream);
                }

                pathCert = Path.Combine("\\Uploads\\Certificate\\" + DateTime.Now.ToString("yyyyMM"), fileName);
            }

            _courseRegisterService.AddCertificate(classID, courseID, employeeID, pathCert, "", expireDate, JwtManager.Instance.GetUserId(HttpContext.Request));

            return Ok();
        }


        private void UpdateRegisStatus(string classID, string courseID, string idList, string status)
        {
            if (idList != null)
            {
                string[] idLists = idList.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < idLists.Length; i++)
                {
                    _courseRegisterService.UpdateStatus(classID, courseID, idLists[i], status, JwtManager.Instance.GetUserId(HttpContext.Request));
                }
            }
        }


        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("CourseRegister/SearchEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Employees(SearchEmployeeModel model, string courseID, string classID)
        {
            int[] departments = new int[0];
            if (model.Departments != null && model.Departments.Any())
            {
                departments = model.Departments.Split(",").Select(int.Parse).ToArray();
            }

            int[] sections = new int[0];
            if (model.Sections != null && model.Sections.Any())
            {
                sections = model.Sections.Split(",").Select(int.Parse).ToArray();
            }

            int[] grades = new int[0];
            if (model.JobGrades != null && model.JobGrades.Any())
            {
                grades = model.JobGrades.Split(",").Select(int.Parse).ToArray();
            }

            int[] titles = new int[0];
            if (model.JobTitles != null && model.JobTitles.Any())
            {
                titles = model.JobTitles.Split(",").Select(int.Parse).ToArray();
            }
            var users = (await _courseRegisterService.GetEmployee(model.EmployeeId, model.EmployeeName, departments.ToList(), sections.ToList(),
                grades.ToList(), titles.ToList(), courseID, classID)).Data as List<LDMS_M_User>;
            var userView = users.Select(user => CovertToView(user)).ToList();
            return Response(new ServiceResult(userView));
        }

        private EmployeeModel CovertToView(LDMS_M_User user)
        {
            return new EmployeeModel()
            {
                RowIndex = user.RowIndex,
                CenterId = user.ID_Center,
                DivisionId = user.ID_Division,
                DepartmentId = user.ID_Department,
                SectionId = user.ID_Section,
                JobGradeId = user.ID_JobGrade,
                JobTitleId = user.ID_JobTitle,

                DepartmentName = user.LDMS_M_Department != null ? user.LDMS_M_Department.DepartmentID : "",
                SectionName = user.LDMS_M_Section != null ? user.LDMS_M_Section.SectionID : "",
                JobGrade = user.LDMS_M_JobGrade != null ? user.LDMS_M_JobGrade.JobGradeID : "",
                JobTitle = user.LDMS_M_JobTitle != null ? user.LDMS_M_JobTitle.JobTitleID : "",

                Email = user.Email,
                EmployeeId = user.EmployeeID,
                EmployeeName = user.Name,
                EmployeeSurName = user.Surname,
                Gender = user.Gender,
                Nationality = user.Nationality,
                Password = user.Password,
                PhoneNumber = user.PhoneNumber,
                Remark = user.Remark,
                RoleId = user.ID_Role,
                IsInstructer = user.IsInstructor,
                IsSectionHead = user.IsSectionHead,
                IsAD = user.IsAD
            };
        }


    }
}