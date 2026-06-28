using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceConfigConfiguration : IEntityTypeConfiguration<DeviceConfig>
{
    public void Configure(EntityTypeBuilder<DeviceConfig> builder)
    {
        builder.HasKey(dc => dc.Id);

        builder.HasIndex(dc => new { dc.DeviceId, dc.ConfigType, dc.UpdatedFrom })
            .IsUnique()
            .HasDatabaseName("IX_DeviceConfigs_DeviceId_ConfigType_UpdatedFrom");

        builder.Property(dc => dc.DeviceId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(dc => dc.ConfigType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(dc => dc.ConfigVersion)
            .IsRequired();

        builder.Property(dc => dc.UpdateTime)
            .IsRequired();

        builder.Property(dc => dc.Config)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        builder.Property(dc => dc.UpdatedFrom)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(dc => dc.Device)
            .WithMany(d => d.DeviceConfigs)
            .HasForeignKey(dc => dc.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
