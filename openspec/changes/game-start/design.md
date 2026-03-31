## Context

The game transition from `Setup` to `InProgress` is partially implemented. `LockFleetCommandHandler` calls `game.LockFleet(playerId)`; when both players have locked, the `Game` aggregate transitions phase and currently hard-codes `CurrentTurnPlayerId = host.PlayerId` — the host always goes first. This violates the intended random first-turn rule.

The `GameStartedIntegrationEvent` (carrying `GameCode` + `FirstTurnPlayerId`) is already specified in the `add-integration-events-and-realtime` change but not yet published. The Realtime domain is currently a stub. The Angular frontend has a setup-phase shell (`game-route-shell`) but has no attack or defence view; those are planned in `add-gameplay-combat`.

This change wires the four pieces together: random turn selection → event publication → Realtime fan-out → frontend view routing.

## Goals / Non-Goals

**Goals:**
- Replace the hard-coded host-first turn assignment with a server-side random selection inside `Game.LockFleet`.
- Publish `GameStartedIntegrationEvent` (Dapr topic `battleship.game.game-started`) when both fleets are locked and the game transitions to `InProgress`.
- Realtime domain subscribes to that event and pushes a `GameStarted` SignalR notification to both players in the game group.
- Frontend in-game shell reacts to the `GameStarted` SignalR message: the player whose ID matches `FirstTurnPlayerId` routes to the attack view; the other player routes to the defence view.

**Non-Goals:**
- Implementing the full attack/defence view UI (that is the scope of `add-gameplay-combat`).
- Changing any other phase transition logic (lobby → setup, in-progress → finished, etc.).
- Adding a player preference for who goes first.
- Implementing the Realtime domain's full SignalR hub infrastructure (that is the scope of `add-integration-events-and-realtime`).

## Decisions

### Decision: Random selection inside the `Game` domain aggregate

**Chosen**: Inject an `IRandomProvider` (or accept a `bool` flag derived by the handler) into `game.LockFleet` so the aggregate itself does not depend on `System.Random` directly. The handler resolves randomness and passes the result in.

**Rationale**: Domain aggregates should be deterministic and easily unit-tested. Abstracting the random source makes the lock-fleet handler trivially testable.

**Alternatives considered**:
- *`System.Random` directly in the aggregate*: Simple but makes unit tests non-deterministic.
- *Random selection in the handler only*: The aggregate would then expose a separate `SetFirstTurn(playerId)` method, splitting an atomic operation into two calls and risking state inconsistency.

---

### Decision: Publish integration event from the `LockFleetCommandHandler`

**Chosen**: After `game.LockFleet(...)` succeeds and the game is in `InProgress`, the handler publishes `GameStartedIntegrationEvent` via the Dapr client before returning.

**Rationale**: The handler already owns the unit-of-work boundary (fetch → modify → persist). Publishing the event from the same handler keeps the outbox pattern simple and consistent with other handlers in this codebase.

**Alternatives considered**:
- *Domain event raised by aggregate, dispatched by infrastructure*: More idiomatic DDD but requires additional domain-event dispatch infrastructure not yet present in this codebase.
- *Outbox table*: Reliable but adds persistence complexity not justified for the current in-memory repository.

---

### Decision: Frontend uses `PlayerGameStateProjection` for view routing

**Chosen**: The `GameStateResponseDto` already contains a `PlayerGameStateProjection` field. When the frontend receives a `GameStarted` SignalR push it re-fetches (or derives from push payload) the current projection: `YourTurn` → attack view, `OpponentTurn` → defence view.

**Rationale**: The projection enum already encodes "whose turn" cleanly and is already returned by the existing `GetGameStateForPlayer` endpoint. This avoids a bespoke ID-comparison on the client.

**Alternatives considered**:
- *Compare `FirstTurnPlayerId` directly on client*: Requires the frontend to know its own player ID and do a string comparison — functional but redundant with the server-computed projection.

## Risks / Trade-offs

- **`add-integration-events-and-realtime` not yet complete** → `GameStartedIntegrationEvent` type and Dapr infrastructure may not exist when this change is implemented. Mitigation: define a local stub event type in `Games.Abstractions` and integrate with the shared event project once that change lands; tasks note the dependency explicitly.
- **Realtime domain is a stub** → The SignalR fan-out task will block until the hub scaffold exists. Mitigation: task ordering in `tasks.md` reflects this dependency.
- **Attack/defence views not yet built** → The frontend routing task can set a state signal and render a placeholder until `add-gameplay-combat` delivers the actual view components. Tasks note this.
- **Race condition on simultaneous lock** → Both players could call `POST /lock` within milliseconds. The in-memory repository is single-threaded; when a persistent store is introduced a distributed lock or optimistic concurrency must be added.

## Open Questions

- Should the `GameStarted` SignalR push carry the `FirstTurnPlayerId` inline, or should it only signal that the game has started and the client must re-query for state? (Assumed: push carries `FirstTurnPlayerId` directly to avoid an extra round-trip.)
- Once the `add-integration-events-and-realtime` shared event project exists, should `GameStartedIntegrationEvent` move there or stay in `Games.Abstractions`? (Assumed: move to shared project; interim stub stays in `Games.Abstractions`.)
