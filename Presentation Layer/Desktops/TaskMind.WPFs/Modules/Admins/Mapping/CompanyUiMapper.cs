using TaskMind.Applications.Admins.Dtos;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Mapping
{
    /// <summary>
    /// Chuyển đổi CompanyDto (Application layer) <-> CompanyModel (WPF Presentation model).
    /// Toàn bộ logic parse enum/Guid đặt tại đây, ViewModel không đụng tới Domain/DTO trực tiếp.
    /// </summary>
    public static class CompanyUiMapper
    {
        public static CompanyModel ToUi(CompanyDto dto) => new CompanyModel
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            TaxCode = dto.TaxCode,
            Field = dto.Field,
            Email = dto.Email,
            Phone = dto.Phone,
            Package = dto.Package,
            Status = Enum.TryParse<CompanyStatus>(dto.Status, true, out var status) ? status : CompanyStatus.Pending,
            JoinedDate = dto.JoinedDateUtc,
            StaffCount = dto.StaffCount,
            ProjectCount = dto.ProjectCount
        };

        public static void ApplyDetail(CompanyModel model, CompanyDetailDto dto)
        {
            model.Address = dto.Address;
        }
    }
}