using System;

namespace LDMS.ViewModels
{
    public class LDMS_T_Class
    {
        public int ID { get; set; }
        public string ClassID { get; set; }
        public string LMS_PACD_courseID { get; set; }
        public int ID_Course { get; set; }
        public string CourseName { get; set; }
        public int ID_Instructor { get; set; }
        public string InstructorName { get; set; }
        public int ClassCapacity { get; set; }
        public decimal ClassFee { get; set; }
        public string LearnDateStart { get; set; }
        public string LearnTimeStart { get; set; }
        public string LearnDateEnd { get; set; }
        public string LearnTimeEnd { get; set; }
        public string RegisterDateStart { get; set; }
        public string RegisterDateEnd { get; set; }
        public int ID_PlantVenue { get; set; }
        public int ID_VenueRoom { get; set; }
        public string PlaceAndLocation { get; set; }
        public int ClassStatus { get; set; }
        public byte IsAttend { get; set; }
        public int AttendNum { get; set; }
        public byte IsTest { get; set; }
        public int TestFullScore { get; set; }
        public int TestPercentage { get; set; }
        public byte IsSkill { get; set; }
        public int SkillFullScore { get; set; }
        public int SkillPercentage { get; set; }
        public byte IsCoaching { get; set; }
        public byte IsCertificate { get; set; }
        public byte IsAttachCert { get; set; }
        public string CertificationRemark { get; set; }
        public int ReminderDurationDay { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public byte IsActive { get; set; }

        public string LearnDateStart_Show { get; set; }
        public string LearnDateEnd_Show { get; set; }

        public string RegisterDateStart_Show { get; set; }
        public string RegisterDateEnd_Show { get; set; }
        public int ID_LearnMethod { get; set; }
    }
}
