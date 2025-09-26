using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDeck.Api.Models;

public enum GameStatus : byte
{
    DealingToPlayer = 0,
    DealingToDealer = 1,
    RoundEnd = 2,
}

public class Game
{
    public Game()
    {
        Id = Guid.CreateVersion7();
    }

    public Game(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; private set; } // uniqueidentifier, uuidv7

    // --- session-level data ---
    public decimal Target { get; set; } = 21.0m; // decimal(18), default to 21.0
    public int OverdrawLimit { get; set; } = 5; // int, default to 5
    public decimal Money { get; set; } = 1000.0m; // decimal(18,2), default to 1000.0
    public DateTimeOffset CreatedAt { get; set; } // datetimeoffset

    public List<GameCard> GameCards { get; set; } = []; // navigation property

    // -- round-level data ---
    public int Round { get; set; } = 0; // int, default to 0
    public decimal Bet { get; set; } = 0.0m; // decimal(18,2), default to 0.0
    public GameStatus Status { get; set; } = GameStatus.DealingToPlayer; // tinyint: 0 -> dealing to player, 1 -> dealing to dealer, 2 -> round end
    public int OverdrawsRemaining { get; set; } = 0;
    public DateTimeOffset UpdatedAt { get; set; } // datetimeoffset
    public byte[] RowVersion { get; set; } = []; // rowversion/timestamp for concurrency
}

// Fluent API configuration
public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Target).HasPrecision(18, 2).HasDefaultValue(21.0m);

        builder.Property(g => g.OverdrawLimit).HasDefaultValue(5);

        builder.Property(g => g.Money).HasPrecision(18, 2).HasDefaultValue(1000.0m);

        builder.Property(g => g.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.Property(g => g.Round).HasDefaultValue(0);

        builder.Property(g => g.Bet).HasPrecision(18, 2).HasDefaultValue(0.0m);

        // store enum as numeric byte and default to DealingToPlayer
        builder
            .Property(g => g.Status)
            .HasConversion<byte>()
            .HasDefaultValue(GameStatus.DealingToPlayer);

        builder.Property(g => g.UpdatedAt).ValueGeneratedOnAddOrUpdate();

        builder.Property(g => g.RowVersion).IsRowVersion().IsConcurrencyToken();
    }
}
