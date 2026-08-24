namespace RuneGrid.Tactics.Core;

public enum ElementKind { None, Fire, Frost, Storm, Nature, Arcane, Void }
public enum Faction { Hero, Enemy }
public enum TileKind { Floor, Wall, Difficult, Healing, Hazard, Teleport, Gate, Destructible }
public enum AbilityKind { Damage, Heal, Shield, Teleport, Tile }
public enum AbilityShape { Single, Area, Line, Self }
public enum GamePhase { Briefing, Player, Resolving, Enemy, Victory, Defeat }
public enum GameMode { Campaign, Expedition, Daily, Weekly, Puzzle, Survival, BossRush, Custom, Training, Tutorial, Endless }
public enum Difficulty { Field, Veteran, Legend }

public readonly record struct GridPoint(int X, int Y)
{
    public int ManhattanDistance(GridPoint other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
    public override string ToString() => $"{X}:{Y}";
}

public sealed class Tile
{
    public required GridPoint Position { get; init; }
    public TileKind Kind { get; set; } = TileKind.Floor;
    public int Elevation { get; set; }
    public int? Integrity { get; set; }
    public GridPoint? LinkedTo { get; set; }
}

public sealed class AbilityDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required string Description { get; init; }
    public AbilityKind Kind { get; init; }
    public AbilityShape Shape { get; init; }
    public int Range { get; init; }
    public int Radius { get; init; } = 1;
    public int Power { get; init; }
    public int Cooldown { get; init; }
    public int EnergyCost { get; init; }
    public ElementKind Element { get; init; }
    public string? Status { get; init; }
}

public sealed class UnitTemplate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Title { get; init; }
    public Faction Faction { get; init; }
    public required string Archetype { get; init; }
    public int MaxHealth { get; init; }
    public int Attack { get; init; }
    public int Defense { get; init; }
    public int Movement { get; init; }
    public int Energy { get; init; }
    public ElementKind Element { get; init; }
    public required string Passive { get; init; }
    public required string Ultimate { get; init; }
    public IReadOnlyList<string> AbilityIds { get; init; } = Array.Empty<string>();
    public string? AiProfile { get; init; }
    public required string Color { get; init; }
}

public sealed class ItemDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Slot { get; init; }
    public required string Description { get; init; }
    public required string Rarity { get; init; }
}

public sealed class LevelDefinition
{
    public required string Id { get; init; }
    public GameMode Mode { get; init; }
    public required string Title { get; init; }
    public required string Objective { get; init; }
    public int RewardShards { get; init; }
    public string? UnlockHero { get; init; }
    public required string TerrainProfile { get; init; }
}

public sealed class DifficultyBalance
{
    public int ReinforcedEnemyHealth { get; init; }
    public int AdditionalEnemies { get; init; }
}

public sealed class TerrainBalance
{
    public int DifficultMovementCost { get; init; }
    public int HealingAmount { get; init; }
    public int HazardDamage { get; init; }
    public int DestructibleIntegrity { get; init; }
}

public sealed class RewardBalance
{
    public int StandardShards { get; init; }
    public int BossShards { get; init; }
    public int TrainingMastery { get; init; }
    public int StandardMastery { get; init; }
}

public sealed class BalanceDefinition
{
    public int SchemaVersion { get; init; }
    public required Dictionary<string, DifficultyBalance> Difficulty { get; init; }
    public required TerrainBalance Terrain { get; init; }
    public required RewardBalance Rewards { get; init; }
}

public sealed class UnitState
{
    public required string Id { get; init; }
    public required UnitTemplate Template { get; init; }
    public GridPoint Position { get; set; }
    public int Health { get; set; }
    public int Energy { get; set; }
    public int Shield { get; set; }
    public bool Moved { get; set; }
    public bool Acted { get; set; }
    public Dictionary<string, int> Cooldowns { get; } = new();
    public Dictionary<string, int> Statuses { get; } = new();
    public bool IsAlive => Health > 0;
    public Faction Faction => Template.Faction;
}

public sealed class EncounterState
{
    public required string Id { get; init; }
    public required string Seed { get; init; }
    public GameMode Mode { get; init; }
    public Difficulty Difficulty { get; init; }
    public required string Title { get; init; }
    public required string Briefing { get; init; }
    public required string Objective { get; init; }
    public int? TurnLimit { get; init; }
    public required TacticalGrid Grid { get; init; }
    public required List<UnitState> Units { get; init; }
    public int RewardShards { get; init; }
    public int RewardMastery { get; init; }
}

public sealed record TacticalAction(int Turn, string ActorId, string Type, GridPoint? Target, string? AbilityId, string Note);
public sealed record ReplayRecord(int SchemaVersion, string EncounterId, string Seed, GameMode Mode, Difficulty Difficulty, DateTimeOffset CreatedAt, IReadOnlyList<TacticalAction> Actions, string? Outcome);

public sealed class TacticalHighlights
{
    public IReadOnlySet<GridPoint> Reachable { get; init; } = new HashSet<GridPoint>();
    public IReadOnlySet<GridPoint> Targets { get; init; } = new HashSet<GridPoint>();
    public IReadOnlySet<GridPoint> Danger { get; init; } = new HashSet<GridPoint>();
    public GridPoint? Selected { get; init; }
}
