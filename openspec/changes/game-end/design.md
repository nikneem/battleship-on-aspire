## Context

The Battleship game backend already transitions a game to the `Finished` phase and records the winner when the final ship segment is destroyed (as specified in `game-lifecycle`). The Angular 21 frontend (`src/App`) drives the active-game experience but currently has no code path to detect that terminal transition and communicate the outcome to each player.

The frontend communicates game state through a combination of HTTP polling/queries and a real-time SignalR channel (`src/Realtime`). Both players must receive the outcome message as soon as possible after the `Finished` event arrives.

## Goals / Non-Goals

**Goals:**
- Detect the `Finished` phase transition in the Angular frontend.
- Display a personalised outcome overlay ("Winner!" or "You lost") to each player.
- Provide a clear navigation action (Back to Main) that returns the player to the home page.
- Integrate with the existing real-time notification channel so the overlay appears without requiring a manual page refresh.
- Follow Battle Ops tactical visual language (tokens, typography, PrimeNG with Aura preset).

**Non-Goals:**
- Changing any backend API contracts or game-state endpoints.
- Adding game statistics, rematch flows, or leaderboard updates (separate concerns).
- Supporting cancelled or abandoned terminal phases in this change (those have different UX needs).
- Modifying the real-time infrastructure or SignalR hub.

## Decisions

### Decision: Standalone overlay component, not a route

**Chosen**: Implement the outcome as a full-screen overlay component rendered inside the active-game page rather than as a separate route.

**Rationale**: The overlay needs to appear immediately on top of the in-progress board without a URL change or a navigation guard. A route transition would lose in-memory board state and require passing winner information through router state, adding unnecessary complexity.

**Alternatives considered**:
- *Dialog (PrimeNG Dialog)*: PrimeNG `Dialog` could work but adds unnecessary overhead (portal, close button, header/footer conventions) for a full-screen branded moment.
- *Separate route*: Clean URL but requires passing ephemeral outcome data through router state or re-fetching; poor UX if the user refreshes mid-overlay.

---

### Decision: Watch `Finished` phase via existing real-time channel

**Chosen**: Subscribe to the existing real-time game-state update channel in the active-game component. When the received phase equals `Finished`, the overlay signal is set to `true` and the winner identity is stored.

**Rationale**: The Realtime domain already pushes game-state updates to both players. Reusing this channel avoids a secondary polling loop and delivers the overlay as fast as the real-time push.

**Alternatives considered**:
- *Periodic HTTP polling*: Higher latency, wastes bandwidth, and already superseded by the real-time channel.
- *New SignalR event type*: Would require backend changes and is unnecessary when the full game state (including phase) is already broadcast.

---

### Decision: Use Angular signals for overlay visibility

**Chosen**: Model overlay visibility and winner identity as `signal<boolean>` and `signal<string | null>` respectively inside the active-game component (or a thin game-outcome service if reuse is needed).

**Rationale**: The app already mandates `ChangeDetectionStrategy.OnPush` and prefers signals/computed/effect for local UI state (per project conventions). Signal-driven state avoids manual `markForCheck()` calls.

## Risks / Trade-offs

- **Late real-time delivery** → If the SignalR push is delayed or dropped, the overlay may not appear. Mitigation: the active-game component can fall back to an HTTP fetch on reconnect to re-check phase.
- **Both players in same browser tab (testing only)** → Not a production concern; no special handling needed.
- **Back-to-main navigation while game is transitioning** → If the player clicks "Back to Main" before the overlay, they will still land on the main page. This is acceptable behaviour.

## Open Questions

- Should the overlay auto-dismiss after a timeout, or only on explicit button press? (Assumed: explicit press only, matching the issue description.)
- Does the existing real-time channel broadcast the winner's player ID directly, or must the frontend derive it by comparing the winner field against the current player's profile? (Assumed: winner player ID is included in the game-state DTO already sent by the backend.)
