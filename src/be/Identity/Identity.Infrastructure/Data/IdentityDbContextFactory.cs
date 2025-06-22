using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EFCore.NamingConventions;

namespace Identity.Infrastructure.Data;

/// <summary>
///     Design-time factory for IdentityDbContext to support EF Core migrations
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();        // Use the connection string matching docker-compose.dev.yml
        optionsBuilder.UseNpgsql("Host=localhost;Database=identity;Username=identity_user;Password=identity_pass;Port=5431")
                      .UseSnakeCaseNamingConvention();

        return new IdentityDbContext(optionsBuilder.Options);
    }
}