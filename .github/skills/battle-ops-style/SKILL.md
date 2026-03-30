---
name: battle-ops-style
description: Apply the Battle Ops tactical brand to UI work, including theme tokens, logo usage, PrimeNG styling, and microcopy.
license: MIT
compatibility: Best used for Angular and PrimeNG UI changes in this repository.
metadata:
  author: nikneem
  version: "1.0"
  generatedBy: "manual"
---

Apply the Battle Ops tactical identity consistently across repository UI work.

## Purpose

Use this skill whenever you create or modify branded UI in this repository. It converts the approved Battle Ops brand into operational guidance for implementation work so future UI changes stay visually and verbally consistent.

## Source of truth

- Brand assets live under `src\App\public\branding\`
- Theme tokens and PrimeNG styling live in the Angular app styles under `src\App\src\`
- This skill is the behavior guide for future contributors and agents

## Visual identity

### Palette

- Abyss Black: `#05070A`
- Midnight Blue: `#0A1A2A`
- Electric Cyan: `#00E5FF`
- Soft Aqua: `#66F2FF`
- Steel Blue-Gray: `#2C3A47`
- Amber: `#FFB300`
- Damage Red: `#B33A3A`

Use dark surfaces by default. Bright accents should feel like sonar or tactical instrumentation, not playful neon clutter.

### Typography

- Primary font: IBM Plex Mono
- Style: uppercase-forward, monospace, tactical, condensed spacing
- Default letter spacing target: around `0.5px`
- Size guidance:
  - Display: `32px`
  - Heading: `20px`
  - Body: `14px`
  - Label: `12px`

### Logo variants

Supported variants:

- Hero logo
- Minimal logo
- Icon/favicon logo
- Monochrome logo

Rules:

- Use on dark backgrounds
- Preserve proportions
- Do not stretch
- Do not recolor outside approved variants
- Do not add shadows or noisy backgrounds behind the logo
- Sweep angle should remain in the approved northeast tactical posture when represented statically

## Motion and settings

Supported Battle Ops-compatible settings:

- Reduced motion
- Density
- Accent intensity

Do not introduce arbitrary user theming or freeform recoloring. Preferences must stay within the Battle Ops system.

When reduced motion is enabled:

- Remove or simplify non-essential sweep, glow, and pulse effects
- Preserve the underlying tactical appearance

## PrimeNG guidance

PrimeNG components should inherit Battle Ops tokens rather than looking like stock defaults.

- Buttons: cyan border emphasis, glow-hover energy, command-like labels
- Panels: midnight surfaces, steel separators, cyan titles
- Dialogs: squared tactical framing, dark backdrops, cyan outlines
- Tables: steel grid lines, aqua hover, cyan active selection
- Inputs: transparent/dark fields, cyan emphasis, precise focus states

Prefer token-driven styling and shared overrides over one-off component hacks.

## Brand voice

Tone must be tactical, cold, precise.

Prefer phrasing such as:

- `SECTOR SCAN INITIATED`
- `CONTACT DETECTED`
- `TORPEDO IMPACT CONFIRMED`
- `HULL INTEGRITY COMPROMISED`
- `UNAUTHORIZED MOVE`

Buttons and labels should feel command-oriented:

- `ENGAGE`
- `DEPLOY SHIPS`
- `LOCK TRAJECTORY`
- `AWAITING COORDINATES`

## Implementation guardrails

- Use existing Battle Ops tokens before inventing new colors
- Reuse assets from `src\App\public\branding\`
- Keep UX accessible and WCAG AA compliant
- Maintain strong contrast on dark surfaces
- If adding PrimeNG components, ensure they inherit Battle Ops rules immediately
- Keep new UI text aligned with the tactical voice
