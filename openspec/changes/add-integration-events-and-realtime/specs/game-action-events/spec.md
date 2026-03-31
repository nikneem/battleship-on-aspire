## ADDED Requirements

### Requirement: IEventBus abstraction in Core
The system SHALL provide an `IEventBus` interface in `HexMaster.BattleShip.Core` with a single method `PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)` where `TEvent : IntegrationEvent`. A `DaprEventBus` implementation SHALL be registered in DI, wrapping `DaprClient.PublishEventAsync` and reading the topic name from the `TopicName` static property on the event type via the `IntegrationEventTopics` constants.

#### Scenario: Event bus publishes to correct Dapr topic
- **WHEN** `IEventBus.PublishAsync` is called with a `ShotFiredIntegrationEvent`
- **THEN** the Dapr client SHALL publish to the topic `battleship.game.shot-fired` on the configured pub/sub component

#### Scenario: Publish failure does not fail the HTTP request
- **WHEN** Dapr pub/sub is unavailable and a handler calls `IEventBus.PublishAsync`
- **THEN** the failure SHALL be logged and the handler SHALL still return its result DTO without throwing

### Requirement: Games command handlers publish integration events
Every Games command handler that mutates game state SHALL call `IEventBus.PublishAsync` with the corresponding integration event after a successful `gameRepository.SaveAsync`. The following handlers SHALL publish events:

| Handler | Event Published |
|---|---|
| `CreateGameHandler` | `GameCreatedIntegrationEvent` |
| `JoinGameByCodeHandler` | `PlayerJoinedGameIntegrationEvent` |
| `MarkReadyHandler` | `PlayerMarkedReadyIntegrationEvent` |
| `SubmitFleetHandler` | `FleetSubmittedIntegrationEvent` |
| `LockFleetHandler` | `FleetLockedIntegrationEvent`; additionally `GameStartedIntegrationEvent` when both players' fleets are now locked |
| `FireShotHandler` | `ShotFiredIntegrationEvent`; additionally `GameFinishedIntegrationEvent` when the game phase becomes Finished |
| `CancelGameHandler` | `GameCancelledIntegrationEvent` |
| `AbandonGameHandler` | `GameAbandonedIntegrationEvent` |

#### Scenario: Single action triggers two events when game transitions
- **WHEN** the second player calls `LockFleet` and both fleets are now locked
- **THEN** both `FleetLockedIntegrationEvent` AND `GameStartedIntegrationEvent` SHALL be published

#### Scenario: FireShot triggers game finished event when all ships sunk
- **WHEN** `FireShotHandler` processes a shot that sinks the last ship
- **THEN** both `ShotFiredIntegrationEvent` AND `GameFinishedIntegrationEvent` SHALL be published

#### Scenario: Query handlers do not publish events
- **WHEN** `GetGameStateForPlayerHandler` or `GetGameLobbyByCodeHandler` is invoked
- **THEN** no integration events SHALL be published

### Requirement: Dapr pub/sub component wired in AppHost
The Aspire AppHost SHALL add a Dapr pub/sub component named `pubsub` (Redis-backed in local development) and reference it from both the API project and the Realtime project so that both can publish and subscribe.

#### Scenario: API project can publish events at runtime
- **WHEN** the Aspire application starts in local development
- **THEN** the API resource SHALL have access to the `pubsub` Dapr component without manual configuration
