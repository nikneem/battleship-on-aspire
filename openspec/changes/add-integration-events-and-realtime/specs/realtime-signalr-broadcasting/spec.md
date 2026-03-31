## ADDED Requirements

### Requirement: SignalR hub for game connections
The Realtime module SHALL expose a `GameHub` (SignalR hub) at the route `/hubs/game`. Clients SHALL connect to this hub to receive real-time game state updates. Upon connection, the client SHALL send a `JoinGame` invocation with `gameCode` and `playerId`, which registers the connection in the active connection map and adds the connection to the SignalR group for that game.

#### Scenario: Client joins game group on connect
- **WHEN** a client connects to `/hubs/game` and invokes `JoinGame(gameCode, playerId)`
- **THEN** the connection SHALL be added to a SignalR group named after the `gameCode` and the mapping `connectionId → (gameCode, playerId)` SHALL be stored

#### Scenario: Disconnected client is removed from group
- **WHEN** `OnDisconnectedAsync` fires for a connection
- **THEN** the connection SHALL be removed from the SignalR group and the connection map entry SHALL be cleared

### Requirement: Dapr subscription endpoints for Games integration events
The Realtime service SHALL expose Dapr subscription endpoints (HTTP POST routes decorated with `[Topic]`) for each Games integration event. Each handler SHALL push a corresponding SignalR message to the game's group.

The following subscriptions and their SignalR method names SHALL be implemented:

| Dapr Topic | SignalR Method Pushed |
|---|---|
| `battleship.game.player-joined` | `PlayerJoined` |
| `battleship.game.player-marked-ready` | `PlayerReady` |
| `battleship.game.fleet-submitted` | `FleetSubmitted` |
| `battleship.game.fleet-locked` | `FleetLocked` |
| `battleship.game.game-started` | `GameStarted` |
| `battleship.game.shot-fired` | `ShotFired` |
| `battleship.game.game-finished` | `GameFinished` |
| `battleship.game.game-cancelled` | `GameCancelled` |
| `battleship.game.game-abandoned` | `GameAbandoned` |

#### Scenario: ShotFired event is broadcast to both players in the game
- **WHEN** a `ShotFiredIntegrationEvent` arrives via Dapr subscription
- **THEN** the Realtime service SHALL push a `ShotFired` SignalR message to ALL connections in the game's SignalR group

#### Scenario: GameAbandoned event triggers return-home signal
- **WHEN** a `GameAbandonedIntegrationEvent` arrives via Dapr subscription
- **THEN** the Realtime service SHALL push a `GameAbandoned` SignalR message to all connections in the group

### Requirement: Realtime DI module registration
The Realtime implementation project SHALL expose a `AddRealtimeModule` extension method on `IServiceCollection` that registers the hub, connection tracker, timer service, and all Dapr subscription handlers. The API's `Program.cs` SHALL call this extension and map the `GameHub` endpoint.

#### Scenario: Module registers without error
- **WHEN** `AddRealtimeModule` is called during application startup
- **THEN** the DI container SHALL resolve `IScheduledTimerService`, `IGameConnectionTracker`, and all subscription handlers without errors
