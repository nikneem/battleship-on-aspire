## Why

GitHub issue `#2` defines the first player-facing gameplay screen, but the repository does not yet describe the `/games/{game-code}` route or the board-setup interactions needed before a player can mark themselves ready.

We need a dedicated change for this UI flow so the route layout, ship-placement behavior, readiness gating, and host/player presentation can be implemented against a clear product contract.

## What Changes

- Add a gameplay page at `/games/{game-code}` that shows the current player, opponent, game state, and a Battleship play field.
- Add fleet-placement interactions for dragging ships onto the grid, snapping ships into valid cells, and rotating placed ships through a contextual control.
- Add readiness behavior so the `Ready` action appears only after all ships are positioned and locks the player's fleet once confirmed.
- Define the client-side interaction contract for setup-state presentation and player status visibility on the gameplay page.

## Capabilities

### New Capabilities
- `gameplay-page`: Defines the `/games/{game-code}` page layout, setup-board interactions, readiness gating, and locked-board behavior for a player preparing to start a match.

### Modified Capabilities

None.

## Impact

- Adds a new OpenSpec capability for the Angular app's gameplay route and setup interactions.
- Drives new frontend route, state, and interaction work in `src\App`.
- Will need to align with future or parallel game-state APIs exposed by the `Games` module.
