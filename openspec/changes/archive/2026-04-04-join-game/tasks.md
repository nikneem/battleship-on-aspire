## 1. Angular Service

- [x] 1.1 Add `joinGame(gameCode: string, joinSecret?: string): Observable<GameLobbyResponse>` method to `GamesApiService`
- [x] 1.2 Add `GameLobbyResponse` interface to `GamesApiService` (matching `GameLobbyResponseDto` from backend)

## 2. SignalR - PlayerJoined Event

- [x] 2.1 Add `playerJoined$` subject to `GameSignalRService`
- [x] 2.2 Register `PlayerJoined` SignalR event handler in `GameSignalRService.connect()`

## 3. Join Game Page

- [x] 3.1 Generate join-game page component under `src/App/src/app/pages/public/games/join-game-page/`
- [x] 3.2 Add game-code input field and optional join-secret field to the join-game page form
- [x] 3.3 Disable the submit button when the game-code field is empty
- [x] 3.4 Call `gamesApiService.joinGame()` on submit and navigate to `/games/{gameCode}` on success
- [x] 3.5 Display an error message when the join request fails

## 4. Routing

- [x] 4.1 Add `/games/join` route to `app.routes.ts` pointing to the join-game page component

## 5. Landing Page Entry Point

- [x] 5.1 Add a "Join Game" call-to-action button to the landing page alongside the existing "Create Game" button
- [x] 5.2 Wire the button to navigate to `/games/join`

## 6. Lobby - Real-time Guest Arrival

- [x] 6.1 Subscribe to `gameSignalRService.playerJoined$` in the game-route-shell lobby view
- [x] 6.2 On `PlayerJoined` event, re-fetch lobby state so the host sees the guest participant and `LobbyFull` phase
