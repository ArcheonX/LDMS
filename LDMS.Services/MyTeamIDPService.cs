﻿using Dapper;
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

        public ViewModels.Paging_Result GetMy_Team_IDP() {

            using (IDbConnection conn = Connection)
            {
                try
                {

                    ViewModels.Paging_Result ret = new ViewModels.Paging_Result();
                    List<ViewModels.LDMS_T_IDP_Master_Result> coachingList = new List<ViewModels.LDMS_T_IDP_Master_Result>();
                    var p = new DynamicParameters();
                    //if (ID_Employee != null) p.Add("@ID_Employee", ID_Employee);

                    p.Add("@ID_Assigner_Employee", "5044441111"); //CurrentUserId
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
