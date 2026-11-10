using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Assessments.Dtos;
using TaskMind.ApplicationLayer.Features.Auths;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Assessments;

public sealed record CreateTestPaperCommand(
    TestOwnerType OwnerType,
    Guid OwnerId,
    string Title,
    int DurationMinutes) : IRequest<ServiceResult<TestPaperDto>>;

public sealed class CreateTestPaperHandler :
    IRequestHandler<CreateTestPaperCommand, ServiceResult<TestPaperDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public CreateTestPaperHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<TestPaperDto>> Handle(
        CreateTestPaperCommand command,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(command.OwnerType) || command.OwnerId == Guid.Empty)
            return ServiceResult<TestPaperDto>.Failure("Chủ sở hữu bài kiểm tra không hợp lệ.");
        if (string.IsNullOrWhiteSpace(command.Title) || command.DurationMinutes <= 0)
            return ServiceResult<TestPaperDto>.Failure("Tiêu đề phải có nội dung và thời lượng phải lớn hơn 0.");

        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<TestPaperDto>.Forbidden(accountResult.Message);
        if (!await AssessmentAuthorization.CanManageOwnerAsync(
                _dbContext, accountResult.Data, command.OwnerType, command.OwnerId, cancellationToken))
            return ServiceResult<TestPaperDto>.Forbidden("Bạn không có quyền quản lý bài kiểm tra của đơn vị này.");

        var createResult = TestPaper.Create(
            command.OwnerType, command.OwnerId, command.Title, command.DurationMinutes);
        if (!createResult.IsSuccess || createResult.Data is null)
            return ServiceResult<TestPaperDto>.Failure(createResult.Message);

        _dbContext.TestPapers.Add(createResult.Data);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<TestPaperDto>.Success(AssessmentMappings.ToDto(createResult.Data));
    }
}

public sealed record UpdateTestPaperCommand(Guid TestPaperId, string Title, int DurationMinutes)
    : IRequest<ServiceResult<TestPaperDto>>;

public sealed class UpdateTestPaperHandler :
    IRequestHandler<UpdateTestPaperCommand, ServiceResult<TestPaperDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public UpdateTestPaperHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<TestPaperDto>> Handle(
        UpdateTestPaperCommand command,
        CancellationToken cancellationToken)
    {
        var paper = await _dbContext.TestPapers.SingleOrDefaultAsync(
            item => item.Id == command.TestPaperId,
            cancellationToken);
        if (paper is null)
            return ServiceResult<TestPaperDto>.NotFound("Không tìm thấy bài kiểm tra.");

        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<TestPaperDto>.Forbidden(accountResult.Message);
        if (!await AssessmentAuthorization.CanManageOwnerAsync(
                _dbContext, accountResult.Data, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<TestPaperDto>.Forbidden("Bạn không có quyền cập nhật bài kiểm tra này.");

        if (await _dbContext.Submissions.AnyAsync(
                submission => submission.TestPaperId == paper.Id &&
                              submission.SubmittedAtUtc != null,
                cancellationToken))
            return ServiceResult<TestPaperDto>.Failure(
                "Không thể sửa bài kiểm tra sau khi đã có bài nộp.");

        var updateResult = paper.UpdateDetails(command.Title, command.DurationMinutes);
        if (!updateResult.IsSuccess)
            return ServiceResult<TestPaperDto>.Failure(updateResult.Message);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<TestPaperDto>.Success(AssessmentMappings.ToDto(paper));
    }
}

public sealed record AddTestQuestionCommand(
    Guid TestPaperId, string Prompt, IReadOnlyCollection<string> Options,
    int CorrectOptionIndex, decimal Points) : IRequest<ServiceResult<TestQuestionDto>>;

public sealed class AddTestQuestionHandler :
    IRequestHandler<AddTestQuestionCommand, ServiceResult<TestQuestionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public AddTestQuestionHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<TestQuestionDto>> Handle(
        AddTestQuestionCommand command, CancellationToken cancellationToken)
    {
        var paper = await _dbContext.TestPapers.Include(item => item.Questions)
            .SingleOrDefaultAsync(item => item.Id == command.TestPaperId, cancellationToken);
        if (paper is null) return ServiceResult<TestQuestionDto>.NotFound("Không tìm thấy bài kiểm tra.");
        var actor = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!actor.IsSuccess || actor.Data is null)
            return ServiceResult<TestQuestionDto>.Forbidden(actor.Message);
        if (!await AssessmentAuthorization.CanManageOwnerAsync(
                _dbContext, actor.Data, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<TestQuestionDto>.Forbidden("Bạn không có quyền quản lý câu hỏi của bài kiểm tra.");
        if (await _dbContext.Submissions.AnyAsync(item => item.TestPaperId == paper.Id, cancellationToken))
            return ServiceResult<TestQuestionDto>.Failure("Không thể thay đổi ngân hàng câu hỏi sau khi tạo bài nộp.");
        var added = paper.AddQuestion(
            command.Prompt, command.Options, command.CorrectOptionIndex, command.Points);
        if (!added.IsSuccess || added.Data is null)
            return ServiceResult<TestQuestionDto>.Failure(added.Message);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<TestQuestionDto>.Success(AssessmentMappings.ToDto(added.Data));
    }
}

public sealed record SaveSubmissionAnswersCommand(
    Guid SubmissionId, IReadOnlyCollection<SubmissionAnswerInput> Answers)
    : IRequest<ServiceResult>;

public sealed class SaveSubmissionAnswersHandler :
    IRequestHandler<SaveSubmissionAnswersCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public SaveSubmissionAnswersHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult> Handle(SaveSubmissionAnswersCommand command, CancellationToken cancellationToken)
    {
        var actor = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!actor.IsSuccess || actor.Data?.UserId is null)
            return ServiceResult.Forbidden(actor.Message);
        var submission = await _dbContext.Submissions.Include(item => item.Answers)
            .SingleOrDefaultAsync(item => item.Id == command.SubmissionId &&
                                          item.UserId == actor.Data.UserId.Value, cancellationToken);
        if (submission is null) return ServiceResult.NotFound("Không tìm thấy bài nộp của bạn.");
        var paper = await _dbContext.TestPapers.Include(item => item.Questions)
            .SingleOrDefaultAsync(item => item.Id == submission.TestPaperId, cancellationToken);
        if (paper is null) return ServiceResult.NotFound("Không tìm thấy bài kiểm tra.");
        var saved = submission.SaveAnswers(paper, command.Answers);
        if (!saved.IsSuccess) return ServiceResult.Failure(saved.Message);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã lưu câu trả lời.");
    }
}

public sealed record CreateSubmissionCommand(Guid TestPaperId) : IRequest<ServiceResult<SubmissionDto>>;

public sealed class CreateSubmissionHandler :
    IRequestHandler<CreateSubmissionCommand, ServiceResult<SubmissionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public CreateSubmissionHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<SubmissionDto>> Handle(
        CreateSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<SubmissionDto>.Forbidden(accountResult.Message);

        var paper = await _dbContext.TestPapers.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == command.TestPaperId && item.Status == EntityStatus.Active,
            cancellationToken);
        if (paper is null)
            return ServiceResult<SubmissionDto>.NotFound("Không tìm thấy bài kiểm tra đang hoạt động.");
        if (!await AssessmentAuthorization.IsOwnerActiveAsync(
                _dbContext, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<SubmissionDto>.Failure("Đơn vị sở hữu bài kiểm tra không còn hoạt động.");

        if (!accountResult.Data.UserId.HasValue)
            return ServiceResult<SubmissionDto>.Forbidden("Tài khoản này không được liên kết với một hồ sơ User.");

        var createResult = Submission.Create(paper.Id, accountResult.Data.UserId.Value);
        if (!createResult.IsSuccess || createResult.Data is null)
            return ServiceResult<SubmissionDto>.Failure(createResult.Message);

        _dbContext.Submissions.Add(createResult.Data);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ServiceResult<SubmissionDto>.Success(
            AssessmentMappings.ToDto(createResult.Data, paper.Title));
    }
}

public sealed record SubmitSubmissionCommand(Guid SubmissionId) : IRequest<ServiceResult<SubmissionDto>>;

public sealed class SubmitSubmissionHandler :
    IRequestHandler<SubmitSubmissionCommand, ServiceResult<SubmissionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public SubmitSubmissionHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<SubmissionDto>> Handle(
        SubmitSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<SubmissionDto>.Forbidden(accountResult.Message);
        if (!accountResult.Data.UserId.HasValue)
            return ServiceResult<SubmissionDto>.Forbidden("Tài khoản này không được liên kết với một hồ sơ User.");

        var submission = await _dbContext.Submissions.Include(item => item.Answers).SingleOrDefaultAsync(
            item => item.Id == command.SubmissionId &&
                    item.UserId == accountResult.Data.UserId.Value,
            cancellationToken);
        if (submission is null)
            return ServiceResult<SubmissionDto>.NotFound("Không tìm thấy bài nộp của bạn.");

        var paper = await _dbContext.TestPapers.Include(item => item.Questions)
            .SingleOrDefaultAsync(item => item.Id == submission.TestPaperId, cancellationToken);
        if (paper is null)
            return ServiceResult<SubmissionDto>.NotFound("Không tìm thấy bài kiểm tra.");
        var result = submission.Submit(paper);
        if (!result.IsSuccess)
            return ServiceResult<SubmissionDto>.Failure(result.Message);

        var title = paper.Title;
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<SubmissionDto>.Failure(
                "Bài nộp vừa được cập nhật bởi một yêu cầu khác. Hãy tải lại và thử lại.");
        }
        return ServiceResult<SubmissionDto>.Success(AssessmentMappings.ToDto(submission, title));
    }
}

public sealed record GradeSubmissionCommand(Guid SubmissionId, decimal Score)
    : IRequest<ServiceResult<SubmissionDto>>;

public sealed class GradeSubmissionHandler :
    IRequestHandler<GradeSubmissionCommand, ServiceResult<SubmissionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public GradeSubmissionHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<SubmissionDto>> Handle(
        GradeSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Score < 0)
            return ServiceResult<SubmissionDto>.Failure("Điểm số không thể nhỏ hơn 0.");

        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<SubmissionDto>.Forbidden(accountResult.Message);

        var submission = await _dbContext.Submissions.SingleOrDefaultAsync(
            item => item.Id == command.SubmissionId,
            cancellationToken);
        if (submission is null)
            return ServiceResult<SubmissionDto>.NotFound("Không tìm thấy bài nộp.");

        var paper = await _dbContext.TestPapers.SingleOrDefaultAsync(
            item => item.Id == submission.TestPaperId,
            cancellationToken);
        if (paper is null)
            return ServiceResult<SubmissionDto>.Failure("Bài nộp không còn bài kiểm tra hợp lệ.");

        if (!await AssessmentAuthorization.CanManageOwnerAsync(
                _dbContext, accountResult.Data, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<SubmissionDto>.Forbidden("Bạn không có quyền chấm bài này.");

        var gradeResult = submission.Grade(command.Score);
        if (!gradeResult.IsSuccess)
            return ServiceResult<SubmissionDto>.Failure(gradeResult.Message);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<SubmissionDto>.Failure(
                "Bài nộp vừa được cập nhật bởi một yêu cầu khác. Hãy tải lại và thử lại.");
        }

        return ServiceResult<SubmissionDto>.Success(
            AssessmentMappings.ToDto(submission, paper.Title),
            "Chấm điểm thành công.");
    }
}

public sealed record GetSubmissionQuery(Guid SubmissionId) : IRequest<ServiceResult<SubmissionDto>>;

public sealed class GetSubmissionHandler : IRequestHandler<GetSubmissionQuery, ServiceResult<SubmissionDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public GetSubmissionHandler(IApplicationDbContext dbContext, ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult<SubmissionDto>> Handle(
        GetSubmissionQuery query,
        CancellationToken cancellationToken)
    {
        var accountResult = await AssessmentAccountResolver.ResolveAsync(
            _dbContext, _sessionProvider, cancellationToken);
        if (!accountResult.IsSuccess || accountResult.Data is null)
            return ServiceResult<SubmissionDto>.Forbidden(accountResult.Message);

        var submission = await _dbContext.Submissions.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == query.SubmissionId,
            cancellationToken);
        if (submission is null)
            return ServiceResult<SubmissionDto>.NotFound("Không tìm thấy bài nộp.");

        var paper = await _dbContext.TestPapers.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == submission.TestPaperId,
            cancellationToken);
        if (paper is null)
            return ServiceResult<SubmissionDto>.NotFound("Không tìm thấy bài kiểm tra.");

        if (submission.UserId != accountResult.Data.UserId &&
            !await AssessmentAuthorization.CanManageOwnerAsync(
                _dbContext, accountResult.Data, paper.OwnerType, paper.OwnerId, cancellationToken))
            return ServiceResult<SubmissionDto>.Forbidden("Bạn không có quyền xem bài nộp này.");

        return ServiceResult<SubmissionDto>.Success(
            AssessmentMappings.ToDto(submission, paper.Title));
    }
}

public sealed record VerifyCertificateQuery(string VerificationCode)
    : IRequest<ServiceResult<CertificateVerificationDto>>;

public sealed class VerifyCertificateHandler :
    IRequestHandler<VerifyCertificateQuery, ServiceResult<CertificateVerificationDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public VerifyCertificateHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<CertificateVerificationDto>> Handle(
        VerifyCertificateQuery query,
        CancellationToken cancellationToken)
    {
        var code = query.VerificationCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code) || code.Length != 12 ||
            code.Any(character => !char.IsAsciiLetterOrDigit(character)))
            return ServiceResult<CertificateVerificationDto>.Failure("Mã xác minh phải gồm 12 ký tự chữ hoặc số.");

        var certificate = await _dbContext.Certificates.AsNoTracking()
            .SingleOrDefaultAsync(item => item.VerificationCode == code, cancellationToken);
        if (certificate is null)
            return ServiceResult<CertificateVerificationDto>.NotFound("Không tìm thấy chứng chỉ hợp lệ.");

        var user = await _dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == certificate.UserId,
                cancellationToken);
        if (user is null)
            return ServiceResult<CertificateVerificationDto>.NotFound("Không tìm thấy chứng chỉ hợp lệ.");

        string? title = null;
        if (certificate.SubmissionId.HasValue)
        {
            title = await (
                    from submission in _dbContext.Submissions.AsNoTracking()
                    join paper in _dbContext.TestPapers.AsNoTracking()
                        on submission.TestPaperId equals paper.Id
                    where submission.Id == certificate.SubmissionId.Value
                    select paper.Title)
                .SingleOrDefaultAsync(cancellationToken);
        }

        return ServiceResult<CertificateVerificationDto>.Success(new CertificateVerificationDto
        {
            IsValid = true,
            VerificationCode = certificate.VerificationCode,
            RecipientName = string.Join(' ', new[] { user.Profile.FirstName, user.Profile.LastName }
                .Where(name => !string.IsNullOrWhiteSpace(name))),
            TestPaperTitle = title,
            IssuedAtUtc = certificate.IssuedAtUtc
        });
    }
}

internal sealed record AssessmentAccount(Guid AccountId, Guid? UserId, Account Account);

internal static class AssessmentAccountResolver
{
    public static async Task<TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<AssessmentAccount>> ResolveAsync(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider,
        CancellationToken cancellationToken)
    {
        var accountId = sessionProvider.GetUserId();
        if (!accountId.HasValue || accountId.Value == Guid.Empty)
            return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<AssessmentAccount>.Failure(
                "Không xác định được tài khoản hiện tại.");

        var account = await dbContext.Accounts.SingleOrDefaultAsync(
            item => item.Id == accountId.Value,
            cancellationToken);
        if (account is null || !account.IsVerified || account.Status != EntityStatus.Active)
            return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<AssessmentAccount>.Failure(
                "Tài khoản không đủ điều kiện thực hiện thao tác này.");

        var userId = account switch
        {
            User user => user.Id,
            Staff staff when staff.IsActive => staff.LinkedUserId,
            Student student when student.IsActive => student.LinkedUserId,
            Teacher teacher when teacher.IsActive => teacher.LinkedUserId,
            AdminCompany adminCompany => adminCompany.LinkedUserId,
            AdminSchool adminSchool => adminSchool.LinkedUserId,
            _ => (Guid?)null
        };
        if (userId.HasValue &&
            !await dbContext.Users.AnyAsync(
                user => user.Id == userId.Value &&
                        user.IsVerified &&
                        user.Status == EntityStatus.Active,
                cancellationToken))
            userId = null;

        return TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result<AssessmentAccount>.Success(
            new AssessmentAccount(account.Id, userId, account));
    }
}

internal static class AssessmentAuthorization
{
    public static async Task<bool> CanManageOwnerAsync(
        IApplicationDbContext dbContext,
        AssessmentAccount assessmentAccount,
        TestOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        if (assessmentAccount.Account.Role == AccountRole.Admin)
            return await IsOwnerActiveAsync(dbContext, ownerType, ownerId, cancellationToken);

        if (!await IsOwnerActiveAsync(dbContext, ownerType, ownerId, cancellationToken))
            return false;

        return (ownerType, assessmentAccount.Account) switch
        {
            (TestOwnerType.Company, AdminCompany admin) => admin.CompanyId == ownerId,
            (TestOwnerType.School, AdminSchool admin) => admin.SchoolId == ownerId,
            (TestOwnerType.School, Teacher teacher) =>
                teacher.IsActive && teacher.SchoolId == ownerId,
            _ => false
        };
    }

    public static async Task<bool> IsOwnerActiveAsync(
        IApplicationDbContext dbContext,
        TestOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken) =>
        ownerType switch
        {
            TestOwnerType.Company => await dbContext.Companies.AnyAsync(
                company => company.Id == ownerId &&
                           company.IsVerified &&
                           company.Status == EntityStatus.Active,
                cancellationToken),
            TestOwnerType.School => await dbContext.Schools.AnyAsync(
                school => school.Id == ownerId &&
                          school.IsVerified &&
                          school.Status == EntityStatus.Active,
                cancellationToken),
            _ => false
        };
}

internal static class AssessmentMappings
{
    public static TestPaperDto ToDto(TestPaper paper) => new()
    {
        Id = paper.Id,
        OwnerType = paper.OwnerType.ToString(),
        OwnerId = paper.OwnerId,
        Title = paper.Title,
        DurationMinutes = paper.DurationMinutes,
        Questions = paper.Questions.Select(ToDto).ToArray()
    };

    public static TestQuestionDto ToDto(TestQuestion question) => new()
    {
        Id = question.Id,
        Prompt = question.Prompt,
        Options = question.Options,
        Points = question.Points
    };

    public static SubmissionDto ToDto(Submission submission, string title) => new()
    {
        Id = submission.Id,
        TestPaperId = submission.TestPaperId,
        TestPaperTitle = title,
        UserId = submission.UserId,
        Score = submission.Score,
        SubmittedAtUtc = submission.SubmittedAtUtc,
        IsPassed = submission.Score >= 5m,
        IsGraded = submission.IsGraded
    };
}
