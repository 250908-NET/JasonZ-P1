using System.Text.Json.Serialization;
using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// output DTO for returning a suit
public record SuitDTO
{
    public int Id { get; init; }
    public string Name { get; init; }
    public char Symbol { get; init; }
    public string ColorHex { get; init; }

    [JsonConstructor]
    public SuitDTO(int Id, string Name, char Symbol, string ColorHex)
    {
        this.Id = Id;
        this.Name = Name;
        this.Symbol = Symbol;
        this.ColorHex = ColorHex;
    }

    // constructor to convert from an integer color value
    public SuitDTO(int Id, string Name, char Symbol, int ColorRGB)
        : this(Id, Name, Symbol, $"#{ColorRGB:X6}") { }
}

// input DTO for creating a new suit
public record CreateSuitDTO
{
    public string Name { get; init; }
    public char Symbol { get; init; }
    public string ColorHex { get; init; }

    [JsonConstructor]
    public CreateSuitDTO(string name, char symbol, string colorHex)
    {
        Name = name;
        Symbol = symbol;
        ColorHex = colorHex;
    }

    // constructor to convert from an integer color value
    public CreateSuitDTO(string Name, char Symbol, int ColorRGB)
        : this(Name, Symbol, $"#{ColorRGB:X6}") { }
}

// input DTO for updating an existing suit
public record UpdateSuitDTO
{
    public string Name { get; init; }
    public char Symbol { get; init; }
    public string ColorHex { get; init; }

    [JsonConstructor]
    public UpdateSuitDTO(string name, char symbol, string colorHex)
    {
        Name = name;
        Symbol = symbol;
        ColorHex = colorHex;
    }

    // constructor to convert from an integer color value
    public UpdateSuitDTO(string Name, char Symbol, int ColorRGB)
        : this(Name, Symbol, $"#{ColorRGB:X6}") { }
}

// input DTO for updating part of an existing suit
public record PartialUpdateSuitDTO
{
    public string? Name { get; init; }
    public char? Symbol { get; init; }
    public string? ColorHex { get; init; }

    [JsonConstructor]
    public PartialUpdateSuitDTO(string? name, char? symbol, string? colorHex)
    {
        Name = name;
        Symbol = symbol;
        ColorHex = colorHex;
    }

    // parameterless constructor
    public PartialUpdateSuitDTO()
        : this(null, null, (string?)null) { }

    // constructor to convert from an integer color value
    public PartialUpdateSuitDTO(string? Name, char? Symbol, int? ColorRGB)
        : this(Name, Symbol, ColorRGB != null ? $"#{ColorRGB:X6}" : null) { }
}

// --- helper methods ---

public static class SuitDTOExtensions
{
    /// <summary>
    /// Helper method to convert ColorHex to an integer color value.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public static int ToRgb(this CreateSuitDTO dto)
    {
        return Convert.ToInt32(dto.ColorHex.TrimStart('#'), 16);
    }

    public static int ToRgb(this UpdateSuitDTO dto)
    {
        return Convert.ToInt32(dto.ColorHex.TrimStart('#'), 16);
    }

    public static int? ToRgb(this PartialUpdateSuitDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ColorHex))
        {
            return null;
        }
        return Convert.ToInt32(dto.ColorHex.TrimStart('#'), 16);
    }
}

// --- validators ---

public class CreateSuitDTOValidator : AbstractValidator<CreateSuitDTO>
{
    public CreateSuitDTOValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Suit name is required.")
            .MaximumLength(32)
            .WithMessage("Suit name cannot exceed 32 characters.");

        RuleFor(s => s.Symbol).NotEmpty().WithMessage("Symbol is required.");

        // RuleFor(s => s.ColorRGB)
        //     .InclusiveBetween(0, 16777215)
        //     .WithMessage("ColorRGB must be between 0 and 16777215.");

        RuleFor(s => s.ColorHex)
            .NotEmpty()
            .WithMessage("ColorHex is required.")
            .Matches(@"^#[0-9a-fA-F]{6}$")
            .WithMessage("ColorHex must be a valid 6-digit hex color code (e.g., #RRGGBB).");
    }
}

public class UpdateSuitDTOValidator : AbstractValidator<UpdateSuitDTO>
{
    public UpdateSuitDTOValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Suit name is required.")
            .MaximumLength(32)
            .WithMessage("Suit name cannot exceed 32 characters.");

        RuleFor(s => s.Symbol).NotEmpty().WithMessage("Symbol is required.");

        // RuleFor(s => s.ColorRGB)
        //     .InclusiveBetween(0x000000, 0xFFFFFF)
        //     .WithMessage("ColorRGB must be between 0x000000 and 0xFFFFFF.");

        RuleFor(s => s.ColorHex)
            .NotEmpty()
            .WithMessage("ColorHex is required.")
            .Matches(@"^#[0-9a-fA-F]{6}$")
            .WithMessage("ColorHex must be a valid 6-digit hex color code (e.g., #RRGGBB).");
    }
}

public class PartialUpdateSuitDTOValidator : AbstractValidator<PartialUpdateSuitDTO>
{
    public PartialUpdateSuitDTOValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Suit name cannot be empty if provided.")
            .MaximumLength(32)
            .WithMessage("Suit name cannot exceed 32 characters if provided.")
            .When(s => s.Name != null);

        RuleFor(s => s.Symbol)
            .NotEmpty()
            .WithMessage("Symbol cannot be empty if provided.")
            .When(s => s.Symbol != null);

        // RuleFor(s => s.ColorRGB)
        //     .InclusiveBetween(0x000000, 0xFFFFFF)
        //     .WithMessage("ColorRGB must be between 0x000000 and 0xFFFFFF if provided.")
        //     .When(s => s.ColorRGB != null);

        RuleFor(s => s.ColorHex)
            .Matches(@"^#[0-9a-fA-F]{6}$")
            .WithMessage(
                "ColorHex must be a valid 6-digit hex color code (e.g., #RRGGBB) if provided."
            )
            .When(s => s.ColorHex != null);
    }
}
