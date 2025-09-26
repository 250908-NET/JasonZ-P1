using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDeck.Api.Models;

public record CardEffect(string Operation, decimal Value);

public class Card
{
    public int Id { get; private set; }
    public string Rank { get; set; } = null!; // nvarchar(5)
    public int SuitId { get; set; } // foreign key to Suit
    public Suit Suit { get; set; } = null!; // navigation property?
    private List<CardEffect> _effects = []; // backing field to ensure non-null
    public List<CardEffect> Effects // nvarchar(255), default to empty string
    {
        get => _effects;
        set => _effects = value ?? [];
    }
    public DateTimeOffset CreatedAt { get; set; } // datetimeoffset
    public DateTimeOffset UpdatedAt { get; set; } // datetimeoffset
    public byte[] RowVersion { get; set; } = []; // rowversion/timestamp for concurrency
}

// Fluent API configuration
public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Rank).HasMaxLength(16).IsRequired();

        builder.Property(c => c.SuitId).IsRequired();
        builder
            .HasOne(c => c.Suit)
            .WithMany()
            .HasForeignKey(c => c.SuitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.Rank, c.SuitId }).IsUnique();

        // configure Effects as JSON stored in nvarchar(max)
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var effectsProp = builder
            .Property(c => c.Effects)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<CardEffect>>(v, jsonOptions) ?? new()
            )
            .HasColumnType("nvarchar(max)");

        // ensures EF compares the value contents, not object references
        effectsProp.Metadata.SetValueComparer(
            new ValueComparer<List<CardEffect>>(
                (a, b) =>
                    (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
                v => v == null ? 0 : v.Aggregate(0, (h, e) => HashCode.Combine(h, e.GetHashCode())),
                v => v == null ? new() : v.ToList()
            )
        );

        builder.Property(c => c.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}
