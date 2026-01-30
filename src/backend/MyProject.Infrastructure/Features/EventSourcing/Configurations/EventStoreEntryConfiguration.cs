using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Infrastructure.Features.EventSourcing.Models;

namespace MyProject.Infrastructure.Features.EventSourcing.Configurations;

internal class EventStoreEntryConfiguration : IEntityTypeConfiguration<EventStoreEntry>
{
    public void Configure(EntityTypeBuilder<EventStoreEntry> builder)
    {
        builder.ToTable("event_store");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.AggregateId)
            .IsRequired();
        
        builder.Property(e => e.AggregateType)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(e => e.EventType)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(e => e.EventData)
            .IsRequired();
        
        builder.Property(e => e.Version)
            .IsRequired();
        
        builder.Property(e => e.OccurredAt)
            .IsRequired();
        
        builder.Property(e => e.StoredAt)
            .IsRequired();
        
        // Index for efficient querying by aggregate
        builder.HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique();
        
        builder.HasIndex(e => e.AggregateType);
    }
}
