using MediatR;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Mapping;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Schools
{
    public class CreateSchoolCommand : IRequest<SchoolDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        /// <summary>"Starter" | "Pro" | "Enterprise"</summary>
        public string Package { get; set; } = "Starter";
    }

    public class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, SchoolDto>
    {
        private readonly IApplicationDbContext _db;

        public CreateSchoolCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SchoolDto> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
        {
            var address = new Address(request.Street ?? string.Empty, request.City ?? string.Empty, request.Country ?? string.Empty);

            var result = School.Create(request.Name, request.Field, request.Email, request.Phone ?? string.Empty, address);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);

            var school = result.Data!;
            var packageResult = school.ChangeMembershipPackage(request.Package);
            if (!packageResult.IsSuccess)
                throw new InvalidOperationException(packageResult.Message);

            _db.Schools.Add(school);
            await _db.SaveChangesAsync(cancellationToken);

            return SchoolMapper.ToDto(school);
        }
    }
}
