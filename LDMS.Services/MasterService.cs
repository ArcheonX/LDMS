using Dapper;
using LDMS.Daos;
using LDMS.ViewModels.ReportModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LDMS.Services
{
    public class MasterService : ILDMSService
    {
        private readonly ILogger<MasterService> _logger;
        public MasterService(ILogger<MasterService> logger,
            ILDMSConnection iLDMSConnection,
            IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<ServiceResult> GetAllPlants()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Plant>("Plant"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetAllPlant()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Plant>("Plant"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllCenters()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Center>("Center"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetIMasterReport(MasterReportFilterModel model)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
                {
                    DynamicParameters parameter = new DynamicParameters();
                    parameter.Add("@paramTrainingDateFrm", model.TrainingDateFrm, System.Data.DbType.DateTime);
                    parameter.Add("@paramTrainingDateTo", model.TrainingDateTo, System.Data.DbType.DateTime);
                    parameter.Add("@paramDepartmentId", model.DepartmentId, System.Data.DbType.Int32);
                    parameter.Add("@paramJobGradeId", model.JobGradeId, System.Data.DbType.Int32);
                    parameter.Add("@paramStatus", model.ActiveStatus, System.Data.DbType.Int32);
                    parameter.Add("@paramTrainingStatus ", model.TrainingStatus, System.Data.DbType.Int32);
                    string commad = "";

                    System.Data.DataSet dataSet = new System.Data.DataSet();
                    if (model.MasterReportType == MasterReportType.Instructor)
                    {
                        parameter.Add("@paramInstructorId ", model.InstructorId, System.Data.DbType.String);
                        commad = _schema + ".[usp_Instructor_Master_Report]";
                    }
                    else if (model.MasterReportType == MasterReportType.Course)
                    {
                        parameter.Add("@paramCourseId ", model.CourseId, System.Data.DbType.Int32);
                        commad = _schema + ".[usp_Course_Master_Report]";
                    }
                    else if (model.MasterReportType == MasterReportType.Platform)
                    {
                        parameter.Add("@paramPlaformId ", model.PlatformId, System.Data.DbType.Int32);
                        commad = _schema + ".[usp_Plaform_Master_Report]";
                    }

                    using (var reader = Connection.ExecuteReader(
                          sql: commad,
                          param: parameter,
                          commandTimeout: 0,
                          commandType: System.Data.CommandType.StoredProcedure))
                    {
                        var dataTable = new System.Data.DataTable();
                        dataTable.Load(reader);
                        dataSet.Tables.Add(dataTable);
                    }
                    var buffer = dataSet.ToExcelDynamicReport("", "");
                    string fileName = String.Format("MasterReport.xls");
                    Microsoft.AspNetCore.Mvc.FileContentResult fileContentResult = new Microsoft.AspNetCore.Mvc.FileContentResult(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = fileName
                    };
                    return new ServiceResult(fileContentResult);
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetAllDivisions()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Division>("Division"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllDepartments()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Department>("Department"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllSections()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Section>("Section"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetAllCenters(int plantId)
        {
            try
            {
                var items = ((await GetAllCenters()).Data as List<ViewModels.LDMS_M_Center>).Where(e => e.ID_Plant == plantId).ToList();
                foreach (var item in items)
                {
                    item.RowIndex = items.IndexOf(item) + 1;
                }
                return new ServiceResult(items);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllDivisions(int centerId)
        {
            try
            {
                var items = ((await GetAllDivisions()).Data as List<ViewModels.LDMS_M_Division>).Where(e => e.ID_Center == centerId).ToList();
                foreach (var item in items)
                {
                    item.RowIndex = items.IndexOf(item) + 1;
                }
                return new ServiceResult(items);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllDepartments(int divisionId)
        {
            try
            {
                var items = ((await GetAllDepartments()).Data as List<ViewModels.LDMS_M_Department>).Where(e => e.ID_Division == divisionId).ToList();
                foreach (var item in items)
                {
                    item.RowIndex = items.IndexOf(item) + 1;
                }
                return new ServiceResult(items);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllSections(int departmentId)
        {
            try
            {
                var items = ((await GetAllSections()).Data as List<ViewModels.LDMS_M_Section>).Where(e => e.ID_Department == departmentId).ToList();
                foreach (var item in items)
                {
                    item.RowIndex = items.IndexOf(item) + 1;
                }
                return new ServiceResult(items);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetDepartment(int departmentId)
        {
            try
            {
                var items = (await GetAllDepartments()).Data as List<ViewModels.LDMS_M_Department>;
                return new ServiceResult(items.Where(e => e.ID_Department == departmentId).FirstOrDefault());
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetDivision(int divisionId)
        {
            try
            {
                var items = (await GetAllDivisions()).Data as List<ViewModels.LDMS_M_Division>;
                return new ServiceResult(items.Where(e => e.ID_Division == divisionId).FirstOrDefault());
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetSection(int sectionId)
        {
            try
            {
                var items = (await GetAllSections()).Data as List<ViewModels.LDMS_M_Section>;
                return new ServiceResult(items.Where(e => e.ID_Section == sectionId).FirstOrDefault());
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllJobTitles()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_JobTitle>("JobTitle"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllJobGrades()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_JobGrade>("JobGrade"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllRoles()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Role>("Role"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllCourses()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Course>("Course"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllPlatforms()
        {
            try
            {
                return new ServiceResult(await All<ViewModels.LDMS_M_Platform>("Platform"));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> DeleteSection(int sectionId)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<SQLError>(_schema + ".[usp_Section_Delete] @param_sectionId,@paramUpdateBy",
                        new
                        {
                            @param_sectionId = sectionId,
                            @paramUpdateBy = CurrentUserId
                        });
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    return new ServiceResult();
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> UpdateSection(ViewModels.LDMS_M_Section section)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<SQLError>(_schema + ".[usp_Section_Update] @param_sectionId,@param_derpartmentId,@para_sectionCode,@param_sectionNameEN,@param_sectionNameTH,@param_sectionDesc,@paramUpdateBy",
                        new
                        {
                            @param_sectionId = section.ID_Section,
                            @param_derpartmentId = section.ID_Department,
                            @para_sectionCode = section.SectionID,
                            @param_sectionNameEN = section.SectionName_EN,
                            @param_sectionNameTH = section.SectionName_TH,
                            @param_sectionDesc = section.SectionDescription,
                            @paramUpdateBy = CurrentUserId
                        });
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    return await GetSection(section.ID_Section);
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> CreateSection(ViewModels.LDMS_M_Section section)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<SQLError>(_schema + ".[usp_Section_Create] @param_derpartmentId,@para_sectionCode,@param_sectionNameEN,@param_sectionNameTH,@param_sectionDesc,@paramCreateBy",
                        new
                        {
                            @param_derpartmentId = section.ID_Department,
                            @para_sectionCode = section.SectionID,
                            @param_sectionNameEN = section.SectionName_EN,
                            @param_sectionNameTH = section.SectionName_TH,
                            @param_sectionDesc = section.SectionDescription,
                            @paramCreateBy = CurrentUserId
                        });
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    return new ServiceResult();
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public List<ViewModels.LDMS_M_CodeLookUp> GetCodeLookups(string tableName, string fieldName)
        {
            List<ViewModels.LDMS_M_CodeLookUp> result = new List<ViewModels.LDMS_M_CodeLookUp>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@TableName", tableName);
                p.Add("@FieldName", fieldName);

                result = conn.Query<ViewModels.LDMS_M_CodeLookUp>("[dbo].[sp_M_CodeLookUp_Select]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return result;
            }
        }

        public List<ViewModels.LDMS_M_Provinces> GetProvinces()
        {
            List<ViewModels.LDMS_M_Provinces> result = new List<ViewModels.LDMS_M_Provinces>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();

                result = conn.Query<ViewModels.LDMS_M_Provinces>("[dbo].[sp_M_Provinces_Select]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return result;
            }
        }

        public List<ViewModels.LDMS_M_Amphurs> GetAmphurs(string provinceID)
        {
            List<ViewModels.LDMS_M_Amphurs> result = new List<ViewModels.LDMS_M_Amphurs>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@PROVINCE_ID", provinceID);

                result = conn.Query<ViewModels.LDMS_M_Amphurs>("[dbo].[sp_M_Amphur_Select]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return result;
            }
        }

        public List<ViewModels.LDMS_M_Districts> GetDistricts(string provinceID, string amphurID)
        {
            List<ViewModels.LDMS_M_Districts> result = new List<ViewModels.LDMS_M_Districts>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@PROVINCE_ID", provinceID);
                p.Add("@AMPHUR_ID", amphurID);

                result = conn.Query<ViewModels.LDMS_M_Districts>("[dbo].[sp_M_District_Select]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return result;
            }
        }

        public string GetRedirectMenuBySubmenuId(string submeuId)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@submenuId", submeuId);

                var result = conn.Query<ViewModels.LDMS_M_SubModule>("[dbo].[usp_MenuRedirectByMenuId]", p, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();
                if (result != null && !string.IsNullOrEmpty(result.SubModule_URL)) return result.SubModule_URL;
                return string.Empty;
            }
        }
    }
}
