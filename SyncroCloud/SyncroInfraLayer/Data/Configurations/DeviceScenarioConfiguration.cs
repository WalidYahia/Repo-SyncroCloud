using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class DeviceScenarioConfiguration : IEntityTypeConfiguration<DeviceScenario>
{
    public void Configure(EntityTypeBuilder<DeviceScenario> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Payload)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.HasOne(s => s.Device)
            .WithMany()
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.DeviceId);
    }
}
