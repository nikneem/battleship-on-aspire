## ADDED Requirements

### Requirement: Battle Ops style skill defines approved visual rules
The repository SHALL provide a style skill that translates the approved Battle Ops brand into operational guidance for future UI work, including palette, typography, logo variants, component styling expectations, and prohibited usage.

#### Scenario: Agent needs style guidance for a new UI change
- **WHEN** an agent or contributor uses the style skill during a UI task
- **THEN** the skill provides the approved Battle Ops color, typography, logo, PrimeNG, and microcopy guidance
- **AND** the guidance is specific enough to apply consistently without rereading issue `#7`

### Requirement: Style skill preserves brand constraints
The style skill MUST instruct future work to preserve Battle Ops proportions, dark-background usage, logo construction rules, and prohibited modifications such as stretching, recoloring, or shadowing the logo.

#### Scenario: Proposed logo usage violates brand rules
- **WHEN** a UI change attempts to stretch the logo, place it on a busy background, or recolor it outside approved variants
- **THEN** the style skill flags that usage as invalid
- **AND** the change is redirected to an approved Battle Ops presentation

### Requirement: Style skill defines tactical voice and microcopy
The style skill SHALL define the approved Battle Ops voice so UI labels, alerts, and helper text use the tactical, cold, and precise tone from the approved branding issue.

#### Scenario: UI copy is generated for an alert or action
- **WHEN** a contributor creates or edits branded UI copy with the style skill
- **THEN** the resulting copy follows the approved tactical voice
- **AND** it prefers Battle Ops-aligned phrasing such as command-style labels and tactical status messages
