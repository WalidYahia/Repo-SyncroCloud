using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Identity;

namespace SyncroInfraLayer.Identity.Configurations;

public class DeviceApiKeyConfiguration : IEntityTypeConfiguration<DeviceApiKey>
{
    public void Configure(EntityTypeBuilder<DeviceApiKey> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(k => k.KeyHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(k => k.KeyHash).IsUnique();

        builder.Property(k => k.Prefix)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(k => k.CreatedAt).IsRequired();

        builder.Property(k => k.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(k => k.Device)
            .WithMany()
            .HasForeignKey(k => k.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(k => k.DeviceId).IsUnique();  // one key per device
    }
}
