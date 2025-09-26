using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// input DTO for starting a new game
public record StartGameDTO(
    decimal? Target = null,
    int? DrawLimit = null,
    decimal? InitialMoney = null
);

// output DTO for a card in a game
public record GameCardDTO(
    int Id, // GameCard Id
    CardDTO Card,
    OwnerType OwnerType
);

// output DTO for the current state of a blackjack game
public record GameDTO(
    Guid Id,
    decimal Target,
    int DrawLimit,
    decimal Money,
    int Round,
    decimal Bet,
    GameStatus Status,
    List<GameCardDTO> PlayerHand,
    List<decimal> PlayerHandValues,
    List<GameCardDTO> DealerHand,
    List<decimal> DealerHandValues,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

// Input DTO for placing a bet
public record PlaceBetDTO(decimal Amount);

// --- Validators ---

public class StartGameDTOValidator : AbstractValidator<StartGameDTO>
{
    public StartGameDTOValidator()
    {
        RuleFor(x => x.Target)
            .GreaterThanOrEqualTo(1.0m)
            .WithMessage("Target must be at least 1.0.")
            .When(x => x.Target.HasValue);

        RuleFor(x => x.DrawLimit)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Draw limit must be at least 1.")
            .When(x => x.DrawLimit.HasValue);

        RuleFor(x => x.InitialMoney)
            .GreaterThanOrEqualTo(0.0m)
            .WithMessage("Initial money cannot be negative.")
            .When(x => x.InitialMoney.HasValue);
    }
}

public class PlaceBetDTOValidator : AbstractValidator<PlaceBetDTO>
{
    public PlaceBetDTOValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0.0m).WithMessage("Bet amount must be greater than 0.");
    }
}
