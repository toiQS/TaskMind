using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace TaskMind.APIs.Controllers;

public sealed class ProjectsAndCoursesController : ScaffoldControllerBase
{
    [HttpGet("projects")]
    [EndpointSummary("List Projects")]
    [AllowAnonymous]
    public IActionResult ListProjects([FromQuery] string? source, [FromQuery] string? status, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListProjects));

    [HttpPost("projects")]
    [EndpointSummary("Create Project")]
    public IActionResult CreateProject([FromBody] JsonElement request) => NotImplemented(nameof(CreateProject));
    [HttpGet("projects/{projectId:guid}")]
    [EndpointSummary("Get Project")]
    [AllowAnonymous]
    public IActionResult GetProject(Guid projectId) => NotImplemented(nameof(GetProject));

    [HttpPatch("projects/{projectId:guid}")]
    [EndpointSummary("Update Project")]
    public IActionResult UpdateProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(UpdateProject));

    [HttpPost("projects/{projectId:guid}/pause")]
    [EndpointSummary("Pause Project")]
    public IActionResult PauseProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(PauseProject));

    [HttpPost("projects/{projectId:guid}/resume")]
    [EndpointSummary("Resume Project")]
    public IActionResult ResumeProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(ResumeProject));

    [HttpPost("projects/{projectId:guid}/complete")]
    [EndpointSummary("Complete Project")]
    public IActionResult CompleteProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(CompleteProject));

    [HttpPost("projects/{projectId:guid}/cancel")]
    [EndpointSummary("Cancel Project")]
    public IActionResult CancelProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(CancelProject));

    [HttpPost("admin/projects/{projectId:guid}/force-pause")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Force Pause Project")]
    public IActionResult ForcePauseProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(ForcePauseProject));

    [HttpPost("admin/projects/{projectId:guid}/force-cancel")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Force Cancel Project")]
    public IActionResult ForceCancelProject(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(ForceCancelProject));

    [HttpGet("projects/{projectId:guid}/members")]
    [EndpointSummary("List Project Members")]
    public IActionResult ListProjectMembers(Guid projectId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListProjectMembers));

    [HttpPost("projects/{projectId:guid}/members")]
    [EndpointSummary("Add Project Member")]
    public IActionResult AddProjectMember(Guid projectId, [FromBody] JsonElement request) => NotImplemented(nameof(AddProjectMember));

    [HttpPatch("projects/{projectId:guid}/members/{memberId:guid}/role")]
    [EndpointSummary("Change Project Member Role")]
    public IActionResult ChangeProjectMemberRole(Guid projectId, Guid memberId, [FromBody] JsonElement request) => NotImplemented(nameof(ChangeProjectMemberRole));

    [HttpPost("projects/{projectId:guid}/members/{memberId:guid}/leave")]
    [EndpointSummary("Leave Project")]
    public IActionResult LeaveProject(Guid projectId, Guid memberId) => NotImplemented(nameof(LeaveProject));

    [HttpPost("admin/projects/{projectId:guid}/members/{memberId:guid}/remove")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Force Remove Project Member")]
    public IActionResult ForceRemoveProjectMember(Guid projectId, Guid memberId, [FromBody] JsonElement request) => NotImplemented(nameof(ForceRemoveProjectMember));

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
