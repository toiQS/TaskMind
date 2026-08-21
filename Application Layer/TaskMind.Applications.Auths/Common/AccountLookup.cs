using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Auths.Common
{
    /// <summary>
    /// Tra cứu Account theo email trên toàn bộ loại tài khoản (User/Staff/Teacher/Student/AdminCompany/AdminSchool/Admin).
    /// Cần thiết vì hệ thống chưa có DbSet&lt;Account&gt; hợp nhất — mỗi vai trò là tài khoản liên kết riêng (mục 2.1).
    /// </summary>
    public static class AccountLookup
    {
        public static async Task<Account?> FindByEmailAsync(IApplicationDbContext db, string email, CancellationToken cancellationToken)
        {
            // Tạo Queryable gộp tất cả các bảng
            var query = db.Users.Cast<Account>().Where(u => u.Profile.Email == email)
                .Concat(db.Staffs.Cast<Account>().Where(s => s.Profile.Email == email))
                .Concat(db.Teachers.Cast<Account>().Where(t => t.Profile.Email == email))
                .Concat(db.Students.Cast<Account>().Where(st => st.Profile.Email == email))
                .Concat(db.AdminCompanies.Cast<Account>().Where(ac => ac.Profile.Email == email))
                .Concat(db.AdminSchools.Cast<Account>().Where(ash => ash.Profile.Email == email))
                .Concat(db.Admins.Cast<Account>().Where(a => a.Profile.Email == email));

            // DB chỉ nhận 1 câu SQL duy nhất chứa UNION ALL
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<bool> ExistsByEmailAsync(IApplicationDbContext db, string email, CancellationToken cancellationToken)
            => await FindByEmailAsync(db, email, cancellationToken) is not null;
    }
}
