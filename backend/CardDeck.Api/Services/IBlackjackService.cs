using CardDeck.Api.Models.DTOs;

namespace CardDeck.Api.Services;

public interface IBlackjackService
{
    Task<GameDTO> StartNewGameAsync(StartGameDTO startGameDTO);
    Task<GameDTO> GetGameStateAsync(Guid gameId);
    Task<GameDTO> StartRoundAsync(Guid gameId, PlaceBetDTO placeBetDTO);
    Task<GameDTO> PlayerHitAsync(Guid gameId);
    Task<GameDTO> PlayerStandAsync(Guid gameId);
}
