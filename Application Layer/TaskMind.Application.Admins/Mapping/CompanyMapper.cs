using TaskMind.Applications.Admins.Common;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class CompanyMapper
    {
        public static CompanyDto ToDto(Company c, int staffCount = 0, int projectCount = 0) => new CompanyDto
        {
            Id = c.Id,
            Name = c.CompanyName,
            TaxCode = c.TaxCode,
            Field = c.Field,
            Email = c.Email,
            Phone = c.Phone,
            Package = c.MembershipPackage,
            Status = VerifiableEntityStatusHelper.Derive(c.IsVerified, c.Status),
            JoinedDateUtc = c.JoinDate,
            StaffCount = staffCount,
            ProjectCount = projectCount
        };

        public static CompanyDetailDto ToDetailDto(Company c, int staffCount, int projectCount) => new CompanyDetailDto
        {
            Id = c.Id,
            Name = c.CompanyName,
            TaxCode = c.TaxCode,
            Field = c.Field,
            Email = c.Email,
            Phone = c.Phone,
            Package = c.MembershipPackage,
            Status = VerifiableEntityStatusHelper.Derive(c.IsVerified, c.Status),
            JoinedDateUtc = c.JoinDate,
            StaffCount = staffCount,
            ProjectCount = projectCount,
            Address = $"{c.Address.Street}, {c.Address.City}, {c.Address.Country}".Trim().Trim(',').Trim()
        };
    }
}
