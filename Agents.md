# Agents and Skills

This file lists the custom agents and repository skills configured in this repository under `.github\agents\` and `.github\skills\`.

## Agents

### `csharp-architecture-enforcer`

- File: `.github\agents\csharp-architecture-enforcer.agent.md`
- Purpose: Reviews and designs C# code to enforce the repository's modular-monolith, CQRS, and feature-driven architecture.
- Use for:
  - architecture reviews
  - CQRS validation
  - endpoint mapping checks
  - domain boundary validation
- Key rules it enforces:
  - each domain should follow the `Implementation` + `Abstractions` + `Tests` project pattern
  - endpoints accept DTOs only
  - endpoints map DTOs to commands or queries before invoking DI-injected handlers
  - response payloads must be DTOs from `Abstractions.DataTransferObjects`

### `domain-model-enforcer`

- File: `.github\agents\domain-model-enforcer.agent.md`
- Purpose: Validates domain-driven design patterns, especially domain-model ownership, command/query handling, and repository interactions.
- Use for:
  - domain model reviews
  - aggregate design
  - command-handler validation
  - repository-pattern validation
- Key rules it enforces:
  - modifications happen through domain models
  - command handlers follow factory/fetch -> modify -> persist
  - queries return DTOs directly instead of creating unnecessary domain models
  - domain models should have corresponding abstraction interfaces

## Skills

### `battle-ops-style`

- File: `.github\skills\battle-ops-style\SKILL.md`
- Purpose: Applies the Battle Ops tactical UI identity to Angular and PrimeNG work.
- Covers:
  - palette
  - typography
  - logo usage rules
  - PrimeNG styling guidance
  - tactical microcopy and motion/settings constraints

### `openspec-apply-change`

- File: `.github\skills\openspec-apply-change\SKILL.md`
- Purpose: Implements tasks from an existing OpenSpec change.
- Use for:
  - continuing implementation
  - working through `tasks.md`
  - reading proposal/design/spec context before coding

### `openspec-archive-change`

- File: `.github\skills\openspec-archive-change\SKILL.md`
- Purpose: Archives a completed OpenSpec change after checking artifact and task completion.
- Use for:
  - finalizing completed changes
  - moving completed work into the archive workflow

### `openspec-explore`

- File: `.github\skills\openspec-explore\SKILL.md`
- Purpose: Explore mode for thinking, requirement clarification, codebase investigation, and design discussion.
- Important note:
  - this mode is for exploration, not implementation

### `openspec-propose`

- File: `.github\skills\openspec-propose\SKILL.md`
- Purpose: Creates a new OpenSpec change with proposal, design, specs, and tasks.
- Use for:
  - turning a feature idea into an implementation-ready change
  - scaffolding change artifacts in dependency order

### `openspec-sync-github-issues`

- File: `.github\skills\openspec-sync-github-issues\SKILL.md`
- Purpose: Synchronizes OpenSpec specs with GitHub issues.
- Supports:
  - manual GitHub issue -> OpenSpec spec import
  - automatic bound-spec -> GitHub issue sync
- Important rules:
  - issue import is always manual
  - a spec syncs only when it contains an explicit binding marker
  - sync updates only the managed block in the GitHub issue body
  - when applying changes, commits should be frequent

## Source directories

- Agents: `.github\agents\`
- Skills: `.github\skills\`
