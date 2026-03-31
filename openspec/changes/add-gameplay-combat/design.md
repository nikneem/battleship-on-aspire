## Context

The Angular gameplay page at `/games/{game-code}` currently handles only the **Setup** phase — ship placement, drag-and-drop positioning, and the Ready action. Once both players lock their fleets and the game transitions to `InProgress`, the page has no combat UI.

The backend provides `FireShotHandler`, a full `Game` domain model with turn enforcement and shot outcome tracking, and (via `add-integration-events-and-realtime`) a SignalR hub that pushes `ShotFired`, `GameStarted`, `GameFinished`, and `GameAbandoned` events to connected clients. The frontend must consume these signals and render the appropriate board state.

## Goals / Non-Goals

**Goals:**
- Render an attack-mode board when it is the current player's turn
- Render a defend-mode board when it is the opponent's turn
- Display correct shot marker icons (miss / hit / sunk) in both modes
- Connect to the SignalR hub on page load and update board state reactively
- Display game-finished and game-abandoned states clearly

**Non-Goals:**
- Implementing the SignalR hub or backend event publishing (covered by `add-integration-events-and-realtime`)
- Connection-loss grace period UI — countdown and reconnect messaging (separate concern)
- Sounds or animations beyond static visual markers

## Decisions

### D1 — Single board component with an `[mode]` input, not two separate components

Both modes share the same 10×10 grid structure; only what is overlaid on the cells differs. A single `BoardComponent` with a `mode: 'attack' | 'defend'` input avoids duplicating grid layout logic and keeps the template conditional with `@if`.

**Alternative considered:** Separate `AttackBoardComponent` and `DefendBoardComponent`. Rejected because the grid rendering is identical and maintaining two copies introduces drift risk.

### D2 — Signals and computed for turn/mode state, not BehaviorSubject

The gameplay page already uses OnPush change detection. Turn state (`currentTurnPlayerId`) arrives via SignalR and is stored in a `signal<string | null>`. The active mode (`'attack' | 'defend'`) is a `computed()` derived from turn state vs current player id. This keeps the template clean and avoids manual subscription management.

### D3 — Pre-selection marker is purely local UI state, not sent to server

When a player clicks a cell in attack mode, the cell is highlighted semi-transparent red locally. Only when the player clicks "Fire" is the shot POSTed to the backend. This avoids partial-state writes to the server and keeps the single source of truth clean.

**Alternative considered:** Optimistic update — mark immediately and reconcile on server response. Rejected because latency is low and a failed shot should simply clear the local selection without a rollback.

### D4 — SignalR service is a singleton Angular service, not inline hub logic in the component

A `GameSignalRService` manages the hub connection lifecycle (connect on page enter, disconnect on page leave). The gameplay page injects it and subscribes to typed event observables. This keeps the component free of hub bootstrapping concerns and makes the service independently testable.

### D5 — Board cell click in defend mode is a no-op

In defend mode, the player is waiting — clicks on the board do nothing. The template disables pointer events on cells in defend mode rather than conditionally rendering them, to preserve consistent grid layout.

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| SignalR hub not yet implemented when this UI is built | Use a mock SignalR service behind `IGameSignalRService` interface for local development; swap in real implementation once `add-integration-events-and-realtime` is delivered |
| Race condition: page loads after `GameStarted` event was already pushed | On connect, fetch full game state via REST (`GetGameStateForPlayer`) to hydrate local state; SignalR updates are incremental from that point |
| Shot marker icon assets may not exist yet | Define icon class names from the Battle Ops style system; fall back to CSS-only circles/targets if PrimeNG icons are unavailable |

## Open Questions

- Should the "Fire" button be disabled while a shot request is in-flight, or is optimistic dismissal preferred? → Recommend: disable button while pending, re-enable on error.
- When a game finishes, should the board lock in place and show a result overlay, or navigate the player away? → Issue #3 implies the board stays; a result area in the game-state panel is sufficient.
