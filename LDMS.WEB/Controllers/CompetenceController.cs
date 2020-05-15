using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LDMS.Core;
using LDMS.Services;
using LDMS.WEB.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace LDMS.WEB.Controllers
{
    public class CompetenceController : BaseController
    {
        private string _filePath;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly CompetenceAnalyticService CompetenceAnalyticService;
        public CompetenceController(CompetenceAnalyticService competenceAnalyticService, IHostingEnvironment hostingEnvironment)
        {
            CompetenceAnalyticService = competenceAnalyticService;
            _hostingEnvironment = hostingEnvironment;
            _filePath = _hostingEnvironment.WebRootPath;
        }

        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Index")]
        [Route("Competence")]
        public IActionResult Index()
        {
            ViewData["Title"] = "My Team Competence Analytic";
            ViewData["MainTitle"] = "My Team / My Monitoring";
            return View();
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Competence")]
        public async Task<IActionResult> ReadCompetence(int competenceId)
        {
            return Response(await CompetenceAnalyticService.ReadById(competenceId));
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpDelete]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Delete")]
        public async Task<IActionResult> Delete(int competenceId)
        {
            return Response(await CompetenceAnalyticService.Delete(competenceId));
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Analytic/{ID}")]
        public IActionResult Analytic(string ID)
        {
            if (string.IsNullOrEmpty(ID))
            {
                Redirect("Competence/Add");
            }
            ViewBag.ID_Analytic = ID;
            ViewData["Title"] = "My Team Competence Analytic";
            ViewData["MainTitle"] = "My Team / My Monitoring";
            return View();
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Export")]
        [HttpPost]
        public async Task<IActionResult> Export(int competenceId, string chartImage)// [FromBody]Microsoft.AspNetCore.Http.IFormFile fileImage)// int competenceId,string chartImage)
        {
           // int.TryParse(Request.Form.FirstOrDefault(x => x.Key == "competenceId").Value, out int competenceId);
            var result = await CompetenceAnalyticService.ReadById(competenceId);
            if (!result.IsOk)
            {
                return Response(result);
            }
            if (result.Data == null)
            {
                return Response(new ServiceResult(new ArgumentNullException("Competence")));
            }
            var competence = result.Data as  ViewModels.TCompetenceAnalytic;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Competence"); 
                worksheet.Range("A1:C1").Merge();
                worksheet.Cell("A1").Value = string.Format("Last Update : {0:dd/MM/yyyy}", competence.LastUpdate);

                worksheet.Range("A3:C3").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("A3").Value = string.Format("Competence Platform : {0}", competence.CompetenceAnalyticName); 

                worksheet.Range("A4:C4").Merge();
                worksheet.Cell("A4").Value = string.Format("Department : {0}", competence.DepartmentName); 

                var expec= worksheet.Range("A5:A9").Merge();
                expec.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                worksheet.Cell("A5").Value = string.Format("Expectatoin Criteria : ");  
                worksheet.Cell("B5").Value = string.Format("5 = {0}", competence.Criteria5); 
                worksheet.Cell("B6").Value = string.Format("4 = {0}", competence.Criteria4); 
                worksheet.Cell("B7").Value = string.Format("3 = {0}", competence.Criteria3); 
                worksheet.Cell("B8").Value = string.Format("2 = {0}", competence.Criteria2); 
                worksheet.Cell("B9").Value = string.Format("1 = {0}", competence.Criteria1);

                worksheet.Range("A10:B10").Merge();
                worksheet.Cell("A10").Value = string.Format("Amount of Topic : {0:N0}", competence.Topics.Count()); 
                worksheet.Range("A11:B11").Merge();
                worksheet.Cell("A11").Value = string.Format("Amount of Employee : {0:N0} ", competence.Employees.Count()); 

                var currentRow = 20;
                var startRow = 20;
                var currentCol = 1;
                worksheet.Cell(currentRow, currentCol).Value = "Knowledge Topic";
                currentCol++; 
                worksheet.Cell(currentRow, currentCol).Value = "Expectatoin";
                currentCol++;
                 
                foreach (var item in competence.Employees.OrderBy(e=>e.EmployeeID))
                {
                    worksheet.Cell(currentRow, currentCol).Value = string.Format("{0} {1}", item.LDMS_M_User.Name, item.LDMS_M_User.Surname);
                    currentCol++;
                }
                foreach (var item in competence.Topics)
                {
                    currentRow++;
                    currentCol = 1;
                    worksheet.Cell(currentRow, currentCol).Value = item.KnowledgeTopicName;
                    currentCol++;
                    worksheet.Cell(currentRow, currentCol).Value = string.Format("{0:N0}", item.Expectatoin);
                    currentCol++;
                    foreach (var emp in competence.Employees.OrderBy(e => e.EmployeeID))
                    {
                        var score = competence.Scores.FirstOrDefault(e => e.ID_CompetenceKnowledgeTopic == item.ID && e.ID_CompetenceEmployee == emp.EmployeeID);
                        var score_value = 0;
                        if (score != null) { score_value = score.Score; }
                        worksheet.Cell(currentRow, currentCol).Value = string.Format("{0:N0}", score_value);
                        currentCol++;
                    }
                }
                var rang=  worksheet.Range(worksheet.Cell(startRow, 1), worksheet.Cell(currentRow, currentCol-1));
                rang.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rang.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                rang.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                rang.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rang.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                rang.Style.Border.TopBorder = XLBorderStyleValues.Thin; 

                worksheet.Range(worksheet.Cell(1, currentCol - 4), worksheet.Cell(1, currentCol-1)).Merge();
                worksheet.Cell(1, currentCol - 4).Value = string.Format("Print Date Time: {0:dd/MM/yyyy HH:mm}",DateTime.Now);

                worksheet.Range(worksheet.Cell(2, 1), worksheet.Cell(2, currentCol)).Merge();
                worksheet.Cell(2, 1).Value = string.Format("Completence Analytic Report: {0}", competence.CompetenceAnalyticName);

                string fileName = Guid.NewGuid().ToString() + "_Competence.xlsx";
                string filepath = Path.Combine(_filePath, fileName);
                using (FileStream fileStream = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite))
                {
                    workbook.SaveAs(fileStream);
                }
                return Response(new ServiceResult(fileName));
            } 
        }
        [HttpGet]
        [Route("Competence/Download")]
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

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Add")]
        public IActionResult Add()
        {
            ViewData["Title"] = "My Team Competence Analytic";
            ViewData["MainTitle"] = "My Team / My Monitoring";
            return View();
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Edit/{ID}")]
        public IActionResult Edit(string ID)
        {
            if (string.IsNullOrEmpty(ID))
            {
                Redirect("Competence/Add");
            }
            ViewBag.ID_Analytic = ID;
            ViewData["Title"] = "My Team Competence Analytic";
            ViewData["MainTitle"] = "My Team / My Monitoring";
            return View();
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpGet]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/Analytics")]
        public async Task<IActionResult> Analytics()
        {
            return Response(await CompetenceAnalyticService.ReadAll());
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpPost]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/CreateCompetence")]
        public async Task<IActionResult> CreateCompetence(ViewModels.TCompetenceAnalytic competenceAnalytic
            , List<ViewModels.TCompetenceAnalyticEmployee> employees
            , List<ViewModels.TCompetenceAnalyticTopic> topics)
        {
            return Response(await CompetenceAnalyticService.CreateCompetence(competenceAnalytic, employees,topics));
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpPost]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/UpdateCompetence")]
        public async Task<IActionResult> UpdateCompetence(ViewModels.TCompetenceAnalytic competenceAnalytic
            , List<ViewModels.TCompetenceAnalyticEmployee> employees
            , List<ViewModels.TCompetenceAnalyticTopic> topics)
        {
            return Response(await CompetenceAnalyticService.UpdateCompetence(competenceAnalytic, employees, topics));
        }

        [ResponseCache(Duration = 1, Location = ResponseCacheLocation.None)]
        [HttpPost]
        [AuthorizeRole(UserRole.All)]
        [Route("Competence/UpdateCompetenceScore")]
        public async Task<IActionResult> UpdateCompetenceScore(int competenceId
            , List<ViewModels.TCompetenceAnalyticScore> scores )
        {
            return Response(await CompetenceAnalyticService.UpdateCompetenceScore(competenceId, scores));
        }
    }
}