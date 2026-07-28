using TaskMind.Applications.Admins.Common;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class SchoolMapper
    {
        public static SchoolDto ToDto(School s) => new SchoolDto
        {
            Id = s.Id,
            Name = s.SchoolName,
            Field = s.Field,
            Email = s.Email,
            Phone = s.Phone,
            Package = s.MembershipPackage,
            Status = VerifiableEntityStatusHelper.Derive(s.IsVerified, s.Status),
            JoinedDateUtc = s.JoinDate
        };

        public static SchoolDetailDto ToDetailDto(School s, int teacherCount, int studentCount, int projectCount) => new SchoolDetailDto
        {
            Id = s.Id,
            Name = s.SchoolName,
            Field = s.Field,
            Email = s.Email,
            Phone = s.Phone,
            Package = s.MembershipPackage,
            Status = VerifiableEntityStatusHelper.Derive(s.IsVerified, s.Status),
            JoinedDateUtc = s.JoinDate,
            Address = $"{s.Address.Street}, {s.Address.City}, {s.Address.Country}".Trim().Trim(',').Trim(),
            TeacherCount = teacherCount,
            StudentCount = studentCount,
            ProjectCount = projectCount,
            CourseCount = 0
        };
    }
}