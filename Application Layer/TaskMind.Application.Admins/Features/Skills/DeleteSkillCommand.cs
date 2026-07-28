using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin xoá một kỹ năng khỏi danh mục chính thức (mục 4.15).</summary>
    public class DeleteSkillCommand : IRequest<Unit>
    {
        public Guid SkillId { get; set; }
    }

    public class DeleteSkillCommandHandler : IRequestHandler<DeleteSkillCommand, Unit>
    {
        private readonly IApplicationDbContext _db;

        public DeleteSkillCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(DeleteSkillCommand request, CancellationToken cancellationToken)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == request.SkillId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy kỹ năng.");

            _db.Skills.Remove(skill);
            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
