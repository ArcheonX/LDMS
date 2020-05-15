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
    public class MyTeamIDPService : ILDMSService
    {
        private readonly ILogger<CoachingService> _logger;
        public MyTeamIDPService(
           ILogger<CoachingService> logger,
           ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _logger = logger;
        }
        public List<ViewModels.LDMS_M_IDP_Employee> GetEmployee(ViewModels.SearchModel.LDMS_M_IDP_Employee_Search criteria)
        {
            using (IDbConnection conn = Connection)
            {
                ViewModels.Paging_Result ret = new ViewModels.Paging_Result();
                List<ViewModels.LDMS_M_IDP_Employee> EmployeeList = new List<ViewModels.LDMS_M_IDP_Employee>();
                try
                {

                    var p = new DynamicParameters();
                    if (criteria.EmployeeID != null) p.Add("@EmployeeID", criteria.EmployeeID);
                    if (criteria.EmployeeName != null) p.Add("@EmployeeName", criteria.EmployeeName);
                    if (criteria.DepartmentID != null) p.Add("@DepartmentID", criteria.DepartmentID);
                    if (criteria.DivisionID != null) p.Add("@DivisionID", criteria.DivisionID);
                    if (criteria.CenterID != null) p.Add("@CenterID", criteria.CenterID);
                    if (criteria.PlantID != null) p.Add("@PlantID", criteria.PlantID);

                    //p.Add("@PageNum", criteria.PageNum);
                    //p.Add("@PageSize", criteria.PageSize);
                    //p.Add("@SortField", criteria.SortField);
                    //p.Add("@SortOrder", criteria.SortOrder);


                    var grid = conn.QueryMultiple("[dbo].[sp_M_IDP_Employee_SelectPaging]", p, commandType: CommandType.StoredProcedure);

                    EmployeeList = grid.Read<ViewModels.LDMS_M_IDP_Employee>().ToList();
                    //var totalRec = grid.Read().ToList();

                    //ret.data = EmployeeList;
                    //ret.recordsTotal = totalRec[0].TotalRecords;
                    //ret.recordsFiltered = totalRec[0].TotalRecords;
                    //ret.draw = 0;

                    return EmployeeList;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
        public ViewModels.Paging_Result GetMy_Team_IDP(LDMS_T_IDP_Master criteria) {

            using (IDbConnection conn = Connection)
            {
                try
                {

                    ViewModels.Paging_Result ret = new ViewModels.Paging_Result();
                    List<ViewModels.LDMS_T_IDP_Master_Result> coachingList = new List<ViewModels.LDMS_T_IDP_Master_Result>();
                    var p = new DynamicParameters();
                    //if (ID_Employee != null) p.Add("@ID_Employee", ID_Employee);

                    p.Add("@ID_Assigner_Employee", criteria.ID_Manager); //CurrentUserId
                    p.Add("@ID_Assignee_Employee", criteria.ID_Employee);
                    p.Add("@ID_plant", criteria.ID_Plant);
                    p.Add("@ID_Center", criteria.ID_Center);
                    p.Add("@ID_Division", criteria.ID_Division);
                    p.Add("@ID_Department", criteria.ID_Department);
                    p.Add("@ID_Status", criteria.ID_Status);
                    var grid = conn.QueryMultiple("[dbo].[sp_T_IDP_Master_Select_Paging]", p, commandType: CommandType.StoredProcedure);
                    //var myTeamIDP = conn.Query<ViewModels.LDMS_T_IDP_Master_Result>(_schema + ".[sp_T_IDP_Master_Select_Paging]", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    coachingList = grid.Read<ViewModels.LDMS_T_IDP_Master_Result>().ToList();
                    var totalRec = grid.Read().ToList();

                    ret.data = coachingList;
                    ret.recordsTotal = totalRec[0].TotalRecords;
                    ret.recordsFiltered = totalRec[0].TotalRecords;
                    ret.draw = 0;

                    return ret;
                   // return myTeamIDP;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }
    }
}
