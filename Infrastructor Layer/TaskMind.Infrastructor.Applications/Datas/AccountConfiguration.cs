using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskMind.Domain.Entities;

namespace TaskMind.Infrastructor.Applications.Datas
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.OwnsOne(x => x.Profile, p =>
            {
                p.WithOwner();
                p.OwnsOne(pp => pp.Address);
            });

            builder.OwnsOne(x => x.Security);
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
            builder.OwnsOne(x => x.Security);
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
            builder.OwnsOne(x => x.Security);
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
            builder.OwnsOne(x => x.Security);
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
            builder.OwnsOne(x => x.Security);
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
            builder.OwnsOne(x => x.Security);
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
            builder.OwnsOne(x => x.Security);
        }
    }

}
