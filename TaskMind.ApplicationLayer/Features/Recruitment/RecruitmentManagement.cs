using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Recruitment;

public sealed record JobPostingRequest(string Title, IReadOnlyCollection<Guid>? RequiredSkillIds);

public sealed record JobPostingDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Title,
    IReadOnlyCollection<Guid> RequiredSkillIds,
    string Status,
    DateTime CreatedAtUtc);

public sealed record JobPostingPageDto(
    IReadOnlyCollection<JobPostingDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record JobApplicationItemDto(
    Guid Id,
    Guid JobPostingId,
    Guid UserId,
    string ApplicantName,
    string Status,
    DateTime AppliedAtUtc);

public sealed record JobApplicationPageDto(
    IReadOnlyCollection<JobApplicationItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CreateJobPostingCommand(Guid CompanyId, JobPostingRequest Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record ListCompanyJobPostingsQuery(Guid CompanyId, string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<JobPostingPageDto>>;
public sealed record GetJobPostingQuery(Guid JobPostingId)
    : IRequest<ServiceResult<JobPostingDto>>;
public sealed record UpdateJobPostingCommand(Guid JobPostingId, JobPostingRequest Request)
    : IRequest<ServiceResult>;
public sealed record PublishJobPostingCommand(Guid JobPostingId) : IRequest<ServiceResult>;
public sealed record CloseJobPostingCommand(Guid JobPostingId) : IRequest<ServiceResult>;
public sealed record CancelJobPostingCommand(Guid JobPostingId, string? Reason, bool IsSystemAdmin)
    : IRequest<ServiceResult>;
public sealed record ApplyToJobCommand(Guid JobPostingId) : IRequest<ServiceResult<Guid>>;
public sealed record ListJobApplicationsQuery(Guid JobPostingId, string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<JobApplicationPageDto>>;
public sealed record TransitionJobApplicationCommand(Guid ApplicationId, string Status)
    : IRequest<ServiceResult>;

public sealed class CreateJobPostingHandler
    : IRequestHandler<CreateJobPostingCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CreateJobPostingHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateJobPostingCommand command, CancellationToken ct)
    {
        var company = await RecruitmentRules.GetOwnedCompany(_db, _session, command.CompanyId, ct);
        if (company is null)
            return ServiceResult<Guid>.Forbidden("Chỉ quản trị viên đang hoạt động của công ty mới có thể tạo tin tuyển dụng.");
        if (!await RecruitmentRules.SkillIdsExist(_db, command.Request.RequiredSkillIds, ct))
            return ServiceResult<Guid>.Failure("Một hoặc nhiều kỹ năng không tồn tại hoặc chưa được duyệt.");
        var created = JobPosting.Create(company.Id, command.Request.Title, command.Request.RequiredSkillIds);
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        _db.JobPostings.Add(created.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã tạo tin tuyển dụng ở trạng thái nháp.");
    }
}

public sealed class ListCompanyJobPostingsHandler
    : IRequestHandler<ListCompanyJobPostingsQuery, ServiceResult<JobPostingPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListCompanyJobPostingsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<JobPostingPageDto>> Handle(ListCompanyJobPostingsQuery query, CancellationToken ct)
    {
        if (!RecruitmentRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<JobPostingPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!RecruitmentRules.TryPostingStatus(query.Status, out var status))
            return ServiceResult<JobPostingPageDto>.Failure("Trạng thái tin tuyển dụng không hợp lệ.");

        var account = await RecruitmentRules.GetCurrentAccount(_db, _session, ct);
        var isOwner = account is AdminCompany admin &&
                      admin.CompanyId == query.CompanyId &&
                      admin.IsVerified &&
                      admin.Status == EntityStatus.Active;
        var companyName = await _db.Companies.AsNoTracking()
            .Where(company =>
                company.Id == query.CompanyId &&
                company.IsVerified &&
                company.Status == EntityStatus.Active)
            .Select(company => company.CompanyName)
            .SingleOrDefaultAsync(ct);
        if (companyName is null)
            return ServiceResult<JobPostingPageDto>.NotFound("Không tìm thấy công ty đang hoạt động.");
        if (!isOwner && status.HasValue && status.Value != JobPostingStatus.Open)
            return ServiceResult<JobPostingPageDto>.Forbidden("Chỉ quản trị viên công ty mới có thể xem tin không công khai.");

        var postings = _db.JobPostings.AsNoTracking().Where(item => item.CompanyId == query.CompanyId);
        if (isOwner)
        {
            if (status.HasValue) postings = postings.Where(item => item.PostingStatus == status.Value);
        }
        else
        {
            postings = postings.Where(item => item.PostingStatus == JobPostingStatus.Open);
        }

        var total = await postings.CountAsync(ct);
        var rows = await postings.OrderByDescending(item => item.CreateAt).ThenBy(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .ToListAsync(ct);
        var items = rows.Select(item => new JobPostingDto(
            item.Id,
            item.CompanyId,
            companyName,
            item.Title,
            item.RequiredSkillIds.ToArray(),
            item.PostingStatus.ToString(),
            item.CreateAt)).ToArray();
        return ServiceResult<JobPostingPageDto>.Success(new JobPostingPageDto(items, query.Page, query.PageSize, total));
    }
}

public sealed class GetJobPostingHandler : IRequestHandler<GetJobPostingQuery, ServiceResult<JobPostingDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public GetJobPostingHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<JobPostingDto>> Handle(GetJobPostingQuery query, CancellationToken ct)
    {
        var posting = await _db.JobPostings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.JobPostingId, ct);
        if (posting is null)
            return ServiceResult<JobPostingDto>.NotFound("Không tìm thấy tin tuyển dụng.");
        var isOwner = await RecruitmentRules.IsCompanyAdmin(_db, _session, posting.CompanyId, ct);
        var companyName = await _db.Companies.AsNoTracking()
            .Where(company =>
                company.Id == posting.CompanyId &&
                company.IsVerified &&
                company.Status == EntityStatus.Active)
            .Select(company => company.CompanyName)
            .SingleOrDefaultAsync(ct);
        if (!isOwner && (companyName is null || posting.PostingStatus != JobPostingStatus.Open))
            return ServiceResult<JobPostingDto>.NotFound("Không tìm thấy tin tuyển dụng.");
        return ServiceResult<JobPostingDto>.Success(new JobPostingDto(
            posting.Id,
            posting.CompanyId,
            companyName ?? string.Empty,
            posting.Title,
            posting.RequiredSkillIds.ToArray(),
            posting.PostingStatus.ToString(),
            posting.CreateAt));
    }
}

public sealed class UpdateJobPostingHandler : IRequestHandler<UpdateJobPostingCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public UpdateJobPostingHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(UpdateJobPostingCommand command, CancellationToken ct)
    {
        var posting = await _db.JobPostings.SingleOrDefaultAsync(item => item.Id == command.JobPostingId, ct);
        if (posting is null) return ServiceResult.NotFound("Không tìm thấy tin tuyển dụng.");
        if (await RecruitmentRules.GetOwnedCompany(_db, _session, posting.CompanyId, ct) is null)
            return ServiceResult.Forbidden("Chỉ quản trị viên của công ty sở hữu tin mới có thể cập nhật.");
        if (!await RecruitmentRules.SkillIdsExist(_db, command.Request.RequiredSkillIds, ct))
            return ServiceResult.Failure("Một hoặc nhiều kỹ năng không tồn tại hoặc chưa được duyệt.");
        var result = posting.UpdateDraft(command.Request.Title, command.Request.RequiredSkillIds);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã cập nhật tin tuyển dụng.");
    }
}

public sealed class PublishJobPostingHandler : IRequestHandler<PublishJobPostingCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public PublishJobPostingHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(PublishJobPostingCommand command, CancellationToken ct)
    {
        var posting = await _db.JobPostings.SingleOrDefaultAsync(item => item.Id == command.JobPostingId, ct);
        if (posting is null) return ServiceResult.NotFound("Không tìm thấy tin tuyển dụng.");
        if (await RecruitmentRules.GetOwnedCompany(_db, _session, posting.CompanyId, ct) is null)
            return ServiceResult.Forbidden("Chỉ quản trị viên của công ty sở hữu tin mới có thể đăng tin.");
        var companyActive = await _db.Companies.AnyAsync(company =>
            company.Id == posting.CompanyId && company.IsVerified && company.Status == EntityStatus.Active, ct);
        if (!companyActive) return ServiceResult.Failure("Công ty phải đang hoạt động để đăng tin.");
        var opened = posting.Open();
        if (!opened.IsSuccess) return ServiceResult.Failure(opened.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã đăng tin tuyển dụng.");
    }
}

public sealed class CloseJobPostingHandler : IRequestHandler<CloseJobPostingCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CloseJobPostingHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(CloseJobPostingCommand command, CancellationToken ct)
    {
        var posting = await _db.JobPostings.SingleOrDefaultAsync(item => item.Id == command.JobPostingId, ct);
        if (posting is null) return ServiceResult.NotFound("Không tìm thấy tin tuyển dụng.");
        if (await RecruitmentRules.GetOwnedCompany(_db, _session, posting.CompanyId, ct) is null)
            return ServiceResult.Forbidden("Chỉ quản trị viên của công ty sở hữu tin mới có thể đóng tin.");
        var closed = posting.Close();
        if (!closed.IsSuccess) return ServiceResult.Failure(closed.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã đóng tin tuyển dụng.");
    }
}

public sealed class CancelJobPostingHandler : IRequestHandler<CancelJobPostingCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CancelJobPostingHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(CancelJobPostingCommand command, CancellationToken ct)
    {
        var posting = await _db.JobPostings.SingleOrDefaultAsync(item => item.Id == command.JobPostingId, ct);
        if (posting is null) return ServiceResult.NotFound("Không tìm thấy tin tuyển dụng.");
        Account? actor;
        if (command.IsSystemAdmin)
        {
            actor = await RecruitmentRules.GetSystemAdmin(_db, _session, ct);
            if (actor is null) return ServiceResult.Forbidden();
        }
        else
        {
            if (await RecruitmentRules.GetOwnedCompany(_db, _session, posting.CompanyId, ct) is null)
                return ServiceResult.Forbidden("Chỉ quản trị viên của công ty sở hữu tin mới có thể huỷ tin.");
            actor = await RecruitmentRules.GetCurrentAccount(_db, _session, ct);
        }
        var cancelled = posting.Cancel();
        if (!cancelled.IsSuccess) return ServiceResult.Failure(cancelled.Message);
        if (actor is null) return ServiceResult.Forbidden();
        var audit = AuditLog.Record(
            actor.Id,
            command.IsSystemAdmin ? "JobPostingCancelledByAdmin" : "JobPostingCancelled",
            "JobPosting",
            posting.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        var recipientIds = await _db.JobApplications
            .Where(item => item.JobPostingId == posting.Id)
            .Select(item => item.UserId)
            .Distinct()
            .ToListAsync(ct);
        var notice = $"Tin tuyển dụng {posting.Title} đã bị huỷ." +
                     (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Lý do: {command.Reason.Trim()}");
        foreach (var recipientId in recipientIds)
        {
            var notification = Notification.Create(
                recipientId, "Tin tuyển dụng đã bị huỷ", notice, NotificationType.Warning);
            if (!notification.IsSuccess || notification.Data is null)
                return ServiceResult.Error(notification.Message);
            _db.Notifications.Add(notification.Data);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã huỷ tin tuyển dụng.");
    }
}

public sealed class ApplyToJobHandler : IRequestHandler<ApplyToJobCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ApplyToJobHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(ApplyToJobCommand command, CancellationToken ct)
    {
        var user = await RecruitmentRules.GetCurrentUser(_db, _session, ct);
        if (user is null) return ServiceResult<Guid>.Forbidden("Chỉ người dùng đã xác minh mới có thể ứng tuyển.");
        var posting = await _db.JobPostings.SingleOrDefaultAsync(item => item.Id == command.JobPostingId, ct);
        if (posting is null || posting.PostingStatus != JobPostingStatus.Open)
            return ServiceResult<Guid>.NotFound("Tin tuyển dụng không tồn tại hoặc không còn mở.");
        var companyActive = await _db.Companies.AnyAsync(company =>
            company.Id == posting.CompanyId && company.IsVerified && company.Status == EntityStatus.Active, ct);
        if (!companyActive) return ServiceResult<Guid>.NotFound("Không tìm thấy công ty đang tuyển dụng.");
        if (await _db.JobApplications.AnyAsync(item =>
                item.JobPostingId == posting.Id && item.UserId == user.Id, ct))
            return ServiceResult<Guid>.Failure("Bạn đã ứng tuyển vào tin tuyển dụng này.");
        var application = JobApplication.Apply(posting, user.Id);
        if (!application.IsSuccess || application.Data is null)
            return ServiceResult<Guid>.Failure(application.Message);
        _db.JobApplications.Add(application.Data);
        var administratorIds = await _db.AdminCompanies.AsNoTracking()
            .Where(item => item.CompanyId == posting.CompanyId &&
                           item.IsVerified &&
                           item.Status == EntityStatus.Active)
            .Select(item => item.Id)
            .ToListAsync(ct);
        foreach (var administratorId in administratorIds)
        {
            var notification = Notification.Create(
                administratorId,
                "Hồ sơ ứng tuyển mới",
                $"Đã nhận được hồ sơ mới cho tin tuyển dụng {posting.Title}.",
                NotificationType.System);
            if (!notification.IsSuccess || notification.Data is null)
                return ServiceResult<Guid>.Error(notification.Message);
            _db.Notifications.Add(notification.Data);
        }
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (RecruitmentRules.IsUniqueViolation(
                   exception, "IX_JobApplications_JobPostingId_UserId"))
        {
            return ServiceResult<Guid>.Failure("Bạn đã ứng tuyển vào tin tuyển dụng này.");
        }
        return ServiceResult<Guid>.Success(application.Data.Id, "Đã gửi hồ sơ ứng tuyển.");
    }
}

public sealed class ListJobApplicationsHandler
    : IRequestHandler<ListJobApplicationsQuery, ServiceResult<JobApplicationPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListJobApplicationsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<JobApplicationPageDto>> Handle(ListJobApplicationsQuery query, CancellationToken ct)
    {
        if (!RecruitmentRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<JobApplicationPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!RecruitmentRules.TryApplicationStatus(query.Status, out var status))
            return ServiceResult<JobApplicationPageDto>.Failure("Trạng thái hồ sơ ứng tuyển không hợp lệ.");
        var posting = await _db.JobPostings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.JobPostingId, ct);
        if (posting is null) return ServiceResult<JobApplicationPageDto>.NotFound("Không tìm thấy tin tuyển dụng.");
        if (await RecruitmentRules.GetOwnedCompany(_db, _session, posting.CompanyId, ct) is null)
            return ServiceResult<JobApplicationPageDto>.Forbidden("Chỉ quản trị viên công ty sở hữu tin mới có thể xem hồ sơ.");

        var applications = _db.JobApplications.AsNoTracking()
            .Where(item => item.JobPostingId == posting.Id);
        if (status.HasValue) applications = applications.Where(item => item.ApplicationStatus == status.Value);
        var total = await applications.CountAsync(ct);
        var items = await (
                from application in applications
                join user in _db.Users.AsNoTracking() on application.UserId equals user.Id
                orderby application.AppliedAtUtc descending, application.Id
                select new JobApplicationItemDto(
                    application.Id,
                    application.JobPostingId,
                    application.UserId,
                    user.Profile.NickName,
                    application.ApplicationStatus.ToString(),
                    application.AppliedAtUtc))
            .Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<JobApplicationPageDto>.Success(
            new JobApplicationPageDto(items, query.Page, query.PageSize, total));
    }
}

public sealed class TransitionJobApplicationHandler
    : IRequestHandler<TransitionJobApplicationCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public TransitionJobApplicationHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(TransitionJobApplicationCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<ApplicationStatus>(command.Status, true, out var status) ||
            !Enum.IsDefined(status) ||
            !string.Equals(Enum.GetName(status), command.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            return ServiceResult.Failure("Trạng thái hồ sơ ứng tuyển không hợp lệ.");
        var application = await _db.JobApplications.SingleOrDefaultAsync(item => item.Id == command.ApplicationId, ct);
        if (application is null) return ServiceResult.NotFound("Không tìm thấy hồ sơ ứng tuyển.");
        var posting = await _db.JobPostings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == application.JobPostingId, ct);
        if (posting is null) return ServiceResult.NotFound("Không tìm thấy tin tuyển dụng.");
        if (await RecruitmentRules.GetOwnedCompany(_db, _session, posting.CompanyId, ct) is null)
            return ServiceResult.Forbidden("Chỉ quản trị viên công ty sở hữu tin mới có thể xử lý hồ sơ.");
        var changed = application.UpdateStatus(status);
        if (!changed.IsSuccess) return ServiceResult.Failure(changed.Message);
        var notification = Notification.Create(
            application.UserId,
            "Cập nhật hồ sơ ứng tuyển",
            $"Trạng thái hồ sơ ứng tuyển của bạn đã thay đổi thành {status}.",
            status is ApplicationStatus.Accepted or ApplicationStatus.Shortlisted
                ? NotificationType.Success
                : status == ApplicationStatus.Rejected ? NotificationType.Warning : NotificationType.System);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult.Error(notification.Message);
        _db.Notifications.Add(notification.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã cập nhật trạng thái hồ sơ ứng tuyển.");
    }
}

public sealed record CreateReviewRequest(string TargetType, Guid TargetId, int Rating, string? Comment);
public sealed record ReviewItemDto(
    Guid Id,
    string TargetType,
    Guid TargetId,
    Guid ReviewerAccountId,
    string ReviewerName,
    int Rating,
    string Comment,
    DateTime CreatedAtUtc);
public sealed record ReviewPageDto(
    IReadOnlyCollection<ReviewItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    double? AverageRating);
public sealed record CreateReviewCommand(CreateReviewRequest Request) : IRequest<ServiceResult<Guid>>;
public sealed record ListReviewsQuery(ReviewTargetType TargetType, Guid TargetId, int Page, int PageSize)
    : IRequest<ServiceResult<ReviewPageDto>>;
public sealed record RemoveReviewCommand(Guid ReviewId, string? Reason) : IRequest<ServiceResult>;

public sealed class CreateReviewHandler : IRequestHandler<CreateReviewCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CreateReviewHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateReviewCommand command, CancellationToken ct)
    {
        if (!Enum.TryParse<ReviewTargetType>(command.Request.TargetType, true, out var targetType) ||
            !Enum.IsDefined(targetType) ||
            !string.Equals(
                Enum.GetName(targetType),
                command.Request.TargetType?.Trim(),
                StringComparison.OrdinalIgnoreCase))
            return ServiceResult<Guid>.Failure("Loại đối tượng đánh giá không hợp lệ.");
        var account = await RecruitmentRules.GetCurrentAccount(_db, _session, ct);
        if (account is null || !account.IsVerified || account.Status != EntityStatus.Active)
            return ServiceResult<Guid>.Forbidden();
        if (!await RecruitmentRules.CanReview(_db, account, targetType, command.Request.TargetId, ct))
            return ServiceResult<Guid>.Forbidden("Bạn chưa có quan hệ đã xác minh để đánh giá đối tượng này.");
        var created = Review.Create(targetType, command.Request.TargetId, account.Id,
            command.Request.Rating, command.Request.Comment);
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        _db.Reviews.Add(created.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã gửi đánh giá.");
    }
}

public sealed class ListReviewsHandler : IRequestHandler<ListReviewsQuery, ServiceResult<ReviewPageDto>>
{
    private readonly IApplicationDbContext _db;
    public ListReviewsHandler(IApplicationDbContext db) => _db = db;

    public async Task<ServiceResult<ReviewPageDto>> Handle(ListReviewsQuery query, CancellationToken ct)
    {
        if (!RecruitmentRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<ReviewPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!Enum.IsDefined(query.TargetType) || query.TargetId == Guid.Empty)
            return ServiceResult<ReviewPageDto>.Failure("Đối tượng đánh giá không hợp lệ.");
        var reviews = _db.Reviews.AsNoTracking().Where(review =>
            review.TargetType == query.TargetType &&
            review.TargetRefId == query.TargetId &&
            review.Status == EntityStatus.Active);
        var total = await reviews.CountAsync(ct);
        var average = total == 0 ? (double?)null : await reviews.AverageAsync(item => (double)item.Rating, ct);
        var items = await (
                from review in reviews
                join account in _db.Accounts.AsNoTracking() on review.ReviewerAccountId equals account.Id
                orderby review.CreatedAtUtc descending, review.Id
                select new ReviewItemDto(
                    review.Id,
                    review.TargetType.ToString(),
                    review.TargetRefId,
                    review.ReviewerAccountId,
                    account.Profile.NickName,
                    review.Rating,
                    review.Comment,
                    review.CreatedAtUtc))
            .Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<ReviewPageDto>.Success(
            new ReviewPageDto(items, query.Page, query.PageSize, total, average));
    }
}

public sealed class RemoveReviewHandler : IRequestHandler<RemoveReviewCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public RemoveReviewHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(RemoveReviewCommand command, CancellationToken ct)
    {
        var admin = await RecruitmentRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden();
        var review = await _db.Reviews.SingleOrDefaultAsync(item => item.Id == command.ReviewId, ct);
        if (review is null || review.Status == EntityStatus.Deleted)
            return ServiceResult.NotFound("Không tìm thấy đánh giá đang hiển thị.");
        review.UpdateStatus(EntityStatus.Deleted);
        var audit = AuditLog.Record(admin.Id, "ReviewRemoved", "Review", review.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        var notification = Notification.Create(
            review.ReviewerAccountId,
            "Đánh giá đã được gỡ",
            "Đánh giá của bạn đã được gỡ khỏi nội dung công khai." +
            (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Lý do: {command.Reason.Trim()}"),
            NotificationType.Warning);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult.Error(notification.Message);
        _db.Notifications.Add(notification.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã gỡ đánh giá khỏi nội dung công khai.");
    }
}

internal static class RecruitmentRules
{
    public static async Task<Account?> GetCurrentAccount(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var id = session.GetUserId();
        return !id.HasValue || id.Value == Guid.Empty
            ? null
            : await db.Accounts.SingleOrDefaultAsync(account => account.Id == id.Value, ct);
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

    public static async Task<Company?> GetOwnedCompany(
        IApplicationDbContext db, ICurrentSessionProvider session, Guid companyId, CancellationToken ct)
    {
        var account = await GetCurrentAccount(db, session, ct);
        if (account is not AdminCompany adminCompany ||
            !adminCompany.IsVerified ||
            adminCompany.Status != EntityStatus.Active ||
            adminCompany.CompanyId != companyId)
            return null;
        return await db.Companies.SingleOrDefaultAsync(company =>
            company.Id == companyId && company.IsVerified && company.Status == EntityStatus.Active, ct);
    }

    public static async Task<bool> IsCompanyAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, Guid companyId, CancellationToken ct) =>
        await GetOwnedCompany(db, session, companyId, ct) is not null;

    public static async Task<bool> SkillIdsExist(
        IApplicationDbContext db, IReadOnlyCollection<Guid>? skillIds, CancellationToken ct)
    {
        if (skillIds is null || skillIds.Count == 0) return true;
        var ids = skillIds.Distinct().ToArray();
        if (ids.Contains(Guid.Empty)) return false;
        return await db.Skills.CountAsync(skill =>
            ids.Contains(skill.Id) && skill.IsApproved && skill.Status == EntityStatus.Active, ct) == ids.Length;
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

    public static bool TryPostingStatus(string? value, out JobPostingStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!Enum.TryParse(value.Trim(), true, out JobPostingStatus parsed) ||
            !Enum.IsDefined(parsed) ||
            !string.Equals(Enum.GetName(parsed), value.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;
        status = parsed;
        return true;
    }

    public static bool TryApplicationStatus(string? value, out ApplicationStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!Enum.TryParse(value.Trim(), true, out ApplicationStatus parsed) ||
            !Enum.IsDefined(parsed) ||
            !string.Equals(Enum.GetName(parsed), value.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;
        status = parsed;
        return true;
    }

    public static bool IsUniqueViolation(DbUpdateException exception, params string[] constraints) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: not null
        } postgresException && constraints.Contains(postgresException.ConstraintName, StringComparer.Ordinal);

    public static async Task<bool> CanReview(
        IApplicationDbContext db,
        Account reviewer,
        ReviewTargetType targetType,
        Guid targetId,
        CancellationToken ct)
    {
        if (targetId == Guid.Empty || targetId == reviewer.Id) return false;
        switch (targetType)
        {
            case ReviewTargetType.Company:
                return reviewer is User &&
                       await db.Companies.AnyAsync(company =>
                           company.Id == targetId && company.IsVerified && company.Status == EntityStatus.Active, ct) &&
                       await (
                           from application in db.JobApplications
                           join posting in db.JobPostings on application.JobPostingId equals posting.Id
                           where application.UserId == reviewer.Id &&
                                 application.ApplicationStatus == ApplicationStatus.Accepted &&
                                 posting.CompanyId == targetId
                           select application.Id).AnyAsync(ct);
            case ReviewTargetType.User:
                if (reviewer is not AdminCompany companyAdmin ||
                    companyAdmin.Status != EntityStatus.Active ||
                    !await db.Companies.AnyAsync(company =>
                        company.Id == companyAdmin.CompanyId &&
                        company.IsVerified &&
                        company.Status == EntityStatus.Active, ct))
                    return false;
                return await db.Users.AnyAsync(user =>
                           user.Id == targetId && user.IsVerified && user.Status == EntityStatus.Active, ct) &&
                       await db.JobApplications.AnyAsync(application =>
                           application.UserId == targetId &&
                           application.ApplicationStatus == ApplicationStatus.Accepted &&
                           db.JobPostings.Any(posting =>
                               posting.Id == application.JobPostingId &&
                               posting.CompanyId == companyAdmin.CompanyId), ct);
            case ReviewTargetType.School:
                if (!await db.Schools.AnyAsync(school =>
                        school.Id == targetId && school.IsVerified && school.Status == EntityStatus.Active, ct))
                    return false;
                return reviewer switch
                {
                    Student student => student.SchoolId == targetId && student.IsActive,
                    Teacher teacher => teacher.SchoolId == targetId && teacher.IsActive,
                    _ => false
                };
            default:
                return false;
        }
    }
}
