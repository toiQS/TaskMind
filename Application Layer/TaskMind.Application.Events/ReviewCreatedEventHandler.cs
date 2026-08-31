using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    public class ReviewCreatedEventHandler : INotificationHandler<ReviewCreatedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public ReviewCreatedEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            Guid? recipientAccountId = notification.TargetType switch
            {
                ReviewTargetType.User => notification.TargetRefId,

                ReviewTargetType.Company => (await _dbContext.AdminCompanies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ac => ac.CompanyId == notification.TargetRefId, cancellationToken))?.LinkedUserId,

                ReviewTargetType.School => (await _dbContext.AdminSchools
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.SchoolId == notification.TargetRefId, cancellationToken))?.LinkedUserId,

                _ => null
            };

            if (recipientAccountId is null || recipientAccountId == Guid.Empty)
                return;

            var notifResult = Notification.Create(
                recipientAccountId.Value,
                "Bạn có đánh giá mới",
                $"Bạn vừa nhận được một đánh giá {notification.Rating}/5 sao.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            // TODO: cần DbSet<Review> + entity điểm uy tín tổng hợp để tính lại reputation score.
        }
    }
}