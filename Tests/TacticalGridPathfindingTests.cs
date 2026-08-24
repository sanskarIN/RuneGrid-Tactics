using RuneGrid.Tactics.Core;
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

    private static TacticalGrid CreateGrid(int width, int height, Action<List<Tile>>? configure = null)
    {
        var tiles = Enumerable.Range(0, height)
            .SelectMany(y => Enumerable.Range(0, width).Select(x => new Tile { Position = new GridPoint(x, y) }))
            .ToList();
        configure?.Invoke(tiles);
        return new TacticalGrid(width, height, tiles);
    }

    private static Tile TileAt(IEnumerable<Tile> tiles, int x, int y) => tiles.Single(tile => tile.Position == new GridPoint(x, y));

    private static UnitState Unit(string id, GridPoint position, MobilityProfile mobility = MobilityProfile.Standard, int health = 10, GridPoint? reserved = null) => new()
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
            Faction = Faction.Hero,
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
}
