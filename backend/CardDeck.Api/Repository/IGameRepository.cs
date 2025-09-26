using CardDeck.Api.Models;

namespace CardDeck.Api.Repository;

public interface IGameRepository
{
    Task<Game> CreateGameAsync(Game newGame);
    Task<Game?> GetGameByIdAsync(Guid gameId);
    Task<bool> UpdateGameAsync(Game game);
    Task AddCardToGameAsync(GameCard gameCard);
    Task ClearGameCardsAsync(Guid gameId);
    Task<Card?> GetRandomAvailableCardAsync(Guid gameId);
}
