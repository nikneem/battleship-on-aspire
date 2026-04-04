## Why

When all of one player's ship segments are destroyed the game transitions to `Finished`, but the Angular frontend currently has no mechanism to surface that outcome to either player. Players are left in an ambiguous state with no clear signal that the game is over or who won.

## What Changes

- Add a game-end outcome overlay to the active-game UI that is shown as soon as the game reaches the `Finished` phase.
- The overlay displays **"Winner!"** to the winning player and **"You lost"** to the losing player.
- Both overlays include a **Back to Main** button that navigates the player to the application's main/home page.
- The frontend polls or receives a real-time signal indicating the `Finished` phase and winner, then triggers the overlay.

## Capabilities

### New Capabilities

- `game-end-overlay`: Frontend overlay component shown to both players when a Battleship game reaches the `Finished` phase, displaying a personalised outcome message ("Winner!" or "You lost") and a navigation button back to the main page.

### Modified Capabilities

- `game-lifecycle`: The game-lifecycle spec already defines that the backend records the winning player on the `Finished` transition. The frontend-visible requirement that the game state response **includes the winner identity** in the terminal-state view is already covered by the existing scenario "Read terminal state after the game ends". No new requirement changes needed.

## Impact

- **Frontend (`src/App`)**: New standalone Angular overlay component, integration into the in-game page to watch for `Finished` phase, and routing back to main on dismissal.
- **API / Games domain**: No new endpoints required; existing game-state endpoint must return winner information in the `Finished` phase response (already specified in `game-lifecycle`).
- **Realtime domain**: The overlay should be triggered as soon as the phase change is received via the existing real-time channel; no new events required.
