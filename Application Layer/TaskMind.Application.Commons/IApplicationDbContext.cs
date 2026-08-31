using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Commons
{
    public interface IApplicationDbContext
    {
     
        public DbSet<AuditTrail> AuditTrails { get; }
        public DbSet<Admin> Admins { get; }
        public DbSet<AdminCompany> AdminCompanies { get; }
        public DbSet<AdminSchool> AdminSchools { get; }
        public DbSet<Company> Companies { get; }
        public DbSet<ExchangeContract> ExchangeContracts { get; }
        public DbSet<Invoice> Invoices { get; }
        public DbSet<Notification> Notifications { get; }
        public DbSet<Project> Projects { get; }
        public DbSet<ProjectMember> ProjectMembers { get; }
        public DbSet<School> Schools { get; }
        public DbSet<Skill> Skills { get; }
        public DbSet<SkillLevelUpRequest> SkillLevelUpRequests { get; }
        public DbSet<SkillProfile> SkillProfiles { get; }
        public DbSet<Staff> Staffs { get; }
        public DbSet<Student> Students { get; }
        public DbSet<Teacher> Teachers { get; } 
        public DbSet<User> Users { get; }
        public DbSet<Chat> Chats { get; }
        public DbSet<JobApplication> JobApplications { get; }
        public DbSet<JobPosting> JobPostings { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
