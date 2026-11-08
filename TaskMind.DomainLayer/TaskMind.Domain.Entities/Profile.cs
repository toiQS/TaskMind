using System.ComponentModel.DataAnnotations;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.ObjectValues;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
{
    public class Profile
    {
        [Key]
        public Guid Id { get; private set; }

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

        public Result UpdateProfile(
            string? firstName,
            string? lastName,
            string? nickName,
            string? bio,
            string? imageUrl,
            string? phoneNumber,
            string? street,
            string? city,
            string? country)
        {
            if (firstName is not null && string.IsNullOrWhiteSpace(firstName))
                return Result.Failure("First name cannot be empty.");
            if (lastName is not null && string.IsNullOrWhiteSpace(lastName))
                return Result.Failure("Last name cannot be empty.");
            if (nickName is not null && string.IsNullOrWhiteSpace(nickName))
                return Result.Failure("Nickname cannot be empty.");

            if (imageUrl is not null && !string.IsNullOrWhiteSpace(imageUrl))
            {
                if (!Uri.TryCreate(imageUrl.Trim(), UriKind.Absolute, out var imageUri) ||
                    imageUri.Scheme is not ("http" or "https"))
                    return Result.Failure("ImageUrl must be an absolute HTTP or HTTPS URL.");
            }

            if (firstName is not null)
                FirstName = firstName.Trim();
            if (lastName is not null)
                LastName = lastName.Trim();
            if (nickName is not null)
                NickName = nickName.Trim();
            if (bio is not null)
                Bio = bio.Trim();
            if (imageUrl is not null)
                ImageUrl = imageUrl.Trim();
            if (phoneNumber is not null)
                PhoneNumber = phoneNumber.Trim();

            if (street is not null || city is not null || country is not null)
            {
                Address = new Address(
                    street?.Trim() ?? Address.Street,
                    city?.Trim() ?? Address.City,
                    country?.Trim() ?? Address.Country);
            }

            return Result.Success();
        }
    }
}