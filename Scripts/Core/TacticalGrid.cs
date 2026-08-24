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
    public bool IsWalkable(GridPoint point) => Get(point) is { Kind: not TileKind.Wall and not TileKind.Gate };

    public int MovementCost(GridPoint point) => Get(point)?.Kind switch
    {
        TileKind.Difficult => 2,
        TileKind.Wall or TileKind.Gate => int.MaxValue,
        _ => 1
    };

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

    public IReadOnlyDictionary<GridPoint, int> Reachable(GridPoint start, int allowance, IReadOnlyList<UnitState> units, string? ignoredUnitId = null)
    {
        var frontier = new PriorityQueue<GridPoint, int>();
        var costs = new Dictionary<GridPoint, int> { [start] = 0 };
        frontier.Enqueue(start, 0);
        while (frontier.TryDequeue(out var current, out var currentCost))
        {
            foreach (var next in Neighbors(current))
            {
                if (!IsWalkable(next) || IsOccupied(next, units, ignoredUnitId))
                    continue;
                var cost = currentCost + MovementCost(next);
                if (cost > allowance || (costs.TryGetValue(next, out var known) && known <= cost))
                    continue;
                costs[next] = cost;
                frontier.Enqueue(next, cost);
            }
        }
        costs.Remove(start);
        return costs;
    }

    public IReadOnlyList<GridPoint>? FindPath(GridPoint start, GridPoint goal, IReadOnlyList<UnitState> units, int allowance = int.MaxValue, string? ignoredUnitId = null)
    {
        if (!InBounds(goal) || !IsWalkable(goal) || IsOccupied(goal, units, ignoredUnitId))
            return null;

        var frontier = new PriorityQueue<GridPoint, int>();
        var costs = new Dictionary<GridPoint, int> { [start] = 0 };
        var previous = new Dictionary<GridPoint, GridPoint>();
        frontier.Enqueue(start, start.ManhattanDistance(goal));

        while (frontier.TryDequeue(out var current, out _))
        {
            if (current == goal)
            {
                var path = new List<GridPoint>();
                for (var cursor = goal; cursor != start; cursor = previous[cursor])
                    path.Insert(0, cursor);
                return costs[goal] <= allowance ? path : null;
            }

            foreach (var next in Neighbors(current))
            {
                if (!IsWalkable(next) || IsOccupied(next, units, ignoredUnitId))
                    continue;
                var nextCost = costs[current] + MovementCost(next);
                if (nextCost > allowance || (costs.TryGetValue(next, out var known) && known <= nextCost))
                    continue;
                costs[next] = nextCost;
                previous[next] = current;
                frontier.Enqueue(next, nextCost + next.ManhattanDistance(goal));
            }
        }
        return null;
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
