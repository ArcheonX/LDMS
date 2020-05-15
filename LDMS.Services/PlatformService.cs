using Dapper;
using LDMS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LDMS.Daos;
using LDMS.Identity;

namespace LDMS.Services
{
    public class PlatformService : ILDMSService
    {
        private readonly ILogger<PlatformService> _logger;
        public PlatformService(
             ILogger<PlatformService> logger,
            ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _logger = logger;
        }

        public int SelectPlatform(string PlatformID/*, string PlatformName*/)
        {

            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@PlatformID", PlatformID);
                //p.Add("@PlatformName", PlatformName);

                int ret = conn.Query<int>(_schema + ".[sp_M_PlatformByPlatformID_AndName]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return ret;
            }
        }


        public int CreatePlatform(string PlatformID, string PlatformName_EN, string PlatformName_TH, 
                                  string ID_PlatformType, string PlatformDescription, string ID_Department_Create,
                                  string PlatformStatus)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@PlatformID ", PlatformID);
                p.Add("@PlatformName_EN", PlatformName_EN);
                p.Add("@PlatformName_TH", PlatformName_TH);
                p.Add("@ID_PlatformType", ID_PlatformType);
                p.Add("@PlatformDescription", PlatformDescription);
                p.Add("@ID_Department_Create", JwtManager.Instance.GetFromToken(HttpContext.Request, "DEPARTMENTID")); 
                p.Add("@PlatformStatus", PlatformStatus);
                p.Add("@CreateBy", CurrentUserId); // JwtManager.Instance.GetUserId(HttpContext.Request) //Example

                int ret = conn.Query<int>(_schema + ".[sp_M_Platform_Insert]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return ret;
            }
        }

        public bool UpdatePlatform(string ID_Platform, string PlatformID, string PlatformName_EN, string PlatformName_TH,
                                  string ID_PlatformType, string PlatformDescription, string ID_Department_Create, string PlatformStatus)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Platform ", ID_Platform);
                p.Add("@PlatformID ", PlatformID);
                p.Add("@PlatformName_EN", PlatformName_EN);
                p.Add("@PlatformName_TH", PlatformName_TH);
                p.Add("@ID_PlatformType", ID_PlatformType);
                p.Add("@PlatformDescription", PlatformDescription);
                p.Add("@ID_Department_Create", JwtManager.Instance.GetFromToken(HttpContext.Request, "DEPARTMENTID"));
                p.Add("@PlatformStatus", PlatformStatus);
                p.Add("@UpdateBy", CurrentUserId); // JwtManager.Instance.GetUserId(HttpContext.Request) //Example

                int ret = conn.Query<int>(_schema + ".[sp_M_Platform_Update]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return true;
            }
        }


        public LDMS_M_Platform GetPlatformByID(string ID)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@ID", ID);

                    LDMS_M_Platform item = conn.Query<LDMS_M_Platform>("[dbo].[sp_M_Platform_Select_byID]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                    return item;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public async Task<List<LDMS_M_Platform>> GetAll(string PlatformId = null, string PlatformName = null, string PlatformType = null, int PlatformStatus = 1)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                try
                {
                    var items = Connection.Query<LDMS_M_Platform>
                    (_schema + ".[sp_M_Platfrom_Select] @paramPlatformId,@paramPlatformName,@paramPlatformType,@paramPlatformStatus",
                     new { @paramPlatformId = PlatformId, 
                           @paramPlatformName = PlatformName, 
                           @paramPlatformType = PlatformType, 
                           @paramPlatformStatus = PlatformStatus
                     });

                    return (List<LDMS_M_Platform>)items;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public Paging_Result GetPlatform(LDMS_M_Platform_Search criteria)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                Paging_Result ret = new Paging_Result();
                List<LDMS_M_Platform> PlatformList = new List<LDMS_M_Platform>();
                try
                {

                    var p = new DynamicParameters();
                    if (criteria.PlatformId != null) p.Add("@PlatformId", criteria.PlatformId);
                    if (criteria.PlatformName != null) p.Add("@PlatformName", criteria.PlatformName);
                    if (criteria.PlatformType != null) p.Add("@PlatformType", criteria.PlatformType);
                    if (criteria.PlatformStatus != null) p.Add("@PlatformStatus", criteria.PlatformStatus);
                    
                    p.Add("@PageNum", criteria.PageNum);
                    p.Add("@PageSize", criteria.PageSize);
                    p.Add("@SortField", criteria.SortField);
                    p.Add("@SortOrder", criteria.SortOrder);


                    var grid = conn.QueryMultiple("[dbo].[sp_M_Platfrom_Select_Paging]", p, commandType: System.Data.CommandType.StoredProcedure);

                    PlatformList = grid.Read<LDMS_M_Platform>().ToList();
                    var totalRec = grid.Read().ToList();

                    ret.data = PlatformList;
                    ret.recordsTotal = totalRec[0].TotalRecords;
                    ret.recordsFiltered = totalRec[0].TotalRecords;
                    ret.draw = 0;

                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }


        public async Task<List<ViewModels.LDMS_M_PlatformType>> GetAllPlatformType()
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                try
                {
                    var items = Connection.Query<LDMS_M_PlatformType>
                    (_schema + ".[sp_M_PlatformType_ALL]");
                    
                    return (List<LDMS_M_PlatformType>)items;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public async Task<List<LDMS_M_Course>> GetCourseAll(string CourseID, string CourseName, string CourseStatus, string LearnMethod)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                try
                {
                    var items = Connection.Query<LDMS_M_Course>
                    (_schema + ".[sp_M_Course_Select] @paramCourseId,@paramCourseName,@paramCourseStatus,@paramLearnMethod",
                     new
                     {
                         @paramCourseId = CourseID,
                         @paramCourseName = CourseName,
                         @paramCourseStatus = CourseStatus,
                         @paramLearnMethod = LearnMethod
                     });

                    return (List<LDMS_M_Course>)items;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }


        public async Task<List<ViewModels.LDMS_M_CourseLearnMethod>> GetAllCourseLearnMethod()
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                try
                {
                    var items = Connection.Query<LDMS_M_CourseLearnMethod>
                    (_schema + ".[sp_M_CourseLearnMethod_ALL]");

                    return (List<LDMS_M_CourseLearnMethod>)items;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public int CreateSubPlatform(string ID_Platform, string SubPlatformName_EN, string SubPlatformName_TH)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Platform ", ID_Platform);
                p.Add("@SubPlatformName_EN", SubPlatformName_EN);
                p.Add("@SubPlatformName_TH", SubPlatformName_TH);
                p.Add("@CreateBy", CurrentUserId); // JwtManager.Instance.GetUserId(HttpContext.Request) //Example

                int ret = conn.Query<int>( _schema + ".[sp_M_SubPlatform_Insert]", p, commandType: System.Data.CommandType.StoredProcedure).Single();
  
                return ret;
            }
        }

        public int UpdateSubPlatform(string ID_SubPlatform, string SubPlatformName_EN, string SubPlatformName_TH)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_SubPlatform ", ID_SubPlatform);
                p.Add("@SubPlatformName_EN", SubPlatformName_EN);
                p.Add("@SubPlatformName_TH", SubPlatformName_TH);
                p.Add("@UpdateBy", CurrentUserId); // JwtManager.Instance.GetUserId(HttpContext.Request) //Example

                int ret = conn.Query<int>(_schema + ".[sp_M_SubPlatform_Update]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return ret;
            }
        }


        public int DeleteSubPlatform(string ID_SubPlatform)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_SubPlatform ", ID_SubPlatform);

                int ret = conn.Query<int>(_schema + ".[sp_M_SubPlatform_Delete]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return ret;
            }
        }


        public Paging_Result GetSubPlatform(LDMS_M_SubPlatform_Search criteria)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
              
                Paging_Result ret = new Paging_Result();
                List<LDMS_M_SubPlatform> subPlatformList = new List<LDMS_M_SubPlatform>();
                try
                {

                    var p = new DynamicParameters();
                    if (criteria.ID_Platform != null) p.Add("@ID_Platform", criteria.ID_Platform);
                    if (criteria.Keyword != null) p.Add("@keyword", criteria.Keyword);


                    p.Add("@PageNum", criteria.PageNum);
                    p.Add("@PageSize", criteria.PageSize);
                    p.Add("@SortField", criteria.SortField);
                    p.Add("@SortOrder", criteria.SortOrder);


                    var grid = conn.QueryMultiple("[dbo].[sp_M_SubPlatform_Select_Paging]", criteria, commandType: System.Data.CommandType.StoredProcedure);

                    subPlatformList = grid.Read<LDMS_M_SubPlatform>().ToList();
                    var totalRec = grid.Read().ToList();

                    ret.data = subPlatformList;
                    ret.recordsTotal = totalRec[0].TotalRecords;
                    ret.recordsFiltered = totalRec[0].TotalRecords;
                    ret.draw = 0;

                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                
            }
        }

        public int GetSubPlatform(string ID_Platform, string SubPlatformNameEN )
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                try
                {
                    var p = new DynamicParameters();
                    p.Add("@ID_Platform ", ID_Platform);
                    p.Add("@SubPlatformNameEN ", SubPlatformNameEN);

                    var items = conn.Query<LDMS_M_SubPlatform>("[dbo].[sp_M_SubPlatform_Select]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    
                    return items.Count;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }




        public List<LDMS_M_SubPlatformCourse> SelectLDMS_M_SubPlatformCourse(string ID_Platform)
        {
            List<LDMS_M_SubPlatformCourse> listSubPlatformCourse = new List<LDMS_M_SubPlatformCourse>();
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                //p.Add("@ID_Course", ID_Course);
                p.Add("@ID_Platform", ID_Platform);
                
                var grid = conn.QueryMultiple("[dbo].[sp_M_SubPlatformCourseJson_Select_with_ID_Platfrom]", p, commandType: System.Data.CommandType.StoredProcedure);

                listSubPlatformCourse = grid.Read<LDMS_M_SubPlatformCourse>().ToList();


                return listSubPlatformCourse;
            }
        }

        public bool CreateSubPlatformCourse(string model, string ID_SubPlatform)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@json", model);
                p.Add("@ID_SubPlatform", ID_SubPlatform);
                int ret = conn.Query<int>(_schema + ".[sp_M_SubPlatformCourseJson_Insert]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return true;
            }
        }

        public LDMS_M_SubPlatform LoadSubPlatformByID(string ID_SubPlatform)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_SubPlatform", ID_SubPlatform);
                LDMS_M_SubPlatform ret = conn.Query<LDMS_M_SubPlatform>(_schema + ".[sp_M_SubPlatformByID]", p, commandType: System.Data.CommandType.StoredProcedure).Single();
                //ret.ListCourse = conn.Query<LDMS_M_SubPlatformCourse>(_schema + ".[sp_M_SubPlatformCourseBy_SubPlatformID]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();
                return ret;
            }
        }

        public List<LDMS_M_Course> LoadSubPlatformCourseByID(string ID_SubPlatform)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_SubPlatform", ID_SubPlatform);

                List<LDMS_M_Course> ret = conn.Query<LDMS_M_Course>(_schema + ".[sp_M_SubPlatformCourseBy_SubPlatformID]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();
                return ret;
            }
        }

        public Paging_Result LoadSubPlatformCourse(LDMS_M_SubPlatformCourse_Search criteria )
        {
            using (System.Data.IDbConnection conn = Connection)
            {

                Paging_Result ret = new Paging_Result();
                List<LDMS_M_Course> courses = new List<LDMS_M_Course>();
                try
                {

                    var p = new DynamicParameters();
                    if (criteria.ID_SubPlatform != null) p.Add("@ID_SubPlatform", criteria.ID_SubPlatform);
                    if (criteria.Keyword != null) p.Add("@Keyword", criteria.Keyword);


                    p.Add("@PageNum", criteria.PageNum);
                    p.Add("@PageSize", criteria.PageSize);
                    p.Add("@SortField", criteria.SortField);
                    p.Add("@SortOrder", criteria.SortOrder);


                    var grid = conn.QueryMultiple("[dbo].[sp_M_SubPlatformCourseSelect_Paging]", criteria, commandType: System.Data.CommandType.StoredProcedure);

                    courses = grid.Read<LDMS_M_Course>().ToList();
                    var totalRec = grid.Read().ToList();

                    ret.data = courses;
                    ret.recordsTotal = totalRec[0].TotalRecords;
                    ret.recordsFiltered = totalRec[0].TotalRecords;
                    ret.draw = 0;

                    return ret;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

            }
        }

        public int DeleteSubPlatformCourse( string ID_SubPlatform, string ID_Course)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_SubPlatform ", ID_SubPlatform);
                p.Add("@ID_Course", ID_Course);
              
                int ret = conn.Query<int>(_schema + ".[sp_M_SubPlatformCourseJson_Delete]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return ret;
            }

        }

        public List<LDMS_M_Platform> GetPlatformByDep()
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@ID_Department ", JwtManager.Instance.GetFromToken(HttpContext.Request, "DEPARTMENTID"));

                List<LDMS_M_Platform> ret = conn.Query<LDMS_M_Platform>(_schema + ".[sp_M_Platfrom_SelectALL_WithDep]", p, commandType: System.Data.CommandType.StoredProcedure).ToList();

                return ret;
            }
        } 

    }
}
