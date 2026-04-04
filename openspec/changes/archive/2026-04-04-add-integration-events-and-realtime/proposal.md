## Why

The backend currently has no way to notify players in real time when game state changes — every mutation happens silently and clients have no push channel. Without a shared integration event contract and a Dapr pub/sub backbone, the Realtime module cannot know what to broadcast, and the Games module cannot signal what just happened.

## What Changes

- Introduce a new cross-cutting `HexMaster.BattleShip.IntegrationEvents` project containing all integration event record types, a versioned base record, and Dapr topic name constants.
- Every Games command handler publishes an integration event to Dapr pub/sub after a successful state mutation.
- The Realtime module implements a SignalR hub and subscribes to Games integration events, pushing state updates to connected clients.
- The Realtime module detects SignalR connection loss and manages a 60-second grace period per player via an in-memory timer service; if a player does not reconnect in time, a timeout event is published, which causes the game to be abandoned.
- The Aspire AppHost is wired with a Dapr pub/sub component so all services can publish and subscribe.

## Capabilities

### New Capabilities

- `integration-events`: Cross-cutting event contracts library — versioned base record, all integration event types published by Games and Realtime, and Dapr topic name constants.
- `game-action-events`: Games command handlers publish integration events via Dapr pub/sub after every successful state mutation (game created, player joined, ready, fleet locked, game started, shot fired, game finished, cancelled, abandoned).
- `realtime-signalr-broadcasting`: Realtime module hosts a SignalR hub and subscribes to Games integration events via Dapr, pushing corresponding messages to connected clients over WebSocket.
- `player-connection-grace`: Realtime detects SignalR connection loss, starts a 60-second in-memory grace timer per player, notifies the opponent with a countdown, and publishes a timeout event if the player does not reconnect — triggering game abandonment.

### Modified Capabilities

## Impact

- New project: `src/IntegrationEvents/HexMaster.BattleShip.IntegrationEvents`
- All Games command handlers gain a dependency on `IEventBus` (Dapr pub/sub abstraction)
- `HexMaster.BattleShip.Realtime` is fully implemented (currently a stub)
- `HexMaster.BattleShip.Realtime.Abstractions` is populated with SignalR hub contracts and connection-tracking interfaces
- `HexMaster.BattleShip.Aspire.AppHost` gains a Dapr pub/sub component and wires it to the API and Realtime services
- Angular frontend gains SignalR connectivity (out of scope for this change — frontend will be addressed separately)
