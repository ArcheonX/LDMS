using Dapper;
using LDMS.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using LDMS.Daos;
using LDMS.ViewModels;

namespace LDMS.Services
{
    public class ScheduleService : ILDMSService
    {
        private readonly ILogger<ScheduleService> _logger;
        public ScheduleService(
           ILogger<ScheduleService> logger,
           ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _logger = logger;
        }

        public List<ViewModels.LDMS_T_ScheduleClass> GetScheduleClass()
        {
            List<ViewModels.LDMS_T_ScheduleClass> schedule = new List<ViewModels.LDMS_T_ScheduleClass>();
            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                p.Add("@ID_Employee", CurrentUserId);

                var grid = conn.QueryMultiple("[dbo].[sp_T_Class_Schedule]", p, commandType: CommandType.StoredProcedure);

                schedule = grid.Read<ViewModels.LDMS_T_ScheduleClass>().ToList();

                return schedule;
            }
        }

        public List<ViewModels.LDMS_T_ScheduleClass> GetTeamScheduleClass( string ID_Department)
        {
            List<ViewModels.LDMS_T_ScheduleClass> schedule = new List<ViewModels.LDMS_T_ScheduleClass>();
            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                p.Add("@ID_Department", ID_Department);

                var grid = conn.QueryMultiple("[dbo].[sp_T_Class_TeamSchedule]", p, commandType: CommandType.StoredProcedure);

                schedule = grid.Read<ViewModels.LDMS_T_ScheduleClass>().ToList();

                return schedule;
            }
        }

        public List<ViewModels.LDMS_T_ScheduleClass_Detail> GetScheduleClassDetail(string ID_Course)
        {
            List<ViewModels.LDMS_T_ScheduleClass_Detail> schedule = new List<ViewModels.LDMS_T_ScheduleClass_Detail>();
            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                p.Add("@ID_Course", ID_Course);

                var grid = conn.QueryMultiple("[dbo].[sp_T_Class_Select_ByID_Course]", p, commandType: CommandType.StoredProcedure);

                schedule = grid.Read<ViewModels.LDMS_T_ScheduleClass_Detail>().ToList();

                return schedule;
            }
        }

        public List<ViewModels.LDMS_T_ScheduleClass_Reg> GetClassReg(string ID_Course)
        {
            List<ViewModels.LDMS_T_ScheduleClass_Reg> schedule = new List<ViewModels.LDMS_T_ScheduleClass_Reg>();
            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                if(ID_Course != "")
                    p.Add("@ID_Course", ID_Course);

                var grid = conn.QueryMultiple("[dbo].[sp_T_ClassReg_Select_ByID_Course]", p, commandType: CommandType.StoredProcedure);

                schedule = grid.Read<ViewModels.LDMS_T_ScheduleClass_Reg>().ToList();

                return schedule;
            }
        }

        public ViewModels.LDMS_T_ScheduleCourseInfo GetCourseInfo(string ID_Course)
        {
            using (IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@ID_Course", ID_Course);

                    ViewModels.LDMS_T_ScheduleCourseInfo item = conn.Query<ViewModels.LDMS_T_ScheduleCourseInfo>("[dbo].[sp_T_CourseInfo_ByID_Course]", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    return item;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }


        public List<ViewModels.LDMS_T_ScheduleClass_Reg> GetTeamClassReg(string ID_Department, string keyword)
        {
            List<ViewModels.LDMS_T_ScheduleClass_Reg> schedule = new List<ViewModels.LDMS_T_ScheduleClass_Reg>();
            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                if (ID_Department != "")
                    p.Add("@ID_Department", ID_Department); //ID_Department
                if (keyword != "")
                    p.Add("@keyword", keyword); //keyword

                var grid = conn.QueryMultiple("[dbo].[sp_T_ClassReg_Select_ByID_Department_Search]", p, commandType: CommandType.StoredProcedure);

                schedule = grid.Read<ViewModels.LDMS_T_ScheduleClass_Reg>().ToList();

                return schedule;
            }
        }


        public void UpdateStatusReject(string idClass, string idCourse, string employeeID, string registerStatus, string remarkManager, string updateBy)
        {
            List<LDMS_M_CodeLookUp> result = new List<LDMS_M_CodeLookUp>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Class", idClass);
                p.Add("@ID_Course", idCourse);
                p.Add("@EmployeeID", employeeID);
                p.Add("@RegisterStatus", registerStatus);
                p.Add("@RemarkManager", remarkManager);
                p.Add("@UpdateBy", updateBy);

                conn.Execute("[dbo].[sp_T_ClassRegistration_UpdateStatus_ManagerReject]", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }


        public List<ViewModels.LDMS_T_ScheduleClass_EmployeeInfo> GetClassRegEmployee(string ID_Department, string ID_Course, string month)
        {
            List<ViewModels.LDMS_T_ScheduleClass_EmployeeInfo> schedule = new List<ViewModels.LDMS_T_ScheduleClass_EmployeeInfo>();
            using (IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                if (ID_Department != "")
                    p.Add("@ID_Department", ID_Department);

                if (ID_Course != "")
                    p.Add("@ID_Course", ID_Course);

                if (month != "")
                    p.Add("@Month", month);


                var grid = conn.QueryMultiple("[dbo].[sp_T_ClassReg_Select_ByID_Department]", p, commandType: CommandType.StoredProcedure);

                schedule = grid.Read<ViewModels.LDMS_T_ScheduleClass_EmployeeInfo>().ToList();

                return schedule;
            }
        }
    }
}
