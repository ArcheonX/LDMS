using System;

namespace LDMS.ViewModels
{
    public class LDMS_T_IDP
    {
        public int ID { get; set; }
        public int ID_Employee { get; set; }
        public int ID_Manager { get; set; }
        public string IDP_StartDate { get; set; }
        public string IDP_EndDate { get; set; }
        public string IDP_Objective { get; set; }
        public int IDP_Status { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public int ID_Department { get; set; }
        public string EmployeeName { get; set; }
        public string IDP_StatusName { get; set; }
        public string Position { get; set; }
        public string ID_SubTopic { get; set; }
        public int Total_Review { get; set; }
        public string Service_Timeline { get; set; }
        public string IDP_TimeLine { get; set; }
    }
}
