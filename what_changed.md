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
| Verification | Built the Godot C# project in Release with zero warnings/errors and ran the complete native suite successfully: **24 passed, 0 failed**. |
| GitHub | Pushed five focused inspector model, command-table UI, test, validation, and documentation commits, ending at `d44c124`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.5-scrubbing — Replay inspector timeline navigation

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Deterministic seek | Added `Seek(actionIndex)`, `StepBackward`, and `StepForward` to rebuild replay state from the original seed at any valid action position. |
| Command table | Added a discrete timeline slider, previous/next controls, action-position labels, and clickable action rows that scrub to the state after the selected action. |
| State safety | Invalid indices leave the visible inspector state untouched; a seek that reaches a malformed event surfaces that replay’s existing deterministic error. |
| Verification | Added direct seek equality, previous/next fingerprint stability, and bounds preservation tests. |
| Native validation | Added seek methods, scrub controls, and navigation scenario checks to the repository validator. |
| Verification | Built the Godot C# project in Release with zero warnings/errors and ran the complete native suite successfully: **27 passed, 0 failed**. |
| GitHub | Pushed five focused seeking, command-table UI, test, validation, and documentation commits, ending at `ae4abd1`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.6-replay-shortcuts — Replay inspector keyboard navigation

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Keyboard controls | Added unmodified **Left Arrow** for previous, **Right Arrow** or **Space** for next, **Home** for the opening state, **End** for the final state, and **P** for play-to-end. |
| Input safety | The Godot key handler ignores modified shortcuts, key-repeat events, and active text-entry controls; the timeline slider does not capture keyboard focus. |
| Command table | Added a persistent in-inspector key legend and a timeline tooltip that identifies the available keyboard commands. |
| Core contract | Added framework-independent `ReplayInspectorShortcutMap` so keyboard mapping stays unit-testable without a Godot renderer. |
| Verification | Added accepted-key and rejected modified/unknown-key coverage; the native validator now requires the shortcut model, early key route, visible hint, and named scenario. Godot C# Release builds with zero warnings/errors and the native suite passes **28 tests, 0 failures**. |
| GitHub | Pushed four focused keyboard navigation, test, validation, and documentation commits ending at `e93dd34`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.7-configurable-replay-keys — Persistent replay key settings

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Local bindings | Added a save-record `ReplayInspectorKeyBindings` model with distinct defaults for previous, next, opening, final, and play-to-end commands. |
| Settings panel | Added a **REPLAY INSPECTOR KEYS** section to native settings. Players select a command, capture one supported unmodified key, receive clear notices, can cancel with Escape, and can restore all defaults. |
| Safety | Assignments reject duplicates and unsupported keys. Existing or imported records normalize missing, malformed, or colliding values before replay input is routed. |
| Inspector | The key legend and timeline tooltip now reflect the player’s active saved bindings rather than static labels. |
| Verification | Added framework-independent tests for custom assignment serialization/resolution, conflict rejection, malformed import repair, and reset behavior; validation now requires the binding model and named scenarios. Godot C# Release builds with zero warnings/errors and the native suite passes **30 tests, 0 failures**. |
| GitHub | Pushed four focused binding, test, validation, and documentation commits ending at `54a7a19`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.8-replay-shortcut-reference — In-game active-binding overlay

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Overlay access | Added **KEYS · F1** to the replay command table and an unmodified **F1** key route while inspecting a replay. |
| Active reference | Added a modal **SHORTCUT REFERENCE** that lists the user’s current saved binding, command name, and deterministic effect for every replay action. |
| Input safety | The overlay consumes keyboard events so viewing help cannot scrub or play the replay. It closes by F1, Escape, or an explicit close command without changing replay state. |
| Core contract | Added a framework-independent reference builder in fixed replay-command order so active local bindings can be tested independently of Godot rendering. |
| Verification | Added deterministic reference ordering and custom-binding coverage; native validation now requires the overlay builder, F1 route, command trigger, and close behavior. Godot C# Release builds with zero warnings/errors and the native suite passes **31 tests, 0 failures**. |
| GitHub | Pushed four focused overlay, test, validation, and documentation commits ending at `d462261`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.9-replay-onboarding — First-time replay inspector introduction

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Local state | Added a persistent `ReplayInspectorOnboarding` record to local accessibility settings. Existing and imported records receive a safe not-yet-seen default. |
| First-use tooltip | The first opened replay displays **INTRODUCING REPLAY INSPECTOR**, explaining deterministic state reconstruction and how to access active controls through **VIEW SHORTCUTS** or **F1**. |
| Dismissal | **GOT IT** and **VIEW SHORTCUTS** both persist acknowledgement. The former returns directly to inspection; the latter opens the existing active-binding reference overlay. |
| Verification | Added framework-independent serialized onboarding lifecycle coverage; native validation now requires the state contract, tooltip, acknowledgement, and shortcut-reference entry route. Godot C# Release builds with zero warnings/errors and the native suite passes **32 tests, 0 failures**. |
| GitHub | Pushed four focused onboarding, test, validation, and documentation commits ending at `73ae81a`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.10-replay-mismatch-warnings — Contextual determinism guidance

**Date:** 2026-08-24

| Area | Delivered change |
| --- | --- |
| Detection | The inspector now derives a mismatch signature from current action index plus expected/current fingerprints whenever the determinism audit detects a divergence. |
| Contextual warning | Added a non-blocking **REPLAY DETERMINISM WARNING** tooltip with action position, the first human-readable difference, a pointer to **DETERMINISM AUDIT**, and a **VIEW SHORTCUTS** path. |
| Acknowledgement | Dismissing a warning persists only the exact mismatch signature. A later changed action or fingerprint divergence still produces a new warning. |
| Interaction safety | The tooltip uses pass-through input so it does not halt timeline controls; mismatch warnings take priority over first-time onboarding guidance. |
| Verification | Added framework-independent mismatch-signature persistence coverage; native validation now requires warning state, summary contract, tooltip, audit guidance, acknowledgement, and shortcut-reference path. Godot C# Release builds with zero warnings/errors and the native suite passes **33 tests, 0 failures**. |
| GitHub | Pushed four focused warning, test, validation, and documentation commits ending at `85f19de`, with repository-local author address `sanskarin@outlook.in`. |

## v0.3.11-filtered-replay-diffs — Focused mismatch inspection

**Date:** 2026-08-26

| Area | Delivered change |
| --- | --- |
| Structured filter | Added framework-independent classification for replay differences: **All**, **Phase / state**, **Tile**, **Unit**, and **Action**. |
| Affected entities | Tile entries parse canonical `x:y` coordinates and unit entries retain stable unit IDs, producing exact marker sets for the selected filter. |
| Inspector controls | Added a category button group with per-category counts under **FILTERED DIFF · AFFECTED BOARD MARKERS**. The All filter retains the full human-readable audit. |
| Board clarity | Selected mismatched tiles gain a warm inner border. Selected mismatched living units gain a delta ring and marker, without changing tactical selection or replay state. |
| Verification | Added category isolation, marker extraction, and human-readable filtered output coverage; validator now requires the diff model, inspector route, and board markers. Godot C# Release builds with zero warnings/errors and the native suite passes **34 tests, 0 failures**. |
| GitHub | Pushed four focused filtered-audit, test, validation, and documentation commits ending at `92638b7`, with repository-local author address `sanskarin@outlook.in`. |
