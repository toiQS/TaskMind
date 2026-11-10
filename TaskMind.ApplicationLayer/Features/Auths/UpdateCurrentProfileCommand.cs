using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record UpdateCurrentProfileCommand(
    string? FirstName,
    string? LastName,
    string? NickName,
    string? Bio,
    string? ImageUrl,
    string? PhoneNumber,
    string? Street,
    string? City,
    string? Country,
    string? GitHubUrl = null,
    string? LinkedInUrl = null,
    bool? IsPublic = null,
    IReadOnlyCollection<ProfileEducationRequest>? Education = null) : IRequest<ServiceResult>;

public sealed record ProfileEducationRequest(
    string Institution,
    string? Degree,
    string? FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed class UpdateCurrentProfileHandler : IRequestHandler<UpdateCurrentProfileCommand, ServiceResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentSessionProvider _sessionProvider;

    public UpdateCurrentProfileHandler(
        IApplicationDbContext dbContext,
        ICurrentSessionProvider sessionProvider)
    {
        _dbContext = dbContext;
        _sessionProvider = sessionProvider;
    }

    public async Task<ServiceResult> Handle(
        UpdateCurrentProfileCommand command,
        CancellationToken cancellationToken)
    {
        if (command.FirstName is null &&
            command.LastName is null &&
            command.NickName is null &&
            command.Bio is null &&
            command.ImageUrl is null &&
            command.PhoneNumber is null &&
            command.Street is null &&
            command.City is null &&
            command.Country is null &&
            command.GitHubUrl is null &&
            command.LinkedInUrl is null &&
            command.IsPublic is null &&
            command.Education is null)
            return ServiceResult.Failure("Cần cung cấp ít nhất một trường hồ sơ để cập nhật.");

        var accountId = _sessionProvider.GetUserId();
        if (!accountId.HasValue || accountId.Value == Guid.Empty)
            return ServiceResult.Unauthorized("Không xác định được tài khoản hiện tại.");

        var account = await _dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Id == accountId.Value,
            cancellationToken);
        if (account is null)
            return ServiceResult.NotFound("Không tìm thấy tài khoản.");
        if (!account.IsVerified || account.Status != EntityStatus.Active)
            return ServiceResult.Forbidden("Chỉ tài khoản đã xác minh và đang hoạt động mới được cập nhật hồ sơ.");

        var updateResult = account.Profile.UpdateProfile(
            command.FirstName,
            command.LastName,
            command.NickName,
            command.Bio,
            command.ImageUrl,
            command.PhoneNumber,
            command.Street,
            command.City,
            command.Country);
        if (!updateResult.IsSuccess)
            return ServiceResult.Failure(updateResult.Message);
        var education = command.Education?.Select(item => ProfileEducation.Create(
            item.Institution, item.Degree, item.FieldOfStudy, item.StartDate, item.EndDate));
        var professionalUpdate = account.Profile.UpdateProfessionalProfile(
            command.GitHubUrl, command.LinkedInUrl, command.IsPublic, education);
        if (!professionalUpdate.IsSuccess)
            return ServiceResult.Failure(professionalUpdate.Message);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Error("Hồ sơ vừa được cập nhật bởi một yêu cầu khác. Vui lòng tải lại và thử lại.");
        }

        return ServiceResult.Success("Đã cập nhật hồ sơ cá nhân.");
    }
}
