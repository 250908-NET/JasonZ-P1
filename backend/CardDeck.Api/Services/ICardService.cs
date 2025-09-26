using CardDeck.Api.Models.DTOs;

namespace CardDeck.Api.Services;

public interface ICardService
{
    Task<List<CardDTO>> GetAllCardsAsync();
    Task<CardDTO> GetCardByIdAsync(int cardId);
    Task<CardDTO> CreateCardAsync(CreateCardDTO newCard);
    Task UpdateCardAsync(int cardId, UpdateCardDTO updateCard);
    Task PartialUpdateCardAsync(int cardId, PartialUpdateCardDTO partialCard);
    Task DeleteCardAsync(int cardId);
}
