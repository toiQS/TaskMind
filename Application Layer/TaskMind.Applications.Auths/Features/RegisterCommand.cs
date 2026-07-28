using MediatR;
using System.Text.RegularExpressions;
using TaskMind.Applications.Auths.Common;
using TaskMind.Applications.Auths.Dtos;
using TaskMind.Applications.Auths.Interfaces;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Auths.Features
{
    /// <summary>Đăng ký tài khoản User công khai (mục 4.1). Vai trò khác chỉ được cấp khi được công ty/cơ sở đào tạo mời (mục 2.1).</summary>
    public class RegisterCommand : IRequest<RegisterResultDto>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResultDto>
    {
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private readonly IApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new InvalidOperationException("Vui lòng nhập họ và tên.");

            if (string.IsNullOrWhiteSpace(request.Email) || !EmailRegex.IsMatch(request.Email))
                throw new InvalidOperationException("Email không hợp lệ.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                throw new InvalidOperationException("Mật khẩu phải có ít nhất 8 ký tự.");

            if (request.Password != request.ConfirmPassword)
                throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

            var email = request.Email.Trim();

            if (await AccountLookup.ExistsByEmailAsync(_db, email, cancellationToken))
                throw new InvalidOperationException("Email đã được sử dụng.");

            // TODO: Profile.CreateProfile yêu cầu CitizenId 12 ký tự (mục 4.2) nhưng RegisterView hiện
            // chưa thu thập CCCD. Sinh placeholder tạm để thoả điều kiện Domain; cần bổ sung bước
            // xác minh CCCD thật (và cập nhật lại CitizenId) ở giai đoạn thiết kế UI tiếp theo.
            var citizenIdPlaceholder = Guid.NewGuid().ToString("N")[..12];

            var passwordHash = _passwordHasher.Hash(request.Password);

            var userResult = User.CreateUser(citizenIdPlaceholder, email, passwordHash);
            if (!userResult.IsSuccess)
                throw new InvalidOperationException(userResult.Message);

            var user = userResult.Data!;

            var (firstName, lastName) = SplitFullName(request.FullName.Trim());
            var profileResult = user.Profile.UpdatePersonalInfo(firstName, lastName, bio: null, imageUrl: null);
            if (!profileResult.IsSuccess)
                throw new InvalidOperationException(profileResult.Message);

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new RegisterResultDto
            {
                UserId = user.Id,
                FullName = request.FullName.Trim(),
                Email = email,
                Role = user.Role.ToString()
            };
        }

        /// <summary>"Họ và tên" kiểu VN: từ đầu tiên là Họ (LastName), phần còn lại là Tên đệm+Tên (FirstName).</summary>
        private static (string FirstName, string LastName) SplitFullName(string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return (fullName, string.Empty);
            if (parts.Length == 1) return (parts[0], string.Empty);

            var lastName = parts[0];
            var firstName = string.Join(' ', parts[1..]);
            return (firstName, lastName);
        }
    }
}