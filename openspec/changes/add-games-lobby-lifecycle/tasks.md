## 1. Games abstractions

- [x] 1.1 Add `Games.Abstractions` DTOs for creating a game, joining by code, reading lobby state, and reading player-specific game state, including the create-game result data needed for route handoff.
- [x] 1.2 Add command and query contracts for creating a game, joining by code, marking ready, submitting fleets, locking fleets, firing shots, cancelling, abandoning, and reading game state.
- [x] 1.3 Add shared enums/value contracts for game phases, lobby protection visibility, shot outcomes, and player-facing state projections.

## 2. Games domain model

- [x] 2.1 Replace placeholder `Games` domain types with a `Game` aggregate and supporting entities/value objects for player slots, boards, ship placement, coordinates, and shot records.
- [x] 2.2 Implement game code generation and aggregate state transitions for `LobbyOpen`, `LobbyFull`, `Setup`, `InProgress`, `Finished`, `Cancelled`, and `Abandoned`.
- [x] 2.3 Implement join-secret protection using a hashed secret model and enforce join validation without storing raw secret material.
- [x] 2.4 Implement domain rules for readiness, fleet validation, board locking, turn ownership, duplicate-shot rejection, and terminal outcomes.

## 3. Application handlers and API integration

- [x] 3.1 Add command and query handlers that map DTO-driven requests onto the `Game` aggregate and return caller-safe read models, ensuring host creation uses an existing player identity.
- [x] 3.2 Add minimal API endpoints that accept DTOs from `Games.Abstractions.DataTransferObjects`, map them to commands or queries, and invoke handlers through DI.
- [x] 3.3 Wire `Games` services, handlers, and any required secret hashing abstraction into the API startup configuration.

## 4. App create-game flow

- [x] 4.1 Add a create-game route and UI that lets a visitor enter a player name and an optional lobby password from the app.
- [x] 4.2 Establish the player profile as part of the create-game flow and enable submission only after player setup succeeds.
- [x] 4.3 Submit the create-game request, then navigate the host to `/games/{game-code}` using the server-generated game code.

## 5. Verification

- [x] 5.1 Add unit tests for aggregate creation, protected and unprotected joins, phase transitions, fleet validation, shot legality, and terminal states.
- [x] 5.2 Add API or handler-level tests for DTO mapping and caller-safe read models that do not leak hidden opponent board data.
- [x] 5.3 Add frontend tests for the create-game flow, including player-name gating and post-create navigation.
- [x] 5.4 Run the repository build and test commands and fix any issues needed for the new `Games` flow to pass cleanly.
