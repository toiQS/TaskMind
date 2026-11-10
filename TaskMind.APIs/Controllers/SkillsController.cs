using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TaskMind.ApplicationLayer.Features.Auths;
using TaskMind.ApplicationLayer.Features.Skills;

namespace TaskMind.APIs.Controllers;

public sealed class SkillsController : ScaffoldControllerBase
{
    private readonly ISender _sender;

    public SkillsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("skills")]
    [EndpointSummary("List Skills")]
    [AllowAnonymous]
    public async Task<IActionResult> ListSkills(
        [FromQuery] string? keyword,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new GetSkillCatalogQuery(keyword, category, page, pageSize),
            cancellationToken));

    [HttpPost("skill-proposals")]
    [EndpointSummary("Propose Skill")]
    public async Task<IActionResult> ProposeSkill(
        [FromBody] SkillProposalRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProposeSkillCommand(request), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/skill-proposals")]
    [EndpointSummary("List Skill Proposals")]
    public async Task<IActionResult> ListSkillProposals(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListSkillProposalsQuery(status, page, pageSize), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/skill-proposals/{proposalId:guid}/approve")]
    [EndpointSummary("Approve Skill Proposal")]
    public async Task<IActionResult> ApproveSkillProposal(
        Guid proposalId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new DecideSkillProposalCommand(proposalId, true, null), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/skill-proposals/{proposalId:guid}/reject")]
    [EndpointSummary("Reject Skill Proposal")]
    public async Task<IActionResult> RejectSkillProposal(
        Guid proposalId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new DecideSkillProposalCommand(proposalId, false, null), cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/skills/{skillId:guid}/related-skills")]
    [EndpointSummary("Link Related Skills")]
    public async Task<IActionResult> LinkRelatedSkills(
        Guid skillId,
        [FromBody] RelatedSkillsRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new LinkRelatedSkillsCommand(skillId, request.SkillIds), cancellationToken));

    [HttpGet("me/skills")]
    [EndpointSummary("List My Skills")]
    public async Task<IActionResult> ListMySkills(CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetMySkillsQuery(), cancellationToken));

    [HttpPost("me/skills")]
    [EndpointSummary("Add My Skill")]
    public async Task<IActionResult> AddMySkill(
        [FromBody] AddMySkillCommand request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(request, cancellationToken));

    [HttpGet("me/skill-history")]
    [EndpointSummary("Get My Skill History")]
    public async Task<IActionResult> GetMySkillHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new GetMySkillHistoryQuery(page, pageSize), cancellationToken));

    [HttpPost("me/skill-level-up-requests")]
    [EndpointSummary("Create Skill Level Up Request")]
    public async Task<IActionResult> CreateSkillLevelUpRequest(
        [FromBody] SkillLevelUpRequestBody request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateSkillLevelUpRequestCommand(request), cancellationToken));

    [HttpGet("me/skill-level-up-requests")]
    [EndpointSummary("List My Skill Level Up Requests")]
    public async Task<IActionResult> ListMySkillLevelUpRequests(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListMySkillLevelUpRequestsQuery(status, page, pageSize),
            cancellationToken));

    [HttpGet("me/skill-level-up-requests/{requestId:guid}")]
    [EndpointSummary("Get My Skill Level Up Request")]
    public async Task<IActionResult> GetMySkillLevelUpRequest(
        Guid requestId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetMySkillLevelUpRequestQuery(requestId), cancellationToken));

    [HttpPost("me/skill-level-up-requests/{requestId:guid}/assessment-submissions")]
    [EndpointSummary("Link Skill Assessment Submission")]
    public async Task<IActionResult> LinkSkillAssessmentSubmission(
        Guid requestId,
        [FromBody] LinkSkillAssessmentSubmissionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new LinkSkillAssessmentSubmissionCommand(requestId, request.SubmissionId),
            cancellationToken));

    [HttpPost("skill-level-up-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve Skill Level Up Request")]
    public async Task<IActionResult> ApproveSkillLevelUpRequest(
        Guid requestId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new DecideSkillLevelUpRequestCommand(requestId, true, null), cancellationToken));

    [HttpPost("skill-level-up-requests/{requestId:guid}/reject")]
    [EndpointSummary("Reject Skill Level Up Request")]
    public async Task<IActionResult> RejectSkillLevelUpRequest(
        Guid requestId,
        [FromBody] SkillDecisionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new DecideSkillLevelUpRequestCommand(requestId, false, request.Reason), cancellationToken));

    [HttpPost("companies/{companyId:guid}/staff/{staffId:guid}/skill-reflection-requests")]
    [EndpointSummary("Create Skill Reflection Request")]
    public async Task<IActionResult> CreateSkillReflectionRequest(
        Guid companyId,
        Guid staffId,
        [FromBody] SkillReflectionRequestBody request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new CreateSkillReflectionRequestCommand(companyId, staffId, request), cancellationToken));

    [HttpGet("companies/{companyId:guid}/skill-reflection-requests")]
    [EndpointSummary("List Skill Reflection Requests")]
    public async Task<IActionResult> ListSkillReflectionRequests(
        Guid companyId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListSkillReflectionRequestsQuery(companyId, status, page, pageSize), cancellationToken));

    [HttpGet("skill-reflection-requests/{requestId:guid}")]
    [EndpointSummary("Get Skill Reflection Request")]
    public async Task<IActionResult> GetSkillReflectionRequest(
        Guid requestId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetSkillReflectionRequestQuery(requestId), cancellationToken));

    [HttpPost("skill-reflection-requests/{requestId:guid}/test-paper-assignments")]
    [EndpointSummary("Assign Reflection Test Paper")]
    public async Task<IActionResult> AssignReflectionTestPaper(
        Guid requestId,
        [FromBody] SkillReflectionTestPaperRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new AssignReflectionTestPaperCommand(requestId, request.TestPaperId), cancellationToken));

    [HttpPost("skill-reflection-requests/{requestId:guid}/assessment-submissions")]
    [EndpointSummary("Link Reflection Assessment Submission")]
    public async Task<IActionResult> LinkReflectionAssessmentSubmission(
        Guid requestId,
        [FromBody] SkillReflectionSubmissionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new LinkReflectionAssessmentSubmissionCommand(requestId, request.SubmissionId), cancellationToken));

    [HttpPost("skill-reflection-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve Skill Reflection Request")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveSkillReflectionRequest(
        Guid requestId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new ReviewSkillReflectionRequestCommand(requestId, true, null), cancellationToken));

    [HttpPost("skill-reflection-requests/{requestId:guid}/dismiss")]
    [EndpointSummary("Dismiss Skill Reflection Request")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DismissSkillReflectionRequest(
        Guid requestId,
        [FromBody] SkillDecisionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new ReviewSkillReflectionRequestCommand(requestId, false, request.Reason), cancellationToken));

}

public sealed record LinkSkillAssessmentSubmissionRequest(Guid SubmissionId);
