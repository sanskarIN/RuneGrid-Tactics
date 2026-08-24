# Changelog

All notable native RuneGrid Tactics changes are recorded here.

## [0.2.1-godot] — 2026-08-24

### Changed

RuneGrid Tactics is now a **Godot 4 .NET C#–only repository**. The retired non-native client stack, its package graph, its build configuration, its documentation, and its automation have been removed. The Godot project has moved to the repository root.

### Added

The root now contains the native C# source, JSON content, Godot main scene, export presets, release scripts, native CI validation, and executable build guidance.

### Validation

The native structure and all six JSON content files validate through `node Tools/validate-project.mjs .`. Final editor compilation, export, and device testing remain a required native-toolchain release gate.
