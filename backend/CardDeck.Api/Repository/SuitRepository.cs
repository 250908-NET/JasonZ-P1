using CardDeck.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Repository;

public class SuitRepository(CardDeckContext context) : ISuitRepository
{
    private readonly CardDeckContext _context = context;

    public async Task<List<Suit>> GetAllSuitsAsync()
    {
        return await _context.Suits.ToListAsync();
    }

    public async Task<Suit?> GetSuitByIdAsync(int suitId)
    {
        return await _context.Suits.FindAsync(suitId);
    }

    public async Task<Suit> CreateSuitAsync(Suit newSuit)
    {
        _context.Suits.Add(newSuit);
        await _context.SaveChangesAsync();
        return newSuit;
    }

    public async Task<bool> UpdateSuitAsync(Suit updateSuit)
    {
        _context.Entry(updateSuit).State = EntityState.Modified;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteSuitAsync(Suit suit)
    {
        _context.Suits.Remove(suit);
        return await _context.SaveChangesAsync() > 0;
    }
}
