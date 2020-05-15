using System;
using System.Collections.Generic;
using System.Text;

namespace LDMS.ViewModels
{
    public class TeamLearningPerformance
    {
        public int ID_Platform { get; set; }
        public int ID_Course { get; set; }
        public string PlatformID { get; set; }
        public string PlatformName_EN { get; set; }
        public string PlatformName_TH { get; set; }
        public string SubPlatformName_EN { get; set; }
        public string SubPlatformName_TH { get; set; }

        public string CourseID { get; set; }
        public string CourseName { get; set; }

        public string Course_LearnMethodName_EN { get; set; }
        public string Course_LearnMethodName_TH { get; set; } 
        public DateTime? TargetDate { get; set; }
        public int TargetMonth { get; set; }
        public int TargetYear { get; set; }

        public int RemainDay { get; set; }
        public string CourseStatus { get; set; } 
        public string CourseObjective { get; set; }

        public string CourseDescription { get; set; }
        public string CourseOutLine { get; set; }

        public DateTime LearnDateStart { get; set; }
        public string LearnTimeStart { get; set; }
        public DateTime LearnDateEnd { get; set; }
        public string LearnTimeEnd { get; set; }

        public int ClassCapacity { get; set; }
        public decimal ClassFee { get; set; }
        public decimal ClassFeePerPerson { get; set; }
        public string VenueRoomID { get; set; }
        public string RoomName_EN { get; set; }

        public int ID_JobGrade { get; set; }
        public string JobGradeID { get; set; }
        public string JobGradeName_EN { get; set; }
        public string JobGradeName_TH { get; set; }

        public int ID_JobTitle { get; set; }
        public string JobTitleID { get; set; }
        public string JobTitleName_EN { get; set; }
        public string JobTitleName_TH { get; set; }

        public int ID_Plant { get; set; }
        public string PlantID { get; set; }
        public string PlantName_EN { get; set; }
        public string PlantName_TH { get; set; }

        public int ID_Center { get; set; }
        public string CenterID { get; set; } 
        public string CenterName_EN { get; set; }
        public string CenterName_TH { get; set; }

        public int ID_Division { get; set; }
        public string DivisionID { get; set; } 
        public string DivisionName_EN { get; set; }
        public string DivisionName_TH { get; set; }

        public int ID_Department { get; set; }
        public string DepartmentID { get; set; } 
        public string DepartmentName_EN { get; set; }
        public string DepartmentName_TH { get; set; }

        public int ID_Section { get; set; }
        public string SectionID { get; set; } 
        public string SectionName_EN { get; set; }
        public string SectionName_TH { get; set; }

        public string TargetJobGrade { get; set; }
        public string TargetJobTitle { get; set; }
        public string TargetEmployeeID { get; set; }
    }
}
