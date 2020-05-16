using LDMS.Core;
using LDMS.Services;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class IDPController : Controller
    {
        private readonly IDPService _IDPService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<IDPController> _logger;

        public IDPController(ILogger<IDPController> logger, IDPService idpService, IWebHostEnvironment environment)
        {
            _logger = logger;
            _IDPService = idpService;
            _hostingEnvironment = environment;
        }

        public IActionResult Index()
        {
            var myIDP = _IDPService.GetMyIDP();
            return View("/Views/IDP/Index.cshtml", myIDP);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("IDP/Detail/{ID}")]
        public IActionResult Detail(string ID)
        {
            var myIDP = _IDPService.GetMyIDP();
            myIDP.ID_SubTopic = ID;

            return View("/Views/IDP/Detail.cshtml", myIDP);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("IDP/Topisc")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Topisc(string ID_IDP, string Year)
        {
            string sortOrder = Request.Form["order[0][dir]"];
            string sortIndex = Request.Form["order[0][column]"];

            string sortColumn = "";
            if (sortIndex != "0") sortColumn = Request.Form["columns[" + sortIndex + "][data]"].ToString();
            Paging_Search criteria = new Paging_Search();
            criteria.PageNum = int.Parse(Request.Form["start"]) / int.Parse(Request.Form["length"]) + 1;
            criteria.PageSize = int.Parse(Request.Form["length"]);
            criteria.SortField = sortIndex;
            criteria.SortOrder = sortOrder;


            var topics = _IDPService.GetTopics(ID_IDP, Year, criteria);

            return Json(topics);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("IDP/GetTopisc")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetTopisc(string ID_IDP, string Year)
        {

            var topics = _IDPService.GetTopicsList(ID_IDP, Year);

            for (int i = 0; i < topics.Count; i++)
            {
                topics[i].listSubTopic = _IDPService.GetSubTopicsList(topics[i].ID.ToString());
            }


            return Json(topics);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("IDP/GetSubTopicsID")]
        public IActionResult GetSubTopicsID(string ID_IDP_SubTopic)
        {
           var subTopic = _IDPService.GetSubTopicsID(ID_IDP_SubTopic);
           return Json(subTopic);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("IDP/InsertReviewHead")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InsertReviewHead(string ID_IDP_SubTopic, string EmployeeReport, IFormFile fileAttach)
        {

            string attachFile = "";
            string attachName = "";
            if (fileAttach != null)
            {
                var fileName = ContentDispositionHeaderValue.Parse(fileAttach.ContentDisposition).FileName.Replace("\"", "");
                string filesPath = _hostingEnvironment.WebRootPath + "\\Uploads\\IDP\\" + ID_IDP_SubTopic;

                fileName = fileName.Contains("\\")
                    ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName.Trim('"');
                fileName = ID_IDP_SubTopic + "_" + fileName;

                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

                var fullFilePath = Path.Combine(filesPath, fileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    fileAttach.CopyTo(stream);
                }

                attachFile = Path.Combine("\\Uploads\\IDP\\" + ID_IDP_SubTopic, fileName);
                attachName = fileName;
            }




            LDMS_T_IDP_ReviewHead idpHeader = _IDPService.GetIDPReviewHeadCurrent(ID_IDP_SubTopic);
            if( idpHeader == null)
            {
                idpHeader = new LDMS_T_IDP_ReviewHead();
                int ret = _IDPService.CreateIDPHeadReview(ID_IDP_SubTopic, EmployeeReport, attachFile, attachName);
                idpHeader.ID = ret;
                idpHeader.ID_IDP_SubTopic = int.Parse(ID_IDP_SubTopic);
                idpHeader.EmployeeReport = EmployeeReport;
                idpHeader.AttachFilePath = attachFile;
                idpHeader.AttachFileName = attachName;


                LDMS_T_IDP_SubTopic c = new LDMS_T_IDP_SubTopic();
                c = _IDPService.UpdateIDPSubtopicStatus(ID_IDP_SubTopic, "30"); // On Progress*/
            }

            return Json(idpHeader);
        }

        [HttpPost]
        [Route("IDP/GetIDPReviewHead")]
        public IActionResult GetIDPReviewHead(string ID_IDP_SubTopic)
        {
            List<LDMS_T_IDP_ReviewHead> idpHead = _IDPService.GetIDPReviewHead(ID_IDP_SubTopic);

            for( int i = 0; i< idpHead.Count; i++ )
            {
                idpHead[i].listDetail = _IDPService.GetIDPReviewDetail(idpHead[i].ID.ToString());
            }

            return Json(idpHead);
        }


        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("IDP/InsertReviewDetail")]
        //[AutoValidateAntiforgeryToken]
        public IActionResult InsertReviewDetail(string ID_IDP_SubTopic, string ID_IDP_ReviewHead, string PostDetail, IFormFile fileAttach)
        {

            string attachFile = "";
            string attachName = "";
            if (fileAttach != null)
            {
                var fileName = ContentDispositionHeaderValue.Parse(fileAttach.ContentDisposition).FileName.Replace("\"", "");
                string filesPath = _hostingEnvironment.WebRootPath + "\\Uploads\\IDP\\" + ID_IDP_SubTopic + "\\" + ID_IDP_ReviewHead;

                fileName = fileName.Contains("\\")
                    ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1) : fileName.Trim('"');
                fileName = ID_IDP_SubTopic + "_" + ID_IDP_ReviewHead + "_" + fileName;

                if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

                var fullFilePath = Path.Combine(filesPath, fileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    fileAttach.CopyTo(stream);
                }

                attachFile = Path.Combine("\\Uploads\\IDP\\" + ID_IDP_SubTopic + "\\" + ID_IDP_ReviewHead, fileName);
                attachName = fileName;
            }


            /*LDMS_T_Coaching c = new LDMS_T_Coaching();
            c = _CoachingService.UpdateCoachingDetail(ID_Coaching, Topic, "30"); // On Progress*/

            LDMS_T_IDP_ReviewDetail idpHeader = new LDMS_T_IDP_ReviewDetail();

            int ret = _IDPService.CreateIDPDetailReview(ID_IDP_ReviewHead, PostDetail, attachFile, attachName);
            idpHeader.ID = ret;
            idpHeader.ID_IDP_ReviewHead = int.Parse(ID_IDP_ReviewHead);
            idpHeader.PostDetail = PostDetail;
            idpHeader.AttachFilePath = attachFile;
            idpHeader.AttachFileName = attachName;

            return Json(idpHeader);
        }
    }
}