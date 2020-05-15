using System;

namespace LDMS.ViewModels
{
    public class LDMS_M_User : BaseViewModel
    {
        public long ID_User { get; set; }
        public string EmployeeID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } 
        public string DriverLicenseID { get; set; }
        public string IDCardNumber { get; set; }

        public int ID_JobGrade { get; set; }
        public int ID_JobTitle { get; set; }
        public int ID_Plant { get; set; }
        public int ID_Center { get; set; }
        public int ID_Division { get; set; }
        public int ID_Department { get; set; }
        public int ID_Section { get; set; }
        public int ID_Role { get; set; }

        public DateTime? JoinDate { get; set; }
        public DateTime? OutDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ProfilePath { get; set; }
        
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Remark { get; set; }

        public bool IsAD { get; set; }
        public bool IsInstructor { get; set; }
        public bool IsSectionHead { get; set; }
        public bool IsForceChangePass { get; set; }
        public bool IsAllowGPP { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public LDMS_M_Plant LDMS_M_Plant { get; set; }
        public LDMS_M_Center LDMS_M_Center { get; set; } 
        public LDMS_M_Division LDMS_M_Division { get; set; }
        public LDMS_M_Department LDMS_M_Department { get; set; }
        public LDMS_M_Section LDMS_M_Section { get; set; }
        public LDMS_M_JobGrade LDMS_M_JobGrade { get; set; }
        public LDMS_M_JobTitle LDMS_M_JobTitle { get; set; } 
        public LDMS_M_Role LDMS_M_Role { get; set; }  
        
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", Name, Surname);
            }
        }
        public LDMS_M_User()
        {
            LDMS_M_Plant = new LDMS_M_Plant();
            LDMS_M_Center = new LDMS_M_Center();
            LDMS_M_Division = new LDMS_M_Division();
            LDMS_M_Department = new LDMS_M_Department();
            LDMS_M_Section = new LDMS_M_Section();
            LDMS_M_JobGrade = new LDMS_M_JobGrade();
            LDMS_M_JobTitle = new LDMS_M_JobTitle();
            LDMS_M_Role = new LDMS_M_Role();
        }
    }
}
public enum UserLevel
{
    PlantManager = 1,
    CenterManager = 2,
    DivisionManager = 3,
    DepartmentManager = 4,
    SectionManager = 5,
    User = 6
}
