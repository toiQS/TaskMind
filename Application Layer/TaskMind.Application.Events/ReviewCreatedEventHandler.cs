using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events.Handlers
{
    /// <summary>
    /// Xử lý khi một Review mới được tạo (mục 4.19: cập nhật điểm uy tín tổng hợp).
    /// TODO: chưa có entity/aggregate lưu điểm uy tín tổng hợp (ReputationScore) ở Domain layer và
    /// IApplicationDbContext cũng chưa có DbSet&lt;Review&gt; — cần bổ sung cả hai trước khi handler này
    /// có thể tính lại điểm uy tín. Hiện chỉ là điểm mở rộng (placeholder).
    /// </summary>
    internal class ReviewCreatedEventHandler : INotificationHandler<ReviewCreatedEvent>
    {
        public Task Handle(ReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
