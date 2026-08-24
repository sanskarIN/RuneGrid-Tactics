# Architecture Guide

RuneGrid Tactics separates rendering, field rules, local persistence, and command UI so the tactical model can grow without coupling every feature to the browser canvas. The principal rule is simple: **gameplay state owns decisions; the renderer and UI only display and request them.**

| Layer             | Main modules                                                       | Responsibility                                                                                                                                      |
| ----------------- | ------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| Lifecycle frame   | `GameCanvas.tsx`                                                   | Creates one Babylon engine, handles resizing, disposes the engine and listeners, and hosts the DOM command layer.                                   |
| Scene integration | `scene.ts`, `TacticalRenderer.ts`                                  | Creates the Babylon scene, camera, lights, board meshes, physical tokens, material cues, and pointer-to-tile translation.                           |
| Tactical domain   | `GameSession.ts`, `GridModel.ts`, `types.ts`                       | Owns phase transitions, valid actions, movement, ability resolution, damage, tile effects, AI turns, undo, and replay actions.                      |
| Content domain    | `content.ts`, `rng.ts`                                             | Defines original characters and ability records, seeded encounter generation, mode metadata, elemental notes, and achievement definitions.          |
| Player services   | `Progression.ts`, `Replay.ts`, `SaveManager.ts`, `AudioManager.ts` | Tracks local progression, maintains compact replay records, validates storage, recovers backups, and provides original browser-safe feedback tones. |
| Command interface | `GameUIController.ts`                                              | Renders screens, accessibility settings, field cards, codex, local save controls, and battle commands without importing React.                      |

## State boundaries

The board advances through explicit states: `briefing → player → enemy → player`, or `briefing/player/enemy → victory|defeat`. The UI cannot mutate a unit directly. It selects an actor or an ability and requests `chooseTile`; `GameSession` validates the state, calculates legality, records the action, and emits an event. The renderer then synchronizes to that legal state.

## Data extension pattern

Hero and enemy records are plain data objects. Adding a new hero normally means adding a `UnitTemplate`, its ability definitions, content references, and visual color identity. The renderer does not use a subclass hierarchy per hero. This keeps future roster expansion, cosmetics, equipment metadata, and localization keys practical.

## Persistence contract

`SaveManager` writes a versioned envelope with a checksum. Before overwriting a primary save, it copies the previous valid value into a backup key. Imports are parsed, migrated if necessary, and verified before becoming the primary save. If a primary record fails validation at startup, the backup is tried; otherwise a clean local profile is created.

Consult [STRUCTURE.md](STRUCTURE.md) for the resumable implementation map and [TESTING.md](TESTING.md) for regression coverage.
