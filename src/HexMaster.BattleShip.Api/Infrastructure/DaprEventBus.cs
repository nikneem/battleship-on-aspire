using Dapr.Client;
using HexMaster.BattleShip.Core.Eventing;
using HexMaster.BattleShip.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace HexMaster.BattleShip.Api.Infrastructure;

public sealed class DaprEventBus(DaprClient daprClient, ILogger<DaprEventBus> logger) : IEventBus
{
    private const string PubSubName = "pubsub";

    private static readonly Dictionary<Type, string> TopicMap = new()
    {
        [typeof(GameCreatedIntegrationEvent)] = IntegrationEventTopics.GameCreated,
        [typeof(PlayerJoinedGameIntegrationEvent)] = IntegrationEventTopics.PlayerJoined,
        [typeof(PlayerMarkedReadyIntegrationEvent)] = IntegrationEventTopics.PlayerMarkedReady,
        [typeof(FleetSubmittedIntegrationEvent)] = IntegrationEventTopics.FleetSubmitted,
        [typeof(FleetLockedIntegrationEvent)] = IntegrationEventTopics.FleetLocked,
        [typeof(GameStartedIntegrationEvent)] = IntegrationEventTopics.GameStarted,
        [typeof(ShotFiredIntegrationEvent)] = IntegrationEventTopics.ShotFired,
        [typeof(GameFinishedIntegrationEvent)] = IntegrationEventTopics.GameFinished,
        [typeof(GameCancelledIntegrationEvent)] = IntegrationEventTopics.GameCancelled,
        [typeof(GameAbandonedIntegrationEvent)] = IntegrationEventTopics.GameAbandoned,
        [typeof(PlayerConnectionLostIntegrationEvent)] = IntegrationEventTopics.PlayerConnectionLost,
        [typeof(PlayerConnectionReestablishedIntegrationEvent)] = IntegrationEventTopics.PlayerConnectionReestablished,
        [typeof(PlayerConnectionTimedOutIntegrationEvent)] = IntegrationEventTopics.PlayerConnectionTimedOut,
    };

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (!TopicMap.TryGetValue(typeof(TEvent), out var topic))
        {
            logger.LogWarning("No Dapr topic mapping found for event type {EventType}", typeof(TEvent).Name);
            return;
        }

        try
        {
            await daprClient.PublishEventAsync(PubSubName, topic, integrationEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish {EventType} to topic {Topic}", typeof(TEvent).Name, topic);
        }
    }
}
