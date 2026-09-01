// TeacherLeftEvent.cs
using TaskMind.Domain.Commons.Events;

namespace TaskMind.Domain.Events
{
    /// <summary>Phát sinh khi một Teacher rời cơ sở đào tạo — bản ghi đóng băng VĨNH VIỄN, không tái kích hoạt (mục 2.1.1). [MỚI - v2.1]</summary>
    public class TeacherLeftEvent : DomainEvent
    {
        public Guid TeacherAccountId { get; init; }
        public Guid LinkedUserId { get; init; }
        public Guid SchoolId { get; init; }
        public DateTimeOffset JoinedAtUtc { get; init; }
        public DateTimeOffset LeftAtUtc { get; init; }
    }
}
