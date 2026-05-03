using Microsoft.EntityFrameworkCore;
using RentalCarApplication.Core.Model;
using RentalCarApplication.EntityFramework.Configurations;

namespace RentalCarApplication.EntityFramework
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public ApplicationContext()
        {
            EnsureOrderStatusCompatibility();
            EnsureOrderCompletionCompatibility();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost; Database=RentalCarDB; Trusted_Connection=True; TrustServerCertificate=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CarConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

        }

        private void EnsureOrderStatusCompatibility()
        {
            try
            {
                Database.ExecuteSqlRaw(@"
if exists (
    select 1
    from sys.columns
    where object_id = object_id('Orders')
      and name = 'Status'
      and system_type_id in (167, 175, 231, 239)
)
begin
    if not exists (
        select 1
        from sys.columns
        where object_id = object_id('Orders')
          and name = 'StatusBit'
    )
    begin
        alter table Orders add StatusBit bit null
    end

    update Orders
    set StatusBit =
        case
            when Status is null then null
            when cast(Status as nvarchar(50)) in ('Pending', '') then null
            when cast(Status as nvarchar(50)) in ('Approved', 'Completed') then 1
            when cast(Status as nvarchar(50)) = 'Canceled' then 0
            else null
        end

    alter table Orders drop column Status
    exec sp_rename 'Orders.StatusBit', 'Status', 'COLUMN'
end
");
            }
            catch
            {
                // Best-effort compatibility patch for local existing databases.
            }
        }

        private void EnsureOrderCompletionCompatibility()
        {
            try
            {
                Database.ExecuteSqlRaw(@"
if not exists (
    select 1
    from sys.columns
    where object_id = object_id('Orders')
      and name = 'FrontPhotoPath'
)
begin
    alter table Orders add FrontPhotoPath nvarchar(max) null
end

if not exists (
    select 1
    from sys.columns
    where object_id = object_id('Orders')
      and name = 'RearPhotoPath'
)
begin
    alter table Orders add RearPhotoPath nvarchar(max) null
end

if not exists (
    select 1
    from sys.columns
    where object_id = object_id('Orders')
      and name = 'SidePhotoPath'
)
begin
    alter table Orders add SidePhotoPath nvarchar(max) null
end

if not exists (
    select 1
    from sys.columns
    where object_id = object_id('Orders')
      and name = 'CompletedAt'
)
begin
    alter table Orders add CompletedAt datetime2 null
end
");
            }
            catch
            {
            }
        }
    }
}
