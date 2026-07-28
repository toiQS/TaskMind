namespace TaskMind.Applications.Admins.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>AccountRole: User/Student/Staff/Teacher/Admin/AdminCompany/AdminSchool</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>EntityStatus: Active/Paused (Tạm khoá)/Blocked (Bị cấm)/Deleted</summary>
        public string Status { get; set; } = string.Empty;

        public bool IsVerified { get; set; }
        public DateTime JoinedDateUtc { get; set; }
    }

    public class UserDetailDto : UserDto
    {
        public string Bio { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}