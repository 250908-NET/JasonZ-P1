namespace CardDeck.Api.Models.DTOs;

// DTO for returning a suit
public record SuitDTO(int Id, string Name, char Symbol, int ColorRGB);

// DTO for creating a new suit
public record CreateSuitDTO(string Name, char Symbol, int ColorRGB);

// DTO for updating an existing suit
public record UpdateSuitDTO(string Name, char Symbol, int ColorRGB);
