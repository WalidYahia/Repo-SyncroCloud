using Microsoft.EntityFrameworkCore;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Data.Configurations;

namespace SyncroInfraLayer.Data;

public class SyncroDbContext : DbContext
{
    public SyncroDbContext(DbContextOptions<SyncroDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
    }
}
