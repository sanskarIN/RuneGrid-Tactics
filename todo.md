# RuneGrid Tactics — Native Release Checklist

- [x] Convert the source repository to a Godot 4 .NET C#–only project.
- [x] Move the native project, JSON content, export tools, and presets to the repository root.
- [x] Replace non-native automation with the Godot .NET source validation workflow.
- [x] Validate the root native project structure and JSON-authored records.
- [x] Commit and push the Godot-only repository conversion to GitHub.
- [ ] Open `project.godot` in Godot 4 .NET and resolve any editor-import or script compilation findings.
- [ ] Run `dotnet build RuneGrid.Tactics.csproj` on a toolchain-ready machine.
- [ ] Export and smoke-test Windows, Linux, and Android native packages before release distribution.
