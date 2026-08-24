# Changelog

All notable native RuneGrid Tactics changes are recorded here.

## [0.3.4-inspector] — 2026-08-24

### Added

Added a command-table replay inspector for archived records. Players can select **INSPECT** from the route archive, step through actions, play to completion, reset playback, view the reconstructed tactical board, compare current and expected fingerprints, review state deltas, and read an action timeline.

### Validation

Added inspector tests for initial state reporting, stepping, full playback, reset behavior, action timeline state, and rejected replay error display. Native validation now requires the replay inspector domain model, command-table controls, and test scenarios.

## [0.3.3-replay-diffs] — 2026-08-24

### Added

Added canonical replay state snapshots and a deterministic human-readable diff generator. Mismatch diagnostics now identify scalar encounter state, tile, unit, cooldown, status, and action-order differences in a stable expected-versus-actual format.

### Validation

Added explicit replay diff scenarios for exact snapshot equality, phase divergence, and combined tile, unit, and action divergences. Replay equivalence assertions now emit the formatted diff when a canonical state check fails.

## [0.3.2-replays] — 2026-08-24

### Added

Added canonical replay fingerprints and five saved-encounter determinism scenarios. The test suite now round-trips serialized replay records, verifies independent playback states after each action, resets to the exact seeded state, rejects out-of-order actions without consuming them, and distinguishes encounter seeds.

### Changed

Replay playback now validates recorded turns and produced tactical events before advancing. Player end-turn events are recorded explicitly, while the replayer finishes an enemy phase without inventing extra replay actions.

## [0.3.1-tests] — 2026-08-24

### Added

Added a standalone .NET 8 xUnit project containing twelve deterministic advanced-pathfinding tests. The suite covers standard, Trailblazer, Winged, and Phasewalker movement; threat-safe and fastest routes; shared and owner reservations; legal approaches to occupied targets; flank anchors; route diagnostics; and reservation-state filtering.

### Changed

The Godot runtime project now excludes test source from its runtime assembly. The native CI workflow restores and executes the test suite after compiling the Godot C# project. Native callback delegates were corrected during full C# compilation.

### Validation

The Godot C# Release build completed with zero warnings and zero errors. All twelve pathfinding tests passed under .NET SDK 8.0.424.

## [0.3.0-tactics] — 2026-08-24

### Added

Added Duelist, Runesmith, Seer, and Skywarden heroes, plus Iron Sentinel, Gale Harrier, Cinder Artillery, and Shade Stalker enemies. Each role has JSON-authored tactical class, mobility profile, role tags, abilities, and integrated deterministic encounter coverage.

Added mobility-aware movement, cover and high-ground tiles, threat-weighted safe routes, reservation-aware route costs, legal approach routes, flank-anchor discovery, class-rule combat modifiers, suggested-route reservations, and board cues for cover, high ground, flank anchors, and suggested paths.

### Validation

The native validator now cross-checks expanded roster identifiers, enum-compatible class metadata, ability references, and the advanced pathfinding APIs alongside project structure, content files, and export presets.

## [0.2.1-godot] — 2026-08-24

### Changed

RuneGrid Tactics is now a **Godot 4 .NET C#–only repository**. The retired non-native client stack, its package graph, its build configuration, its documentation, and its automation have been removed. The Godot project has moved to the repository root.

### Added

The root now contains the native C# source, JSON content, Godot main scene, export presets, release scripts, native CI validation, and executable build guidance.

### Validation

The native structure and all six JSON content files validate through `node Tools/validate-project.mjs .`. Final editor compilation, export, and device testing remain a required native-toolchain release gate.
