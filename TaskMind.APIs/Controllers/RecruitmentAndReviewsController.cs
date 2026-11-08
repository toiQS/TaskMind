using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Text.Json;
using TaskMind.ApplicationLayer.Features.Auths;

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
    public IActionResult ListCompanyJobPostings(Guid companyId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListCompanyJobPostings));

    [HttpPost("companies/{companyId:guid}/job-postings")]
    [EndpointSummary("Create Job Posting")]
    public IActionResult CreateJobPosting(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateJobPosting));
    [HttpGet("job-postings/{jobPostingId:guid}")]
    [EndpointSummary("Get Job Posting")]
    [AllowAnonymous]
    public IActionResult GetJobPosting(Guid jobPostingId) => NotImplemented(nameof(GetJobPosting));

    [HttpPatch("job-postings/{jobPostingId:guid}")]
    [EndpointSummary("Update Job Posting")]
    public IActionResult UpdateJobPosting(Guid jobPostingId, [FromBody] JsonElement request) => NotImplemented(nameof(UpdateJobPosting));

    [HttpPost("job-postings/{jobPostingId:guid}/publish")]
    [EndpointSummary("Publish Job Posting")]
    public IActionResult PublishJobPosting(Guid jobPostingId) => NotImplemented(nameof(PublishJobPosting));

    [HttpPost("job-postings/{jobPostingId:guid}/close")]
    [EndpointSummary("Close Job Posting")]
    public IActionResult CloseJobPosting(Guid jobPostingId) => NotImplemented(nameof(CloseJobPosting));

    [HttpPost("job-postings/{jobPostingId:guid}/cancel")]
    [EndpointSummary("Cancel Job Posting")]
    public IActionResult CancelJobPosting(Guid jobPostingId, [FromBody] JsonElement request) => NotImplemented(nameof(CancelJobPosting));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/job-postings/{jobPostingId:guid}/cancel")]
    [EndpointSummary("Admin Cancel Job Posting")]
    public IActionResult AdminCancelJobPosting(Guid jobPostingId, [FromBody] JsonElement request) => NotImplemented(nameof(AdminCancelJobPosting));

    [HttpPost("job-postings/{jobPostingId:guid}/applications")]
    [EndpointSummary("Apply To Job")]
    public IActionResult ApplyToJob(Guid jobPostingId, [FromBody] JsonElement request) => NotImplemented(nameof(ApplyToJob));

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
    public IActionResult ListJobApplications(Guid jobPostingId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListJobApplications));

    [HttpPost("job-applications/{applicationId:guid}/status-transitions")]
    [EndpointSummary("Transition Job Application")]
    public IActionResult TransitionJobApplication(Guid applicationId, [FromBody] JsonElement request) => NotImplemented(nameof(TransitionJobApplication));

    [HttpPost("job-applications/{applicationId:guid}/staff-invitations")]
    [EndpointSummary("Invite Successful Applicant")]
    public IActionResult InviteSuccessfulApplicant(Guid applicationId, [FromBody] JsonElement request) => NotImplemented(nameof(InviteSuccessfulApplicant));

    [HttpPost("reviews")]
    [EndpointSummary("Create Review")]
    public IActionResult CreateReview([FromBody] JsonElement request) => NotImplemented(nameof(CreateReview));
    [HttpGet("users/{userId:guid}/reviews")]
    [EndpointSummary("List User Reviews")]
    [AllowAnonymous]
    public IActionResult ListUserReviews(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListUserReviews));
    [HttpGet("companies/{companyId:guid}/reviews")]
    [EndpointSummary("List Company Reviews")]
    [AllowAnonymous]
    public IActionResult ListCompanyReviews(Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListCompanyReviews));
    [HttpGet("schools/{schoolId:guid}/reviews")]
    [EndpointSummary("List School Reviews")]
    [AllowAnonymous]
    public IActionResult ListSchoolReviews(Guid schoolId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListSchoolReviews));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/reviews/{reviewId:guid}/removal")]
    [EndpointSummary("Remove Review")]
    public IActionResult RemoveReview(Guid reviewId, [FromBody] JsonElement request) => NotImplemented(nameof(RemoveReview));
}
