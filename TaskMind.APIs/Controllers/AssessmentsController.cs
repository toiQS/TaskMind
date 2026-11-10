using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Text.Json;
using TaskMind.ApplicationLayer.Features.Assessments;
using TaskMind.ApplicationLayer.Features.Auths;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

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
    public async Task<IActionResult> ListCompanyTestPapers(
        Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListTestPapersQuery(TestOwnerType.Company, companyId, page, pageSize),
            cancellationToken));

    [HttpPost("companies/{companyId:guid}/test-papers")]
    [EndpointSummary("Create Company Test Paper")]
    public async Task<IActionResult> CreateCompanyTestPaper(
        Guid companyId,
        [FromBody] CreateTestPaperRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new CreateTestPaperCommand(TestOwnerType.Company, companyId, request.Title, request.DurationMinutes),
            cancellationToken));

    [HttpGet("schools/{schoolId:guid}/test-papers")]
    [EndpointSummary("List School Test Papers")]
    public async Task<IActionResult> ListSchoolTestPapers(
        Guid schoolId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListTestPapersQuery(TestOwnerType.School, schoolId, page, pageSize),
            cancellationToken));

    [HttpPost("schools/{schoolId:guid}/test-papers")]
    [EndpointSummary("Create School Test Paper")]
    public async Task<IActionResult> CreateSchoolTestPaper(
        Guid schoolId,
        [FromBody] CreateTestPaperRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new CreateTestPaperCommand(TestOwnerType.School, schoolId, request.Title, request.DurationMinutes),
            cancellationToken));

    [HttpGet("test-papers/{testPaperId:guid}")]
    [EndpointSummary("Get Test Paper")]
    public async Task<IActionResult> GetTestPaper(Guid testPaperId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetTestPaperQuery(testPaperId), cancellationToken));

    [HttpGet("test-papers/{testPaperId:guid}/submissions")]
    [EndpointSummary("List Test Paper Submissions")]
    public async Task<IActionResult> ListTestPaperSubmissions(
        Guid testPaperId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await _sender.Send(
            new ListTestPaperSubmissionsQuery(testPaperId, status, page, pageSize),
            cancellationToken));

    [HttpPatch("test-papers/{testPaperId:guid}")]
    [EndpointSummary("Update Test Paper")]
    public async Task<IActionResult> UpdateTestPaper(
        Guid testPaperId,
        [FromBody] UpdateTestPaperRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new UpdateTestPaperCommand(testPaperId, request.Title, request.DurationMinutes),
            cancellationToken));

    [HttpPost("test-papers/{testPaperId:guid}/questions")]
    [EndpointSummary("Add Multiple Choice Question")]
    public async Task<IActionResult> AddTestQuestion(
        Guid testPaperId,
        [FromBody] AddTestQuestionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new AddTestQuestionCommand(
                testPaperId, request.Prompt, request.Options,
                request.CorrectOptionIndex, request.Points),
            cancellationToken));

    [HttpPost("test-papers/{testPaperId:guid}/submissions")]
    [EndpointSummary("Create Submission")]
    public async Task<IActionResult> CreateSubmission(Guid testPaperId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new CreateSubmissionCommand(testPaperId), cancellationToken));

    [HttpPut("submissions/{submissionId:guid}/answers")]
    [EndpointSummary("Save Submission Answers")]
    public async Task<IActionResult> SaveSubmissionAnswers(
        Guid submissionId,
        [FromBody] SubmissionAnswersRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new SaveSubmissionAnswersCommand(
                submissionId,
                request.Answers.Select(item =>
                    new TaskMind.DomainLayer.TaskMind.Domain.Entities.SubmissionAnswerInput(
                        item.QuestionId, item.SelectedOptionIndex)).ToArray()),
            cancellationToken));

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
    public async Task<IActionResult> GetSubmission(Guid submissionId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new GetSubmissionQuery(submissionId), cancellationToken));

    [HttpPost("submissions/{submissionId:guid}/submit")]
    [EndpointSummary("Submit Submission")]
    public async Task<IActionResult> SubmitSubmission(Guid submissionId, CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new SubmitSubmissionCommand(submissionId), cancellationToken));

    [HttpPost("submissions/{submissionId:guid}/grades")]
    [EndpointSummary("Grade Submission")]
    public async Task<IActionResult> GradeSubmission(
        Guid submissionId,
        [FromBody] GradeSubmissionRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(
            new GradeSubmissionCommand(submissionId, request.Score),
            cancellationToken));

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
    public async Task<IActionResult> VerifyCertificate(
        string verificationCode,
        CancellationToken cancellationToken) =>
        ToActionResult(await _sender.Send(new VerifyCertificateQuery(verificationCode), cancellationToken));
}

public sealed record CreateTestPaperRequest(string Title, int DurationMinutes);
public sealed record UpdateTestPaperRequest(string Title, int DurationMinutes);
public sealed record GradeSubmissionRequest(decimal Score);
public sealed record AddTestQuestionRequest(
    string Prompt, IReadOnlyCollection<string> Options, int CorrectOptionIndex, decimal Points);
public sealed record SubmissionAnswersRequest(
    IReadOnlyCollection<SubmissionAnswerRequest> Answers);
public sealed record SubmissionAnswerRequest(Guid QuestionId, int SelectedOptionIndex);
