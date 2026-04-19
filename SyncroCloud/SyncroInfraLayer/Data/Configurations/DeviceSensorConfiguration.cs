using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceSensorConfiguration : IEntityTypeConfiguration<DeviceSensor>
{
    public void Configure(EntityTypeBuilder<DeviceSensor> builder)
    {
        builder.HasKey(ds => new { ds.DeviceId, ds.SensorId });

        builder.Property(ds => ds.SwitchNo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ds => ds.UnitId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ds => ds.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ds => ds.Url)
            .HasMaxLength(500);

        builder.Property(ds => ds.UnitType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ds => ds.SensorType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ds => ds.Protocol)
            .IsRequired();

        builder.Property(ds => ds.InstalledAt)
            .IsRequired();

        builder.Property(ds => ds.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ds => ds.Notes)
            .HasMaxLength(500);

        builder.Property(ds => ds.LastReading)
            .HasColumnType("jsonb");

        builder.HasOne(ds => ds.Device)
            .WithMany(d => d.DeviceSensors)
            .HasForeignKey(ds => ds.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ds => ds.Sensor)
            .WithMany(s => s.DeviceSensors)
            .HasForeignKey(ds => ds.SensorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ds => ds.InstalledBy)
            .WithMany()
            .HasForeignKey(ds => ds.InstalledById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
