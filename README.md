# battleship-on-aspire

## MCP configuration

This repository includes a project-level `.mcp.json` that enables:

- `aspire` via the local `aspire agent mcp` command
- `github` via GitHub's hosted MCP endpoint

To use the GitHub MCP server, set a `GITHUB_PERSONAL_ACCESS_TOKEN` environment variable before starting your MCP host or Copilot CLI session.

## Battle Ops style system

UI styling work should follow the Battle Ops repository skill at `.github/skills/battle-ops-style/SKILL.md`.

Shared brand assets live under `src\App\public\branding\`, and the Angular theme/settings implementation lives under `src\App\src\`.
