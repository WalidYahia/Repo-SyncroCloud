using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceLatestReadingConfiguration : IEntityTypeConfiguration<DeviceLatestReading>
{
    public void Configure(EntityTypeBuilder<DeviceLatestReading> builder)
    {
        builder.HasKey(r => r.DeviceSensorId);

        builder.Property(r => r.DeviceSensorId).HasMaxLength(500);
        builder.Property(r => r.DeviceId).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ReadingTime).IsRequired();
        builder.Property(r => r.PublishedAt).IsRequired();
        builder.Property(r => r.WriteTime).IsRequired();
        builder.Property(r => r.Payload).IsRequired().HasColumnType("jsonb");
    }
}
