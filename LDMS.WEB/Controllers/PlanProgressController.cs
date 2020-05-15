using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LDMS.Core;
using LDMS.Services;
using LDMS.WEB.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LDMS.WEB.Controllers
{
    public class PlanProgressController : BaseController
    {
        private CourseService CourseService;
        public PlanProgressController(CourseService courseService)
        {
            CourseService = courseService;
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [AuthorizeRole(UserRole.All)]
        [Route("Course/PlanProgress")]
        public async Task<IActionResult> GetPlanAndProgress(string employeeId, int ficialYear, string quater)
        {
            int[] mquatuer = new int[0];
            if (quater != null && quater!="")
            {
                mquatuer = quater.Split(",").Select(int.Parse).ToArray();
            }
            return Response(await CourseService.GetPlanAndProgress(employeeId, ficialYear, mquatuer));
        }

    }
}