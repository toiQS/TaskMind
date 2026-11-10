using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths.Dtos;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record GetCurrentAccountQuery(Guid AccountId) : IRequest<ServiceResult<CurrentAccountDto>>;

public sealed class GetCurrentAccountHandler :
    IRequestHandler<GetCurrentAccountQuery, ServiceResult<CurrentAccountDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurrentAccountHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<CurrentAccountDto>> Handle(
        GetCurrentAccountQuery query,
        CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts.SingleOrDefaultAsync(
            candidate => candidate.Id == query.AccountId,
            cancellationToken);
        if (account is null)
            return ServiceResult<CurrentAccountDto>.NotFound("Không tìm thấy tài khoản.");

        return ServiceResult<CurrentAccountDto>.Success(new CurrentAccountDto
        {
            AccountId = account.Id,
            Email = account.Profile.Email,
            FirstName = account.Profile.FirstName,
            LastName = account.Profile.LastName,
            FullName = AuthRules.FullName(account),
            Role = account.Role.ToString(),
            IsVerified = account.IsVerified,
            NickName = account.Profile.NickName,
            ImageUrl = account.Profile.ImageUrl,
            Bio = account.Profile.Bio,
            PhoneNumber = account.Profile.PhoneNumber,
            GitHubUrl = account.Profile.GitHubUrl,
            LinkedInUrl = account.Profile.LinkedInUrl,
            IsPublic = account.Profile.IsPublic,
            Education = account.Profile.Education.Select(item => new ProfileEducationDto
            {
                Institution = item.Institution,
                Degree = item.Degree,
                FieldOfStudy = item.FieldOfStudy,
                StartDate = item.StartDate,
                EndDate = item.EndDate
            }).ToArray(),
            Address = new ProfileAddressDto
            {
                Street = account.Profile.Address.Street,
                City = account.Profile.Address.City,
                Country = account.Profile.Address.Country
            }
        });
    }
}
