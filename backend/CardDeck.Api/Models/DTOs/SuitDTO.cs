using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// output DTO for returning a suit
public record SuitDTO(int Id, string Name, char Symbol, int ColorRGB);

// input DTO for creating a new suit
public record CreateSuitDTO(string Name, char Symbol, int ColorRGB);

// input DTO for updating an existing suit
public record UpdateSuitDTO(string Name, char Symbol, int ColorRGB);

// --- validators ---

public class CreateSuitDTOValidator : AbstractValidator<CreateSuitDTO>
{
    public CreateSuitDTOValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("Suit name is required.")
            .MaximumLength(15)
            .WithMessage("Suit name cannot exceed 15 characters.");

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
            .MaximumLength(15)
            .WithMessage("Suit name cannot exceed 15 characters.");

        RuleFor(s => s.Symbol).NotEmpty().WithMessage("Symbol is required.");

        RuleFor(s => s.ColorRGB)
            .InclusiveBetween(0, 16777215)
            .WithMessage("ColorRGB must be between 0 and 16777215.");
    }
}
