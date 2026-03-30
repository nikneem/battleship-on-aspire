# Copilot Instructions for `battleship-on-aspire`

## Solution overview

This repository is a Battleship application organized as an Aspire-orchestrated, modular .NET backend plus an Angular frontend.

Top-level runtime pieces:

- `src\Aspire\HexMaster.BattleShip.Aspire.AppHost\` boots the distributed application, the API, the Angular app, and Dapr components.
- `src\HexMaster.BattleShip.Api\` is the ASP.NET Core API entrypoint.
- `src\App\` is the Angular 21 application.
- `openspec\` contains proposal-driven product and implementation changes.

Primary solution file:

- `src\Battleship.slnx`

## Project structure

The backend is organized by domain. The intended pattern is a three-project slice per domain:

- `HexMaster.BattleShip.{Domain}`: implementation
- `HexMaster.BattleShip.{Domain}.Abstractions`: contracts, DTOs, handler interfaces, shared public types
- `HexMaster.BattleShip.{Domain}.Tests`: tests

Current domains in `src\`:

- `Profiles\`
  - `HexMaster.BattleShip.Profiles`
  - `HexMaster.BattleShip.Profiles.Abstractions`
  - `HexMaster.BattleShip.Profiles.Tests`
- `Games\`
  - `HexMaster.BattleShip.Games`
  - `HexMaster.BattleShip.Games.Abstractions`
  - `HexMaster.BattleShip.Games.Tests`
- `Realtime\`
  - `HexMaster.BattleShip.Realtime`
  - `HexMaster.BattleShip.Realtime.Abstractions`
  - `HexMaster.BattleShip.Realtime.Tests`
- shared/cross-cutting:
  - `HexMaster.BattleShip.Core`
  - `HexMaster.BattleShip.Core.Tests`

Aspire support:

- `src\Aspire\HexMaster.BattleShip.Aspire.AppHost\`
- `src\Aspire\HexMaster.BattleShip.Aspire.ServiceDefaults\`

## Backend architecture rules

Follow strict DTO -> command/query -> handler boundaries.

### HTTP boundary rules

For HTTP endpoints:

- Endpoints MUST accept DTO records only.
- DTOs MUST live in `HexMaster.BattleShip.{Domain}.Abstractions.DataTransferObjects` or a deeper namespace under it.
- Endpoints MUST NOT accept commands or queries directly.
- Endpoint code maps DTOs to commands/queries and invokes injected handlers.
- Endpoint responses MUST be DTO records, not domain entities or internal models.

Representative files:

- `src\HexMaster.BattleShip.Api\ProfilesEndpoints.cs`
- `src\Profiles\HexMaster.BattleShip.Profiles.Abstractions\DataTransferObjects\`

### DI and module composition

Each domain should expose a focused service-registration extension method from the implementation project. The API composes modules through those extensions.

Representative files:

- `src\HexMaster.BattleShip.Api\Program.cs`
- `src\Profiles\HexMaster.BattleShip.Profiles\AnonymousPlayerSessionServiceCollectionExtensions.cs`

### CQRS and domain modeling

When adding backend features:

- Put public contracts in the `Abstractions` project.
- Keep implementation details in the domain implementation project.
- Prefer command/query handlers for business logic.
- Avoid direct cross-domain references to non-Abstractions namespaces.

Current code already follows the DTO + handler pattern in `Profiles`. `Games` and `Realtime` are still evolving, so preserve the same architecture as those areas are implemented.

## Frontend architecture rules

The frontend is an Angular 21 standalone application.

Representative files:

- `src\App\angular.json`
- `src\App\src\app\app.config.ts`
- `src\App\src\app\app.routes.ts`
- `src\App\src\app\app.ts`

Current conventions:

- Use standalone components, not NgModules.
- Use `ChangeDetectionStrategy.OnPush`.
- Prefer signals/computed/effect for local UI state.
- Organize UI by route/page folders under `src\App\src\app\pages\`.
- Keep app-wide styling in `src\App\src\styles.scss`.
- Keep cross-cutting UI state in services, e.g.:
  - `src\App\src\app\battle-ops-style-settings.service.ts`
  - `src\App\src\app\battle-ops-sound-settings.service.ts`

The app currently uses:

- PrimeNG with Aura preset
- Battle Ops theme tokens and branding
- route-driven page composition

## Battle Ops UI guidance

For branded UI work, use the repository skill:

- `.github\skills\battle-ops-style\SKILL.md`

Source-of-truth locations:

- shared assets: `src\App\public\branding\`
- theme tokens and PrimeNG overrides: `src\App\src\styles.scss`

When changing UI:

- Preserve the Battle Ops tactical visual language.
- Reuse existing tokens before adding new colors.
- Ensure PrimeNG components inherit Battle Ops styling instead of default stock styling.

## Aspire and runtime composition

The app is intended to run through Aspire.

Representative files:

- `src\Aspire\HexMaster.BattleShip.Aspire.AppHost\AppHost.cs`
- `src\Aspire\HexMaster.BattleShip.Aspire.ServiceDefaults\Extensions.cs`

Important runtime details:

- The API is added as an Aspire project resource.
- The Angular app is added as a JavaScript resource.
- Dapr state store `statestore` is used for anonymous player session persistence.
- Service defaults provide health checks, service discovery, resilience, and OpenTelemetry wiring.

## OpenSpec workflow

This repository uses OpenSpec for change-driven work.

Important directories:

- `openspec\changes\`
- `openspec\changes\archive\`

When work is driven by a feature/change request:

- Use `openspec-propose` to create proposal/design/spec/tasks artifacts.
- Use `openspec-apply-change` to implement tasks.
- Use `openspec-archive-change` after completion.
- Use `openspec-explore` for requirement or design exploration before implementation.

Always keep `tasks.md` in sync with completed work.

## GitHub issue <-> OpenSpec sync

This repo includes a skill for syncing specs with GitHub issues:

- `.github\skills\openspec-sync-github-issues\SKILL.md`

Rules from that workflow:

- Importing an issue into a spec is manual.
- A spec is considered bound to an issue only if it includes a marker like:

```md
<!-- openspec:github-issue owner/repo#123 -->
```

- Only bound specs should sync back to GitHub.
- Sync should update only the managed issue-body block, not overwrite the entire issue body.

## Repo-specific agents and when to use them

This repo defines custom agents under `.github\agents\`.

### `csharp-architecture-enforcer`

Use for:

- architecture reviews
- CQRS validation
- checking endpoint mapping patterns
- validating domain boundaries

Source:

- `.github\agents\csharp-architecture-enforcer.agent.md`

### `domain-model-enforcer`

Use for:

- DDD/domain model validation
- aggregate design
- command-handler and repository-pattern reviews

Source:

- `.github\agents\domain-model-enforcer.agent.md`

## Skills to prefer

Prefer repository skills when applicable:

- `battle-ops-style` for Angular/PrimeNG UI styling
- `openspec-explore` for change exploration
- `openspec-propose` for new change creation
- `openspec-apply-change` for implementation
- `openspec-archive-change` for archiving completed changes
- `openspec-sync-github-issues` for issue/spec synchronization workflows

## Testing and validation commands

Use existing commands; do not invent new validation tooling.

Backend:

```powershell
dotnet test .\src\Battleship.slnx --nologo
```

Frontend:

```powershell
Set-Location .\src\App
npm run build
npm test -- --watch=false
```

OpenSpec:

```powershell
openspec validate <change-name> --type change
```

## MCP configuration

The repository includes `.mcp.json` at the root.

Configured MCP servers:

- `aspire`
- `github`

GitHub MCP requires `GITHUB_PERSONAL_ACCESS_TOKEN` to be present in the environment.

## Practical implementation guidance

When changing backend code:

- start from `Abstractions`
- add DTOs/contracts first
- add handlers and services in the domain project
- wire the module into the API via DI
- expose HTTP endpoints that accept/return DTOs only
- add focused tests in the domain test project

When changing frontend code:

- keep routing under `app.routes.ts`
- place new UI under an appropriate page/feature folder
- use Angular standalone patterns and signals
- keep Battle Ops styling consistent
- add/update Angular tests alongside the change

When changing specs:

- update OpenSpec artifacts first when the work is change-driven
- bind specs to GitHub issues only when explicitly intended
- keep `tasks.md` accurate as implementation progresses
