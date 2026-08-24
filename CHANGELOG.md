# Changelog

All notable changes are recorded here using a Keep-a-Changelog-inspired structure.

## [0.2.0-godot] — 2026-08-24

### Added

The repository now includes a native Godot 4 .NET C# project with JSON-authored tactical content, deterministic encounter creation, weighted pathfinding, line-of-sight, legal target previews, ability resolution, turn phases, simple deterministic enemy decisions, local save recovery, progression, replay records, native command-table UI code, export presets, platform release scripts, CI C# source validation, and complete executable build guidance.

### Migration note

The browser implementation is retained as a reference rather than being deleted. The current workspace did not include Godot .NET or a .NET SDK, so a machine with the documented native toolchain must perform the final C# compilation, Godot import, and executable export verification.

## [0.1.0] — 2026-08-24

### Added

The project now includes a playable Babylon.js tactical board with three original playable heroes, deterministic procedural encounters, obstacles, terrain states, target previews, weighted pathfinding, line-of-sight, abilities, enemy resolution, turn ownership, replay action metadata, achievements, local statistics, responsive screen navigation, local-save export/import/recovery, and accessibility preferences.

The release also adds original generated visual assets, an original procedural audio feedback layer, contributor documentation, privacy and support statements, test coverage, and GitHub Actions CI.

### Fixed

The renderer now creates an active camera before the engine render loop begins. Enemy turn scheduling is environment-safe for browser runtime and automated tests. Unit undo restores authoritative session state instead of relying on stale unit references.

### Known limitations

This release is browser-ready and Android-first in responsive design; it is not a signed native Android package. Campaign regions beyond the initial playable field are represented as accessible route records and seeded encounter starts rather than a fully authored multi-chapter narrative.
