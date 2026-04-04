## Why

The application needs a lightweight way for players to start playing without creating an account or integrating with an external identity provider. Temporary anonymous sessions let the server verify gameplay requests with JWTs while keeping player state short-lived and operationally simple.

## What Changes

- Add an anonymous player session flow that accepts a player name and creates a temporary server-side player record.
- Persist temporary player records in a Dapr state store with a one-hour time-to-live.
- Issue JWT access tokens that include the temporary player identifier and player name claims.
- Add token renewal behavior so active browser sessions can continue without manual re-entry while a token is nearing expiration.
- Allow later browser visits to use the locally stored name to create a fresh temporary player session with a new player identifier.

## Capabilities

### New Capabilities
- `anonymous-player-sessions`: Create, persist, renew, and validate short-lived anonymous player sessions backed by Dapr state and JWTs.

### Modified Capabilities

None.

## Impact

- Affected code: `src\\Profiles\\HexMaster.BattleShip.Profiles`, `src\\Profiles\\HexMaster.BattleShip.Profiles.Abstractions`, `src\\HexMaster.BattleShip.Api`, and `src\\Aspire\\HexMaster.BattleShip.Aspire.AppHost`
- APIs: new anonymous session creation and renewal endpoints plus JWT-protected gameplay integration points
- Dependencies: Dapr state store integration and ASP.NET Core JWT bearer authentication
- Systems: browser local storage behavior, temporary player lifecycle, and short-lived identity for gameplay requests
