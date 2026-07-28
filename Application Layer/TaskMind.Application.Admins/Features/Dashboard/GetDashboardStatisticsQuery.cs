using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Dashboard
{
    /// <summary>Số liệu tổng quan cho trang Dashboard của Admin (mục 4.13, tổng hợp toàn hệ thống).</summary>
    public class GetDashboardStatisticsQuery : IRequest<DashboardStatisticDto>
    {
        /// <summary>Số ngày gần đây tính là "mới"; mặc định 30 ngày.</summary>
        public int RecentDays { get; set; } = 30;
    }

    public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, DashboardStatisticDto>
    {
        private readonly IApplicationDbContext _db;

        public GetDashboardStatisticsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardStatisticDto> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
        {
            var since = DateTimeOffset.UtcNow.AddDays(-Math.Max(1, request.RecentDays));

            var countAllUsers = await _db.Users.CountAsync(cancellationToken);
            var countNewUsers = await _db.Users.CountAsync(u => u.CreatedAtUtc >= since, cancellationToken);

            var countAllCompanies = await _db.Companies.CountAsync(cancellationToken);
            var countNewCompanies = await _db.Companies.CountAsync(c => c.CreatedAtUtc >= since, cancellationToken);
            var countPendingCompanyApprovals = await _db.Companies
                .CountAsync(c => !c.IsVerified && c.Status != EntityStatus.Blocked, cancellationToken);

            var countAllSchools = await _db.Schools.CountAsync(cancellationToken);
            var countNewSchools = await _db.Schools.CountAsync(s => s.CreatedAtUtc >= since, cancellationToken);
            var countPendingSchoolApprovals = await _db.Schools
                .CountAsync(s => !s.IsVerified && s.Status != EntityStatus.Blocked, cancellationToken);

            var countAllTeachers = await _db.Teachers.CountAsync(cancellationToken);
            var countNewTeachers = await _db.Teachers.CountAsync(t => t.CreatedAtUtc >= since, cancellationToken);

            var countAllStaff = await _db.Staffs.CountAsync(cancellationToken);
            var countNewStaff = await _db.Staffs.CountAsync(s => s.CreatedAtUtc >= since, cancellationToken);

            var countAllProjects = await _db.Projects.CountAsync(cancellationToken);
            var countNewProjects = await _db.Projects.CountAsync(p => p.CreatedAtUtc >= since, cancellationToken);

            var countPendingSkillApprovals = await _db.Skills.CountAsync(s => !s.IsApproved, cancellationToken);

            return new DashboardStatisticDto
            {
                CountAllUsers = countAllUsers,
                CountNewUsers = countNewUsers,
                CountAllCompanies = countAllCompanies,
                CountNewCompanies = countNewCompanies,
                CountAllSchools = countAllSchools,
                CountNewSchools = countNewSchools,
                CountAllTeachers = countAllTeachers,
                CountNewTeachers = countNewTeachers,
                CountAllStaff = countAllStaff,
                CountNewStaff = countNewStaff,
                CountAllProjects = countAllProjects,
                CountNewProjects = countNewProjects,
                CountPendingCompanyApprovals = countPendingCompanyApprovals,
                CountPendingSchoolApprovals = countPendingSchoolApprovals,
                CountPendingSkillApprovals = countPendingSkillApprovals
            };
        }
    }
}
