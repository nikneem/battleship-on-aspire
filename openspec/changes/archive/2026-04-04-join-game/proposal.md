## Why

Players have no way to join an existing game from the frontend — the backend `POST /api/games/join` endpoint and the full `JoinGameByCodeCommand` handler are already implemented, but the Angular app is missing the service method and the UI to call it. Players can create games but cannot join them.

## What Changes

- Add `joinGame(gameCode, joinSecret?)` method to `GamesApiService`
- Add a "Join Game" page where a player enters a game code and optional join secret
- Route the join page from the app's navigation (e.g. `/games/join`)
- After a successful join, navigate the player into the game lobby

## Capabilities

### New Capabilities

- `join-game-ui`: Player-facing page and service integration to join an existing game by code, with optional join-secret support

### Modified Capabilities

- `game-lobbies`: The lobby view must handle the guest player arriving and the transition from `LobbyOpen` → `LobbyFull`

## Impact

- `src/App/src/app/games-api.service.ts` — new `joinGame()` method
- `src/App/src/app/pages/` — new join-game page component
- `src/App/src/app/app.routes.ts` — new route `/games/join`
- No backend changes required
