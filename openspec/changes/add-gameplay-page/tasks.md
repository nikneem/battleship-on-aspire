## 1. Gameplay route shell

- [ ] 1.1 Add the `/games/{game-code}` route and a gameplay page shell that shows the current player, opponent, and game-state header areas.
- [ ] 1.2 Render the setup board and ship inventory area for a player who is preparing their fleet.

## 2. Fleet placement interactions

- [ ] 2.1 Implement local board state for ship inventory, placed ships, orientation, and selected ship context.
- [ ] 2.2 Implement dragging and snapping behavior so ships land on valid grid cells and remain within board bounds.
- [ ] 2.3 Allow already placed ships to be repositioned and rotated through contextual controls.

## 3. Readiness and lock state

- [ ] 3.1 Reveal the `Ready` action only after the full fleet has been placed legally on the board.
- [ ] 3.2 Lock ship movement and rotation after the player confirms readiness.

## 4. Verification

- [ ] 4.1 Add frontend tests for route rendering, setup-board presentation, fleet-placement state transitions, and ready-state locking.
- [ ] 4.2 Run the relevant app and repository validation commands and fix any issues required for the gameplay page change to pass cleanly.
