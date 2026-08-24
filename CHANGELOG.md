# Changelog

All notable native RuneGrid Tactics changes are recorded here.

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
