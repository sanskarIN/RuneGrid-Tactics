# Godot C# Migration

RuneGrid Tactics now has a native Godot 4 .NET implementation under [`godot/`](godot/). The prior React/Babylon browser project remains in the repository as an existing web reference and playable browser build; it is not deleted during this migration. The native project becomes the preferred path for an Android-first executable roadmap because it can package native desktop and mobile builds, keep gameplay local, and retain the tactical domain in C#.

| Browser reference                          | Godot C# implementation                                           |
| ------------------------------------------ | ----------------------------------------------------------------- |
| React lifecycle frame and Babylon renderer | Godot `Node`, `Control`, and custom-drawn `BoardView`             |
| TypeScript `GridModel` and `GameSession`   | C# `TacticalGrid` and `GameSession`                               |
| TypeScript configuration objects           | JSON content in `godot/Data/`                                     |
| Browser local storage                      | Versioned JSON save envelope at `user://`                         |
| Vite build and browser deployment          | Godot desktop/mobile export presets and native executable scripts |

The C# project deliberately keeps core rules independent of the scene graph. `TacticalGrid`, deterministic generation, encounters, abilities, turn control, and persistence can be unit-tested or reused without making tile decisions inside a UI script. `GameRoot` and `BoardView` are responsible for the native command-table presentation and translate player input into legal `GameSession` requests.

## Migration status

The native project includes JSON content, a playable command table, deterministic field modes, weighted pathfinding, line-of-sight, ability resolution, enemy turns, local progression and replays, local save integrity checks, accessibility preference state, native export presets, and platform build guidance. The sandbox did not include Godot .NET or a .NET SDK, so source validation and runtime export must run in the CI workflow or on a provisioned local Godot machine before distributing executables.

See [`godot/BUILD_EXECUTABLES.md`](godot/BUILD_EXECUTABLES.md) for the complete platform build procedure.
