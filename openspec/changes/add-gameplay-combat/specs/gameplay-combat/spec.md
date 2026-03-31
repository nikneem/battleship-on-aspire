<!-- openspec:github-issue nikneem/battleship-on-aspire#3 -->
> Synced from GitHub issue #3 by manual import.

## ADDED Requirements

### Requirement: Gameplay page switches between attack and defend modes based on turn
The system SHALL display the gameplay board in **attack mode** when it is the current player's turn, and in **defend mode** when it is the opponent's turn. The active mode SHALL update reactively whenever a `ShotFired` or `GameStarted` SignalR event is received.

#### Scenario: Board enters attack mode on player's turn
- **WHEN** the current turn belongs to the authenticated player
- **THEN** the board SHALL render in attack mode showing only the grid without the player's own ships
- **AND** cells SHALL be clickable for targeting

#### Scenario: Board enters defend mode on opponent's turn
- **WHEN** the current turn belongs to the opponent
- **THEN** the board SHALL render in defend mode showing the player's own positioned ships
- **AND** board cells SHALL NOT be interactive for targeting

#### Scenario: Mode updates after a shot is processed
- **WHEN** a `ShotFired` SignalR event is received and the turn changes
- **THEN** the board SHALL immediately switch to the correct mode without a page reload

### Requirement: Attack mode shows pre-selection and fire controls
In attack mode the system SHALL allow the player to select a target cell and fire a shot.

#### Scenario: Cell is selected for targeting
- **WHEN** the player clicks an untargeted cell in attack mode
- **THEN** the cell SHALL display a semi-transparent red selection marker
- **AND** a "Fire" button SHALL appear in the game-state area

#### Scenario: Previously fired cells are not selectable
- **WHEN** the player clicks a cell that has already been targeted in attack mode
- **THEN** the click SHALL have no effect and the selection SHALL not change

#### Scenario: Fire button submits the shot
- **WHEN** the player clicks "Fire" with a cell selected
- **THEN** the system SHALL submit the shot to the backend
- **AND** the "Fire" button SHALL be disabled while the request is in-flight

#### Scenario: Fire button is absent without a selection
- **WHEN** no cell is selected in attack mode
- **THEN** the "Fire" button SHALL NOT be visible in the game-state area

### Requirement: Shot markers reflect outcomes in attack mode
After a shot is processed, the targeted cell SHALL display a permanent marker reflecting the server-confirmed outcome.

#### Scenario: Miss is shown as a solid red dot
- **WHEN** a fired shot results in a `Miss` outcome
- **THEN** the targeted cell SHALL display a solid red dot marker
- **AND** the semi-transparent selection marker SHALL be replaced

#### Scenario: Hit is shown as a red target icon
- **WHEN** a fired shot results in a `Hit` outcome
- **THEN** the targeted cell SHALL display a red target icon marker

#### Scenario: Sunk is shown as a red target icon
- **WHEN** a fired shot results in a `Sunk` outcome
- **THEN** the targeted cell SHALL display a red target icon marker (same as hit)

### Requirement: Defend mode overlays opponent shot markers on own ships
In defend mode the system SHALL show the player's own ships at their locked positions and overlay markers where the opponent has fired.

#### Scenario: Opponent miss shown as solid blue dot
- **WHEN** the opponent fired at a coordinate that does not occupy a player ship
- **THEN** the cell SHALL display a solid blue dot marker over the empty grid cell

#### Scenario: Opponent hit shown as blue target icon
- **WHEN** the opponent fired at a coordinate that occupies a player ship
- **THEN** the cell SHALL display a blue target icon marker over the ship segment

### Requirement: Game state area reflects current turn and phase
The game-state area SHALL always display the current phase and, during `InProgress`, which player holds the turn.

#### Scenario: Active turn indicator shown during play
- **WHEN** the game is in the `InProgress` phase
- **THEN** the game-state area SHALL show a turn indicator identifying whose turn it is

#### Scenario: Game finished state displayed in state area
- **WHEN** a `GameFinished` SignalR event is received
- **THEN** the game-state area SHALL display the result (win or loss for the current player)
- **AND** the board SHALL become non-interactive

#### Scenario: Game abandoned state redirects player
- **WHEN** a `GameAbandoned` SignalR event is received
- **THEN** the system SHALL navigate the player to the home page

### Requirement: Page hydrates full state from REST on initial load
On entering the gameplay page, the system SHALL fetch the current game state from the REST API before connecting to SignalR, so that players who navigate in after game start see the correct board without waiting for a push event.

#### Scenario: Late-joining player sees current board state
- **WHEN** a player navigates to `/games/{game-code}` after the game has started
- **THEN** the system SHALL load the current game state via the `GetGameStateForPlayer` endpoint
- **AND** all previously fired shots SHALL be visible on the board before any new SignalR event arrives

### Requirement: SignalR connection managed by a singleton service
A `GameSignalRService` SHALL manage the hub connection lifecycle. It SHALL connect when the gameplay page is activated and disconnect when the player leaves the page.

#### Scenario: Connection established on page enter
- **WHEN** the player navigates to the gameplay page
- **THEN** the `GameSignalRService` SHALL establish a connection to `/hubs/game`
- **AND** invoke `JoinGame` with the current game code and player id

#### Scenario: Connection closed on page leave
- **WHEN** the player navigates away from the gameplay page
- **THEN** the `GameSignalRService` SHALL close the SignalR connection
