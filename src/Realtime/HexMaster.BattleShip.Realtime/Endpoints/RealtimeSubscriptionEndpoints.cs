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
        app.MapPost("/subscriptions/realtime/player-joined",
            async (PlayerJoinedGameIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("PlayerJoined", evt.GuestPlayerId, evt.GuestPlayerName);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.PlayerJoined);

        app.MapPost("/subscriptions/realtime/player-marked-ready",
            async (PlayerMarkedReadyIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("PlayerReady", evt.PlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.PlayerMarkedReady);

        app.MapPost("/subscriptions/realtime/fleet-submitted",
            async (FleetSubmittedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("FleetSubmitted", evt.PlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.FleetSubmitted);

        app.MapPost("/subscriptions/realtime/fleet-locked",
            async (FleetLockedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("FleetLocked", evt.PlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.FleetLocked);

        app.MapPost("/subscriptions/realtime/game-started",
            async (GameStartedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameStarted", evt.FirstTurnPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameStarted);

        app.MapPost("/subscriptions/realtime/shot-fired",
            async (ShotFiredIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode)
                    .SendAsync("ShotFired", evt.FiringPlayerId, evt.TargetRow, evt.TargetColumn, (int)evt.Outcome);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.ShotFired);

        app.MapPost("/subscriptions/realtime/game-finished",
            async (GameFinishedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameFinished", evt.WinnerPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameFinished);

        app.MapPost("/subscriptions/realtime/game-cancelled",
            async (GameCancelledIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameCancelled", evt.CancelledByPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameCancelled);

        app.MapPost("/subscriptions/realtime/game-abandoned",
            async (GameAbandonedIntegrationEvent evt, IHubContext<GameHub> hub) =>
            {
                await hub.Clients.Group(evt.GameCode).SendAsync("GameAbandoned", evt.AbandoningPlayerId);
                return Results.Ok();
            }).WithTopic("pubsub", IntegrationEventTopics.GameAbandoned);

        return app;
    }
}
