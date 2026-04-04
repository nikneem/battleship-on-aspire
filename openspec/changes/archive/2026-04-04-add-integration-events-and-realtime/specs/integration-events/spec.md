## ADDED Requirements

### Requirement: Versioned base integration event record
The system SHALL provide an abstract `IntegrationEvent` base record in `HexMaster.BattleShip.IntegrationEvents` that carries `EventId` (unique identifier), `OccurredOn` (UTC timestamp), and an abstract `SchemaVersion` property. All concrete integration events SHALL inherit from this record and declare their own schema version string.

#### Scenario: Base record fields are auto-populated
- **WHEN** a concrete integration event is instantiated
- **THEN** `EventId` SHALL be a new non-empty GUID string and `OccurredOn` SHALL be the current UTC time

#### Scenario: Concrete event declares schema version
- **WHEN** a concrete integration event type is defined
- **THEN** it SHALL provide a non-empty `SchemaVersion` string (e.g., `"1.0"`)

### Requirement: Integration event types for Games domain actions
The system SHALL define the following sealed record types in `HexMaster.BattleShip.IntegrationEvents`, each carrying the minimum payload needed for subscribers to act without querying back:
- `GameCreatedIntegrationEvent` (GameCode, HostPlayerId, HostPlayerName)
- `PlayerJoinedGameIntegrationEvent` (GameCode, GuestPlayerId, GuestPlayerName)
- `PlayerMarkedReadyIntegrationEvent` (GameCode, PlayerId)
- `FleetSubmittedIntegrationEvent` (GameCode, PlayerId) — MUST NOT include ship coordinates
- `FleetLockedIntegrationEvent` (GameCode, PlayerId)
- `GameStartedIntegrationEvent` (GameCode, FirstTurnPlayerId)
- `ShotFiredIntegrationEvent` (GameCode, FiringPlayerId, TargetRow, TargetColumn, Outcome)
- `GameFinishedIntegrationEvent` (GameCode, WinnerPlayerId)
- `GameCancelledIntegrationEvent` (GameCode, CancelledByPlayerId)
- `GameAbandonedIntegrationEvent` (GameCode, AbandoningPlayerId)

#### Scenario: Shot fired event carries outcome
- **WHEN** `ShotFiredIntegrationEvent` is published
- **THEN** the `Outcome` field SHALL be one of `Miss`, `Hit`, or `Sunk`

#### Scenario: Fleet submitted event carries no coordinates
- **WHEN** `FleetSubmittedIntegrationEvent` is published
- **THEN** the payload SHALL NOT contain any ship position, orientation, or size information

### Requirement: Integration event types for Realtime connection lifecycle
The system SHALL define the following sealed record types for connection events:
- `PlayerConnectionLostIntegrationEvent` (GameCode, PlayerId, DisconnectedAt)
- `PlayerConnectionReestablishedIntegrationEvent` (GameCode, PlayerId)
- `PlayerConnectionTimedOutIntegrationEvent` (GameCode, PlayerId)

#### Scenario: Connection lost event records timestamp
- **WHEN** `PlayerConnectionLostIntegrationEvent` is created
- **THEN** `DisconnectedAt` SHALL be set to the UTC time of detection

### Requirement: Dapr topic name constants
The system SHALL provide a static class `IntegrationEventTopics` containing a string constant for each integration event type, following the naming convention `battleship.{domain}.{verb-past-tense}` (e.g., `battleship.game.shot-fired`, `battleship.player.connection-lost`).

#### Scenario: Topic constant matches event type
- **WHEN** a handler publishes an integration event
- **THEN** it SHALL use the corresponding constant from `IntegrationEventTopics` as the Dapr topic name

### Requirement: Zero external dependencies
The `HexMaster.BattleShip.IntegrationEvents` project SHALL have no references to any other project in the solution and no NuGet dependencies beyond the .NET BCL.

#### Scenario: Project compiles without domain references
- **WHEN** the IntegrationEvents project is built in isolation
- **THEN** it SHALL compile successfully without referencing Games, Realtime, Core, or any domain project
