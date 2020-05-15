using System;
using System.Collections.Generic;

namespace LDMS.ViewModels
{
    public class TCompetenceAnalytic : BaseViewModel
    {
        public int? ID_Analytic { get; set; }
        public string ID_EmployeeManager { get; set; }
        public string CompetenceAnalyticName { get; set; }
        public string DepartmentName { get; set; }

        public string Criteria1 { get; set; }
        public string Criteria2 { get; set; }
        public string Criteria3 { get; set; }
        public string Criteria4 { get; set; }
        public string Criteria5 { get; set; } 
        public int? ID_Department { get; set; }
        public int? ID_JobGrade { get; set; }
        public string JobGrade { get; set; }

        public DateTime LastUpdate { get; set; }
        public List<TCompetenceAnalyticEmployee> Employees { get; set; }
        public List<TCompetenceAnalyticTopic> Topics { get; set; }
        public List<TCompetenceAnalyticScore> Scores { get; set; }

        //public List<ViewModels.TCompetenceAnalyticExpectatoin> Expectatoins { get; set; }
        public TCompetenceAnalytic()
        {
            Employees = new List<TCompetenceAnalyticEmployee>();
            Topics = new List<TCompetenceAnalyticTopic>();
            Scores = new List<TCompetenceAnalyticScore>();
            //Expectatoins = new List<TCompetenceAnalyticExpectatoin>();
        }
    }
}
