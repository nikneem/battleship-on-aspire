## Why

The application currently lacks a memorable public entry point that immediately communicates the Battleship theme and the project's educational purpose. Adding a polished landing page now creates a stronger first impression, gives the frontend a clear default route, and frames the app as a showcase for AI-driven development with Aspire.

## What Changes

- Add a new public landing page component under `src/app/pages/public/home/landing-page`.
- Route the default path (`/`) to the new landing page.
- Design the page as an immersive submarine control station experience with a hero/jumbo section, animated gauges, and rotating radar visuals.
- Add ambient sonar audio playback using the existing `sonar-pulse.mp3` asset at randomized 10-15 second intervals.
- Add a terminal-style activity panel that rotates through a generated pool of 50 themed tactical messages.
- Add a footer/system information section that explains the application's goal of teaching AI-driven development, Aspire, and related techniques.

## Capabilities

### New Capabilities
- `public-landing-page`: Provide an animated, thematic landing experience on the default route that invites users to play and explains the system's purpose.

### Modified Capabilities
None.

## Impact

- Frontend routing and public page composition in `src`.
- Frontend assets and UI behavior for animation, timed audio playback, and rotating terminal messages.
- No backend or API contract changes are expected.
