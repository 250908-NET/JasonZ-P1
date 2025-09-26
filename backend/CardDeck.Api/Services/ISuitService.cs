using CardDeck.Api.Models.DTOs;

namespace CardDeck.Api.Services;

public interface ISuitService
{
    Task<List<SuitDTO>> GetAllSuitsAsync();
    Task<SuitDTO?> GetSuitByIdAsync(int suitId);
    Task<SuitDTO> CreateSuitAsync(CreateSuitDTO newSuit);
    Task UpdateSuitAsync(int suitId, UpdateSuitDTO updateSuit);
    Task PartialUpdateSuitAsync(int suitId, PartialUpdateSuitDTO patchSuit);
    Task DeleteSuitAsync(int suitId);
}
