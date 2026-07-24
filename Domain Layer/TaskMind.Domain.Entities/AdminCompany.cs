using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// tài khoản admin company là tài khoản quản trị viên của công ty, có quyền quản lý các thông tin liên quan đến công ty và các tài khoản người dùng trong công ty đó.
    /// tài khoản không thể khởi tạo thông thường, chỉ có thể được tạo ra thông qua việc tạo công ty mới hoặc được cấp quyền từ admin system.
    /// </summary>
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
