using ClosedXML.Excel;
using Dapper;
using LDMS.Core;
using LDMS.Daos;
using LDMS.Identity; 
using LDMS.ViewModels.Menu;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LDMS.Services
{
    public class UserService : ILDMSService
    {
        private readonly ILogger<UserService> _logger;
        protected IHttpContextAccessor HttpContextAccessor { get; private set; }
        private readonly LDAPAuthenticationService _ldAPAuthenticationService;
        private readonly LocalAuthenticationService _localAuthenticationService;
        private readonly MasterService MasterService;
        public UserService(ILogger<UserService> logger, 
            LDAPAuthenticationService ldAPAuthenticationService, 
            LocalAuthenticationService localAuthenticationService,
            MasterService masterService,
            ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor; 
            _logger = logger;
            _ldAPAuthenticationService = ldAPAuthenticationService;
            _localAuthenticationService = localAuthenticationService;
            MasterService = masterService;
        }

        public async Task<ServiceResult> GetAll()
        {
            try
            {
                return new ServiceResult(await ReadAllEmployee());
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> SearchOrganizationEmployee(int departmentId, int sectionId, string keyword)
        {
            try
            {
                var grades = (await MasterService.GetAllJobGrades()).Data as List<ViewModels.LDMS_M_JobGrade>;
                var titles = (await MasterService.GetAllJobTitles()).Data as List<ViewModels.LDMS_M_JobTitle>;
                var sections = (await MasterService.GetAllSections(departmentId)).Data as List<ViewModels.LDMS_M_Section>;
                var users = (await GetAllEmployeeByDepartmentId(departmentId)).Data as List<ViewModels.LDMS_M_User>;
                var employees = users.Select(emp => new ViewModels.EmployeeSectionView(emp)
                {
                    JobGrade = grades.FirstOrDefault(e => e.ID_JobGrade == emp.ID_JobGrade)?.JobGradeName_EN,
                    JobTitle = titles.FirstOrDefault(e => e.ID_JobTitle == emp.ID_JobTitle)?.JobTitleName_EN,
                    LDMS_M_Sections = sections
                }).ToList();
                if (sectionId > 0)
                {
                    employees = employees.Where(e => e.ID_Section == sectionId).ToList();
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    employees = employees.Where(e =>
                    (e.Name != null && e.Name.ToLower().StartsWith(keyword.ToLower()))
                    || (e.Surname != null && e.Surname.ToLower().StartsWith(keyword.ToLower()))
                    || (e.Email != null && e.Email.ToLower().StartsWith(keyword.ToLower()))
                    || (e.EmployeeID != null && e.EmployeeID.ToLower().StartsWith(keyword.ToLower()))
                    ).ToList();
                }
                int index = 1;
                employees.ForEach(item =>
                {
                    item.RowIndex = index;
                    index++;
                });
                return new ServiceResult(employees);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        //public async Task<ServiceResult> ExportOrganizationEmployee(int departmentId, int sectionId, string keyword)
        //{
        //    try
        //    {
        //        var result = (await SearchOrganizationEmployee(departmentId, sectionId, keyword));
        //        if (!result.IsOk) return result;
        //        var list = result.Data as List<ViewModels.EmployeeSectionView>;

        //        using (var workbook = new XLWorkbook())
        //        {
        //            var worksheet = workbook.Worksheets.Add("Section");
        //            var currentRow = 1;
        //            worksheet.Cell(currentRow, 1).Value = "Employee ID";
        //            worksheet.Cell(currentRow, 2).Value = "Employee Name";
        //            worksheet.Cell(currentRow, 3).Value = "Job Grade";
        //            worksheet.Cell(currentRow, 4).Value = "Job Title";
        //            worksheet.Cell(currentRow, 5).Value = "Section";
        //            foreach (var user in list)
        //            {
        //                currentRow++;
        //                worksheet.Cell(currentRow, 1).Value = user.EmployeeID;
        //                worksheet.Cell(currentRow, 2).Value = user.FullName;
        //                worksheet.Cell(currentRow, 3).Value = user.JobGrade;
        //                worksheet.Cell(currentRow, 4).Value = user.JobTitle;
        //                worksheet.Cell(currentRow, 5).Value = user.LDMS_M_Section.SectionID;
        //            }
        //            using (var stream = new MemoryStream())
        //            {
        //                workbook.SaveAs(stream);
        //                var content = stream.ToArray();
        //                string fileName = String.Format("Section.xlsx"); 
        //                Microsoft.AspNetCore.Mvc.FileContentResult fileContentResult = new Microsoft.AspNetCore.Mvc.FileContentResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        //                {
        //                    FileDownloadName = fileName
        //                };
        //                return new ServiceResult(content);
        //            }
        //        }
        //        //DataSet ds = new DataSet();
        //        //DataTable dt = new DataTable("Section");
        //        //dt.Columns.Add("Employee ID");
        //        //dt.Columns.Add("Employee Name");
        //        //dt.Columns.Add("Job Grade");
        //        //dt.Columns.Add("Job Title");
        //        //dt.Columns.Add("Section");
        //        //foreach (var item in list)
        //        //{
        //        //    var row = dt.NewRow();
        //        //    row["Employee ID"] = item.EmployeeID;
        //        //    row["Employee Name"] = item.FullName;
        //        //    row["Job Grade"] = item.JobGrade;
        //        //    row["Job Title"] = item.JobTitle;
        //        //    row["Section"] = item.LDMS_M_Section.SectionID;
        //        //    dt.Rows.Add(row);
        //        //}
        //        //ds.Tables.Add(dt);
        //        //var buffer = ds.ToExcelDynamicReport("", "");
        //        //string fileName = String.Format("Section.xlsx");
        //        //Microsoft.AspNetCore.Mvc.FileContentResult fileContentResult = new Microsoft.AspNetCore.Mvc.FileContentResult(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        //        //{
        //        //    FileDownloadName = fileName
        //        //};
        //        //return new ServiceResult(fileContentResult);
        //    }
        //    catch (Exception x)
        //    {
        //        _logger.LogError(x.Message);
        //        return new ServiceResult(x);
        //    }
        //}


        public async Task<ServiceResult> GetAll( string employeeId = null, string employeeName = null,
            List<int> departments = null,
            List<int> sectionIds = null,
            List<int> jobgrades = null,
            List<int> jobtitles = null)
        {
            try
            {
                return new ServiceResult(await ReadAllEmployee(employeeId, employeeName, departments, sectionIds, jobgrades, jobtitles));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        private async Task<List<ViewModels.LDMS_M_User>> ReadAllEmployee( string employeeId = null, string employeeName = null,
            List<int> departments = null,
            List<int> sectionIds = null,
            List<int> jobgrades = null,
            List<int> jobtitles = null)
        {
            var roles = await All<ViewModels.LDMS_M_Role>("Role");
            var jobGrades = await All<ViewModels.LDMS_M_JobGrade>("JobGrade");
            var jobTitles = await All<ViewModels.LDMS_M_JobTitle>("JobTitle");

            var parameters = new DynamicParameters(); 
            parameters.Add("@paramEmployeeId", employeeId);
            parameters.Add("@paramEmployeeName", employeeName);
            parameters.Add("@paramdepartments", departments != null ? string.Join(",", departments) : "");
            parameters.Add("@paramsections", sectionIds != null ? string.Join(",", sectionIds) : "");
            parameters.Add("@paramjobgrades", jobgrades != null ? string.Join(",", jobgrades) : "");
            parameters.Add("@paramjobtitles", jobtitles != null ? string.Join(",", jobtitles) : "");

            var items = Connection.Query<ViewModels.LDMS_M_User, ViewModels.LDMS_M_Plant, ViewModels.LDMS_M_Center, ViewModels.LDMS_M_Division, ViewModels.LDMS_M_Department, ViewModels.LDMS_M_Section, ViewModels.LDMS_M_User>
                (_schema + ".[usp_User_READ_ALL]",
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
                  param:  parameters,
                  commandType: CommandType.StoredProcedure,
                  commandTimeout:0);
                var users = items.ToList();
                users.ForEach(user =>
                {
                    user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == user.ID_Role);
                    user.LDMS_M_JobGrade = jobGrades.FirstOrDefault(e => e.ID_JobGrade == user.ID_JobGrade);
                    user.LDMS_M_JobTitle = jobTitles.FirstOrDefault(e => e.ID_JobTitle == user.ID_JobTitle);

                    if (user.LDMS_M_Role == null || user.LDMS_M_Role.ID_Role == 0)
                    {
                        if (user.LDMS_M_JobTitle.ID_JobTitle == 13)
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 3);
                        }
                        else if ((user.LDMS_M_JobTitle.ID_JobTitle == 17 || user.LDMS_M_JobTitle.ID_JobTitle == 24) &&
                            (user.LDMS_M_JobGrade.ID_JobGrade == 7 || user.LDMS_M_JobGrade.ID_JobGrade == 8))
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 4);
                        }
                    }
                });
                return users; 
        }

        public async Task<ServiceResult> ImportEmployeeSection(IFormFile fileUpload, int divisionId, int departmentId)
        {
            ServiceResult serviceResult = new ServiceResult();
            try
            {
                if (fileUpload == null)
                {
                    serviceResult.AddException(new ArgumentNullException("fileUpload"));
                }
                string extension = Path.GetExtension(fileUpload.FileName);
                if ((extension == ".xlsx") || (extension == ".xls") || (extension == ".csv"))
                {
                    var ms = fileUpload.OpenReadStream();

                    var jobGrades = (await All<ViewModels.LDMS_M_JobGrade>("JobGrade")).ToDictionary(e => e.JobGradeID.ToLower());
                    var jobTitles = (await All<ViewModels.LDMS_M_JobTitle>("JobTitle")).ToDictionary(e => e.JobTitleID.ToLower());
                    var allUsers = (await ReadAllEmployee()).ToDictionary(e => e.EmployeeID.ToLower());
                    var sections = (await All<ViewModels.LDMS_M_Section>("Section")).Where(e => e.ID_Department == departmentId).ToDictionary(e => e.SectionID.ToLower());
                    DataTable dt = ConvertStreamToDatatable(ms, "Section");
                    var list = new List<ViewModels.ImportSectionModel>();
                    foreach (DataRow row in dt.Rows)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        string[] empName = row["Employee Name".ToLower()].ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        ViewModels.ImportSectionModel model = new ViewModels.ImportSectionModel()
                        {
                            EmployeeID = row["Employee ID".ToLower()].ToString().Trim(),
                            EmployeeName = empName.Length > 0 ? empName[0] : "",
                            EmployeeSurname = empName.Length > 1 ? empName[1] : "",
                            JobGrade = row["Job Grade".ToLower()].ToString().Trim(),
                            JobTitle = row["Job Title".ToLower()].ToString().Trim(),
                            Section = row["Section".ToLower()].ToString().Trim(),
                            IsValid = true,
                            Remark = "",
                        };
                        if (!jobGrades.ContainsKey(model.JobGrade.ToLower()))
                        {
                            stringBuilder.AppendLine(string.Format("Invalid Job Grade '{0}'", model.JobGrade));
                            model.IsValid = false;
                        }
                        else
                        {
                            model.ID_JobGrade = jobGrades.GetValueOrDefault(model.JobGrade.ToLower()).ID_JobGrade;
                        }

                        if (!jobTitles.ContainsKey(model.JobTitle.ToLower()))
                        {
                            stringBuilder.AppendLine(string.Format("Invalid Job Title '{0}'", model.JobTitle));
                            model.IsValid = false;
                        }
                        else
                        {
                            model.ID_JobTitle = jobTitles.GetValueOrDefault(model.JobTitle.ToLower()).ID_JobTitle;
                        }

                        if (!string.IsNullOrEmpty(model.Section.ToLower()))
                        {
                            if (!sections.ContainsKey(model.Section))
                            {
                                stringBuilder.AppendLine(string.Format("Invalid Section '{0}'", model.Section));
                                model.IsValid = false;
                            }
                            else
                            {
                                model.ID_Section = sections.GetValueOrDefault(model.Section.ToLower()).ID_Section;
                            }
                        }
                        if (!allUsers.ContainsKey(model.EmployeeID.ToLower()))
                        {
                            stringBuilder.AppendLine(string.Format("Invalid Employee ID '{0}'", model.EmployeeID));
                            model.IsValid = false;
                        }
                        model.Remark = stringBuilder.ToString();
                        list.Add(model);
                    }
                    if (list.Any(e => !e.IsValid))
                    {
                        serviceResult.CreateResponseData(list);
                        return serviceResult;
                    }
                    else
                    {
                        using (IDbTransaction transaction = Connection.BeginTransaction())
                        {
                            foreach (var item in list)
                            {
                                var user = allUsers.GetValueOrDefault(item.EmployeeID.ToLower());
                                if (user != null)
                                {
                                    user.ID_JobGrade = item.ID_JobGrade;
                                    user.ID_JobTitle = item.ID_JobTitle;
                                    user.ID_Section = item.ID_Section;
                                    user.Name = item.EmployeeName;
                                    user.Surname = item.EmployeeSurname;
                                    user.EmployeeID = item.EmployeeID;
                                    var res = await UpdateUser(user);
                                    if (!res.IsOk)
                                    {
                                        item.Remark = string.Join(",", res.Errors.ToArray());
                                        item.IsValid = false;
                                        transaction.Rollback();
                                        serviceResult.CreateResponseData(list);
                                        return serviceResult;
                                    }
                                }
                            }
                            transaction.Commit();
                        }

                        return new ServiceResult(list);
                    }
                }
                else
                {
                    _logger.LogError("Invalid file format.");
                    serviceResult.AddException(new Exception("Invalid file format."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                serviceResult.AddException(ex);
            }
            return serviceResult;
        } 
        public async Task<ServiceResult> GetUserByEmployeeId(string employeeId)
        {
            try
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@employeeId", employeeId);
                var items = Connection.Query<ViewModels.LDMS_M_User, ViewModels.LDMS_M_Plant, ViewModels.LDMS_M_Center, ViewModels.LDMS_M_Division, ViewModels.LDMS_M_Department, ViewModels.LDMS_M_Section, ViewModels.LDMS_M_User>
                    (_schema + ".[usp_User_READ_BY_EmployeeId]",
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
                  param: parameter,
                  commandType: CommandType.StoredProcedure,
                  commandTimeout: 0);

                var user = items.FirstOrDefault();
                if (user != null)
                {
                    var roles = await All<ViewModels.LDMS_M_Role>("Role");
                    var jobGrades = await All<ViewModels.LDMS_M_JobGrade>("JobGrade");
                    var jobTitles = await All<ViewModels.LDMS_M_JobTitle>("JobTitle");
                    user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == user.ID_Role);
                    user.LDMS_M_JobGrade = jobGrades.FirstOrDefault(e => e.ID_JobGrade == user.ID_JobGrade);
                    user.LDMS_M_JobTitle = jobTitles.FirstOrDefault(e => e.ID_JobTitle == user.ID_JobTitle);
                    if (user.LDMS_M_Role == null || user.LDMS_M_Role.ID_Role == 0)
                    {
                        if (user.LDMS_M_JobTitle.ID_JobTitle == 13)
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 3);
                        }
                        else if ((user.LDMS_M_JobTitle.ID_JobTitle == 17 || user.LDMS_M_JobTitle.ID_JobTitle == 24) &&
                            (user.LDMS_M_JobGrade.ID_JobGrade == 7 || user.LDMS_M_JobGrade.ID_JobGrade == 8))
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 4);
                        }
                    }
                }
                return new ServiceResult(user);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> UpdateUserSection(List<ViewModels.LDMS_M_User> users)
        {
            try
            {
                var groupBySection = users.GroupBy(e => e.ID_Section);
                foreach (var section in groupBySection)
                {
                    var secHeads = section.Where(e => e.IsSectionHead).ToList();
                    if (secHeads.Count > 1)
                    {
                        return new ServiceResult(new Exception("One Section Allow One Header"));
                    }
                }
                var updateBy = JwtManager.Instance.GetUserId(HttpContext.Request);
                DataTable dt = new DataTable();
                dt.Columns.Add("Employee_ID", typeof(string));
                dt.Columns.Add("ID_Section", typeof(int));
                dt.Columns.Add("Is_Header", typeof(bool));
                foreach (var role in users)
                {
                    DataRow row = dt.NewRow();
                    row["Employee_ID"] = role.EmployeeID;
                    row["ID_Section"] = role.ID_Section;
                    row["Is_Header"] = role.ID_Section > 0 ? role.IsSectionHead : false;
                    dt.Rows.Add(row);
                } 
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserSectionTable", dt, DbType.Object);
                    parameters.Add("@updateBy", updateBy);
                    var items = Connection.Query<SQLError>(_schema + ".[usp_LDMS_M_User_UpdateSection]", param: parameters, commandType: CommandType.StoredProcedure);
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    return new ServiceResult(); 
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetAllEmployeeByDepartmentId(int departmentId)
        {
            try
            {
                var roles = await All<ViewModels.LDMS_M_Role>("Role");
                var jobGrades = await All<ViewModels.LDMS_M_JobGrade>("JobGrade");
                var parameters = new DynamicParameters();
                parameters.Add("@param_DepartmentId", departmentId);

                var jobTitles = await All<ViewModels.LDMS_M_JobTitle>("JobTitle");
                var items = Connection.Query<ViewModels.LDMS_M_User, ViewModels.LDMS_M_Plant, ViewModels.LDMS_M_Center, ViewModels.LDMS_M_Division, ViewModels.LDMS_M_Department, ViewModels.LDMS_M_Section, ViewModels.LDMS_M_User>
                  (_schema + ".[usp_User_READ_BY_DepartmentId]",
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
                  param: parameters, commandType: CommandType.StoredProcedure, commandTimeout: 0);
                var users = items.ToList();
                users.ForEach(user =>
                {
                    user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == user.ID_Role);
                    user.LDMS_M_JobGrade = jobGrades.FirstOrDefault(e => e.ID_JobGrade == user.ID_JobGrade);
                    user.LDMS_M_JobTitle = jobTitles.FirstOrDefault(e => e.ID_JobTitle == user.ID_JobTitle);

                    if (user.LDMS_M_Role == null || user.LDMS_M_Role.ID_Role == 0)
                    {
                        if (user.LDMS_M_JobTitle.ID_JobTitle == 13)
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 3);
                        }
                        else if ((user.LDMS_M_JobTitle.ID_JobTitle == 17 || user.LDMS_M_JobTitle.ID_JobTitle == 24) &&
                            (user.LDMS_M_JobGrade.ID_JobGrade == 7 || user.LDMS_M_JobGrade.ID_JobGrade == 8))
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 4);
                        }
                    }
                });
                return new ServiceResult(users);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAllEmployeeBySectionId(int sectionId)
        {
            try
            {
                var roles = await All<ViewModels.LDMS_M_Role>("Role");
                var jobGrades = await All<ViewModels.LDMS_M_JobGrade>("JobGrade");
                var jobTitles = await All<ViewModels.LDMS_M_JobTitle>("JobTitle");
                var parameters = new DynamicParameters();
                parameters.Add("@param_SectionId", sectionId);


                var items = Connection.Query<ViewModels.LDMS_M_User, ViewModels.LDMS_M_Plant, ViewModels.LDMS_M_Center, ViewModels.LDMS_M_Division, ViewModels.LDMS_M_Department, ViewModels.LDMS_M_Section, ViewModels.LDMS_M_User>
               (_schema + ".[usp_User_READ_BY_SectionId]",
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
                  param: parameters,
                  commandType: CommandType.StoredProcedure,
                  commandTimeout: 0);
                var users = items.ToList();
                users.ForEach(user =>
                {
                    user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == user.ID_Role);
                    user.LDMS_M_JobGrade = jobGrades.FirstOrDefault(e => e.ID_JobGrade == user.ID_JobGrade);
                    user.LDMS_M_JobTitle = jobTitles.FirstOrDefault(e => e.ID_JobTitle == user.ID_JobTitle);

                    if (user.LDMS_M_Role == null || user.LDMS_M_Role.ID_Role == 0)
                    {
                        if (user.LDMS_M_JobTitle.ID_JobTitle == 13)
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 3);
                        }
                        else if ((user.LDMS_M_JobTitle.ID_JobTitle == 17 || user.LDMS_M_JobTitle.ID_JobTitle == 24) &&
                            (user.LDMS_M_JobGrade.ID_JobGrade == 7 || user.LDMS_M_JobGrade.ID_JobGrade == 8))
                        {
                            user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 4);
                        }
                    }
                });
                return new ServiceResult(users);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> Authenticattion(string username, string password)
        {
            try
            {
                ServiceResult serviceResult = (await GetUserByEmployeeId(username));
                if (!serviceResult.IsOk)
                {
                    return serviceResult;
                }
                var user = serviceResult.Data as ViewModels.LDMS_M_User;
                if (user == null)
                {
                    CreateDataLog(DataLogType.LoginFaild, username, "EmployeeID not found");
                    throw new Exception("EmployeeID not found");
                }
                bool isAuthenPass = false;
                if (user.IsAD)
                {
                    isAuthenPass = _ldAPAuthenticationService.Authenticate(username, password);
                    if (!isAuthenPass) CreateDataLog(DataLogType.LoginFaild, username, "user signin with AD.");
                }
                else
                {
                    var passwordHash = PasswordHelper.GenerateSaltedHash(password, user.PasswordSalt);
                    isAuthenPass = _localAuthenticationService.Authenticate(username, passwordHash);
                    if (!isAuthenPass) CreateDataLog(DataLogType.LoginFaild, username, "user signin with Local.");
                }
                if (!isAuthenPass)
                {
                    throw new Exception("Unauthorized");
                }

                UserLevel userLevel = UserLevel.User;
                if (user.ID_Plant > 0 && user.ID_Center <= 0 && user.ID_Division <= 0 && user.ID_Department <= 0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.PlantManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division <= 0 && user.ID_Department <= 0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.CenterManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division > 0 && user.ID_Department <= 0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.DivisionManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division > 0 && user.ID_Department > 0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.DepartmentManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division > 0 && user.ID_Department > 0 && user.ID_Section > 0 && user.IsSectionHead)
                {
                    userLevel = UserLevel.SectionManager;
                }
                List<Claim> claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.EmployeeID),
                            new Claim(ClaimTypes.GivenName, string.IsNullOrEmpty(user.Name)?"":user.Name),
                            new Claim(ClaimTypes.Surname, string.IsNullOrEmpty(user.Surname)?"":user.Surname),
                            new Claim(ClaimTypes.Email, string.IsNullOrEmpty(user.Email)?"":user.Email),
                            new Claim(ClaimTypes.NameIdentifier, user.EmployeeID),
                            new Claim(ClaimTypes.Name, user.EmployeeID),
                            new Claim("PLANTID", user.ID_Plant.ToString()),
                            new Claim("CENTERID", user.ID_Center.ToString()),
                            new Claim("DIVISIONID", user.ID_Division.ToString()),
                            new Claim("DEPARTMENTID", user.ID_Department.ToString()),
                            new Claim("SECTIONTID",user.ID_Section.ToString()),
                            new Claim("USERLEVEL",userLevel.ToString()),
                            new Claim(ClaimTypes.Role,user.ID_Role.ToString()),
                        };
                user.Token = JwtManager.Instance.GenerateJWT(claims);
                user.RefreshToken = JwtManager.Instance.GenerateRefreshToken();
                System.Security.Principal.GenericIdentity userIdentity = new System.Security.Principal.GenericIdentity(user.EmployeeID);
                userIdentity.AddClaim(new Claim(ClaimTypes.Role, user.ID_Role.ToString()));
                userIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.EmployeeID));
                HttpContext.User = new ClaimsPrincipal(userIdentity);
                user.Password = null;
                HttpContext.Response.Set("FIRSNAME", user.Name, 120);
                HttpContext.Response.Set("LASTNAME", user.Surname, 120);
                HttpContext.Response.Set("FULLNAME", string.Format("{0} {1}", user.Name, user.Surname), 120);
                HttpContext.Response.Set("EMPLOYEEID", user.EmployeeID, 120);
                HttpContext.Response.Set("JOINDATE", user.JoinDate.HasValue ? string.Format("{0:dd-MMM-yyyy}", user.JoinDate.GetValueOrDefault()) : "", 120);
                HttpContext.Response.Set("DEPARTMENT", user.LDMS_M_Department != null ? string.Format("{0}", user.LDMS_M_Department.DepartmentID) : "", 120);
                HttpContext.Response.Set("DEPARTMENTNAME", user.LDMS_M_Department != null ? string.Format("{0}", user.LDMS_M_Department.DepartmentName_EN) : "", 120);
                HttpContext.Response.Set("PLANTID", user.LDMS_M_Plant != null ? user.LDMS_M_Plant.ID_Plant.ToString() : user.ID_Plant.ToString(), 120);
                HttpContext.Response.Set("CENTERID", user.LDMS_M_Center != null ? user.LDMS_M_Center.ID_Center.ToString() : user.ID_Center.ToString(), 120);
                HttpContext.Response.Set("DIVISIONID", user.LDMS_M_Division != null ? user.LDMS_M_Division.ID_Division.ToString() : user.ID_Division.ToString(), 120);
                HttpContext.Response.Set("DEPARTMENTID", user.LDMS_M_Department != null ? string.Format("{0}", user.LDMS_M_Department.ID_Department) : "", 120);
                HttpContext.Response.Set("SECTIONTID", user.LDMS_M_Section != null ? user.LDMS_M_Section.ID_Section.ToString() : "0", 120);
                HttpContext.Response.Set("JOBGRADEID", user.ID_JobGrade.ToString(), 120);
                HttpContext.Response.Set("JOBTITLEID", user.ID_JobTitle.ToString(), 120);
                HttpContext.Response.Set("FACEIMAGE", string.IsNullOrEmpty(user.ProfilePath) ? "~/assets/images/svg/user-icon.svg" : user.ProfilePath, 120);
                HttpContext.Response.Set("FORCECHANGEPASS", user.IsForceChangePass.ToString(), 120);
                HttpContext.Response.Set("ALLOWGPP", user.IsAllowGPP.ToString(), 120);
                HttpContext.Response.Set("ISAD", user.IsAD.ToString(), 120);
                HttpContext.Response.Set("JWToken", user.Token, 120);
                HttpContext.Response.Set("USERLEVEL", userLevel.ToString(), 120);
                CheckRedirectPage(user);
                HttpContext.Session.SetString("JWToken", user.Token);
                CreateDataLog(DataLogType.LoginSuccess, username, "user signin.");
                return new ServiceResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ServiceResult(ex);
            }
        }

        private void CheckRedirectPage(ViewModels.LDMS_M_User user)
        {
            if (user.IsForceChangePass || user.IsFirstLogin)
            {
                HttpContext.Response.Set("REDIRECTPAGE", "/Account/ForceChange", 120);
            }
            //else if (!user.IsAllowGPP&& !user.IsAD)
            //{
            //    HttpContext.Response.Set("REDIRECTPAGE", "/Account/Privacy", 120);
            //}
            else
            {
                HttpContext.Response.Set("REDIRECTPAGE", "/Home/Index", 120);
            }
        }

        public async Task<List<NavigationMenu>> GetMenuItemsAsync()
        {
            try
            {
                var token = JwtManager.Instance.GetToken(HttpContext.Request);
                if (!JwtManager.Instance.IsTokenValid(token))
                {
                    throw new Exception("Unauthorized");
                }
                var cliams = JwtManager.Instance.GetClaims(token).ToList();
                Claim claim = cliams.FirstOrDefault(o => o.Type.ToUpper() == ClaimTypes.Role.ToUpper());
                if (claim == null) throw new Exception("Unauthorized");
                int.TryParse(claim.Value, out int rolId);
                return BuildUserMenu(rolId).AsList();
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                throw new Exception("Unauthorized");
            }
        }

        private IEnumerable<NavigationMenu> BuildUserMenu(int roleId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@paramRoleId", roleId);

            var items = Connection.Query<ViewModels.LDMS_M_SubModule, ViewModels.LDMS_M_Module, ViewModels.LDMS_M_RolePermission, ViewModels.LDMS_M_Role, ViewModels.LDMS_M_SubModule>
                (_schema + ".[usp_RoleMenu_READ_By_Role]",
                  map: (submodule, module, rolepermission, role) =>
                  {
                      submodule.LDMS_M_Module = module;
                      submodule.LDMS_M_RolePermission = rolepermission;
                      if (rolepermission != null)
                      {
                          submodule.LDMS_M_RolePermission.LDMS_M_Role = role;
                      }
                      return submodule;
                  },
                  splitOn: "ID_Module,RolePermissionId,RoleId",
                  param: parameters,
                  commandType: CommandType.StoredProcedure,
                  commandTimeout: 0);

            var groupMenu = items.OrderBy(e => e.LDMS_M_Module.Module_Sequence).GroupBy(e => e.LDMS_M_Module.ID_Module);
                bool isFirst = true;
                foreach (var item in groupMenu)
                {
                    var module = items.Where(e => e.LDMS_M_Module.ID_Module == item.Key).Select(e => e.LDMS_M_Module).FirstOrDefault();
                    var menu = new NavigationMenu()
                    {
                        ActionName = "",
                        CadWrite = true,
                        CanRead = true,
                        ControllerName = "",
                        MenuIco = "",
                        MenuID = module.ModuleID,
                        MenuName = module.ModuleName_EN,
                        MenuUrl = module.Module_URL,
                        FirstMenu = isFirst,
                        SubMenus = item.OrderBy(e => e.Sequence).Select(e => new SubNavigationMenu()
                        {
                            MenuUrl = e.SubModule_URL,
                            ActionName = e.SubModule_URL.Split('/').Length > 1 ? e.SubModule_URL.Split('/')[1] : "",
                            CadWrite = true,
                            CanRead = true,
                            ControllerName = e.SubModule_URL.Split('/').Length > 0 ? e.SubModule_URL.Split('/')[0] : "",
                            MenuIco = "",
                            MenuID = e.SubModuleID,
                            MenuName = e.SubModuleName_EN
                        }).ToList()
                    };
                    isFirst = false;
                    yield return menu;
                } 
        }

        public async Task<ServiceResult> DeleteUser(string employeeId)
        {
            try
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@paramEmployeeId", employeeId);
                parameter.Add("@paramUpdateBy", JwtManager.Instance.GetUserId(HttpContext.Request));
                var items = Connection.Query<SQLError>(_schema + ".[usp_User_Delete]", param: parameter, commandType: CommandType.StoredProcedure, commandTimeout: 0);
                if (items != null && items.Any())
                {
                    return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                }
                CreateDataLog(DataLogType.DeleteAccount, employeeId, string.Format("Delete {0}", employeeId));
                return new ServiceResult();
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> ResetPassword(string employeeId)
        {
            try
            {
                var passsalt = PasswordHelper.CreateSalt();
                var newHaspass = PasswordHelper.GenerateSaltedHash(employeeId, passsalt);
               
                    DynamicParameters parameter = new DynamicParameters();
                    parameter.Add("@EmployeeId", employeeId);
                    parameter.Add("@Password", newHaspass);
                    parameter.Add("@PasswordSalt", passsalt);
                    parameter.Add("@UpdateBy", JwtManager.Instance.GetUserId(HttpContext.Request));
                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_ResetPassword]", param: parameter, commandType: CommandType.StoredProcedure, commandTimeout: 0);
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    CreateDataLog(DataLogType.ResetPassword, employeeId, "Reset password."); 
                return new ServiceResult(); 
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            } 
        }

        public async Task<ServiceResult> CreateUser(ViewModels.LDMS_M_User user)
        {
            try
            { 
                    var passsalt = PasswordHelper.CreateSalt();
                    DynamicParameters parameter = new DynamicParameters();
                    parameter.Add("@EmployeeId", user.EmployeeID);
                    parameter.Add("@EmployeeName", user.Name);
                    parameter.Add("@EmployeeSurName", user.Surname);
                    parameter.Add("@JobGradeId", user.ID_JobGrade);
                    parameter.Add("@JobTitleId", user.ID_JobTitle);
                    parameter.Add("@CenterId", user.ID_Center);
                    parameter.Add("@DivisionId", user.ID_Division);
                    parameter.Add("@DepartmentId", user.ID_Department);
                    parameter.Add("@SectionId", user.ID_Section);
                    parameter.Add("@RoleId", user.ID_Role > 0 ? user.ID_Role : 1);
                    parameter.Add("@IsInstructer", user.IsInstructor);
                    parameter.Add("@IsSectionHead", user.IsSectionHead);
                    parameter.Add("@Nationality", user.Nationality);
                    parameter.Add("@Gender", user.Gender);
                    parameter.Add("@Password", PasswordHelper.GenerateSaltedHash(user.EmployeeID, passsalt));
                    parameter.Add("@PasswordSalt", passsalt);
                    parameter.Add("@Remark", user.Remark, DbType.AnsiString);
                    parameter.Add("@PhoneNumber", user.PhoneNumber);
                    parameter.Add("@Email", user.Email, DbType.AnsiString);
                    parameter.Add("@CreateBy", JwtManager.Instance.GetUserId(HttpContext.Request));
                    parameter.Add("@IDCardNumber", user.IDCardNumber);
                    parameter.Add("@JoinDate", user.JoinDate);
                    parameter.Add("@OutDate", user.OutDate);
                    parameter.Add("@DateOfBirth", user.DateOfBirth);
                    parameter.Add("@ProfilePath", user.ProfilePath);
                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_Create]", param: parameter, commandType: CommandType.StoredProcedure, commandTimeout: 0);
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    return await GetUserByEmployeeId(user.EmployeeID); 
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> UpdateUser(ViewModels.LDMS_M_User user)
        {
            try
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@ID_Employee", user.ID_User);
                parameter.Add("@EmployeeId", user.EmployeeID);
                parameter.Add("@EmployeeName", user.Name);
                parameter.Add("@EmployeeSurName", user.Surname);
                parameter.Add("@JobGradeId", user.ID_JobGrade);
                parameter.Add("@JobTitleId", user.ID_JobTitle);
                parameter.Add("@CenterId", user.ID_Center);
                parameter.Add("@DivisionId", user.ID_Division);
                parameter.Add("@DepartmentId", user.ID_Department);
                parameter.Add("@SectionId", user.ID_Section);
                parameter.Add("@RoleId", user.ID_Role > 0 ? user.ID_Role : 1);
                parameter.Add("@IsInstructer", user.IsInstructor);
                parameter.Add("@IsSectionHead", user.IsSectionHead);
                parameter.Add("@Nationality", user.Nationality);
                parameter.Add("@Gender", user.Gender);
                parameter.Add("@Remark", user.Remark);
                parameter.Add("@PhoneNumber", user.PhoneNumber);
                parameter.Add("@Email", user.Email);
                parameter.Add("@UpdateBy", JwtManager.Instance.GetUserId(HttpContext.Request));
                parameter.Add("@IDCardNumber", user.IDCardNumber);
                parameter.Add("@JoinDate", user.JoinDate);
                parameter.Add("@OutDate", user.OutDate);
                parameter.Add("@DateOfBirth", user.DateOfBirth);
                parameter.Add("@ProfilePath", user.ProfilePath);
                var items = Connection.Query<SQLError>(_schema + ".[usp_User_Update]", param: parameter, commandType: CommandType.StoredProcedure, commandTimeout: 0);
                if (items != null && items.Any())
                {
                    return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                }
                CreateDataLog(DataLogType.UpdateAccount, user.EmployeeID, string.Format("Update {0}", user.EmployeeID));
                return await GetUserByEmployeeId(user.EmployeeID);
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> ChangePassword(string employeeId, string currentPassword, string newpassword)
        {
            try
            {
                var emp = await GetUserByEmployeeId(employeeId);
                var passsalt = PasswordHelper.CreateSalt();
                var newHaspass = PasswordHelper.GenerateSaltedHash(newpassword, passsalt);
                var oldPasshash = PasswordHelper.GenerateSaltedHash(currentPassword, (emp.Data as ViewModels.LDMS_M_User).PasswordSalt);
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@EmployeeId", employeeId);
                parameter.Add("@OldPassword", newHaspass);
                parameter.Add("@Password", newHaspass);
                parameter.Add("@PasswordSalt", passsalt);
                parameter.Add("@UpdateBy", JwtManager.Instance.GetUserId(HttpContext.Request));
                var items = Connection.Query<SQLError>(_schema + ".[usp_User_ChangePassword]", param: parameter, commandType: CommandType.StoredProcedure, commandTimeout: 0);
                if (items != null && items.Any())
                {
                    return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                }
                var user = emp.Data as ViewModels.LDMS_M_User;

                HttpContext.Response.Set("FORCECHANGEPASS", user.IsForceChangePass.ToString(), 120);
                HttpContext.Response.Set("ALLOWGPP", user.IsAllowGPP.ToString(), 120);
                CheckRedirectPage(emp.Data as ViewModels.LDMS_M_User);
                CreateDataLog(DataLogType.ChangePassword, employeeId, "Change Password.");
                return emp;
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> AllowGPP(string employeeId, bool isAllow)
        {
            try
            {
                DynamicParameters parameter = new DynamicParameters();
                parameter.Add("@EmployeeId", employeeId, DbType.String);
                parameter.Add("@IsAllow", isAllow ? 1 : 0, DbType.Int32);
                parameter.Add("@UpdateBy", JwtManager.Instance.GetUserId(HttpContext.Request), DbType.String);

                var items = Connection.Query<SQLError>(_schema + ".[usp_User_AllowGPP]", param: parameter, commandType: CommandType.StoredProcedure);
                if (items != null && items.Any())
                {
                    return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                }
                var emp = await GetUserByEmployeeId(employeeId);
                var user = emp.Data as ViewModels.LDMS_M_User;
                HttpContext.Response.Set("FORCECHANGEPASS", user.IsForceChangePass.ToString(), 120);
                HttpContext.Response.Set("ALLOWGPP", user.IsAllowGPP.ToString(), 120);

                CheckRedirectPage(emp.Data as ViewModels.LDMS_M_User);
                CreateDataLog(DataLogType.AcceptGPP, employeeId, string.Format("Allow {0}", isAllow));
                return emp;
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
         
    }
}