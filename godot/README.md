# RuneGrid Tactics — Godot 4 C# Edition

This directory is the native, offline-first Godot 4 .NET implementation of RuneGrid Tactics. It uses C# for tactical rules, native Godot controls for the command-table interface, and JSON files for original heroes, enemies, abilities, equipment, levels, and balance values.

| Directory            | Purpose                                                                                                                                                           |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Scripts/Core/`      | Framework-independent tactical contracts, seeded generation, pathfinding, line-of-sight, turn state, AI resolution, local saves, progression, and replay records. |
| `Scripts/Godot/`     | Godot autoload services, interactive board renderer, and command-table screen construction.                                                                       |
| `Data/`              | JSON-authored hero, enemy, ability, item, level, and balance records.                                                                                             |
| `Scenes/`            | Godot scene entry points.                                                                                                                                         |
| `Tools/`             | Windows/Linux/Android release export commands.                                                                                                                    |
| `export_presets.cfg` | Version-controlled non-secret export presets.                                                                                                                     |

The source is engineered so new tactical content normally enters through JSON. Adding a hero does not require changing pathfinding, local persistence, replay handling, or board interaction code.

## Quick start

1. Install Godot 4.5 .NET and a compatible 64-bit .NET SDK.
2. Open `project.godot` in the .NET-enabled editor.
3. Build the C# project with the editor’s **Build** action or `dotnet build RuneGrid.Tactics.csproj`.
4. Press **F5** to run the command table and start any listed field mode.
5. Follow [BUILD_EXECUTABLES.md](BUILD_EXECUTABLES.md) for Windows, Linux, macOS, and Android package guidance.

The current source-first migration was created in a workspace without Godot .NET or a .NET SDK. Native export must be validated on a properly provisioned Godot .NET machine before release distribution.
