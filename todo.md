# RuneGrid Tactics — Native Release Checklist

- [x] Convert the source repository to a Godot 4 .NET C#–only project.
- [x] Move the native project, JSON content, export tools, and presets to the repository root.
- [x] Replace non-native automation with the Godot .NET source validation workflow.
- [x] Validate the root native project structure and JSON-authored records.
- [x] Commit and push the Godot-only repository conversion to GitHub.
- [ ] Open `project.godot` in Godot 4 .NET and resolve any editor-import or script compilation findings.
- [ ] Run `dotnet build RuneGrid.Tactics.csproj` on a toolchain-ready machine.
- [ ] Export and smoke-test Windows, Linux, and Android native packages before release distribution.
- [ ] Restart and verify the managed preview service after the native-project conversion.
- [x] Add new JSON-driven tactical unit classes, abilities, and enemy role coverage.
- [x] Add advanced C# routefinding features for terrain profiles, tactical reservations, flank routes, and route analysis.
- [x] Integrate new tactical metadata into native encounters and board highlights, validate, commit, and push the expansion.
- [x] Add a Godot .NET pathfinding test project with deterministic grid scenarios.
- [x] Cover mobility, weighted safety, reservations, legal approach, flank, phasewalking, and route diagnostics with unit tests.
- [ ] Commit and push the validated Godot .NET pathfinding test suite to GitHub.
