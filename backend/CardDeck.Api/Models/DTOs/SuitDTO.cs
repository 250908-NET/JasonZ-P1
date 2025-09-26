using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// output DTO for returning a suit
public record SuitDTO(int Id, string Name, char Symbol, int ColorRGB);

// input DTO for creating a new suit
public record CreateSuitDTO(string Name, char Symbol, int ColorRGB);

// input DTO for updating an existing suit
public record UpdateSuitDTO(string Name, char Symbol, int ColorRGB);

// input DTO for updating part of an existing suit
public record PartialUpdateSuitDTO(string? Name, char? Symbol, int? ColorRGB)
{
    // parameterless constructor
    public PartialUpdateSuitDTO()
        : this(null, null, null) { }
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

        RuleFor(s => s.ColorRGB)
            .InclusiveBetween(0, 16777215)
            .WithMessage("ColorRGB must be between 0 and 16777215.");
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

        RuleFor(s => s.ColorRGB)
            .InclusiveBetween(0x000000, 0xFFFFFF)
            .WithMessage("ColorRGB must be between 0x000000 and 0xFFFFFF.");
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

        RuleFor(s => s.ColorRGB)
            .InclusiveBetween(0x000000, 0xFFFFFF)
            .WithMessage("ColorRGB must be between 0x000000 and 0xFFFFFF if provided.")
            .When(s => s.ColorRGB != null);
    }
}
