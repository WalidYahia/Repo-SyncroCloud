using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Identity;

namespace SyncroInfraLayer.Data.Configurations;

public class RolePrivilegeConfiguration : IEntityTypeConfiguration<RolePrivilege>
{
    public void Configure(EntityTypeBuilder<RolePrivilege> builder)
    {
        builder.HasKey(rp => new { rp.RoleId, rp.PrivilegeId });

        // FK to AspNetRoles — no nav property on AppRole to avoid conflicting with Identity's builder
        builder.HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Privilege)
            .WithMany(p => p.RolePrivileges)
            .HasForeignKey(rp => rp.PrivilegeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
