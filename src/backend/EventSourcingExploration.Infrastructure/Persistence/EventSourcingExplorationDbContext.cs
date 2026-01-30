using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventSourcingExploration.Infrastructure.Features.Authentication.Models;
using EventSourcingExploration.Infrastructure.Features.EventSourcing.Models;
using EventSourcingExploration.Infrastructure.Persistence.Extensions;

namespace EventSourcingExploration.Infrastructure.Persistence;

internal class EventSourcingExplorationDbContext(DbContextOptions<EventSourcingExplorationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EventStoreEntry> EventStore { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventSourcingExplorationDbContext).Assembly);
        modelBuilder.ApplyAuthSchema();
        modelBuilder.ApplyFuzzySearch();

        // Seed default roles
        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = Guid.Parse("76b99507-9cf8-4708-9fe8-4dc4e9ea09ae"),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "76b99507-9cf8-4708-9fe8-4dc4e9ea09ae"
            },
            new ApplicationRole
            {
                Id = Guid.Parse("971e674f-c4fb-4bb2-9170-3ad7a753182c"),
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "971e674f-c4fb-4bb2-9170-3ad7a753182c"
            }
        );
    }
}
