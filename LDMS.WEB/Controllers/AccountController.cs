using LDMS.Core;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LDMS.WEB.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserService UserService;
        // GET: /<controller>/
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger, UserService userService)
        {
            UserService = userService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        [Route("Account/Login")]
        public async Task<IActionResult> Login(LoginViewModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userModel);
            }
            var result = (await UserService.Authenticattion(userModel.Username, userModel.Password));
            if (!result.IsOk)
            {
                return Response(result);
            } 
            var user = result.Data as LDMS_M_User; 
            if (user != null && !string.IsNullOrEmpty(user.Token))
            {
                var page = HttpContext.Request.Get("REDIRECTPAGE");
                if (string.IsNullOrEmpty(page))
                {
                    page = "/Home/Index";
                }
                return Response(new ServiceResult(page)); 
            }
            else
            {
                return Response(new ServiceResult(new UnauthorizedAccessException("Invalid Account")));
            }
        }

        [Route("")]
        [Route("Account")]
        [Route("Account/Index")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("Account/ForceChange")] 
        [AuthorizeRole(UserRole.All)]
        public async Task<IActionResult> ForceChangePassword()
        {
            return View();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost] 
        [Route("Account/ChangePassword")]
        public async Task<IActionResult> SubmitChangePassword(string employeeId, string currentPassword,string newpassword)
        {
            var result = (await UserService.ChangePassword(employeeId, currentPassword, newpassword));
            if (!result.IsOk) return Response(result);
            var user = result.Data as LDMS_M_User;
            if (user != null)
            {
                var page = HttpContext.Request.Get("REDIRECTPAGE");
                if (string.IsNullOrEmpty(page))
                {
                    page = "/Home/Index";
                }
                return Response(new ServiceResult(page));
            }
            else
            {
                return Response(new ServiceResult(new UnauthorizedAccessException("Invalid Account")));
            } 
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("Account/AllowGPP")]
        public async Task<IActionResult> AllowGPP(string employeeId, bool IsAllow)
        {
            var user = (await UserService.AllowGPP(employeeId, IsAllow)).Data as LDMS_M_User;
            if (user != null)
            {
                var page = HttpContext.Request.Get("REDIRECTPAGE");
                if (string.IsNullOrEmpty(page))
                {
                    page = "/Home/Index";
                }
                if (!IsAllow)
                {
                    HttpContext.Request.ExpireAllCookies(HttpContext.Response);
                    HttpContext.Session.Clear();
                    page = "/Account/Index";
                }
                return Response(new ServiceResult(page));
            }
            else
            {
                return Response(new ServiceResult(new UnauthorizedAccessException("Invalid Account")));
            }
        }

        [AuthorizeRole(UserRole.All)]
        [Route("Account/Privacy")]        
        public IActionResult Privacy()
        {
            return View();
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("Logout")]
        [Route("Account/Logout")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Request.ExpireAllCookies(HttpContext.Response);
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index");
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpGet]
        [Route("Account/UserManagement")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> UserManagement()
        {
            return View();
        }
        [AuthorizeRole(UserRole.All)]
        [Route("Account/MyProfile")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> MyProfile()
        {
            return View();
        }

        [AuthorizeRole(UserRole.SuperAdmin, UserRole.AdminHR)]
        [HttpGet]
        [Route("Account/SearchEmployee")]
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> SearchEmployee(SearchEmployeeModel model)
        {
            int[] departments = new int[0];
            if (model.Departments != null && model.Departments.Any())
            {
                departments = model.Departments.Split(",").Select(int.Parse).ToArray();
            }
            var users = (await UserService.GetAll(model.EmployeeId, model.EmployeeName, departments.ToList())).Data as List<LDMS_M_User>; 
            return PartialView("_ViewAllUser", users);
        }   
    }
}
