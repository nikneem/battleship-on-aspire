## Purpose

Define how Battleship game lobbies are created, protected, joined by public game code, and exposed through client-safe lobby state responses.

## Requirements

### Requirement: Visitors can enter the create-game flow
The system SHALL provide a create-game entry flow where a visitor can navigate from the landing page to a create-game route, enter a player name, and optionally provide a lobby password before creating a game.

#### Scenario: Navigate from the landing page to create a game
- **WHEN** a visitor selects the create-game call to action from the landing page
- **THEN** the client navigates the visitor to the create-game route
- **AND** the create-game form is displayed with fields for player name and an optional lobby password

#### Scenario: Enable game creation only after player setup
- **WHEN** the visitor enters a valid player name in the create-game flow
- **THEN** the surrounding application establishes the player profile needed for hosting
- **AND** the create-game action becomes enabled only after that prerequisite succeeds

### Requirement: Host can create a game lobby
The system SHALL allow a player to create a Battleship game lobby that reserves the host as the first participant, assigns a unique server-generated 8-digit public game code, and optionally protects the lobby with a join secret.

#### Scenario: Create an open lobby
- **WHEN** a player creates a lobby without a join secret
- **THEN** the system creates a lobby in the `LobbyOpen` phase
- **AND** the system assigns a unique server-generated 8-digit public game code
- **AND** the lobby is marked as not protected

#### Scenario: Create a protected lobby
- **WHEN** a player creates a lobby with a join secret
- **THEN** the system creates a lobby in the `LobbyOpen` phase
- **AND** the system assigns a unique server-generated 8-digit public game code
- **AND** the lobby is marked as protected
- **AND** the raw join secret is not returned in subsequent lobby state responses

#### Scenario: Host is added to game state at creation time
- **WHEN** a player successfully creates a game lobby
- **THEN** the system adds that player as the host participant in server-side game state
- **AND** the creation result includes the generated game code needed for host navigation

#### Scenario: Navigate into the created game
- **WHEN** the create-game request succeeds
- **THEN** the client navigates the host to `/games/{game-code}`
- **AND** `{game-code}` matches the server-generated code for the created lobby

### Requirement: Guest can join a lobby by game code
The system SHALL allow a second player to join an open lobby by supplying the lobby game code, provided the lobby is still joinable and the joining player is distinct from the host.

#### Scenario: Join an unprotected lobby
- **WHEN** a second player submits a valid game code for a lobby in the `LobbyOpen` phase
- **THEN** the system adds that player as the guest participant
- **AND** the lobby transitions to the `LobbyFull` phase

#### Scenario: Reject joining an unavailable lobby
- **WHEN** a player submits a game code for a lobby that does not exist, is already full, or is no longer joinable
- **THEN** the system rejects the join request
- **AND** the existing lobby state remains unchanged

#### Scenario: Reject host joining their own lobby
- **WHEN** the host submits the game code for their own lobby as a guest join request
- **THEN** the system rejects the join request
- **AND** the lobby remains in its current phase

### Requirement: Protected lobbies require secret validation
The system MUST require a valid join secret for joining a protected lobby, and it MUST store only a verifiable representation of that secret.

#### Scenario: Join a protected lobby with the correct secret
- **WHEN** a second player submits a valid game code and the correct join secret for a protected lobby in the `LobbyOpen` phase
- **THEN** the system admits the player to the lobby
- **AND** the lobby transitions to the `LobbyFull` phase

#### Scenario: Reject joining a protected lobby with the wrong secret
- **WHEN** a player submits a valid game code with an invalid join secret for a protected lobby
- **THEN** the system rejects the join request
- **AND** the guest slot remains empty

#### Scenario: Protect stored secret material
- **WHEN** the system persists or returns lobby state for a protected lobby
- **THEN** the system stores only a hashed or otherwise one-way verifiable representation of the join secret
- **AND** the system does not return the raw join secret or its stored representation in lobby responses

### Requirement: Lobby state responses are safe for clients
The system SHALL provide lobby state responses that expose the game code, protection status, participants, and joinability information without disclosing secret material or hidden gameplay state.

#### Scenario: Read lobby state before a guest joins
- **WHEN** a client requests lobby state for a valid game code in the `LobbyOpen` phase
- **THEN** the response includes the game code, host participation, current phase, and whether the lobby is protected
- **AND** the response does not include any join secret material

#### Scenario: Read lobby state after a guest joins
- **WHEN** a joined player requests lobby state for a lobby in the `LobbyFull` phase
- **THEN** the response includes both participants and the current phase
- **AND** the response does not reveal any gameplay information that does not yet exist
