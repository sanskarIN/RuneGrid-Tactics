# RuneGrid Tactics

> **Mark a route. Own the turn.**

RuneGrid Tactics is an original, open-source, local-first tactical roguelite created by **Sanskar**. It presents a browser-deployable, Android-first command-table experience built around meaningful positioning, deterministic seeded fields, procedural encounters, elemental effects, accessibility-first controls, and locally stored progression.

This project is not affiliated with or derived from any commercial game. Its heroes, enemies, tactical systems, fiction, art direction, UI language, and generated assets have been created for RuneGrid Tactics.

## Current playable release

The current release implements a complete browser gameplay loop: open a mode, inspect a deterministic field briefing, select a hero, mark movement, choose a valid ability target, resolve enemy turns, finish the field, collect local progression, and retain a replay record. The UI also includes working campaign, expedition, daily, weekly, puzzle, survival, boss, custom, training, tutorial, and endless-field entry points.

| Area                | Included in v0.1.0                                                                                                                                                                          |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Tactical board      | Weighted movement, obstacles, difficult terrain, healing/hazard/teleport tiles, line-of-sight, target previews, danger previews, undo before turn end, and deterministic action resolution. |
| Content             | Original Vanguard, Rune Mage, Ranger, and Guardian records; five original enemy profiles; data-driven abilities, passive/ultimate hooks, and elemental interaction ledger.                  |
| Modes               | Campaign, expedition, daily, weekly, puzzle, survival, boss rush, custom, training, tutorial, and endless encounter entry points.                                                           |
| Local-first systems | Versioned saves, checksum validation, rolling backup, export/import, migration path, statistics, achievements, unlocks, challenge history, and replay metadata.                             |
| Accessibility       | Text scaling, high contrast, reduced motion/flashing, color-independent legend, audio mute, volume controls, handedness preference, and confirmation preference.                            |
| Quality             | Unit, generation, tactical-session, and save regression tests; TypeScript validation; production build; GitHub Actions CI.                                                                  |

## Playing locally

The project uses React, TypeScript, Vite, Babylon.js, and a browser-local save envelope. A current Node.js LTS environment and pnpm are recommended.

```bash
pnpm install
pnpm dev
```

Open the local address printed by Vite. For a deterministic visual proof of the tactical table, open `/?demo`; it loads the training field with a fixed seed. Refer to [BUILDING.md](BUILDING.md) for checks, build output, and deployment preparation.

## Controls

| Input                                    | Field action                                  |
| ---------------------------------------- | --------------------------------------------- |
| Tap/click a friendly token or squad card | Select the hero and show reachable routes.    |
| Tap/click a highlighted route tile       | Move the selected hero if the route is valid. |
| Tap/click an ability in the command bar  | Enter target-preview mode for that ability.   |
| Tap/click a highlighted target           | Resolve the ability and record the action.    |
| **Enter**                                | End the current player turn.                  |
| **Z**                                    | Undo the latest permitted player action.      |

The field layer uses redundant cues. Cyan tile overlays mean reachable routes, rust-red tile overlays mean valid targets, and muted red field overlays mean enemy threat range. The on-screen legend repeats these meanings for players who do not distinguish the colors.

## Architecture

React provides only the full-screen lifecycle frame. Babylon.js owns the canvas, tactical board meshes, camera, light, material, and pointer picking. Framework-independent TypeScript modules own game rules, generation, AI, persistence, progression, replay records, and the DOM command layer.

```text
GameCanvas → Babylon scene → TacticalRenderer
                     ↘ GameSession → GridModel / content / replay
GameUIController → local commands → SaveManager / ProgressionService
```

The full ownership model and extension guidance are recorded in [ARCHITECTURE.md](ARCHITECTURE.md), while live pipeline details are maintained in [STRUCTURE.md](STRUCTURE.md), [PLAN.md](PLAN.md), [MEMORY.md](MEMORY.md), and [ASSETS.md](ASSETS.md).

## Local-first data and privacy

Core game data remains in browser local storage by default. The save envelope includes player progression, settings, achievements, statistics, challenge history, replays, and the current encounter snapshot. It includes a schema version, checksum validation, a rolling local backup, an export path, and an import validator. No account is required for the core game loop.

> Exported field records are initiated by the player. They should be treated as personal game data and shared only at the player’s discretion.

Read the data handling statement in [PRIVACY.md](PRIVACY.md). Report a security concern using the private process in [SECURITY.md](SECURITY.md).

## Contribution and community

Contributions are welcome, especially new data-driven heroes, encounter templates, accessibility checks, test cases, and localization-ready content. Please read [CONTRIBUTING.md](CONTRIBUTING.md), [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md), and [TESTING.md](TESTING.md) before opening a pull request.

## Support

Development support is optional, never blocks gameplay, and is never represented as a purchase.

> [Support development on Buy Me a Coffee](https://buymeacoffee.com/sanskarIN)

For player support, contact **supportramsandesh@gmail.com**. Project-related business contact addresses are **sanskarin@outlook.in** and **sanskarin.business@gmail.com**.

## License

RuneGrid Tactics is released under the [MIT License](LICENSE). “Made by the Sanskar” remains the project’s creator credit and watermark.
