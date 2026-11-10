using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using TaskMind.ApplicationLayer.Features.Organizations;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.APIs.Controllers;

public sealed class OrganizationsController : ScaffoldControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("company-registration-requests")]
    [EndpointSummary("Request Company Registration")]
    public async Task<IActionResult> RequestCompanyRegistration([FromBody] CompanyRegistrationDto request) =>
        ToActionResult(await _mediator.Send(new RequestCompanyRegistrationCommand(request)));

    [HttpPost("school-registration-requests")]
    [EndpointSummary("Request School Registration")]
    public async Task<IActionResult> RequestSchoolRegistration([FromBody] SchoolRegistrationDto request) =>
        ToActionResult(await _mediator.Send(new RequestSchoolRegistrationCommand(request)));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/company-registration-requests")]
    [EndpointSummary("List Company Registration Requests")]
    public async Task<IActionResult> ListCompanyRegistrationRequests([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        ToActionResult(await _mediator.Send(new ListCompanyRegistrationRequestsQuery(status, page, pageSize)));

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/school-registration-requests")]
    [EndpointSummary("List School Registration Requests")]
    public async Task<IActionResult> ListSchoolRegistrationRequests([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        ToActionResult(await _mediator.Send(new ListSchoolRegistrationRequestsQuery(status, page, pageSize)));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/company-registration-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve Company Registration")]
    public async Task<IActionResult> ApproveCompanyRegistration(Guid requestId) =>
        ToActionResult(await _mediator.Send(new ApproveCompanyRegistrationCommand(requestId)));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/company-registration-requests/{requestId:guid}/reject")]
    [EndpointSummary("Reject Company Registration")]
    public async Task<IActionResult> RejectCompanyRegistration(Guid requestId, [FromBody] OrganizationReasonRequest request) =>
        ToActionResult(await _mediator.Send(new RejectCompanyRegistrationCommand(
            requestId, request.Reason)));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/school-registration-requests/{requestId:guid}/approve")]
    [EndpointSummary("Approve School Registration")]
    public async Task<IActionResult> ApproveSchoolRegistration(Guid requestId) =>
        ToActionResult(await _mediator.Send(new ApproveSchoolRegistrationCommand(requestId)));

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/school-registration-requests/{requestId:guid}/reject")]
    [EndpointSummary("Reject School Registration")]
    public async Task<IActionResult> RejectSchoolRegistration(Guid requestId, [FromBody] OrganizationReasonRequest request) =>
        ToActionResult(await _mediator.Send(new RejectSchoolRegistrationCommand(
            requestId, request.Reason)));
    [HttpGet("companies/{companyId:guid}")]
    [EndpointSummary("Get Company")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompany(Guid companyId) =>
        ToActionResult(await _mediator.Send(new GetCompanyQuery(companyId)));

    [HttpPatch("companies/{companyId:guid}")]
    [EndpointSummary("Update Company")]
    public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] OrganizationUpdateDto request) =>
        ToActionResult(await _mediator.Send(new UpdateCompanyCommand(companyId, request)));

    [HttpPost("companies/{companyId:guid}/suspension")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Suspend Company")]
    public async Task<IActionResult> SuspendCompany(Guid companyId) =>
        ToActionResult(await _mediator.Send(new SuspendCompanyCommand(companyId)));

    [HttpPost("companies/{companyId:guid}/reactivation")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Reactivate Company")]
    public async Task<IActionResult> ReactivateCompany(Guid companyId) =>
        ToActionResult(await _mediator.Send(new ReactivateCompanyCommand(companyId)));

    [HttpPost("companies/{companyId:guid}/membership-changes")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Change Company Membership")]
    public async Task<IActionResult> ChangeCompanyMembership(Guid companyId, [FromBody] OrganizationMembershipRequest request) =>
        ToActionResult(await _mediator.Send(new ChangeCompanyMembershipCommand(
            companyId, request.Package)));
    [HttpGet("schools/{schoolId:guid}")]
    [EndpointSummary("Get School")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSchool(Guid schoolId) =>
        ToActionResult(await _mediator.Send(new GetSchoolQuery(schoolId)));

    [HttpPatch("schools/{schoolId:guid}")]
    [EndpointSummary("Update School")]
    public async Task<IActionResult> UpdateSchool(Guid schoolId, [FromBody] OrganizationUpdateDto request) =>
        ToActionResult(await _mediator.Send(new UpdateSchoolCommand(schoolId, request)));

    [HttpPost("schools/{schoolId:guid}/suspension")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Suspend School")]
    public async Task<IActionResult> SuspendSchool(Guid schoolId) =>
        ToActionResult(await _mediator.Send(new SuspendSchoolCommand(schoolId)));

    [HttpPost("schools/{schoolId:guid}/reactivation")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Reactivate School")]
    public async Task<IActionResult> ReactivateSchool(Guid schoolId) =>
        ToActionResult(await _mediator.Send(new ReactivateSchoolCommand(schoolId)));

    [HttpPost("schools/{schoolId:guid}/membership-changes")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Change School Membership")]
    public async Task<IActionResult> ChangeSchoolMembership(Guid schoolId, [FromBody] OrganizationMembershipRequest request) =>
        ToActionResult(await _mediator.Send(new ChangeSchoolMembershipCommand(
            schoolId, request.Package)));

    [HttpPost("companies/{companyId:guid}/staff-invitations")]
    [EndpointSummary("Invite Staff")]
    public async Task<IActionResult> InviteStaff(
        Guid companyId,
        [FromBody] OrganizationInvitationRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new InviteOrganizationMemberCommand(companyId, request.Email, OrganizationMemberType.Staff),
            cancellationToken));

    [HttpPost("schools/{schoolId:guid}/teacher-invitations")]
    [EndpointSummary("Invite Teacher")]
    public async Task<IActionResult> InviteTeacher(
        Guid schoolId,
        [FromBody] OrganizationInvitationRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new InviteOrganizationMemberCommand(schoolId, request.Email, OrganizationMemberType.Teacher),
            cancellationToken));

    [HttpPost("schools/{schoolId:guid}/student-enrollments")]
    [EndpointSummary("Enroll Student")]
    public async Task<IActionResult> EnrollStudent(
        Guid schoolId,
        [FromBody] OrganizationInvitationRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new InviteOrganizationMemberCommand(schoolId, request.Email, OrganizationMemberType.Student),
            cancellationToken));

    [HttpPost("invitations/{invitationToken}/accept")]
    [EndpointSummary("Accept Invitation")]
    public async Task<IActionResult> AcceptInvitation(
        string invitationToken,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new AcceptOrganizationInvitationCommand(invitationToken), cancellationToken));

    [HttpGet("companies/{companyId:guid}/staff")]
    [EndpointSummary("List Staff")]
    public async Task<IActionResult> ListStaff(
        Guid companyId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _mediator.Send(
            new ListCompanyStaffQuery(companyId, status, page, pageSize), cancellationToken));

    [HttpPost("companies/{companyId:guid}/staff/{staffId:guid}/deactivation")]
    [EndpointSummary("Deactivate Staff")]
    public async Task<IActionResult> DeactivateStaff(
        Guid companyId,
        Guid staffId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new DeactivateOrganizationMemberCommand(companyId, staffId, "Staff"), cancellationToken));

    [HttpPost("schools/{schoolId:guid}/teachers/{teacherId:guid}/deactivation")]
    [EndpointSummary("Deactivate Teacher")]
    public async Task<IActionResult> DeactivateTeacher(
        Guid schoolId,
        Guid teacherId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new DeactivateOrganizationMemberCommand(schoolId, teacherId, "Teacher"), cancellationToken));

    [HttpPost("schools/{schoolId:guid}/students/{studentId:guid}/deactivation")]
    [EndpointSummary("Deactivate Student")]
    public async Task<IActionResult> DeactivateStudent(
        Guid schoolId,
        Guid studentId,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new DeactivateOrganizationMemberCommand(schoolId, studentId, "Student"), cancellationToken));

    [HttpPost("schools/{schoolId:guid}/students/{studentId:guid}/transfers")]
    [EndpointSummary("Transfer Student")]
    public async Task<IActionResult> TransferStudent(
        Guid schoolId,
        Guid studentId,
        [FromBody] TransferStudentRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _mediator.Send(
            new TransferStudentCommand(schoolId, studentId, request.TargetSchoolId), cancellationToken));
}

public sealed record OrganizationInvitationRequest(string Email);
