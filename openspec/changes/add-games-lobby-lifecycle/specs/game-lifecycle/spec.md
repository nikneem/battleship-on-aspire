## ADDED Requirements

### Requirement: Game phases follow the Battleship lifecycle
The system SHALL model each game as progressing through the phases `LobbyOpen`, `LobbyFull`, `Setup`, `InProgress`, `Finished`, `Cancelled`, or `Abandoned`, with transitions allowed only through valid game actions.

#### Scenario: Move from lobby to setup
- **WHEN** both joined players have marked themselves ready in a game in the `LobbyFull` phase
- **THEN** the game transitions to the `Setup` phase

#### Scenario: Move from setup to active play
- **WHEN** both players have submitted valid fleets and locked their boards in a game in the `Setup` phase
- **THEN** the game transitions to the `InProgress` phase

#### Scenario: Prevent invalid phase regression
- **WHEN** a client attempts an action that would move the game to a prior incompatible phase
- **THEN** the system rejects the action
- **AND** the game remains in its current phase

### Requirement: Players can prepare boards only during setup
The system SHALL allow joined players to mark ready, submit fleets, and lock boards only during the appropriate pre-game phases, and it MUST validate fleet legality before active play begins.

#### Scenario: Mark players ready before setup
- **WHEN** a joined player marks themselves ready in a game in the `LobbyFull` phase
- **THEN** the system records that player's readiness
- **AND** the game remains in `LobbyFull` until both players are ready

#### Scenario: Reject invalid fleet placement
- **WHEN** a player submits a fleet with out-of-bounds, overlapping, or otherwise invalid ship placements during the `Setup` phase
- **THEN** the system rejects the submitted fleet
- **AND** the player's board remains unlocked

#### Scenario: Prevent board changes after locking
- **WHEN** a player attempts to modify ship placement after their board has been locked
- **THEN** the system rejects the change
- **AND** the locked board remains unchanged

### Requirement: Active play enforces turn order and shot legality
The system MUST allow firing only during the `InProgress` phase, only from the player whose turn it is, and only against target coordinates that have not already been fired upon for that opponent board.

#### Scenario: Current player fires a valid shot
- **WHEN** the current-turn player fires at a previously untried coordinate in a game in the `InProgress` phase
- **THEN** the system records the shot result against the opponent board
- **AND** the system updates turn state according to the game rules

#### Scenario: Reject out-of-turn shots
- **WHEN** a player who does not own the current turn attempts to fire a shot
- **THEN** the system rejects the action
- **AND** the game state remains unchanged

#### Scenario: Reject duplicate target coordinates
- **WHEN** the current-turn player attempts to fire at a coordinate that has already been targeted against the opponent board
- **THEN** the system rejects the action
- **AND** no additional shot is recorded

### Requirement: Players receive caller-safe match views
The system SHALL provide match state views that allow each joined player to see the current phase, turn state, their own board, known shot outcomes, and terminal results without exposing undiscovered ship positions on the opponent board.

#### Scenario: Read in-progress state as a player
- **WHEN** a joined player requests game state for a game in the `InProgress` phase
- **THEN** the response includes that player's own board state, known shot outcomes, current phase, and whose turn it is
- **AND** the response excludes undiscovered opponent ship coordinates

#### Scenario: Read terminal state after the game ends
- **WHEN** a joined player requests game state for a game in the `Finished`, `Cancelled`, or `Abandoned` phase
- **THEN** the response includes the terminal phase and outcome information relevant to that player

### Requirement: Games end through completion or termination actions
The system MUST allow games to reach a terminal state when one player sinks the opponent fleet, when the host cancels a joinable lobby, or when a joined player abandons the game.

#### Scenario: Finish the game by sinking the final ship
- **WHEN** a valid shot causes the defending player's final remaining ship segment to be destroyed
- **THEN** the system transitions the game to the `Finished` phase
- **AND** the system records the winning player

#### Scenario: Cancel a joinable lobby
- **WHEN** the host cancels a game in the `LobbyOpen` or `LobbyFull` phase
- **THEN** the system transitions the game to the `Cancelled` phase
- **AND** no further join or gameplay actions are accepted

#### Scenario: Abandon an active or joined game
- **WHEN** a joined player abandons a game in the `LobbyFull`, `Setup`, or `InProgress` phase
- **THEN** the system transitions the game to the `Abandoned` phase
- **AND** no further setup or gameplay actions are accepted
