using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardDeck.Api.Models;

public class Suit
{
    public int Id { get; private set; }
    public string Name { get; set; } = null!; // nvarchar(15)
    public char Symbol { get; set; } // nchar(1)
    public int ColorRGB { get; set; } = 0; // default to black
}

// Fluent API configuration
public class SuitConfiguration : IEntityTypeConfiguration<Suit>
{
    public void Configure(EntityTypeBuilder<Suit> builder)
    {
        builder.ToTable("Suits");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(15).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();

        // char -> nchar(1)
        builder.Property(s => s.Symbol).HasColumnType("nchar(1)");

        builder.Property(s => s.ColorRGB).HasDefaultValue(0);
    }
}
