using System;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities.parents
{
    public class User : Account
    {
        private User() : base() { }

        public static Result<User> Create(
            Guid id,
            string citizenId,
            string email,
            string passwordHash,
            string refreshToken,
            AccountRole role = AccountRole.User)
        {
            var user = new User();
            var initResult = user.InitializeWithCredentials(id, citizenId, email, role, passwordHash, refreshToken);

            if (!initResult.IsSuccess)
            {
                return Result<User>.Failure(initResult.Message);
            }

            return Result<User>.Success(user);
        }
    }
}