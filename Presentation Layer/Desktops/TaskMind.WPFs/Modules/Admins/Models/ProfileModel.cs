using System;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public class ProfileModel
    {
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; } = "Quản trị viên";
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Bio { get; set; }

        public string GithubUrl { get; set; }
        public string LinkedinUrl { get; set; }
        public string WebsiteUrl { get; set; }

        public bool IsProfilePublic { get; set; }
        public DateTime JoinedDate { get; set; }

        public int ManagedCompanies { get; set; }
        public int ManagedSchools { get; set; }
        public int PendingApprovals { get; set; }
    }
}