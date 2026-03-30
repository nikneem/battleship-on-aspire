## Context

The Angular frontend lives in `src\App` and currently uses the generated starter shell. Routing is wired through `src\app\app.routes.ts`, but the route table is empty, so the default route does not yet lead to an intentional page experience. The root shell already uses `RouterOutlet`, PrimeIcons are globally available, and the requested audio asset already exists at `public\audio\sonar-pulse.mp3`.

This change introduces the first real public page in the frontend and establishes the visual tone for the application. Because the page combines routing, animation, ambient audio, generated content, and educational messaging, a small design document helps keep the implementation coherent and browser-safe.

## Goals / Non-Goals

**Goals:**
- Add a standalone landing page under `src\app\pages\public\home\landing-page`.
- Map `/` to that page through the existing Angular router.
- Create a submarine control station presentation with a clear invitation to play.
- Include animated visual elements such as gauges and radar using CSS-first motion.
- Play the existing sonar audio on a randomized 10-15 second interval in a browser-safe way.
- Rotate through a pool of 50 prewritten terminal messages with a tactical flavor.
- Explain the product's educational purpose in the lower part of the page.

**Non-Goals:**
- Implement game setup, authentication, matchmaking, or backend integration.
- Introduce new frontend state management libraries or animation libraries.
- Change non-public routes or create a broader design system for the full application.

## Decisions

### Use a standalone page component and route directly to it
The frontend is already using modern Angular application configuration, so the landing page should be added as a standalone component and referenced directly from `app.routes.ts`. This keeps the route definition simple and matches Angular 21 conventions.

Alternative considered: adding an intermediate feature module or nested route tree. This was rejected because the app is currently minimal and the issue only requires a single public landing route.

### Prefer CSS and template composition for the control-station visuals
Animated gauges, radar sweeps, panel lighting, and terminal presentation should be implemented with semantic markup plus component-scoped SCSS. This keeps the page lightweight, avoids introducing canvas or third-party animation dependencies, and makes the experience easier to maintain.

Alternative considered: implementing the visuals with canvas or SVG-heavy scripting. This was rejected because the desired effects are decorative and can be achieved with lower complexity using CSS transforms, gradients, shadows, and keyframes.

### Isolate timed behaviors inside the landing page and small supporting helpers
The randomized sonar playback and terminal message rotation should be managed by the landing page component, with static message content extracted into a dedicated data file for readability. If audio orchestration becomes noisy, a small focused helper service can be introduced, but the initial implementation should stay close to the page.

Alternative considered: building a broader global audio subsystem up front. This was rejected because only one public page currently needs ambient sound.

### Design audio playback around browser restrictions
Modern browsers may block autoplay with sound until a user interacts with the page. The implementation should attempt playback on the randomized interval, but the page design should tolerate delayed audio activation until the first eligible interaction. The rest of the landing experience must remain fully functional without sound.

Alternative considered: requiring immediate autoplay as a hard guarantee. This was rejected because it is not reliable across browsers and would create a fragile implementation.

### Keep content deterministic but presentation dynamic
The 50 terminal messages should be authored as a fixed array so the tone can be curated, while display order and timing can be randomized or rotated at runtime. This balances themed content quality with a lively page experience.

Alternative considered: generating messages procedurally at runtime. This was rejected because the issue explicitly requests a generated array of 50 messages and curated text is easier to review.

## Risks / Trade-offs

- **[Autoplay restrictions may block sonar audio]** -> Degrade gracefully, start playback only after an allowed interaction, and do not couple core UI behavior to successful audio playback.
- **[Heavy animation can hurt performance on low-powered devices]** -> Use CSS transforms and opacity-based effects, keep concurrent animations limited, and avoid script-driven frame updates where possible.
- **[A dense themed page can become visually noisy]** -> Organize the layout into clearly separated panels with a strong hero call-to-action and restrained color usage based on the existing theme palette.
- **[Large inline content in the component can become hard to maintain]** -> Move terminal message data into a dedicated file and keep helper logic small and named around page behaviors.

## Migration Plan

1. Create the standalone landing page component and its template/styles under `src\app\pages\public\home\landing-page`.
2. Add a default route in `src\app\app.routes.ts` that maps `''` to the landing page component.
3. Add curated terminal message data and page-local timed behavior for message rotation and sonar intervals.
4. Validate that the page renders as the app entry point and that the experience remains usable even when audio autoplay is blocked.
5. If regressions occur, revert the route mapping and remove the new landing page files to restore the starter shell.

## Open Questions

- None at proposal time; browser handling for audio is an implementation concern rather than a blocker.
