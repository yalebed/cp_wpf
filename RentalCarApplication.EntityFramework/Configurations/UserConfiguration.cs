using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalCarApplication.Core.Model;

namespace RentalCarApplication.EntityFramework.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Email);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Surname).IsRequired().HasMaxLength(22);
            builder.Property(x => x.Password).IsRequired();
            builder.Property(x => x.Passport).HasMaxLength(450).IsRequired(false);
            builder.Property(x => x.DriverLicense).HasMaxLength(450).IsRequired(false);
            builder.Property(x => x.IdentitySelfiePhotoData).HasColumnType("varbinary(max)").IsRequired(false);
            builder.Property(x => x.TelNumber).IsRequired();
            builder.Property(x => x.IsAdmin).HasDefaultValue(false);
            builder.Property(x => x.IsDocumentsVerified).HasDefaultValue(false);
            builder.HasData(new User { Email = "lebedzpolina@gmail.com", Password = "I+gmYGS1kGlpneQFKUzyXQ==", Name = "Polina", Surname = "Lebed", Passport = "AB1234567", DriverLicense = "AA5678934", TelNumber = "+375297294012", IsAdmin = true, IsDocumentsVerified = true });

        }
    }
}
