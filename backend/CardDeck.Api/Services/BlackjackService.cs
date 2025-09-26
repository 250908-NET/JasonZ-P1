using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;

namespace CardDeck.Api.Services;

public class BlackjackService(IGameRepository gameRepository, ILogger<BlackjackService> logger)
    : IBlackjackService
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ILogger<BlackjackService> _logger = logger;

    private static readonly Random _random = new();

    public async Task<GameDTO> StartNewGameAsync(StartGameDTO dto)
    {
        _logger.LogInformation("Starting a new game...");

        var game = new Game
        {
            Target = dto.Target ?? 21.0m,
            OverdrawLimit = dto.DrawLimit ?? 5,
            Money = dto.InitialMoney ?? 1000.0m,
            Status = GameStatus.RoundEnd, // Start in a state ready for a bet
        };

        var createdGame = await _gameRepository.CreateGameAsync(game);
        _logger.LogInformation("Started a new game with ID {GameId}!", createdGame.Id);
        return MapGameToDTO(createdGame);
    }

    public async Task<GameDTO> GetGameStateAsync(Guid gameId)
    {
        _logger.LogInformation("Getting game state for game {GameId}...", gameId);
        var game =
            await _gameRepository.GetGameByIdAsync(gameId)
            ?? throw new NotFoundException($"Game with ID {gameId} not found.");
        _logger.LogInformation(
            "Game state for game {GameId} is {GameStatus}!",
            gameId,
            game.Status
        );
        return MapGameToDTO(game);
    }

    public async Task<GameDTO> StartRoundAsync(Guid gameId, PlaceBetDTO placeBetDTO)
    {
        _logger.LogInformation("Placing a bet for game {GameId}...", gameId);
        var game = await GetGameOrThrowAsync(gameId);

        if (game.Status != GameStatus.RoundEnd)
        {
            throw new BadRequestException("Can only place a bet at the end of a round.");
        }
        if (placeBetDTO.Amount <= 0 || placeBetDTO.Amount > game.Money)
        {
            throw new BadRequestException("Invalid bet amount.");
        }

        // start new round
        game.Round++;
        game.Bet = placeBetDTO.Amount;
        game.Money -= placeBetDTO.Amount;
        game.Status = GameStatus.DealingToPlayer;
        game.OverdrawsRemaining = game.OverdrawLimit;
        await _gameRepository.ClearGameCardsAsync(gameId);
        await _gameRepository.UpdateGameAsync(game);

        _logger.LogInformation("Placed a bet of {Bet} for game {GameId}!", game.Bet, gameId);

        // deal initial cards
        await DealCardAsync(game.Id, OwnerType.Player);
        await DealCardAsync(game.Id, OwnerType.Dealer);
        await DealCardAsync(game.Id, OwnerType.Player);
        // await DealCardAsync(game.Id, OwnerType.Dealer); // don't actually deal dealer's face-down card

        _logger.LogInformation("Dealt initial cards for game {GameId}!", gameId);

        var updatedGame = await GetGameOrThrowAsync(gameId);

        // check for blackjack
        var playerValue = CalculateHandValue(
            game.Target,
            [.. updatedGame.GameCards.Where(c => c.OwnerType == OwnerType.Player)]
        );
        if (playerValue == updatedGame.Target)
        {
            _logger.LogInformation(
                "Player already has blackjack for game {GameId}, auto-stand!",
                gameId
            );
            return await PlayerStandAsync(updatedGame.Id);
        }

        _logger.LogInformation(
            "Player does not have blackjack for game {GameId}, player's turn!",
            gameId
        );
        return MapGameToDTO(updatedGame);
    }

    public async Task<GameDTO> PlayerHitAsync(Guid gameId)
    {
        _logger.LogInformation("Player hit for game {GameId}...", gameId);
        var game = await GetGameOrThrowAsync(gameId);

        if (game.Status != GameStatus.DealingToPlayer)
        {
            throw new BadRequestException("It's not the player's turn to hit.");
        }

        await DealCardAsync(game.Id, OwnerType.Player);

        var updatedGame = await GetGameOrThrowAsync(gameId);
        var playerHand = updatedGame.GameCards.Where(c => c.OwnerType == OwnerType.Player).ToList();
        var playerValue = CalculateHandValue(game.Target, playerHand);

        if (playerValue > updatedGame.Target)
        {
            if (updatedGame.OverdrawsRemaining > 0)
            {
                // reduce draws remaining
                updatedGame.OverdrawsRemaining -= 1;
                _logger.LogInformation(
                    "Player is over limit for game {GameId}! {OverdrawsRemaining} overdraws remaining!",
                    gameId,
                    updatedGame.OverdrawsRemaining
                );
            }
            else
            {
                // player bust!
                updatedGame.Status = GameStatus.RoundEnd;
                _logger.LogInformation("Player bust for game {GameId}!", gameId);
            }

            await _gameRepository.UpdateGameAsync(updatedGame);
        }

        return MapGameToDTO(updatedGame);
    }

    public async Task<GameDTO> PlayerStandAsync(Guid gameId)
    {
        _logger.LogInformation("Player stand for game {GameId}...", gameId);
        var game = await GetGameOrThrowAsync(gameId);

        if (game.Status != GameStatus.DealingToPlayer)
        {
            throw new BadRequestException("It's not the player's turn.");
        }

        game.Status = GameStatus.DealingToDealer;
        await _gameRepository.UpdateGameAsync(game);

        decimal dealerHitLimit = game.Target - Math.Max(Math.Floor(game.Target * 21 / 17), 4);
        int dealerOverdraws = game.OverdrawLimit;

        // dealer's turn
        while (true)
        {
            var currentHand = game.GameCards.Where(c => c.OwnerType == OwnerType.Dealer).ToList();
            decimal currentValue = CalculateHandValue(game.Target, currentHand);

            if (currentValue > game.Target)
            {
                // 50% change for dealer to overdraw
                if (dealerOverdraws > 0 && _random.NextDouble() < 0.5)
                {
                    // reduce draws remaining
                    game.OverdrawsRemaining -= 1;
                    _logger.LogInformation(
                        "Dealer tried an overdraw for game {GameId}! {OverdrawsRemaining} overdraws remaining!",
                        gameId,
                        game.OverdrawsRemaining
                    );
                    continue;
                }

                // otherwise, dealer bust!
                _logger.LogInformation(
                    "Dealer bust at {Value} for game {GameId}!",
                    currentValue,
                    gameId
                );
                break;
            }

            if (currentValue >= dealerHitLimit)
            {
                // 1% chance for dealer to hit
                if (_random.NextDouble() < 0.01)
                {
                    _logger.LogInformation(
                        "Dealer gambled a hit at {Value} for game {GameId}!",
                        currentValue,
                        gameId
                    );
                    continue;
                }

                // otherwise, dealer stand
                _logger.LogInformation(
                    "Dealer stand at {Value} for game {GameId}!",
                    currentValue,
                    gameId
                );
                break;
            }

            await DealCardAsync(game.Id, OwnerType.Dealer);
            game = await GetGameOrThrowAsync(gameId); // refresh game state
        }

        // determine winner
        var playerHand = game.GameCards.Where(c => c.OwnerType == OwnerType.Player).ToList();
        decimal playerValue = CalculateHandValue(game.Target, playerHand);

        var dealerHand = game.GameCards.Where(c => c.OwnerType == OwnerType.Dealer).ToList();
        decimal dealerValue = CalculateHandValue(game.Target, dealerHand);

        if (dealerValue > game.Target || (playerValue <= game.Target && playerValue > dealerValue))
        {
            // player wins
            game.Money += game.Bet * 2; // I LOVE GAMBLING!!!

            if (playerValue == game.Target && dealerValue != game.Target)
            {
                game.Money += game.Bet; // blackjack pays extra!
            }
        }
        else if (playerValue == dealerValue)
        {
            // push
            game.Money += game.Bet; // return original bet
        }
        // else dealer wins, player loses the bet (money already subtracted)

        // if player money is negative, take interest
        if (game.Money < 0)
        {
            game.Money *= 1.01m;
        }

        // end round
        game.Status = GameStatus.RoundEnd;
        await _gameRepository.UpdateGameAsync(game);

        return MapGameToDTO(game);
    }

    // --- helper methods ---

    private async Task<Game> GetGameOrThrowAsync(Guid gameId)
    {
        var game = await _gameRepository.GetGameByIdAsync(gameId);
        return game ?? throw new NotFoundException($"Game with ID {gameId} not found.");
    }

    private async Task DealCardAsync(Guid gameId, OwnerType owner)
    {
        var card = await _gameRepository.GetRandomAvailableCardAsync(gameId);
        if (card == null)
        {
            throw new InvalidOperationException("No more cards available in the deck.");
        }

        var gameCard = new GameCard
        {
            GameId = gameId,
            CardId = card.Id,
            OwnerType = owner,
        };
        await _gameRepository.AddCardToGameAsync(gameCard);
    }

    /// <summary>
    /// Calculates all possible values for a given hand.
    /// </summary>
    private static List<decimal> CalculateHandValues(List<GameCard> hand)
    {
        if (hand == null || hand.Count == 0)
            return [0m];

        var possibleValues = new HashSet<decimal> { 0 };

        // apply effects in order of draw
        foreach (var gameCard in hand)
        {
            var nextPossibleValues = new HashSet<decimal>();
            foreach (var value in possibleValues)
            {
                foreach (CardEffect effect in gameCard.Card.Effects)
                {
                    nextPossibleValues.Add(value.Apply(effect));
                }
            }

            if (nextPossibleValues.Count > 0)
            {
                possibleValues = nextPossibleValues;
            }
        }

        return [.. possibleValues.OrderBy(v => v)];
    }

    /// <summary>
    /// Returns the closest value to the target value in the given hand.
    /// </summary>
    private static decimal CalculateHandValue(decimal target, List<GameCard> hand)
    {
        var possibleValues = CalculateHandValues(hand);

        // find the highest score that isn't a bust.
        var bestValue = possibleValues.Where(v => v <= target).DefaultIfEmpty(0).Max();

        // if all scores resulted in a bust, return the lowest bust value
        if (bestValue == 0 && possibleValues.Any(v => v > 0))
        {
            return possibleValues.Min();
        }
        // otherwise return best non-bust value
        return bestValue;
    }

    private static GameDTO MapGameToDTO(Game game)
    {
        var playerHand = game.GameCards.Where(c => c.OwnerType == OwnerType.Player).ToList();
        var dealerHand = game.GameCards.Where(c => c.OwnerType == OwnerType.Dealer).ToList();

        return new GameDTO(
            game.Id,
            game.Target,
            game.OverdrawLimit,
            game.Money,
            game.Round,
            game.Bet,
            game.Status,
            [.. playerHand.Select(MapGameCardToDTO)],
            CalculateHandValues(playerHand),
            [.. dealerHand.Select(MapGameCardToDTO)],
            CalculateHandValues(dealerHand),
            game.CreatedAt,
            game.UpdatedAt
        );
    }

    private static GameCardDTO MapGameCardToDTO(GameCard gc)
    {
        return new GameCardDTO(
            gc.Id,
            new CardDTO(
                gc.Card.Id,
                gc.Card.Rank,
                new SuitDTO(
                    gc.Card.Suit.Id,
                    gc.Card.Suit.Name,
                    gc.Card.Suit.Symbol,
                    gc.Card.Suit.ColorRGB
                ),
                gc.Card.Effects,
                gc.Card.CreatedAt,
                gc.Card.UpdatedAt
            ),
            gc.OwnerType
        );
    }
}
