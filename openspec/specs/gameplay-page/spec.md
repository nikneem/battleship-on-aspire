<!-- openspec:github-issue nikneem/battleship-on-aspire#2 -->
> Synced from GitHub issue #2 by manual import.

## ADDED Requirements

### Requirement: Gameplay route presents the current match shell
The system SHALL provide a gameplay page at `/games/{game-code}` that presents the current player's summary, the opponent summary, the current game state, and the player's setup board.

#### Scenario: Open the gameplay page
- **WHEN** a player navigates to `/games/{game-code}`
- **THEN** the system renders a gameplay page for that game code
- **AND** the page shows the current player in the top-left area, the opponent in the top-right area, and the game state in the top-center area

### Requirement: Gameplay page shows an empty setup board and ship inventory
The system SHALL display a Battleship setup board together with the full ship inventory that the player can place during setup.

#### Scenario: Render the initial setup state
- **WHEN** the gameplay page is shown for a player who has not locked their fleet
- **THEN** the system shows an empty play field for ship placement
- **AND** the system shows the available ships with their different sizes in a ship area below the board

### Requirement: Players can place and reposition ships on the board
The system SHALL allow players to drag ships from the ship area onto the setup board, reposition already placed ships, and continue placing remaining ships until the fleet is complete.

#### Scenario: Place a ship on the board
- **WHEN** the player drags a ship onto the play field
- **THEN** the system places the ship onto snapped board cells within the play field bounds

#### Scenario: Reposition a placed ship
- **WHEN** the player drags a ship that is already positioned on the play field
- **THEN** the system allows the ship to be moved to a new valid snapped position on the play field
- **AND** the player may still drag other remaining ships onto the play field afterward

### Requirement: Placed ships can be rotated through contextual controls
The system MUST expose rotation controls for ships that have already been positioned on the play field so players can adjust orientation without removing the ship from setup.

#### Scenario: Show contextual rotation options for a placed ship
- **WHEN** a positioned ship is selected in context on the play field
- **THEN** the system shows rotation options for that ship

#### Scenario: Keep rotated ships within the board
- **WHEN** the player applies a rotation option to a positioned ship
- **THEN** the system updates the ship orientation
- **AND** no part of the ship ends up outside the play field bounds

### Requirement: Ready becomes available only after full placement
The system SHALL reveal a `Ready` action in the game-state area only after all required ships have been positioned on valid board cells.

#### Scenario: Show Ready after the fleet is fully placed
- **WHEN** the player has positioned all required ships on the play field
- **THEN** the system shows a `Ready` action in the game-state area
- **AND** the action visually draws attention to itself

### Requirement: Confirming Ready locks the player's fleet
The system MUST lock the player's ship positions after the player confirms readiness so ships can no longer be moved.

#### Scenario: Lock the fleet after confirmation
- **WHEN** the player clicks the `Ready` action after fully placing the fleet
- **THEN** the system fixes the ships in their current positions
- **AND** the ships can no longer be dragged or rotated
