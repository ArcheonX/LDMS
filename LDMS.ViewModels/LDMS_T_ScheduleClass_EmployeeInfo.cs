using System;
using System.Collections.Generic;
using System.Text;

namespace LDMS.ViewModels
{
    public class LDMS_T_ScheduleClass_EmployeeInfo : LDMS_T_ScheduleClass_Reg
    {
        public string CourseID { get; set; }
        public string Registered { get; set; }
        public string Waiting { get; set; }
        public string Approved { get; set; }
        public string PlantName_EN { get; set; }
        public string PlaceAndLocation { get; set; }
        public string EmployeeName { get; set; }
        public string JobGradeName_EN { get; set; }
       
        
    }
}
