using CardDeck.Api.Models;

namespace CardDeck.Api.Repository;

public interface IAvailableCardRepository
{
    Task<List<AvailableCard>> GetAllAvailableCardsAsync();
    Task<AvailableCard?> GetAvailableCardByIdAsync(int id);
    Task<AvailableCard> CreateAvailableCardAsync(AvailableCard availableCard);
    Task<bool> DeleteAvailableCardAsync(AvailableCard availableCard);
    Task<List<AvailableCard>> GetRandomAvailableCardsAsync(int numberOfCards);
}
