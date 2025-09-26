using CardDeck.Api.Models.DTOs;

namespace CardDeck.Api.Services;

public interface ISuitService
{
    Task<List<SuitDTO>> GetAllSuitsAsync();
    Task<SuitDTO?> GetSuitByIdAsync(int suitId);
    Task<SuitDTO> CreateSuitAsync(CreateSuitDTO newSuit);
    Task<bool> UpdateSuitAsync(int suitId, UpdateSuitDTO updateSuit);
    Task<bool> PartialUpdateSuitAsync(int suitId, PartialUpdateSuitDTO patchSuit);
    Task<bool> DeleteSuitAsync(int suitId);
}
