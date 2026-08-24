# Contributing to RuneGrid Tactics

RuneGrid Tactics is a Godot 4 .NET C# project. Contributions should preserve its native-only architecture, local-first data contract, accessibility baseline, original identity, and JSON-driven content model.

Use focused branches such as `feature/native-terrain-hooks`, `fix/godot-save-validation`, or `test/csharp-pathfinding-edges`. Configure the repository-local author identity:

```bash
git config user.name "Sanskar"
git config user.email "sanskarin@outlook.in"
```

Before opening a pull request, run `node Tools/validate-project.mjs .`, then run `dotnet build RuneGrid.Tactics.csproj` on a Godot .NET-ready machine. New tactical rules belong in `Scripts/Core/`; Godot scene and interface work belongs in `Scripts/Godot/`; heroes, enemies, abilities, items, levels, and balance belong in `Data/`. Do not introduce a second client stack, cloud-only progression, copyrighted game assets, fabricated reviews, or personal-data collection.

Follow the [Code of Conduct](CODE_OF_CONDUCT.md) and report security concerns through [SECURITY.md](SECURITY.md).
