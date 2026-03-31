## ADDED Requirements

### Requirement: Frontend routes each player to the correct view when the game starts
The system SHALL route each connected player to either the attack view or the defence view as soon as a `GameStarted` real-time notification is received, based on that player's turn state.

#### Scenario: First-turn player enters attack view
- **WHEN** the frontend receives a `GameStarted` SignalR notification
- **AND** the `FirstTurnPlayerId` in the notification matches the current player's identity
- **THEN** the in-game shell transitions the player's view to the attack view

#### Scenario: Waiting player enters defence view
- **WHEN** the frontend receives a `GameStarted` SignalR notification
- **AND** the `FirstTurnPlayerId` in the notification does not match the current player's identity
- **THEN** the in-game shell transitions the player's view to the defence view

#### Scenario: View transition requires no manual action from the player
- **WHEN** the `GameStarted` notification is received
- **THEN** the view switch happens automatically without the player clicking any button

### Requirement: Realtime domain fans out GameStarted to both players in the game group
The system SHALL push a `GameStarted` SignalR message — containing the `GameCode` and `FirstTurnPlayerId` — to every client connected to the affected game's SignalR group when it receives a `GameStartedIntegrationEvent` from the Dapr topic `battleship.game.game-started`.

#### Scenario: Both players receive GameStarted push
- **WHEN** the Realtime service receives a `GameStartedIntegrationEvent` for game code `G`
- **THEN** a `GameStarted` SignalR message is sent to all clients in the group for game `G`
- **AND** the message carries the `GameCode` and `FirstTurnPlayerId` fields from the event

#### Scenario: No push sent for unrecognised game codes
- **WHEN** the Realtime service receives a `GameStartedIntegrationEvent` for a game code with no connected clients
- **THEN** no SignalR message is sent
- **AND** no error is raised

### Requirement: GameStartedIntegrationEvent is published when the game transitions to InProgress
The Games domain MUST publish a `GameStartedIntegrationEvent` to the Dapr pub/sub topic `battleship.game.game-started` whenever the game transitions from `Setup` to `InProgress`.

#### Scenario: Event published after second player locks fleet
- **WHEN** the second player calls the lock-fleet endpoint for a game in the `Setup` phase
- **AND** both players' fleets are now locked
- **THEN** the system publishes a `GameStartedIntegrationEvent` with the game's code and the randomly selected `FirstTurnPlayerId`

#### Scenario: Event not published on first lock
- **WHEN** the first player calls the lock-fleet endpoint for a game in the `Setup` phase
- **AND** the other player's fleet is not yet locked
- **THEN** no `GameStartedIntegrationEvent` is published
- **AND** the game remains in the `Setup` phase
