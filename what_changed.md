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
