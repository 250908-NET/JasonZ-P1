using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// output DTO for a card (including suit details)
public record CardDTO(
    int Id,
    string Rank,
    SuitDTO Suit,
    List<CardEffect> Effects,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

// input DTO for creating a new card
public record CreateCardDTO(string Rank, int SuitId, List<CardEffect> Effects);

// input DTO for updating an existing card
public record UpdateCardDTO(string Rank, int SuitId, List<CardEffect> Effects);

// input DTO for updating part of an existing card
public record PartialUpdateCardDTO(string? Rank, int? SuitId, List<CardEffect>? Effects)
{
    // parameterless constructor
    public PartialUpdateCardDTO()
        : this(null, null, null) { }
}

// --- validators ---

public class CreateCardDTOValidator : AbstractValidator<CreateCardDTO>
{
    public CreateCardDTOValidator()
    {
        RuleFor(c => c.Rank)
            .NotEmpty()
            .WithMessage("Card rank is required.")
            .MaximumLength(16)
            .WithMessage("Card rank cannot exceed 16 characters.");

        RuleFor(c => c.SuitId).GreaterThan(0).WithMessage("Valid SuitId is required.");

        RuleFor(c => c.Effects).NotNull().WithMessage("Effects list is required.");

        RuleFor(c => c.Effects)
            .NotEmpty()
            .WithMessage("Effects list cannot be empty if provided.")
            .Must(e => e!.All(effect => !string.IsNullOrWhiteSpace(effect.Operation)))
            .WithMessage("Each effect must have a valid operation.")
            .When(c => c.Effects != null); // avoid null ref
    }
}

public class UpdateCardDTOValidator : AbstractValidator<UpdateCardDTO>
{
    public UpdateCardDTOValidator()
    {
        RuleFor(c => c.Rank)
            .NotEmpty()
            .WithMessage("Card rank is required.")
            .MaximumLength(16)
            .WithMessage("Card rank cannot exceed 16 characters.");

        RuleFor(c => c.SuitId).GreaterThan(0).WithMessage("Valid SuitId is required.");

        RuleFor(c => c.Effects).NotNull().WithMessage("Effects list is required.");

        RuleFor(c => c.Effects)
            .NotEmpty()
            .WithMessage("Effects list cannot be empty if provided.")
            .Must(e => e!.All(effect => !string.IsNullOrWhiteSpace(effect.Operation)))
            .WithMessage("Each effect must have a valid operation.")
            .When(c => c.Effects != null); // avoid null ref
    }
}

public class PartialUpdateCardDTOValidator : AbstractValidator<PartialUpdateCardDTO>
{
    public PartialUpdateCardDTOValidator()
    {
        RuleFor(c => c.Rank)
            .NotEmpty()
            .WithMessage("Card rank cannot be empty if provided.")
            .MaximumLength(16)
            .WithMessage("Card rank cannot exceed 16 characters if provided.")
            .When(c => c.Rank != null);

        RuleFor(c => c.SuitId)
            .GreaterThan(0)
            .WithMessage("SuitId must be greater than 0 if provided.")
            .When(c => c.SuitId != null);

        RuleFor(c => c.Effects)
            .NotEmpty()
            .WithMessage("Effects list cannot be empty if provided.")
            .Must(e => e!.All(effect => !string.IsNullOrWhiteSpace(effect.Operation)))
            .WithMessage("Each effect must have a valid operation.")
            .When(c => c.Effects != null); // avoid null ref
    }
}
