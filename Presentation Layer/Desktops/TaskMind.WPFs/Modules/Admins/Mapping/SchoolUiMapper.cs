using TaskMind.Applications.Admins.Dtos;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Mapping
{
    public static class SchoolUiMapper
    {
        public static SchoolModel ToUi(SchoolDto dto) => new SchoolModel
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Field = dto.Field,
            Email = dto.Email,
            Phone = dto.Phone,
            Package = dto.Package,
            Status = Enum.TryParse<SchoolStatus>(dto.Status, true, out var status) ? status : SchoolStatus.Pending,
            JoinedDate = dto.JoinedDateUtc,
            TeacherCount = dto.TeacherCount,
            CourseCount = dto.CourseCount,
            StudentCount = dto.StudentCount
        };

        public static void ApplyDetail(SchoolModel model, SchoolDetailDto dto)
        {
            model.Address = dto.Address;
        }
    }
}