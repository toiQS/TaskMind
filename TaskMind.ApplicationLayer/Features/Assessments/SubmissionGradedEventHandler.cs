using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.ApplicationLayer.Features.Assessments;

public sealed class SubmissionGradedEventHandler : INotificationHandler<SubmissionGradedEvent>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<SubmissionGradedEventHandler> _logger;

    public SubmissionGradedEventHandler(
        IApplicationDbContext dbContext,
        ILogger<SubmissionGradedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(SubmissionGradedEvent notification, CancellationToken cancellationToken)
    {
        var submission = await _dbContext.Submissions.SingleOrDefaultAsync(
            item => item.Id == notification.SubmissionId &&
                    item.TestPaperId == notification.TestPaperId &&
                    item.UserId == notification.UserId &&
                    item.Score == notification.Score,
            cancellationToken);
        if (submission is null)
            throw new InvalidOperationException(
                $"Graded submission {notification.SubmissionId} could not be reloaded consistently.");

        var testPaper = await _dbContext.TestPapers.SingleOrDefaultAsync(
            item => item.Id == notification.TestPaperId,
            cancellationToken);
        if (testPaper is null)
            throw new InvalidOperationException(
                $"Test paper {notification.TestPaperId} for graded submission {submission.Id} was not found.");

        if (testPaper.OwnerType == TestOwnerType.School &&
            notification.Score >= 5m &&
            !await _dbContext.Certificates.AnyAsync(
                certificate => certificate.SubmissionId == submission.Id,
                cancellationToken))
        {
            var certificateResult = Certificate.Issue(submission.UserId, submission.Id);
            if (!certificateResult.IsSuccess || certificateResult.Data is null)
                throw new InvalidOperationException(
                    $"Could not issue a certificate for passed School submission {submission.Id}: " +
                    certificateResult.Message);
            _dbContext.Certificates.Add(certificateResult.Data);
        }

        await ApplySkillLevelUpAssessmentAsync(submission, cancellationToken);
        await ApplyCompanySkillReflectionAsync(submission, cancellationToken);
        await CreateGradeNotificationAsync(submission, testPaper, cancellationToken);
    }

    private async Task ApplySkillLevelUpAssessmentAsync(
        Submission submission,
        CancellationToken cancellationToken)
    {
        var request = await _dbContext.SkillLevelUpRequests.SingleOrDefaultAsync(
            item => item.SubmissionId == submission.Id &&
                    item.RequestType == SkillLevelUpMethod.Assessment &&
                    item.RequestStatus == SkillLevelUpRequestStatus.PendingAssessment,
            cancellationToken);
        if (request is null)
        {
            if (await _dbContext.SkillLevelUpRequests.AnyAsync(
                    item => item.SubmissionId == submission.Id,
                    cancellationToken))
                _logger.LogWarning(
                    "Submission {SubmissionId} is linked to a skill level-up request that is no longer pending",
                    submission.Id);
            return;
        }

        var result = request.ApplyAssessmentResult(submission);
        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"Could not apply assessment result to skill level-up request {request.Id}: {result.Message}");

        SkillLevel? levelAfter = null;
        SkillLevel? levelBefore = request.CurrentLevel;
        if (request.RequestStatus == SkillLevelUpRequestStatus.Approved)
        {
            var profile = await _dbContext.SkillProfiles
                .Include(item => item.Records)
                .SingleOrDefaultAsync(item => item.UserId == request.UserId, cancellationToken);
            if (profile is null)
                throw new InvalidOperationException(
                    $"Skill profile for assessed User {request.UserId} was not found.");

            var levelUpResult = profile.ApplyLevelUp(request);
            if (!levelUpResult.IsSuccess)
                throw new InvalidOperationException(
                    $"Could not apply approved skill level-up request {request.Id}: {levelUpResult.Message}");

            levelAfter = (SkillLevel)((int)request.CurrentLevel + 1);
        }

        AddSkillHistory(
            userId: request.UserId,
            skillId: request.SkillId,
            changeSource: SkillChangeSource.UserInitiated,
            responsibleAccountId: request.UserId,
            outcome: request.RequestStatus == SkillLevelUpRequestStatus.Approved
                ? SkillHistoryOutcome.Applied
                : SkillHistoryOutcome.Rejected,
            levelBefore: levelBefore,
            levelAfter: levelAfter,
            isNewlyAdded: false,
            relatedSubmissionId: submission.Id,
            relatedRequestId: request.Id,
            rejectionReason: request.RejectionReason);
    }

    private async Task ApplyCompanySkillReflectionAsync(
        Submission submission,
        CancellationToken cancellationToken)
    {
        var request = await _dbContext.CompanySkillReflectionRequests.SingleOrDefaultAsync(
            item => item.VerificationSubmissionId == submission.Id &&
                    item.RequestStatus == SkillReflectionStatus.PendingVerification,
            cancellationToken);
        if (request is null)
        {
            if (await _dbContext.CompanySkillReflectionRequests.AnyAsync(
                    item => item.VerificationSubmissionId == submission.Id,
                    cancellationToken))
                _logger.LogWarning(
                    "Submission {SubmissionId} is linked to a company skill reflection request that is no longer pending",
                    submission.Id);
            return;
        }

        var profile = await _dbContext.SkillProfiles
            .Include(item => item.Records)
            .SingleOrDefaultAsync(item => item.UserId == request.UserId, cancellationToken);
        if (profile is null)
        {
            var profileResult = SkillProfile.Create(request.UserId);
            if (!profileResult.IsSuccess || profileResult.Data is null)
                throw new InvalidOperationException(
                    $"Could not create SkillProfile for reflected User {request.UserId}: {profileResult.Message}");
            profile = profileResult.Data;
            _dbContext.SkillProfiles.Add(profile);
        }

        var result = request.ApplyVerificationResult(submission, profile);
        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"Could not apply company skill reflection {request.Id} for submission {submission.Id}: {result.Message}");

        var outcome = request.RequestStatus == SkillReflectionStatus.Applied
            ? SkillHistoryOutcome.Applied
            : SkillHistoryOutcome.Rejected;
        var historyResult = SkillHistoryEntry.Record(
            userId: request.UserId,
            skillId: request.SkillId,
            changeSource: SkillChangeSource.CompanyReflection,
            responsibleAccountId: request.ResponsibleAccountId,
            outcome: outcome,
            levelBefore: request.CurrentLevelAtRequest,
            levelAfter: outcome == SkillHistoryOutcome.Applied ? request.ResultLevel : null,
            isNewlyAdded: request.ReflectionType == SkillReflectionType.Add,
            companyId: request.CompanyId,
            projectId: request.ProjectId,
            evidenceDescription: request.EvidenceDescription,
            relatedSubmissionId: submission.Id,
            relatedRequestId: request.Id,
            rejectionReason: request.RejectionReason);
        if (!historyResult.IsSuccess || historyResult.Data is null)
            throw new InvalidOperationException(
                $"Could not record history for company skill reflection {request.Id}: {historyResult.Message}");

        _dbContext.SkillHistoryEntries.Add(historyResult.Data);
    }

    private async Task CreateGradeNotificationAsync(
        Submission submission,
        TestPaper testPaper,
        CancellationToken cancellationToken)
    {
        var notificationResult = Notification.Create(
            submission.UserId,
            "Kết quả bài kiểm tra",
            $"Bài kiểm tra \"{testPaper.Title}\" đã được chấm: {submission.Score:0.##} điểm." +
            (submission.Score >= 5m ? " Bạn đã đạt." : " Bạn chưa đạt."),
            submission.Score >= 5m ? NotificationType.Success : NotificationType.Warning);
        if (!notificationResult.IsSuccess || notificationResult.Data is null)
        {
            _logger.LogError(
                "Could not create grade notification for submission {SubmissionId}: {Reason}",
                submission.Id,
                notificationResult.Message);
            throw new InvalidOperationException(
                $"Could not notify User about graded submission {submission.Id}: {notificationResult.Message}");
        }
        _dbContext.Notifications.Add(notificationResult.Data);
        _dbContext.Notifications.Add(notificationResult.Data);
    }

    private void AddSkillHistory(
        Guid userId,
        Guid skillId,
        SkillChangeSource changeSource,
        Guid responsibleAccountId,
        SkillHistoryOutcome outcome,
        SkillLevel? levelBefore,
        SkillLevel? levelAfter,
        bool isNewlyAdded,
        Guid? relatedSubmissionId,
        Guid? relatedRequestId,
        string? rejectionReason)
    {
        var historyResult = SkillHistoryEntry.Record(
            userId: userId,
            skillId: skillId,
            changeSource: changeSource,
            responsibleAccountId: responsibleAccountId,
            outcome: outcome,
            levelBefore: levelBefore,
            levelAfter: levelAfter,
            isNewlyAdded: isNewlyAdded,
            relatedSubmissionId: relatedSubmissionId,
            relatedRequestId: relatedRequestId,
            rejectionReason: rejectionReason);
        if (!historyResult.IsSuccess || historyResult.Data is null)
            throw new InvalidOperationException(
                $"Could not record skill assessment history for User {userId}, Skill {skillId}: " +
                historyResult.Message);
        _dbContext.SkillHistoryEntries.Add(historyResult.Data);
    }
}
