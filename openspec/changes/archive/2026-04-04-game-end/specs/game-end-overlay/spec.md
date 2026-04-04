<!-- openspec:github-issue nikneem/battleship-on-aspire#4 -->

> Synced from GitHub issue #4 by manual import.

## ADDED Requirements

### Requirement: Game-end overlay is shown to both players when a game finishes
The system SHALL display a full-screen outcome overlay to each player as soon as the active game transitions to the `Finished` phase. The overlay MUST personalise its message based on whether the viewing player is the winner or the loser.

#### Scenario: Winner sees victory overlay
- **WHEN** the frontend receives a game-state update indicating the game is in the `Finished` phase
- **AND** the current player's identity matches the recorded winner
- **THEN** the outcome overlay is displayed with the message "Winner!"

#### Scenario: Loser sees defeat overlay
- **WHEN** the frontend receives a game-state update indicating the game is in the `Finished` phase
- **AND** the current player's identity does not match the recorded winner
- **THEN** the outcome overlay is displayed with the message "You lost"

#### Scenario: Overlay appears without manual page refresh
- **WHEN** the real-time channel delivers a `Finished` phase update
- **THEN** the outcome overlay becomes visible immediately without requiring the player to reload or navigate

### Requirement: Outcome overlay provides navigation back to main
The overlay SHALL include a single call-to-action button labelled "Back to Main" that navigates the player to the application's main/home page when activated.

#### Scenario: Back to Main button navigates home
- **WHEN** the player activates the "Back to Main" button on the outcome overlay
- **THEN** the application navigates to the home/main page
- **AND** the active game view is no longer visible

#### Scenario: Overlay cannot be dismissed without navigation
- **WHEN** the outcome overlay is visible
- **THEN** the player cannot close or dismiss the overlay without using the "Back to Main" button
- **AND** interaction with the underlying game board is blocked

### Requirement: Outcome overlay uses Battle Ops visual language
The overlay component SHALL use Battle Ops design tokens, typography, and PrimeNG with Aura preset for all visual styling, consistent with the rest of the application.

#### Scenario: Overlay renders with Battle Ops styling
- **WHEN** the outcome overlay is displayed
- **THEN** it uses only colours, fonts, and spacing defined by the Battle Ops design tokens from `src/App/src/styles.scss`
- **AND** it does not introduce new standalone colour values outside the existing token set
