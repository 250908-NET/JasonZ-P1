using CardDeck.Api.Models.DTOs;
using CardDeck.Api.Services;

namespace CardDeck.Api.Endpoints;

public static class StatusEndpoints
{
    public static void MapStatusEndpoints(
        this WebApplication app,
        string prefix = "/status",
        params string[] tags
    )
    {
        if (tags is not { Length: > 0 })
        {
            tags = ["Status"];
        }
        var group = app.MapGroup(prefix).WithTags(tags);

        group
            .MapGet(
                "/",
                async (IStatusService statusService) => await statusService.CheckConnectionAsync()
            )
            .WithName("GetStatus")
            .Produces<StatusDTO>(StatusCodes.Status200OK)
            .WithOpenApi();
    }
}
