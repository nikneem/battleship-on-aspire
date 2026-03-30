## ADDED Requirements

### Requirement: Application uses the Battle Ops theme system
The application SHALL expose a Battle Ops theme system that applies the approved deep-ocean tactical palette, IBM Plex Mono typography, and branded visual tokens as the default presentation layer for the UI.

#### Scenario: Application loads with default styling
- **WHEN** a user opens the application without any prior style preferences
- **THEN** the UI renders with the Battle Ops color palette, typography, and default branded styling
- **AND** the presentation uses dark tactical surfaces rather than scaffold defaults

### Requirement: Battle Ops logo assets are available in supported variants
The application MUST provide the approved Battle Ops logo variants needed for hero, minimal, icon, and monochrome usage, and each variant MUST follow the approved construction and background rules.

#### Scenario: UI surface needs a compact brand mark
- **WHEN** a screen or browser surface requires a compact or icon-sized logo
- **THEN** the application uses the supported minimal or icon variant
- **AND** the asset remains consistent with the approved sonar, compass, and sweep identity

### Requirement: PrimeNG components adopt Battle Ops styling rules
The application SHALL define Battle Ops styling behavior for PrimeNG-integrated components, including buttons, panels, dialogs, tables, and inputs, so those components inherit the approved borders, casing, hover states, focus states, and tactical emphasis.

#### Scenario: PrimeNG component is rendered in the application
- **WHEN** a supported PrimeNG component appears in the UI
- **THEN** it inherits the Battle Ops visual rules for its component class
- **AND** it does not fall back to unrelated default component styling

### Requirement: Style settings persist supported presentation preferences
The application SHALL allow users to change supported style settings that remain compatible with the Battle Ops identity, and those settings SHALL persist across browser reloads on the same device.

#### Scenario: User changes a supported style preference
- **WHEN** a user updates a supported style setting such as reduced motion or another approved Battle Ops-compatible preference
- **THEN** the application applies the updated setting
- **AND** the same setting is restored on the next visit in the same browser

### Requirement: Reduced-motion users can keep the Battle Ops theme
The application MUST support a reduced-motion presentation mode that preserves the Battle Ops identity while suppressing or simplifying non-essential animated effects such as sonar sweeps or glow pulses.

#### Scenario: User enables reduced motion
- **WHEN** reduced motion is enabled through supported style settings
- **THEN** non-essential theme animations are removed or simplified
- **AND** the application still presents a recognizably Battle Ops-branded interface
