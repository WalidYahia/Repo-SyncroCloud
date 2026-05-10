using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceActionLogConfiguration : IEntityTypeConfiguration<DeviceActionLog>
{
    public void Configure(EntityTypeBuilder<DeviceActionLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.DeviceId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.InstalledSensorId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Source)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(l => l.Result)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        builder.Property(l => l.Timestamp)
            .IsRequired();

        builder.HasIndex(l => l.DeviceId);
        builder.HasIndex(l => l.InstalledSensorId);
        builder.HasIndex(l => l.Timestamp);
    }
}
