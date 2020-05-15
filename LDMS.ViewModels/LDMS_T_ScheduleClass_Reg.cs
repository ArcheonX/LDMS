using System;
using System.Collections.Generic;
using System.Text;

namespace LDMS.ViewModels
{
    public class LDMS_T_ScheduleClass_Reg
    {
        public string PlatformName_EN { get; set; }
        public string SubPlatformName_EN { get; set; }
        public string ID_Course { get; set; }
        public string CourseName { get; set; }
        public string ID_Class { get; set; }
        public string ClassID { get; set; }
        public string InstructorName { get; set; }
        public string LearnDateStart { get; set; }
        public string LearnTimeStart { get; set; }
        public string LearnTimeEnd { get; set; }
        public string Capacity { get; set; }
        public string Registed { get; set; }
        public string VenueRoomID { get; set; }
        public string RoomName_EN { get; set; }
        public string CapReg { get; set; }
        public string Time { get; set; }
        public string EmployeeID { get; set; }
        public string Requester { get; set; }
        public string JobTitleName_EN { get; set; }
        public string RemarkAdmin { get; set; }
        public string RemarkManager { get; set; }

        public List<LDMS_T_ScheduleClass_EmployeeInfo> lsEmployee { get; set; }


    }
}
