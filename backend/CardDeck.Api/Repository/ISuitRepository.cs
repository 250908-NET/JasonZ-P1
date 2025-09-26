using CardDeck.Api.Models;

namespace CardDeck.Api.Repository;

public interface ISuitRepository
{
    Task<List<Suit>> GetAllSuitsAsync();
    Task<Suit?> GetSuitByIdAsync(int suitId);
    Task<Suit> CreateSuitAsync(Suit newSuit);
    Task<bool> UpdateSuitAsync(Suit updateSuit);
    Task<bool> DeleteSuitAsync(Suit suit);
}
