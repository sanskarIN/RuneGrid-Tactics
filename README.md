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

## Tactical roster and routecraft

The field roster now spans **Vanguard, Channeler, Pathfinder, Warden, Duelist, Runesmith, Seer, and Skywarden** hero classes, supported by Sentinel, Harrier, Stalker, and Artillery enemy roles. Each unit declares a tactical class, mobility profile, role tags, and JSON-authored ability list.

The C# grid engine supports weighted movement, mobility-specific terrain treatment, cover and high-ground analysis, threat-weighted safe routes, squad reservations, nearest legal approaches to occupied targets, flank-anchor discovery, and recommended-route highlights. The command bar can reserve the selected hero’s suggested route so the squad plan remains legible before movement is committed.

## Start development

Install the **.NET-enabled Godot 4.5 editor** and a compatible 64-bit .NET SDK. Open [`project.godot`](project.godot), build the C# project in the editor, and press **F5**. See [BUILD_EXECUTABLES.md](BUILD_EXECUTABLES.md) for complete Windows, Linux, macOS, and Android executable guidance, and [STRUCTURE.md](STRUCTURE.md) for the architecture.

Run the deterministic advanced pathfinding suite with `dotnet test Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj --configuration Release`. The complete native test contract is documented in [TESTING.md](TESTING.md).

Archived replay records can be reviewed through **REPLAYS → INSPECT** in the command table. The inspector shows the reconstructed field, current and expected fingerprints, deterministic audit result, differences from the opening state, action timeline, rejected-action messages, and step/reset/play-to-end controls. Use the timeline slider, **PREVIOUS**, **NEXT**, or any action row to rebuild the exact post-action state without accumulating playback drift. While inspecting, use **Left Arrow** for previous, **Right Arrow** or **Space** for next, **Home** for the opening state, **End** for the final state, and **P** to play to the end; modified key combinations and active text entry are intentionally ignored.

Replay inspector commands are configurable from **SETTINGS & ACCESSIBILITY → REPLAY INSPECTOR KEYS**. Select a command, press one unmodified supported key, and the local record saves it immediately. Each command must remain unique. The settings panel accepts letters, arrows, **Space**, **Home**, **End**, **Page Up**, and **Page Down**; it rejects duplicate, unsupported, and modifier-key assignments, and offers **RESTORE DEFAULT REPLAY KEYS**.

While viewing a replay, press **F1** or select **KEYS · F1** to open the in-game **SHORTCUT REFERENCE** overlay. It lists the player’s active saved bindings and explains each deterministic timeline action. The overlay safely pauses replay keyboard navigation until dismissed with **F1**, **Escape**, or **CLOSE REFERENCE**.

On the first replay inspection, a local **INTRODUCING REPLAY INSPECTOR** tooltip explains deterministic action review and points directly to **VIEW SHORTCUTS** and **F1**. Selecting **GOT IT** or **VIEW SHORTCUTS** marks the guidance as seen in the local record, so it does not reappear on later inspector sessions.

If the replay inspector’s expected reconstruction differs from the visible playback state, it presents a non-blocking **REPLAY DETERMINISM WARNING**. The tooltip identifies the action position and first detected difference, directs the player to **DETERMINISM AUDIT**, and offers **VIEW SHORTCUTS** for safe navigation. Dismissing a warning acknowledges only that exact action and fingerprint signature; a changed mismatch remains visible.

The **DETERMINISM AUDIT** now includes a **FILTERED DIFF · AFFECTED BOARD MARKERS** control set. Choose **ALL**, **PHASE / STATE**, **TILE**, **UNIT**, or **ACTION** to isolate the relevant mismatch lines. Tile filters draw a warm outline on affected cells, while unit filters draw a delta ring and marker around affected living units; **ALL** restores the complete human-readable audit.

The repository intentionally ships no user account, advertising network, or cloud persistence dependency. Core gameplay remains local to the native Godot application.

## Current build status

The source repository includes the native scene, tactical engine, local record system, export presets, release scripts, and CI workflow. Local C# Release compilation and framework-independent test validation run with .NET 8; final editor import, native export, and device smoke testing still require a provisioned Godot 4 .NET machine.

## Support and license

Development support is optional and never changes combat, progression, or access to the game. [Support RuneGrid Tactics on Buy Me a Coffee](https://buymeacoffee.com/sanskarIN). For support, write to **supportramsandesh@gmail.com**. The project is released under the [MIT License](LICENSE).
