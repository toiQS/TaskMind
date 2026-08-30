using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public AccountRole Role { get; set; }
        public bool IsVerified { get; set; }
        public EntityStatus Status { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
    }

    public class GetUsersFilter
    {
        public EntityStatus? Status { get; set; }
        public string? Keyword { get; set; } // tìm theo Email/FullName
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}