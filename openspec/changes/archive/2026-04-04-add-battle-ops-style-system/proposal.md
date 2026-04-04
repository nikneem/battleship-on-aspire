## Why

The application still uses scaffold-level styling and has no cohesive visual system, theme settings, or reusable design guidance for future UI work. Issue `#7` defines a concrete "Battle Ops" tactical identity, and capturing it in OpenSpec now creates a shared contract for both implementation and future agent-assisted styling work.

## What Changes

- Add a style skill that codifies how agents should apply the Battle Ops brand, theme tokens, typography, logo usage, PrimeNG styling, and tactical microcopy.
- Add a Battle Ops visual theme for the Angular app, including color tokens, typography, logo assets/variants, and PrimeNG-compatible component styling guidance.
- Add user-facing style settings so the app can apply and persist supported appearance preferences without breaking the Battle Ops identity.
- Define brand voice, usage rules, and deliverables so future UI changes stay aligned with the approved tactical design system.

## Capabilities

### New Capabilities
- `style-skill`: Defines the reusable skill and operating rules for applying the Battle Ops visual and UX system consistently across future work.
- `style-theme-settings`: Defines the Battle Ops theme system, branded assets, PrimeNG styling behavior, and supported style settings in the application.

### Modified Capabilities

None.

## Impact

- Affected code: `.github/skills`, `src\App\src`, `src\App\public`, Angular styling/theme assets, and any future PrimeNG setup in the app.
- APIs: none initially, unless settings persistence later requires a server contract.
- Dependencies: likely PrimeNG, PrimeIcons, IBM Plex Mono font integration, and application theme token infrastructure.
- Systems: application branding, UI theming, style preference persistence, and agent guidance for future styling work.
