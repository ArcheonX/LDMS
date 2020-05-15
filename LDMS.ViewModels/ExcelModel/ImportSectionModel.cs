using System;
using System.Collections.Generic;
using System.Text;

namespace LDMS.ViewModels
{
    public class ImportSectionModel
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeSurname { get; set; }
        public string JobGrade { get; set; }
        public string JobTitle { get; set; }
        public string Section { get; set; }
        public bool IsValid { get; set; }
        public string Remark { get; set; }
        public int ID_Section { get; set; }
        public int ID_JobGrade { get; set; }
        public int ID_JobTitle { get; set; }
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", EmployeeName, EmployeeSurname);
            }
        }
    }
}
