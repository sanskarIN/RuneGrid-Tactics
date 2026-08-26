namespace RuneGrid.Tactics.Core;

public enum ReplayDiffCategory { All, Phase, Tile, Unit, Action }

public sealed record ReplayDiffEntry(ReplayDiffCategory Category, string Line, GridPoint? Tile, string? UnitId);

public sealed record ReplayDiffFilterResult(ReplayDiffCategory Category, IReadOnlyList<ReplayDiffEntry> Entries, IReadOnlySet<GridPoint> AffectedTiles, IReadOnlySet<string> AffectedUnitIds)
{
    public bool HasEntries => Entries.Count > 0;

    public string ToHumanReadable()
    {
        if (!HasEntries) return Category == ReplayDiffCategory.All ? "Replay states match exactly." : $"No {ReplayDiffFilter.LabelFor(Category).ToLowerInvariant()} differences are present.";
        return string.Join(Environment.NewLine, Entries.Select(entry => $"{ReplayDiffFilter.LabelFor(entry.Category).ToUpperInvariant()} · {entry.Line}"));
    }
}

/// <summary>Classifies stable replay diff lines and extracts entity markers for filtered inspection.</summary>
public static class ReplayDiffFilter
{
    public static ReplayDiffFilterResult Filter(ReplayStateDiff difference, ReplayDiffCategory category)
    {
        var entries = difference.Lines.Select(Classify).Where(entry => category == ReplayDiffCategory.All || entry.Category == category).ToList();
        var tiles = entries.Where(entry => entry.Tile is not null).Select(entry => entry.Tile!.Value).ToHashSet();
        var units = entries.Where(entry => !string.IsNullOrWhiteSpace(entry.UnitId)).Select(entry => entry.UnitId!).ToHashSet(StringComparer.Ordinal);
        return new ReplayDiffFilterResult(category, entries, tiles, units);
    }

    public static ReplayDiffEntry Classify(string line)
    {
        if (line.StartsWith("tile ", StringComparison.Ordinal)) return new ReplayDiffEntry(ReplayDiffCategory.Tile, line, TryParseTile(line), null);
        if (line.StartsWith("unit ", StringComparison.Ordinal)) return new ReplayDiffEntry(ReplayDiffCategory.Unit, line, null, ParseUnitId(line));
        if (line.StartsWith("action[", StringComparison.Ordinal)) return new ReplayDiffEntry(ReplayDiffCategory.Action, line, null, null);
        return new ReplayDiffEntry(ReplayDiffCategory.Phase, line, null, null);
    }

    public static string LabelFor(ReplayDiffCategory category) => category switch
    {
        ReplayDiffCategory.All => "All",
        ReplayDiffCategory.Phase => "Phase / state",
        ReplayDiffCategory.Tile => "Tile",
        ReplayDiffCategory.Unit => "Unit",
        ReplayDiffCategory.Action => "Action",
        _ => "Difference"
    };

    private static GridPoint? TryParseTile(string line)
    {
        const string prefix = "tile ";
        var end = line.IndexOf(": ", prefix.Length, StringComparison.Ordinal);
        if (end <= prefix.Length) return null;
        var parts = line[prefix.Length..end].Split(':');
        return parts.Length == 2 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y) ? new GridPoint(x, y) : null;
    }

    private static string? ParseUnitId(string line)
    {
        const string prefix = "unit ";
        var end = line.IndexOf(": ", prefix.Length, StringComparison.Ordinal);
        return end > prefix.Length ? line[prefix.Length..end] : null;
    }
}
