using DocumentFormat.OpenXml.Drawing.Charts;
using LDMS.Core;
using LDMS.Identity;
using LDMS.Services;
//using LDMS.WEB.Models;
using LDMS.ViewModels;
using LDMS.WEB.Filters;
using LDMS.WEB.Models;
using LDMS.WEB.Models.Employee;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LDMS.WEB.Controllers
{
    public class ILearningPerformanceController : BaseController
    {
        private readonly ILearningPerformanceService _lService;
        private IWebHostEnvironment _hostingEnvironment;

        public ILearningPerformanceController(ILearningPerformanceService lService, IWebHostEnvironment environment)
        {
            _lService = lService;
            _hostingEnvironment = environment;
        }



        [AuthorizeRole(UserRole.All)]
        [Route("ILearningPerform/Index/")]
        public IActionResult Index()
        {
            return View("~/Views/ILearningPerformance/Index.cshtml");
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("ILearningPerform/GetCourse")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetCourse(string platformID)
        {
            var ret = _lService.GetCourseByPlatformID(platformID);

            return Json(ret);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpPost]
        [Route("ILearningPerform/GetTotal")]
        [AutoValidateAntiforgeryToken]
        public IActionResult GetTotal(string platformID, string courseID, string year, string q1, string q2, string q3, string q4)
        {
            if (platformID == "null") platformID = ""; if (courseID == "null") courseID = "";
            var ret = _lService.GetTotal(platformID, courseID, year, q1, q2, q3, q4, JwtManager.Instance.GetUserId(HttpContext.Request));

            HttpContext.Session.SetString("ILR_CostSp", JsonConvert.SerializeObject((object)ret[1]));
            HttpContext.Session.SetString("ILR_Progress", JsonConvert.SerializeObject((object)ret[2]));
            

            return Json(ret[0]);
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("ILearningPerform/GetCostSpend")]
        public IActionResult GetCostSpend()
        {
            dynamic ret = JsonConvert.DeserializeObject<dynamic>( HttpContext.Session.GetString("ILR_CostSp"));

            StringBuilder sb = new StringBuilder();
            StringBuilder sbCate = new StringBuilder();
            StringBuilder sbInvest = new StringBuilder();
            StringBuilder sbQ = new StringBuilder();
            StringBuilder sbL = new StringBuilder();

            if (ret.Count > 0)
            {
                for(int i =0; i < ret.Count; i++)
                {
                    sbCate.Append("'"+ret[i].MonthAttr+"', ");

                    if (ret[i].Invest != null) sbInvest.Append(ret[i].Invest + ", "); else sbInvest.Append("0, ");
                    if (ret[i].Qualify != null) sbQ.Append(ret[i].Qualify + ", "); else sbQ.Append("0, ");
                    if (ret[i].Loss != null) sbL.Append(ret[i].Loss + ", "); else sbL.Append("0, ");
                }

                sb.Append(@"Highcharts.chart('divCostSpend', {
                chart: {
                    height: 320,
                    type: 'line'
                },
                title: {
                    text: 'Total Learning Cost Spending',
                    align: 'left',
                    x: 20
                },
                subtitle: {
                    text: ''
                },
                credits: {
                    enabled: false
                },
                exporting: {
                    enabled: false
                },
                xAxis: {
                    categories: [" + sbCate.ToString()+ @"]
                },
                yAxis: {
                    title: {
                        text: 'Baht'
                    }
                },
                plotOptions: {
                    line: {
                        dataLabels: {
                            enabled: true
                        }
                    }
                },
                series: [{
                    name: 'Invest',
                    color: '#4774c5',
                    data: ["+ sbInvest.ToString() + @"]
                },
                {
                    name: 'Qualified',
                    color: '#02b151',
                    data: ["+sbQ.ToString() +@"]
                },
                {
                    name: 'Lost',
                    color: '#ff0505',
                    data: ["+ sbL.ToString() +@"]
                }]
            });");
            }

            return new JavaScriptResult(sb.ToString());
        }


        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("ILearningPerform/GetLearningProgress")]
        public IActionResult GetLearningProgress()
        {
            dynamic ret = JsonConvert.DeserializeObject<dynamic>(HttpContext.Session.GetString("ILR_Progress"));
            int iOverDue = 0;
            int iNotStart = 0;
            int iOnProgress = 0;
            int iCompleted = 0;

            for(int i = 0; i < ret.Count; i++)
            {
                switch(ret[i].CourseStatus.ToString())
                {
                    case "COMPLETED": iCompleted += 1;  break;
                    case "Not Start": iNotStart += 1; break;
                    case "ON Progress": iOnProgress += 1; break;
                    case "OverDue": iOverDue += 1; break;
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(@"Highcharts.chart('divLearningProgress', {
                chart: {
                    height: 320,
                    plotBackgroundColor: null,
                    plotBorderWidth: 0,
                    plotShadow: false
                },
                exporting: {
                    enabled: false
                },
                credits: {
                    enabled: false
                },
                title: {
                    text: '"+ iCompleted + @"%',
                    align: 'center',
                    verticalAlign: 'middle',
                    y: 60
                },
                tooltip: {
                    pointFormat: '{series.name}: <b>{point.percentage:.0f}</b>'
                },
                accessibility: {
                    point: {
                        valueSuffix: '%'
                    }
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        dataLabels: {
                            enabled: true,
                            distance: -50,
                            format: '<b>{point.name}</b>: {point.percentage:.1f}',
                            style: {
                                fontWeight: 'bold',
                                color: 'white'
                            }
                        },
                        startAngle: -90,
                        endAngle: 90,
                        center: ['50%', '75%'],
                        size: '110%',
                    }
                },
                series: [{
                    type: 'pie',
                    name: 'Learning Progress',
                    innerSize: '50%',
                    colorByPoint: true,
                    data: [
                        { name: 'OverDue', y: "+ iOverDue + @", color: '#ff0000', index : 1 },
                        { name: 'Not Start', y: " + iNotStart + @", color: '#d0cece', index: 2 },
                        { name: 'On Progress', y: " + iOnProgress + @", color: '#ffff00', index: 3 },
                        { name: 'Completed', y: " + iCompleted + @", color: '#00b050', index: 4}
                    ]
                }]
            });");

            return new JavaScriptResult(sb.ToString());
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("ILearningPerform/GetGPlatform")]
        public IActionResult GetGPlatform(string sort)
        {
            dynamic ret = JsonConvert.DeserializeObject<dynamic>(HttpContext.Session.GetString("ILR_Progress"));
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("Platform");
            dt.Columns.Add("CourseStatus");
            for(int i = 0; i < ret.Count; i++)
            {
                dt.Rows.Add(new object[] { ret[i].PlatformName.ToString(), ret[i].CourseStatus.ToString() });
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder sbCate = new StringBuilder();
            StringBuilder sbNotStart = new StringBuilder();
            StringBuilder sbOnProgress = new StringBuilder();
            StringBuilder sbComplete = new StringBuilder();
            StringBuilder sbOverDue = new StringBuilder();

            DataView dv = dt.DefaultView;
            dv.Sort = "Platform";
            string platform = "";
            int iNotStart = 0, iOnProgress = 0, iComplete = 0, iOverDue = 0;
            System.Data.DataTable dtResult = new System.Data.DataTable();
            dtResult.Columns.Add("Platform");
            dtResult.Columns.Add("iNotStart",typeof(int));
            dtResult.Columns.Add("iOnProgress", typeof(int));
            dtResult.Columns.Add("iComplete", typeof(int));
            dtResult.Columns.Add("iOverDue", typeof(int));
            int rowIndex = -1;
            for(int i =0; i < dv.Count; i++)
            {
                if(platform != dv[i]["Platform"].ToString())
                {
                    if (platform != "")
                    {
                        dtResult.Rows[rowIndex]["iNotStart"] = iNotStart;
                        dtResult.Rows[rowIndex]["iOnProgress"] = iOnProgress;
                        dtResult.Rows[rowIndex]["iComplete"] = iComplete;
                        dtResult.Rows[rowIndex]["iOverDue"] = iOverDue;

                        iNotStart = 0; iOnProgress = 0; iComplete = 0; iOverDue = 0;
                    }

                    dtResult.Rows.Add( dtResult.NewRow() );
                    rowIndex++;

                    platform = dv[i]["Platform"].ToString();
                    dtResult.Rows[rowIndex]["Platform"] = platform;
                }

                switch (dt.Rows[i]["CourseStatus"].ToString())
                {
                    case "COMPLETED": iComplete += 1; break;
                    case "Not Start": iNotStart += 1; break;
                    case "ON Progress": iOnProgress += 1; break;
                    case "OverDue": iOverDue += 1; break;
                }
            }

            dtResult.Rows[rowIndex]["iNotStart"] = iNotStart;
            dtResult.Rows[rowIndex]["iOnProgress"] = iOnProgress;
            dtResult.Rows[rowIndex]["iComplete"] = iComplete;
            dtResult.Rows[rowIndex]["iOverDue"] = iOverDue;

            DataView dvResult = dtResult.DefaultView;
            if(sort == "1")
                dvResult.Sort = "iComplete DESC";
            else
                dvResult.Sort = "iOverDue DESC";

            int count = dvResult.Count > 5? 5 : dvResult.Count;
            for (int i = 0; i < count; i++)
            {
                sbCate.Append("'"+ dvResult[i]["Platform"].ToString() + "',");
                sbNotStart.Append(dvResult[i]["iNotStart"].ToString() + ", ");
                sbOnProgress.Append(dvResult[i]["iOnProgress"].ToString() + ", ");
                sbComplete.Append(dvResult[i]["iComplete"].ToString() + ", ");
                sbOverDue.Append(dvResult[i]["iOverDue"].ToString() + ", ");
            }
        
            sb.Append(@"Highcharts.chart('divPlatform', {
                  chart: {
                    height: 320,
                    type: 'bar'
                  },
                  title: {
                    text: ''
                  },
                 exporting: {
                    enabled: false
                 },
                 credits: {
                    enabled: false
                 },
                  xAxis: {
                    categories: [" + sbCate.ToString()+ @"]
                  },
                  yAxis: {
                    min: 0,
                    title: {
                      text: ''
                    }
                  },
                  stackLabels: {
                      enabled: true,
                  },
                  tooltip: {
                    pointFormat: '<span style=\""color:{ series.color}\"">{series.name}</span>: <b>{point.y}</b> ({point.percentage:.0f}%)<br/>',
                    shared: true
                  },
                  plotOptions: {
                    bar: {
                      stacking: 'percent',
                      dataLabels: {
                            enabled: true
                      }
                    }
                },
                  series: [{
                    name: 'Not Start',
                    color: '#d0cece', index: 41,
                    data: [" + sbNotStart.ToString() + @"]
                  }, {
                    name: 'On Progress',
                    color: '#ffff00', index: 3,
                    data: [" + sbOnProgress.ToString() + @"]
                  }, {
                    name: 'Completed',
                    color: '#00b050', index: 2,
                    data: [" + sbComplete.ToString() + @"]
                  }, {
                    name: 'Over Due',
                    color: '#ff0000', index : 1,
                    data: [" + sbOverDue.ToString() + @"]
                  }]
                });");

            return new JavaScriptResult(sb.ToString());
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("ILearningPerform/GetGArea")]
        public IActionResult GetGArea(string sort)
        {
            dynamic ret = JsonConvert.DeserializeObject<dynamic>(HttpContext.Session.GetString("ILR_Progress"));
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("JobTitleName_TH");
            dt.Columns.Add("CourseStatus");
            for (int i = 0; i < ret.Count; i++)
            {
                dt.Rows.Add(new object[] { ret[i].JobTitleName_TH.ToString(), ret[i].CourseStatus.ToString() });
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder sbCate = new StringBuilder();
            StringBuilder sbNotStart = new StringBuilder();
            StringBuilder sbOnProgress = new StringBuilder();
            StringBuilder sbComplete = new StringBuilder();
            StringBuilder sbOverDue = new StringBuilder();

            DataView dv = dt.DefaultView;
            dv.Sort = "JobTitleName_TH";
            string JobTitleName_TH = "";
            int iNotStart = 0, iOnProgress = 0, iComplete = 0, iOverDue = 0;
            System.Data.DataTable dtResult = new System.Data.DataTable();
            dtResult.Columns.Add("JobTitleName_TH");
            dtResult.Columns.Add("iNotStart", typeof(int));
            dtResult.Columns.Add("iOnProgress", typeof(int));
            dtResult.Columns.Add("iComplete", typeof(int));
            dtResult.Columns.Add("iOverDue", typeof(int));
            int rowIndex = -1;
            for (int i = 0; i < dv.Count; i++)
            {
                if (JobTitleName_TH != dv[i]["JobTitleName_TH"].ToString())
                {
                    if (JobTitleName_TH != "")
                    {
                        dtResult.Rows[rowIndex]["iNotStart"] = iNotStart;
                        dtResult.Rows[rowIndex]["iOnProgress"] = iOnProgress;
                        dtResult.Rows[rowIndex]["iComplete"] = iComplete;
                        dtResult.Rows[rowIndex]["iOverDue"] = iOverDue;

                        iNotStart = 0; iOnProgress = 0; iComplete = 0; iOverDue = 0;
                    }

                    dtResult.Rows.Add(dtResult.NewRow());
                    rowIndex++;

                    JobTitleName_TH = dv[i]["JobTitleName_TH"].ToString();
                    dtResult.Rows[rowIndex]["JobTitleName_TH"] = JobTitleName_TH;
                }

                switch (dt.Rows[i]["CourseStatus"].ToString())
                {
                    case "COMPLETED": iComplete += 1; break;
                    case "Not Start": iNotStart += 1; break;
                    case "ON Progress": iOnProgress += 1; break;
                    case "OverDue": iOverDue += 1; break;
                }
            }

            dtResult.Rows[rowIndex]["iNotStart"] = iNotStart;
            dtResult.Rows[rowIndex]["iOnProgress"] = iOnProgress;
            dtResult.Rows[rowIndex]["iComplete"] = iComplete;
            dtResult.Rows[rowIndex]["iOverDue"] = iOverDue;

            DataView dvResult = dtResult.DefaultView;
            if (sort == "1")
                dvResult.Sort = "iComplete DESC";
            else
                dvResult.Sort = "iOverDue DESC";

            int count = dvResult.Count > 5 ? 5 : dvResult.Count;
            for (int i = 0; i < count; i++)
            {
                sbCate.Append("'" + dvResult[i]["JobTitleName_TH"].ToString() + "',");
                sbNotStart.Append(dvResult[i]["iNotStart"].ToString() + ", ");
                sbOnProgress.Append(dvResult[i]["iOnProgress"].ToString() + ", ");
                sbComplete.Append(dvResult[i]["iComplete"].ToString() + ", ");
                sbOverDue.Append(dvResult[i]["iOverDue"].ToString() + ", ");
            }

            sb.Append(@"Highcharts.chart('divArea', {
                  chart: {
                    height: 320,
                    type: 'bar'
                  },
                  title: {
                    text: ''
                  },
                 exporting: {
                    enabled: false
                 },
                 credits: {
                    enabled: false
                 },
                  xAxis: {
                    categories: [" + sbCate.ToString() + @"]
                  },
                  yAxis: {
                    min: 0,
                    title: {
                      text: ''
                    }
                  },
                  stackLabels: {
                      enabled: true,
                  },
                  tooltip: {
                    pointFormat: '<span style=\""color:{ series.color}\"">{series.name}</span>: <b>{point.y}</b> ({point.percentage:.0f}%)<br/>',
                    shared: true
                  },
                  plotOptions: {
                    bar: {
                      stacking: 'percent',
                      dataLabels: {
                            enabled: true
                      }
                    }
                },
                  series: [{
                    name: 'Not Start',
                    color: '#d0cece', index: 4,
                    data: [" + sbNotStart.ToString() + @"]
                  }, {
                    name: 'On Progress',
                    color: '#ffff00', index: 3,
                    data: [" + sbOnProgress.ToString() + @"]
                  }, {
                    name: 'Completed',
                    color: '#00b050', index: 2,
                    data: [" + sbComplete.ToString() + @"]
                  }, {
                    name: 'Over Due',
                    color: '#ff0000', index : 1,
                    data: [" + sbOverDue.ToString() + @"]
                  }]
                });");

            return new JavaScriptResult(sb.ToString());
        }

        [AuthorizeRole(UserRole.All)]
        [HttpGet]
        [Route("ILearningPerform/GetGJobLevel")]
        public IActionResult GetGJobLevel()
        {
            dynamic ret = JsonConvert.DeserializeObject<dynamic>(HttpContext.Session.GetString("ILR_Progress"));
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("JobGradeName_TH");
            dt.Columns.Add("CourseStatus");
            for (int i = 0; i < ret.Count; i++)
            {
                dt.Rows.Add(new object[] { ret[i].JobGradeName_TH.ToString(), ret[i].CourseStatus.ToString() });
            }

            StringBuilder sb = new StringBuilder();
            StringBuilder sbCate = new StringBuilder();
            StringBuilder sbNotStart = new StringBuilder();
            StringBuilder sbOnProgress = new StringBuilder();
            StringBuilder sbComplete = new StringBuilder();
            StringBuilder sbOverDue = new StringBuilder();

            DataView dv = dt.DefaultView;
            dv.Sort = "JobGradeName_TH";
            string JobGradeName_TH = "";
            int iNotStart = 0, iOnProgress = 0, iComplete = 0, iOverDue = 0;
            System.Data.DataTable dtResult = new System.Data.DataTable();
            dtResult.Columns.Add("JobGradeName_TH");
            dtResult.Columns.Add("iNotStart", typeof(int));
            dtResult.Columns.Add("iOnProgress", typeof(int));
            dtResult.Columns.Add("iComplete", typeof(int));
            dtResult.Columns.Add("iOverDue", typeof(int));
            int rowIndex = -1;
            for (int i = 0; i < dv.Count; i++)
            {
                if (JobGradeName_TH != dv[i]["JobGradeName_TH"].ToString())
                {
                    if (JobGradeName_TH != "")
                    {
                        dtResult.Rows[rowIndex]["iNotStart"] = iNotStart;
                        dtResult.Rows[rowIndex]["iOnProgress"] = iOnProgress;
                        dtResult.Rows[rowIndex]["iComplete"] = iComplete;
                        dtResult.Rows[rowIndex]["iOverDue"] = iOverDue;

                        iNotStart = 0; iOnProgress = 0; iComplete = 0; iOverDue = 0;
                    }

                    dtResult.Rows.Add(dtResult.NewRow());
                    rowIndex++;

                    JobGradeName_TH = dv[i]["JobGradeName_TH"].ToString();
                    dtResult.Rows[rowIndex]["JobGradeName_TH"] = JobGradeName_TH;
                }

                switch (dt.Rows[i]["CourseStatus"].ToString())
                {
                    case "COMPLETED": iComplete += 1; break;
                    case "Not Start": iNotStart += 1; break;
                    case "ON Progress": iOnProgress += 1; break;
                    case "OverDue": iOverDue += 1; break;
                }
            }

            dtResult.Rows[rowIndex]["iNotStart"] = iNotStart;
            dtResult.Rows[rowIndex]["iOnProgress"] = iOnProgress;
            dtResult.Rows[rowIndex]["iComplete"] = iComplete;
            dtResult.Rows[rowIndex]["iOverDue"] = iOverDue;

            DataView dvResult = dtResult.DefaultView;
            dvResult.Sort = "JobGradeName_TH ASC";

            int count = dvResult.Count > 5 ? 5 : dvResult.Count;
            for (int i = 0; i < count; i++)
            {
                sbCate.Append("'" + dvResult[i]["JobGradeName_TH"].ToString() + "',");
                sbNotStart.Append(dvResult[i]["iNotStart"].ToString() + ", ");
                sbOnProgress.Append(dvResult[i]["iOnProgress"].ToString() + ", ");
                sbComplete.Append(dvResult[i]["iComplete"].ToString() + ", ");
                sbOverDue.Append(dvResult[i]["iOverDue"].ToString() + ", ");
            }

            sb.Append(@"Highcharts.chart('divJobLevel', {
                  chart: {
                    height: 320,
                    type: 'column'
                  },
                  title: {
                    text: 'By Job Level',
                    align: 'left',
                    x: 20
                  },
                 exporting: {
                    enabled: false
                 },
                 credits: {
                    enabled: false
                 },
                  xAxis: {
                    categories: [" + sbCate.ToString() + @"]
                  },
                  yAxis: {
                    min: 0,
                    title: {
                      text: ''
                    }
                  },
                  stackLabels: {
                      enabled: true,
                  },
                  tooltip: {
                    pointFormat: '<span style=\""color:{ series.color}\"">{series.name}</span>: <b>{point.y}</b> ({point.percentage:.0f}%)<br/>',
                    shared: true
                  },
                  plotOptions: {
                    column: {
                      stacking: 'percent',
                      dataLabels: {
                            enabled: true
                      }
                    }
                },
                  series: [{
                    name: 'Not Start',
                    color: '#d0cece', index: 4,
                    data: [" + sbNotStart.ToString() + @"]
                  }, {
                    name: 'On Progress',
                    color: '#ffff00', index: 3,
                    data: [" + sbOnProgress.ToString() + @"]
                  }, {
                    name: 'Completed',
                    color: '#00b050', index: 2,
                    data: [" + sbComplete.ToString() + @"]
                  }, {
                    name: 'Over Due',
                    color: '#ff0000', index : 1,
                    data: [" + sbOverDue.ToString() + @"]
                  }]
                });");

            return new JavaScriptResult(sb.ToString());
        }
    }

    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";
        }
    }
}