using System.ComponentModel.DataAnnotations;
using System.Text;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.ApplicationLayer.Features.Auths;

internal static class AuthRules
{
    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        email.Trim().Length <= 254 &&
        new EmailAddressAttribute().IsValid(email.Trim());

    public static bool IsValidPassword(string? password) =>
        !string.IsNullOrEmpty(password) &&
        password.Length >= 8 &&
        Encoding.UTF8.GetByteCount(password) <= 72;

    public static bool PasswordMatches(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) ||
            string.IsNullOrEmpty(password) ||
            Encoding.UTF8.GetByteCount(password) > 72)
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static string FullName(Account account) =>
        string.Join(' ', new[] { account.Profile.FirstName, account.Profile.LastName }
            .Where(name => !string.IsNullOrWhiteSpace(name)));
}
