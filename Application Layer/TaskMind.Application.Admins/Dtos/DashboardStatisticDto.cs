namespace TaskMind.Applications.Admins.Dtos
{
    public class DashboardStatisticDto
    {
        public int CountAllUsers { get; set; }
        public int CountNewUsers { get; set; }

        public int CountAllCompanies { get; set; }
        public int CountNewCompanies { get; set; }

        public int CountAllSchools { get; set; }
        public int CountNewSchools { get; set; }

        public int CountAllTeachers { get; set; }
        public int CountNewTeachers { get; set; }

        public int CountAllStaff { get; set; }
        public int CountNewStaff { get; set; }

        public int CountAllProjects { get; set; }
        public int CountNewProjects { get; set; }

        public int CountPendingCompanyApprovals { get; set; }
        public int CountPendingSchoolApprovals { get; set; }
        public int CountPendingSkillApprovals { get; set; }
    }
}