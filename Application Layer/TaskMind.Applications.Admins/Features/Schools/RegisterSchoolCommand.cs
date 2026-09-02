// RegisterSchoolCommand.cs — [MỚI - fix] tương tự RegisterCompanyCommand, cho phía cơ sở đào tạo
// (mục 4.1.1, 4.8, 7.3.1).
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class RegisterSchoolCommand : IRequest<ServiceResult<Guid>>
    {
        public Guid RequestedByUserId { get; }
        public string SchoolName { get; }
        public string Field { get; }
        public string Email { get; }
        public string Phone { get; }
        public Address? Address { get; }

        public RegisterSchoolCommand(Guid requestedByUserId, string schoolName, string field, string email, string phone, Address? address = null)
        {
            RequestedByUserId = requestedByUserId;
            SchoolName = schoolName;
            Field = field;
            Email = email;
            Phone = phone;
            Address = address;
        }
    }

    public class RegisterSchoolHandler : IRequestHandler<RegisterSchoolCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public RegisterSchoolHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(RegisterSchoolCommand command, CancellationToken cancellationToken)
        {
            var requesterExists = await _dbContext.Users.AsNoTracking()
                .AnyAsync(u => u.Id == command.RequestedByUserId, cancellationToken);
            if (!requesterExists)
                return ServiceResult<Guid>.NotFound("Không tìm thấy tài khoản User đăng ký.");

            var emailExists = await _dbContext.Schools.AsNoTracking()
                .AnyAsync(s => s.Email == command.Email.Trim(), cancellationToken);
            if (emailExists)
                return ServiceResult<Guid>.Failure("Email đã được đăng ký bởi cơ sở đào tạo khác.");

            var schoolResult = Domain.Entities.School.Create(
                command.SchoolName, command.Field, command.Email, command.Phone,
                command.RequestedByUserId, command.Address);

            if (!schoolResult.IsSuccess)
                return ServiceResult<Guid>.Failure(schoolResult.Message);

            _dbContext.Schools.Add(schoolResult.Data!);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Success(schoolResult.Data!.Id,
                "Đăng ký thành lập cơ sở đào tạo thành công, đang chờ Admin hệ thống xác minh");
        }
    }
}
