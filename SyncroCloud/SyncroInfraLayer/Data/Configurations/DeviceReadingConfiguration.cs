using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceReadingConfiguration : IEntityTypeConfiguration<DeviceReading>
{
    public void Configure(EntityTypeBuilder<DeviceReading> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RecordedAt).IsRequired();
        builder.Property(r => r.ReceivedAt).IsRequired();
        builder.Property(r => r.Payload).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(r => new { r.DeviceId, r.SensorId });
        builder.HasIndex(r => r.RecordedAt);

        builder.HasOne(r => r.DeviceSensor)
            .WithMany(ds => ds.Readings)
            .HasForeignKey(r => r.DeviceSensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
