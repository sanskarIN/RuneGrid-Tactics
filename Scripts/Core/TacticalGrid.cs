namespace RuneGrid.Tactics.Core;

public sealed class TacticalGrid
{
    private readonly Dictionary<GridPoint, Tile> _tiles;

    public int Width { get; }
    public int Height { get; }
    public IEnumerable<Tile> Tiles => _tiles.Values;

    public TacticalGrid(int width, int height, IEnumerable<Tile> tiles)
    {
        Width = width;
        Height = height;
        _tiles = tiles.ToDictionary(tile => tile.Position);
    }

    public bool InBounds(GridPoint point) => point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height;
    public Tile? Get(GridPoint point) => _tiles.GetValueOrDefault(point);
    public bool IsWalkable(GridPoint point, MobilityProfile mobility = MobilityProfile.Standard)
    {
        var tile = Get(point);
        if (tile is null || tile.Kind == TileKind.Gate) return false;
        if (tile.Kind == TileKind.Wall) return mobility == MobilityProfile.Phasewalker;
        return true;
    }

    public bool CanOccupy(GridPoint point) => Get(point) is { Kind: not TileKind.Wall and not TileKind.Gate };

    public int MovementCost(GridPoint point, MobilityProfile mobility = MobilityProfile.Standard)
    {
        var tile = Get(point);
        if (tile is null || !IsWalkable(point, mobility)) return int.MaxValue;
        if (mobility == MobilityProfile.Winged && (tile.Kind is TileKind.Difficult or TileKind.Hazard)) return 1;
        if ((mobility is MobilityProfile.Trailblazer or MobilityProfile.Skirmisher) && tile.Kind == TileKind.Difficult) return 1;
        if (mobility == MobilityProfile.Juggernaut && tile.Kind == TileKind.Difficult) return 3;
        return tile.Kind switch
        {
            TileKind.Difficult => 2,
            TileKind.Hazard => 2,
            _ => 1
        };
    }

    public IEnumerable<GridPoint> Neighbors(GridPoint point)
    {
        var candidates = new[]
        {
            new GridPoint(point.X + 1, point.Y), new GridPoint(point.X - 1, point.Y),
            new GridPoint(point.X, point.Y + 1), new GridPoint(point.X, point.Y - 1)
        };
        return candidates.Where(InBounds);
    }

    public bool IsOccupied(GridPoint point, IEnumerable<UnitState> units, string? ignoredUnitId = null) =>
        units.Any(unit => unit.IsAlive && unit.Id != ignoredUnitId && unit.Position == point);

    public IReadOnlyDictionary<GridPoint, string> BuildReservations(IEnumerable<UnitState> units) =>
        units.Where(unit => unit.IsAlive && unit.ReservedDestination is not null)
            .GroupBy(unit => unit.ReservedDestination!.Value)
            .ToDictionary(group => group.Key, group => group.First().Id);

    public int CoverAt(GridPoint point) => Get(point)?.CoverValue ?? 0;

    public bool IsHighGround(GridPoint point) => Get(point)?.IsHighGround ?? false;

    public IReadOnlyDictionary<GridPoint, int> Reachable(GridPoint start, int allowance, IReadOnlyList<UnitState> units, string? ignoredUnitId = null, RouteOptions? options = null)
    {
        options ??= new RouteOptions();
        var frontier = new PriorityQueue<GridPoint, int>();
        var costs = new Dictionary<GridPoint, int> { [start] = 0 };
        frontier.Enqueue(start, 0);
        while (frontier.TryDequeue(out var current, out var currentCost))
        {
            foreach (var next in Neighbors(current))
            {
                if (!IsWalkable(next, options.Mobility) || IsOccupied(next, units, ignoredUnitId))
                    continue;
                var cost = currentCost + MovementCost(next, options.Mobility);
                if (options.Reservations.TryGetValue(next, out var owner) && owner != options.ReservationOwnerId) cost += options.ReservationPenalty;
                if (cost > allowance || (costs.TryGetValue(next, out var known) && known <= cost))
                    continue;
                costs[next] = cost;
                frontier.Enqueue(next, cost);
            }
        }
        costs.Remove(start);
        foreach (var wall in costs.Keys.Where(point => Get(point)?.Kind == TileKind.Wall).ToList()) costs.Remove(wall);
        return costs;
    }

    public IReadOnlyList<GridPoint>? FindPath(GridPoint start, GridPoint goal, IReadOnlyList<UnitState> units, int allowance = int.MaxValue, string? ignoredUnitId = null) =>
        FindTacticalRoute(start, goal, units, allowance, ignoredUnitId, new RouteOptions())?.Path;

    public RouteAnalysis? FindTacticalRoute(GridPoint start, GridPoint goal, IReadOnlyList<UnitState> units, int allowance, string? ignoredUnitId, RouteOptions options)
    {
        if (!InBounds(goal) || !CanOccupy(goal) || IsOccupied(goal, units, ignoredUnitId))
            return null;

        var frontier = new PriorityQueue<GridPoint, int>();
        var costs = new Dictionary<GridPoint, int> { [start] = 0 };
        var travelCosts = new Dictionary<GridPoint, int> { [start] = 0 };
        var previous = new Dictionary<GridPoint, GridPoint>();
        frontier.Enqueue(start, start.ManhattanDistance(goal));

        while (frontier.TryDequeue(out var current, out _))
        {
            if (current == goal)
            {
                var path = new List<GridPoint>();
                for (var cursor = goal; cursor != start; cursor = previous[cursor])
                    path.Insert(0, cursor);
                var threatened = path.Count(point => options.ThreatenedTiles.Contains(point));
                var cover = path.Count(point => CoverAt(point) > 0);
                var reserved = path.Any(point => options.Reservations.TryGetValue(point, out var owner) && owner != options.ReservationOwnerId);
                return costs[goal] <= allowance
                    ? new RouteAnalysis(path, travelCosts[goal], costs[goal], threatened, cover, reserved, IsHighGround(goal))
                    : null;
            }

            foreach (var next in Neighbors(current))
            {
                if (!IsWalkable(next, options.Mobility) || IsOccupied(next, units, ignoredUnitId))
                    continue;
                var movement = MovementCost(next, options.Mobility);
                var tacticalAdjustment = 0;
                if (options.Intent == RouteIntent.Safe && options.ThreatenedTiles.Contains(next)) tacticalAdjustment += options.ThreatPenalty;
                if (options.Intent == RouteIntent.Safe && Get(next)?.Kind == TileKind.Hazard) tacticalAdjustment += options.HazardPenalty;
                if (options.Intent == RouteIntent.Safe) tacticalAdjustment -= CoverAt(next) * options.CoverReward;
                if (options.Reservations.TryGetValue(next, out var owner) && owner != options.ReservationOwnerId) tacticalAdjustment += options.ReservationPenalty;
                if (options.Intent == RouteIntent.Fastest) tacticalAdjustment = 0;
                var nextCost = costs[current] + Math.Max(1, movement + tacticalAdjustment);
                if (nextCost > allowance || (costs.TryGetValue(next, out var known) && known <= nextCost))
                    continue;
                costs[next] = nextCost;
                travelCosts[next] = travelCosts[current] + movement;
                previous[next] = current;
                frontier.Enqueue(next, nextCost + next.ManhattanDistance(goal));
            }
        }
        return null;
    }

    public RouteAnalysis? FindBestApproach(GridPoint start, GridPoint occupiedGoal, IReadOnlyList<UnitState> units, int allowance, string? ignoredUnitId, RouteOptions options)
    {
        var candidates = Neighbors(occupiedGoal).Where(point => CanOccupy(point) && !IsOccupied(point, units, ignoredUnitId));
        var routes = candidates.Select(point => FindTacticalRoute(start, point, units, allowance, ignoredUnitId, options)).Where(route => route is not null).Cast<RouteAnalysis>();
        return options.Intent == RouteIntent.Safe
            ? routes.OrderBy(route => route.TacticalCost).ThenBy(route => route.ThreatenedSteps).FirstOrDefault()
            : routes.OrderBy(route => route.TravelCost).ThenBy(route => route.TacticalCost).FirstOrDefault();
    }

    public IReadOnlySet<GridPoint> FindFlankAnchors(GridPoint start, GridPoint target, IReadOnlyList<UnitState> units, int allowance, string? ignoredUnitId, RouteOptions options)
    {
        var anchors = new HashSet<GridPoint>();
        foreach (var candidate in Neighbors(target))
        {
            if (!CanOccupy(candidate) || IsOccupied(candidate, units, ignoredUnitId)) continue;
            var onOppositeAxis = (candidate.X - target.X) * (start.X - target.X) < 0 || (candidate.Y - target.Y) * (start.Y - target.Y) < 0;
            if (!onOppositeAxis) continue;
            var flankOptions = new RouteOptions
            {
                Mobility = options.Mobility,
                Intent = RouteIntent.Flank,
                HazardPenalty = options.HazardPenalty,
                ThreatPenalty = options.ThreatPenalty,
                ReservationPenalty = options.ReservationPenalty,
                CoverReward = options.CoverReward,
                ThreatenedTiles = options.ThreatenedTiles,
                Reservations = options.Reservations,
                ReservationOwnerId = options.ReservationOwnerId
            };
            var route = FindTacticalRoute(start, candidate, units, allowance, ignoredUnitId, flankOptions);
            if (route is not null) anchors.Add(candidate);
        }
        return anchors;
    }

    public bool HasLineOfSight(GridPoint start, GridPoint end)
    {
        var dx = Math.Abs(end.X - start.X);
        var dy = Math.Abs(end.Y - start.Y);
        var stepX = start.X < end.X ? 1 : -1;
        var stepY = start.Y < end.Y ? 1 : -1;
        var error = dx - dy;
        var x = start.X;
        var y = start.Y;

        while (x != end.X || y != end.Y)
        {
            if (x != start.X || y != start.Y)
            {
                var kind = Get(new GridPoint(x, y))?.Kind;
                if (kind is TileKind.Wall or TileKind.Gate)
                    return false;
            }
            var doubled = error * 2;
            if (doubled > -dy) { error -= dy; x += stepX; }
            if (doubled < dx) { error += dx; y += stepY; }
        }
        return true;
    }

    public IEnumerable<GridPoint> PointsInRange(GridPoint origin, int range) =>
        _tiles.Keys.Where(point => origin.ManhattanDistance(point) <= range);
}
