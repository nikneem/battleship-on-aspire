## 1. IntegrationEvents Library

- [x] 1.1 Create project `src/IntegrationEvents/HexMaster.BattleShip.IntegrationEvents` and add to `Battleship.slnx`
- [x] 1.2 Implement abstract `IntegrationEvent` base record with `EventId`, `OccurredOn`, and abstract `SchemaVersion`
- [x] 1.3 Implement all Games domain integration event records (`GameCreatedIntegrationEvent`, `PlayerJoinedGameIntegrationEvent`, `PlayerMarkedReadyIntegrationEvent`, `FleetSubmittedIntegrationEvent`, `FleetLockedIntegrationEvent`, `GameStartedIntegrationEvent`, `ShotFiredIntegrationEvent`, `GameFinishedIntegrationEvent`, `GameCancelledIntegrationEvent`, `GameAbandonedIntegrationEvent`)
- [x] 1.4 Implement all Realtime connection lifecycle event records (`PlayerConnectionLostIntegrationEvent`, `PlayerConnectionReestablishedIntegrationEvent`, `PlayerConnectionTimedOutIntegrationEvent`)
- [x] 1.5 Implement static `IntegrationEventTopics` class with one string constant per event type following `battleship.{domain}.{verb-past-tense}` naming
- [x] 1.6 Verify the project has zero references to other solution projects and no external NuGet dependencies

## 2. IEventBus Abstraction

- [ ] 2.1 Add `IEventBus` interface to `HexMaster.BattleShip.Core` with `PublishAsync<TEvent>(TEvent, CancellationToken)` where `TEvent : IntegrationEvent`
- [ ] 2.2 Add a reference to `HexMaster.BattleShip.IntegrationEvents` in `HexMaster.BattleShip.Core`
- [ ] 2.3 Implement `DaprEventBus` in the API project (or a new infrastructure project) wrapping `DaprClient.PublishEventAsync`, using `IntegrationEventTopics` to resolve the topic name; log and swallow publish failures without throwing
- [ ] 2.4 Register `IEventBus` → `DaprEventBus` as a singleton in the API's DI composition

## 3. Dapr Pub/Sub AppHost Wiring

- [ ] 3.1 Add the Dapr pub/sub Redis component (`pubsub`) to the Aspire AppHost for local development
- [ ] 3.2 Reference the `pubsub` component from the API project resource in `AppHost.cs`
- [ ] 3.3 Reference the `pubsub` component from the Realtime project resource in `AppHost.cs` (or confirm Realtime runs inside the same API process and shares the component reference)

## 4. Games Command Handlers — Event Publishing

- [ ] 4.1 Add `HexMaster.BattleShip.IntegrationEvents` reference to `HexMaster.BattleShip.Games`
- [ ] 4.2 Inject `IEventBus` into `CreateGameHandler` and publish `GameCreatedIntegrationEvent` after save
- [ ] 4.3 Inject `IEventBus` into `JoinGameByCodeHandler` and publish `PlayerJoinedGameIntegrationEvent` after save
- [ ] 4.4 Inject `IEventBus` into `MarkReadyHandler` and publish `PlayerMarkedReadyIntegrationEvent` after save
- [ ] 4.5 Inject `IEventBus` into `SubmitFleetHandler` and publish `FleetSubmittedIntegrationEvent` after save (payload: GameCode + PlayerId only, no ship data)
- [ ] 4.6 Inject `IEventBus` into `LockFleetHandler` and publish `FleetLockedIntegrationEvent` after save; additionally publish `GameStartedIntegrationEvent` when the game phase has transitioned to `InProgress`
- [ ] 4.7 Inject `IEventBus` into `FireShotHandler` and publish `ShotFiredIntegrationEvent` after save; additionally publish `GameFinishedIntegrationEvent` when the game phase has transitioned to `Finished`
- [ ] 4.8 Inject `IEventBus` into `CancelGameHandler` and publish `GameCancelledIntegrationEvent` after save
- [ ] 4.9 Inject `IEventBus` into `AbandonGameHandler` and publish `GameAbandonedIntegrationEvent` after save

## 5. Scheduled Timer Service

- [ ] 5.1 Define `IScheduledTimerService` interface in `HexMaster.BattleShip.Realtime.Abstractions` with `Schedule` and `Cancel` methods
- [ ] 5.2 Implement `ScheduledTimerService` in `HexMaster.BattleShip.Realtime` using `ConcurrentDictionary<string, CancellationTokenSource>` and `Task.Delay`-based background tasks
- [ ] 5.3 Ensure duplicate `Schedule` calls for the same id are silently ignored and `Cancel` returns `false` for unknown ids

## 6. Realtime — SignalR Hub and Connection Tracking

- [ ] 6.1 Add SignalR and Dapr NuGet packages to `HexMaster.BattleShip.Realtime`
- [ ] 6.2 Define `IGameConnectionTracker` interface in `HexMaster.BattleShip.Realtime.Abstractions` for storing and querying `connectionId → (GameCode, PlayerId)` mappings
- [ ] 6.3 Implement `InMemoryGameConnectionTracker` in `HexMaster.BattleShip.Realtime`
- [ ] 6.4 Implement `GameHub` in `HexMaster.BattleShip.Realtime` with `JoinGame(gameCode, playerId)` client-invokable method, `OnConnectedAsync`, and `OnDisconnectedAsync`
- [ ] 6.5 On `JoinGame`: add connection to game SignalR group, store in connection tracker, cancel any existing grace timer and publish `PlayerConnectionReestablishedIntegrationEvent` if a timer was active
- [ ] 6.6 On `OnDisconnectedAsync` with non-null exception: look up game context, publish `PlayerConnectionLostIntegrationEvent`, start 60-second grace timer, push `OpponentConnectionLost` message to group
- [ ] 6.7 On `OnDisconnectedAsync` with null exception: remove from connection tracker and group; no timer, no event
- [ ] 6.8 Grace timer callback: publish `PlayerConnectionTimedOutIntegrationEvent`

## 7. Realtime — Dapr Subscription Endpoints

- [ ] 7.1 Implement Dapr subscription handler for `battleship.game.player-joined` → push `PlayerJoined` SignalR message to game group
- [ ] 7.2 Implement Dapr subscription handler for `battleship.game.player-marked-ready` → push `PlayerReady`
- [ ] 7.3 Implement Dapr subscription handler for `battleship.game.fleet-submitted` → push `FleetSubmitted`
- [ ] 7.4 Implement Dapr subscription handler for `battleship.game.fleet-locked` → push `FleetLocked`
- [ ] 7.5 Implement Dapr subscription handler for `battleship.game.game-started` → push `GameStarted`
- [ ] 7.6 Implement Dapr subscription handler for `battleship.game.shot-fired` → push `ShotFired`
- [ ] 7.7 Implement Dapr subscription handler for `battleship.game.game-finished` → push `GameFinished`
- [ ] 7.8 Implement Dapr subscription handler for `battleship.game.game-cancelled` → push `GameCancelled`
- [ ] 7.9 Implement Dapr subscription handler for `battleship.game.game-abandoned` → push `GameAbandoned`

## 8. Games — Connection Timeout Subscription

- [ ] 8.1 Implement Dapr subscription handler in the Games module for `battleship.player.connection-timed-out` that invokes `AbandonGameHandler` for the timed-out player

## 9. Realtime DI Module and API Registration

- [ ] 9.1 Implement `AddRealtimeModule` extension method in `HexMaster.BattleShip.Realtime` registering `IScheduledTimerService`, `IGameConnectionTracker`, hub, and all subscription handlers
- [ ] 9.2 Call `AddRealtimeModule` in `Program.cs` and map `GameHub` at `/hubs/game`
- [ ] 9.3 Register the Dapr subscription endpoint for `PlayerConnectionTimedOut` in the API routing

## 10. Validation

- [ ] 10.1 Build the full solution with `dotnet build .\src\Battleship.slnx --nologo` and resolve any errors
- [ ] 10.2 Run `dotnet test .\src\Battleship.slnx --nologo` and confirm baseline tests still pass
- [ ] 10.3 Verify `IntegrationEvents` project has no solution project references in its `.csproj`
- [ ] 10.4 Manually verify (or write a unit test) that `ScheduledTimerService` cancels correctly and does not fire after `Cancel` is called
