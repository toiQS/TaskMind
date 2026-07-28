using TaskMind.Applications.Admins.Dtos;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Mapping
{
    public static class UserUiMapper
    {
        /// <summary>
        /// UserDto.Role là AccountRole đầy đủ (User/Student/Staff/Teacher/Admin/...), trong khi
        /// UserModel.Type (WPF) chỉ có Student/JobSeeker/OssContributor cho nhóm "người dùng cấp thấp"
        /// theo mục 4.1 tài liệu nghiệp vụ. Map các role còn lại về OssContributor làm mặc định an toàn
        /// cho tới khi UserType được mở rộng để khớp AccountRole đầy đủ.
        /// </summary>
        public static UserModel ToUi(UserDto dto) => new UserModel
        {
            Id = dto.Id.ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            AvatarUrl = dto.AvatarUrl,
            Type = MapType(dto.Role),
            Status = MapStatus(dto.Status),
            JoinedDate = dto.JoinedDateUtc,
            LastActiveDate = dto.JoinedDateUtc, // UserDto chưa có LastActiveDateUtc; UserDetailDto mới có
            SkillCount = dto.SkillCount,
            ProjectCount = dto.ProjectCount
        };

        public static void ApplyDetail(DetailUserModel model, UserDetailDto dto)
        {
            model.User.LastActiveDate = dto.LastActiveDateUtc ?? dto.JoinedDateUtc;

            model.Skills.Clear();
            foreach (var s in dto.Skills)
            {
                model.Skills.Add(new UserSkillItem
                {
                    SkillName = s.SkillName,
                    Category = Enum.TryParse<SkillCategory>(s.Category, true, out var cat) ? cat : SkillCategory.Other,
                    Level = Enum.TryParse<SkillLevel>(s.Level, true, out var lvl) ? lvl : SkillLevel.Beginner,
                    EndorsementCount = s.EndorsementCount
                });
            }

            model.ProjectHistory.Clear();
            foreach (var p in dto.ProjectHistory)
            {
                model.ProjectHistory.Add(new UserProjectHistoryItem
                {
                    ProjectName = p.ProjectName,
                    ProjectRole = p.ProjectRole,
                    ProjectSource = p.ProjectSource,
                    StartDate = p.StartDateUtc,
                    EndDate = p.EndDateUtc,
                    IsOngoing = p.IsOngoing
                });
            }

            model.AuditLogs.Clear();
            foreach (var a in dto.AuditLogs)
            {
                model.AuditLogs.Add(new AuditLogEntryModel
                {
                    Id = a.Id.ToString(),
                    EntityId = model.User.Id,
                    Action = a.Action,
                    Description = a.Description,
                    PerformedBy = a.PerformedBy,
                    Timestamp = a.TimestampUtc.UtcDateTime
                });
            }

            // Reviews/Reports (mục 5.2, 5.7 phần report vi phạm) chưa có Query tương ứng ở Application.Admins.
            // TODO: bổ sung GetUserReviewsQuery / GetUserReportsQuery khi Domain có Review/Report aggregate.
        }

        private static UserType MapType(string role) => role switch
        {
            "Student" => UserType.Student,
            _ => UserType.OssContributor
        };

        private static UserAccountStatus MapStatus(string status) => status switch
        {
            "Paused" => UserAccountStatus.Locked,
            "Blocked" => UserAccountStatus.Banned,
            _ => UserAccountStatus.Active
        };
    }
}