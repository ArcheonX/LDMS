﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDMS.Core;
using LDMS.Services;
using LDMS.WEB.Filters;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LDMS.WEB.Controllers
{
    public class OrganizationController : BaseController
    {
        private readonly MasterService MasterService;
        private readonly UserService UserService;

        public OrganizationController(MasterService masterService, UserService userService)
        {
            MasterService = masterService;
            UserService = userService;
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [AutoValidateAntiforgeryToken]
        [Route("Organization/Section")]
        public async Task<IActionResult> Section()
        {
            return View();
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/SectionByDepartment")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SectionByDepartment(int departmentId)
        {
            return Response(await MasterService.GetAllSections(departmentId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/Employees")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Employees(int departmentId, int sectionId, string keyword)
        {
            var grades = (await MasterService.GetAllJobGrades()).Data as List<ViewModels.LDMS_M_JobGrade>;
            var titles = (await MasterService.GetAllJobTitles()).Data as List<ViewModels.LDMS_M_JobTitle>;
            var sections = (await MasterService.GetAllSections(departmentId)).Data as List<ViewModels.LDMS_M_Section>;
            var users = (await UserService.GetAllEmployeeByDepartmentId(departmentId)).Data as List<ViewModels.LDMS_M_User>;
            var employees = users.Select(emp => new Models.Employee.EmployeeSectionView(emp)
            {
                JobGrade = grades.FirstOrDefault(e => e.ID_JobGrade == emp.ID_JobGrade.GetValueOrDefault())?.JobGradeName_EN,
                JobTitle = titles.FirstOrDefault(e => e.ID_JobTitle == emp.ID_JobTitle.GetValueOrDefault())?.JobTitleName_EN,
                LDMS_M_Sections = sections
            }).ToList();
            if (sectionId > 0)
            {
                employees = employees.Where(e => e.LDMS_M_UserRole != null && e.LDMS_M_UserRole.ID_Section == sectionId).ToList();
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                employees = employees.Where(e =>
                (e.Name != null && e.Name.ToLower().StartsWith(keyword.ToLower()))
                || (e.Surname != null && e.Surname.ToLower().StartsWith(keyword.ToLower()))
                || (e.Email != null && e.Email.ToLower().StartsWith(keyword.ToLower()))
                || (e.EmployeeID != null && e.EmployeeID.ToLower().StartsWith(keyword.ToLower()))
                ).ToList();
            }
            int index = 1;
            employees.ForEach(item =>
            {
                item.RowIndex = index;
                index++;
            });
            return PartialView("section/_employeeList", employees);
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/Sections")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllSections(int departmentId)
        {
            var sections = (await MasterService.GetAllSections(departmentId)).Data as List<ViewModels.LDMS_M_Section>;
            return PartialView("section/_sectionListView", sections);
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/CreateSection")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CreateSection(ViewModels.LDMS_M_Section model)
        {
            return Response(await MasterService.CreateSection(model));
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/UpdateSection")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UpdateSection(ViewModels.LDMS_M_Section model)
        {
            return Response(await MasterService.UpdateSection(model));
        }

        [AuthorizeRole(UserRole.AdminHR,UserRole.SuperAdmin)]
        [HttpDelete]
        [Route("Organization/DeleteSection")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            return Response(await MasterService.DeleteSection(sectionId));
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/SectionEmployeeSave")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> SectionEmployeeSave(List<SectionEmployeeSaveModel> models)
        {
            List<ViewModels.LDMS_M_UserRole> userRoles = models.Select(e => new ViewModels.LDMS_M_UserRole()
            {
                EmployeeID = e.EmployeeId,
                ID_Section = e.SectionId,
                IsSectionHead = e.IsSectionHead ? 1 : 0
            }).ToList();
            return Response(await UserService.UpdateUserSection(userRoles));
        }
    }
}