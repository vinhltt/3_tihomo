using System.Text.Json;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Identity.Infrastructure.Data;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OAuthClient> OAuthClients { get; set; }

    // OpenIddict entities
    public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.GoogleId).IsUnique();
            
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.GoogleId).HasMaxLength(100);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.EmailConfirmed).IsRequired().HasDefaultValue(false);

            // Soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
              entity.Property(e => e.Permissions)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            // Soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.KeyHash).HasMaxLength(255).IsRequired();
              entity.Property(e => e.Scopes)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.ApiKeys)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // UserRole many-to-many configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            
            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            entity.Property(e => e.RevokedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // OAuthClient configuration
        modelBuilder.Entity<OAuthClient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ClientId).IsUnique();
            
            entity.Property(e => e.ClientId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ClientSecretHash).HasMaxLength(255);
            entity.Property(e => e.RedirectUris).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.PostLogoutRedirectUris).HasMaxLength(2000);
            entity.Property(e => e.AllowedScopes).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ApplicationUrl).HasMaxLength(500);

            // Soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "User",
                Description = "Standard user role",
                Permissions = ["read:profile", "update:profile"],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Admin",
                Description = "Administrator role",
                Permissions = ["*"],
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed default OAuth clients
        modelBuilder.Entity<OAuthClient>().HasData(
            new OAuthClient
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ClientId = "tihomo-web-client",
                Name = "TiHoMo Web Application",
                Description = "Nuxt.js web application client",
                Type = OAuthClientType.Public,
                Platform = ClientPlatform.Web,
                RedirectUris = "http://localhost:3333/auth/callback,https://app.tihomo.vn/auth/callback",
                PostLogoutRedirectUris = "http://localhost:3333,https://app.tihomo.vn",
                AllowedScopes = "openid,profile,email,offline_access",
                IsActive = true,
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenLifetime = 2592000, // 30 days
                AllowRefreshTokens = true,
                RequirePkce = true,
                ApplicationUrl = "https://app.tihomo.vn",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new OAuthClient
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ClientId = "tihomo-mobile-ios",
                Name = "TiHoMo iOS Application",
                Description = "iOS mobile application client",
                Type = OAuthClientType.Public,
                Platform = ClientPlatform.IOs,
                RedirectUris = "tihomo://auth/callback,tihomo-ios://auth/callback",
                PostLogoutRedirectUris = "tihomo://logout,tihomo-ios://logout",
                AllowedScopes = "openid,profile,email,offline_access",
                IsActive = true,
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenLifetime = 2592000, // 30 days
                AllowRefreshTokens = true,
                RequirePkce = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new OAuthClient
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                ClientId = "tihomo-mobile-android",
                Name = "TiHoMo Android Application",
                Description = "Android mobile application client",
                Type = OAuthClientType.Public,
                Platform = ClientPlatform.Android,
                RedirectUris = "tihomo://auth/callback,tihomo-android://auth/callback",
                PostLogoutRedirectUris = "tihomo://logout,tihomo-android://logout",
                AllowedScopes = "openid,profile,email,offline_access",
                IsActive = true,
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenLifetime = 2592000, // 30 days
                AllowRefreshTokens = true,
                RequirePkce = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }        );

        base.OnModelCreating(modelBuilder);
        
        // Configure OpenIddict entities
        modelBuilder.UseOpenIddict();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
