// LinkedAccountLeftEventHandlers.cs — [MỚI - fix, mục 2.1.1]
// Trước đây StaffLeftEvent/TeacherLeftEvent/StudentLeftEvent hoàn toàn không có handler nào — không
// nhất quán với phần còn lại của hệ thống (BlockUserCommand, SuspendCompanyCommand,
// AdminRemoveProjectMemberCommand...) vốn luôn thông báo cho bên bị ảnh hưởng trực tiếp. LinkedUserId
// là tài khoản User gốc cần được biết bản ghi liên kết của mình đã đóng băng vĩnh viễn.
using MediatR;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Applications.Events
{
    public class StaffLeftEventHandler : INotificationHandler<StaffLeftEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        public StaffLeftEventHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public Task Handle(StaffLeftEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Bạn đã rời công ty",
                "Tài khoản Staff của bạn tại công ty đã chuyển sang trạng thái ngừng hoạt động. " +
                "Lịch sử công tác vẫn được lưu lại làm dữ liệu tham chiếu.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }

    public class TeacherLeftEventHandler : INotificationHandler<TeacherLeftEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        public TeacherLeftEventHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public Task Handle(TeacherLeftEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Bạn đã rời cơ sở đào tạo",
                "Tài khoản Teacher của bạn tại cơ sở đào tạo đã chuyển sang trạng thái ngừng hoạt động. " +
                "Lịch sử công tác vẫn được lưu lại làm dữ liệu tham chiếu.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }

    public class StudentLeftEventHandler : INotificationHandler<StudentLeftEvent>
    {
        private readonly IApplicationDbContext _dbContext;
        public StudentLeftEventHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

        public Task Handle(StudentLeftEvent notification, CancellationToken cancellationToken)
        {
            var notifResult = Notification.Create(
                notification.LinkedUserId,
                "Bạn đã rời cơ sở đào tạo",
                "Tài khoản Student của bạn tại cơ sở đào tạo đã chuyển sang trạng thái ngừng hoạt động. " +
                "Lịch sử học tập vẫn được lưu lại làm dữ liệu tham chiếu.",
                NotificationType.System);

            if (notifResult.IsSuccess)
                _dbContext.Notifications.Add(notifResult.Data!);

            return Task.CompletedTask;
        }
    }
}