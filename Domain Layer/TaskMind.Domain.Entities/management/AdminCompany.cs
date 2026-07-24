using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Entities.company;
using TaskMind.Domain.Entities.parents;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities.management
{
    internal class AdminCompany : Account
    {
        public Guid CompanyId { get; private set; }
        public virtual Company Company { get; private set; } = default!;

        private AdminCompany(Guid companyId) : base()
        {
            CompanyId = companyId;
        }

        public static Result<AdminCompany> CreateAdminCompany(
            string citizenId,
            string email,
            string passwordHash,
            Guid companyId)
        {
            var adminCompany = new AdminCompany(companyId);
            var result = adminCompany.InitializeWithCredentials(citizenId, email, AccountRole.AdminCompany, passwordHash);
            if (!result.IsSuccess)
                return Result<AdminCompany>.Failure(result.Message);
            return Result<AdminCompany>.Success(adminCompany);

        }
    }
}
