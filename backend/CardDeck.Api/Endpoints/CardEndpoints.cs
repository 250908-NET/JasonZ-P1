using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace CardDeck.Api.Endpoints;

public static class CardEndpoints
{
    public static void MapCardEndpoints(
        this WebApplication app,
        string prefix = "/cards",
        params string[] tags
    )
    {
        if (tags is not { Length: > 0 })
        {
            tags = ["Cards"];
        }
        var group = app.MapGroup(prefix).WithTags(tags).AddFluentValidationAutoValidation();

        group
            .MapGet(
                "/",
                async (ICardService service) =>
                {
                    return Results.Ok(await service.GetAllCardsAsync());
                }
            )
            .WithName("GetAllCards")
            .Produces<List<CardDTO>>(StatusCodes.Status200OK)
            .WithOpenApi();

        group
            .MapGet(
                "/{cardId:int}",
                async (int cardId, ICardService service) =>
                {
                    return Results.Ok(await service.GetCardByIdAsync(cardId));
                }
            )
            .WithName("GetCardById")
            .Produces<CardDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group
            .MapPost(
                "/",
                async (CreateCardDTO newCard, ICardService service) =>
                {
                    var createdCard = await service.CreateCardAsync(newCard);
                    return Results.Created($"{prefix}/{createdCard.Id}", createdCard);
                }
            )
            .WithName("CreateCard")
            .Produces<CardDTO>(StatusCodes.Status201Created)
            .ProducesValidationProblem() // for 400 validation errors
            .WithOpenApi();

        group
            .MapPut(
                "/{cardId:int}",
                async (int cardId, UpdateCardDTO updateCard, ICardService service) =>
                {
                    await service.UpdateCardAsync(cardId, updateCard);
                    return Results.NoContent();
                }
            )
            .WithName("UpdateCard")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem() // for 400 validation errors
            .WithOpenApi();

        group
            .MapPatch(
                "/{cardId:int}",
                async (int cardId, PartialUpdateCardDTO patchCard, ICardService service) =>
                {
                    await service.PartialUpdateCardAsync(cardId, patchCard);
                    return Results.NoContent();
                }
            )
            .WithName("PartialUpdateCard")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem() // for 400 validation errors
            .WithOpenApi();

        group
            .MapDelete(
                "/{cardId:int}",
                async (int cardId, ICardService service) =>
                {
                    await service.DeleteCardAsync(cardId);
                    return Results.NoContent();
                }
            )
            .WithName("DeleteCard")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }
}
