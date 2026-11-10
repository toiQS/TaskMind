using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Skills.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Skills;

public sealed record SkillProposalRequest(string Name, string Category);
public sealed record SkillDecisionRequest(string? Reason);
public sealed record RelatedSkillsRequest(IReadOnlyCollection<Guid> SkillIds);
public sealed record SkillLevelUpRequestBody(Guid SkillId, string RequestType, Guid? ApproverAccountId);
public sealed record SkillReflectionRequestBody(
    Guid SkillId,
    string Type,
    string? ProposedLevel,
    Guid? ProjectId,
    string EvidenceDescription,
    int? IncidentFrequency);
public sealed record SkillReflectionTestPaperRequest(Guid TestPaperId);
public sealed record SkillReflectionSubmissionRequest(Guid SubmissionId);

public sealed record SkillProposalItemDto(
    Guid Id, string Name, string Category, string SuggestedBy, string Status);

public sealed record SkillReflectionItemDto(
    Guid Id,
    Guid CompanyId,
    Guid UserId,
    Guid StaffAccountId,
    Guid SkillId,
    string ReflectionType,
    string? ProposedLevel,
    string? CurrentLevelAtRequest,
    Guid? ProjectId,
    string EvidenceDescription,
    int? IncidentFrequency,
    Guid ResponsibleAccountId,
    string Status,
    Guid? AssignedTestPaperId,
    Guid? VerificationSubmissionId,
    string? ResultLevel,
    string? RejectionReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ResolvedAtUtc);

public sealed record ProposeSkillCommand(SkillProposalRequest Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record ListSkillProposalsQuery(string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<PagedResultDto<SkillProposalItemDto>>>;
public sealed record DecideSkillProposalCommand(Guid SkillId, bool Approve, string? Reason)
    : IRequest<ServiceResult>;
public sealed record LinkRelatedSkillsCommand(Guid SkillId, IReadOnlyCollection<Guid> SkillIds)
    : IRequest<ServiceResult>;
public sealed record CreateSkillLevelUpRequestCommand(SkillLevelUpRequestBody Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record DecideSkillLevelUpRequestCommand(Guid RequestId, bool Approve, string? Reason)
    : IRequest<ServiceResult>;
public sealed record CreateSkillReflectionRequestCommand(
    Guid CompanyId, Guid StaffId, SkillReflectionRequestBody Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record ListSkillReflectionRequestsQuery(
    Guid CompanyId, string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<PagedResultDto<SkillReflectionItemDto>>>;
public sealed record GetSkillReflectionRequestQuery(Guid RequestId)
    : IRequest<ServiceResult<SkillReflectionItemDto>>;
public sealed record AssignReflectionTestPaperCommand(Guid RequestId, Guid TestPaperId)
    : IRequest<ServiceResult>;
public sealed record LinkReflectionAssessmentSubmissionCommand(Guid RequestId, Guid SubmissionId)
    : IRequest<ServiceResult>;
public sealed record ReviewSkillReflectionRequestCommand(Guid RequestId, bool Accept, string? Reason)
    : IRequest<ServiceResult>;

public sealed class ProposeSkillHandler :
    IRequestHandler<ProposeSkillCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ProposeSkillHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(ProposeSkillCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Request.Name))
            return ServiceResult<Guid>.Failure("Tên kỹ năng không được để trống.");
        if (!SkillWorkflowRules.TryCategory(command.Request.Category, out var category))
            return ServiceResult<Guid>.Failure("Danh mục kỹ năng không hợp lệ.");
        var actor = await SkillWorkflowRules.GetActiveAccount(_db, _session, ct);
        var source = actor switch
        {
            AdminCompany adminCompany => await _db.Companies
                .Where(item => item.Id == adminCompany.CompanyId && item.IsVerified && item.Status == EntityStatus.Active)
                .Select(item => item.CompanyName).SingleOrDefaultAsync(ct),
            AdminSchool adminSchool => await _db.Schools
                .Where(item => item.Id == adminSchool.SchoolId && item.IsVerified && item.Status == EntityStatus.Active)
                .Select(item => item.SchoolName).SingleOrDefaultAsync(ct),
            _ => null
        };
        if (actor is null || source is null)
            return ServiceResult<Guid>.Forbidden("Chỉ quản trị viên công ty hoặc cơ sở đào tạo đang hoạt động mới có thể đề xuất kỹ năng.");
        if (await _db.Skills.AnyAsync(item => item.NormalizedName == command.Request.Name.Trim().ToUpperInvariant(), ct))
            return ServiceResult<Guid>.Failure("Kỹ năng đã tồn tại hoặc đang chờ duyệt.");

        var created = Skill.Propose(command.Request.Name, category, source);
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        _db.Skills.Add(created.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Skills_NormalizedName"
            })
        {
            return ServiceResult<Guid>.Failure("Kỹ năng đã tồn tại hoặc đang chờ duyệt.");
        }
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã gửi đề xuất kỹ năng.");
    }
}

public sealed class ListSkillProposalsHandler :
    IRequestHandler<ListSkillProposalsQuery, ServiceResult<PagedResultDto<SkillProposalItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ListSkillProposalsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<PagedResultDto<SkillProposalItemDto>>> Handle(
        ListSkillProposalsQuery query, CancellationToken ct)
    {
        if (await SkillWorkflowRules.GetSystemAdmin(_db, _session, ct) is null)
            return ServiceResult<PagedResultDto<SkillProposalItemDto>>.Forbidden("Chỉ Admin hệ thống mới có quyền xem đề xuất.");
        if (!SkillWorkflowRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<PagedResultDto<SkillProposalItemDto>>.Failure("Page phải từ 1 trở lên và PageSize phải từ 1 đến 100.");

        var status = query.Status?.Trim();
        if (!string.IsNullOrEmpty(status) &&
            !status.Equals("Pending", StringComparison.OrdinalIgnoreCase) &&
            !status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
            !status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<PagedResultDto<SkillProposalItemDto>>.Failure("Trạng thái đề xuất không hợp lệ.");

        var skills = _db.Skills.AsNoTracking().Where(item => item.SuggestedBy != string.Empty);
        if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
            skills = skills.Where(item => !item.IsApproved && item.Status == EntityStatus.Active);
        else if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
            skills = skills.Where(item => item.IsApproved && item.Status == EntityStatus.Active);
        else if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
            skills = skills.Where(item => !item.IsApproved && item.Status == EntityStatus.Deleted);

        var total = await skills.CountAsync(ct);
        var items = await skills.OrderBy(item => item.CreateAt).ThenBy(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .Select(item => new SkillProposalItemDto(
                item.Id, item.SkillName, item.Category.ToString(), item.SuggestedBy,
                item.IsApproved ? "Approved" : item.Status == EntityStatus.Deleted ? "Rejected" : "Pending"))
            .ToArrayAsync(ct);
        return ServiceResult<PagedResultDto<SkillProposalItemDto>>.Success(
            new PagedResultDto<SkillProposalItemDto>
            {
                Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total
            });
    }
}

public sealed class DecideSkillProposalHandler :
    IRequestHandler<DecideSkillProposalCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public DecideSkillProposalHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(DecideSkillProposalCommand command, CancellationToken ct)
    {
        if (await SkillWorkflowRules.GetSystemAdmin(_db, _session, ct) is null)
            return ServiceResult.Forbidden("Chỉ Admin hệ thống mới có quyền duyệt đề xuất.");
        var skill = await _db.Skills.SingleOrDefaultAsync(item => item.Id == command.SkillId, ct);
        if (skill is null) return ServiceResult.NotFound("Không tìm thấy đề xuất kỹ năng.");
        var result = command.Approve ? skill.Approve() : skill.Reject();
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success(command.Approve ? "Đã duyệt kỹ năng." : "Đã từ chối đề xuất kỹ năng.");
    }
}

public sealed class LinkRelatedSkillsHandler :
    IRequestHandler<LinkRelatedSkillsCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public LinkRelatedSkillsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(LinkRelatedSkillsCommand command, CancellationToken ct)
    {
        if (await SkillWorkflowRules.GetSystemAdmin(_db, _session, ct) is null)
            return ServiceResult.Forbidden("Chỉ Admin hệ thống mới có quyền liên kết kỹ năng.");
        if (command.SkillIds is null || command.SkillIds.Count == 0 ||
            command.SkillIds.Any(id => id == Guid.Empty) ||
            command.SkillIds.Distinct().Count() != command.SkillIds.Count ||
            command.SkillIds.Contains(command.SkillId))
            return ServiceResult.Failure("Danh sách kỹ năng liên quan không hợp lệ.");
        var skills = await _db.Skills.Where(item =>
            item.IsApproved && item.Status == EntityStatus.Active &&
            (item.Id == command.SkillId || command.SkillIds.Contains(item.Id))).ToListAsync(ct);
        if (skills.Count != command.SkillIds.Count + 1)
            return ServiceResult.NotFound("Không tìm thấy kỹ năng nguồn hoặc một/một số kỹ năng liên quan đã duyệt.");
        var source = skills.Single(item => item.Id == command.SkillId);
        foreach (var id in command.SkillIds)
        {
            var result = source.LinkRelatedSkill(id);
            if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã liên kết các kỹ năng liên quan.");
    }
}

public sealed class CreateSkillLevelUpRequestHandler :
    IRequestHandler<CreateSkillLevelUpRequestCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public CreateSkillLevelUpRequestHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateSkillLevelUpRequestCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<SkillLevelUpMethod>(command.Request.RequestType?.Trim(), true, out var type) ||
            !Enum.IsDefined(type) ||
            !string.Equals(Enum.GetName(type), command.Request.RequestType?.Trim(), StringComparison.OrdinalIgnoreCase))
            return ServiceResult<Guid>.Failure("Hình thức nâng cấp không hợp lệ.");
        var accountResult = await SkillAccountResolver.ResolveAsync(_db, _session, ct);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<Guid>.Forbidden(accountResult.Message);
        var skill = await _db.Skills.AnyAsync(item =>
            item.Id == command.Request.SkillId && item.IsApproved && item.Status == EntityStatus.Active, ct);
        if (!skill) return ServiceResult<Guid>.NotFound("Không tìm thấy kỹ năng đã được duyệt.");

        var approverId = type == SkillLevelUpMethod.Endorsement
            ? command.Request.ApproverAccountId.GetValueOrDefault()
            : Guid.Empty;
        if (type == SkillLevelUpMethod.Endorsement)
        {
            var approver = await _db.Accounts.SingleOrDefaultAsync(item =>
                item.Id == approverId && item.IsVerified && item.Status == EntityStatus.Active, ct);
            if (approver is null || approver.Id == accountResult.Data.AccountId ||
                !await SkillWorkflowRules.CanEndorse(
                    _db, approver, accountResult.Data.AccountId, accountResult.Data.UserId, ct))
                return ServiceResult<Guid>.Failure("Người bảo lãnh không hợp lệ hoặc không đủ thẩm quyền.");
        }

        var profile = await _db.SkillProfiles.Include(item => item.Records)
            .SingleOrDefaultAsync(item => item.UserId == accountResult.Data.UserId, ct);
        if (profile is null) return ServiceResult<Guid>.NotFound("Bạn chưa có hồ sơ kỹ năng.");
        var requested = profile.RequestLevelUp(command.Request.SkillId, approverId, type);
        if (!requested.IsSuccess || requested.Data is null)
            return ServiceResult<Guid>.Failure(requested.Message);
        _db.SkillLevelUpRequests.Add(requested.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(requested.Data.Id, "Đã gửi yêu cầu nâng cấp kỹ năng.");
    }
}

public sealed class DecideSkillLevelUpRequestHandler :
    IRequestHandler<DecideSkillLevelUpRequestCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public DecideSkillLevelUpRequestHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(DecideSkillLevelUpRequestCommand command, CancellationToken ct)
    {
        var actor = await SkillWorkflowRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden("Không xác định được tài khoản đang hoạt động.");
        var request = await _db.SkillLevelUpRequests.SingleOrDefaultAsync(item => item.Id == command.RequestId, ct);
        if (request is null) return ServiceResult.NotFound("Không tìm thấy yêu cầu nâng kỹ năng.");
        var result = command.Approve
            ? request.Approve(actor.Id)
            : request.Reject(actor.Id, command.Reason ?? string.Empty);
        if (!result.IsSuccess) return ServiceResult.Forbidden(result.Message);

        if (command.Approve)
        {
            var profile = await _db.SkillProfiles.Include(item => item.Records)
                .SingleOrDefaultAsync(item => item.UserId == request.UserId, ct);
            if (profile is null) return ServiceResult.NotFound("Không tìm thấy hồ sơ kỹ năng.");
            var applied = profile.ApplyLevelUp(request);
            if (!applied.IsSuccess) return ServiceResult.Failure(applied.Message);
            var history = SkillHistoryEntry.Record(
                request.UserId, request.SkillId, SkillChangeSource.UserInitiated, actor.Id,
                SkillHistoryOutcome.Applied, request.CurrentLevel,
                (SkillLevel)((int)request.CurrentLevel + 1), relatedRequestId: request.Id);
            if (!history.IsSuccess || history.Data is null)
                return ServiceResult.Failure(history.Message);
            _db.SkillHistoryEntries.Add(history.Data);
        }
        else
        {
            var history = SkillHistoryEntry.Record(
                request.UserId, request.SkillId, SkillChangeSource.UserInitiated, actor.Id,
                SkillHistoryOutcome.Rejected, request.CurrentLevel,
                relatedRequestId: request.Id, rejectionReason: request.RejectionReason);
            if (!history.IsSuccess || history.Data is null)
                return ServiceResult.Failure(history.Message);
            _db.SkillHistoryEntries.Add(history.Data);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success(command.Approve ? "Đã duyệt và áp dụng yêu cầu nâng kỹ năng." : "Đã từ chối yêu cầu nâng kỹ năng.");
    }
}

public sealed class CreateSkillReflectionRequestHandler :
    IRequestHandler<CreateSkillReflectionRequestCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public CreateSkillReflectionRequestHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateSkillReflectionRequestCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<SkillReflectionType>(command.Request.Type?.Trim(), true, out var type) ||
            !Enum.IsDefined(type) ||
            !string.Equals(Enum.GetName(type), command.Request.Type?.Trim(), StringComparison.OrdinalIgnoreCase))
            return ServiceResult<Guid>.Failure("Loại phản ánh kỹ năng không hợp lệ.");
        var actor = await SkillWorkflowRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<Guid>.Forbidden("Không xác định được tài khoản đang hoạt động.");
        var company = await _db.Companies.AnyAsync(item =>
            item.Id == command.CompanyId && item.IsVerified && item.Status == EntityStatus.Active, ct);
        if (!company) return ServiceResult<Guid>.NotFound("Không tìm thấy công ty đang hoạt động.");
        var staff = await _db.Staffs.SingleOrDefaultAsync(item =>
            item.Id == command.StaffId && item.CompanyId == command.CompanyId &&
            item.IsActive && item.IsVerified && item.Status == EntityStatus.Active, ct);
        if (staff is null) return ServiceResult<Guid>.NotFound("Không tìm thấy nhân sự đang hoạt động của công ty.");
        if (command.Request.ProjectId.HasValue &&
            !await _db.Projects.AnyAsync(item =>
                item.Id == command.Request.ProjectId &&
                item.SourceType == ProjectSourceType.Company &&
                item.OwningEntityId == command.CompanyId, ct))
            return ServiceResult<Guid>.Failure("Dự án tham chiếu không thuộc công ty.");
        if (!await SkillWorkflowRules.CanManageCompanyReflection(
                _db, actor, command.CompanyId, command.Request.ProjectId, ct))
            return ServiceResult<Guid>.Forbidden("Chỉ Admin công ty hoặc quản lý dự án liên quan mới có thể đề xuất phản ánh kỹ năng.");
        var skill = await _db.Skills.AnyAsync(item =>
            item.Id == command.Request.SkillId && item.IsApproved && item.Status == EntityStatus.Active, ct);
        if (!skill) return ServiceResult<Guid>.NotFound("Không tìm thấy kỹ năng đã được duyệt.");

        var profile = await _db.SkillProfiles.Include(item => item.Records)
            .SingleOrDefaultAsync(item => item.UserId == staff.LinkedUserId, ct);
        var existing = profile?.Records.SingleOrDefault(item => item.SkillId == command.Request.SkillId);
        if (type == SkillReflectionType.Add && existing is not null)
            return ServiceResult<Guid>.Failure("Kỹ năng đã tồn tại trong hồ sơ.");
        if (type is SkillReflectionType.Up or SkillReflectionType.Down && existing is null)
            return ServiceResult<Guid>.Failure("Không tìm thấy kỹ năng trong hồ sơ để phản ánh.");
        if (!SkillWorkflowRules.TryOptionalLevel(command.Request.ProposedLevel, out var proposedLevel))
            return ServiceResult<Guid>.Failure("Cấp độ kỹ năng đề xuất không hợp lệ.");

        var created = type switch
        {
            SkillReflectionType.Up when existing is not null && proposedLevel.HasValue =>
                CompanySkillReflectionRequest.CreateUp(
                    command.CompanyId, staff.LinkedUserId, staff.Id, command.Request.SkillId, actor.Id,
                    command.Request.ProjectId, command.Request.EvidenceDescription, existing.Level, proposedLevel.Value),
            SkillReflectionType.Down when existing is not null =>
                CompanySkillReflectionRequest.CreateDown(
                    command.CompanyId, staff.LinkedUserId, staff.Id, command.Request.SkillId, actor.Id,
                    command.Request.ProjectId, command.Request.EvidenceDescription, existing.Level,
                    command.Request.IncidentFrequency),
            SkillReflectionType.Add when proposedLevel.HasValue =>
                CompanySkillReflectionRequest.CreateAdd(
                    command.CompanyId, staff.LinkedUserId, staff.Id, command.Request.SkillId, actor.Id,
                    command.Request.ProjectId, command.Request.EvidenceDescription, proposedLevel.Value),
            _ => TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<CompanySkillReflectionRequest>
                .Failure("Cấp độ đề xuất là bắt buộc với loại Up/Add.")
        };
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        _db.CompanySkillReflectionRequests.Add(created.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_CompanySkillReflectionRequests_CompanyId_StaffAccountId_SkillId"
            })
        {
            return ServiceResult<Guid>.Failure("Đã tồn tại yêu cầu phản ánh đang xử lý cho kỹ năng này.");
        }
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã gửi đề xuất phản ánh kỹ năng.");
    }
}

public sealed class ListSkillReflectionRequestsHandler :
    IRequestHandler<ListSkillReflectionRequestsQuery, ServiceResult<PagedResultDto<SkillReflectionItemDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ListSkillReflectionRequestsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<PagedResultDto<SkillReflectionItemDto>>> Handle(
        ListSkillReflectionRequestsQuery query, CancellationToken ct)
    {
        if (!SkillWorkflowRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<PagedResultDto<SkillReflectionItemDto>>.Failure("Page phải từ 1 trở lên và PageSize phải từ 1 đến 100.");
        if (!SkillWorkflowRules.TryReflectionStatus(query.Status, out var status))
            return ServiceResult<PagedResultDto<SkillReflectionItemDto>>.Failure("Trạng thái phản ánh không hợp lệ.");
        var actor = await SkillWorkflowRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<PagedResultDto<SkillReflectionItemDto>>.Forbidden();
        var managedProjectIds = await SkillWorkflowRules.GetManagedReflectionProjectIds(
            _db, actor, query.CompanyId, ct);
        var isCompanyAdmin = actor is AdminCompany adminCompany &&
                             adminCompany.CompanyId == query.CompanyId &&
                             await _db.Companies.AnyAsync(item => item.Id == query.CompanyId &&
                                 item.IsVerified && item.Status == EntityStatus.Active, ct);
        var isSystemAdmin = await SkillWorkflowRules.GetSystemAdmin(_db, _session, ct) is not null;
        if (!isCompanyAdmin && !isSystemAdmin && managedProjectIds.Length == 0)
            return ServiceResult<PagedResultDto<SkillReflectionItemDto>>.Forbidden("Không có quyền xem yêu cầu phản ánh của công ty.");
        var requests = _db.CompanySkillReflectionRequests.AsNoTracking()
            .Where(item => item.CompanyId == query.CompanyId);
        if (!isCompanyAdmin && !isSystemAdmin)
            requests = requests.Where(item => item.ProjectId.HasValue &&
                                               managedProjectIds.Contains(item.ProjectId.Value));
        if (status.HasValue) requests = requests.Where(item => item.RequestStatus == status.Value);
        var total = await requests.CountAsync(ct);
        var pageItems = await requests.OrderByDescending(item => item.CreatedAtUtc).ThenByDescending(item => item.Id)
            .Skip(skip).Take(query.PageSize).ToArrayAsync(ct);
        var items = pageItems.Select(SkillWorkflowRules.ToDto).ToArray();
        return ServiceResult<PagedResultDto<SkillReflectionItemDto>>.Success(
            new PagedResultDto<SkillReflectionItemDto>
            {
                Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total
            });
    }
}

public sealed class GetSkillReflectionRequestHandler :
    IRequestHandler<GetSkillReflectionRequestQuery, ServiceResult<SkillReflectionItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public GetSkillReflectionRequestHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<SkillReflectionItemDto>> Handle(GetSkillReflectionRequestQuery query, CancellationToken ct)
    {
        var request = await _db.CompanySkillReflectionRequests.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.RequestId, ct);
        if (request is null) return ServiceResult<SkillReflectionItemDto>.NotFound("Không tìm thấy yêu cầu phản ánh kỹ năng.");
        var actor = await SkillWorkflowRules.GetActiveAccount(_db, _session, ct);
        var isSystemAdmin = actor is not null &&
                            await SkillWorkflowRules.GetSystemAdmin(_db, _session, ct) is not null;
        if (actor is null || (!isSystemAdmin &&
                              !await SkillWorkflowRules.CanViewReflection(_db, actor, request, ct)))
            return ServiceResult<SkillReflectionItemDto>.Forbidden("Không có quyền xem yêu cầu phản ánh này.");
        return ServiceResult<SkillReflectionItemDto>.Success(SkillWorkflowRules.ToDto(request));
    }
}

public sealed class AssignReflectionTestPaperHandler :
    IRequestHandler<AssignReflectionTestPaperCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public AssignReflectionTestPaperHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(AssignReflectionTestPaperCommand command, CancellationToken ct)
    {
        var actor = await SkillWorkflowRules.GetActiveAccount(_db, _session, ct);
        var request = await _db.CompanySkillReflectionRequests.SingleOrDefaultAsync(item => item.Id == command.RequestId, ct);
        if (request is null) return ServiceResult.NotFound("Không tìm thấy yêu cầu phản ánh kỹ năng.");
        if (actor is null || !await SkillWorkflowRules.CanManageCompanyReflection(
                _db, actor, request.CompanyId, request.ProjectId, ct))
            return ServiceResult.Forbidden("Không có quyền gán bài kiểm tra cho yêu cầu này.");
        var paper = await _db.TestPapers.SingleOrDefaultAsync(item =>
            item.Id == command.TestPaperId && item.OwnerType == TestOwnerType.Company &&
            item.OwnerId == request.CompanyId, ct);
        if (paper is null) return ServiceResult.NotFound("Không tìm thấy bài kiểm tra thuộc công ty.");
        var result = request.AssignTestPaper(paper);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã gán bài kiểm tra xác minh.");
    }
}

public sealed class LinkReflectionAssessmentSubmissionHandler :
    IRequestHandler<LinkReflectionAssessmentSubmissionCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public LinkReflectionAssessmentSubmissionHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(LinkReflectionAssessmentSubmissionCommand command, CancellationToken ct)
    {
        var account = await SkillAccountResolver.ResolveAsync(_db, _session, ct);
        if (!account.IsSuccess || account.Data is null) return ServiceResult.Forbidden(account.Message);
        var request = await _db.CompanySkillReflectionRequests.SingleOrDefaultAsync(
            item => item.Id == command.RequestId && item.UserId == account.Data.UserId, ct);
        if (request is null) return ServiceResult.NotFound("Không tìm thấy yêu cầu phản ánh của bạn.");
        if (await _db.SkillLevelUpRequests.AnyAsync(item => item.SubmissionId == command.SubmissionId, ct) ||
            await _db.CompanySkillReflectionRequests.AnyAsync(item =>
                item.VerificationSubmissionId == command.SubmissionId, ct))
            return ServiceResult.Failure("Bài nộp đã được liên kết với một yêu cầu kỹ năng khác.");
        var submission = await _db.Submissions.SingleOrDefaultAsync(item => item.Id == command.SubmissionId, ct);
        if (submission is null) return ServiceResult.NotFound("Không tìm thấy bài nộp.");
        var result = request.LinkVerificationSubmission(submission);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã liên kết bài nộp với yêu cầu phản ánh.");
    }
}

public sealed class ReviewSkillReflectionRequestHandler :
    IRequestHandler<ReviewSkillReflectionRequestCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ReviewSkillReflectionRequestHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ReviewSkillReflectionRequestCommand command, CancellationToken ct)
    {
        var admin = await SkillWorkflowRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden("Chỉ Admin hệ thống mới có quyền xét duyệt yêu cầu Down.");
        var request = await _db.CompanySkillReflectionRequests.SingleOrDefaultAsync(item => item.Id == command.RequestId, ct);
        if (request is null) return ServiceResult.NotFound("Không tìm thấy yêu cầu phản ánh kỹ năng.");
        var result = command.Accept
            ? request.AdminAccept(admin.Id)
            : request.AdminDismiss(command.Reason ?? string.Empty);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success(command.Accept
            ? "Đã chấp nhận xử lý yêu cầu; nhân sự cần hoàn thành xác minh."
            : "Đã từ chối yêu cầu phản ánh.");
    }
}

internal static class SkillWorkflowRules
{
    public static async Task<Account?> GetActiveAccount(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var id = session.GetUserId();
        if (!id.HasValue || id.Value == Guid.Empty) return null;
        var account = await db.Accounts.SingleOrDefaultAsync(item => item.Id == id.Value, ct);
        return account is { IsVerified: true, Status: EntityStatus.Active } ? account : null;
    }

    public static async Task<Admin?> GetSystemAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct) =>
        await GetActiveAccount(db, session, ct) as Admin;

    public static bool TryCategory(string? value, out SkillCategory category) =>
        Enum.TryParse(value?.Trim(), true, out category) &&
        Enum.IsDefined(category) &&
        string.Equals(Enum.GetName(category), value?.Trim(), StringComparison.OrdinalIgnoreCase);

    public static bool TryOptionalLevel(string? value, out SkillLevel? level)
    {
        level = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!Enum.TryParse<SkillLevel>(value.Trim(), true, out var parsed) ||
            !Enum.IsDefined(parsed) ||
            !string.Equals(Enum.GetName(parsed), value.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;
        level = parsed;
        return true;
    }

    public static bool TryPaging(int page, int pageSize, out int skip)
    {
        skip = 0;
        if (page < 1 || pageSize is < 1 or > 100) return false;
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue) return false;
        skip = (int)offset;
        return true;
    }

    public static bool TryReflectionStatus(string? value, out SkillReflectionStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!Enum.TryParse<SkillReflectionStatus>(value.Trim(), true, out var parsed) ||
            !Enum.IsDefined(parsed) ||
            !string.Equals(Enum.GetName(parsed), value.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;
        status = parsed;
        return true;
    }

    public static async Task<bool> CanEndorse(
        IApplicationDbContext db,
        Account account,
        Guid requesterAccountId,
        Guid requesterUserId,
        CancellationToken ct)
    {
        if (account is AdminCompany adminCompany)
            return await db.Companies.AnyAsync(item => item.Id == adminCompany.CompanyId &&
                    item.IsVerified && item.Status == EntityStatus.Active, ct) &&
                   await db.Staffs.AnyAsync(item => item.LinkedUserId == requesterUserId &&
                       item.CompanyId == adminCompany.CompanyId && item.IsActive, ct);
        if (account is AdminSchool adminSchool)
            return await db.Schools.AnyAsync(item => item.Id == adminSchool.SchoolId &&
                    item.IsVerified && item.Status == EntityStatus.Active, ct) &&
                   (await db.Students.AnyAsync(item => item.LinkedUserId == requesterUserId &&
                        item.SchoolId == adminSchool.SchoolId && item.IsActive, ct) ||
                    await db.Teachers.AnyAsync(item => item.LinkedUserId == requesterUserId &&
                        item.SchoolId == adminSchool.SchoolId && item.IsActive, ct));
        var requesterAccountIds = await db.Accounts.Where(item => item.Id == requesterAccountId)
            .Select(item => item.Id).ToArrayAsync(ct);
        var requesterStaffIds = await db.Staffs.Where(item =>
                item.LinkedUserId == requesterUserId && item.IsActive)
            .Select(item => item.Id).ToArrayAsync(ct);
        var memberIds = requesterAccountIds.Concat(requesterStaffIds).ToArray();
        return await db.ProjectMembers.AnyAsync(manager =>
            manager.AccountId == account.Id && manager.LeftAt == null &&
            (manager.Role == ProjectRole.Owner || manager.Role == ProjectRole.ProjectManager ||
             manager.Role == ProjectRole.TechnicalLeader) &&
            db.ProjectMembers.Any(member =>
                EF.Property<Guid>(member, "ProjectId") == EF.Property<Guid>(manager, "ProjectId") &&
                member.AccountId != account.Id && memberIds.Contains(member.AccountId) &&
                member.LeftAt == null), ct);
    }

    public static async Task<bool> CanManageCompanyReflection(
        IApplicationDbContext db, Account actor, Guid companyId, Guid? projectId, CancellationToken ct)
    {
        if (actor is AdminCompany adminCompany && adminCompany.CompanyId == companyId)
            return await db.Companies.AnyAsync(item => item.Id == companyId &&
                item.IsVerified && item.Status == EntityStatus.Active, ct);
        if (!projectId.HasValue) return false;
        var project = await db.Projects.SingleOrDefaultAsync(item =>
            item.Id == projectId && item.SourceType == ProjectSourceType.Company &&
            item.OwningEntityId == companyId, ct);
        if (project is null) return false;
        return await db.ProjectMembers.AnyAsync(item =>
            EF.Property<Guid>(item, "ProjectId") == project.Id &&
            item.AccountId == actor.Id && item.LeftAt == null &&
            (item.Role == ProjectRole.Owner || item.Role == ProjectRole.ProjectManager), ct);
    }

    public static async Task<Guid[]> GetManagedReflectionProjectIds(
        IApplicationDbContext db, Account actor, Guid companyId, CancellationToken ct) =>
        await db.Projects.Where(project =>
                project.SourceType == ProjectSourceType.Company &&
                project.OwningEntityId == companyId &&
                db.ProjectMembers.Any(member =>
                    EF.Property<Guid>(member, "ProjectId") == project.Id &&
                    member.AccountId == actor.Id && member.LeftAt == null &&
                    (member.Role == ProjectRole.Owner || member.Role == ProjectRole.ProjectManager)))
            .Select(project => project.Id)
            .ToArrayAsync(ct);

    public static async Task<bool> CanViewReflection(
        IApplicationDbContext db, Account actor, CompanySkillReflectionRequest request, CancellationToken ct)
    {
        if (actor is AdminCompany adminCompany && adminCompany.CompanyId == request.CompanyId)
            return await db.Companies.AnyAsync(item => item.Id == request.CompanyId &&
                item.IsVerified && item.Status == EntityStatus.Active, ct);
        var userId = actor switch
        {
            User user => user.Id,
            Staff staff when staff.IsActive => staff.LinkedUserId,
            Student student when student.IsActive => student.LinkedUserId,
            Teacher teacher when teacher.IsActive => teacher.LinkedUserId,
            AdminCompany companyAdmin => companyAdmin.LinkedUserId,
            AdminSchool schoolAdmin => schoolAdmin.LinkedUserId,
            _ => Guid.Empty
        };
        if (userId == request.UserId) return true;
        return request.ProjectId.HasValue &&
               await CanManageCompanyReflection(db, actor, request.CompanyId, request.ProjectId, ct);
    }

    public static SkillReflectionItemDto ToDto(CompanySkillReflectionRequest item) => new(
        item.Id, item.CompanyId, item.UserId, item.StaffAccountId, item.SkillId,
        item.ReflectionType.ToString(), item.ProposedLevel?.ToString(),
        item.CurrentLevelAtRequest?.ToString(), item.ProjectId, item.EvidenceDescription,
        item.IncidentFrequency, item.ResponsibleAccountId, item.RequestStatus.ToString(),
        item.AssignedTestPaperId, item.VerificationSubmissionId, item.ResultLevel?.ToString(),
        item.RejectionReason, item.CreatedAtUtc, item.ResolvedAtUtc);
}
