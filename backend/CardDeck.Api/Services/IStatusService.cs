namespace CardDeck.Api.Services;
public record StatusResult(Dictionary<string, bool> Status);

public interface IStatusService
{
    public Task<StatusResult> CheckConnectionAsync();
}
