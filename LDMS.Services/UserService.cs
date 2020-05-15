using Dapper;
using LDMS.Core;
using LDMS.Daos;
using LDMS.Identity;
using LDMS.ViewModels;
using LDMS.ViewModels.Menu;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LDMS.Services
{
    public class UserService : ILDMSService
    {
        private readonly ILogger<UserService> _logger;
        protected IHttpContextAccessor HttpContextAccessor { get; private set; }
        private readonly LDAPAuthenticationService _ldAPAuthenticationService;
        private readonly LocalAuthenticationService _localAuthenticationService;
        public UserService(ILogger<UserService> logger,
            LDAPAuthenticationService ldAPAuthenticationService,
            LocalAuthenticationService localAuthenticationService,
            ILDMSConnection iLDMSConnection,
            IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;

            _logger = logger;
            _ldAPAuthenticationService = ldAPAuthenticationService;
            _localAuthenticationService = localAuthenticationService;
        }

        public async Task<ServiceResult> GetAll()
        {
            try
            {
                return new ServiceResult(await GetAll(null, null, null));
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
        public async Task<ServiceResult> GetAll(
            string employeeId = null, 
            string employeeName = null, 
            List<int> departments = null, 
            List<int> sectionIds = null, 
            List<int> jobgrades = null, 
            List<int> jobtitles = null)
        {
            try
            {
                var roles = await All<LDMS_M_Role>("Role");
                var jobGrades = await All<LDMS_M_JobGrade>("JobGrade");
                var jobTitles = await All<LDMS_M_JobTitle>("JobTitle");
                using (System.Data.IDbConnection conn = Connection)
                { 
                    var items = Connection.Query<LDMS_M_User,LDMS_M_Plant, LDMS_M_Center, LDMS_M_Division, LDMS_M_Department, LDMS_M_Section, LDMS_M_User>
                    (_schema + ".[usp_User_READ_ALL] @paramEmployeeId,@paramEmployeeName,@paramdepartments,@paramsections,@paramjobgrades,@paramjobtitles",
                      map: (user, plant,center,division, depart,section) =>
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
                      });
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
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> GetUserByEmployeeId(string employeeId)
        {
            try
            {
                using (IDbConnection conn = Connection)
                {
                    var items = Connection.Query<LDMS_M_User, LDMS_M_Plant, LDMS_M_Center, LDMS_M_Division, LDMS_M_Department, LDMS_M_Section, LDMS_M_User>
                        (_schema + ".[usp_User_READ_BY_EmployeeId] @param_EmployeeId",
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
                           param: new { @param_EmployeeId = employeeId });

                    var user = items.FirstOrDefault();
                    if (user != null)
                    {
                        var roles = await All<LDMS_M_Role>("Role");
                        var jobGrades = await All<LDMS_M_JobGrade>("JobGrade");
                        var jobTitles = await All<LDMS_M_JobTitle>("JobTitle");
                        user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == user.ID_Role);
                        user.LDMS_M_JobGrade = jobGrades.FirstOrDefault(e => e.ID_JobGrade == user.ID_JobGrade);
                        user.LDMS_M_JobTitle = jobTitles.FirstOrDefault(e => e.ID_JobTitle == user.ID_JobTitle);   
                        if(user.LDMS_M_Role==null || user.LDMS_M_Role.ID_Role == 0)
                        {
                            if(user.LDMS_M_JobTitle.ID_JobTitle== 13)
                            {
                                user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 3);
                            }
                            else if ((user.LDMS_M_JobTitle.ID_JobTitle == 17 || user.LDMS_M_JobTitle.ID_JobTitle==24) &&
                                (user.LDMS_M_JobGrade.ID_JobGrade== 7 || user.LDMS_M_JobGrade.ID_JobGrade == 8))
                            {
                                user.LDMS_M_Role = roles.FirstOrDefault(e => e.ID_Role == 4);
                            }
                        }
                    }
                    return new ServiceResult(user);
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> UpdateUserSection(List<LDMS_M_User> userRoles)
        {
            try
            {
                var groupBySection = userRoles.GroupBy(e => e.ID_Section);
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
                foreach (var role in userRoles)
                {
                    DataRow row = dt.NewRow();
                    row["Employee_ID"] = role.EmployeeID;
                    row["ID_Section"] = role.ID_Section;
                    row["Is_Header"] = role.ID_Section > 0?role.IsSectionHead:false;
                    dt.Rows.Add(row);
                }

                using (IDbConnection conn = Connection)
                {
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
                var roles = await All<LDMS_M_Role>("Role");
                var jobGrades = await All<LDMS_M_JobGrade>("JobGrade");
                var jobTitles = await All<LDMS_M_JobTitle>("JobTitle");
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<LDMS_M_User, LDMS_M_Plant, LDMS_M_Center, LDMS_M_Division, LDMS_M_Department, LDMS_M_Section, LDMS_M_User>
                      (_schema + ".[usp_User_READ_BY_DepartmentId] @param_DepartmentId",
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
                           param: new { @param_DepartmentId = departmentId });
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
                var roles = await All<LDMS_M_Role>("Role");
                var jobGrades = await All<LDMS_M_JobGrade>("JobGrade");
                var jobTitles = await All<LDMS_M_JobTitle>("JobTitle");

                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<LDMS_M_User, LDMS_M_Plant, LDMS_M_Center, LDMS_M_Division, LDMS_M_Department, LDMS_M_Section, LDMS_M_User>
                   (_schema + ".[usp_User_READ_BY_SectionId] @param_SectionId",
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
                           param: new { @param_SectionId = sectionId });
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
                var user = serviceResult.Data as LDMS_M_User;
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
                if(user.ID_Plant>0 && user.ID_Center<=0 && user.ID_Division<=0 && user.ID_Department<=0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.PlantManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division <= 0 && user.ID_Department <= 0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.CenterManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division >0 && user.ID_Department <= 0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.DivisionManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division > 0 && user.ID_Department >0 && user.ID_Section <= 0)
                {
                    userLevel = UserLevel.DepartmentManager;
                }
                else if (user.ID_Plant > 0 && user.ID_Center > 0 && user.ID_Division > 0 && user.ID_Department > 0 && user.ID_Section >0 && user.IsSectionHead)
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

        private void CheckRedirectPage(LDMS_M_User user)
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
                int rolId = 0;
                int.TryParse(claim.Value, out rolId);
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
            using (System.Data.IDbConnection conn = Connection)
            {
                var items = Connection.Query<LDMS_M_SubModule, LDMS_M_Module, LDMS_M_RolePermission, LDMS_M_Role, LDMS_M_SubModule>
                (_schema + ".[usp_RoleMenu_READ_By_Role] @paramRoleId",
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
                  param: new { @paramRoleId = roleId });

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
        }

        public async Task<ServiceResult> DeleteUser(string employeeId)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_Delete] @paramEmployeeId,@paramUpdateBy",
                        new
                        {
                            @paramEmployeeId = employeeId,
                            @paramUpdateBy = JwtManager.Instance.GetUserId(HttpContext.Request)
                        });
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    CreateDataLog(DataLogType.DeleteAccount, employeeId, string.Format("Delete {0}", employeeId));
                    return new ServiceResult();
                }
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
                using (System.Data.IDbConnection conn = Connection)
                {
                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_ResetPassword] @EmployeeId,@Password,@PasswordSalt,@UpdateBy",
                        new
                        {
                            @EmployeeId = employeeId,
                            @Password = newHaspass,
                            @PasswordSalt = passsalt,
                            @UpdateBy = JwtManager.Instance.GetUserId(HttpContext.Request)
                        });
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    CreateDataLog(DataLogType.ResetPassword, employeeId, "Reset password.");
                    return new ServiceResult();
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> CreateUser(LDMS_M_User user)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
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
                    parameter.Add("@RoleId", user.ID_Role);
                    parameter.Add("@IsInstructer", user.IsInstructor);
                    parameter.Add("@IsSectionHead", user.IsSectionHead);
                    parameter.Add("@Nationality", user.Nationality); 
                    parameter.Add("@Gender", user.Gender);
                    parameter.Add("@Password", PasswordHelper.GenerateSaltedHash(user.EmployeeID, passsalt));
                    parameter.Add("@PasswordSalt", passsalt);
                    parameter.Add("@Remark", user.Remark, System.Data.DbType.AnsiString);
                    parameter.Add("@PhoneNumber", user.PhoneNumber);
                    parameter.Add("@Email", user.Email, System.Data.DbType.AnsiString);
                    parameter.Add("@CreateBy", JwtManager.Instance.GetUserId(HttpContext.Request));
                    parameter.Add("@IDCardNumber", user.IDCardNumber);
                    parameter.Add("@JoinDate", user.JoinDate);
                    parameter.Add("@OutDate", user.OutDate);
                    parameter.Add("@DateOfBirth", user.DateOfBirth);
                    parameter.Add("@ProfilePath",user.ProfilePath);
                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_Create]", param: parameter);
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    return await GetUserByEmployeeId(user.EmployeeID);
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }

        public async Task<ServiceResult> UpdateUser(LDMS_M_User user)
        {
            try
            {
                using (System.Data.IDbConnection conn = Connection)
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
                    parameter.Add("@RoleId", user.ID_Role);
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
                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_Create]", param: parameter);
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    } 
                    CreateDataLog(DataLogType.UpdateAccount, user.EmployeeID, string.Format("Update {0}", user.EmployeeID));
                    return await GetUserByEmployeeId(user.EmployeeID);
                }
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

                using (System.Data.IDbConnection conn = Connection)
                {
                    var passsalt = PasswordHelper.CreateSalt();
                    var newHaspass = PasswordHelper.GenerateSaltedHash(newpassword, passsalt);
                    var oldPasshash = PasswordHelper.GenerateSaltedHash(currentPassword, (emp.Data as LDMS_M_User).PasswordSalt);

                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_ChangePassword] @EmployeeId, @OldPassword, @Password,@PasswordSalt,@UpdateBy",
                        new
                        {
                            @EmployeeId = employeeId,
                            @OldPassword = oldPasshash,
                            @Password = newHaspass,
                            @PasswordSalt = passsalt,
                            @UpdateBy = JwtManager.Instance.GetUserId(HttpContext.Request)
                        });
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    emp = await GetUserByEmployeeId(employeeId);

                    var user = emp.Data as LDMS_M_User;

                    HttpContext.Response.Set("FORCECHANGEPASS", user.IsForceChangePass.ToString(), 120);
                    HttpContext.Response.Set("ALLOWGPP", user.IsAllowGPP.ToString(), 120); 
                    CheckRedirectPage(emp.Data as LDMS_M_User);
                    CreateDataLog(DataLogType.ChangePassword, employeeId, "Change Password.");
                    return emp;
                }
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
              //  var emp = await GetUserByEmployeeId(employeeId);

                using (System.Data.IDbConnection conn = Connection)
                {
                    DynamicParameters parameter = new DynamicParameters();
                    parameter.Add("@EmployeeId", employeeId, System.Data.DbType.String);
                    parameter.Add("@IsAllow", isAllow?1:0, System.Data.DbType.Int32);
                    parameter.Add("@UpdateBy", JwtManager.Instance.GetUserId(HttpContext.Request), System.Data.DbType.String);

                    var items = Connection.Query<SQLError>(_schema + ".[usp_User_AllowGPP]", param: parameter, commandType: System.Data.CommandType.StoredProcedure);
                    if (items != null && items.Any())
                    {
                        return new ServiceResult(new Exception(items.FirstOrDefault().ErrorMessage));
                    }
                    var emp = await GetUserByEmployeeId(employeeId);
                    var user = emp.Data as LDMS_M_User;
                    HttpContext.Response.Set("FORCECHANGEPASS", user.IsForceChangePass.ToString(), 120);
                    HttpContext.Response.Set("ALLOWGPP", user.IsAllowGPP.ToString(), 120); 
                
                    CheckRedirectPage(emp.Data as LDMS_M_User);
                    CreateDataLog(DataLogType.AcceptGPP, employeeId, string.Format("Allow {0}", isAllow));
                    return emp;
                }
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return new ServiceResult(x);
            }
        }
    }
}