using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

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

    public static async Task<bool> IsOrganizationActive(
        IApplicationDbContext dbContext,
        Account account,
        CancellationToken cancellationToken) =>
        account switch
        {
            AdminCompany admin => await dbContext.Companies.AnyAsync(
                company => company.Id == admin.CompanyId &&
                           company.IsVerified &&
                           company.Status == EntityStatus.Active,
                cancellationToken),
            AdminSchool admin => await dbContext.Schools.AnyAsync(
                school => school.Id == admin.SchoolId &&
                          school.IsVerified &&
                          school.Status == EntityStatus.Active,
                cancellationToken),
            _ => true
        };
}
