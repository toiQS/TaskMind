using Microsoft.EntityFrameworkCore;
using MediatR;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.ObjectValues;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using Npgsql;

namespace TaskMind.ApplicationLayer.Features.Organizations;

public sealed record OrganizationAddressDto(string Street, string City, string Country);

public sealed record CompanyRegistrationDto(
    string CompanyName,
    string TaxCode,
    string Field,
    string Email,
    string Phone,
    OrganizationAddressDto? Address);

public sealed record SchoolRegistrationDto(
    string SchoolName,
    string Field,
    string Email,
    string Phone,
    OrganizationAddressDto? Address);

public sealed record OrganizationUpdateDto(
    string Name,
    string Field,
    string Email,
    string Phone,
    OrganizationAddressDto? Address);
public sealed record OrganizationReasonRequest(string? Reason);
public sealed record OrganizationMembershipRequest(string Package);

public sealed record OrganizationRegistrationItemDto(
    Guid Id,
    string Name,
    string Field,
    string? TaxCode,
    string Email,
    string Phone,
    OrganizationAddressDto Address,
    string Status,
    DateTime JoinDate,
    Guid RequestedByUserId);

public sealed record OrganizationRegistrationPageDto(
    IReadOnlyList<OrganizationRegistrationItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record PublicOrganizationDto(
    Guid Id,
    string Name,
    string Field,
    string Email,
    string Phone,
    OrganizationAddressDto Address,
    string MembershipPackage);

public sealed record RequestCompanyRegistrationCommand(CompanyRegistrationDto Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record RequestSchoolRegistrationCommand(SchoolRegistrationDto Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record ListCompanyRegistrationRequestsQuery(string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<OrganizationRegistrationPageDto>>;
public sealed record ListSchoolRegistrationRequestsQuery(string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<OrganizationRegistrationPageDto>>;
public sealed record ApproveCompanyRegistrationCommand(Guid CompanyId)
    : IRequest<ServiceResult>;
public sealed record RejectCompanyRegistrationCommand(Guid CompanyId, string? Reason)
    : IRequest<ServiceResult>;
public sealed record ApproveSchoolRegistrationCommand(Guid SchoolId)
    : IRequest<ServiceResult>;
public sealed record RejectSchoolRegistrationCommand(Guid SchoolId, string? Reason)
    : IRequest<ServiceResult>;
public sealed record GetCompanyQuery(Guid CompanyId) : IRequest<ServiceResult<PublicOrganizationDto>>;
public sealed record GetSchoolQuery(Guid SchoolId) : IRequest<ServiceResult<PublicOrganizationDto>>;
public sealed record UpdateCompanyCommand(Guid CompanyId, OrganizationUpdateDto Request)
    : IRequest<ServiceResult>;
public sealed record UpdateSchoolCommand(Guid SchoolId, OrganizationUpdateDto Request)
    : IRequest<ServiceResult>;
public sealed record SuspendCompanyCommand(Guid CompanyId) : IRequest<ServiceResult>;
public sealed record ReactivateCompanyCommand(Guid CompanyId) : IRequest<ServiceResult>;
public sealed record ChangeCompanyMembershipCommand(Guid CompanyId, string Package) : IRequest<ServiceResult>;
public sealed record SuspendSchoolCommand(Guid SchoolId) : IRequest<ServiceResult>;
public sealed record ReactivateSchoolCommand(Guid SchoolId) : IRequest<ServiceResult>;
public sealed record ChangeSchoolMembershipCommand(Guid SchoolId, string Package) : IRequest<ServiceResult>;

public sealed class RequestCompanyRegistrationHandler
    : IRequestHandler<RequestCompanyRegistrationCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public RequestCompanyRegistrationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(RequestCompanyRegistrationCommand command, CancellationToken ct)
    {
        var user = await OrganizationRules.GetCurrentUser(_db, _session, ct);
        if (user is null)
            return ServiceResult<Guid>.Forbidden("Chỉ người dùng đã xác minh mới có thể đăng ký công ty.");

        var email = command.Request.Email?.Trim().ToLowerInvariant();
        if (!OrganizationRules.IsValidEmail(email))
            return ServiceResult<Guid>.Failure("Email công ty không hợp lệ.");
        if (string.IsNullOrWhiteSpace(command.Request.Field) ||
            string.IsNullOrWhiteSpace(command.Request.Phone) ||
            !OrganizationRules.IsAddressValid(command.Request.Address))
            return ServiceResult<Guid>.Failure("Lĩnh vực, số điện thoại và địa chỉ công ty là bắt buộc.");
        var taxCode = command.Request.TaxCode?.Trim();
        if (taxCode is null || taxCode.Length is < 10 or > 13 || taxCode.Any(character => !char.IsDigit(character)))
            return ServiceResult<Guid>.Failure("Mã số thuế phải gồm 10 đến 13 chữ số.");

        var created = Company.Create(
            command.Request.CompanyName,
            taxCode,
            command.Request.Field,
            email!,
            command.Request.Phone,
            user.Id,
            OrganizationRules.ToAddress(command.Request.Address));
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        if (await _db.Companies.AnyAsync(item =>
                item.TaxCode == taxCode || item.Email.ToUpper() == email!.ToUpper(), ct))
            return ServiceResult<Guid>.Failure("Mã số thuế hoặc email công ty đã được đăng ký.");

        _db.Companies.Add(created.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (OrganizationRules.IsUniqueViolation(
                   exception, "IX_Companies_TaxCode", "IX_Companies_Email"))
        {
            return ServiceResult<Guid>.Failure("Mã số thuế hoặc email công ty đã được đăng ký.");
        }
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã gửi yêu cầu đăng ký công ty.");
    }
}

public sealed class RequestSchoolRegistrationHandler
    : IRequestHandler<RequestSchoolRegistrationCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public RequestSchoolRegistrationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(RequestSchoolRegistrationCommand command, CancellationToken ct)
    {
        var user = await OrganizationRules.GetCurrentUser(_db, _session, ct);
        if (user is null)
            return ServiceResult<Guid>.Forbidden("Chỉ người dùng đã xác minh mới có thể đăng ký cơ sở đào tạo.");

        var email = command.Request.Email?.Trim().ToLowerInvariant();
        if (!OrganizationRules.IsValidEmail(email))
            return ServiceResult<Guid>.Failure("Email cơ sở đào tạo không hợp lệ.");
        if (string.IsNullOrWhiteSpace(command.Request.Field) ||
            string.IsNullOrWhiteSpace(command.Request.Phone) ||
            !OrganizationRules.IsAddressValid(command.Request.Address))
            return ServiceResult<Guid>.Failure("Lĩnh vực, số điện thoại và địa chỉ cơ sở đào tạo là bắt buộc.");
        var created = School.Create(
            command.Request.SchoolName,
            command.Request.Field,
            email!,
            command.Request.Phone,
            user.Id,
            OrganizationRules.ToAddress(command.Request.Address));
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        if (await _db.Schools.AnyAsync(item => item.Email.ToUpper() == email!.ToUpper(), ct))
            return ServiceResult<Guid>.Failure("Email cơ sở đào tạo đã được đăng ký.");

        _db.Schools.Add(created.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (OrganizationRules.IsUniqueViolation(exception, "IX_Schools_Email"))
        {
            return ServiceResult<Guid>.Failure("Email cơ sở đào tạo đã được đăng ký.");
        }
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã gửi yêu cầu đăng ký cơ sở đào tạo.");
    }
}

public sealed class ListCompanyRegistrationRequestsHandler
    : IRequestHandler<ListCompanyRegistrationRequestsQuery, ServiceResult<OrganizationRegistrationPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ListCompanyRegistrationRequestsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<OrganizationRegistrationPageDto>> Handle(
        ListCompanyRegistrationRequestsQuery query, CancellationToken ct)
    {
        if (!await OrganizationRules.IsSystemAdmin(_db, _session, ct))
            return ServiceResult<OrganizationRegistrationPageDto>.Forbidden();
        if (!OrganizationRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<OrganizationRegistrationPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!OrganizationRules.TryStatus(query.Status, out var status))
            return ServiceResult<OrganizationRegistrationPageDto>.Failure("Status chỉ nhận Pending, Approved hoặc Rejected.");

        var companies = _db.Companies.AsNoTracking().AsQueryable();
        if (status == "Pending")
            companies = companies.Where(item => !item.IsVerified && item.Status == EntityStatus.Paused);
        else if (status == "Approved")
            companies = companies.Where(item => item.IsVerified);
        else if (status == "Rejected")
            companies = companies.Where(item => item.Status == EntityStatus.Deleted);
        var total = await companies.CountAsync(ct);
        var items = await companies.OrderByDescending(item => item.JoinDate).ThenBy(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .Select(item => new OrganizationRegistrationItemDto(
                item.Id, item.CompanyName, item.Field, item.TaxCode, item.Email, item.Phone,
                new OrganizationAddressDto(item.Address.Street, item.Address.City, item.Address.Country),
                item.IsVerified ? "Approved" : item.Status == EntityStatus.Deleted ? "Rejected" : "Pending",
                item.JoinDate, item.RequestedByUserId))
            .ToListAsync(ct);
        return ServiceResult<OrganizationRegistrationPageDto>.Success(
            new OrganizationRegistrationPageDto(items, query.Page, query.PageSize, total));
    }
}

public sealed class ListSchoolRegistrationRequestsHandler
    : IRequestHandler<ListSchoolRegistrationRequestsQuery, ServiceResult<OrganizationRegistrationPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ListSchoolRegistrationRequestsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<OrganizationRegistrationPageDto>> Handle(
        ListSchoolRegistrationRequestsQuery query, CancellationToken ct)
    {
        if (!await OrganizationRules.IsSystemAdmin(_db, _session, ct))
            return ServiceResult<OrganizationRegistrationPageDto>.Forbidden();
        if (!OrganizationRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<OrganizationRegistrationPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!OrganizationRules.TryStatus(query.Status, out var status))
            return ServiceResult<OrganizationRegistrationPageDto>.Failure("Status chỉ nhận Pending, Approved hoặc Rejected.");

        var schools = _db.Schools.AsNoTracking().AsQueryable();
        if (status == "Pending")
            schools = schools.Where(item => !item.IsVerified && item.Status == EntityStatus.Paused);
        else if (status == "Approved")
            schools = schools.Where(item => item.IsVerified);
        else if (status == "Rejected")
            schools = schools.Where(item => item.Status == EntityStatus.Deleted);
        var total = await schools.CountAsync(ct);
        var items = await schools.OrderByDescending(item => item.JoinDate).ThenBy(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .Select(item => new OrganizationRegistrationItemDto(
                item.Id, item.SchoolName, item.Field, null, item.Email, item.Phone,
                new OrganizationAddressDto(item.Address.Street, item.Address.City, item.Address.Country),
                item.IsVerified ? "Approved" : item.Status == EntityStatus.Deleted ? "Rejected" : "Pending",
                item.JoinDate, item.RequestedByUserId))
            .ToListAsync(ct);
        return ServiceResult<OrganizationRegistrationPageDto>.Success(
            new OrganizationRegistrationPageDto(items, query.Page, query.PageSize, total));
    }
}

public sealed class ApproveCompanyRegistrationHandler
    : IRequestHandler<ApproveCompanyRegistrationCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ApproveCompanyRegistrationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ApproveCompanyRegistrationCommand command, CancellationToken ct)
    {
        var actor = await OrganizationRules.GetSystemAdmin(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var company = await _db.Companies.SingleOrDefaultAsync(item => item.Id == command.CompanyId, ct);
        if (company is null) return ServiceResult.NotFound("Không tìm thấy công ty.");
        var owner = await _db.Users.SingleOrDefaultAsync(item => item.Id == company.RequestedByUserId, ct);
        if (owner is null) return ServiceResult.Failure("Không tìm thấy người dùng đã đăng ký công ty.");
        if (await _db.AdminCompanies.AnyAsync(item => item.CompanyId == company.Id, ct))
            return ServiceResult.Failure("Công ty hoặc người đăng ký đã có tài khoản quản trị.");

        var verified = company.Verify();
        if (!verified.IsSuccess) return ServiceResult.Failure(verified.Message);
        var adminResult = AdminCompany.CreateAdminCompany(
            owner.Profile.CitizenId, owner.Profile.Email, owner.Security.PasswordHash, company.Id, owner.Id);
        if (!adminResult.IsSuccess || adminResult.Data is null)
            return ServiceResult.Failure(adminResult.Message);
        var adminVerified = adminResult.Data.Verify();
        if (!adminVerified.IsSuccess) return ServiceResult.Failure(adminVerified.Message);
        var records = OrganizationRules.AddOrganizationRecord(
            _db, actor.Id, owner.Id, company.Id, "Company", "RegistrationApproved",
            "Công ty đã được duyệt", $"Yêu cầu đăng ký {company.CompanyName} đã được duyệt. Bạn có thể đăng nhập bằng vai trò quản trị tổ chức.",
            NotificationType.Approval);
        if (!records.IsSuccess) return records;

        _db.AdminCompanies.Add(adminResult.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã duyệt công ty và cấp tài khoản quản trị.");
    }
}

public sealed class ApproveSchoolRegistrationHandler
    : IRequestHandler<ApproveSchoolRegistrationCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ApproveSchoolRegistrationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ApproveSchoolRegistrationCommand command, CancellationToken ct)
    {
        var actor = await OrganizationRules.GetSystemAdmin(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var school = await _db.Schools.SingleOrDefaultAsync(item => item.Id == command.SchoolId, ct);
        if (school is null) return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");
        var owner = await _db.Users.SingleOrDefaultAsync(item => item.Id == school.RequestedByUserId, ct);
        if (owner is null) return ServiceResult.Failure("Không tìm thấy người dùng đã đăng ký cơ sở đào tạo.");
        if (await _db.AdminSchools.AnyAsync(item => item.SchoolId == school.Id, ct))
            return ServiceResult.Failure("Cơ sở đào tạo hoặc người đăng ký đã có tài khoản quản trị.");

        var verified = school.Verify();
        if (!verified.IsSuccess) return ServiceResult.Failure(verified.Message);
        var adminResult = AdminSchool.CreateAdminSchool(
            owner.Profile.CitizenId, owner.Profile.Email, owner.Security.PasswordHash, school.Id, owner.Id);
        if (!adminResult.IsSuccess || adminResult.Data is null)
            return ServiceResult.Failure(adminResult.Message);
        var adminVerified = adminResult.Data.Verify();
        if (!adminVerified.IsSuccess) return ServiceResult.Failure(adminVerified.Message);
        var records = OrganizationRules.AddOrganizationRecord(
            _db, actor.Id, owner.Id, school.Id, "School", "RegistrationApproved",
            "Cơ sở đào tạo đã được duyệt", $"Yêu cầu đăng ký {school.SchoolName} đã được duyệt. Bạn có thể đăng nhập bằng vai trò quản trị tổ chức.",
            NotificationType.Approval);
        if (!records.IsSuccess) return records;

        _db.AdminSchools.Add(adminResult.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã duyệt cơ sở đào tạo và cấp tài khoản quản trị.");
    }
}

public sealed class RejectCompanyRegistrationHandler
    : IRequestHandler<RejectCompanyRegistrationCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public RejectCompanyRegistrationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(RejectCompanyRegistrationCommand command, CancellationToken ct)
    {
        var actor = await OrganizationRules.GetSystemAdmin(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var company = await _db.Companies.SingleOrDefaultAsync(item => item.Id == command.CompanyId, ct);
        if (company is null) return ServiceResult.NotFound("Không tìm thấy công ty.");
        var rejected = company.RejectRegistration();
        if (!rejected.IsSuccess) return ServiceResult.Failure(rejected.Message);
        var message = $"Yêu cầu đăng ký {company.CompanyName} đã bị từ chối." +
                      (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Lý do: {command.Reason.Trim()}");
        var records = OrganizationRules.AddOrganizationRecord(
            _db, actor.Id, company.RequestedByUserId, company.Id, "Company", "RegistrationRejected",
            "Yêu cầu đăng ký công ty bị từ chối", message, NotificationType.Warning);
        if (!records.IsSuccess) return records;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã từ chối yêu cầu đăng ký công ty.");
    }
}

public sealed class RejectSchoolRegistrationHandler
    : IRequestHandler<RejectSchoolRegistrationCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public RejectSchoolRegistrationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(RejectSchoolRegistrationCommand command, CancellationToken ct)
    {
        var actor = await OrganizationRules.GetSystemAdmin(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var school = await _db.Schools.SingleOrDefaultAsync(item => item.Id == command.SchoolId, ct);
        if (school is null) return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");
        var rejected = school.RejectRegistration();
        if (!rejected.IsSuccess) return ServiceResult.Failure(rejected.Message);
        var message = $"Yêu cầu đăng ký {school.SchoolName} đã bị từ chối." +
                      (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Lý do: {command.Reason.Trim()}");
        var records = OrganizationRules.AddOrganizationRecord(
            _db, actor.Id, school.RequestedByUserId, school.Id, "School", "RegistrationRejected",
            "Yêu cầu đăng ký cơ sở đào tạo bị từ chối", message, NotificationType.Warning);
        if (!records.IsSuccess) return records;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã từ chối yêu cầu đăng ký cơ sở đào tạo.");
    }
}

public sealed class GetCompanyHandler : IRequestHandler<GetCompanyQuery, ServiceResult<PublicOrganizationDto>>
{
    private readonly IApplicationDbContext _db;
    public GetCompanyHandler(IApplicationDbContext db) => _db = db;

    public async Task<ServiceResult<PublicOrganizationDto>> Handle(GetCompanyQuery query, CancellationToken ct)
    {
        var item = await _db.Companies.AsNoTracking().Where(company =>
                company.Id == query.CompanyId && company.IsVerified && company.Status == EntityStatus.Active)
            .Select(company => new PublicOrganizationDto(
                company.Id, company.CompanyName, company.Field, company.Email, company.Phone,
                new OrganizationAddressDto(company.Address.Street, company.Address.City, company.Address.Country),
                company.MembershipPackage))
            .SingleOrDefaultAsync(ct);
        return item is null
            ? ServiceResult<PublicOrganizationDto>.NotFound("Không tìm thấy công ty.")
            : ServiceResult<PublicOrganizationDto>.Success(item);
    }
}

public sealed class GetSchoolHandler : IRequestHandler<GetSchoolQuery, ServiceResult<PublicOrganizationDto>>
{
    private readonly IApplicationDbContext _db;
    public GetSchoolHandler(IApplicationDbContext db) => _db = db;

    public async Task<ServiceResult<PublicOrganizationDto>> Handle(GetSchoolQuery query, CancellationToken ct)
    {
        var item = await _db.Schools.AsNoTracking().Where(school =>
                school.Id == query.SchoolId && school.IsVerified && school.Status == EntityStatus.Active)
            .Select(school => new PublicOrganizationDto(
                school.Id, school.SchoolName, school.Field, school.Email, school.Phone,
                new OrganizationAddressDto(school.Address.Street, school.Address.City, school.Address.Country),
                school.MembershipPackage))
            .SingleOrDefaultAsync(ct);
        return item is null
            ? ServiceResult<PublicOrganizationDto>.NotFound("Không tìm thấy cơ sở đào tạo.")
            : ServiceResult<PublicOrganizationDto>.Success(item);
    }
}

public sealed class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public UpdateCompanyHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(UpdateCompanyCommand command, CancellationToken ct)
    {
        var account = await OrganizationRules.GetCurrentAccount(_db, _session, ct);
        if (account is not AdminCompany admin ||
            admin.CompanyId != command.CompanyId ||
            !admin.IsVerified ||
            admin.Status != EntityStatus.Active)
            return ServiceResult.Forbidden("Chỉ quản trị viên của công ty đó mới có thể cập nhật thông tin.");
        var company = await _db.Companies.SingleOrDefaultAsync(item => item.Id == command.CompanyId, ct);
        if (company is null) return ServiceResult.NotFound("Không tìm thấy công ty.");
        var email = command.Request.Email?.Trim().ToLowerInvariant();
        if (!OrganizationRules.IsValidEmail(email))
            return ServiceResult.Failure("Email công ty không hợp lệ.");
        if (string.IsNullOrWhiteSpace(command.Request.Field) ||
            string.IsNullOrWhiteSpace(command.Request.Phone) ||
            !OrganizationRules.IsAddressValid(command.Request.Address))
            return ServiceResult.Failure("Lĩnh vực, số điện thoại và địa chỉ công ty là bắt buộc.");
        if (await _db.Companies.AnyAsync(item => item.Id != company.Id && item.Email.ToUpper() == email!.ToUpper(), ct))
            return ServiceResult.Failure("Email công ty đã được sử dụng.");
        var updated = company.UpdateDetails(command.Request.Name, command.Request.Field, email!,
            command.Request.Phone, OrganizationRules.ToAddress(command.Request.Address));
        if (!updated.IsSuccess) return ServiceResult.Failure(updated.Message);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (OrganizationRules.IsUniqueViolation(exception, "IX_Companies_Email"))
        {
            return ServiceResult.Failure("Email công ty đã được sử dụng.");
        }
        return ServiceResult.Success("Đã cập nhật thông tin công ty.");
    }
}

public sealed class UpdateSchoolHandler : IRequestHandler<UpdateSchoolCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public UpdateSchoolHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(UpdateSchoolCommand command, CancellationToken ct)
    {
        var account = await OrganizationRules.GetCurrentAccount(_db, _session, ct);
        if (account is not AdminSchool admin ||
            admin.SchoolId != command.SchoolId ||
            !admin.IsVerified ||
            admin.Status != EntityStatus.Active)
            return ServiceResult.Forbidden("Chỉ quản trị viên của cơ sở đào tạo đó mới có thể cập nhật thông tin.");
        var school = await _db.Schools.SingleOrDefaultAsync(item => item.Id == command.SchoolId, ct);
        if (school is null) return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");
        var email = command.Request.Email?.Trim().ToLowerInvariant();
        if (!OrganizationRules.IsValidEmail(email))
            return ServiceResult.Failure("Email cơ sở đào tạo không hợp lệ.");
        if (string.IsNullOrWhiteSpace(command.Request.Field) ||
            string.IsNullOrWhiteSpace(command.Request.Phone) ||
            !OrganizationRules.IsAddressValid(command.Request.Address))
            return ServiceResult.Failure("Lĩnh vực, số điện thoại và địa chỉ cơ sở đào tạo là bắt buộc.");
        if (await _db.Schools.AnyAsync(item => item.Id != school.Id && item.Email.ToUpper() == email!.ToUpper(), ct))
            return ServiceResult.Failure("Email cơ sở đào tạo đã được sử dụng.");
        var updated = school.UpdateDetails(command.Request.Name, command.Request.Field, email!,
            command.Request.Phone, OrganizationRules.ToAddress(command.Request.Address));
        if (!updated.IsSuccess) return ServiceResult.Failure(updated.Message);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (OrganizationRules.IsUniqueViolation(exception, "IX_Schools_Email"))
        {
            return ServiceResult.Failure("Email cơ sở đào tạo đã được sử dụng.");
        }
        return ServiceResult.Success("Đã cập nhật thông tin cơ sở đào tạo.");
    }
}

public sealed class SuspendCompanyHandler : IRequestHandler<SuspendCompanyCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public SuspendCompanyHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public Task<ServiceResult> Handle(SuspendCompanyCommand command, CancellationToken ct) =>
        OrganizationRules.ChangeStatus(_db, _session, command.CompanyId, true, true, ct);
}

public sealed class ReactivateCompanyHandler : IRequestHandler<ReactivateCompanyCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ReactivateCompanyHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public Task<ServiceResult> Handle(ReactivateCompanyCommand command, CancellationToken ct) =>
        OrganizationRules.ChangeStatus(_db, _session, command.CompanyId, false, true, ct);
}

public sealed class SuspendSchoolHandler : IRequestHandler<SuspendSchoolCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public SuspendSchoolHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public Task<ServiceResult> Handle(SuspendSchoolCommand command, CancellationToken ct) =>
        OrganizationRules.ChangeStatus(_db, _session, command.SchoolId, true, false, ct);
}

public sealed class ReactivateSchoolHandler : IRequestHandler<ReactivateSchoolCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ReactivateSchoolHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public Task<ServiceResult> Handle(ReactivateSchoolCommand command, CancellationToken ct) =>
        OrganizationRules.ChangeStatus(_db, _session, command.SchoolId, false, false, ct);
}

public sealed class ChangeCompanyMembershipHandler : IRequestHandler<ChangeCompanyMembershipCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ChangeCompanyMembershipHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public Task<ServiceResult> Handle(ChangeCompanyMembershipCommand command, CancellationToken ct) =>
        OrganizationRules.ChangeMembership(_db, _session, command.CompanyId, command.Package, true, ct);
}

public sealed class ChangeSchoolMembershipHandler : IRequestHandler<ChangeSchoolMembershipCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ChangeSchoolMembershipHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public Task<ServiceResult> Handle(ChangeSchoolMembershipCommand command, CancellationToken ct) =>
        OrganizationRules.ChangeMembership(_db, _session, command.SchoolId, command.Package, false, ct);
}

internal static class OrganizationRules
{
    public static async Task<Account?> GetCurrentAccount(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var id = session.GetUserId();
        if (!id.HasValue || id.Value == Guid.Empty) return null;
        return await db.Accounts.SingleOrDefaultAsync(item => item.Id == id.Value, ct);
    }

    public static async Task<User?> GetCurrentUser(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var account = await GetCurrentAccount(db, session, ct);
        return account is User { IsVerified: true } user && user.Status == EntityStatus.Active ? user : null;
    }

    public static async Task<Admin?> GetSystemAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var account = await GetCurrentAccount(db, session, ct);
        return account is Admin { IsVerified: true } admin && admin.Status == EntityStatus.Active ? admin : null;
    }

    public static async Task<bool> IsSystemAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct) =>
        await GetSystemAdmin(db, session, ct) is not null;

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        System.Net.Mail.MailAddress.TryCreate(email, out var parsed) &&
        string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase);

    public static bool IsAddressValid(OrganizationAddressDto? address) =>
        address is not null &&
        !string.IsNullOrWhiteSpace(address.Street) &&
        !string.IsNullOrWhiteSpace(address.City) &&
        !string.IsNullOrWhiteSpace(address.Country);

    public static bool IsUniqueViolation(DbUpdateException exception, params string[] constraints) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: not null
        } postgresException && constraints.Contains(postgresException.ConstraintName, StringComparer.Ordinal);

    public static Address ToAddress(OrganizationAddressDto? address) =>
        address is null ? new Address() : new Address(address.Street ?? "", address.City ?? "", address.Country ?? "");

    public static bool TryPaging(int page, int pageSize, out int skip)
    {
        skip = 0;
        if (page < 1 || pageSize is < 1 or > 100) return false;
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue) return false;
        skip = (int)offset;
        return true;
    }

    public static bool TryStatus(string? value, out string? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (string.Equals(value, "Pending", StringComparison.OrdinalIgnoreCase))
            status = "Pending";
        else if (string.Equals(value, "Approved", StringComparison.OrdinalIgnoreCase))
            status = "Approved";
        else if (string.Equals(value, "Rejected", StringComparison.OrdinalIgnoreCase))
            status = "Rejected";
        else
            return false;
        return true;
    }

    public static ServiceResult AddOrganizationRecord(
        IApplicationDbContext db,
        Guid actorId,
        Guid ownerId,
        Guid entityId,
        string entityType,
        string action,
        string title,
        string message,
        NotificationType notificationType)
    {
        var audit = AuditLog.Record(actorId, action, entityType, entityId);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        var notification = Notification.Create(
            ownerId,
            title,
            message,
            notificationType);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult.Error(notification.Message);
        db.AuditLogs.Add(audit.Data);
        db.Notifications.Add(notification.Data);
        return ServiceResult.Success();
    }

    public static async Task<ServiceResult> ChangeStatus(
        IApplicationDbContext db,
        ICurrentSessionProvider session,
        Guid entityId,
        bool suspend,
        bool isCompany,
        CancellationToken ct)
    {
        var actor = await GetSystemAdmin(db, session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var entityType = isCompany ? "Company" : "School";
        var status = suspend ? "tạm ngưng" : "kích hoạt lại";
        if (isCompany)
        {
            var entity = await db.Companies.SingleOrDefaultAsync(item => item.Id == entityId, ct);
            if (entity is null) return ServiceResult.NotFound("Không tìm thấy công ty.");
            var result = suspend ? entity.Suspend() : entity.Reactivate();
            if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
            var recipientId = await db.AdminCompanies.Where(item => item.CompanyId == entity.Id)
                .Select(item => item.Id).FirstOrDefaultAsync(ct);
            if (recipientId == Guid.Empty)
                return ServiceResult.Failure("Không tìm thấy tài khoản quản trị công ty.");
            var records = AddOrganizationRecord(db, actor.Id, recipientId, entity.Id,
                entityType, suspend ? "OrganizationSuspended" : "OrganizationReactivated",
                $"{entityType} đã được {(suspend ? "tạm ngưng" : "kích hoạt lại")}.",
                $"{entityType} {entity.CompanyName} đã được {(suspend ? "tạm ngưng" : "kích hoạt lại")} bởi quản trị viên.",
                suspend ? NotificationType.Warning : NotificationType.Success);
            if (!records.IsSuccess) return records;
        }
        else
        {
            var entity = await db.Schools.SingleOrDefaultAsync(item => item.Id == entityId, ct);
            if (entity is null) return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");
            var result = suspend ? entity.Suspend() : entity.Reactivate();
            if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
            var recipientId = await db.AdminSchools.Where(item => item.SchoolId == entity.Id)
                .Select(item => item.Id).FirstOrDefaultAsync(ct);
            if (recipientId == Guid.Empty)
                return ServiceResult.Failure("Không tìm thấy tài khoản quản trị cơ sở đào tạo.");
            var records = AddOrganizationRecord(db, actor.Id, recipientId, entity.Id,
                entityType, suspend ? "OrganizationSuspended" : "OrganizationReactivated",
                $"{entityType} đã được {(suspend ? "tạm ngưng" : "kích hoạt lại")}.",
                $"{entityType} {entity.SchoolName} đã được {(suspend ? "tạm ngưng" : "kích hoạt lại")} bởi quản trị viên.",
                suspend ? NotificationType.Warning : NotificationType.Success);
            if (!records.IsSuccess) return records;
        }
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success($"Đã {status} {entityType.ToLowerInvariant()}.");
    }

    public static async Task<ServiceResult> ChangeMembership(
        IApplicationDbContext db,
        ICurrentSessionProvider session,
        Guid entityId,
        string package,
        bool isCompany,
        CancellationToken ct)
    {
        var actor = await GetSystemAdmin(db, session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        if (string.IsNullOrWhiteSpace(package))
            return ServiceResult.Failure("Gói dịch vụ không được để trống.");
        var entityType = isCompany ? "Company" : "School";
        if (isCompany)
        {
            var entity = await db.Companies.SingleOrDefaultAsync(item => item.Id == entityId, ct);
            if (entity is null) return ServiceResult.NotFound("Không tìm thấy công ty.");
            var changed = entity.ChangeMembershipPackage(package);
            if (!changed.IsSuccess) return ServiceResult.Failure(changed.Message);
            var recipientId = await db.AdminCompanies.Where(item => item.CompanyId == entity.Id)
                .Select(item => item.Id).FirstOrDefaultAsync(ct);
            if (recipientId == Guid.Empty)
                return ServiceResult.Failure("Không tìm thấy tài khoản quản trị công ty.");
            var records = AddOrganizationRecord(db, actor.Id, recipientId, entity.Id,
                entityType, "MembershipPackageChanged", "Gói dịch vụ đã được cập nhật.",
                $"Gói dịch vụ của {entity.CompanyName} đã được cập nhật thành {package.Trim()}.",
                NotificationType.System);
            if (!records.IsSuccess) return records;
        }
        else
        {
            var entity = await db.Schools.SingleOrDefaultAsync(item => item.Id == entityId, ct);
            if (entity is null) return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo.");
            var changed = entity.ChangeMembershipPackage(package);
            if (!changed.IsSuccess) return ServiceResult.Failure(changed.Message);
            var recipientId = await db.AdminSchools.Where(item => item.SchoolId == entity.Id)
                .Select(item => item.Id).FirstOrDefaultAsync(ct);
            if (recipientId == Guid.Empty)
                return ServiceResult.Failure("Không tìm thấy tài khoản quản trị cơ sở đào tạo.");
            var records = AddOrganizationRecord(db, actor.Id, recipientId, entity.Id,
                entityType, "MembershipPackageChanged", "Gói dịch vụ đã được cập nhật.",
                $"Gói dịch vụ của {entity.SchoolName} đã được cập nhật thành {package.Trim()}.",
                NotificationType.System);
            if (!records.IsSuccess) return records;
        }
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success($"Đã cập nhật gói dịch vụ {entityType.ToLowerInvariant()}.");
    }
}
