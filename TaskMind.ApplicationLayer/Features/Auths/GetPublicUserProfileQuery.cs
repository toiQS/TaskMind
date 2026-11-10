using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Auths.Dtos;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Auths;

public sealed record GetPublicUserProfileQuery(Guid UserId)
    : IRequest<ServiceResult<PublicUserProfileDto>>;

public sealed class GetPublicUserProfileHandler :
    IRequestHandler<GetPublicUserProfileQuery, ServiceResult<PublicUserProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPublicUserProfileHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<PublicUserProfileDto>> Handle(
        GetPublicUserProfileQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
            return ServiceResult<PublicUserProfileDto>.Failure("UserId không hợp lệ.");

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == query.UserId &&
                             candidate.IsVerified &&
                             candidate.Status == EntityStatus.Active,
                cancellationToken);
        if (user is null)
            return ServiceResult<PublicUserProfileDto>.NotFound("Không tìm thấy hồ sơ công khai.");
        if (!user.Profile.IsPublic)
            return ServiceResult<PublicUserProfileDto>.NotFound("Không tìm thấy hồ sơ công khai.");

        var skillProfile = await _dbContext.SkillProfiles
            .AsNoTracking()
            .Include(profile => profile.Records)
            .SingleOrDefaultAsync(profile => profile.UserId == user.Id, cancellationToken);

        var skills = Array.Empty<PublicUserSkillDto>();
        if (skillProfile is not null && skillProfile.Records.Count > 0)
        {
            var skillIds = skillProfile.Records.Select(record => record.SkillId).ToArray();
            var skillsById = await _dbContext.Skills
                .AsNoTracking()
                .Where(skill => skillIds.Contains(skill.Id))
                .ToDictionaryAsync(skill => skill.Id, cancellationToken);

            skills = skillProfile.Records
                .Where(record => skillsById.ContainsKey(record.SkillId))
                .Select(record =>
                {
                    var skill = skillsById[record.SkillId];
                    return new PublicUserSkillDto
                    {
                        SkillId = skill.Id,
                        Name = skill.SkillName,
                        Category = skill.Category.ToString(),
                        Level = record.Level.ToString(),
                        EndorsementCount = record.EndorsementCount
                    };
                })
                .OrderBy(skill => skill.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        return ServiceResult<PublicUserProfileDto>.Success(new PublicUserProfileDto
        {
            UserId = user.Id,
            FirstName = user.Profile.FirstName,
            LastName = user.Profile.LastName,
            FullName = AuthRules.FullName(user),
            NickName = user.Profile.NickName,
            ImageUrl = user.Profile.ImageUrl,
            Bio = user.Profile.Bio,
            GitHubUrl = user.Profile.GitHubUrl,
            LinkedInUrl = user.Profile.LinkedInUrl,
            Education = user.Profile.Education.Select(item => new ProfileEducationDto
            {
                Institution = item.Institution,
                Degree = item.Degree,
                FieldOfStudy = item.FieldOfStudy,
                StartDate = item.StartDate,
                EndDate = item.EndDate
            }).ToArray(),
            Skills = skills
        });
    }
}
