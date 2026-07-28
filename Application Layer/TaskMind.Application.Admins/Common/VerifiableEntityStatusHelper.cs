using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Common
{
    /// <summary>
    /// EntityStatus (Domain) chỉ có Active/Paused/Deleted/Blocked, không có khái niệm
    /// Pending/Rejected riêng cho Company/School như UI hiện đang hiển thị. Helper này
    /// quy ước cách suy ra trạng thái hiển thị (Pending/Active/Suspended/Rejected) từ
    /// (IsVerified, Status) để khớp với SchoolStatus/CompanyStatus phía WPF:
    ///
    ///   IsVerified = false, Status != Blocked  -> Pending   (chưa từng được duyệt)
    ///   IsVerified = false, Status == Blocked  -> Rejected  (bị từ chối trước khi duyệt)
    ///   IsVerified = true,  Status != Blocked  -> Active
    ///   IsVerified = true,  Status == Blocked  -> Suspended (đã duyệt nhưng bị tạm ngưng)
    /// </summary>
    public static class VerifiableEntityStatusHelper
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Suspended = "Suspended";
        public const string Rejected = "Rejected";

        public static string Derive(bool isVerified, EntityStatus status)
        {
            if (!isVerified)
                return status == EntityStatus.Blocked ? Rejected : Pending;

            return status == EntityStatus.Blocked ? Suspended : Active;
        }
    }
}