using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => d.DeviceId).IsUnique();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.RegisteredAt)
            .IsRequired();

        builder.HasIndex(d => d.SerialNumber).IsUnique();

        builder.HasOne(d => d.Tenant)
            .WithMany(t => t.Devices)
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Devices)
            .HasForeignKey(d => d.CreatedByUser)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(d => d.Longitude)
            .HasColumnType("double precision");

        builder.Property(d => d.Latitude)
            .HasColumnType("double precision");

        builder.HasOne(d => d.City)
            .WithMany(c => c.Devices)
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
