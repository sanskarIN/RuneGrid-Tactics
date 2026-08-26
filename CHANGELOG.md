# Changelog

All notable native RuneGrid Tactics changes are recorded here.

## [0.3.11-filtered-replay-diffs] — 2026-08-26

### Added

Added filtered replay determinism diffs for **ALL**, **PHASE / STATE**, **TILE**, **UNIT**, and **ACTION** categories. The inspector now highlights selected affected tiles and living units directly on the replay board while preserving the complete audit through the All filter.

### Validation

Added deterministic classification and marker extraction coverage. Native validation now requires the filter model, inspector controls, and tile/unit board marker route.

## [0.3.10-replay-mismatch-warnings] — 2026-08-24

### Added

Added contextual **REPLAY DETERMINISM WARNING** tooltips to the replay inspector. A warning appears when expected and visible state diverge, identifies the first difference and action position, directs players to the audit panel, and offers shortcut reference access without blocking playback.

### Validation

Added deterministic mismatch-signature acknowledgement coverage. Native validation now requires warning-state tracking, mismatch summary contracts, warning tooltip composition, audit guidance, acknowledgement, and shortcut-reference routing.

## [0.3.9-replay-onboarding] — 2026-08-24

### Added

Added a first-time **INTRODUCING REPLAY INSPECTOR** tooltip. It explains deterministic replay review, directs players to **VIEW SHORTCUTS** and **F1**, and records acknowledgement locally through **GOT IT** or the shortcut-reference route so later inspections remain unobstructed.

### Validation

Added serialized onboarding-state coverage and native validation requirements for onboarding persistence, tooltip composition, acknowledgement, and shortcut-reference entry.

## [0.3.8-replay-shortcut-reference] — 2026-08-24

### Added

Added an in-game **SHORTCUT REFERENCE** overlay to the replay inspector. **F1** and **KEYS · F1** open a modal list of the player’s active saved bindings, command descriptions, and explicit close instructions. **F1**, **Escape**, and **CLOSE REFERENCE** dismiss it without changing replay state.

### Validation

Added deterministic active-binding reference coverage and native validation requirements for the overlay builder, F1 route, command-table trigger, and close behavior.

## [0.3.7-configurable-replay-keys] — 2026-08-24

### Added

Added persistent replay inspector key configuration under **SETTINGS & ACCESSIBILITY**. Players can capture a distinct supported key for previous, next, opening state, final state, and play-to-end commands, restore defaults, and see the active bindings in the inspector legend and timeline tooltip.

### Validation

Added deterministic tests for custom binding serialization and resolution, duplicate and unsupported key rejection, malformed imported-binding normalization, and default restoration. Native validation now requires the binding model and named test scenarios.

## [0.3.6-replay-shortcuts] — 2026-08-24

### Added

Added replay inspector keyboard navigation: **Left Arrow** moves to the previous action, **Right Arrow** or **Space** moves to the next action, **Home** and **End** rebuild the opening and final states, and **P** plays to the end. The command table displays this key legend while a replay is open.

### Validation

Added framework-independent shortcut mapping coverage for accepted timeline keys and rejected modified or unknown keys. Native validation now requires the key mapping contract, early Godot key input, visible hint, and test scenario.

## [0.3.5-scrubbing] — 2026-08-24

### Added

Added deterministic timeline scrubbing to the command-table replay inspector. The inspector now supports a discrete timeline slider, direct action-row selection, previous and next controls, and exact state reconstruction at any archived action position.

### Validation

Added native tests for direct seek equality, previous/next fingerprint stability, and out-of-range seek preservation. The replay inspector validator now requires scrubbing methods, slider controls, and navigation scenarios.

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
