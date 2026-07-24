using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;

namespace TaskMind.Domain.Entities.parents
{
    public class Profile
    {
        [Key, ForeignKey(nameof(Account))]
        public Guid Id { get; private set; }
        public virtual Account Account { get; private set; } = default!;

        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string NickName { get; private set; } = string.Empty;
        public string ImageUrl { get; private set; } = string.Empty;
        public string Bio { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string CitizenId { get; private set; } = string.Empty;
        public Address Address { get; private set; } = new Address();

        private Profile() { }

        private Profile(Guid id, string email, string citizenId)
        {
            Id = id;
            Email = email;
            CitizenId = citizenId;
            NickName = $"user{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public static Result<Profile> CreateProfile(Guid id, string email, string citizenId)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<Profile>.Failure("Email cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(citizenId) || citizenId.Length != 12)
            {
                return Result<Profile>.Failure("Citizen ID cannot be null or empty and must be 12 characters long.");
            }

            var profile = new Profile(id, email, citizenId);
            return Result<Profile>.Success(profile);
        }

        // --- Granular Domain Behaviors ---

        public Result ChangeNickName(string newNickName)
        {
            if (string.IsNullOrWhiteSpace(newNickName))
            {
                return Result.Failure("Nickname cannot be null or empty.");
            }

            NickName = newNickName;
            return Result.Success();
        }

        public Result UpdatePersonalInfo(string firstName, string lastName, string? bio, string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return Result.Failure("First name cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                return Result.Failure("Last name cannot be null or empty.");
            }

            FirstName = firstName;
            LastName = lastName;
            Bio = bio ?? string.Empty;
            ImageUrl = imageUrl ?? string.Empty;

            return Result.Success();
        }

        public Result UpdateContactInfo(string phoneNumber, Address? address)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Result.Failure("Phone number cannot be null or empty.");
            }

            PhoneNumber = phoneNumber;
            if (address != null)
            {
                Address = address;
            }

            return Result.Success();
        }
    }
}