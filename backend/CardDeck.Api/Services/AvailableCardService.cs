using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;

namespace CardDeck.Api.Services;

public class AvailableCardService(
    IAvailableCardRepository availableCardRepository,
    ICardRepository cardRepository,
    ILogger<AvailableCardService> logger
) : IAvailableCardService
{
    private readonly IAvailableCardRepository _availableCardRepository = availableCardRepository;
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly ILogger<AvailableCardService> _logger = logger;

    /// <summary>
    /// Helper method to map an AvailableCard entity to an AvailableCardDTO.
    /// </summary>
    private static AvailableCardDTO MapAvailableCardToDTO(AvailableCard availableCard) =>
        new(availableCard.Id, MapCardToDTO(availableCard.Card), availableCard.CreatedAt);

    /// <summary>
    /// Helper method to map a Card entity to a CardDTO.
    /// </summary>
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
    /// Helper method to check whether a given CardId exists.
    /// </summary>
    private async Task EnsureCardExistsAsync(int cardId)
    {
        _ =
            await _cardRepository.GetCardByIdAsync(cardId)
            ?? throw new BadRequestException($"Card with ID {cardId} does not exist.");
    }

    public async Task<List<AvailableCardDTO>> GetAllAvailableCardsAsync()
    {
        _logger.LogInformation("Fetching all available cards in the deck...");
        var availableCards = await _availableCardRepository.GetAllAvailableCardsAsync();
        _logger.LogInformation("Fetched {Count} available cards!", availableCards.Count);
        return [.. availableCards.Select(MapAvailableCardToDTO)];
    }

    public async Task<AvailableCardDTO> InsertAvailableCardAsync(
        CreateAvailableCardDTO newAvailableCard
    )
    {
        _logger.LogInformation(
            "Adding card with ID {CardId} to the available deck...",
            newAvailableCard.CardId
        );

        // ensure the referenced Card exists
        await EnsureCardExistsAsync(newAvailableCard.CardId);

        var availableCardEntity = new AvailableCard
        {
            CardId = newAvailableCard.CardId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var createdAvailableCard = await _availableCardRepository.CreateAvailableCardAsync(
            availableCardEntity
        );
        _logger.LogInformation(
            "Added available card with ID {AvailableCardId} (Card ID: {CardId})!",
            createdAvailableCard.Id,
            createdAvailableCard.CardId
        );
        return MapAvailableCardToDTO(createdAvailableCard);
    }

    public async Task<List<CardDTO>> DrawRandomCardsAsync(int numberOfCards)
    {
        _logger.LogInformation(
            "Attempting to draw {NumberOfCards} random cards from the deck...",
            numberOfCards
        );

        if (numberOfCards <= 0)
        {
            _logger.LogWarning(
                "Requested to draw {NumberOfCards} cards. Returning empty list.",
                numberOfCards
            );
            return [];
        }

        var randomAvailableCards = await _availableCardRepository.GetRandomAvailableCardsAsync(
            numberOfCards
        );

        if (!randomAvailableCards.Any())
        {
            _logger.LogWarning("No cards available in the deck to draw.");
            throw new NotFoundException("No cards available in the deck to draw.");
        }

        var drawnCardDTOs = new List<CardDTO>();
        foreach (var availableCard in randomAvailableCards)
        {
            var drawnCard = availableCard.Card; // store card details before deletion

            // delete the available card from the deck
            var deleted = await _availableCardRepository.DeleteAvailableCardAsync(availableCard);
            if (deleted)
            {
                _logger.LogInformation(
                    "Successfully drawn and removed card {CardRank} of {CardSuit} (AvailableCard ID: {AvailableCardId}, Card ID: {CardId}) from the deck.",
                    drawnCard.Rank,
                    drawnCard.Suit.Name,
                    availableCard.Id,
                    drawnCard.Id
                );
                drawnCardDTOs.Add(MapCardToDTO(drawnCard));
            }
            else
            {
                _logger.LogWarning(
                    "Failed to delete available card with ID {AvailableCardId} (Card ID: {CardId}) after drawing. It might have been modified concurrently.",
                    availableCard.Id,
                    drawnCard.Id
                );
                // client might need to re-request or handle fewer cards than expected
            }
        }

        if (drawnCardDTOs.Count == 0 && randomAvailableCards.Count != 0)
        {
            // cards were selected but none could be deleted due to concurrency.
            throw new ConflictException(
                "Failed to draw any cards due to concurrent modifications. Please try again."
            );
        }

        _logger.LogInformation("Successfully drawn {Count} cards.", drawnCardDTOs.Count);
        return drawnCardDTOs;
    }
}
