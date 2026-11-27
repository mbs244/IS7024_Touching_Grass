using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Touching_Grass.Models;

namespace Touching_Grass.Data;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserPlant> UserPlants => Set<UserPlant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DefaultZipCode).HasMaxLength(5).IsRequired();
            entity.Property(user => user.UserName).HasMaxLength(191);
            entity.Property(user => user.NormalizedUserName).HasMaxLength(191);
            entity.Property(user => user.Email).HasMaxLength(191);
            entity.Property(user => user.NormalizedEmail).HasMaxLength(191);
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(role => role.Name).HasMaxLength(191);
            entity.Property(role => role.NormalizedName).HasMaxLength(191);
        });

        builder.Entity<UserPlant>(entity =>
        {
            entity.Property(plant => plant.UserId).HasMaxLength(450).IsRequired();
            entity.Property(plant => plant.CommonNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(plant => plant.ScientificNameSnapshot).HasMaxLength(300).IsRequired();
            entity.HasIndex(plant => new { plant.UserId, plant.SourcePlantId }).IsUnique();
            entity.HasOne(plant => plant.User)
                .WithMany(user => user.UserPlants)
                .HasForeignKey(plant => plant.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}