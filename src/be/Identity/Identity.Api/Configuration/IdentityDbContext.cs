using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Configuration;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserLogin> UserLogins { get; set; } = null!;
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PictureUrl).HasMaxLength(200);
        });

        // UserLogin configuration
        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProviderUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProviderDisplayName).HasMaxLength(200);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.KeyPrefix);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.KeyHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(32);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Scopes).HasMaxLength(1000);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}