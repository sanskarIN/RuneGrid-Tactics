# Game Plan: RuneGrid Tactics

RuneGrid Tactics is delivered as an original, Android-first browser tactical roguelite. The production target is a self-contained Babylon.js field board with local-first progression and a responsive HTML command layer. The core release is designed around a fully playable campaign skirmish, deterministic expeditions, tactical training, and data-driven extension points for the broader roadmap in the uploaded specification.

## Risk Tasks

### 1. Deterministic seeded encounter generation

- **Why isolated:** Small differences in random-number use can make a seed unreplayable, invalidate action replays, and silently corrupt challenge history.
- **Approach:** Centralize all random choices in a seeded Mulberry32 generator. Map, enemies, hazards, rewards, and encounter labels are generated from a versioned seed and a pure configuration object. The generated payload is serializable and replay metadata records the seed, version, and action list.
- **Verify:** Repeating the same seed and difficulty produces the same grid layout, tile types, unit IDs, enemy roster, objective, and reward data. A replay can rebuild the starting encounter before applying recorded actions.

### 2. Tile selection, pathfinding, and tactical previews

- **Why isolated:** Pointer picking, blocked paths, difficult terrain, range checks, and line-of-sight can conflict in a board renderer and produce misleading valid-target highlights.
- **Approach:** Keep grid calculations in `GridModel` with no Babylon imports. Use weighted A\* for movement, Manhattan range for areas, Bresenham-style line tracing for line-of-sight, and a single selector state that the renderer reads to update tile materials. The presentation layer never decides validity.
- **Verify:** Selecting a hero exposes only reachable tiles within available movement; blocked tiles cannot be selected; difficult terrain consumes extra points; line attacks stop behind obstacles; attack previews agree with final combat resolution.

### 3. Turn handoff between player input and AI

- **Why isolated:** Input can race with turn changes, and AI scheduling can leave the board locked or allow player actions while enemies resolve.
- **Approach:** `GameSession` owns an explicit `phase` state machine (`player`, `resolving`, `enemy`, `victory`, `defeat`). The scene supplies `setTimeout` only through a disposable scheduler. Enemy decisions are deterministic, one unit at a time, and every state change notifies the UI and renderer.
- **Verify:** After a player action, controls lock during resolution; each living enemy takes at most one action per enemy phase; the player cannot act while enemy movement resolves; victory or defeat cleanly stops further input.

### 4. Local-save schema evolution and recovery

- **Why isolated:** Versioned saves often fail at import boundaries or leave the player without a recoverable state after an invalid payload.
- **Approach:** Save a schema-versioned envelope with a JSON checksum, validation, migration functions, rolling backup, import/export, and a safe default recovery path. No network service is needed for core play.
- **Verify:** A valid export re-imports correctly; malformed input is rejected without overwriting an existing save; a legacy v1 envelope is migrated to the current version; if the primary save fails validation, a valid backup is restored.

## Main Build

The main build contains an illustrated tactical board, original heroes and enemy archetypes, movement and ability turns, elemental tags, hazards, teleport tiles, destructible obstacles, seedable expeditions, basic campaign progression, achievement tracking, statistics, local saves, replay metadata, accessibility settings, an encyclopedia, settings, credits, support links, and complete project documentation.

- **Assets needed:** A 16:9 tactical board visual reference; a square basalt board texture; hero token sheet; enemy token sheet; transparent compass-rune brand mark. Generated assets are used for the menu context, board material, visual vocabulary, and header mark. Units use procedural tabletop figures for clear interaction at all device sizes.
- **Verify:**
  - A pointer selection maps to the correct board coordinate and visible valid state.
  - A player can select a hero, move it, use an ability, end a turn, observe AI resolution, and reach a win or loss state.
  - Generated expedition seeds recreate encounters exactly for the current release version.
  - Menu actions have a working destination or an explanatory, playable in-app implementation; there are no inert controls.
  - Saved settings, campaign completion, statistics, unlocked achievements, and last seed persist after reload.
  - High-contrast, text-size, reduced-motion, color-independent tile state, and keyboard input work without losing core context.
  - UI is readable at mobile 375px and desktop 1280px widths without overlap.
  - There are no missing generated asset URLs, visual placeholder artifacts, browser-console errors, or TypeScript errors during capture.
  - The implementation stays aligned to the reference: 3/4 tactical grid, basalt and parchment palette, cyan player routes, information-rich field dossier, and restrained tabletop density.
