using Dapr;
using HexMaster.BattleShip.IntegrationEvents;
using HexMaster.BattleShip.Realtime.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;

namespace HexMaster.BattleShip.Realtime.Endpoints;

public static class RealtimeSubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapRealtimeSubscriptions(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/subscriptions/realtime").AllowAnonymous();

        group.MapPost("/player-joined",
            async (PlayerJoinedGameIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("PlayerJoined", evt.GuestPlayerId, evt.GuestPlayerName);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.PlayerJoined);

        group.MapPost("/player-marked-ready",
            async (PlayerMarkedReadyIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("PlayerReady", evt.PlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.PlayerMarkedReady);

        group.MapPost("/fleet-submitted",
            async (FleetSubmittedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("FleetSubmitted", evt.PlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.FleetSubmitted);

        group.MapPost("/fleet-locked",
            async (FleetLockedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("FleetLocked", evt.PlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.FleetLocked);

        group.MapPost("/game-started",
            async (GameStartedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameStarted", evt.FirstTurnPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameStarted);

        group.MapPost("/shot-fired",
            async (ShotFiredIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode)
                    .SendAsync("ShotFired", evt.FiringPlayerId, evt.TargetRow, evt.TargetColumn, (int)evt.Outcome);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.ShotFired);

        group.MapPost("/game-finished",
            async (GameFinishedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameFinished", evt.WinnerPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameFinished);

        group.MapPost("/game-cancelled",
            async (GameCancelledIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameCancelled", evt.CancelledByPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameCancelled);

        group.MapPost("/game-abandoned",
            async (GameAbandonedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameAbandoned", evt.AbandoningPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameAbandoned);

        return app;
    }
}
