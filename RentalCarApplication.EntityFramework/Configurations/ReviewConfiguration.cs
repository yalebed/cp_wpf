using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalCarApplication.Core.Model;

namespace RentalCarApplication.EntityFramework.Configurations
{
    internal class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.ReviewId);
            builder.Property(x => x.ReviewId).ValueGeneratedOnAdd();

            builder.Property(x => x.Text).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.Rating).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.PhotoData).HasColumnType("varbinary(max)").IsRequired(false);

            builder.HasIndex(x => x.OrderId).IsUnique();

            builder.HasOne(x => x.Car).WithMany(x => x.Reviews).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.User).WithMany(x => x.Reviews).HasForeignKey(x => x.Email).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Order).WithMany(x => x.Reviews).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
