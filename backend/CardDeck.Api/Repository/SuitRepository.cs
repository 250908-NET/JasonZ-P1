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
        var existingSuit = await _context.Suits.FindAsync(updateSuit.Id);
        if (existingSuit == null)
        {
            return false;
        }

        existingSuit.Name = updateSuit.Name;
        existingSuit.Symbol = updateSuit.Symbol;
        existingSuit.ColorRGB = updateSuit.ColorRGB;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSuitAsync(int suitId)
    {
        // return false if suit not found
        var suit = await _context.Suits.FindAsync(suitId);
        if (suit == null)
        {
            return false;
        }

        _context.Suits.Remove(suit);
        return await _context.SaveChangesAsync() > 0;
    }
}
