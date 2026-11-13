using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using MediatR;
using TaskMind.ApplicationLayer.Features.Projects;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.APIs.Controllers;

public sealed class ProjectsAndCoursesController : ScaffoldControllerBase
{
    private readonly ISender _sender;

    public ProjectsAndCoursesController(ISender sender) => _sender = sender;

    [HttpGet("projects")]
    [EndpointSummary("List Projects")]
    [AllowAnonymous]
    public async Task<IActionResult> ListProjects([FromQuery] string? source, [FromQuery] string? status, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListProjectsQuery(source, status, keyword, page, pageSize), cancellationToken));

    [HttpPost("projects")]
    [EndpointSummary("Create Project")]
    public async Task<IActionResult> CreateProject([FromBody] ProjectRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateProjectCommand(request), cancellationToken));
    [HttpGet("projects/{projectId:guid}")]
    [EndpointSummary("Get Project")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProject(Guid projectId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetProjectQuery(projectId), cancellationToken));

    [HttpPatch("projects/{projectId:guid}")]
    [EndpointSummary("Update Project")]
    public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new UpdateProjectCommand(projectId, request.Title, request.Description), cancellationToken));

    [HttpPost("projects/{projectId:guid}/pause")]
    [EndpointSummary("Pause Project")]
    public async Task<IActionResult> PauseProject(Guid projectId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProjectStatusCommand(projectId, ProjectStatus.Paused, false, request.Reason), cancellationToken));

    [HttpPost("projects/{projectId:guid}/resume")]
    [EndpointSummary("Resume Project")]
    public async Task<IActionResult> ResumeProject(Guid projectId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProjectStatusCommand(projectId, ProjectStatus.InProgress, false, request.Reason), cancellationToken));

    [HttpPost("projects/{projectId:guid}/complete")]
    [EndpointSummary("Complete Project")]
    public async Task<IActionResult> CompleteProject(Guid projectId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProjectStatusCommand(projectId, ProjectStatus.Completed, false, request.Reason), cancellationToken));

    [HttpPost("projects/{projectId:guid}/cancel")]
    [EndpointSummary("Cancel Project")]
    public async Task<IActionResult> CancelProject(Guid projectId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProjectStatusCommand(projectId, ProjectStatus.Cancelled, false, request.Reason), cancellationToken));

    [HttpPost("admin/projects/{projectId:guid}/force-pause")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Force Pause Project")]
    public async Task<IActionResult> ForcePauseProject(Guid projectId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProjectStatusCommand(projectId, ProjectStatus.Paused, true, request.Reason), cancellationToken));

    [HttpPost("admin/projects/{projectId:guid}/force-cancel")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Force Cancel Project")]
    public async Task<IActionResult> ForceCancelProject(Guid projectId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ProjectStatusCommand(projectId, ProjectStatus.Cancelled, true, request.Reason), cancellationToken));

    [HttpGet("projects/{projectId:guid}/members")]
    [EndpointSummary("List Project Members")]
    public async Task<IActionResult> ListProjectMembers(Guid projectId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListProjectMembersQuery(projectId, status, page, pageSize), cancellationToken));

    [HttpPost("projects/{projectId:guid}/members")]
    [EndpointSummary("Add Project Member")]
    public async Task<IActionResult> AddProjectMember(Guid projectId, [FromBody] ProjectMemberRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new AddProjectMemberCommand(projectId, request.AccountId, request.Role), cancellationToken));

    [HttpPatch("projects/{projectId:guid}/members/{memberId:guid}/role")]
    [EndpointSummary("Change Project Member Role")]
    public async Task<IActionResult> ChangeProjectMemberRole(Guid projectId, Guid memberId, [FromBody] ProjectMemberRoleRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ChangeProjectMemberRoleCommand(projectId, memberId, request.Role), cancellationToken));

    [HttpPost("projects/{projectId:guid}/members/{memberId:guid}/leave")]
    [EndpointSummary("Leave Project")]
    public async Task<IActionResult> LeaveProject(Guid projectId, Guid memberId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new LeaveProjectCommand(projectId, memberId), cancellationToken));

    [HttpPost("admin/projects/{projectId:guid}/members/{memberId:guid}/remove")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Force Remove Project Member")]
    public async Task<IActionResult> ForceRemoveProjectMember(Guid projectId, Guid memberId, [FromBody] ProjectReasonRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ForceRemoveProjectMemberCommand(projectId, memberId, request.Reason), cancellationToken));

    [HttpPost("projects/{projectId:guid}/repository")]
    [EndpointSummary("Create Project Resource Repository")]
    public async Task<IActionResult> CreateProjectRepository(
        Guid projectId, [FromBody] CreateProjectRepositoryRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateProjectRepositoryCommand(projectId, request), cancellationToken));

    [HttpGet("projects/{projectId:guid}/repository")]
    [EndpointSummary("Get Project Resource Repository")]
    public async Task<IActionResult> GetProjectRepository(Guid projectId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetProjectRepositoryQuery(projectId), cancellationToken));

    [HttpGet("projects/{projectId:guid}/repository/revisions")]
    [EndpointSummary("List Project Revisions")]
    public async Task<IActionResult> ListProjectRevisions(
        Guid projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListProjectRevisionsQuery(projectId, page, pageSize), cancellationToken));

    [HttpGet("projects/{projectId:guid}/repository/promotions")]
    [EndpointSummary("List Revision Promotions")]
    public async Task<IActionResult> ListRevisionPromotions(
        Guid projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListRevisionPromotionsQuery(projectId, page, pageSize), cancellationToken));

    [HttpPost("projects/{projectId:guid}/repository/workspaces")]
    [EndpointSummary("Register Project Workspace")]
    public async Task<IActionResult> RegisterProjectWorkspace(
        Guid projectId, [FromBody] RegisterProjectWorkspaceRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new RegisterProjectWorkspaceCommand(projectId, request), cancellationToken));

    [HttpPost("projects/{projectId:guid}/repository/workspaces/{workspaceId:guid}/pull")]
    [EndpointSummary("Pull Project Workspace Snapshot")]
    public async Task<IActionResult> PullProjectWorkspace(
        Guid projectId, Guid workspaceId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new PullProjectWorkspaceCommand(projectId, workspaceId), cancellationToken));

    [HttpPost("projects/{projectId:guid}/repository/revisions")]
    [EndpointSummary("Push Project Revision Metadata")]
    public async Task<IActionResult> PushProjectRevision(
        Guid projectId, [FromBody] PushProjectRevisionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new PushProjectRevisionCommand(projectId, request), cancellationToken));

    [HttpPost("projects/{projectId:guid}/repository/revisions/{revisionId:guid}/reviews")]
    [EndpointSummary("Review Project Revision")]
    public async Task<IActionResult> ReviewProjectRevision(
        Guid projectId, Guid revisionId, [FromBody] ReviewProjectRevisionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new ReviewProjectRevisionCommand(projectId, revisionId, request), cancellationToken));

    [HttpPost("projects/{projectId:guid}/repository/promote")]
    [EndpointSummary("Promote Project Revision")]
    public async Task<IActionResult> PromoteProjectRevision(
        Guid projectId, [FromBody] PromoteProjectRevisionRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new PromoteProjectRevisionCommand(projectId, request), cancellationToken));

    [HttpPost("admin/projects/{projectId:guid}/repository/revisions/{revisionId:guid}/redact")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Redact Project Revision Metadata")]
    public async Task<IActionResult> RedactProjectRevision(
        Guid projectId, Guid revisionId, [FromBody] RedactProjectRevisionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new RedactProjectRevisionCommand(projectId, revisionId, request), cancellationToken));

    [HttpDelete("projects/{projectId:guid}/repository/workspaces/{workspaceId:guid}")]
    [EndpointSummary("Revoke Project Workspace")]
    public async Task<IActionResult> RevokeProjectWorkspace(
        Guid projectId, Guid workspaceId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new RevokeProjectWorkspaceCommand(projectId, workspaceId), cancellationToken));

    [HttpPost("projects/{projectId:guid}/course-links")]
    [EndpointSummary("Link Course")]
    public IActionResult LinkCourse(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(LinkCourse));

    [HttpDelete("projects/{projectId:guid}/course-links/{courseId:guid}")]
    [EndpointSummary("Unlink Course")]
    public IActionResult UnlinkCourse(Guid projectId, Guid courseId) => NotImplemented(nameof(UnlinkCourse));

    [HttpGet("schools/{schoolId:guid}/courses")]
    [EndpointSummary("List Courses")]
    [AllowAnonymous]
    public IActionResult ListCourses(Guid schoolId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListCourses));

    [HttpPost("schools/{schoolId:guid}/courses")]
    [EndpointSummary("Create Course")]
    public IActionResult CreateCourse(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateCourse));

    [HttpGet("courses/{courseId:guid}")]
    [EndpointSummary("Get Course")]
    [AllowAnonymous]
    public IActionResult GetCourse(Guid courseId) => NotImplemented(nameof(GetCourse));

    [HttpPatch("courses/{courseId:guid}")]
    [EndpointSummary("Update Course")]
    public IActionResult UpdateCourse(Guid courseId, [FromBody] JsonElement request) => NotImplemented(nameof(UpdateCourse));

    [HttpPut("courses/{courseId:guid}/teacher")]
    [EndpointSummary("Assign Course Teacher")]
    public IActionResult AssignCourseTeacher(Guid courseId, [FromBody] JsonElement request) => NotImplemented(nameof(AssignCourseTeacher));

    [HttpPost("courses/{courseId:guid}/students")]
    [EndpointSummary("Add Course Student")]
    public IActionResult AddCourseStudent(Guid courseId, [FromBody] JsonElement request) => NotImplemented(nameof(AddCourseStudent));

    [HttpDelete("courses/{courseId:guid}/students/{studentId:guid}")]
    [EndpointSummary("Remove Course Student")]
    public IActionResult RemoveCourseStudent(Guid courseId, Guid studentId) => NotImplemented(nameof(RemoveCourseStudent));
}
