## ADDED Requirements

### Requirement: Anonymous player session creation
The system SHALL allow a client to create an anonymous player session by submitting a player name. When a session is created, the system SHALL generate a new temporary player identifier, persist a player record in the Dapr state store, and return a JWT access token for future requests.

#### Scenario: First visit creates a temporary player
- **WHEN** a client submits a valid player name for a new anonymous session
- **THEN** the system creates a new temporary player identifier
- **AND** persists a player record containing the player identifier and player name in the Dapr state store
- **AND** applies a one-hour time-to-live to the persisted player record
- **AND** returns a JWT access token for that temporary player session

#### Scenario: Later visit creates a fresh temporary player from stored name
- **WHEN** a later browser visit submits a player name previously stored in local storage
- **THEN** the system creates a new temporary player identifier
- **AND** persists a new player record with a one-hour time-to-live
- **AND** returns a JWT access token for the new temporary player session

### Requirement: JWT claims for anonymous player sessions
The system SHALL issue JWT access tokens for anonymous player sessions that contain the temporary player identifier and player name claims needed to verify gameplay requests.

#### Scenario: Session token includes player identity claims
- **WHEN** the system issues a JWT for an anonymous player session
- **THEN** the token contains the temporary player identifier as the subject claim
- **AND** the token contains the player name as a claim

### Requirement: Token renewal for active anonymous sessions
The system SHALL allow an active anonymous player session to renew its JWT access token before expiration without creating a new player identifier, as long as the backing player record still exists.

#### Scenario: Active session renews near token expiry
- **WHEN** a client presents a valid anonymous player JWT that is nearing expiration
- **THEN** the system issues a replacement JWT for the same temporary player identifier
- **AND** includes the same player name claim in the replacement token

#### Scenario: Renewal fails after temporary player record expires
- **WHEN** a client requests renewal for an anonymous player JWT whose backing player record no longer exists in the Dapr state store
- **THEN** the system rejects the renewal request
- **AND** requires the client to create a new anonymous player session

### Requirement: Bearer token verification for gameplay requests
The system SHALL verify anonymous player JWTs on authenticated requests so that downstream gameplay actions can trust the temporary player identity.

#### Scenario: Authenticated request uses anonymous player token
- **WHEN** a gameplay-related request includes a valid anonymous player JWT bearer token
- **THEN** the system authenticates the request using the token
- **AND** makes the temporary player identifier and player name claims available to the request handler
