using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;

namespace TaskMind.InfrastructorLayer.Datas
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
                p.HasIndex(profile => profile.Email)
                    .IsUnique()
                    .HasFilter("\"Discriminator\" = 'User'")
                    .HasDatabaseName("IX_Accounts_User_Email");
            });

            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }

    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });
            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }

    public class AdminSchoolConfiguration : IEntityTypeConfiguration<AdminSchool>
    {
        public void Configure(EntityTypeBuilder<AdminSchool> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });
            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }


    public class AdminCompanyConfiguration : IEntityTypeConfiguration<AdminCompany>
    {
        public void Configure(EntityTypeBuilder<AdminCompany> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });
            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }

    public class StaffConfiguration : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });
            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }

    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });
            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }

    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });
            builder.OwnsOne(x => x.Security, AccountConfigurationHelpers.ConfigureSecurity);
        }
    }

}

internal static class AccountConfigurationHelpers
{
    public static void ConfigureSecurity<TOwner>(
        OwnedNavigationBuilder<TOwner, Security> security)
        where TOwner : Account
    {
        security.Property(x => x.RefreshTokenHash).IsConcurrencyToken();
        security.Property(x => x.EmailVerificationTokenHash).IsConcurrencyToken();
        security.Property(x => x.PasswordResetTokenHash).IsConcurrencyToken();
    }
}
