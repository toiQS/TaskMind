using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    [Index(nameof(CompanyId), nameof(LinkedUserId), IsUnique = true)]
    public class AdminCompany : Account
    {
        public Guid CompanyId { get; private set; }
        public virtual Company Company { get; private set; } = default!;

        /// <summary>Trỏ về tài khoản User gốc đã đăng ký thành lập công ty (mục 2.1, 4.1.1).</summary>
        public Guid LinkedUserId { get; private set; }

        private AdminCompany(Guid companyId, Guid linkedUserId) : base()
        {
            CompanyId = companyId;
            LinkedUserId = linkedUserId;
        }

        public static Result<AdminCompany> CreateAdminCompany(
            string citizenId,
            string email,
            string passwordHash,
            Guid companyId,
            Guid linkedUserId)
        {
            var adminCompany = new AdminCompany(companyId, linkedUserId);
            var result = adminCompany.InitializeWithCredentials(citizenId, email, AccountRole.AdminCompany, passwordHash);
            if (!result.IsSuccess)
                return Result<AdminCompany>.Failure(result.Message);

            adminCompany.AddDomainEvent(new AdminCompanyLinkedEvent
            {
                AdminCompanyAccountId = adminCompany.Id,
                LinkedUserId = linkedUserId,
                CompanyId = companyId
            });

            return Result<AdminCompany>.Success(adminCompany);
        }
    }
}
