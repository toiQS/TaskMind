using System;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities.parents
{
    public abstract class Account : AuditableAggregateRoot
    {
        public AccountRole Role { get; protected set; }
        public bool IsVerified { get; protected set; } = false;

        public virtual Profile Profile { get; protected set; } = default!;
        public virtual Security Security { get; protected set; } = default!;

        protected Account() { }

        protected Result InitializeWithCredentials(
            string citizenId,
            string email,
            AccountRole role,
            string passwordHash)
        {

            var profileResult = Profile.CreateProfile(Id, email, citizenId);
            if (!profileResult.IsSuccess)
                return Result.Failure(profileResult.Message);

            var securityResult = Security.Create(Id, passwordHash);
            if (!securityResult.IsSuccess)
                return Result.Failure(securityResult.Message);

            Role = role;
            IsVerified = false;
            Profile = profileResult.Data!;
            Security = securityResult.Data!;

            return Result.Success();
        }

        public Result Verify()
        {
            if (IsVerified)
                return Result.Failure("Account is already verified.");

            IsVerified = true;
            return Result.Success();
        }

        public Result ChangeRole(AccountRole newRole)
        {
            Role = newRole;
            return Result.Success();
        }
    }
}