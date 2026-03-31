<!-- openspec:github-issue nikneem/battleship-on-aspire#3 -->
> Synced from GitHub issue #3 by manual import.

## Why

Players can set up their fleet and enter the game, but once both fleets are locked there is nothing to play — the combat phase has no frontend implementation. The gameplay page currently only supports the setup phase; there is no attack mode, no defend mode, no visual shot feedback, and no real-time state push to both players.

## What Changes

- Add an **attack mode** view to the gameplay page that activates when it is the current player's turn: the board shows only the grid (no own ships), clicked cells show a semi-transparent red selection marker, a "Fire" button appears, and after firing the cell is updated with the shot result.
- Add a **defend mode** view to the gameplay page that activates when it is the opponent's turn: the board shows the player's own positioned ships overlaid with shot markers indicating where the opponent has fired.
- Implement visual shot markers: miss → solid red dot, hit → red target icon (attack mode); opponent miss → solid blue dot, opponent hit → blue target icon (defend mode).
- Wire the gameplay page to the SignalR hub so both players receive real-time game state updates after every shot.
- Display the current turn state and any relevant phase transitions (game started, game finished, winner) in the game-state area.

## Capabilities

### New Capabilities

- `gameplay-combat`: The full combat phase UI — attack mode, defend mode, shot marker visuals, turn state display, and real-time SignalR-driven board updates.

### Modified Capabilities

## Impact

- `src/App` — Angular gameplay page gains attack/defend mode components and shot marker rendering
- `src/App` — Angular SignalR service connects to `/hubs/game` and subscribes to combat events
- Depends on `add-integration-events-and-realtime` for the SignalR hub and `ShotFired`, `GameStarted`, `GameFinished` push events
