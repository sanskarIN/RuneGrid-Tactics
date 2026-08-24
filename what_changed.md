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
| Remaining release gate | Use a machine with Godot 4 .NET and the matching .NET SDK to import the project, compile C#, create platform packages, and run desktop/mobile smoke tests. |
