namespace RuneGrid.Tactics.Core;

public enum ReplayDiffCategory { All, Phase, Tile, Unit, Action }

public enum ReplayDiffFilterShortcut { Previous, Next }

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

/// <summary>Reserved, non-configurable navigation for the currently focused replay diff category.</summary>
public static class ReplayDiffFilterNavigator
{
    private static readonly ReplayDiffCategory[] Categories = Enum.GetValues<ReplayDiffCategory>();

    public static ReplayDiffCategory Previous(ReplayDiffCategory current) => Cycle(current, -1);

    public static ReplayDiffCategory Next(ReplayDiffCategory current) => Cycle(current, 1);

    public static bool TryParseShortcut(string? key, bool hasModifier, out ReplayDiffFilterShortcut shortcut)
    {
        shortcut = default;
        if (hasModifier) return false;
        if (string.Equals(key, "F2", StringComparison.OrdinalIgnoreCase)) { shortcut = ReplayDiffFilterShortcut.Previous; return true; }
        if (string.Equals(key, "F3", StringComparison.OrdinalIgnoreCase)) { shortcut = ReplayDiffFilterShortcut.Next; return true; }
        return false;
    }

    private static ReplayDiffCategory Cycle(ReplayDiffCategory current, int offset)
    {
        var currentIndex = Array.IndexOf(Categories, current);
        if (currentIndex < 0) currentIndex = 0;
        return Categories[(currentIndex + offset + Categories.Length) % Categories.Length];
    }
}

public sealed record ReplayDiffFilterShortcutReferenceLine(string Command, string Binding, string Description);

public static class ReplayDiffFilterShortcutReference
{
    public static IReadOnlyList<ReplayDiffFilterShortcutReferenceLine> Build() =>
    [
        new("Previous diff filter", "F2", "Move to the prior focused mismatch category."),
        new("Next diff filter", "F3", "Move to the next focused mismatch category.")
    ];
}
