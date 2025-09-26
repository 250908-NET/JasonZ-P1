using CardDeck.Api.Models.DTOs;

namespace CardDeck.Api.Services;

public interface IAvailableCardService
{
    Task<List<AvailableCardDTO>> GetAllAvailableCardsAsync();
    Task<AvailableCardDTO> InsertAvailableCardAsync(CreateAvailableCardDTO newAvailableCard);
    Task<List<CardDTO>> DrawRandomCardsAsync(int numberOfCards);
}
