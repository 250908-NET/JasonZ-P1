using CardDeck.Api.Models;
using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Repository;

namespace CardDeck.Api.Services;

public class SuitService(ISuitRepository suitRepository, ILogger<SuitService> logger) : ISuitService
{
    private readonly ISuitRepository _suitRepository = suitRepository;
    private readonly ILogger<SuitService> _logger = logger;

    public async Task<List<SuitDTO>> GetAllSuitsAsync()
    {
        _logger.LogInformation("Fetching all suits...");
        var suits = await _suitRepository.GetAllSuitsAsync();
        _logger.LogInformation("Fetched {Count} suits!", suits.Count);
        return [.. suits.Select(s => new SuitDTO(s.Id, s.Name, s.Symbol, s.ColorRGB))];
    }

    public async Task<SuitDTO?> GetSuitByIdAsync(int suitId)
    {
        _logger.LogInformation("Fetching suit with ID {SuitId}...", suitId);
        var suit = await _suitRepository.GetSuitByIdAsync(suitId);

        if (suit == null)
        {
            _logger.LogWarning("Suit with ID {SuitId} not found.", suitId);
            return null;
        }

        _logger.LogInformation("Fetched suit {SuitName} (ID: {SuitId})!", suit.Name, suit.Id);
        return new SuitDTO(suit.Id, suit.Name, suit.Symbol, suit.ColorRGB);
    }

    public async Task<SuitDTO> CreateSuitAsync(CreateSuitDTO newSuit)
    {
        _logger.LogInformation("Creating new suit {SuitName}...", newSuit.Name);
        var suit = new Suit
        {
            Name = newSuit.Name,
            Symbol = newSuit.Symbol,
            ColorRGB = newSuit.ColorRGB,
        };
        var createdSuit = await _suitRepository.CreateSuitAsync(suit);

        _logger.LogInformation(
            "Created suit {SuitName} with ID {SuitId}!",
            createdSuit.Name,
            createdSuit.Id
        );
        return new SuitDTO(
            createdSuit.Id,
            createdSuit.Name,
            createdSuit.Symbol,
            createdSuit.ColorRGB
        );
    }

    public async Task<bool> UpdateSuitAsync(int suitId, UpdateSuitDTO updateSuit)
    {
        _logger.LogInformation("Updating suit with ID {SuitId}...", suitId);

        var existingSuit = await _suitRepository.GetSuitByIdAsync(suitId);

        if (existingSuit == null)
        {
            _logger.LogWarning("Suit with ID {SuitId} not found for update.", suitId);
            return false;
        }

        // if found apply changes
        existingSuit.Name = updateSuit.Name;
        existingSuit.Symbol = updateSuit.Symbol;
        existingSuit.ColorRGB = updateSuit.ColorRGB;

        if (!await _suitRepository.UpdateSuitAsync(existingSuit))
        {
            _logger.LogError("Failed to update suit with ID {SuitId}.", suitId);
            return false;
        }

        _logger.LogInformation("Successfully updated suit with ID {SuitId}!", suitId);
        return true;
    }

    public async Task<bool> PartialUpdateSuitAsync(int suitId, PartialUpdateSuitDTO partialSuit)
    {
        _logger.LogInformation("Patching suit with ID {SuitId}...", suitId);

        var existingSuit = await _suitRepository.GetSuitByIdAsync(suitId);

        if (existingSuit == null)
        {
            _logger.LogWarning("Suit with ID {SuitId} not found for patch.", suitId);
            return false;
        }

        // if found apply changes
        if (partialSuit.Name != null)
            existingSuit.Name = partialSuit.Name;
        if (partialSuit.Symbol != null)
            existingSuit.Symbol = partialSuit.Symbol.Value;
        if (partialSuit.ColorRGB != null)
            existingSuit.ColorRGB = partialSuit.ColorRGB.Value;

        if (!await _suitRepository.UpdateSuitAsync(existingSuit))
        {
            _logger.LogError("Failed to patch suit with ID {SuitId}.", suitId);
            return false;
        }

        _logger.LogInformation("Successfully patched suit with ID {SuitId}!", suitId);
        return true;
    }

    public async Task<bool> DeleteSuitAsync(int suitId)
    {
        _logger.LogInformation("Deleting suit with ID {SuitId}...", suitId);

        var existingSuit = await _suitRepository.GetSuitByIdAsync(suitId);

        if (existingSuit == null)
        {
            _logger.LogWarning("Suit with ID {SuitId} not found for deletion.", suitId);
            return false;
        }

        if (!await _suitRepository.DeleteSuitAsync(existingSuit))
        {
            _logger.LogError("Failed to delete suit with ID {SuitId}.", suitId);
            return false;
        }

        _logger.LogInformation("Successfully deleted suit with ID {SuitId}!", suitId);
        return true;
    }
}
