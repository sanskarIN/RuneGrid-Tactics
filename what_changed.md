# What Changed — RuneGrid Tactics

## v0.1.0 — Playable tactical foundation

**Date:** 2026-08-24

| Area                    | Delivered change                                                                                                                                                                                                                                                                  |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Foundation              | Initialized a React 19, Vite, Tailwind, and Babylon.js browser host with a lifecycle-safe full-screen game canvas.                                                                                                                                                                |
| Design                  | Committed to the Runic Field Manual direction: cartographic basalt board, smoked parchment dossiers, contour frames, DM Serif Display and Manrope typography, and Routeglass Blue player agency.                                                                                  |
| Generated visual assets | Created and wired a tactical visual reference, basalt board texture, original hero token sheet, original enemy token sheet, and transparent compass-rune brand mark.                                                                                                              |
| Tactical engine         | Implemented a data-driven grid model, weighted A\* pathfinding, line-of-sight, range validation, danger highlighting, movement, ability targeting, hazards, healing, teleport markers, undo, phase ownership, and deterministic enemy turns.                                      |
| Original content        | Added original hero, enemy, ability, elemental-interaction, relic, mode, and achievement records; content can grow primarily through configuration.                                                                                                                               |
| Modes and screens       | Implemented a functional campaign route map, seeded expedition, daily and weekly fields, puzzle, survival, boss rush, custom, training, tutorial, endless entry points, roster, codex, statistics, achievements, replay archive, settings, credits, privacy, and support screens. |
| Local-first progression | Added player level, route shards, unlocks, campaign history, achievements, statistics, challenge history, replay metadata, schema versioning, checksum validation, migration, export, import, rolling backup, and recovery.                                                       |
| Accessibility and audio | Added text scale, high contrast, reduced motion/flashing, explicit state legend, handedness preference, confirmation preference, mute, music/effects controls, and original browser-safe procedural audio cues after user interaction.                                            |
| Tests                   | Added Vitest coverage for grid pathfinding and line-of-sight, seeded generation, turn resolution, ability recording, undo, valid import/export, and malformed save rejection.                                                                                                     |
| Build and CI            | Added test commands, TypeScript check, production build validation, and a GitHub Actions workflow.                                                                                                                                                                                |
| Documentation           | Added the requested README, MIT License, architecture, build, testing, contribution, conduct, security, support, privacy, roadmap, release, and changelog documents plus project pipeline context files.                                                                          |
| Fixes                   | Corrected initial Babylon active camera setup, test-compatible enemy scheduling, authoritative undo verification, and tactical-board camera framing.                                                                                                                              |
| Verification            | Confirmed the main command-table menu and deterministic `/?demo` tactical field at 1280×720; confirmed Android-first field controls at 375×812; ran 10 automated tests, TypeScript validation, production build, and Prettier verification successfully.                          |
| Known limitations       | The project is a polished browser foundation rather than a signed native Android package. Later campaign regions remain seedable playable route records pending authored chapter expansion; native packaging and store compliance require a dedicated future release process.     |

## v0.1.0 — Visual review refinement

**Date:** 2026-08-24

| Area              | Delivered change                                                                                                                                                                                                                         |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Visual refinement | Added field-atlas coordinate annotation, ruled map framing, a smoked-parchment-style field-notes marker, and explicit style rules ensuring every first screen combines tactical surface, planning layer, and Routeglass Blue action cue. |
| Validation        | Re-ran the complete formatting, test, type-check, production-build, desktop screenshot, and mobile screenshot sequence after the refinement.                                                                                             |
| Known limitations | The Babylon production bundle is approximately 1.59 MB before gzip and emits a standard chunk-size advisory; it remains buildable and is a candidate for future code-splitting work.                                                     |

## v0.2.0-godot — Native C# migration in progress

**Date:** 2026-08-24

| Area                | Delivered change                                                                                                                                                                                                                                                                           |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Native foundation   | Added a Godot 4 .NET project, main scene, C# project definition, icon, native command-table root, tactical board view, and autoload game services.                                                                                                                                         |
| C# tactical systems | Ported deterministic random generation, JSON content repository, grid model, weighted pathfinding, line-of-sight, seeded encounter factory, legal target validation, movement, tile effects, abilities, turns, enemy AI steps, undo, replay records, local progression, and save recovery. |
| JSON content        | Added heroes, enemies, abilities, items, campaign levels, and balance configuration files under `godot/Data/`.                                                                                                                                                                             |
| Native UI           | Added functional menu, all eleven mode starts, battle HUD, grid tapping, hero/ability command controls, pause, codex, roster, local statistics, replay listing, settings, and export/import control paths.                                                                                 |
| Executables         | Added committed desktop/Android export presets, Bash and PowerShell release scripts, Godot C# CI source validation, and `godot/BUILD_EXECUTABLES.md` with platform-specific toolchain, export, signing, and verification guidance.                                                         |
| Documentation       | Added `godot/README.md` and `GODOT_MIGRATION.md`; updated the root README and changelog to describe the dual browser/native repository layout.                                                                                                                                             |
| Known limitation    | The current sandbox has no Godot .NET editor or .NET SDK. Native source and export configuration are prepared, but executable compilation and on-device verification must run on a provisioned machine or CI with the required official toolchain.                                         |
