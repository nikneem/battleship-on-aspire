## 1. Frontend page scaffolding

- [x] 1.1 Create the standalone landing page component under `src\App\src\app\pages\public\home\landing-page` with separate template and SCSS files.
- [x] 1.2 Replace the default application route in `src\App\src\app\app.routes.ts` so `/` renders the landing page component.

## 2. Landing page experience

- [x] 2.1 Implement the landing page layout with a hero section, submarine control station panels, animated gauges, and rotating radar visuals.
- [x] 2.2 Add the lower-page mission section describing the system goal and its focus on AI-driven development, Aspire, and related techniques.

## 3. Dynamic behavior and content

- [x] 3.1 Add a dedicated data source with 50 curated terminal messages for the tactical activity panel.
- [x] 3.2 Implement timed terminal message rotation and randomized sonar playback attempts using `public\audio\sonar-pulse.mp3`, ensuring the page remains usable when browser audio autoplay is blocked.

## 4. Verification

- [x] 4.1 Validate that the application root renders the landing page instead of the starter content.
- [x] 4.2 Verify the animated UI, terminal updates, and sonar timing behavior work as intended without introducing build or test regressions in the Angular app.
