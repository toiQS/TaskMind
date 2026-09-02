// RegisterCompanyCommand.cs
// [CẬP NHẬT - fix] Bổ sung AuditLog: đăng ký thành lập công ty là một sự kiện đáng ghi vết (mục 4.21),
// dùng chính RequestedByUserId làm ActorAccountId vì đây là hành động do User tự thực hiện, không phải
// Admin.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Companies
{
    public class RegisterCompanyCommand : IRequest<ServiceResult<Guid>>
    {
        public Guid RequestedByUserId { get; }
        public string CompanyName { get; }
        public string TaxCode { get; }
        public string Field { get; }
        public string Email { get; }
        public string Phone { get; }
        public Address? Address { get; }

        public RegisterCompanyCommand(Guid requestedByUserId, string companyName, string taxCode, string field, string email, string phone, Address? address = null)
        {
            RequestedByUserId = requestedByUserId;
            CompanyName = companyName;
            TaxCode = taxCode;
            Field = field;
            Email = email;
            Phone = phone;
            Address = address;
        }
    }

    public class RegisterCompanyHandler : IRequestHandler<RegisterCompanyCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public RegisterCompanyHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(RegisterCompanyCommand command, CancellationToken cancellationToken)
        {
            var requesterExists = await _dbContext.Users.AsNoTracking()
                .AnyAsync(u => u.Id == command.RequestedByUserId, cancellationToken);
            if (!requesterExists)
                return ServiceResult<Guid>.NotFound("Không tìm thấy tài khoản User đăng ký.");

            var taxCodeExists = await _dbContext.Companies.AsNoTracking()
                .AnyAsync(c => c.TaxCode == command.TaxCode.Trim(), cancellationToken);
            if (taxCodeExists)
                return ServiceResult<Guid>.Failure("Mã số thuế đã được đăng ký bởi công ty khác.");

            var emailExists = await _dbContext.Companies.AsNoTracking()
                .AnyAsync(c => c.Email == command.Email.Trim(), cancellationToken);
            if (emailExists)
                return ServiceResult<Guid>.Failure("Email đã được đăng ký bởi công ty khác.");

            var companyResult = Company.Create(
                command.CompanyName, command.TaxCode, command.Field, command.Email, command.Phone,
                command.RequestedByUserId, command.Address);

            if (!companyResult.IsSuccess)
                return ServiceResult<Guid>.Failure(companyResult.Message);

            _dbContext.Companies.Add(companyResult.Data!);

            var auditResult = AuditLog.Record(command.RequestedByUserId, "CompanyRegistered", nameof(Company), companyResult.Data!.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Success(companyResult.Data!.Id,
                "Đăng ký thành lập công ty thành công, đang chờ Admin hệ thống xác minh");
        }
    }
}
