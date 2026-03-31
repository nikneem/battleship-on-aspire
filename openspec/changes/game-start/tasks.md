## 1. Random turn selection in Games domain

- [ ] 1.1 Add `IRandomProvider` interface to `HexMaster.BattleShip.Games.Abstractions` (or `Core`) with a single `bool NextBool()` method, and a `SystemRandomProvider` implementation registered in `GameServiceCollectionExtensions`
- [ ] 1.2 Update `Game.LockFleet(playerId, bool hostGoesFirst)` signature (or equivalent) so the aggregate accepts the pre-resolved random result instead of calling `System.Random` directly
- [ ] 1.3 In `LockFleetCommandHandler`, inject `IRandomProvider`, resolve `hostGoesFirst = _random.NextBool()`, and pass it to `game.LockFleet`
- [ ] 1.4 Remove the hard-coded `CurrentTurnPlayerId = host.PlayerId` assignment and replace it with the randomly selected player's ID
- [ ] 1.5 Write unit tests in `HexMaster.BattleShip.Games.Tests` covering: host selected first, guest selected first, and that lock order does not influence selection

## 2. GameStartedIntegrationEvent definition

- [ ] 2.1 Add `GameStartedIntegrationEvent` record to `HexMaster.BattleShip.Games.Abstractions` (namespace `HexMaster.BattleShip.Games.Abstractions.IntegrationEvents`) with properties `GameCode` (string) and `FirstTurnPlayerId` (string) — note: move to shared events project once `add-integration-events-and-realtime` lands
- [ ] 2.2 Confirm or add the Dapr topic constant `battleship.game.game-started` (string) alongside other topic constants in `Games.Abstractions` or `Core`

## 3. Publish GameStartedIntegrationEvent from LockFleetCommandHandler

- [ ] 3.1 Inject `DaprClient` (or an `IEventPublisher` abstraction) into `LockFleetCommandHandler`
- [ ] 3.2 After the game transitions to `InProgress`, publish `GameStartedIntegrationEvent` with `GameCode` and `FirstTurnPlayerId` to the Dapr pub/sub topic `battleship.game.game-started`
- [ ] 3.3 Ensure the event is published **only** when the game phase actually becomes `InProgress` (i.e. the second player locked); skip if only one fleet is locked
- [ ] 3.4 Write handler unit tests: verify event is published on second lock, verify event is not published on first lock

## 4. Realtime domain — subscribe and fan out GameStarted

> ⚠️ Depends on `add-integration-events-and-realtime` delivering the SignalR hub scaffold. If the hub does not yet exist, add a placeholder subscriber and wire it once the hub is available.

- [ ] 4.1 Add a Dapr topic subscriber in the Realtime domain for topic `battleship.game.game-started` that deserialises `GameStartedIntegrationEvent`
- [ ] 4.2 In the subscriber, resolve the SignalR `IHubContext<GameHub>` and send a `GameStarted` message to the group for the affected `GameCode`, carrying `GameCode` and `FirstTurnPlayerId`
- [ ] 4.3 Ensure the subscriber silently no-ops when no clients are connected to the group (no exception thrown)
- [ ] 4.4 Register the subscriber endpoint in `RealtimeServiceCollectionExtensions` and wire up the Dapr subscription in the AppHost or service registration

## 5. Frontend — view routing on GameStarted

> ⚠️ Attack and defence view components are delivered by `add-gameplay-combat`. Until they exist, add a signal-driven state placeholder that will render them once they are available.

- [ ] 5.1 In the in-game shell component (`game-route-shell.ts`), inject the SignalR connection service and subscribe to the `GameStarted` hub method
- [ ] 5.2 Add `gamePhase = signal<'setup' | 'attack' | 'defence' | null>(null)` (or extend the existing phase signal) to the shell component
- [ ] 5.3 On `GameStarted` message: compare `firstTurnPlayerId` against the current player's profile ID; set `gamePhase` to `'attack'` if matching, `'defence'` otherwise
- [ ] 5.4 Update the shell template with `@if (gamePhase() === 'attack')` and `@if (gamePhase() === 'defence')` blocks — render placeholder text or the actual attack/defence components once `add-gameplay-combat` provides them
- [ ] 5.5 Write unit tests for the shell component: verify `'attack'` state when IDs match, `'defence'` state when they do not

## 6. Validation

- [ ] 6.1 Run `dotnet test .\src\Battleship.slnx --nologo` and confirm all existing tests pass plus new tests added in tasks 1.5, 3.4, and 5.5
- [ ] 6.2 Run `npm run build` and `npm run test -- --watch=false` from `src\App` to confirm no frontend regressions
