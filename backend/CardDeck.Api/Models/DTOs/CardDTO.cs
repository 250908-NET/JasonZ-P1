using FluentValidation;

namespace CardDeck.Api.Models.DTOs;

// output DTO for a card (including suit details)
public record CardDTO(
    int Id,
    string Rank,
    SuitDTO Suit,
    List<CardEffect> Effects,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

// input DTO for creating a new card
public record CreateCardDTO(string Rank, int SuitId, List<CardEffect> Effects);

// input DTO for updating an existing card
