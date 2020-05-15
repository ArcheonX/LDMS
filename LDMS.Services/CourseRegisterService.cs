using LDMS.Core;
using LDMS.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using LDMS.Daos;
using System.Threading.Tasks;

namespace LDMS.Services
{
    public class CourseRegisterService : ILDMSService
    {
        private readonly AppSettings _appSettings;
        public CourseRegisterService(IOptions<AppSettings> appSettings,
            ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _appSettings = appSettings.Value;
        }

        public List<dynamic> GetInfo(string classID)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID", classID);

                List<dynamic> ret = conn.Query<dynamic>("[dbo].[sp_T_Class_SelectInfo]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        }

        public List<dynamic> Get(string courseID, string classID, string empID, string empName, string departmentName)
        {

            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                if (courseID.Trim().Length > 0) p.Add("@ID_Course", courseID);
                if (classID.Trim().Length > 0) p.Add("@ID_Class", classID);
                if (empID != null && empID.Trim().Length > 0) p.Add("@EmployeeID", empID);
                if (empName != null && empName.Trim().Length > 0) p.Add("@EmployeeName", empName);
                if (departmentName != null && departmentName.Trim().Length > 0) p.Add("@DepartmentName", departmentName);

                List<dynamic> ret = conn.Query<dynamic>("[dbo].[sp_T_ClassRegisteration_Select]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        }

        public void UpdateStatus(string idClass, string idCourse, string employeeID, string registerStatus, string updateBy)
        {
            List<LDMS_M_CodeLookUp> result = new List<LDMS_M_CodeLookUp>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Class", idClass);
                p.Add("@ID_Course", idCourse);
                p.Add("@EmployeeID", employeeID);
                p.Add("@RegisterStatus", registerStatus);
                p.Add("@UpdateBy", updateBy);

                conn.Execute("[dbo].[sp_T_ClassRegistration_UpdateStatus]", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public void UpdateClassResult(string idClass, string idCourse, string employeeID, string classState, string learningResult,
            string attendCountNum, string isScanRFID, string preTestScore, string postTestScore, string skillScore, string coachingStaus,
            string certificateStatus, string createBy)
        {
            List<LDMS_M_CodeLookUp> result = new List<LDMS_M_CodeLookUp>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Class", idClass);
                p.Add("@ID_Course", idCourse);
                p.Add("@EmployeeID", employeeID);
                if (classState != null) p.Add("@ClassState", classState);
                if (learningResult != null) p.Add("@LearningResult", learningResult);
                if (attendCountNum != null) p.Add("@AttendCountNum", attendCountNum);
                if (attendCountNum != null) p.Add("@AttendCountNum", attendCountNum);

                if (isScanRFID != null) p.Add("@IsScanRFID", isScanRFID);
                if (preTestScore != null) p.Add("@PreTestScore", preTestScore);
                if (postTestScore != null) p.Add("@PostTestScore", postTestScore);
                if (skillScore != null) p.Add("@SkillScore", skillScore);
                if (coachingStaus != null) p.Add("@CoachingStatus", coachingStaus);
                if (certificateStatus != null) p.Add("@CertificateStatus", certificateStatus);
                p.Add("@CreateBy", createBy);

                conn.Execute("[dbo].[sp_T_ClassAttendAndResult_Insert]", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }


        public void AddCertificate(string idClass, string idCourse, string employeeID, string certificatePath, string description, string expireDate, string createBy)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Class", idClass);
                p.Add("@ID_Course", idCourse);
                p.Add("@EmployeeID", employeeID);
                p.Add("@Certificate_Path", certificatePath);
                p.Add("@Description", description);
                if(expireDate.Trim().Length > 0) p.Add("@ExpireDate", expireDate);
                p.Add("@CreateBy", createBy);

                conn.Execute("[dbo].[sp_T_ClassCertificate_InsertU]", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public void AddCoaching(string idClass, string idCourse, string employeeID, string coachingStatus, string createBy)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Class", idClass);
                p.Add("@ID_Course", idCourse);
                p.Add("@EmployeeID", employeeID);
                p.Add("@CoachingStatus", coachingStatus);
                p.Add("@CreateBy", createBy);

                conn.Execute("[dbo].[sp_T_Coaching_Insert]", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }


        public void UpdateClass(string classID, string classStatus, string updateBy)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID", classID);
                p.Add("@ClassStatus", classStatus);
                p.Add("@UpdateBy", updateBy);

                conn.Execute("[dbo].[sp_T_Class_UpdateStatus]", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }


        public List<dynamic> GetEmployeeForClose(string courseID, string classID)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                if (courseID.Trim().Length > 0) p.Add("@ID_Course", courseID);
                if (classID.Trim().Length > 0) p.Add("@ID_Class", classID);

                List<dynamic> ret = conn.Query<dynamic>("[dbo].[sp_T_ClassRegisteration_SelectEmployeeForClose]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        }


        public async Task<ServiceResult> GetEmployee(
            string employeeId = null,
            string employeeName = null,
            List<int> departments = null,
            List<int> sectionIds = null,
            List<int> jobgrades = null,
            List<int> jobtitles = null,
            string id_course = null,
            string id_class = null)
        {
            try
            {
                var roles = await All<LDMS_M_Role>("Role");
                var jobGrades = await All<LDMS_M_JobGrade>("JobGrade");
                var jobTitles = await All<LDMS_M_JobTitle>("JobTitle");
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<LDMS_M_User, LDMS_M_Plant, LDMS_M_Center, LDMS_M_Division, LDMS_M_Department, LDMS_M_Section, LDMS_M_User>
                    (_schema + ".[usp_User_READ_ALL_C] @paramEmployeeId,@paramEmployeeName,@paramdepartments,@paramsections,@paramjobgrades,@paramjobtitles,@ID_Course,@ID_Class",
                      map: (user, plant, center, division, depart, section) =>
                      {
                          if (plant != null)
                          {
                              user.LDMS_M_Plant = plant;
                              user.ID_Plant = plant.ID_Plant;
                          }
                          if (center != null)
                          {
                              user.LDMS_M_Center = center;
                              user.ID_Center = center.ID_Center;
                          }
                          if (division != null)
                          {
                              user.LDMS_M_Division = division;
                              user.ID_Division = division.ID_Division;
                          }
                          if (depart != null)
                          {
                              user.LDMS_M_Department = depart;
                              user.ID_Department = depart.ID_Department;
                          }
                          if (section != null)
                          {
                              user.LDMS_M_Section = section;
                              user.ID_Section = section.ID_Section;
                          }
                          return user;
                      },
                      splitOn: "ID_Plant,ID_Center,ID_Division,ID_Department,ID_Section",
                      param: new
                      {
                          @paramEmployeeId = employeeId,
                          @paramEmployeeName = employeeName,
                          @paramdepartments = departments != null ? string.Join(",", departments) : "",
                          @paramsections = sectionIds != null ? string.Join(",", sectionIds) : "",
                          @paramjobgrades = jobgrades != null ? string.Join(",", jobgrades) : "",
                          @paramjobtitles = jobtitles != null ? string.Join(",", jobtitles) : "",
                          @ID_Class = id_class,
                          @ID_Course = id_course,
                      });
                    var users = items.ToList();
                    users.ForEach(user =>
                    {
                        user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == user.ID_Role);
                        user.LDMS_M_JobGrade = jobGrades.FirstOrDefault(e => e.ID_JobGrade == user.ID_JobGrade);
                        user.LDMS_M_JobTitle = jobTitles.FirstOrDefault(e => e.ID_JobTitle == user.ID_JobTitle);
                    });
                    return new ServiceResult(users);
                }
            }
            catch (Exception x)
            {
                //_logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public string AddCourseRegister(string idClass, string idCourse, string employeeID, string status, string updateBy)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@paramID_Class", idClass);
                p.Add("@paramID_Course", idCourse);
                p.Add("@paramID_Employee", employeeID);
                p.Add("@paramRegisterStatus", status);
                p.Add("@paramRemarkManager", "");
                p.Add("@paramRegisterDate", DateTime.Now.ToString("yyyy-MM-dd"));
                p.Add("@paramUpdateBy", updateBy);
                p.Add("@paramRemarkAdmin", "");

                return conn.ExecuteScalar("[dbo].[sp_T_ClassRegistration_Insert]", p, commandType: System.Data.CommandType.StoredProcedure).ToString();
            }
        }

    }
}
