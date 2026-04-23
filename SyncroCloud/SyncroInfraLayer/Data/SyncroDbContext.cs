using Microsoft.EntityFrameworkCore;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Data.Configurations;

namespace SyncroInfraLayer.Data;

public class SyncroDbContext : DbContext
{
    public SyncroDbContext(DbContextOptions<SyncroDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<DeviceSensor> DeviceSensors => Set<DeviceSensor>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();
    public DbSet<AlarmLookup> AlarmLookups => Set<AlarmLookup>();
    public DbSet<DeviceScenario> DeviceScenarios => Set<DeviceScenario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
        modelBuilder.ApplyConfiguration(new CityConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new SensorConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceSensorConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceReadingConfiguration());
        modelBuilder.ApplyConfiguration(new AlarmLookupConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceScenarioConfiguration());
    }
}
