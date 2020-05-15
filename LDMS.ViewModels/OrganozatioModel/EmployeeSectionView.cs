using LDMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LDMS.ViewModels
{
    public class EmployeeSectionView : LDMS_M_User
    {
        public string JobGrade { get; set; }
        public string JobTitle { get; set; }
        public List<LDMS_M_Section> LDMS_M_Sections { get; set; }

        public EmployeeSectionView(LDMS_M_User lDMS_M_User)
        { 
            LDMS_M_Sections = new List<LDMS_M_Section>();
            EmployeeID = lDMS_M_User.EmployeeID;
            IsAD = lDMS_M_User.IsAD;
            DriverLicenseID = lDMS_M_User.DriverLicenseID;
            IDCardNumber = lDMS_M_User.IDCardNumber;
            Gender = lDMS_M_User.Gender;
            Name = lDMS_M_User.Name;
            Surname = lDMS_M_User.Surname;
            Nationality = lDMS_M_User.Nationality;
            ID_JobGrade = lDMS_M_User.ID_JobGrade;
            ID_JobTitle = lDMS_M_User.ID_JobTitle;
            ID_Plant = lDMS_M_User.ID_Plant;
            ID_Center = lDMS_M_User.ID_Center;
            ID_Division = lDMS_M_User.ID_Division;
            ID_Department = lDMS_M_User.ID_Department;
            PhoneNumber = lDMS_M_User.PhoneNumber;
            Email = lDMS_M_User.Email; 
            LDMS_M_Department = lDMS_M_User.LDMS_M_Department;
            LDMS_M_Plant = lDMS_M_User.LDMS_M_Plant;
            RowIndex = lDMS_M_User.RowIndex;
            IsSectionHead = lDMS_M_User.IsSectionHead;
            ID_Section = lDMS_M_User.ID_Section;
        }
    }
}
