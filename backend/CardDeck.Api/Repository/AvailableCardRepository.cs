using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Repository;

public class AvailableCardRepository(CardDeckContext context) : IAvailableCardRepository
{
    private readonly CardDeckContext _context = context;

    public async Task<List<AvailableCard>> GetAllAvailableCardsAsync()
    {
        return await _context
            .AvailableCards.Include(ac => ac.Card)
            .ThenInclude(c => c.Suit)
            .ToListAsync();
    }

    public async Task<AvailableCard?> GetAvailableCardByIdAsync(int availableCardId)
    {
        return await _context.AvailableCards.FindAsync(availableCardId);
    }

    public async Task<AvailableCard> CreateAvailableCardAsync(AvailableCard newAvailableCard)
    {
        _context.AvailableCards.Add(newAvailableCard);
        await _context.SaveChangesAsync();
        return newAvailableCard;
    }

    public async Task<bool> DeleteAvailableCardAsync(AvailableCard availableCard)
    {
        _context.AvailableCards.Remove(availableCard);
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The available card you are trying to delete has been modified by another user. Please refresh and try again."
            );
        }
    }

    public async Task<List<AvailableCard>> GetRandomAvailableCardsAsync(int numberOfCards)
    {
        if (numberOfCards <= 0)
        {
            return [];
        }

        var availableCardIds = await _context.AvailableCards.Select(ac => ac.Id).ToListAsync();
        if (availableCardIds.Count == 0)
        {
            return [];
        }

        // shuffle the list of IDs using the Fisher-Yates algorithm
        // presumably
        // i dunno
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        var rng = new Random();
        int n = availableCardIds.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (availableCardIds[n], availableCardIds[k]) = (availableCardIds[k], availableCardIds[n]);
        }

        // take the requested number of IDs (or all if numberOfCards > count)
        var selectedIds = availableCardIds
            .Take(Math.Min(numberOfCards, availableCardIds.Count))
            .ToList();

        // fetch the corresponding AvailableCard entities for the selected IDs, including navigation properties
        return await _context
            .AvailableCards.Where(ac => selectedIds.Contains(ac.Id))
            .Include(ac => ac.Card)
            .ThenInclude(c => c.Suit)
            .ToListAsync();
    }
}
