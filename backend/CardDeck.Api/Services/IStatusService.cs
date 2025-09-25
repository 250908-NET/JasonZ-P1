using CardDeck.Api.Models.DTOs;

namespace CardDeck.Api.Services;

public interface IStatusService
{
    public Task<StatusDTO> CheckConnectionAsync();
}
