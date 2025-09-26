using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;

namespace CardDeck.Api.Services;

public class CardService(
    ICardRepository cardRepository,
    ISuitRepository suitRepository,
    ILogger<CardService> logger
) : ICardService
{
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly ISuitRepository _suitRepository = suitRepository;
    private readonly ILogger<CardService> _logger = logger;

    /// <summary>
    /// Helper method to map a Card entity to a CardDTO.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private static CardDTO MapCardToDTO(Card card) =>
        new(
            card.Id,
            card.Rank,
            new SuitDTO(card.Suit.Id, card.Suit.Name, card.Suit.Symbol, card.Suit.ColorRGB),
            card.Effects,
            card.CreatedAt,
            card.UpdatedAt
        );

    /// <summary>
    /// Helper method to check whether a given SuitId exists.
    /// </summary>
    private async Task EnsureSuitExistsAsync(int suitId)
    {
        _ =
            await _suitRepository.GetSuitByIdAsync(suitId)
            ?? throw new BadRequestException($"Suit with ID {suitId} does not exist.");
    }

    public async Task<List<CardDTO>> GetAllCardsAsync()
    {
        _logger.LogInformation("Fetching all cards...");
        var cards = await _cardRepository.GetAllCardsAsync();
        _logger.LogInformation("Fetched {Count} cards!", cards.Count);
        return [.. cards.Select(MapCardToDTO)];
    }

    public async Task<CardDTO> GetCardByIdAsync(int cardId)
    {
        _logger.LogInformation("Fetching card with ID {CardId}...", cardId);
        var card =
            await _cardRepository.GetCardByIdAsync(cardId)
            ?? throw new NotFoundException($"Card with ID {cardId} was not found.");
        _logger.LogInformation(
            "Fetched card {CardRank} of {CardSuit} (ID: {CardId})!",
            card.Rank,
            card.Suit.Name,
            card.Id
        );
        return MapCardToDTO(card);
    }

    public async Task<CardDTO> CreateCardAsync(CreateCardDTO newCard)
    {
        _logger.LogInformation(
            "Creating new card {Rank} of Suit ID {SuitId} (Effect: {Effect})...",
            newCard.Rank,
            newCard.SuitId,
            newCard.Effects
        );

        // ensure the referenced Suit exists
        await EnsureSuitExistsAsync(newCard.SuitId);

        // prevent duplicate cards
        if (await _cardRepository.GetCardByRankAndSuitAsync(newCard.Rank, newCard.SuitId) != null)
        {
            throw new ConflictException(
                $"A card with Rank '{newCard.Rank}' and Suit ID '{newCard.SuitId}' already exists."
            );
        }

        var cardEntity = new Card
        {
            Rank = newCard.Rank,
            SuitId = newCard.SuitId,
            Effects = newCard.Effects,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var createdCard = await _cardRepository.CreateCardAsync(cardEntity);
        _logger.LogInformation("Created card with ID {CardId}!", createdCard.Id);
        return MapCardToDTO(createdCard);
    }

    public async Task UpdateCardAsync(int cardId, UpdateCardDTO updateCard)
    {
        _logger.LogInformation("Updating card with ID {CardId}...", cardId);

        // ensure the referenced Suit exists
        await EnsureSuitExistsAsync(updateCard.SuitId);

        var existingCard =
            await _cardRepository.GetCardByIdAsync(cardId)
            ?? throw new NotFoundException($"Card with ID {cardId} not found for update.");

        // if found apply changes
        existingCard.Rank = updateCard.Rank;
        existingCard.SuitId = updateCard.SuitId;
        existingCard.Effects = updateCard.Effects;
        existingCard.UpdatedAt = DateTimeOffset.UtcNow;

        await _cardRepository.UpdateCardAsync(existingCard);
        _logger.LogInformation("Successfully updated card with ID {CardId}!", cardId);
    }

    public async Task PartialUpdateCardAsync(int cardId, PartialUpdateCardDTO partialCard)
    {
        _logger.LogInformation("Patching card with ID {CardId}...", cardId);

        // ensure the referenced Suit exists
        if (partialCard.SuitId.HasValue)
            await EnsureSuitExistsAsync(partialCard.SuitId.Value);

        var existingCard =
            await _cardRepository.GetCardByIdAsync(cardId)
            ?? throw new NotFoundException($"Card with ID {cardId} not found for patch.");

        // if found apply changes
        if (partialCard.Rank != null)
            existingCard.Rank = partialCard.Rank;
        if (partialCard.SuitId.HasValue)
            existingCard.SuitId = partialCard.SuitId.Value;
        if (partialCard.Effects != null)
            existingCard.Effects = partialCard.Effects;
        existingCard.UpdatedAt = DateTimeOffset.UtcNow;

        await _cardRepository.UpdateCardAsync(existingCard);
        _logger.LogInformation("Successfully patched card with ID {CardId}!", cardId);
    }

    public async Task DeleteCardAsync(int cardId)
    {
        _logger.LogInformation("Deleting card with ID {CardId}...", cardId);
        var cardToDelete =
            await _cardRepository.GetCardByIdAsync(cardId)
            ?? throw new NotFoundException($"Card with ID {cardId} not found for deletion.");

        await _cardRepository.DeleteCardAsync(cardToDelete);
        _logger.LogInformation("Successfully deleted card with ID {CardId}!", cardId);
    }
}
