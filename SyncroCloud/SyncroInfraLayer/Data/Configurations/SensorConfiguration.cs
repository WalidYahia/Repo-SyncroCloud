using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.HasKey(s => s.SensorId);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.UnitType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.ConnectionProtocol)
            .IsRequired()
            .HasConversion<string>();
    }
}
