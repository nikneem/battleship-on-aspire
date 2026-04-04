## ADDED Requirements

### Requirement: Host lobby view updates in real-time when guest joins
The system SHALL update the host's lobby view without a page reload when a guest successfully joins, using the SignalR `PlayerJoined` event.

#### Scenario: Host sees guest arrive in lobby
- **WHEN** a guest joins a `LobbyOpen` lobby
- **THEN** the host's lobby view receives a `PlayerJoined` SignalR event
- **AND** the lobby view re-fetches or updates the lobby state to show the guest participant
- **AND** the displayed phase changes to `LobbyFull`

#### Scenario: Guest sees their own lobby state on arrival
- **WHEN** a guest successfully joins and is navigated to `/games/{gameCode}`
- **THEN** the game-route-shell loads the current lobby state for the guest
- **AND** both participants are shown in the `LobbyFull` view
