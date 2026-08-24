using RuneGrid.Tactics.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace RuneGrid.Tactics.Pathfinding.Tests;

public sealed class TacticalGridPathfindingTests
{
    [Fact]
    public void StandardUnit_AccountsForDifficultTerrainInTravelCost()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Difficult);
        var route = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 5, null, Options());

        Assert.NotNull(route);
        Assert.Equal(3, route!.TravelCost);
        Assert.Equal(new[] { new GridPoint(1, 0), new GridPoint(2, 0) }, route.Path);
    }

    [Fact]
    public void Trailblazer_ReducesDifficultTerrainToOneMovement()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Difficult);
        var route = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 2, null, Options(MobilityProfile.Trailblazer));

        Assert.NotNull(route);
        Assert.Equal(2, route!.TravelCost);
    }

    [Fact]
    public void WingedUnit_TreatsHazardAsOneMovement()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Hazard);

        var standard = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 2, null, Options());
        var winged = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 2, null, Options(MobilityProfile.Winged));

        Assert.Null(standard);
        Assert.NotNull(winged);
        Assert.Equal(2, winged!.TravelCost);
    }

    [Fact]
    public void Phasewalker_CanCrossWallButCannotFinishInsideWall()
    {
        var grid = CreateGrid(3, 1, tiles => TileAt(tiles, 1, 0).Kind = TileKind.Wall);

        var standard = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 4, null, Options());
        var phaseRoute = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 4, null, Options(MobilityProfile.Phasewalker));
        var wallGoal = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(1, 0), [], 4, null, Options(MobilityProfile.Phasewalker));
        var reachable = grid.Reachable(new GridPoint(0, 0), 4, [], null, Options(MobilityProfile.Phasewalker));

        Assert.Null(standard);
        Assert.NotNull(phaseRoute);
        Assert.Equal(new[] { new GridPoint(1, 0), new GridPoint(2, 0) }, phaseRoute!.Path);
        Assert.Null(wallGoal);
        Assert.DoesNotContain(new GridPoint(1, 0), reachable.Keys);
        Assert.Contains(new GridPoint(2, 0), reachable.Keys);
    }

    [Fact]
    public void SafeRoute_AvoidsThreatenedDirectCorridorWhenDetourIsCheaperTactically()
    {
        var grid = CreateGrid(5, 3);
        var start = new GridPoint(0, 1);
        var goal = new GridPoint(4, 1);
        var threats = new HashSet<GridPoint> { new(1, 1), new(2, 1), new(3, 1) };

        var route = grid.FindTacticalRoute(start, goal, [], 12, null, Options(intent: RouteIntent.Safe, threats: threats, threatPenalty: 3));

        Assert.NotNull(route);
        Assert.DoesNotContain(route!.Path, threats.Contains);
        Assert.Equal(0, route.ThreatenedSteps);
        Assert.True(route.TravelCost > 4);
    }

    [Fact]
    public void FastestRoute_IgnoresSafetyWeightsAndUsesDirectCorridor()
    {
        var grid = CreateGrid(5, 3);
        var threats = new HashSet<GridPoint> { new(1, 1), new(2, 1), new(3, 1) };

        var route = grid.FindTacticalRoute(new GridPoint(0, 1), new GridPoint(4, 1), [], 8, null, Options(intent: RouteIntent.Fastest, threats: threats, threatPenalty: 8));

        Assert.NotNull(route);
        Assert.Equal(4, route!.TravelCost);
        Assert.Equal(3, route.ThreatenedSteps);
        Assert.All(route.Path, point => Assert.Equal(1, point.Y));
    }

    [Fact]
    public void ReservationPenalty_ReroutesAroundAnAlliedReservedTile()
    {
        var grid = CreateGrid(5, 3);
        var reservedPoint = new GridPoint(2, 1);
        var reservations = new Dictionary<GridPoint, string> { [reservedPoint] = "ally-a" };

        var route = grid.FindTacticalRoute(new GridPoint(0, 1), new GridPoint(4, 1), [], 12, "hero-b", Options(intent: RouteIntent.Safe, reservations: reservations, reservationOwner: "hero-b", reservationPenalty: 5));

        Assert.NotNull(route);
        Assert.DoesNotContain(reservedPoint, route!.Path);
        Assert.False(route.UsesReservation);
    }

    [Fact]
    public void OwnerReservation_DoesNotPenalizeOwnMarkedDestination()
    {
        var grid = CreateGrid(3, 1);
        var target = new GridPoint(2, 0);
        var reservations = new Dictionary<GridPoint, string> { [target] = "hero-a" };

        var route = grid.FindTacticalRoute(new GridPoint(0, 0), target, [], 2, "hero-a", Options(reservations: reservations, reservationOwner: "hero-a", reservationPenalty: 8));

        Assert.NotNull(route);
        Assert.Equal(2, route!.TacticalCost);
        Assert.False(route.UsesReservation);
    }

    [Fact]
    public void BestApproach_TargetsAnOpenTileAdjacentToOccupiedEnemy()
    {
        var grid = CreateGrid(5, 3);
        var enemy = Unit("enemy", new GridPoint(4, 1));

        var route = grid.FindBestApproach(new GridPoint(0, 1), enemy.Position, [enemy], 8, null, Options());

        Assert.NotNull(route);
        Assert.NotEqual(enemy.Position, route!.Path[^1]);
        Assert.Equal(1, route.Path[^1].ManhattanDistance(enemy.Position));
    }

    [Fact]
    public void FlankAnchors_ReturnReachableOppositeSidePositions()
    {
        var grid = CreateGrid(5, 3);
        var target = new GridPoint(2, 1);
        var enemy = Unit("enemy", target);

        var anchors = grid.FindFlankAnchors(new GridPoint(0, 1), target, [enemy], 8, null, Options(intent: RouteIntent.Flank));

        Assert.Contains(new GridPoint(3, 1), anchors);
        Assert.DoesNotContain(target, anchors);
    }

    [Fact]
    public void RouteAnalysis_ReportsCoverHighGroundAndThreatDiagnostics()
    {
        var grid = CreateGrid(3, 1, tiles =>
        {
            var cover = TileAt(tiles, 1, 0);
            cover.CoverValue = 1;
            var highGround = TileAt(tiles, 2, 0);
            highGround.IsHighGround = true;
            highGround.Elevation = 1;
        });
        var threats = new HashSet<GridPoint> { new(1, 0) };

        var route = grid.FindTacticalRoute(new GridPoint(0, 0), new GridPoint(2, 0), [], 8, null, Options(intent: RouteIntent.Direct, threats: threats));

        Assert.NotNull(route);
        Assert.Equal(1, route!.ThreatenedSteps);
        Assert.Equal(1, route.CoverStops);
        Assert.True(route.ReachesHighGround);
    }

    [Fact]
    public void BuildReservations_UsesFirstLivingUnitForSharedDestination()
    {
        var grid = CreateGrid(3, 1);
        var first = Unit("first", new GridPoint(0, 0), reserved: new GridPoint(2, 0));
        var second = Unit("second", new GridPoint(1, 0), reserved: new GridPoint(2, 0));
        var fallen = Unit("fallen", new GridPoint(1, 0), health: 0, reserved: new GridPoint(0, 0));

        var reservations = grid.BuildReservations([first, second, fallen]);

        Assert.Equal("first", reservations[new GridPoint(2, 0)]);
        Assert.DoesNotContain(new GridPoint(0, 0), reservations.Keys);
    }

    [Fact]
    public void SavedReplay_RoundTripsAndReproducesCanonicalFinalFingerprint()
    {
        var recorded = RecordOneRound("replay-roundtrip");
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var saved = JsonSerializer.Serialize(recorded, options);
        var restored = JsonSerializer.Deserialize<ReplayRecord>(saved, options);

        Assert.NotNull(restored);
        var first = new ReplayPlayer(restored!, BuildEncounter, NoAbilities);
        var second = new ReplayPlayer(restored!, BuildEncounter, NoAbilities);
        PlayAll(first);
        PlayAll(second);

        Assert.False(first.IsInvalid);
        Assert.False(second.IsInvalid);
        Assert.True(first.IsComplete);
        Assert.Equal(first.Fingerprint, second.Fingerprint);
        Assert.Equal(ReplayFingerprint.Create(ExecuteReference(restored!), restored), first.Fingerprint);
    }

    [Fact]
    public void ReplayPlayback_IsRepeatableAcrossIndependentSavedEncounterInstances()
    {
        var record = RecordOneRound("replay-repeatable");
        var first = new ReplayPlayer(record, BuildEncounter, NoAbilities);
        var second = new ReplayPlayer(record, BuildEncounter, NoAbilities);

        var firstStates = CapturePlaybackFingerprints(first);
        var secondStates = CapturePlaybackFingerprints(second);

        Assert.Equal(firstStates, secondStates);
        Assert.Equal(record.Actions.Count, first.CurrentActionIndex);
        Assert.Equal(GamePhase.Player, first.Session.Phase);
    }

    [Fact]
    public void ReplayReset_RestoresTheExactSeededInitialFingerprint()
    {
        var record = RecordOneRound("replay-reset");
        var player = new ReplayPlayer(record, BuildEncounter, NoAbilities);
        var initial = player.Fingerprint;

        Assert.True(player.Step());
        Assert.NotEqual(initial, player.Fingerprint);
        player.Reset();

        Assert.Equal(0, player.CurrentActionIndex);
        Assert.False(player.IsInvalid);
        Assert.Equal(initial, player.Fingerprint);
    }

    [Fact]
    public void ReplayRejectsOutOfOrderEnemyActionWithoutAdvancingPlayback()
    {
        var record = new ReplayRecord(1, "saved-invalid", "replay-invalid", GameMode.Training, Difficulty.Field, DateTimeOffset.UnixEpoch,
            [new TacticalAction(1, "enemy-scout", "enemy", new GridPoint(2, 0), null, "invalid early enemy")], null);
        var player = new ReplayPlayer(record, BuildEncounter, NoAbilities);
        var initial = player.Fingerprint;

        Assert.False(player.Step());
        Assert.True(player.IsInvalid);
        Assert.Equal(0, player.CurrentActionIndex);
        Assert.Equal(initial, player.Fingerprint);
        Assert.Contains("expected an enemy phase", player.LastError);
    }

    [Fact]
    public void ReplayFingerprint_ChangesWhenSavedEncounterSeedChanges()
    {
        var first = new ReplayPlayer(RecordOneRound("field-alpha"), BuildEncounter, NoAbilities);
        var second = new ReplayPlayer(RecordOneRound("field-beta"), BuildEncounter, NoAbilities);

        Assert.NotEqual(first.Fingerprint, second.Fingerprint);
    }

    private static TacticalGrid CreateGrid(int width, int height, Action<List<Tile>>? configure = null)
    {
        var tiles = Enumerable.Range(0, height)
            .SelectMany(y => Enumerable.Range(0, width).Select(x => new Tile { Position = new GridPoint(x, y) }))
            .ToList();
        configure?.Invoke(tiles);
        return new TacticalGrid(width, height, tiles);
    }

    private static Tile TileAt(IEnumerable<Tile> tiles, int x, int y) => tiles.Single(tile => tile.Position == new GridPoint(x, y));

    private static UnitState Unit(string id, GridPoint position, MobilityProfile mobility = MobilityProfile.Standard, int health = 10, GridPoint? reserved = null, Faction faction = Faction.Hero) => new()
    {
        Id = id,
        Position = position,
        Health = health,
        Energy = 3,
        ReservedDestination = reserved,
        Template = new UnitTemplate
        {
            Id = $"template-{id}",
            Name = id,
            Title = "Test Unit",
            Faction = faction,
            Archetype = "Test",
            MaxHealth = 10,
            Attack = 3,
            Defense = 1,
            Movement = 4,
            Energy = 3,
            Element = ElementKind.None,
            Passive = "Test",
            Ultimate = "Test",
            Color = "#FFFFFF",
            Mobility = mobility,
            TacticalClass = TacticalClass.Vanguard
        }
    };

    private static RouteOptions Options(
        MobilityProfile mobility = MobilityProfile.Standard,
        RouteIntent intent = RouteIntent.Direct,
        IReadOnlySet<GridPoint>? threats = null,
        IReadOnlyDictionary<GridPoint, string>? reservations = null,
        string? reservationOwner = null,
        int threatPenalty = 2,
        int reservationPenalty = 4) => new()
        {
            Mobility = mobility,
            Intent = intent,
            ThreatenedTiles = threats ?? new HashSet<GridPoint>(),
            Reservations = reservations ?? new Dictionary<GridPoint, string>(),
            ReservationOwnerId = reservationOwner,
            ThreatPenalty = threatPenalty,
            ReservationPenalty = reservationPenalty
        };

    private static readonly IReadOnlyDictionary<string, AbilityDefinition> NoAbilities = new Dictionary<string, AbilityDefinition>();

    private static ReplayRecord RecordOneRound(string seed)
    {
        var session = ReplayReference(new ReplayRecord(1, $"training-{seed}", seed, GameMode.Training, Difficulty.Field, DateTimeOffset.UnixEpoch, [], null));
        Assert.True(session.SelectUnit("hero-scout"));
        Assert.True(session.ChooseTile(new GridPoint(1, 0)));
        Assert.True(session.EndTurn());
        while (session.Phase == GamePhase.Enemy) session.ResolveNextEnemy();
        var record = session.CreateReplay();
        Assert.Equal(new[] { "move", "end-turn", "enemy" }, record.Actions.Select(action => action.Type));
        return record;
    }

    private static GameSession ReplayReference(ReplayRecord record)
    {
        var session = new GameSession(BuildEncounter(record.Seed, record.Mode, record.Difficulty), NoAbilities);
        session.Start();
        return session;
    }

    private static GameSession ExecuteReference(ReplayRecord record)
    {
        var session = ReplayReference(record);
        Assert.True(session.SelectUnit("hero-scout"));
        Assert.True(session.ChooseTile(new GridPoint(1, 0)));
        Assert.True(session.EndTurn());
        while (session.Phase == GamePhase.Enemy) session.ResolveNextEnemy();
        return session;
    }

    private static EncounterState BuildEncounter(string seed, GameMode mode, Difficulty difficulty)
    {
        var random = new DeterministicRandom(seed);
        var grid = CreateGrid(5, 2, tiles =>
        {
            TileAt(tiles, 2, 1).Kind = random.Chance(0.5f) ? TileKind.Difficult : TileKind.Hazard;
            TileAt(tiles, 3, 0).CoverValue = 1;
            TileAt(tiles, 4, 1).IsHighGround = random.Chance(0.5f);
        });
        return new EncounterState
        {
            Id = $"{mode}-{seed}",
            Seed = seed,
            Mode = mode,
            Difficulty = difficulty,
            Title = "Replay Test Field",
            Briefing = "Deterministic replay fixture.",
            Objective = "Complete a recorded turn.",
            Grid = grid,
            Units = [
                Unit("hero-scout", new GridPoint(0, 0), mobility: MobilityProfile.Trailblazer),
                Unit("enemy-scout", new GridPoint(4, 0), mobility: MobilityProfile.Standard, faction: Faction.Enemy)
            ],
            RewardShards = 1,
            RewardMastery = 1
        };
    }

    private static void PlayAll(ReplayPlayer player)
    {
        while (!player.IsComplete && !player.IsInvalid) Assert.True(player.Step());
    }

    private static IReadOnlyList<string> CapturePlaybackFingerprints(ReplayPlayer player)
    {
        var fingerprints = new List<string> { player.Fingerprint };
        while (!player.IsComplete && !player.IsInvalid)
        {
            Assert.True(player.Step());
            fingerprints.Add(player.Fingerprint);
        }
        Assert.False(player.IsInvalid);
        return fingerprints;
    }
}
