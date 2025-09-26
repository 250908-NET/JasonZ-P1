using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// output DTO for an available card
public record AvailableCardDTO(int Id, CardDTO Card, DateTimeOffset CreatedAt);

// input DTO for creating an available card
public record CreateAvailableCardDTO(int CardId);

// --- validators ---

public class CreateAvailableCardDTOValidator : AbstractValidator<CreateAvailableCardDTO>
{
    public CreateAvailableCardDTOValidator()
    {
        RuleFor(ac => ac.CardId).GreaterThan(0).WithMessage("Valid CardId is required.");
    }
}
