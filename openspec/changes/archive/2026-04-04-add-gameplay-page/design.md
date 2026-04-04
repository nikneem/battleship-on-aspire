## Context

GitHub issue `#2` defines the first `/games/{game-code}` player experience, focused on pre-game board setup rather than full turn-based play. The Angular app currently does not expose this route or any fleet-placement interaction model, so the change needs to establish both page structure and the client-side state rules that govern setup.

This change also sits next to `add-games-lobby-lifecycle`, which defines backend-facing game phases and lobby semantics. The gameplay page should therefore be designed to consume game-state information and host local placement interactions without redefining domain rules that belong to the `Games` capability.

## Goals / Non-Goals

**Goals:**
- Define a `/games/{game-code}` route that presents player identity, opponent identity, game state, the setup board, and available ships.
- Define a client interaction model for dragging, snapping, rotating, and repositioning ships on the player's board.
- Define the readiness gate so the `Ready` action appears only when the fleet is fully placed and locks the board once confirmed.
- Keep the UI contract aligned with future game-state APIs rather than embedding hidden gameplay assumptions in the page.

**Non-Goals:**
- Defining backend game creation, joining, or active turn-taking APIs.
- Finalizing realtime synchronization or multiplayer transport behavior.
- Designing advanced touch gestures beyond the basic drag-and-place interaction contract.
- Defining post-ready battle interactions such as firing shots during `InProgress`.

## Decisions

### Use a dedicated gameplay-page capability

The `/games/{game-code}` route and fleet-setup interaction model warrant their own capability instead of being folded into lobby creation or generic game lifecycle specs. This keeps the UI-facing contract testable on its own and avoids mixing route layout concerns with backend state-machine rules.

Alternative considered: extending the broader games-lifecycle change. That would make the spec harder to scan and would blur which requirements belong to frontend interaction versus core game rules.

### Treat fleet placement as local interaction state backed by game-state context

The gameplay page should render the current game state and player/opponent metadata from application state, while ship placement itself is modeled as page-local interaction state until the player confirms readiness. This keeps drag, snap, and rotation behavior responsive without forcing every intermediate move to be treated as a server action.

Alternative considered: persisting every drag or rotation change immediately. That would introduce excessive network chatter and complicate the UX before the player has even committed their setup.

### Model board placement around grid coordinates and orientation

Each ship should be represented by its size, anchor cell, and orientation so snapping and bounds validation can be resolved against a discrete grid. Rotation changes are then expressed as orientation changes around the currently selected ship placement.

Alternative considered: freeform pixel positioning with later normalization. That would make bounds checking, snapping, and overlap detection less deterministic and harder to test.

### Gate the Ready action on complete legal placement

The `Ready` control should stay hidden until every required ship has been placed on valid board cells. Once the player clicks `Ready`, the board transitions to a locked state and no further movement or rotation is allowed.

Alternative considered: always showing a disabled `Ready` button. The issue explicitly describes the button appearing only after all boats are positioned, so visibility gating is the clearer match.

### Use contextual rotation controls for placed ships

Rotation options should appear only when a ship has already been placed and selected in context. This preserves a clean board during drag operations while still allowing repositioned ships to be adjusted after placement.

Alternative considered: showing persistent rotate buttons for every ship in the ship tray. That would not match the issue's context-menu language and would make it less clear which placed ship is being modified.

## Risks / Trade-offs

- [UI spec drifts from backend setup rules] -> Keep this capability focused on presentation and interaction, and rely on `Games` lifecycle capabilities for authoritative readiness and fleet legality.
- [Drag interactions become difficult to test] -> Express requirements in terms of grid outcomes such as snapped placement, bounds enforcement, and lock state instead of raw pointer mechanics.
- [Rotation near edges creates ambiguous placement] -> Require repositioned ships to remain within board bounds after rotation, even if the anchor must be adjusted by the implementation.
- [Subtle attention animation conflicts with reduced-motion preferences] -> Keep the requirement focused on drawing attention, and let implementation adapt the effect under reduced-motion settings.

## Migration Plan

This is a new UI capability, so no migration is required. Implementation can proceed in vertical slices: add the route shell, render the game-state header and empty board, add ship tray and placement logic, add contextual rotation, then add ready-state locking and tests. If rollback is needed, the route and related UI can be removed without affecting persisted game data because setup interactions are new behavior.

## Open Questions

- Should the gameplay page show placeholder opponent state before a second player joins, or is this route only expected after both players exist?
- What exact ship inventory and board dimensions should the UI assume if backend game configuration is not yet exposed?
- Should the context rotation control support both clockwise and counterclockwise rotation, or is a single toggle sufficient?
