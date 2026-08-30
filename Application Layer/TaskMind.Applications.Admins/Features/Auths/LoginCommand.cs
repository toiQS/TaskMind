using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Auths
{
    public class LoginCommand(string email, string password) : ServiceResult<LoginResultDto>
    {
        public string Email { get; } = email;
        public string Password { get; } = password;
    }

    public class LoginHandler : ServiceResult<LoginResultDto>
    {
        private readonly IApplicationDbContext _dbContext;
        public LoginHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<LoginResultDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            // Validate the command
            if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
            {
                return ServiceResult<LoginResultDto>.Failure("Email and password are required.");
            }
            // Check if the user exists in the database
            var user = await _dbContext.Admins
                .Include(x => x.Profile)
                .Include(x => x.Security)
                .Select(x => new
                {
                    Id = x.Id,
                    Email = x.Profile.Email,
                    FirstName = x.Profile.FirstName,
                    Role = x.Role,
                    PasswordHash = x.Security.PasswordHash,
                    Status = (EntityStatus)x.Status
                })
                .Where(x => x.Role == AccountRole.Admin && x.Email == command.Email.Trim()).FirstOrDefaultAsync(cancellationToken);

            if(user == null)
            {
                return ServiceResult<LoginResultDto>.NotFound("User not found.");
            };

            // Không phân biệt rõ "sai email" hay "sai mật khẩu" trong thông báo lỗi (chống dò quét tài khoản).
            if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
                return ServiceResult<LoginResultDto>.Unauthorized("Email hoặc mật khẩu không chính xác.");

            if (user.Status == EntityStatus.Blocked)
                return ServiceResult<LoginResultDto>.Forbidden("Tài khoản đã bị cấm.");

            if (user.Status == EntityStatus.Paused)
                return ServiceResult<LoginResultDto>.Forbidden("Tài khoản đang bị tạm khoá.");


            
            var dto = new LoginResultDto
            {
                AccountId = user.Id,
                Email = user.Email,
                FullName = user.FirstName,
                Role = user.Role.ToString(),
                
            };
            return ServiceResult<LoginResultDto>.Success(dto, "Đăng nhập thành công");
        }
    }
}
