## 1. Game-end overlay component

- [x] 1.1 Generate a standalone Angular component `game-outcome-overlay` in `src/App/src/app/components/` using `ChangeDetectionStrategy.OnPush`
- [x] 1.2 Add an `outcome` input signal accepting `'winner' | 'loser'` to drive the displayed message
- [x] 1.3 Add a `backToMain` output that emits when the "Back to Main" button is clicked
- [x] 1.4 Implement the overlay template: full-screen backdrop, personalised message ("Winner!" or "You lost"), and a "Back to Main" button
- [x] 1.5 Style the overlay using Battle Ops design tokens from `src/App/src/styles.scss` — no new standalone colour values

## 2. Active-game page integration

- [x] 2.1 In the active-game page component, add `signal<boolean>` for overlay visibility and `signal<string | null>` for the winner player ID
- [x] 2.2 Subscribe to the existing real-time game-state update channel; when the received phase equals `Finished`, set the overlay signals accordingly
- [x] 2.3 Compare the winner ID from the game-state DTO against the current player's profile ID to determine `'winner'` or `'loser'` outcome
- [x] 2.4 Render `<app-game-outcome-overlay>` conditionally (using `@if`) when overlay visibility signal is `true`
- [x] 2.5 Block interaction with the underlying game board while the overlay is visible (e.g., pointer-events none on the board container)

## 3. Navigation

- [x] 3.1 Handle the `backToMain` output event in the active-game page and use Angular `Router` to navigate to the home/main route
- [x] 3.2 Verify the home/main route is defined in `src/App/src/app/app.routes.ts` and note its path for navigation

## 4. Validation

- [x] 4.1 Write unit tests for the `game-outcome-overlay` component covering the "Winner!" and "You lost" message rendering and the "Back to Main" button emission
- [x] 4.2 Verify the active-game page test (or add one) that the overlay becomes visible when a `Finished` phase update is processed
- [x] 4.3 Run `npm run build` and `npm run test -- --watch=false` from `src/App` to confirm no regressions
