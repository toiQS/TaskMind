using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace TaskMind.APIs.Controllers;

public sealed class OrganizationsController : ScaffoldControllerBase
{
    [HttpPost("company-registration-requests")]
    [EndpointSummary("Request Company Registration")]
    public IActionResult RequestCompanyRegistration([FromBody] JsonElement request) => NotImplemented(nameof(RequestCompanyRegistration));

    [HttpPost("school-registration-requests")]
    [EndpointSummary("Request School Registration")]
    public IActionResult RequestSchoolRegistration([FromBody] JsonElement request) => NotImplemented(nameof(RequestSchoolRegistration));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/company-registration-requests")]
    [EndpointSummary("List Company Registration Requests")]
    public IActionResult ListCompanyRegistrationRequests([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListCompanyRegistrationRequests));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/school-registration-requests")]
    [EndpointSummary("List School Registration Requests")]
    public IActionResult ListSchoolRegistrationRequests([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListSchoolRegistrationRequests));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/company-registration-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve Company Registration")]
    public IActionResult ApproveCompanyRegistration(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(ApproveCompanyRegistration));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/company-registration-requests/{requestId:guid}/reject")]
    [EndpointSummary("Reject Company Registration")]
    public IActionResult RejectCompanyRegistration(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(RejectCompanyRegistration));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/school-registration-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve School Registration")]
    public IActionResult ApproveSchoolRegistration(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(ApproveSchoolRegistration));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/school-registration-requests/{requestId:guid}/reject")]
    [EndpointSummary("Reject School Registration")]
    public IActionResult RejectSchoolRegistration(Guid requestId, [FromBody] JsonElement request) => NotImplemented(nameof(RejectSchoolRegistration));
    [HttpGet("companies/{companyId:guid}")]
    [EndpointSummary("Get Company")]
    [AllowAnonymous]
    public IActionResult GetCompany(Guid companyId) => NotImplemented(nameof(GetCompany));

    [HttpPatch("companies/{companyId:guid}")]
    [EndpointSummary("Update Company")]
    public IActionResult UpdateCompany(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(UpdateCompany));

    [HttpPost("companies/{companyId:guid}/suspension")]
    [EndpointSummary("Suspend Company")]
    public IActionResult SuspendCompany(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(SuspendCompany));

    [HttpPost("companies/{companyId:guid}/reactivation")]
    [EndpointSummary("Reactivate Company")]
    public IActionResult ReactivateCompany(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(ReactivateCompany));

    [HttpPost("companies/{companyId:guid}/membership-changes")]
    [EndpointSummary("Change Company Membership")]
    public IActionResult ChangeCompanyMembership(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(ChangeCompanyMembership));
    [HttpGet("schools/{schoolId:guid}")]
    [EndpointSummary("Get School")]
    [AllowAnonymous]
    public IActionResult GetSchool(Guid schoolId) => NotImplemented(nameof(GetSchool));

    [HttpPatch("schools/{schoolId:guid}")]
    [EndpointSummary("Update School")]
    public IActionResult UpdateSchool(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(UpdateSchool));

    [HttpPost("schools/{schoolId:guid}/suspension")]
    [EndpointSummary("Suspend School")]
    public IActionResult SuspendSchool(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(SuspendSchool));

    [HttpPost("schools/{schoolId:guid}/reactivation")]
    [EndpointSummary("Reactivate School")]
    public IActionResult ReactivateSchool(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(ReactivateSchool));

    [HttpPost("schools/{schoolId:guid}/membership-changes")]
    [EndpointSummary("Change School Membership")]
    public IActionResult ChangeSchoolMembership(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(ChangeSchoolMembership));

    [HttpPost("companies/{companyId:guid}/staff-invitations")]
    [EndpointSummary("Invite Staff")]
    public IActionResult InviteStaff(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(InviteStaff));

    [HttpPost("schools/{schoolId:guid}/teacher-invitations")]
    [EndpointSummary("Invite Teacher")]
    public IActionResult InviteTeacher(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(InviteTeacher));

    [HttpPost("schools/{schoolId:guid}/student-enrollments")]
    [EndpointSummary("Enroll Student")]
    public IActionResult EnrollStudent(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(EnrollStudent));

    [HttpPost("invitations/{invitationToken}/accept")]
    [EndpointSummary("Accept Invitation")]
    public IActionResult AcceptInvitation(string invitationToken, [FromBody] JsonElement request) => NotImplemented(nameof(AcceptInvitation));

    [HttpGet("companies/{companyId:guid}/staff")]
    [EndpointSummary("List Staff")]
    public IActionResult ListStaff(Guid companyId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListStaff));

    [HttpPost("companies/{companyId:guid}/staff/{staffId:guid}/deactivation")]
    [EndpointSummary("Deactivate Staff")]
    public IActionResult DeactivateStaff(Guid companyId, Guid staffId, [FromBody] JsonElement request) => NotImplemented(nameof(DeactivateStaff));

    [HttpPost("schools/{schoolId:guid}/teachers/{teacherId:guid}/deactivation")]
    [EndpointSummary("Deactivate Teacher")]
    public IActionResult DeactivateTeacher(Guid schoolId, Guid teacherId, [FromBody] JsonElement request) => NotImplemented(nameof(DeactivateTeacher));

    [HttpPost("schools/{schoolId:guid}/students/{studentId:guid}/deactivation")]
    [EndpointSummary("Deactivate Student")]
    public IActionResult DeactivateStudent(Guid schoolId, Guid studentId, [FromBody] JsonElement request) => NotImplemented(nameof(DeactivateStudent));

    [HttpPost("schools/{schoolId:guid}/students/{studentId:guid}/transfers")]
    [EndpointSummary("Transfer Student")]
    public IActionResult TransferStudent(Guid schoolId, Guid studentId, [FromBody] JsonElement request) => NotImplemented(nameof(TransferStudent));
}
