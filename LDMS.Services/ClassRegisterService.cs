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
    public class ClassRegisterService : ILDMSService
    {
        private readonly ILogger<ClassRegisterService> _logger;
        public ClassRegisterService(
           ILogger<ClassRegisterService> logger,
           ILDMSConnection iLDMSConnection, IHttpContextAccessor httpContextAccessor) : base(iLDMSConnection, httpContextAccessor)
        {
            _logger = logger;
        }

        public bool CreateClassRegister(string model)
        {
            using (System.Data.IDbConnection conn = Connection)
            {
                var p = new DynamicParameters();
                p.Add("@json", model);
                p.Add("@EmployeeID", CurrentUserId); // Employee

                int ret = conn.Query<int>(_schema + ".[sp_T_ClassRegistrationJson_Insert]", p, commandType: System.Data.CommandType.StoredProcedure).Single();

                return true;
            }
        }
    }
}
