using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure.Data;

/// <summary>
///     Design-time factory for IdentityDbContext to support EF Core migrations
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

        // Use the connection string from appsettings.json
        optionsBuilder.UseNpgsql("Host=localhost;Database=db_identity;Username=postgres;Password=postgres");

        return new IdentityDbContext(optionsBuilder.Options);
    }
}