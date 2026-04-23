using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SyncroInfraLayer.Data.Configurations;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Identity;
using SyncroInfraLayer.Identity.Configurations;

namespace SyncroInfraLayer.Data;

public class SyncroDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public SyncroDbContext(DbContextOptions<SyncroDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<DeviceSensor> DeviceSensors => Set<DeviceSensor>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();
    public DbSet<AlarmLookup> AlarmLookups => Set<AlarmLookup>();
    public DbSet<DeviceScenario> DeviceScenarios => Set<DeviceScenario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DeviceApiKey> DeviceApiKeys => Set<DeviceApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // must be first — sets up Identity tables

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new AppUserConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new SensorConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceSensorConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceReadingConfiguration());
        modelBuilder.ApplyConfiguration(new AlarmLookupConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceScenarioConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceApiKeyConfiguration());
    }
}
