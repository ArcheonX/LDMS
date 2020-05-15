using System;
using System.Collections.Generic;
using System.Text;

namespace LDMS.ViewModels.SearchModel
{
    public class LDMS_M_IDP_Employee_Search
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentID { get; set; }
        public string DivisionID { get; set; }
        public string CenterID { get; set; }
        public string PlantID
        {
            get; set;
        }
    }
}
