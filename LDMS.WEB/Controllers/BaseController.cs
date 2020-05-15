using LDMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class BaseController : Controller
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        { 
            // logic before action goes here
            Stopwatch stopwacth = new Stopwatch();
            stopwacth.Start(); 
            // the actual action
            await next(); 
            // logic after the action goes here
            stopwacth.Stop(); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected IActionResult Response(ServiceResult result)
        {
            IActionResult response;
            if (result.Code == ServiceResultCodes.Ok)
            {
                response = Ok(result); 
            }
            else if (result.Code == ServiceResultCodes.Bad)
            {
                response = BadRequest(result); 
            }
            else if (result.Code == ServiceResultCodes.Denied)
            {
                response = Unauthorized(result);
            }
            else if (result.Code == ServiceResultCodes.NotFound)
            {
                response = NotFound(result);
            }
            else
            {
                response = BadRequest(result);
            }

            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected IActionResult ResponseMessage(CodeServiceResult result)
        {
            IActionResult response;
            if (result.Code == (int)ServiceResultCodes.Ok)
            {
                response = Ok(result); 
            }
            else if (result.Code == (int)ServiceResultCodes.Bad)
            {
                response = BadRequest(result); 
            }
            else if (result.Code == (int)ServiceResultCodes.Denied)
            {
                response = Unauthorized(result);
            }
            else if (result.Code == 44)
            {
                response = NotFound(result);
            }
            else
            {
                response = BadRequest(result);
            }

            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected IActionResult NoValueResponseMessage(ServiceResultNoValue result)
        {
            if (result.Code == (int)ServiceResultCodes.Ok)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        } 

    }
}