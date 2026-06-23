using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceReadingConfiguration : IEntityTypeConfiguration<DeviceReading>
{
    public void Configure(EntityTypeBuilder<DeviceReading> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.DeviceSensorId).IsRequired().HasMaxLength(500);
        builder.Property(r => r.DeviceId).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ReadingTime).IsRequired();
        builder.Property(r => r.PublishedAt).IsRequired();
        builder.Property(r => r.WriteTime).IsRequired();
        builder.Property(r => r.Payload).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(r => new { r.DeviceSensorId, r.ReadingTime }).IsUnique();
        builder.HasIndex(r => r.WriteTime);
    }
}
