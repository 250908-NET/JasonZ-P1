using CardDeck.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Repository;

public class GameRepository(CardDeckContext context) : IGameRepository
{
    private readonly CardDeckContext _context = context;

    public async Task<Game> CreateGameAsync(Game newGame)
    {
        _context.Games.Add(newGame);
        await _context.SaveChangesAsync();
        return newGame;
    }

    public async Task<Game?> GetGameByIdAsync(Guid gameId)
    {
        // also fetches the cards in the game's current hands, including card and suit details
        return await _context
            .Games.Include(g => g.GameCards)
            .ThenInclude(gc => gc.Card)
            .ThenInclude(c => c.Suit)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<bool> UpdateGameAsync(Game game)
    {
        _context.Entry(game).State = EntityState.Modified;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task AddCardToGameAsync(GameCard gameCard)
    {
        _context.GameCards.Add(gameCard);
        await _context.SaveChangesAsync();
    }

    public async Task ClearGameCardsAsync(Guid gameId)
    {
        var gameCards = _context.GameCards.Where(gc => gc.GameId == gameId);
        if (gameCards.Any())
        {
            _context.GameCards.RemoveRange(gameCards);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Card?> GetRandomAvailableCardAsync(Guid gameId)
    {
        var usedCardIds = await _context
            .GameCards.Where(gc => gc.GameId == gameId)
            .Select(gc => gc.CardId)
            .ToListAsync();

        // This method of getting a random row is not highly performant on large datasets,
        // but is perfectly acceptable for a 52-card deck.
        var randomCard = await _context
            .Cards.Include(c => c.Suit)
            .Where(c => !usedCardIds.Contains(c.Id))
            .OrderBy(c => Guid.NewGuid())
            .FirstOrDefaultAsync();

        return randomCard;
    }
}
