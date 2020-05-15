using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDMS.Core;
using LDMS.Services;
using LDMS.WEB.Filters;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        [Route("Organization/Section")]
        public async Task<IActionResult> Section()
        {
            return View();
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/SectionByDepartment")]
        public async Task<IActionResult> SectionByDepartment(int departmentId)
        {
            return Response(await MasterService.GetAllSections(departmentId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/Employees")]
        public async Task<IActionResult> Employees(int departmentId, int sectionId, string keyword)
        {
            var result = await UserService.SearchOrganizationEmployee(departmentId,sectionId,keyword); 
            return PartialView("section/_employeeList", result.Data);
        }
        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/ExportEmployees")]
        public async Task<IActionResult> ExportEmployees(int departmentId, int sectionId, string keyword)
        {
            return Response(await UserService.ExportOrganizationEmployee(departmentId, sectionId, keyword));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Organization/Sections")]
        public async Task<IActionResult> GetAllSections(int departmentId)
        {
            var sections = (await MasterService.GetAllSections(departmentId)).Data as List<ViewModels.LDMS_M_Section>;
            return PartialView("section/_sectionListView", sections);
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/CreateSection")]
        public async Task<IActionResult> CreateSection(ViewModels.LDMS_M_Section model)
        {
            return Response(await MasterService.CreateSection(model));
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/UpdateSection")]
        public async Task<IActionResult> UpdateSection(ViewModels.LDMS_M_Section model)
        {
            return Response(await MasterService.UpdateSection(model));
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/DeleteSection")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            return Response(await MasterService.DeleteSection(sectionId));
        }

        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        [HttpPost]
        [Route("Organization/SectionEmployeeSave")]
        public async Task<IActionResult> SectionEmployeeSave(List<SectionEmployeeSaveModel> models)
        {
            List<ViewModels.LDMS_M_User> userRoles = models.Select(e => new ViewModels.LDMS_M_User()
            {
                EmployeeID = e.EmployeeId,
                ID_Section = e.SectionId.GetValueOrDefault(),
                IsSectionHead = e.IsSectionHead
            }).ToList();
            return Response(await UserService.UpdateUserSection(userRoles));
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("Organization/ImportSection")]
        [AuthorizeRole(UserRole.AdminHR, UserRole.SuperAdmin)]
        public IActionResult ImportOrganizationEmployee(IFormFile file)
        {
            int.TryParse(Request.Form.FirstOrDefault(x => x.Key == "divisionId").Value, out int divisionId);
            int.TryParse(Request.Form.FirstOrDefault(x => x.Key == "departmentId").Value, out int departmentId);
            return Response(UserService.ImportEmployeeSection(file, divisionId, departmentId).Result);
        }

    }
}