using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record ListMySubmissionsQuery(string? Status = null, int Page = 1, int PageSize = 20)
    : IRequest<ServiceResult<PersonalSubmissionsDto>>;

public sealed class ListMySubmissionsHandler :
    IRequestHandler<ListMySubmissionsQuery, ServiceResult<PersonalSubmissionsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public ListMySubmissionsHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PersonalSubmissionsDto>> Handle(
        ListMySubmissionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!PersonalQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PersonalSubmissionsDto>.Failure(error);

        var status = query.Status?.Trim().ToLowerInvariant();
        if (status is not null && status is not ("draft" or "submitted" or "graded"))
            return ServiceResult<PersonalSubmissionsDto>.Failure(
                "Status phải là draft, submitted hoặc graded.");

        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PersonalSubmissionsDto>.Forbidden(accountResult.Message);

        var submissions = _dbContext.Submissions.AsNoTracking()
            .Where(item => item.UserId == accountResult.Data.UserId);
        submissions = status switch
        {
            "draft" => submissions.Where(item => item.SubmittedAtUtc == null),
            "submitted" => submissions.Where(item => item.SubmittedAtUtc != null && item.Score == null),
            "graded" => submissions.Where(item => item.Score != null),
            _ => submissions
        };

        var totalCount = await submissions.CountAsync(cancellationToken);
        var rows = await (
                from submission in submissions
                join testPaper in _dbContext.TestPapers.AsNoTracking()
                    on submission.TestPaperId equals testPaper.Id
                orderby submission.SubmittedAtUtc descending, submission.Id
                select new PersonalSubmissionDto
                {
                    Id = submission.Id,
                    TestPaperId = testPaper.Id,
                    TestPaperTitle = testPaper.Title,
                    OwnerType = testPaper.OwnerType.ToString(),
                    OwnerId = testPaper.OwnerId,
                    Score = submission.Score,
                    SubmittedAtUtc = submission.SubmittedAtUtc,
                    Status = submission.Score != null
                        ? "Graded"
                        : submission.SubmittedAtUtc != null
                            ? "Submitted"
                            : "Draft"
                })
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PersonalSubmissionsDto>.Success(new PersonalSubmissionsDto
        {
            Items = rows,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }
}

public sealed record ListMyCertificatesQuery(int Page = 1, int PageSize = 20)
    : IRequest<ServiceResult<PersonalCertificatesDto>>;

public sealed class ListMyCertificatesHandler :
    IRequestHandler<ListMyCertificatesQuery, ServiceResult<PersonalCertificatesDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public ListMyCertificatesHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PersonalCertificatesDto>> Handle(
        ListMyCertificatesQuery query,
        CancellationToken cancellationToken)
    {
        if (!PersonalQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PersonalCertificatesDto>.Failure(error);

        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PersonalCertificatesDto>.Forbidden(accountResult.Message);

        var certificates = _dbContext.Certificates.AsNoTracking()
            .Where(item => item.UserId == accountResult.Data.UserId);
        var totalCount = await certificates.CountAsync(cancellationToken);
        var items = await certificates
            .OrderByDescending(item => item.IssuedAtUtc)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(item => new PersonalCertificateDto
            {
                Id = item.Id,
                SubmissionId = item.SubmissionId,
                VerificationCode = item.VerificationCode,
                IssuedAtUtc = item.IssuedAtUtc
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PersonalCertificatesDto>.Success(new PersonalCertificatesDto
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }
}

public sealed record ListMyJobApplicationsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<ServiceResult<PersonalJobApplicationsDto>>;

public sealed class ListMyJobApplicationsHandler :
    IRequestHandler<ListMyJobApplicationsQuery, ServiceResult<PersonalJobApplicationsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public ListMyJobApplicationsHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PersonalJobApplicationsDto>> Handle(
        ListMyJobApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        if (!PersonalQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PersonalJobApplicationsDto>.Failure(error);

        ApplicationStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse(query.Status.Trim(), ignoreCase: true, out ApplicationStatus parsed) ||
                !Enum.IsDefined(parsed) ||
                !string.Equals(Enum.GetName(parsed), query.Status.Trim(), StringComparison.OrdinalIgnoreCase))
                return ServiceResult<PersonalJobApplicationsDto>.Failure("Status hồ sơ ứng tuyển không hợp lệ.");
            status = parsed;
        }

        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PersonalJobApplicationsDto>.Forbidden(accountResult.Message);

        var applications = _dbContext.JobApplications.AsNoTracking()
            .Where(item => item.UserId == accountResult.Data.UserId);
        if (status.HasValue)
            applications = applications.Where(item => item.ApplicationStatus == status.Value);

        var totalCount = await applications.CountAsync(cancellationToken);
        var rows = await (
                from application in applications
                join posting in _dbContext.JobPostings.AsNoTracking()
                    on application.JobPostingId equals posting.Id
                orderby application.AppliedAtUtc descending, application.Id
                select new PersonalJobApplicationDto
                {
                    Id = application.Id,
                    JobPostingId = posting.Id,
                    JobTitle = posting.Title,
                    Status = application.ApplicationStatus.ToString(),
                    AppliedAtUtc = application.AppliedAtUtc
                })
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PersonalJobApplicationsDto>.Success(new PersonalJobApplicationsDto
        {
            Items = rows,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }
}

public sealed record ListMySkillLevelUpRequestsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<ServiceResult<PersonalSkillLevelUpRequestsDto>>;

public sealed record GetMySkillLevelUpRequestQuery(Guid RequestId)
    : IRequest<ServiceResult<PersonalSkillLevelUpRequestDto>>;

public sealed class ListMySkillLevelUpRequestsHandler :
    IRequestHandler<ListMySkillLevelUpRequestsQuery, ServiceResult<PersonalSkillLevelUpRequestsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public ListMySkillLevelUpRequestsHandler(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PersonalSkillLevelUpRequestsDto>> Handle(
        ListMySkillLevelUpRequestsQuery query,
        CancellationToken cancellationToken)
    {
        if (!PersonalQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PersonalSkillLevelUpRequestsDto>.Failure(error);

        SkillLevelUpRequestStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse(query.Status.Trim(), ignoreCase: true, out SkillLevelUpRequestStatus parsed) ||
                !Enum.IsDefined(parsed) ||
                !string.Equals(Enum.GetName(parsed), query.Status.Trim(), StringComparison.OrdinalIgnoreCase))
                return ServiceResult<PersonalSkillLevelUpRequestsDto>.Failure(
                    "Trạng thái yêu cầu nâng kỹ năng không hợp lệ.");
            status = parsed;
        }

        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PersonalSkillLevelUpRequestsDto>.Forbidden(accountResult.Message);

        var requests = _dbContext.SkillLevelUpRequests.AsNoTracking()
            .Where(item => item.UserId == accountResult.Data.UserId);
        if (status.HasValue)
            requests = requests.Where(item => item.RequestStatus == status.Value);

        var totalCount = await requests.CountAsync(cancellationToken);
        var requestRows = await requests
            .OrderByDescending(item => item.CreateAt)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var skillIds = requestRows.Select(item => item.SkillId).Distinct().ToArray();
        var skillNames = await _dbContext.Skills.AsNoTracking()
            .Where(skill => skillIds.Contains(skill.Id))
            .ToDictionaryAsync(skill => skill.Id, skill => skill.SkillName, cancellationToken);
        var items = requestRows.Select(item => MapSkillLevelUpRequest(item, skillNames)).ToArray();

        return ServiceResult<PersonalSkillLevelUpRequestsDto>.Success(new PersonalSkillLevelUpRequestsDto
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    internal static PersonalSkillLevelUpRequestDto MapSkillLevelUpRequest(
        SkillLevelUpRequest request,
        IReadOnlyDictionary<Guid, string> skillNames) => new()
    {
        Id = request.Id,
        SkillId = request.SkillId,
        SkillName = skillNames.GetValueOrDefault(request.SkillId, string.Empty),
        CurrentLevel = request.CurrentLevel.ToString(),
        ApproverAccountId = request.ApproverAccountId,
        RequestType = request.RequestType.ToString(),
        SubmissionId = request.SubmissionId,
        Status = request.RequestStatus.ToString(),
        RejectionReason = request.RejectionReason
    };
}

public sealed class GetMySkillLevelUpRequestHandler :
    IRequestHandler<GetMySkillLevelUpRequestQuery, ServiceResult<PersonalSkillLevelUpRequestDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public GetMySkillLevelUpRequestHandler(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PersonalSkillLevelUpRequestDto>> Handle(
        GetMySkillLevelUpRequestQuery query,
        CancellationToken cancellationToken)
    {
        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PersonalSkillLevelUpRequestDto>.Forbidden(accountResult.Message);

        var request = await _dbContext.SkillLevelUpRequests.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == query.RequestId && item.UserId == accountResult.Data.UserId,
                cancellationToken);
        if (request is null)
            return ServiceResult<PersonalSkillLevelUpRequestDto>.NotFound(
                "Không tìm thấy yêu cầu nâng kỹ năng.");

        var skillName = await _dbContext.Skills.AsNoTracking()
            .Where(skill => skill.Id == request.SkillId)
            .Select(skill => skill.SkillName)
            .SingleOrDefaultAsync(cancellationToken);
        return ServiceResult<PersonalSkillLevelUpRequestDto>.Success(
            ListMySkillLevelUpRequestsHandler.MapSkillLevelUpRequest(
                request,
                skillName is null
                    ? new Dictionary<Guid, string>()
                    : new Dictionary<Guid, string> { [request.SkillId] = skillName }));
    }
}

public sealed record ListMyNotificationsQuery(bool? IsRead = null, int Page = 1, int PageSize = 20)
    : IRequest<ServiceResult<PersonalNotificationsDto>>;

public sealed class ListMyNotificationsHandler :
    IRequestHandler<ListMyNotificationsQuery, ServiceResult<PersonalNotificationsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public ListMyNotificationsHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<PersonalNotificationsDto>> Handle(
        ListMyNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        if (!PersonalQueryRules.TryGetPaging(query.Page, query.PageSize, out var skip, out var error))
            return ServiceResult<PersonalNotificationsDto>.Failure(error);

        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<PersonalNotificationsDto>.Forbidden(accountResult.Message);

        var notifications = _dbContext.Notifications.AsNoTracking()
            .Where(item => item.RecipientAccountId == accountResult.Data.AccountId);
        if (query.IsRead.HasValue)
            notifications = notifications.Where(item => item.IsRead == query.IsRead.Value);

        var totalCount = await notifications.CountAsync(cancellationToken);
        var items = await notifications
            .OrderByDescending(item => item.CreatedAtUtc)
            .ThenBy(item => item.Id)
            .Skip(skip)
            .Take(query.PageSize)
            .Select(item => new PersonalNotificationDto
            {
                Id = item.Id,
                Title = item.Title,
                Message = item.Message,
                Type = item.Type.ToString(),
                IsRead = item.IsRead,
                CreatedAtUtc = item.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PersonalNotificationsDto>.Success(new PersonalNotificationsDto
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }
}

public sealed record MarkMyNotificationReadCommand(Guid NotificationId) : IRequest<ServiceResult>;

public sealed class MarkMyNotificationReadHandler :
    IRequestHandler<MarkMyNotificationReadCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public MarkMyNotificationReadHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult> Handle(
        MarkMyNotificationReadCommand command,
        CancellationToken cancellationToken)
    {
        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult.Forbidden(accountResult.Message);

        var notification = await _dbContext.Notifications.SingleOrDefaultAsync(
            item => item.Id == command.NotificationId &&
                    item.RecipientAccountId == accountResult.Data.AccountId,
            cancellationToken);
        if (notification is null)
            return ServiceResult.NotFound("Không tìm thấy thông báo.");

        var result = notification.MarkAsRead();
        if (!result.IsSuccess)
            return ServiceResult.Failure(result.Message);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã đánh dấu thông báo là đã đọc.");
    }
}

public sealed record MarkAllMyNotificationsReadCommand : IRequest<ServiceResult>;

public sealed class MarkAllMyNotificationsReadHandler :
    IRequestHandler<MarkAllMyNotificationsReadCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public MarkAllMyNotificationsReadHandler(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult> Handle(
        MarkAllMyNotificationsReadCommand command,
        CancellationToken cancellationToken)
    {
        var accountResult = await PersonalAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult.Forbidden(accountResult.Message);

        var unreadNotifications = await _dbContext.Notifications
            .Where(item => item.RecipientAccountId == accountResult.Data.AccountId && !item.IsRead)
            .ToListAsync(cancellationToken);
        foreach (var notification in unreadNotifications)
            notification.MarkAsRead();

        if (unreadNotifications.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success($"Đã đánh dấu {unreadNotifications.Count} thông báo là đã đọc.");
    }
}

internal sealed record PersonalAccount(Guid AccountId, Guid UserId);

internal static class PersonalAccountResolver
{
    public static async Task<Result<PersonalAccount>> ResolveAsync(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider,
        CancellationToken cancellationToken)
    {
        var accountId = sessionProvider.GetUserId();
        if (!accountId.HasValue || accountId.Value == Guid.Empty)
            return Result<PersonalAccount>.Failure("Không xác định được tài khoản hiện tại.");

        var account = await dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Id == accountId.Value,
            cancellationToken);
        if (account is null || !account.IsVerified || account.Status != EntityStatus.Active)
            return Result<PersonalAccount>.Failure("Tài khoản không đủ điều kiện truy cập dữ liệu cá nhân.");

        var userId = account switch
        {
            User user => user.Id,
            Staff staff when staff.IsActive => staff.LinkedUserId,
            Student student when student.IsActive => student.LinkedUserId,
            Teacher teacher when teacher.IsActive => teacher.LinkedUserId,
            AdminCompany adminCompany => adminCompany.LinkedUserId,
            AdminSchool adminSchool => adminSchool.LinkedUserId,
            _ => Guid.Empty
        };
        if (userId == Guid.Empty ||
            !await dbContext.Users.AnyAsync(
                user => user.Id == userId &&
                        user.IsVerified &&
                        user.Status == EntityStatus.Active,
                cancellationToken))
            return Result<PersonalAccount>.Failure("Tài khoản không được liên kết với User đang hoạt động.");

        return Result<PersonalAccount>.Success(new PersonalAccount(account.Id, userId));
    }
}

internal static class PersonalQueryRules
{
    public static bool TryGetPaging(int page, int pageSize, out int skip, out string error)
    {
        skip = 0;
        error = string.Empty;
        if (page < 1 || pageSize is < 1 or > 100)
        {
            error = "Page phải từ 1 trở lên và PageSize phải từ 1 đến 100.";
            return false;
        }

        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            error = "Page vượt quá giới hạn hỗ trợ.";
            return false;
        }

        skip = (int)offset;
        return true;
    }
}
