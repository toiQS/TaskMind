using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;

namespace TaskMind.Applications.Admins.Features.Users
{
    /// <summary>Admin xem danh sách tài khoản User gốc (mục 4.1) — không bao gồm Staff/Teacher/Student/Admin*, vì đó là các LinkedAccount riêng.</summary>
    internal class GetUsersQuery : ServiceResult<PagedResult<UserListItemDto>>
    {
        public GetUsersFilter Filter { get; }

        public GetUsersQuery(GetUsersFilter filter)
        {
            Filter = filter;
        }
    }

    internal class GetUsersHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetUsersHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<PagedResult<UserListItemDto>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            var filter = query.Filter;
            var page = Math.Max(1, filter.Page);
            var pageSize = filter.PageSize is > 0 and <= 100 ? filter.PageSize : 20;

            var usersQuery = _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Profile);

            IQueryable<User> filtered = usersQuery;

            if (filter.Status.HasValue)
                filtered = filtered.Where(u => u.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                filtered = filtered.Where(u =>
                    EF.Functions.ILike(u.Profile.Email, $"%{keyword}%") ||
                    EF.Functions.ILike(u.Profile.FirstName, $"%{keyword}%") ||
                    EF.Functions.ILike(u.Profile.LastName, $"%{keyword}%"));
            }

            var totalCount = await filtered.CountAsync(cancellationToken);

            var items = await filtered
                .OrderByDescending(u => u.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Email = u.Profile.Email,
                    FullName = (u.Profile.FirstName + " " + u.Profile.LastName).Trim(),
                    Role = u.Role,
                    IsVerified = u.IsVerified,
                    Status = u.Status,
                    CreatedAtUtc = u.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<UserListItemDto>>.Success(result, "Lấy danh sách người dùng thành công");
        }
    }
}