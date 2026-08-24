# What Changed — RuneGrid Tactics Native Edition

## v0.2.1-godot — Godot C#–only conversion

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Repository scope | Converted the repository into a single Godot 4 .NET C# game project. The retired non-native client stack, build tools, dependencies, and workflow have been removed. |
| Project root | Moved `project.godot`, `RuneGrid.Tactics.csproj`, scene entry point, C# scripts, JSON content, release tools, and export presets to the repository root. |
| C# tactical engine | Retained the native deterministic encounter factory, weighted pathfinding, line-of-sight, turn ownership, abilities, tile effects, enemy decisions, undo, local saves, progression, and replay records. |
| Native UI | Retained the Godot command-table root, interactive board drawing, mode library, field HUD, pause, codex, roster, statistics, replays, settings, export, and import pathways. |
| Data | Retained JSON-authored heroes, enemies, abilities, items, levels, and balance configuration in `Data/`. |
| Executables | Kept Windows, Linux, and Android export presets plus Bash and PowerShell native release scripts. Updated build instructions to use the repository root. |
| Automation | Replaced the previous automation with the Godot .NET workflow that restores and builds `RuneGrid.Tactics.csproj` and validates native project records. |
| Documentation | Rewrote README, architecture, privacy, security, contribution, release, roadmap, and changelog documentation for the Godot C#–only product. |
| Validation | Native structure and six JSON files validate successfully via `node Tools/validate-project.mjs .`; whitespace validation also passes. |
| GitHub | Pushed the complete native-only conversion in commit `06a0e4a` using the repository-local author address `sanskarin@outlook.in`. |
| Remaining release gate | Use a machine with Godot 4 .NET and the matching .NET SDK to import the project, compile C#, create platform packages, and run desktop/mobile smoke tests. |

## v0.3.0-tactics — Tactical classes and advanced navigation

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Hero classes | Added Duelist, Runesmith, Seer, and Skywarden heroes with distinct combat posture, mobility, JSON class metadata, and authored skills. |
| Enemy classes | Added Iron Sentinel, Gale Harrier, Cinder Artillery, and Shade Stalker roles and included them in deterministic encounter pools. |
| Navigation | Added mobility-specific movement costs, threat and hazard weighting, cover rewards, reservations, tactical route diagnostics, legal approach routing, and flank-anchor analysis. |
| Turn integration | Hero movement uses tactical routes; enemy movement now resolves legal approach tiles and uses safe or flanking intent according to role. |
| Native UI | Added cover, high-ground, flank-anchor, and suggested-route visuals to the board plus a `RESERVE ROUTE` command. |
| Roster persistence | Existing and new local profiles receive the expanded native hero roster through a progression migration helper. |
| Validation | The repository validator now checks expanded units, class metadata, ability links, route APIs, export presets, and JSON content. |
| GitHub | Pushed four focused commits for navigation, roster roles, validation, and documentation, ending at `70a9294`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.1-tests — Comprehensive native pathfinding tests

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Test project | Added `Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj`, a standalone .NET 8 xUnit project that links only framework-independent C# tactical contracts. |
| Test coverage | Added twelve deterministic unit tests for movement profiles, hazards, difficult terrain, phasewalker wall traversal safety, safe and fast route intents, reservations, occupied-target approaches, flank anchors, route diagnostics, high ground, cover, and living-unit reservation filtering. |
| Build boundary | Excluded `Tests/**/*.cs` from the Godot runtime assembly so the game and test runner compile independently. |
| CI | Extended the Godot .NET workflow to restore and execute the pathfinding test project after the native C# Release build. |
| Documentation | Added `TESTING.md` and updated the README and executable build guide with exact native test commands and coverage expectations. |
| Verification | Installed local .NET SDK 8.0.424, built `RuneGrid.Tactics.csproj` in Release with zero warnings/errors, and ran the test suite successfully: **12 passed, 0 failed**. |
| GitHub | Pushed four focused test, runtime-boundary, CI, and documentation commits, ending at `4315590`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.2-replays — Saved encounter replay determinism

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Canonical state | Added `ReplayFingerprint`, which hashes a stable ordering of encounter identity, tiles, unit state, cooldowns, statuses, reservations, and authoritative actions. |
| Playback integrity | Refactored `ReplayPlayer` around a deterministic encounter builder and validates turn, action type, actor, target, ability, and produced event before advancing. |
| Action recording | Added explicit authoritative `end-turn` replay actions so player phase transitions are replayable without timing inference. |
| Test coverage | Added saved replay JSON round-trip, independent playback, reset, malformed action order, and changed-seed fingerprint scenarios. |
| Native validation | Added replay source contracts and required determinism scenario checks to the repository validator. |
| Verification | Built the Godot C# project in Release with zero warnings/errors and ran the complete native suite successfully: **17 passed, 0 failed**. |
| GitHub | Pushed four focused replay engine, test, validation, and documentation commits, ending at `7c75256`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.3-replay-diffs — Human-readable replay mismatch diagnostics

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Canonical snapshot | Added `ReplayStateSnapshot` to preserve a stable textual representation of encounter identity, phase, turn, tiles, units, cooldowns, statuses, reservations, and actions. |
| Diff generator | Added `ReplayStateDiffGenerator`, which reports deterministic expected-versus-actual scalar, tile, unit, and action differences and formats them for assertion output. |
| Fingerprints | Refactored `ReplayFingerprint` to use the shared canonical snapshot rather than duplicating serialization rules. |
| Test ergonomics | Saved-replay equivalence now attaches a human-readable state diff to the assertion message; dedicated tests cover exact match, phase mismatch, and combined entity/action mismatch output. |
| Native validation | The repository validator now requires the snapshot/diff contracts and their diagnostic test scenarios. |
| Verification | Built the Godot C# project in Release with zero warnings/errors and ran the complete native suite successfully: **20 passed, 0 failed**. |
| GitHub | Pushed four focused replay diff engine, test, validation, and documentation commits, ending at `156ef89`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.4-inspector — Command-table replay inspector

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Inspector model | Added `ReplayInspector`, which tracks archived record progress, expected reconstruction, initial state, current state, fingerprint audit, action timeline, reset, single-step, and end-playback behavior. |
| Command table | Added **INSPECT** controls in the route archive and a replay inspector screen with tactical board reconstruction, fingerprints, deterministic audit, opening-state delta, error reporting, and color-coded timeline. |
| Service integration | Added a native `GameServices.InspectReplay` entry point that uses authored content and the existing seeded encounter factory. |
| Verification | Added tests for inspector initial report, stepping, completion/reset, and invalid replay rendering. |
| Native validation | The repository validator now requires inspector source, command-table controls, and inspector test coverage. |
