using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDMS.Core;
using LDMS.Services;
using LDMS.WEB.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LDMS.WEB.Controllers
{
    public class MasterController : BaseController
    {
        private readonly MasterService MasterService; 

        public MasterController(MasterService masterService, UserService userService)
        {
            MasterService = masterService; 
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllPlants")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllPlants()
        {
            return Response(await MasterService.GetAllPlants());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllCenters")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllCenters()
        {
            return Response(await MasterService.GetAllCenters());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllCentersByPlant")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllCenters(int plantId)
        {
            return Response(await MasterService.GetAllCenters(plantId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllDivisions")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllDivisions()
        {
            return Response(await MasterService.GetAllDivisions());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllDivisionsByCenter")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllDivisions(int centerId)
        {
            return Response(await MasterService.GetAllDivisions(centerId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllDepartments")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllDepartments()
        {
            return Response(await MasterService.GetAllDepartments());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllDepartmentsByDivision")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllDepartments(int divisionId)
        {
            return Response(await MasterService.GetAllDepartments(divisionId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllSections")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllSections()
        {
            return Response(await MasterService.GetAllSections());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllSectionsByDepartment")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllSections(int departmentId)
        {
            return Response(await MasterService.GetAllSections(departmentId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/Department")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetDepartment(int departmentId)
        {
            return Response(await MasterService.GetDepartment(departmentId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/Division")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetDivision(int divisionId)
        {
            return Response(await MasterService.GetDivision(divisionId));
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllJobGrades")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllJobGrades()
        {
            return Response(await MasterService.GetAllJobGrades());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllJobTitles")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllJobTitles()
        {
            return Response(await MasterService.GetAllJobTitles());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllRoles")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllRoles()
        {
            return Response(await MasterService.GetAllRoles());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllPlatforms")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllPlatforms()
        {
            return Response(await MasterService.GetAllPlatforms());
        }

        [AuthorizeRole(UserRole.All)]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [Route("Master/GetAllCourses")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAllCourses()
        {
            return Response(await MasterService.GetAllCourses());
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Master/GetCodeLookup")]
        [AutoValidateAntiforgeryToken] 
        public IActionResult GetCodeLookups(string tableName, string fieldName)
        {
            return Json(MasterService.GetCodeLookups(tableName, fieldName));
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Master/GetProvinces")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetProvinces()
        {
            return Json(MasterService.GetProvinces());
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Master/GetAmphurs")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetAmphurs(string provinceID)
        {
            return Json(MasterService.GetAmphurs(provinceID));
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Master/GetDistricts")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetDistricts(string provinceID, string amphurID)
        {
            return Json(MasterService.GetDistricts(provinceID, amphurID));
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Master/RedirectMenu")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetRedirectMenuBySubmenuId(string submenuId)
        {
            return Json(MasterService.GetRedirectMenuBySubmenuId(submenuId));
        }

    }
}