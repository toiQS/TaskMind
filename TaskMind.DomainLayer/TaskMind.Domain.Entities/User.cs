using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
{
    public class User : Account
    {
        private User() : base() { }
        public static Result<User> CreateUser(
            string citizenId,
            string email,
            string passwordHash)
        {
            var user = new User();
            var result = user.InitializeWithCredentials(citizenId, email, AccountRole.User, passwordHash);
            if (!result.IsSuccess)
                return Result<User>.Failure(result.Message);
            return Result<User>.Success(user);
        }
    }
}
