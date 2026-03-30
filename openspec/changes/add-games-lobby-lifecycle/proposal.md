## Why

GitHub issue `#1` defines the first player-facing Battleship flow: a visitor should be able to start from the app, enter a player name, optionally set a game password, create a game with a server-generated code, and be routed into that lobby as the host.

The repository already has a `Games` module, but the actual Battleship game flow is still missing. We need a clear product contract for creating a game, joining by code, optionally protecting the join flow with a secret, and advancing a match through setup, turn-taking, and completion.

## What Changes

- Add a game lobby flow where one player creates a game and receives a public game code.
- Add the initial create-game entry flow so a visitor can navigate to a create route, establish a player profile, and create a lobby as the host.
- Allow a second player to join a lobby by game code, with optional secret validation when the host protects the lobby.
- Define the game lifecycle from lobby creation through readiness, fleet setup, active turns, completion, cancellation, and abandonment.
- Define caller-safe game views so players can see lobby and match state without leaking hidden opponent board information.
- Define command/query boundaries and DTO-oriented API behavior for the `Games` capability in line with the repository architecture.

## Capabilities

### New Capabilities
- `game-lobbies`: Create, protect, discover, and join Battleship game lobbies by public game code and optional join secret.
- `game-lifecycle`: Manage Battleship game phases, readiness, fleet setup, turn-taking, firing, and terminal outcomes.

### Modified Capabilities

None.

## Impact

- Adds the first substantial requirements for `src\Games\HexMaster.BattleShip.Games` and `src\Games\HexMaster.BattleShip.Games.Abstractions`.
- Extends the future app flow to include a dedicated create-game route and host handoff into `/games/{game-code}`.
- Connects game creation to player profile establishment so the host is added to server-side game state immediately.
- Drives new DTOs, commands, queries, handlers, and endpoint mappings in the API project.
- Establishes the state model that future persistence and realtime updates will depend on.
