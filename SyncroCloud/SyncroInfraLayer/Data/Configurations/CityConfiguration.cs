using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Latitude)
            .IsRequired()
            .HasColumnType("double precision");

        builder.Property(c => c.Longitude)
            .IsRequired()
            .HasColumnType("double precision");

        builder.HasOne(c => c.Country)
            .WithMany(co => co.Cities)
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
