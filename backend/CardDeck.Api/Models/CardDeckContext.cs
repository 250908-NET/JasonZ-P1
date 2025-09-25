using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Models;

public partial class CardDeckContext : DbContext
{
    public CardDeckContext() { }

    public CardDeckContext(DbContextOptions<CardDeckContext> options)
        : base(options) { }

    public DbSet<Suit> Suits { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<AvailableCard> AvailableCards { get; set; }
    public DbSet<GameCard> GameCards { get; set; }
    public DbSet<Game> Games { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
