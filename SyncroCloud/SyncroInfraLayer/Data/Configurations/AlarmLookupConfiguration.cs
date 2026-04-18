using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class AlarmLookupConfiguration : IEntityTypeConfiguration<AlarmLookup>
{
    public void Configure(EntityTypeBuilder<AlarmLookup> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.SensorType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.MeasurementKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Condition)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.DefaultThreshold)
            .IsRequired();

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // One lookup row per SensorType + MeasurementKey + Condition combination
        builder.HasIndex(a => new { a.SensorType, a.MeasurementKey, a.Condition }).IsUnique();
    }
}
