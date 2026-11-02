using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace TaskMind.APIs.Controllers;

public sealed class SkillsController : ScaffoldControllerBase
{
    [HttpGet("skills")]
    [EndpointSummary("List Skills")]
    [AllowAnonymous]
    public IActionResult ListSkills([FromQuery] string? keyword, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListSkills));

    [HttpPost("skill-proposals")]
    [EndpointSummary("Propose Skill")]
    public IActionResult ProposeSkill([FromBody] JsonElement request) => NotImplemented(nameof(ProposeSkill));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/skill-proposals")]
    [EndpointSummary("List Skill Proposals")]
    public IActionResult ListSkillProposals([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListSkillProposals));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/skill-proposals/{proposalId:guid}/approve")]
    [EndpointSummary("Approve Skill Proposal")]
    public IActionResult ApproveSkillProposal(Guid proposalId, [FromBody] JsonElement request) => NotImplemented(nameof(ApproveSkillProposal));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/skill-proposals/{proposalId:guid}/reject")]
    [EndpointSummary("Reject Skill Proposal")]
    public IActionResult RejectSkillProposal(Guid proposalId, [FromBody] JsonElement request) => NotImplemented(nameof(RejectSkillProposal));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/skills/{skillId:guid}/related-skills")]
    [EndpointSummary("Link Related Skills")]
    public IActionResult LinkRelatedSkills(Guid skillId, [FromBody] JsonElement request) => NotImplemented(nameof(LinkRelatedSkills));

    [HttpGet("me/skills")]
    [EndpointSummary("List My Skills")]
    public IActionResult ListMySkills() => NotImplemented(nameof(ListMySkills));

    [HttpPost("me/skills")]
    [EndpointSummary("Add My Skill")]
    public IActionResult AddMySkill([FromBody] JsonElement request) => NotImplemented(nameof(AddMySkill));

    [HttpGet("me/skill-history")]
    [EndpointSummary("Get My Skill History")]
    public IActionResult GetMySkillHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(GetMySkillHistory));

    [HttpPost("me/skill-level-up-requests")]
    [EndpointSummary("Create Skill Level Up Request")]
    public IActionResult CreateSkillLevelUpRequest([FromBody] JsonElement request) => NotImplemented(nameof(CreateSkillLevelUpRequest));

    [HttpGet("me/skill-level-up-requests")]
    [EndpointSummary("List My Skill Level Up Requests")]
    public IActionResult ListMySkillLevelUpRequests([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListMySkillLevelUpRequests));

    [HttpGet("me/skill-level-up-requests/{requestId:guid}")]
    [EndpointSummary("Get My Skill Level Up Request")]
    public IActionResult GetMySkillLevelUpRequest(Guid requestId) => NotImplemented(nameof(GetMySkillLevelUpRequest));

    [HttpPost("me/skill-level-up-requests/{requestId:guid}/assessment-submissions")]
    [EndpointSummary("Link Skill Assessment Submission")]
    public IActionResult LinkSkillAssessmentSubmission(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(LinkSkillAssessmentSubmission));

    [HttpPost("skill-level-up-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve Skill Level Up Request")]
    public IActionResult ApproveSkillLevelUpRequest(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(ApproveSkillLevelUpRequest));

    [HttpPost("skill-level-up-requests/{requestId:guid}/reject")]
    [EndpointSummary("Reject Skill Level Up Request")]
    public IActionResult RejectSkillLevelUpRequest(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(RejectSkillLevelUpRequest));

    [HttpPost("companies/{companyId:guid}/staff/{staffId:guid}/skill-reflection-requests")]
    [EndpointSummary("Create Skill Reflection Request")]
    public IActionResult CreateSkillReflectionRequest(Guid companyId, Guid staffId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateSkillReflectionRequest));

    [HttpGet("companies/{companyId:guid}/skill-reflection-requests")]
    [EndpointSummary("List Skill Reflection Requests")]
    public IActionResult ListSkillReflectionRequests(Guid companyId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListSkillReflectionRequests));

    [HttpGet("skill-reflection-requests/{requestId:guid}")]
    [EndpointSummary("Get Skill Reflection Request")]
    public IActionResult GetSkillReflectionRequest(Guid requestId) => NotImplemented(nameof(GetSkillReflectionRequest));

    [HttpPost("skill-reflection-requests/{requestId:guid}/test-paper-assignments")]
    [EndpointSummary("Assign Reflection Test Paper")]
    public IActionResult AssignReflectionTestPaper(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(AssignReflectionTestPaper));

    [HttpPost("skill-reflection-requests/{requestId:guid}/assessment-submissions")]
    [EndpointSummary("Link Reflection Assessment Submission")]
    public IActionResult LinkReflectionAssessmentSubmission(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(LinkReflectionAssessmentSubmission));

    [HttpPost("skill-reflection-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve Skill Reflection Request")]
    public IActionResult ApproveSkillReflectionRequest(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(ApproveSkillReflectionRequest));

    [HttpPost("skill-reflection-requests/{requestId:guid}/dismiss")]
    [EndpointSummary("Dismiss Skill Reflection Request")]
    public IActionResult DismissSkillReflectionRequest(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(DismissSkillReflectionRequest));
}
