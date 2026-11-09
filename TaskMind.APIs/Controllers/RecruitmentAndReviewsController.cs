using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TaskMind.ApplicationLayer.Features.Auths;
using TaskMind.ApplicationLayer.Features.Recruitment;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.APIs.Controllers;

public sealed class RecruitmentAndReviewsController : ScaffoldControllerBase
{
    private readonly ISender _sender;

    public RecruitmentAndReviewsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("companies/{companyId:guid}/job-postings")]
    [EndpointSummary("List Company Job Postings")]
    [AllowAnonymous]
    public async Task<IActionResult> ListCompanyJobPostings(Guid companyId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListCompanyJobPostingsQuery(companyId, status, page, pageSize), cancellationToken));

    [HttpPost("companies/{companyId:guid}/job-postings")]
    [EndpointSummary("Create Job Posting")]
    public async Task<IActionResult> CreateJobPosting(Guid companyId, [FromBody] JobPostingRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateJobPostingCommand(companyId, request), cancellationToken));
    [HttpGet("job-postings/{jobPostingId:guid}")]
    [EndpointSummary("Get Job Posting")]
    [AllowAnonymous]
    public async Task<IActionResult> GetJobPosting(Guid jobPostingId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetJobPostingQuery(jobPostingId), cancellationToken));

    [HttpPatch("job-postings/{jobPostingId:guid}")]
    [EndpointSummary("Update Job Posting")]
    public async Task<IActionResult> UpdateJobPosting(Guid jobPostingId, [FromBody] JobPostingRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new UpdateJobPostingCommand(jobPostingId, request), cancellationToken));

    [HttpPost("job-postings/{jobPostingId:guid}/publish")]
    [EndpointSummary("Publish Job Posting")]
    public async Task<IActionResult> PublishJobPosting(Guid jobPostingId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new PublishJobPostingCommand(jobPostingId), cancellationToken));

    [HttpPost("job-postings/{jobPostingId:guid}/close")]
    [EndpointSummary("Close Job Posting")]
    public async Task<IActionResult> CloseJobPosting(Guid jobPostingId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CloseJobPostingCommand(jobPostingId), cancellationToken));

    [HttpPost("job-postings/{jobPostingId:guid}/cancel")]
    [EndpointSummary("Cancel Job Posting")]
    public async Task<IActionResult> CancelJobPosting(Guid jobPostingId, [FromBody] CancelJobPostingRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CancelJobPostingCommand(jobPostingId, request.Reason, false), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/job-postings/{jobPostingId:guid}/cancel")]
    [EndpointSummary("Admin Cancel Job Posting")]
    public async Task<IActionResult> AdminCancelJobPosting(Guid jobPostingId, [FromBody] CancelJobPostingRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CancelJobPostingCommand(jobPostingId, request.Reason, true), cancellationToken));

    [HttpPost("job-postings/{jobPostingId:guid}/applications")]
    [EndpointSummary("Apply To Job")]
    public async Task<IActionResult> ApplyToJob(Guid jobPostingId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ApplyToJobCommand(jobPostingId), cancellationToken));

    [HttpGet("me/job-applications")]
    [EndpointSummary("List My Job Applications")]
    public async Task<IActionResult> ListMyJobApplications(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListMyJobApplicationsQuery(status, page, pageSize), cancellationToken));

    [HttpGet("job-postings/{jobPostingId:guid}/applications")]
    [EndpointSummary("List Job Applications")]
    public async Task<IActionResult> ListJobApplications(Guid jobPostingId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListJobApplicationsQuery(jobPostingId, status, page, pageSize), cancellationToken));

    [HttpPost("job-applications/{applicationId:guid}/status-transitions")]
    [EndpointSummary("Transition Job Application")]
    public async Task<IActionResult> TransitionJobApplication(Guid applicationId, [FromBody] TransitionJobApplicationRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new TransitionJobApplicationCommand(applicationId, request.Status), cancellationToken));

    [HttpPost("job-applications/{applicationId:guid}/staff-invitations")]
    [EndpointSummary("Invite Successful Applicant")]
    public IActionResult InviteSuccessfulApplicant(Guid applicationId, [FromBody] System.Text.Json.JsonElement request) => NotImplemented(nameof(InviteSuccessfulApplicant));

    [HttpPost("reviews")]
    [EndpointSummary("Create Review")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateReviewCommand(request), cancellationToken));
    [HttpGet("users/{userId:guid}/reviews")]
    [EndpointSummary("List User Reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> ListUserReviews(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListReviewsQuery(ReviewTargetType.User, userId, page, pageSize), cancellationToken));
    [HttpGet("companies/{companyId:guid}/reviews")]
    [EndpointSummary("List Company Reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> ListCompanyReviews(Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListReviewsQuery(ReviewTargetType.Company, companyId, page, pageSize), cancellationToken));
    [HttpGet("schools/{schoolId:guid}/reviews")]
    [EndpointSummary("List School Reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> ListSchoolReviews(Guid schoolId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListReviewsQuery(ReviewTargetType.School, schoolId, page, pageSize), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/reviews/{reviewId:guid}/removal")]
    [EndpointSummary("Remove Review")]
    public async Task<IActionResult> RemoveReview(Guid reviewId, [FromBody] RemoveReviewRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new RemoveReviewCommand(reviewId, request.Reason), cancellationToken));
}

public sealed record CancelJobPostingRequest(string? Reason);
public sealed record TransitionJobApplicationRequest(string Status);
public sealed record RemoveReviewRequest(string? Reason);
