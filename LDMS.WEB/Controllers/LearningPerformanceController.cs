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
    public class LearningPerformanceController : BaseController
    {
        private CourseService CourseService;
        public LearningPerformanceController(CourseService courseService)
        {
            CourseService = courseService;
        }
        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("LearningPerformance/TeamPerformance")]
        public IActionResult TeamPerformance()
        {
            return View();
        }
        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [AuthorizeRole(UserRole.All)]
        [Route("Course/TeamPerformance")]
        public async Task<IActionResult> GetTeamPerformance(int ficialYear, string quater, int plantId, int centerId, int divisionId, int departmentId, int sectionId)
        {
            int[] mquatuer = new int[0];
            if (quater != null && quater != "")
            {
                mquatuer = quater.Split(",").Select(int.Parse).ToArray();
            }
            return Response(await CourseService.GetTeamLearningPerformance(ficialYear, mquatuer, plantId, centerId, divisionId, departmentId, sectionId));
        }
    }
}