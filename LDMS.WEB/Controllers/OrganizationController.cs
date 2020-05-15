using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LDMS.Core;
using LDMS.Services;
using LDMS.WEB.Filters;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LDMS.WEB.Controllers
{
    public class OrganizationController : BaseController
    {
        private readonly MasterService MasterService;
        private readonly UserService UserService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private string _filePath;
        public OrganizationController(MasterService masterService, UserService userService,IHostingEnvironment hostingEnvironment)
        {
            MasterService = masterService;
            UserService = userService;
            _hostingEnvironment = hostingEnvironment;
            _filePath = _hostingEnvironment.WebRootPath;
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
            var result = await UserService.SearchOrganizationEmployee(departmentId, sectionId, keyword);
            if (!result.IsOk)
            {
                return Response(result);
            }
            var list = result.Data as List<ViewModels.EmployeeSectionView>;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Section");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Employee ID";
                worksheet.Cell(currentRow, 2).Value = "Employee Name";
                worksheet.Cell(currentRow, 3).Value = "Job Grade";
                worksheet.Cell(currentRow, 4).Value = "Job Title";
                worksheet.Cell(currentRow, 5).Value = "Section";
                foreach (var user in list)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = user.EmployeeID;
                    worksheet.Cell(currentRow, 2).Value = user.FullName;
                    worksheet.Cell(currentRow, 3).Value = user.JobGrade;
                    worksheet.Cell(currentRow, 4).Value = user.JobTitle;
                    worksheet.Cell(currentRow, 5).Value = user.LDMS_M_Section.SectionID;
                }
                string fileName = System.Guid.NewGuid().ToString()+ "_Section.xlsx";
                string filepath = Path.Combine(_filePath, fileName); 
                using (FileStream fileStream = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite))
                {
                    workbook.SaveAs(fileStream);
                }
                return Response(new ServiceResult(fileName)); 
            }
        }
        [HttpGet]
        [Route("Organization/Download")]
        public ActionResult Download(string fileName)
        {
            string filepath = Path.Combine(_filePath, fileName);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(filepath);
            try
            {
                System.IO.File.Delete(filepath);
            }
            catch
            {

            }
            return File(fileByteArray, "application/vnd.ms-excel", fileName);
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