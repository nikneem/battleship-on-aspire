## Context

The backend join-game path is fully implemented: `POST /api/games/join` maps a `JoinGameByCodeRequestDto` to `JoinGameByCodeCommand`, the handler validates the join secret, calls `game.JoinGuest()`, persists the change, and publishes a `PlayerJoinedGameIntegrationEvent`. SignalR then broadcasts `PlayerJoined` to the game group.

The gap is entirely in the Angular frontend:
- `GamesApiService` has no `joinGame()` method
- There is no page or form for entering a game code and joining
- The existing game-route-shell/lobby view does not show the guest arriving (no `PlayerJoined` SignalR event handler)

## Goals / Non-Goals

**Goals:**
- Add `joinGame(gameCode, joinSecret?)` to `GamesApiService`
- Create a `/games/join` page with a game-code input and optional join-secret field
- Navigate to the game route after a successful join
- Handle the `PlayerJoined` SignalR event in the lobby so the host sees the guest appear

**Non-Goals:**
- Inviting players via link (deep-link join is out of scope)
- Backend changes of any kind
- Handling expired or full lobbies beyond showing an error message

## Decisions

### Route: `/games/join`
A dedicated route keeps the join flow isolated from the game-play shell. The create-game flow already lives at `/games/create`; `/games/join` is the natural sibling.

### Entry point: Landing page
Add a "Join Game" call-to-action alongside the existing "Create Game" button on the landing page. This mirrors the create-game pattern.

### Optional join secret
The form shows the join-secret field at all times (the backend returns `isProtected` only after a lobby lookup, which would add a round-trip). Submitting an empty secret for an unprotected game is safe — the backend ignores it.

### Navigation after join
On success, navigate to `/games/{gameCode}` — the existing `GameRouteShell` handles both host and guest views based on game state.

### SignalR `PlayerJoined` in lobby
The `GameSignalRService` already connects during the game-route-shell lifecycle. Add a `playerJoined$` subject and wire up the `PlayerJoined` event so the host's lobby view refreshes when the guest arrives.

## Risks / Trade-offs

- **Stale lobby state for host**: Without the `PlayerJoined` SignalR handler the host's lobby stays stuck at `LobbyOpen` until they manually refresh. Mitigation: add the event handler as part of this change.
- **Join secret UX**: Showing the secret field for all games may confuse players joining open lobbies. Acceptable trade-off given the cost of the extra round-trip.
