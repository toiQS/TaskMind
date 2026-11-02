using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
{
    public class Admin : Account
    {
        private Admin() : base() { }
        public static Result<Admin> Create(
            string citizenId,
            string email,
            string passwordHash)
        {
            var admin = new Admin();
            var initResult = admin.InitializeWithCredentials(citizenId, email, AccountRole.Admin, passwordHash);
            if (!initResult.IsSuccess)
            {
                return Result<Admin>.Failure(initResult.Message);
            }
            return Result<Admin>.Success(admin);
        }
    }
}
