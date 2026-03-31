using Dapr;
using HexMaster.BattleShip.Core.Cqrs;
using HexMaster.BattleShip.Games.Abstractions.DataTransferObjects;
using HexMaster.BattleShip.Games.Features.AbandonGame;
using HexMaster.BattleShip.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HexMaster.BattleShip.Games.Endpoints;

public static class GamesSubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapGamesSubscriptions(this IEndpointRouteBuilder app)
    {
        app.MapPost("/subscriptions/games/connection-timed-out",
            async (PlayerConnectionTimedOutIntegrationEvent evt,
                ICommandHandler<AbandonGameCommand, GameStateResponseDto> handler) =>
            {
                try
                {
                    await handler.HandleAsync(new AbandonGameCommand(evt.GameCode, evt.PlayerId));
                }
                catch
                {
                    // Game may already be in a terminal state — swallow silently
                }

                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.PlayerConnectionTimedOut);

        return app;
    }
}
