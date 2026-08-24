# RuneGrid Tactics Native Architecture

RuneGrid Tactics is a native Godot 4 .NET game. C# tactical rules own the game state, Godot scripts translate player input into legal requests, and JSON records define content. The board presentation never decides tactical legality.

```text
Godot scene / GameRoot / BoardView
             ↓ player input
        GameSession
             ↓
TacticalGrid · EncounterFactory · ContentRepository
             ↓
LocalSaveRepository · ProgressionService · ReplayPlayer
             ↓
Godot user:// local record
```

| Component | Responsibility |
| --- | --- |
| `Scripts/Core/ContentRepository.cs` | Loads enum-aware JSON content. |
| `Scripts/Core/TacticalGrid.cs` | Owns weighted routefinding, occupancy, ranges, and line-of-sight. |
| `Scripts/Core/EncounterFactory.cs` | Generates deterministic encounters from mode, difficulty, and seed. |
| `Scripts/Core/GameSession.cs` | Owns turn phases, legal actions, ability resolution, AI, undo, outcomes, and replay actions. |
| `Scripts/Core/LocalSaveRepository.cs` | Validates and writes schema-versioned local save envelopes with backup recovery. |
| `Scripts/Godot/GameRoot.cs` | Builds the native command-table screens and sends input into the core session. |
| `Scripts/Godot/BoardView.cs` | Draws interactive native tiles and tokens from authoritative game state. |

Add heroes, enemies, abilities, items, levels, and balance first in `Data/`. Add tactical rules to `Scripts/Core/`, and Godot-specific presentation to `Scripts/Godot/`. The repository has one native application path.
