using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
        public string GitHubUrl { get; private set; } = string.Empty;
        public string LinkedInUrl { get; private set; } = string.Empty;
        public bool IsPublic { get; private set; } = true;

        public string EducationJson { get; private set; } = "[]";
        [NotMapped]
        public IReadOnlyCollection<ProfileEducation> Education =>
            JsonSerializer.Deserialize<ProfileEducation[]>(EducationJson) ?? Array.Empty<ProfileEducation>();

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

        public Result UpdateProfessionalProfile(
            string? gitHubUrl,
            string? linkedInUrl,
            bool? isPublic,
            IEnumerable<ProfileEducation>? education)
        {
            if (gitHubUrl is not null && !IsValidProfileUrl(gitHubUrl, "github.com"))
                return Result.Failure("GitHubUrl phải là URL HTTPS thuộc github.com.");
            if (linkedInUrl is not null && !IsValidProfileUrl(linkedInUrl, "linkedin.com"))
                return Result.Failure("LinkedInUrl phải là URL HTTPS thuộc linkedin.com.");

            var educationEntries = education?.ToArray();
            if (educationEntries is not null &&
                (educationEntries.Length > 100 || educationEntries.Any(item => item is null || !item.IsValid)))
                return Result.Failure("Thông tin học vấn không hợp lệ.");

            if (gitHubUrl is not null) GitHubUrl = gitHubUrl.Trim();
            if (linkedInUrl is not null) LinkedInUrl = linkedInUrl.Trim();
            if (isPublic.HasValue) IsPublic = isPublic.Value;
            if (educationEntries is not null)
                EducationJson = JsonSerializer.Serialize(educationEntries);
            return Result.Success();
        }

        private static bool IsValidProfileUrl(string value, string host)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps)
                return false;
            var hostname = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? uri.Host[4..]
                : uri.Host;
            return hostname.Equals(host, StringComparison.OrdinalIgnoreCase) ||
                   hostname.EndsWith($".{host}", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed record ProfileEducation(
        string Institution,
        string Degree,
        string FieldOfStudy,
        DateOnly? StartDate,
        DateOnly? EndDate)
    {
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Institution) &&
            Institution.Length <= 200 &&
            Degree.Length <= 200 &&
            FieldOfStudy.Length <= 200 &&
            (!StartDate.HasValue || !EndDate.HasValue || StartDate <= EndDate);

        public static ProfileEducation Create(
            string institution,
            string? degree,
            string? fieldOfStudy,
            DateOnly? startDate,
            DateOnly? endDate) =>
            new(institution.Trim(), degree?.Trim() ?? string.Empty,
                fieldOfStudy?.Trim() ?? string.Empty, startDate, endDate);
    }
}