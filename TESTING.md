# Native Testing Guide

RuneGrid Tactics keeps tactical navigation in framework-independent C# contracts, allowing the advanced grid system to be verified without rendering a Godot scene. The `Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj` project links the core routefinding files and uses xUnit under .NET 8.

## Run the suite

Use the .NET SDK that accompanies the Godot 4 .NET development setup. From the repository root, run:

```bash
dotnet restore Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj
dotnet test Tests/RuneGrid.Tactics.Pathfinding.Tests.csproj --no-restore --configuration Release
```

The GitHub Actions Godot .NET workflow restores, compiles, and runs the same test project on every relevant push and pull request. The local implementation sandbox does not include the .NET SDK or Godot .NET executable, so local runtime execution is intentionally deferred to a correctly provisioned native development machine or CI.

## Coverage contract

| Test family | Protected behavior |
| --- | --- |
| Terrain mobility | Standard, Trailblazer, and Winged units receive the expected travel cost across difficult and hazard tiles. |
| Phasewalking | A Phasewalker may route through a wall but can never end a movement or selected route inside a wall. |
| Safe and fast intents | Safe routes avoid threatened corridors when a better tactical detour exists; fastest routes take the short travel path. |
| Reservation awareness | Allied route reservations impose a configurable detour cost, while an owner’s own reservation remains unpenalized. |
| Occupied target approach | An approach route resolves a legal adjacent tile instead of attempting to occupy an enemy’s position. |
| Flank anchors | Opposite-side adjacent positions are discovered only when reachable and legal. |
| Route diagnostics | Route analysis reports threat exposure, cover stops, high-ground arrival, and reservation use. |
| Reservation state | Shared squad reservations retain a living unit’s first declared destination and ignore fallen units. |

When introducing a new mobility profile, tile type, route intent, or routing penalty, add a deterministic scenario to `TacticalGridPathfindingTests.cs` before changing encounter balance. This keeps navigation behavior explainable and protects replay determinism.
