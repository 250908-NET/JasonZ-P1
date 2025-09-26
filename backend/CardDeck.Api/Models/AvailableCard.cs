using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDeck.Api.Models;

public class AvailableCard
{
    public int Id { get; private set; }
    public int CardId { get; set; } // foreign key to Card
    public Card Card { get; set; } = null!; // navigation property?
    public DateTimeOffset CreatedAt { get; private set; } // datetimeoffset
    public byte[] RowVersion { get; set; } = []; // rowversion/timestamp for concurrency
}

// Fluent API configuration
public class AvailableCardConfiguration : IEntityTypeConfiguration<AvailableCard>
{
    public void Configure(EntityTypeBuilder<AvailableCard> builder)
    {
        builder.ToTable("AvailableCards");
        builder.HasKey(ac => ac.Id);

        builder.Property(ac => ac.CardId).IsRequired();
        builder
            .HasOne(ac => ac.Card)
            .WithMany()
            .HasForeignKey(ac => ac.CardId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(ac => ac.CardId).IsUnique(false);

        builder.Property(ac => ac.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.Property(ac => ac.RowVersion).IsRowVersion().IsConcurrencyToken();
    }
}
