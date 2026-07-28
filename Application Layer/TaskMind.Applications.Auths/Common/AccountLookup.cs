using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Auths.Common
{
    /// <summary>
    /// Tra cứu Account theo email trên toàn bộ loại tài khoản (User/Staff/Teacher/Student/AdminCompany/AdminSchool/Admin).
    /// Cần thiết vì hệ thống chưa có DbSet&lt;Account&gt; hợp nhất — mỗi vai trò là tài khoản liên kết riêng (mục 2.1).
    /// </summary>
    internal static class AccountLookup
    {
        public static async Task<Account?> FindByEmailAsync(IApplicationDbContext db, string email, CancellationToken cancellationToken)
        {
            Account? account = await db.Users.FirstOrDefaultAsync(u => u.Profile.Email == email, cancellationToken);
            account ??= await db.Staffs.FirstOrDefaultAsync(s => s.Profile.Email == email, cancellationToken);
            account ??= await db.Teachers.FirstOrDefaultAsync(t => t.Profile.Email == email, cancellationToken);
            account ??= await db.Students.FirstOrDefaultAsync(s => s.Profile.Email == email, cancellationToken);
            account ??= await db.AdminCompanies.FirstOrDefaultAsync(a => a.Profile.Email == email, cancellationToken);
            account ??= await db.AdminSchools.FirstOrDefaultAsync(a => a.Profile.Email == email, cancellationToken);
            account ??= await db.Admins.FirstOrDefaultAsync(a => a.Profile.Email == email, cancellationToken);
            return account;
        }

        public static async Task<bool> ExistsByEmailAsync(IApplicationDbContext db, string email, CancellationToken cancellationToken)
            => await FindByEmailAsync(db, email, cancellationToken) is not null;
    }
}