using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Text.Json;
using TaskMind.ApplicationLayer.Features.Auths;

namespace TaskMind.APIs.Controllers;

public sealed class AssessmentsController : ScaffoldControllerBase
{
    private readonly ISender _sender;

    public AssessmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("companies/{companyId:guid}/test-papers")]
    [EndpointSummary("List Company Test Papers")]
    public IActionResult ListCompanyTestPapers(Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListCompanyTestPapers));

    [HttpPost("companies/{companyId:guid}/test-papers")]
    [EndpointSummary("Create Company Test Paper")]
    public IActionResult CreateCompanyTestPaper(Guid companyId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateCompanyTestPaper));

    [HttpGet("schools/{schoolId:guid}/test-papers")]
    [EndpointSummary("List School Test Papers")]
    public IActionResult ListSchoolTestPapers(Guid schoolId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => NotImplemented(nameof(ListSchoolTestPapers));

    [HttpPost("schools/{schoolId:guid}/test-papers")]
    [EndpointSummary("Create School Test Paper")]
    public IActionResult CreateSchoolTestPaper(Guid schoolId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateSchoolTestPaper));

    [HttpGet("test-papers/{testPaperId:guid}")]
    [EndpointSummary("Get Test Paper")]
    public IActionResult GetTestPaper(Guid testPaperId) => NotImplemented(nameof(GetTestPaper));

    [HttpPatch("test-papers/{testPaperId:guid}")]
    [EndpointSummary("Update Test Paper")]
    public IActionResult UpdateTestPaper(Guid testPaperId, [FromBody] JsonElement request) => NotImplemented(nameof(UpdateTestPaper));

    [HttpPost("test-papers/{testPaperId:guid}/submissions")]
    [EndpointSummary("Create Submission")]
    public IActionResult CreateSubmission(Guid testPaperId, [FromBody] JsonElement request) => NotImplemented(nameof(CreateSubmission));

    [HttpGet("me/submissions")]
    [EndpointSummary("List My Submissions")]
    public async Task<IActionResult> ListMySubmissions(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListMySubmissionsQuery(status, page, pageSize), cancellationToken));

    [HttpGet("submissions/{submissionId:guid}")]
    [EndpointSummary("Get Submission")]
    public IActionResult GetSubmission(Guid submissionId) => NotImplemented(nameof(GetSubmission));

    [HttpPost("submissions/{submissionId:guid}/submit")]
    [EndpointSummary("Submit Submission")]
    public IActionResult SubmitSubmission(Guid submissionId) => NotImplemented(nameof(SubmitSubmission));

    [HttpPost("submissions/{submissionId:guid}/grades")]
    [EndpointSummary("Grade Submission")]
    public IActionResult GradeSubmission(Guid submissionId, [FromBody] JsonElement request) => NotImplemented(nameof(GradeSubmission));

    [HttpGet("me/certificates")]
    [EndpointSummary("List My Certificates")]
    public async Task<IActionResult> ListMyCertificates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(new ListMyCertificatesQuery(page, pageSize), cancellationToken));
    [HttpGet("certificates/{verificationCode}/verification")]
    [EndpointSummary("Verify Certificate")]
    [AllowAnonymous]
    public IActionResult VerifyCertificate(string verificationCode) => NotImplemented(nameof(VerifyCertificate));
}
