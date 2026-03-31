## MODIFIED Requirements

### Requirement: Game phases follow the Battleship lifecycle
The system SHALL model each game as progressing through the phases `LobbyOpen`, `LobbyFull`, `Setup`, `InProgress`, `Finished`, `Cancelled`, or `Abandoned`, with transitions allowed only through valid game actions.

#### Scenario: Move from lobby to setup
- **WHEN** both joined players have marked themselves ready in a game in the `LobbyFull` phase
- **THEN** the game transitions to the `Setup` phase

#### Scenario: Move from setup to active play with random first turn
- **WHEN** both players have submitted valid fleets and locked their boards in a game in the `Setup` phase
- **THEN** the game transitions to the `InProgress` phase
- **AND** the system randomly selects one of the two players as the first-turn player
- **AND** the system records the selected player's ID as `CurrentTurnPlayerId`
- **AND** the system publishes a `GameStartedIntegrationEvent` carrying the game code and the selected player's ID

#### Scenario: Prevent invalid phase regression
- **WHEN** a client attempts an action that would move the game to a prior incompatible phase
- **THEN** the system rejects the action
- **AND** the game remains in its current phase

## ADDED Requirements

### Requirement: First-turn player is selected randomly and unpredictably
The system MUST select the first-turn player using a uniform random distribution so that neither the host nor the guest has a systematic advantage.

#### Scenario: First turn assigned to host
- **WHEN** the game transitions to `InProgress`
- **AND** the random selection yields the host player
- **THEN** `CurrentTurnPlayerId` is set to the host player's ID

#### Scenario: First turn assigned to guest
- **WHEN** the game transitions to `InProgress`
- **AND** the random selection yields the guest player
- **THEN** `CurrentTurnPlayerId` is set to the guest player's ID

#### Scenario: First-turn selection cannot be influenced by lock order
- **WHEN** the host locks their fleet before the guest
- **AND** subsequently the guest locks their fleet, triggering the `InProgress` transition
- **THEN** the first-turn player is still determined by random selection and not by lock order
