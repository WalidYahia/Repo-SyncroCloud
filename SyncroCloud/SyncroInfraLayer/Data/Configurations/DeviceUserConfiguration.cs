using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceUserConfiguration : IEntityTypeConfiguration<DeviceUser>
{
    public void Configure(EntityTypeBuilder<DeviceUser> builder)
    {
        builder.HasKey(du => new { du.DeviceId, du.UserId });

        builder.Property(du => du.LinkedAt)
            .IsRequired();

        builder.Property(du => du.SensorPermissions)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        builder.HasOne(du => du.Device)
            .WithMany(d => d.DeviceUsers)
            .HasForeignKey(du => du.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(du => du.User)
            .WithMany(u => u.DeviceUsers)
            .HasForeignKey(du => du.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
