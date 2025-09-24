using CardDeck.Api.Models;

namespace CardDeck.Api.Services;

public class StatusService(CardDeckContext dbContext, ILogger<StatusService> logger) : IStatusService
{
    private readonly CardDeckContext _dbContext = dbContext;
    private readonly ILogger<StatusService> _logger = logger;

    public async Task<StatusResult> CheckConnectionAsync()
    {
        bool isConnected = false;

        try
        {
            isConnected = await _dbContext.Database.CanConnectAsync();
            if (!isConnected)
            {
                _logger.LogWarning("Database connection failed: CanConnectAsync returned false.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection failed.");
        }

        return new StatusResult(new Dictionary<string, bool>
        {
            { "Database", isConnected }
        });
    }
}
