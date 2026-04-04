## ADDED Requirements

### Requirement: In-memory scheduled timer service
The system SHALL provide an `IScheduledTimerService` interface in `HexMaster.BattleShip.Realtime` with the following contract:
- `void Schedule(string id, TimeSpan delay, Func<CancellationToken, Task> callback)` — registers a timer that fires `callback` after `delay` unless cancelled
- `bool Cancel(string id)` — cancels and removes a pending timer; returns `true` if a timer was found and cancelled

The implementation (`ScheduledTimerService`) SHALL store registrations in a `ConcurrentDictionary<string, CancellationTokenSource>` keyed by the timer `id`. Each registered timer SHALL run as a background `Task.Delay` task. On expiry, the id SHALL be removed from the dictionary before the callback is invoked.

#### Scenario: Timer fires callback after delay
- **WHEN** a timer is scheduled with a 60-second delay and not cancelled
- **THEN** the callback SHALL be invoked after approximately 60 seconds

#### Scenario: Cancelled timer does not fire
- **WHEN** `Cancel` is called for a pending timer before its delay expires
- **THEN** the callback SHALL NOT be invoked and the timer id SHALL be removed from the registry

#### Scenario: Duplicate registration is ignored
- **WHEN** `Schedule` is called with an id that is already registered
- **THEN** the existing timer SHALL NOT be replaced and no error SHALL be thrown

#### Scenario: Registry is clean after timer fires
- **WHEN** a timer fires its callback
- **THEN** the timer id SHALL no longer appear in the active registry

### Requirement: Grace period started on abnormal SignalR disconnect
The `GameHub` SHALL detect abnormal disconnections (where the `Exception` parameter of `OnDisconnectedAsync` is non-null) and start a 60-second grace timer using `IScheduledTimerService`. The timer id SHALL follow the format `grace:{gameCode}:{playerId}`.

On starting the grace timer the hub SHALL:
1. Publish `PlayerConnectionLostIntegrationEvent` via `IEventBus`
2. Push a `OpponentConnectionLost` SignalR message to the opponent's connection(s) in the game group, carrying the `playerId` of the disconnected player and the grace period duration (60 seconds)

#### Scenario: Grace timer started on abnormal disconnect
- **WHEN** `OnDisconnectedAsync` fires with a non-null exception for a player in an active game
- **THEN** a 60-second timer SHALL be registered and `PlayerConnectionLostIntegrationEvent` SHALL be published

#### Scenario: Clean disconnect does not start grace timer
- **WHEN** `OnDisconnectedAsync` fires with a null exception (intentional leave)
- **THEN** no grace timer SHALL be started

### Requirement: Grace period cancelled on player reconnection
When a player reconnects and invokes `JoinGame` on the `GameHub`, the hub SHALL check whether a grace timer exists for `grace:{gameCode}:{playerId}` and cancel it if present. The hub SHALL then publish `PlayerConnectionReestablishedIntegrationEvent` and push an `OpponentReconnected` SignalR message to the other players in the game group.

#### Scenario: Reconnection within grace period cancels timer
- **WHEN** a player invokes `JoinGame` within 60 seconds of being disconnected
- **THEN** the grace timer SHALL be cancelled, `PlayerConnectionReestablishedIntegrationEvent` SHALL be published, and the opponent SHALL receive `OpponentReconnected`

### Requirement: Grace period expiry triggers game abandonment
When the grace timer fires (player did not reconnect within 60 seconds), the callback SHALL:
1. Publish `PlayerConnectionTimedOutIntegrationEvent` via `IEventBus`

The Games module SHALL subscribe to `PlayerConnectionTimedOutIntegrationEvent` via a Dapr subscription endpoint and execute the `AbandonGameHandler` for the timed-out player. This causes `GameAbandonedIntegrationEvent` to be published, which the Realtime module broadcasts to remaining connected players as a `GameAbandoned` SignalR message, prompting the client to navigate to the home page.

#### Scenario: Timeout event causes game abandonment
- **WHEN** the 60-second grace timer fires for a player
- **THEN** `PlayerConnectionTimedOutIntegrationEvent` SHALL be published and the Games module SHALL process an abandon for that player

#### Scenario: Opponent receives game-abandoned after timeout
- **WHEN** the game is abandoned due to connection timeout
- **THEN** the remaining connected player SHALL receive a `GameAbandoned` SignalR message
