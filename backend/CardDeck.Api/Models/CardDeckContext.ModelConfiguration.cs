using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Models;

public partial class CardDeckContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // apply any IEntityTypeConfiguration<> implementations found in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CardDeckContext).Assembly);
    }
}
