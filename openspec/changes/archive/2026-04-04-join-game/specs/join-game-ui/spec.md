## ADDED Requirements

### Requirement: Player can navigate to join a game
The system SHALL provide a join-game entry point on the landing page and a dedicated `/games/join` route where a player can enter a game code.

#### Scenario: Navigate to join from the landing page
- **WHEN** a visitor selects the "Join Game" call-to-action from the landing page
- **THEN** the client navigates to the `/games/join` route
- **AND** a form is displayed with a game-code field and an optional join-secret field

### Requirement: Player can join a game by entering a code
The system SHALL allow a player to submit a game code (and optional join secret) from the join-game page and be admitted to the lobby.

#### Scenario: Successfully join an open lobby
- **WHEN** a player enters a valid game code for a joinable lobby and submits the form
- **THEN** the system calls `POST /api/games/join` with the game code and player identity
- **AND** on success, the client navigates to `/games/{gameCode}`

#### Scenario: Successfully join a protected lobby
- **WHEN** a player enters a valid game code and the correct join secret and submits the form
- **THEN** the system calls `POST /api/games/join` with the game code, player identity, and join secret
- **AND** on success, the client navigates to `/games/{gameCode}`

#### Scenario: Show error when join fails
- **WHEN** the join request is rejected (wrong secret, lobby full, not found, etc.)
- **THEN** the client displays an error message to the player
- **AND** the player remains on the join-game page to try again

#### Scenario: Join is disabled until a game code is entered
- **WHEN** the game-code field is empty
- **THEN** the join submit action is disabled
