using LDMS.Core;
using LDMS.ViewModels;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Dapper;
using System.Linq;
using Microsoft.AspNetCore.Http;
using LDMS.Daos;
using System.Threading.Tasks;
using System;

namespace LDMS.Services
{
    public class ILearningPerformanceService : ILDMSService
    {
        private readonly AppSettings _appSettings;
        public ILearningPerformanceService(IOptions<AppSettings> appSettings,
            ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _appSettings = appSettings.Value;
        }

        public List<dynamic> GetCourseByPlatformID(string platformID)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Platform", platformID);

                List<dynamic> ret = conn.Query<dynamic>("[dbo].[sp_M_Course_SelectByPlatform]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        }


        public List<dynamic> GetTotal(string platformID, string courseID, string year, string q1, string q2, string q3, string q4, string employeeID)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                if(platformID != null && platformID.Trim().Length > 0) p.Add("@ID_Platform", platformID);
                if (courseID != null && courseID.Trim().Length > 0) p.Add("@ID_Course", courseID);
                p.Add("@Year", year);
                p.Add("@IS_Q1", q1);
                p.Add("@IS_Q2", q2);
                p.Add("@IS_Q3", q3);
                p.Add("@IS_Q4", q4);
                p.Add("@EmployeeID", employeeID);

                List<dynamic> ret = conn.Query<dynamic>("[dbo].[sp_Report_LearningPerformance_SelectOverAll]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        }

        public List<dynamic> GetCostSpend(string platformID, string courseID, string year, string q1, string q2, string q3, string q4, string employeeID)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                if (platformID != null && platformID.Trim().Length > 0) p.Add("@ID_Platform", platformID);
                if (courseID != null && courseID.Trim().Length > 0) p.Add("@ID_Course", courseID);
                p.Add("@Year", year);
                p.Add("@IS_Q1", q1);
                p.Add("@IS_Q2", q2);
                p.Add("@IS_Q3", q3);
                p.Add("@IS_Q4", q4);
                p.Add("@EmployeeID", employeeID);

                List<dynamic> ret = conn.Query<dynamic>("[dbo].[sp_Report_LearningPerformance_SelectCostSpend]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        }

    }
}
