using TaskMind.Applications.Admins.Common;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Mapping
{
    public static class CompanyMapper
    {
        public static CompanyDto ToDto(Company c) => new CompanyDto
        {
            Id = c.Id,
            Name = c.CompanyName,
            TaxCode = c.TaxCode,
            Field = c.Field,
            Email = c.Email,
            Phone = c.Phone,
            Package = c.MembershipPackage,
            Status = VerifiableEntityStatusHelper.Derive(c.IsVerified, c.Status),
            JoinedDateUtc = c.JoinDate
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
            Address = $"{c.Address.Street}, {c.Address.City}, {c.Address.Country}".Trim().Trim(',').Trim(),
            StaffCount = staffCount,
            ProjectCount = projectCount
        };
    }
}