using System;

namespace TaskMind.WPFs.Modules.Auths.Models
{
    public enum UserRole
    {
        None,
        AdminCompany,
        AdminSchool,
        Staff,
        Teacher,
        User
    }

    public class RegisterModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        /// <summary>Đăng ký công khai luôn là User; các vai trò khác chỉ được gán khi công ty/cơ sở đào tạo mời (mục 4.1).</summary>
        public UserRole UserRole { get; set; } = UserRole.User;

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool AgreeTerms { get; set; }
    }
}