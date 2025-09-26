using CardDeck.Api.Models;

namespace CardDeck.Api.Repository;

public interface ICardRepository
{
    Task<List<Card>> GetAllCardsAsync();
    Task<Card?> GetCardByIdAsync(int cardId);
    Task<Card?> GetCardByRankAndSuitAsync(string rank, int suitId);
    Task<Card> CreateCardAsync(Card newCard);
    Task<bool> UpdateCardAsync(Card updateCard);
    Task<bool> DeleteCardAsync(Card card);
}
