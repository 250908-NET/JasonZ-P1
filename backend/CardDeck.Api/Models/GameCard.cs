using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDeck.Api.Models;

public enum OwnerType : byte
{
    Player = 0,
    Dealer = 1,
}

public class GameCard
{
    public int Id { get; private set; }
    public int CardId { get; set; } // foreign key to Card
    public Card Card { get; set; } = null!; // navigation property?
    public Guid GameId { get; set; } // foreign key to GameSession
    public Game Game { get; set; } = null!; // navigation property?
    public OwnerType OwnerType { get; set; } = OwnerType.Player; // stored as byte (player or dealer)
}

// Fluent API configuration
public class GameCardConfiguration : IEntityTypeConfiguration<GameCard>
{
    public void Configure(EntityTypeBuilder<GameCard> builder)
    {
        builder.ToTable("GameCards");
        builder.HasKey(gc => gc.Id);

        builder.Property(gc => gc.CardId).IsRequired();
        builder
            .HasOne(gc => gc.Card)
            .WithMany()
            .HasForeignKey(gc => gc.CardId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(gc => gc.CardId).IsUnique(false);

        builder.Property(gc => gc.GameId).IsRequired();
        builder
            .HasOne(gc => gc.Game)
            .WithMany(g => g.GameCards)
            .HasForeignKey(gc => gc.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(gc => gc.GameId).IsUnique(false);

        // store enum as numeric byte and default to Player
        builder
            .Property(gc => gc.OwnerType)
            .HasConversion<byte>()
            .HasDefaultValue(OwnerType.Player);
    }
}
