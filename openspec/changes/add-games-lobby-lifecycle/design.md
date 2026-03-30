## Context

The repository already separates `Games`, `Games.Abstractions`, `Profiles`, `Realtime`, and the API host, but the `Games` module is still a placeholder. This change introduces the first real Battleship workflow and needs to establish a stable domain boundary that can later support persistence, realtime updates, and player-facing APIs without leaking hidden board state.

GitHub issue `#1` also fixes the first end-to-end host journey: a visitor starts from a create-game entry point, enters a player name, optionally supplies a lobby password, triggers profile creation, creates a game, and is navigated into `/games/{game-code}`.

The repository also has an architectural expectation that HTTP endpoints accept and return DTOs from `Abstractions.DataTransferObjects`, then map those DTOs to commands and queries. The design therefore needs to define clear application messages, caller-aware read models, and an aggregate that owns the core game invariants.

## Goals / Non-Goals

**Goals:**
- Support the visitor-to-host create-game flow described in GitHub issue `#1`.
- Define a `Game` aggregate that owns the lobby lifecycle and the active Battleship match lifecycle.
- Support host-created lobbies with a public game code and an optional join secret.
- Ensure secret-protected joins are secure and do not require storing or returning raw secrets.
- Define caller-safe state transitions for readiness, fleet setup, turn-taking, firing, completion, cancellation, and abandonment.
- Provide a CQRS-friendly shape that fits `Games`, `Games.Abstractions`, and API endpoint mapping.

**Non-Goals:**
- Choosing a concrete database or persistence technology.
- Designing websocket/SignalR transport details for realtime updates.
- Adding player authentication flows beyond using player identity values supplied by the surrounding application.
- Defining matchmaking or public lobby browsing beyond joining by game code.

## Decisions

### Use `Game` as the aggregate root

The aggregate root will be `Game`, with `PlayerSlot`, `Board`, `PlacedShip`, and `ShotRecord` owned beneath it. This keeps membership, readiness, ship placement, turn order, shot resolution, and terminal outcomes inside one consistency boundary.

Alternative considered: separate aggregates for `Lobby` and `GameSession`. This would make the join-to-start transition and win-condition enforcement more complex and would push rule coordination into handlers instead of the domain.

### Keep the top-level state machine small and use derived flags for setup details

The primary phases will be `LobbyOpen`, `LobbyFull`, `Setup`, `InProgress`, `Finished`, `Cancelled`, and `Abandoned`. Per-player readiness, board submission, board locking, and protection status remain subordinate state rather than becoming separate top-level phases.

Alternative considered: modeling every intermediate state as a separate phase, such as `WaitingForReady`, `PlacingShips`, `WaitingForHostLock`, and `WaitingForGuestLock`. That approach is more verbose and makes the aggregate harder to reason about without adding stronger guarantees.

### Model join code as public and join secret as hashed

`GameCode` will be a public, unique, shareable 8-digit value object generated on the server. The optional join secret will never be stored in raw form. Only a password hash and a derived `IsProtected` flag are persisted on the aggregate.

Handlers may verify a raw join secret by using a dedicated hasher/verifier abstraction and then call the aggregate with proof that the secret was valid. API DTOs and read models expose only `GameCode` and `IsProtected`, never the stored hash or raw secret.

Alternative considered: storing the secret in plaintext or encrypting it for later reuse. That would create unnecessary risk because the system only needs one-way verification, not recovery.

### Make read models caller-aware

Queries that return game state must be tailored to the requesting player. The host and guest may see their own boards, fired shots, phase, and turn information, but they must not see hidden ship positions on the opponent board except through outcomes already revealed by gameplay.

Alternative considered: exposing a single, generic `GameDto` for every consumer. That would make accidental board leakage more likely and would blur the difference between lobby summaries and player-specific match state.

### Use command/query boundaries that match the game lifecycle

The design assumes explicit commands such as `CreateGame`, `JoinGameByCode`, `MarkReady`, `SubmitFleet`, `LockFleet`, `FireShot`, `CancelGame`, and `AbandonGame`, plus queries such as `GetGameStateForPlayer` and `GetGameLobbyByCode`. Endpoints will accept DTOs, map them into these application messages, and invoke handlers through DI.

Alternative considered: thin endpoints calling domain methods directly. That would not match the repository’s intended CQRS direction and would make evolution of validation, telemetry, and persistence harder.

### Treat player-profile establishment as a prerequisite to host creation

The UI flow should not allow the visitor to submit `CreateGame` until a valid player name has been accepted and a player profile has been established. The game-creation command therefore receives a player identity that already exists, while the create-game UI orchestrates profile establishment before enabling final submission.

Alternative considered: letting `CreateGame` both create the player profile and create the game in one command. That would blur module boundaries between `Profiles` and `Games` and make retries and validation harder to reason about.

### Return enough creation result data for route handoff

The create-game flow should return the generated game code and any identifiers the client needs to navigate directly into `/games/{game-code}` as the host. This keeps the route handoff deterministic and avoids requiring an extra lobby lookup after creation succeeds.

Alternative considered: forcing the client to re-query for the newly created lobby after submit. That adds a redundant round-trip and complicates error handling for the first-host experience.

### Start active play automatically when both fleets are locked

When both players have submitted valid fleets and locked their boards, the aggregate transitions from `Setup` to `InProgress` without a separate explicit start command. This keeps the workflow deterministic and avoids creating an extra orchestration step that has no independent business meaning.

Alternative considered: adding `StartGameCommand`. That adds another command path and race surface without introducing new domain behavior.

## Risks / Trade-offs

- [Concurrency on join or fire commands] -> Use optimistic concurrency at the persistence boundary so only one guest can fill the second slot and duplicate turn actions are rejected cleanly.
- [Leaking opponent board state in queries] -> Use caller-specific DTOs and dedicated query handlers rather than returning domain objects directly.
- [Secret verification drifting into endpoints] -> Keep secret verification in application handlers behind an abstraction so the aggregate remains secret-agnostic but invariants stay enforced.
- [Profile creation and game creation becoming tightly coupled] -> Keep profile establishment in the surrounding application flow so the `Games` module depends on a player identifier, not on `Profiles` internals.
- [State machine becoming too coarse] -> Keep subordinate readiness and board-lock state explicit on `PlayerSlot` and `Board`, with tests covering transition guards.
- [Terminal-state semantics around abandonment] -> Treat abandonment as a neutral terminal state with no winner so joined sessions end consistently without implying a forfeit rule that has not been designed.

## Migration Plan

This is a greenfield capability, so no data migration is required. Implementation should land in vertical slices: abstractions and DTOs first, domain and handlers second, API endpoints third, followed by tests. If rollback is needed, the change can be removed feature-by-feature because no existing user-facing gameplay behavior depends on it yet.

## Resolved Rules

- Turns always alternate after every shot, even after a hit or sink, unless the shot ends the game.
- Abandonment by either joined player ends the game as `Abandoned` with no winner recorded.
- Lobby expiry remains out of scope for this change and can be designed as a follow-up capability.
