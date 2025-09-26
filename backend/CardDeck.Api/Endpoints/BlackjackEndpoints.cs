using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace CardDeck.Api.Endpoints;

public static class BlackjackEndpoints
{
    public static void MapBlackjackEndpoints(
        this WebApplication app,
        string prefix = "/blackjack",
        params string[] tags
    )
    {
        if (tags is not { Length: > 0 })
        {
            tags = ["Blackjack"];
        }
        var group = app.MapGroup(prefix).WithTags(tags).AddFluentValidationAutoValidation();

        group
            .MapPost(
                "/",
                async (StartGameDTO dto, IBlackjackService service) =>
                    Results.Ok(await service.StartNewGameAsync(dto))
            )
            .WithName("StartGame")
            .Produces<List<AvailableCardDTO>>(StatusCodes.Status200OK)
            .WithOpenApi();

        group
            .MapGet(
                "/{gameId:Guid}",
                async (Guid gameId, IBlackjackService service) =>
                    await service.GetGameStateAsync(gameId)
            )
            .WithName("GetGameStatus")
            .Produces<GameDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group
            .MapPost(
                "/{gameId:Guid}/{action}",
                async (Guid gameId, PlaceBetDTO? dto, string action, IBlackjackService service) =>
                {
                    switch (action.ToLowerInvariant())
                    {
                        case "bet":
                            ArgumentNullException.ThrowIfNull(dto);
                            await service.StartRoundAsync(gameId, dto);
                            break;
                        case "hit":
                            await service.PlayerHitAsync(gameId);
                            break;
                        case "stand":
                            await service.PlayerStandAsync(gameId);
                            break;
                        default:
                            throw new ArgumentException($"Invalid action: {action}.");
                    }
                }
            )
            .WithName("DoPlayerAction")
            .Produces<GameDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }
}
