using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RentalCarApplication.Core.Model;
using RentalCarApplication.EntityFramework.Configurations;
using System;
using System.IO;
using System.Text.Json;
namespace RentalCarApplication.EntityFramework
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public ApplicationContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connStr = "Server=localhost; Database=RentalCarDB; Trusted_Connection=True; TrustServerCertificate=True;";
                var jsonPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(jsonPath))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllBytes(jsonPath));
                        var root = doc.RootElement;
                        if (root.TryGetProperty("ConnectionStrings", out var cs)
                            && cs.TryGetProperty("DefaultConnection", out var val))
                        {
                            connStr = val.GetString() ?? connStr;
                        }
                    }
                    catch { }
                }
                optionsBuilder.UseSqlServer(connStr);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new CarConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
        public void InitializeDatabase()
        {
            Database.EnsureCreated();
            EnsureOrderStatusCompatibility();
            EnsureOrderCompletionCompatibility();
            EnsureOrderCityCompatibility();
            EnsureUserOptionalDocumentPhotoCompatibility();
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
        private void EnsureOrderCityCompatibility()
        {
            try
            {
                Database.ExecuteSqlRaw(@"
                    if exists (
                        select 1
                        from sys.columns
                        where object_id = object_id('Orders')
                          and name = 'City'
                    )
                    begin
                        alter table Orders drop column City
                    end
                ");
            }
            catch
            {
                // Best-effort compatibility patch for local existing databases.
            }
        }
        private void EnsureUserOptionalDocumentPhotoCompatibility()
        {
            try
            {
                Database.ExecuteSqlRaw(@"
                    if exists (
                        select 1
                        from sys.columns
                        where object_id = object_id('Users')
                          and name = 'PassportPhotoPath'
                    )
                    begin
                        alter table Users drop column PassportPhotoPath
                    end
                    if exists (
                        select 1
                        from sys.columns
                        where object_id = object_id('Users')
                          and name = 'DriverLicensePhotoPath'
                    )
                    begin
                        alter table Users drop column DriverLicensePhotoPath
                    end
                 ");
            }
            catch
            {
                // Best-effort compatibility patch for local existing databases.
            }
        }
    }
}