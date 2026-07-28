using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.Skills
{
    /// <summary>Admin từ chối một đề xuất kỹ năng đang chờ duyệt — xoá khỏi danh mục (mục 4.15).</summary>
    public class RejectSkillCommand : IRequest<Unit>
    {
        public Guid SkillId { get; set; }
    }

    public class RejectSkillCommandHandler : IRequestHandler<RejectSkillCommand, Unit>
    {
        private readonly IApplicationDbContext _db;

        public RejectSkillCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(RejectSkillCommand request, CancellationToken cancellationToken)
        {
            var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == request.SkillId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy kỹ năng.");

            if (skill.IsApproved)
                throw new InvalidOperationException("Không thể từ chối kỹ năng đã được duyệt vào danh mục chính thức.");

            _db.Skills.Remove(skill);
            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
