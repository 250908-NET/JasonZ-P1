using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;

namespace CardDeck.Api.Endpoints;

public static class SuitEndpoints
{
    public static void MapSuitEndpoints(
        this WebApplication app,
        string prefix = "/suits",
        params string[] tags
    )
    {
        if (tags is not { Length: > 0 })
        {
            tags = ["Suits"];
        }
        var group = app.MapGroup(prefix).WithTags(tags);

        group
            .MapGet(
                "/",
                async (ISuitService service) =>
                {
                    return Results.Ok(await service.GetAllSuitsAsync());
                }
            )
            .WithName("GetAllSuits")
            .Produces<List<SuitDTO>>(StatusCodes.Status200OK)
            .WithOpenApi();

        group
            .MapGet(
                "/{suitId:int}",
                async (int suitId, ISuitService service) =>
                {
                    return Results.Ok(await service.GetSuitByIdAsync(suitId));
                }
            )
            .WithName("GetSuitById")
            .Produces<SuitDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group
            .MapPost(
                "/",
                async (CreateSuitDTO newSuit, ISuitService service) =>
                {
                    var createdSuit = await service.CreateSuitAsync(newSuit);
                    return Results.Created($"{prefix}/{createdSuit.Id}", createdSuit);
                }
            )
            .WithName("CreateSuit")
            .Produces<SuitDTO>(StatusCodes.Status201Created)
            .ProducesValidationProblem() // for 400 validation errors
            .WithOpenApi();

        group
            .MapPut(
                "/{suitId:int}",
                async (int suitId, UpdateSuitDTO updateSuit, ISuitService service) =>
                {
                    await service.UpdateSuitAsync(suitId, updateSuit);
                    return Results.NoContent();
                }
            )
            .WithName("UpdateSuit")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem() // for 400 validation errors
            .WithOpenApi();

        group
            .MapPatch(
                "/{suitId:int}",
                async (int suitId, PartialUpdateSuitDTO partialSuit, ISuitService service) =>
                {
                    await service.PartialUpdateSuitAsync(suitId, partialSuit);
                    return Results.NoContent();
                }
            )
            .WithName("PatchSuit")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem() // for 400 validation errors
            .WithOpenApi();

        group
            .MapDelete(
                "/{suitId:int}",
                async (int suitId, ISuitService service) =>
                {
                    await service.DeleteSuitAsync(suitId);
                    return Results.NoContent();
                }
            )
            .WithName("DeleteSuit")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }
}
