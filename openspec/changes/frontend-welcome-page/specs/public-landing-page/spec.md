## ADDED Requirements

### Requirement: Default route displays the public landing page
The system SHALL map the default route (`/`) to a public landing page component located in the frontend project so visitors arrive on a purpose-built entry experience instead of the generated starter content.

#### Scenario: Visitor opens the application root
- **WHEN** a visitor navigates to `/`
- **THEN** the frontend renders the landing page component as the primary page content

### Requirement: Landing page presents a submarine control station experience
The system SHALL render the landing page with a visually immersive submarine control station theme that includes a prominent invitation to play, animated gauges, and rotating radar-style visuals using the application's themed presentation.

#### Scenario: Visitor views the hero section
- **WHEN** the landing page loads
- **THEN** the page shows a prominent hero section inviting the visitor to play
- **AND** the page shows animated instrumentation elements consistent with a submarine control station theme

### Requirement: Landing page rotates tactical terminal messages
The system SHALL maintain a curated array of 50 themed terminal messages and SHALL display them through a terminal-style panel that updates over time to simulate tactical system activity.

#### Scenario: Terminal panel updates activity feed
- **WHEN** the landing page remains open
- **THEN** the terminal-style panel displays messages selected from the 50-message array
- **AND** the displayed message changes over time without requiring a page refresh

### Requirement: Landing page plays ambient sonar audio opportunistically
The system SHALL use the existing `sonar-pulse.mp3` asset to attempt ambient sonar playback at randomized intervals between 10 and 15 seconds while the landing page is active.

#### Scenario: Audio playback is permitted by the browser
- **WHEN** the landing page is active and the browser allows audio playback
- **THEN** the page plays the sonar pulse sound
- **AND** each playback attempt is scheduled with a randomized delay between 10 and 15 seconds

#### Scenario: Audio playback is blocked by the browser
- **WHEN** the browser blocks sound playback before user interaction
- **THEN** the landing page continues to function without breaking layout or interaction

### Requirement: Landing page explains the system mission
The system SHALL include a lower-page information section that explains what the system is, its goal, and its focus on teaching AI-driven development, Aspire, and related techniques.

#### Scenario: Visitor scrolls to the lower information section
- **WHEN** the visitor reaches the bottom portion of the landing page
- **THEN** the page presents the system mission and educational framing in readable content
