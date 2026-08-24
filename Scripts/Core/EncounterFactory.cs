namespace RuneGrid.Tactics.Core;

public sealed class EncounterFactory
{
    public const string GameVersion = "0.2.0-godot";
    private readonly ContentRepository _content;

    private sealed record ModeDefinition(string Title, string Briefing, string Objective, int EnemyCount, int? TurnLimit = null);

    private static readonly IReadOnlyDictionary<GameMode, ModeDefinition> Modes = new Dictionary<GameMode, ModeDefinition>
    {
        [GameMode.Campaign] = new("The Sunken Causeway", "A collapsed rune-road has opened a route for ash scavengers. Secure the marker before the causeway breaks.", "Clear the hostile patrol.", 3),
        [GameMode.Expedition] = new("Uncharted Field", "A seed-marked route waits beyond the known atlas. Read the field and bring the party home.", "Eliminate all hostiles.", 4),
        [GameMode.Daily] = new("Daily Cartography", "Every field team receives the same marked coordinates today. Complete the route to set a clean record.", "Clear the shared daily field.", 4),
        [GameMode.Weekly] = new("Weekly Survey", "The week’s anomaly has gathered resistance around an old gate. Choose every route carefully.", "Defeat the anomaly guard.", 5),
        [GameMode.Puzzle] = new("Signal Puzzle", "The board is compact, the enemy positions are known, and no action can be wasted.", "Win in six turns or fewer.", 3, 6),
        [GameMode.Survival] = new("Stormline Stand", "Hold the ridge while incoming threats test the line.", "Survive the hostile wave.", 5),
        [GameMode.BossRush] = new("Faultline Crown", "A Stone Brute guards the crown marker. Break the formation without letting it dictate the board.", "Defeat the Stone Brute.", 3),
        [GameMode.Custom] = new("Custom Field", "Set your own seed and make the map your training instrument.", "Clear the configured encounter.", 4),
        [GameMode.Training] = new("Training Grounds", "A controlled board for practicing movement, targeting, and timing without pressure.", "Defeat the training targets.", 2),
        [GameMode.Tutorial] = new("First Marks", "Move, mark a target, and bring the team through the opening field.", "Complete the guided skirmish.", 2),
        [GameMode.Endless] = new("Endless Meridian", "The field shifts after every clean victory. Record your route before the horizon draws away.", "Clear this field to continue the expedition.", 5)
    };

    public EncounterFactory(ContentRepository content) => _content = content;

    public EncounterState Create(string seed, GameMode mode, Difficulty difficulty)
    {
        var descriptor = Modes[mode];
        var random = new DeterministicRandom($"{GameVersion}:{mode}:{difficulty}:{seed}");
        var width = mode == GameMode.Puzzle ? 7 : 9;
        var height = mode == GameMode.Puzzle ? 6 : 7;
        var grid = BuildGrid(random, width, height);
        var heroes = new[]
        {
            MakeUnit(_content.Heroes["vanguard"], new GridPoint(0, height - 2), "hero-vanguard"),
            MakeUnit(_content.Heroes["runemage"], new GridPoint(1, height - 1), "hero-runemage"),
            MakeUnit(_content.Heroes["ranger"], new GridPoint(2, height - 1), "hero-ranger")
        };
        var enemyPool = mode == GameMode.BossRush
            ? new[] { "stone_brute", "ash_raider", "frost_wisp" }
            : new[] { "ash_raider", "frost_wisp", "void_scout", "thorn_caster" };
        var difficultyBalance = _content.Balance.Difficulty[difficulty.ToString()];
        var reinforcement = difficultyBalance.AdditionalEnemies;
        var candidateTiles = random.Shuffle(grid.Tiles.Where(tile => tile.Position.X >= width / 2 && tile.Kind is not TileKind.Wall and not TileKind.Gate).ToList());
        var enemies = Enumerable.Range(0, descriptor.EnemyCount + reinforcement).Select(index =>
        {
            var templateId = index == 0 && mode == GameMode.BossRush ? "stone_brute" : random.Pick(enemyPool);
            var unit = MakeUnit(_content.Enemies[templateId], candidateTiles[index].Position, $"enemy-{index}-{templateId}");
            unit.Health += difficultyBalance.ReinforcedEnemyHealth;
            return unit;
        });

        return new EncounterState
        {
            Id = $"{mode}-{seed}", Seed = seed, Mode = mode, Difficulty = difficulty, Title = descriptor.Title,
            Briefing = descriptor.Briefing, Objective = descriptor.Objective, TurnLimit = descriptor.TurnLimit,
            Grid = grid, Units = heroes.Concat(enemies).ToList(), RewardShards = mode == GameMode.BossRush ? _content.Balance.Rewards.BossShards : _content.Balance.Rewards.StandardShards, RewardMastery = mode == GameMode.Training ? _content.Balance.Rewards.TrainingMastery : _content.Balance.Rewards.StandardMastery
        };
    }

    private static UnitState MakeUnit(UnitTemplate template, GridPoint position, string instanceId) => new()
    {
        Id = instanceId, Template = template, Position = position, Health = template.MaxHealth, Energy = template.Energy
    };

    private static TacticalGrid BuildGrid(DeterministicRandom random, int width, int height)
    {
        var tiles = Enumerable.Range(0, height).SelectMany(y => Enumerable.Range(0, width).Select(x => new Tile { Position = new GridPoint(x, y) })).ToList();
        var protectedPoints = new HashSet<GridPoint> { new(0, height - 2), new(0, height - 1), new(1, height - 1), new(2, height - 1), new(width - 1, 0), new(width - 1, 1) };
        var featureKinds = new[] { TileKind.Wall, TileKind.Difficult, TileKind.Healing, TileKind.Hazard, TileKind.Teleport, TileKind.Destructible };
        for (var index = 0; index < 10; index++)
        {
            var point = new GridPoint(random.NextInt(1, width - 2), random.NextInt(1, height - 2));
            var tile = tiles.Single(candidate => candidate.Position == point);
            if (protectedPoints.Contains(point) || tile.Kind != TileKind.Floor) continue;
            tile.Kind = random.Pick(featureKinds);
            tile.Elevation = tile.Kind == TileKind.Wall ? 1 : 0;
            tile.Integrity = tile.Kind == TileKind.Destructible ? 8 : null;
        }
        var teleports = tiles.Where(tile => tile.Kind == TileKind.Teleport).ToList();
        if (teleports.Count == 1)
        {
            var fallback = tiles.Single(tile => tile.Position == new GridPoint(width - 2, 2));
            fallback.Kind = TileKind.Teleport;
            teleports.Add(fallback);
        }
        if (teleports.Count >= 2)
        {
            teleports[0].LinkedTo = teleports[1].Position;
            teleports[1].LinkedTo = teleports[0].Position;
        }
        return new TacticalGrid(width, height, tiles);
    }
}
