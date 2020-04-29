﻿using System;

namespace LDMS.ViewModels
{
    public class LDMS_T_Coaching
    {
        public int ID { get; set; }
        public int ID_Class { get; set; }
        public int ID_Course { get; set; }
        public string EmployeeID { get; set; }
        public string CoachingStatus { get; set; }
        public string Topic { get; set; }
        public DateTime AssignDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
