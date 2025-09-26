using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace CardDeck.Api.Endpoints;

public static class DeckEndpoints
{
    public static void MapDeckEndpoints(
        this WebApplication app,
        string prefix = "/deck",
        params string[] tags
    )
    {
        if (tags is not { Length: > 0 })
        {
            tags = ["Deck"];
        }
        var group = app.MapGroup(prefix).WithTags(tags).AddFluentValidationAutoValidation();

        group
            .MapGet(
                "/",
                async (IAvailableCardService service) =>
                    Results.Ok(await service.GetAllAvailableCardsAsync())
            )
            .WithName("GetDeck")
            .Produces<List<AvailableCardDTO>>(StatusCodes.Status200OK)
            .WithOpenApi();

        group
            .MapPost(
                "/",
                async (IAvailableCardService service, CreateAvailableCardDTO dto) =>
                    Results.Ok(await service.InsertAvailableCardAsync(dto))
            )
            .WithName("AddCardToDeck")
            .Produces<AvailableCardDTO>(StatusCodes.Status200OK)
            .WithOpenApi();

        group
            .MapPost(
                "/draw/{numberOfCards:int}",
                async (int numberOfCards, IAvailableCardService service) =>
                    Results.Json(await service.DrawRandomCardsAsync(numberOfCards))
            )
            .WithName("DrawCards")
            .Produces<List<CardDTO>>(StatusCodes.Status200OK)
            .WithOpenApi();
    }
}
