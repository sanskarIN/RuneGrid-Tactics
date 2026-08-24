# RuneGrid Tactics — Architecture

## Ownership Model

The project follows the game-dev layering contract:

```text
React GameCanvas = lifecycle-safe full-screen picture frame
Babylon scene + TacticalRenderer = board, meshes, camera, interaction picking
GameSession + domain modules = deterministic game rules and run state
GameUIController = DOM command interface, menus, a11y controls, input translation
LocalSaveManager = local-first persistence, migration, backup, import/export
```

Gameplay modules never import React. `GameSession` is responsible for legal actions and turn state; `TacticalRenderer` is responsible for turning the current state into Babylon meshes; `GameUIController` only requests actions and renders the resulting view model.

## Core Modules

| Module                | Responsibility                                             | Babylon dependency |
| --------------------- | ---------------------------------------------------------- | ------------------ |
| `types.ts`            | Shared tactical, progression, and save contracts           | None               |
| `rng.ts`              | Versioned seeded pseudo-random generator                   | None               |
| `GridModel.ts`        | Tiles, weighted pathfinding, ranges, line-of-sight         | None               |
| `content.ts`          | Data-driven hero, ability, enemy, relic, codex content     | None               |
| `GameSession.ts`      | Turn state machine, combat, ability resolution, AI, events | None               |
| `Progression.ts`      | Statistics, campaign unlocks, achievements, mastery        | None               |
| `SaveManager.ts`      | Versioned local persistence, migration, export/import      | None               |
| `Replay.ts`           | Deterministic action history and replay metadata           | None               |
| `TacticalRenderer.ts` | Grid, units, overlays, camera, Babylon scene ownership     | Babylon            |
| `GameUIController.ts` | Responsive menus, HUD, a11y affordances, event bindings    | DOM                |
| `scene.ts`            | Creates, connects, and disposes the scene graph            | Babylon + DOM      |

## State Machine

```text
main-menu → briefing → player → resolving → enemy → player
                                      ↘ victory
                                      ↘ defeat
```

Every player command passes through validation. Every state transition emits a structured event to the renderer and command UI, preserving a single source of truth for game rules.

## Data-Driven Extension Points

New heroes, enemies, abilities, campaign encounters, hazards, relics, and achievements are authored in `content.ts` and configuration-shaped objects rather than embedded in rendering branches. Rendering maps known unit factions and elements to material cues, so a content addition does not require reworking rules code.

## Local-First Contract

The app writes a schema-versioned save envelope to browser local storage. The envelope includes settings, progression, statistics, achievements, challenge history, and the last active encounter. Validation occurs before writes and imports; an independent rolling backup supports recovery. Exports contain no personal data by default.
