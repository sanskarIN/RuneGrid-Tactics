# RuneGrid Tactics

> **Mark a route. Own the turn.**

RuneGrid Tactics is an original, offline-first tactical roguelite built exclusively with **Godot 4 .NET**, **C#**, and **JSON-authored game content**.

## Native game stack

| Layer | Technology | Responsibility |
| --- | --- | --- |
| Engine | Godot 4 .NET | Native scenes, input, 2D command-table presentation, platform export, and device runtime. |
| Gameplay | C# / .NET 8 | Deterministic fields, grid rules, pathfinding, line-of-sight, abilities, enemy AI, saves, progression, and replay records. |
| Content | JSON | Heroes, enemies, abilities, items, levels, and balance values. |
| Storage | Godot `user://` JSON records | Local settings, statistics, unlocks, replays, and validated save recovery. |
| Automation | GitHub Actions YAML | C# restore/build and content-structure validation. |

## Repository layout

| Directory or file | Purpose |
| --- | --- |
| `Scripts/Core/` | Framework-independent tactical engine, content loading, deterministic field generation, persistence, progression, and replay systems. |
| `Scripts/Godot/` | Godot autoload services, command-table application root, and interactive board view. |
| `Data/` | Original JSON-configured heroes, enemies, abilities, items, campaign levels, and balance data. |
| `Scenes/Main.tscn` | Native game entry scene. |
| `Tools/` | Release export scripts and native project validator. |
| `export_presets.cfg` | Version-controlled non-secret Windows, Linux, and Android export presets. |

## Start development

Install the **.NET-enabled Godot 4.5 editor** and a compatible 64-bit .NET SDK. Open [`project.godot`](project.godot), build the C# project in the editor, and press **F5**. See [BUILD_EXECUTABLES.md](BUILD_EXECUTABLES.md) for complete Windows, Linux, macOS, and Android executable guidance, and [STRUCTURE.md](STRUCTURE.md) for the architecture.

The repository intentionally ships no user account, advertising network, or cloud persistence dependency. Core gameplay remains local to the native Godot application.

## Current build status

The source repository includes the native scene, tactical engine, local record system, export presets, release scripts, and CI workflow. This sandbox did not include Godot .NET or the .NET SDK, so final editor import, C# compilation, native export, and device smoke testing must run on a provisioned Godot .NET machine.

## Support and license

Development support is optional and never changes combat, progression, or access to the game. [Support RuneGrid Tactics on Buy Me a Coffee](https://buymeacoffee.com/sanskarIN). For support, write to **supportramsandesh@gmail.com**. The project is released under the [MIT License](LICENSE).
