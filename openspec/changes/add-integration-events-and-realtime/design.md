## Context

The backend is a modular .NET solution orchestrated by Aspire. Games domain has a rich domain model with command handlers (fetch → modify → save → return DTO) but no event publishing. Realtime is currently stub projects. Dapr is already wired in the AppHost for state store access (`statestore` component on the API).

Clients currently have no push channel — they must poll for state changes. The Angular frontend is out of scope for this change; the backend event infrastructure is the focus.

## Goals / Non-Goals

**Goals:**
- Define a versioned, dependency-free integration event contracts library
- Wire Dapr pub/sub in the AppHost alongside the existing state store
- Have every Games command handler publish an integration event after a successful mutation
- Implement the Realtime SignalR hub and Dapr subscription handlers
- Detect SignalR connection loss and manage the 60-second grace period via an in-memory timer service
- Trigger game abandonment when a grace period expires

**Non-Goals:**
- Angular / frontend SignalR client integration (separate change)
- Dapr Actor-based durable timers (accepted trade-off: in-memory is sufficient)
- Event sourcing or event store persistence
- Dead-letter handling or retry policies on Dapr pub/sub

## Decisions

### D1 — Single cross-cutting `IntegrationEvents` project, not per-domain namespaces

Both `Games` (publisher) and `Realtime` (subscriber) must reference event contracts without referencing each other. A standalone project with zero domain dependencies is the only option that avoids circular references. It lives at `src/IntegrationEvents/HexMaster.BattleShip.IntegrationEvents`.

**Alternative considered:** Event records in each domain's `Abstractions` project. Rejected because Realtime would then depend on `Games.Abstractions`, coupling domains.

### D2 — Versioned base record, not attributes or external libraries

Every integration event inherits from `IntegrationEvent` (abstract record), which carries `EventId` (new `Guid`), `OccurredOn` (`DateTimeOffset.UtcNow`), and abstract `SchemaVersion`. Each concrete event hard-codes its version string (e.g., `"1.0"`).

No external versioning library is introduced. Version is a plain string — consumers can route or reject on it without framework coupling.

**Alternative considered:** Version embedded as a custom attribute. Rejected because attributes are not part of the instance payload and cannot be read without reflection.

### D3 — One Dapr pub/sub topic per integration event type

Topic names follow `battleship.{domain}.{verb-past-tense}`, e.g., `battleship.game.shot-fired`, `battleship.player.connection-lost`. Each event type maps to exactly one topic constant on the event record.

**Alternative considered:** Single `battleship.events` topic with a discriminator field. Rejected because Dapr subscription routing at topic level is simpler and avoids consumers receiving events they never need.

### D4 — Games handlers publish via `IEventBus` abstraction, not Dapr SDK directly

A thin `IEventBus` interface (in `Core` or `IntegrationEvents`) wraps Dapr's `DaprClient.PublishEventAsync`. This keeps handler code testable and isolates the Dapr SDK reference to a single infrastructure adapter.

```
IEventBus ← DaprEventBus (in API or Realtime infra)
```

**Alternative considered:** Inject `DaprClient` directly into handlers. Rejected because it couples domain handlers to the Dapr SDK and makes unit testing awkward.

### D5 — In-memory `IScheduledTimerService` for grace period timers

A singleton `ScheduledTimerService` maintains a `ConcurrentDictionary<string, CancellationTokenSource>` keyed by `"grace:{gameCode}:{playerId}"`. Each registration fires a `Task.Delay`-based background task that publishes a `PlayerConnectionTimedOutIntegrationEvent` if not cancelled within 60 seconds.

**Alternative considered:** Dapr Actor reminders. More durable (survives restart), but adds Dapr Actors infrastructure and scope. Accepted trade-off: a service restart during a 60s window is an edge case; affected games would be left in an unresolved state, which is tolerable for now.

### D6 — Separate `GameStartedIntegrationEvent` and `GameFinishedIntegrationEvent`

Rather than embedding a phase-change flag in action events (`ShotFiredIntegrationEvent.IsGameOver`), dedicated events are published when derived state transitions occur (both fleets locked → `GameStartedIntegrationEvent`; all ships sunk → `GameFinishedIntegrationEvent`). This keeps each event semantically clean and allows subscribers to react to lifecycle milestones independently.

### D7 — Connection-tracking map in Realtime hub

The SignalR hub maintains a `ConcurrentDictionary<string, (string GameCode, string PlayerId)>` keyed by `ConnectionId`. On connect, players register their game context. On disconnect, the hub looks up the context to know which timer to start and which event to publish.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| Grace timer lost on Realtime service restart | Accepted for now; Dapr Actor reminders can be introduced in a follow-up |
| `FleetSubmittedIntegrationEvent` could leak ship positions to opponent | Event payload MUST NOT include ship coordinates — only a boolean or count |
| Dapr pub/sub message delivery is at-least-once | Subscribers must be idempotent; SignalR broadcast duplication is harmless for UI updates |
| `IEventBus` adds a publish call after every save — failure to publish is silent | Consider logging publish failures; do not fail the HTTP request on publish failure |

## Migration Plan

1. Add `HexMaster.BattleShip.IntegrationEvents` project and add to solution
2. Add Dapr pub/sub component to AppHost (Redis-backed in local dev)
3. Implement `DaprEventBus` and register in DI
4. Update all Games command handlers to inject and use `IEventBus`
5. Implement Realtime SignalR hub, connection tracking, and Dapr subscription endpoints
6. Implement `ScheduledTimerService` and wire connection-loss grace period
7. Wire Realtime module DI and register with AppHost

No rollback strategy is required — no schema migrations, no breaking API changes.

## Open Questions

- Should `FleetLockedIntegrationEvent` carry a flag indicating it was the *second* lock (i.e., game is now starting), or should Realtime derive this from receiving `GameStartedIntegrationEvent`? → Prefer separate `GameStartedIntegrationEvent` per D6.
- Should the opponent receive the `FleetSubmitted` event at all (to show "opponent has submitted fleet")? The payload would carry no coordinates, only the player ID. → Include, safe as long as no coordinates are in the payload.
