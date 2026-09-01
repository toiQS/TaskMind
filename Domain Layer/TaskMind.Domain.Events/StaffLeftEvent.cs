// StaffLeftEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>
    /// Phát sinh khi một Staff rời công ty — bản ghi đóng băng VĨNH VIỄN, không tái kích hoạt (mục
    /// 2.1.1). JoinedAtUtc/LeftAtUtc xác định khoảng thời gian công tác, dùng làm căn cứ cho quy trình
    /// phản ánh kỹ năng (mục 4.3.2) và lịch sử kỹ năng (mục 4.3.3). [MỚI - v2.1]
    /// </summary>
    public class StaffLeftEvent : DomainEvent
    {
        public Guid StaffAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid CompanyId { get; init; }
        public DateTimeOffset JoinedAtUtc { get; init; }
        public DateTimeOffset LeftAtUtc { get; init; }
    }
}
