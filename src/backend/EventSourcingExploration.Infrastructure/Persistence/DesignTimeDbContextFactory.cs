using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventSourcingExploration.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating the DbContext during migrations.
/// This is used by EF Core tools when running migrations without the full application context.
/// </summary>
internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EventSourcingExplorationDbContext>
{
    public EventSourcingExplorationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventSourcingExplorationDbContext>();
        
        // Use a default connection string for design-time operations
        // This can be overridden by environment variables if needed
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database") 
            ?? "Host=localhost;Port=5432;Database=eventsourcingexploration_dev;Username=admin;Password=pwd";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new EventSourcingExplorationDbContext(optionsBuilder.Options);
    }
}
