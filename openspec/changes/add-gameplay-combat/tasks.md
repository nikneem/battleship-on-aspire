## 1. SignalR Service

- [ ] 1.1 Create `GameSignalRService` as a singleton Angular service in `src/App` with `connect(gameCode, playerId)` and `disconnect()` methods
- [ ] 1.2 Expose typed observables/signals for `ShotFired`, `GameStarted`, `GameFinished`, `GameAbandoned`, `OpponentConnectionLost`, `OpponentReconnected` events
- [ ] 1.3 Call `connect` when the gameplay page activates and `disconnect` in `ngOnDestroy` / route leave guard

## 2. Game State Hydration

- [ ] 2.1 On gameplay page init, call `GetGameStateForPlayer` REST endpoint and populate local signal state (current phase, turn player, own board shots, known enemy shots)
- [ ] 2.2 Handle loading and error states while the REST call is in-flight before rendering the board

## 3. Board Component

- [ ] 3.1 Create a `BoardComponent` with a `mode: signal<'attack' | 'defend'>` input and a `board: signal<BoardState>` input
- [ ] 3.2 Render a 10×10 grid; in attack mode hide own ships; in defend mode show own ships at locked positions
- [ ] 3.3 In attack mode: clicking an untargeted cell sets a local `selectedCell` signal with a semi-transparent red overlay; clicking an already-targeted cell is a no-op
- [ ] 3.4 In defend mode: cells are non-interactive (pointer-events disabled)
- [ ] 3.5 Render shot markers based on outcome: Miss → solid red dot (attack) / solid blue dot (defend); Hit or Sunk → red target icon (attack) / blue target icon (defend)

## 4. Fire Action

- [ ] 4.1 Show "Fire" button in the game-state area only when `selectedCell` is non-null in attack mode
- [ ] 4.2 On "Fire" click: disable button, POST shot to `FireShot` endpoint, clear `selectedCell` on success, re-enable button on error
- [ ] 4.3 Ensure "Fire" button is absent (not just disabled) when no cell is selected

## 5. Turn and Mode State

- [ ] 5.1 Derive `activeMode` as a `computed()`: `'attack'` if `currentTurnPlayerId === myPlayerId`, else `'defend'`
- [ ] 5.2 Update `currentTurnPlayerId` signal when a `ShotFired` SignalR event is received
- [ ] 5.3 Update the board state (add shot marker) when a `ShotFired` SignalR event is received

## 6. Game State Area — Turn and Phase Display

- [ ] 6.1 Show a turn indicator in the game-state area during `InProgress` (e.g., "Your turn" / "Opponent's turn")
- [ ] 6.2 On `GameStarted` SignalR event: transition the page from setup mode to combat mode and show turn indicator
- [ ] 6.3 On `GameFinished` SignalR event: display win/loss result in the game-state area and lock the board
- [ ] 6.4 On `GameAbandoned` SignalR event: navigate the player to the home page

## 7. Validation

- [ ] 7.1 Run `npm run build` from `src/App` and resolve any compilation errors
- [ ] 7.2 Run `npm run test -- --watch=false` from `src/App` and confirm baseline tests still pass
- [ ] 7.3 Manually verify attack/defend mode switch by running two browser sessions through the Aspire dev environment
