using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDMS.Core;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Mvc;

namespace LDMS.WEB.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly UserService UserService;

        public EmployeeController( UserService userService)
        {
            UserService = userService;
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
                IsAD = user.IsAD,
                Id_Employee = user.ID_User
            };
        }


        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpGet]
        [Route("Employee/SearchEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Employees(SearchEmployeeModel model)
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
            var users = (await UserService.GetAll(model.EmployeeId, model.EmployeeName, departments.ToList(),sections.ToList(), grades.ToList(), titles.ToList())).Data as List<LDMS_M_User>;
            var userView = users.Select(user => CovertToView(user)).ToList();
            return Response(new ServiceResult(userView));
        }


        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Employee/ReadEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ReadEmployee(string employeeId)
        {
            var user = (await UserService.GetUserByEmployeeId(employeeId)).Data as LDMS_M_User;
            if (user != null)
            {
                var userView = CovertToView(user);
                return Response(new ServiceResult(userView));
            }
            else
            {
                return Response(new ServiceResult(new EmployeeModel()));
            }
        }


        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR, UserRole.AdminDepartment, UserRole.UserAdmin)]
        [HttpGet]
        [Route("Employee/EmployeeByDepartment")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ReadEmployeeByDepartment(int departmentId)
        {
            return Response(new ServiceResult((await UserService.GetAllEmployeeByDepartmentId(departmentId))));
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR, UserRole.AdminDepartment, UserRole.UserAdmin)]
        [HttpGet]
        [Route("Employee/EmployeeBySection")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ReadEmployeeBySection(int sectionId)
        {
            return Response(new ServiceResult((await UserService.GetAllEmployeeBySectionId(sectionId))));
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR, UserRole.AdminDepartment, UserRole.UserAdmin)]
        [HttpGet]
        [Route("Employee/SearchEmployeeName")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> SearchEmployeeName(string EmployeeName)
        {
            var users = await UserService.GetAll(null, EmployeeName, null);
            return Json(users);
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpPost]
        [Route("Employee/CreateEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SaveEmployee(EmployeeModel model)
        {
            try
            {
                LDMS_M_User user = new LDMS_M_User()
                {
                    Email = model.Email,
                    Name = model.EmployeeName,
                    Nationality = model.Nationality,
                    PhoneNumber = model.PhoneNumber,
                    Surname = model.EmployeeSurName,
                    IsAD = true,
                    EmployeeID = model.EmployeeId,
                    Gender = model.Gender,
                    ID_Center = model.CenterId,
                    ID_Department = model.DepartmentId,
                    ID_Division = model.DivisionId,
                    ID_JobGrade = model.JobGradeId,
                    ID_JobTitle = model.JobTitleId,
                    IsActive = true,
                    ID_Role = model.RoleId,
                    ID_Section = model.SectionId,
                    IsInstructor = model.IsInstructer,
                    IsSectionHead = model.IsSectionHead,
                    Password = model.Password,
                    Remark = model.Remark
                }; 
                return Response(await UserService.CreateUser(user));
            }
            catch (Exception exp)
            {
                return Response(new ServiceResult(exp));
            }
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpPost]
        [Route("Employee/UpdateEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> UpdateEmployee(EmployeeModel model)
        {
            try
            {
                LDMS_M_User user = new LDMS_M_User()
                {
                    Email = model.Email,
                    Name = model.EmployeeName,
                    Nationality = model.Nationality,
                    PhoneNumber = model.PhoneNumber,
                    Surname = model.EmployeeSurName,
                    IsAD = true,
                    EmployeeID = model.EmployeeId,
                    Gender = model.Gender,
                    ID_Center = model.CenterId,
                    ID_Department = model.DepartmentId,
                    ID_Division = model.DivisionId,
                    ID_JobGrade = model.JobGradeId,
                    ID_JobTitle = model.JobTitleId,
                    IsActive = true,
                    ID_Role = model.RoleId,
                    ID_Section = model.SectionId,
                    IsInstructor = model.IsInstructer,
                    IsSectionHead = model.IsSectionHead,
                    Password = model.Password,
                    Remark = model.Remark,
                    ID_User = model.Id_Employee
                }; 
                return Response(await UserService.UpdateUser(user));
            }
            catch (Exception exp)
            {
                return Response(new ServiceResult(exp));
            }
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpDelete]
        [Route("Employee/RemoveEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> RemoveEmployee(string employeeId)
        {
            try
            {
                return Response(await UserService.DeleteUser(employeeId));
            }
            catch (Exception exp)
            {
                return Response(new ServiceResult(exp));
            }
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpPost]
        [Route("Employee/ResetPassword")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ResetPassword(string employeeId)
        {
            try
            {
                return Response(await UserService.ResetPassword(employeeId));
            }
            catch (Exception exp)
            {
                return Response(new ServiceResult(exp));
            }
        }
    }
}