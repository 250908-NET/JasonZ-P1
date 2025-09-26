using CardDeck.Api.Exceptions;
using CardDeck.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Repository;

public class CardRepository(CardDeckContext context) : ICardRepository
{
    private readonly CardDeckContext _context = context;

    public async Task<List<Card>> GetAllCardsAsync()
    {
        // include related Suit data as read-only
        return await _context.Cards.Include(c => c.Suit).AsNoTracking().ToListAsync();
    }

    public async Task<Card?> GetCardByIdAsync(int cardId)
    {
        // include related Suit data as read-only
        return await _context.Cards.Include(c => c.Suit).FirstOrDefaultAsync(c => c.Id == cardId);
    }

    public async Task<Card?> GetCardByRankAndSuitAsync(string rank, int suitId)
    {
        return await _context.Cards.FirstOrDefaultAsync(c => c.Rank == rank && c.SuitId == suitId);
    }

    public async Task<Card> CreateCardAsync(Card newCard)
    {
        _context.Cards.Add(newCard);
        await _context.SaveChangesAsync();

        await _context.Entry(newCard).Reference(c => c.Suit).LoadAsync();
        return newCard;

        // var createdCard = await _context
        //     .Cards.Include(c => c.Suit)
        //     .FirstOrDefaultAsync(c => c.Id == newCard.Id);

        // int i = 0;
        // while (createdCard == null && i < 1000)
        // {
        //     // This should never happen, but just in case, we retry fetching the created card.
        //     createdCard = await _context
        //         .Cards.Include(c => c.Suit)
        //         .FirstOrDefaultAsync(c => c.Id == newCard.Id);
        //     i++;
        // }

        // return createdCard ?? throw new Exception("Failed to retrieve the created card.");
    }

    public async Task<bool> UpdateCardAsync(Card updateCard)
    {
        _context.Entry(updateCard).State = EntityState.Modified;
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Catch the specific concurrency exception and throw our custom one.
            throw new ConflictException(
                "The card you are trying to update has been modified by another user. Please refresh and try again."
            );
        }
    }

    public async Task<bool> DeleteCardAsync(Card card)
    {
        _context.Cards.Remove(card);
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The card you are trying to delete has been modified by another user. Please refresh and try again."
            );
        }
    }
}
