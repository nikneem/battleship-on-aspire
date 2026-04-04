## Context

Issue `#7` defines a complete "Battle Ops" tactical identity for the project, including brand assets, palette, typography, logo rules, PrimeNG styling guidance, and tactical microcopy. The current Angular app is still close to its scaffolded state, there is no theme token system, no persisted style settings, and no repository-level skill that tells future agents how to apply the approved style consistently.

This change touches both repository guidance and application behavior. It needs a shared design because the skill, theme tokens, branded assets, and settings must all point to the same source of truth instead of drifting over time.

## Goals / Non-Goals

**Goals:**
- Establish the Battle Ops brand as the default visual system for the Angular application.
- Define a style skill that future agent work can follow when creating or editing UI.
- Introduce theme tokens, typography, logo assets, and PrimeNG styling hooks from a single approved design language.
- Add user-facing style settings that persist supported appearance preferences while preserving the Battle Ops identity.
- Keep the design implementation-friendly for Angular and future PrimeNG adoption.

**Non-Goals:**
- Building every branded screen in the application.
- Defining server-side persistence for style settings in this change.
- Supporting arbitrary user-generated themes outside the Battle Ops system.
- Finalizing optional extras from the issue such as splash screens, board mockups, or loading animations unless they are explicitly selected in follow-up work.

## Decisions

### Use a repository skill as the styling source of truth

The repository will add a dedicated style skill that explains the Battle Ops brand, approved tokens, component behavior, microcopy expectations, and usage restrictions. This keeps future agent-assisted UI work aligned with the approved identity instead of relying on scattered issue text or memory.

**Alternatives considered:**
- Rely only on issue `#7`. Rejected because issue text is not an operational workflow for future implementation tasks.
- Put all guidance only in app documentation. Rejected because style instructions must also influence how agents make changes, not just how humans read them.

### Use design tokens and CSS custom properties as the app theme foundation

The Angular app will express Battle Ops colors, typography, spacing, borders, and motion values as centralized design tokens, exposed through CSS custom properties. This allows application styles, PrimeNG overrides, and asset-specific styles to share one theme foundation.

**Alternatives considered:**
- Hard-code colors and typography per component. Rejected because it would cause drift and make PrimeNG customization harder.
- Use only SCSS variables without runtime CSS variables. Rejected because persisted settings are easier to apply at runtime with custom properties and data attributes.

### Implement style settings as client-side persisted preferences

Style settings will be stored client-side, likely in browser local storage, because they affect presentation and do not need server authority. Settings should be limited to Battle Ops-compatible choices such as reduced motion, density, and supported accent/intensity variants rather than arbitrary theming.

**Alternatives considered:**
- Store settings only in memory. Rejected because user preferences should survive page reloads.
- Persist settings on the server. Rejected because there is no authenticated profile model for long-lived preferences yet and this would unnecessarily expand the scope.

### Treat PrimeNG theming as a governed integration point

PrimeNG component styling will be implemented through an explicit Battle Ops mapping layer so buttons, panels, dialogs, tables, and inputs consistently follow the approved issue guidance. The theme contract should define how PrimeNG components inherit tokens, casing, borders, focus styles, and hover/selection states.

**Alternatives considered:**
- Style only application-owned components and leave PrimeNG defaults in place. Rejected because the issue explicitly includes PrimeNG integration as part of the deliverable.
- Fully fork PrimeNG styling without token mapping. Rejected because it would be brittle and harder to maintain.

### Deliver logo assets as reusable variants with usage rules

The implementation should treat the hero logo, minimal logo, icon/favicons, and monochrome variants as reusable branded assets with documented constraints. The visual assets and the style skill should share the same guidance for sweep angle, proportions, background assumptions, and prohibited modifications.

**Alternatives considered:**
- Generate a single logo and defer the rest. Rejected because the issue explicitly defines multiple variants and usage rules.

## Risks / Trade-offs

- [Theme tokens diverge between skill text and app implementation] -> Define the skill and app theme from the same named token set and keep token names stable.
- [PrimeNG introduces styling complexity or override churn] -> Isolate PrimeNG mappings behind a dedicated theme layer rather than scattering overrides across components.
- [Settings broaden into unsupported custom theming] -> Limit settings to Battle Ops-compatible preferences and explicitly reject arbitrary recoloring.
- [Animated sonar/logo effects hurt accessibility or performance] -> Treat motion as optional and support reduced-motion settings from the start.
- [Brand voice becomes inconsistent across the app] -> Encode approved tactical microcopy patterns inside the style skill and UI requirements.

## Migration Plan

1. Add the style skill with Battle Ops guidance and repository usage rules.
2. Introduce application theme tokens, typography, and branded logo assets.
3. Add the Battle Ops theme layer for core UI and PrimeNG component surfaces.
4. Add persisted style settings and bind them to the theme system.
5. Update app entry surfaces to use the branded theme and settings.

Rollback consists of removing the style skill, reverting the app to scaffold/default styling, and removing local style preference handling.

## Open Questions

- Which supported user settings should ship first: reduced motion only, or reduced motion plus density/accent options?
- Should the initial implementation include PrimeNG package installation, or only the theme contract and integration scaffolding until components are introduced?
- Which screen should become the first branded showcase: the landing screen, menu shell, or future game lobby?
