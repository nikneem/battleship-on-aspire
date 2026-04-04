## Why

Once both players lock their boards during the `Setup` phase the game is ready to begin, but the system currently has no mechanism to randomly assign the first turn, publish a `GameStarted` integration event, or direct each player's frontend to the correct view (attack or defence). Without this, the transition from setup to active play is incomplete and neither player knows what to do next.

## What Changes

- When the second player locks their board during the `Setup` phase, the backend transitions the game to `InProgress`, **randomly selects** which player takes the first turn, and **publishes a `GameStarted` integration event** carrying the game ID and the ID of the first-turn player.
- The frontend real-time channel consumes the `GameStarted` event and routes each player's in-game view:
  - The player whose turn it is transitions to the **attack view** (targeting the opponent's grid).
  - The opponent transitions to the **defence view** (observing their own fleet, waiting).
- Board positions are locked server-side when the lock command is processed; no further placement changes are accepted after that point.

## Capabilities

### New Capabilities

- `game-play-views`: Frontend capability that drives the attack/defence view split at game start — the player with the first turn sees the attack grid while the opponent sees the defence grid, both updated in real time via the existing SignalR channel.

### Modified Capabilities

- `game-lifecycle`: The existing "Move from setup to active play" scenario does not specify how the first-turn player is determined or that an integration event is published. This change adds the random first-turn selection requirement and the `GameStarted` event publication as normative scenarios under the existing lifecycle requirement.

## Impact

- **Games domain** (`src/Games/`): `LockBoardCommandHandler` (or equivalent) must trigger random turn assignment and publish a `GameStarted` integration event when both boards are locked.
- **Games.Abstractions**: New `GameStartedIntegrationEvent` DTO carrying `GameId` and `FirstTurnPlayerId`.
- **Realtime domain** (`src/Realtime/`): Subscribes to `GameStarted` integration event and pushes a SignalR notification to both players in the game group.
- **Frontend (`src/App`)**: In-game page component adds a handler for the `GameStarted` real-time push; routes to attack or defence sub-view based on the current player's identity vs `FirstTurnPlayerId`.
