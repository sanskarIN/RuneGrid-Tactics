using System.Text;

namespace RuneGrid.Tactics.Core;

/// <summary>Order-stable replay state data used for fingerprints and actionable mismatch diagnostics.</summary>
public sealed record ReplayStateSnapshot(
    string EncounterId,
    string Seed,
    GameMode Mode,
    Difficulty Difficulty,
    GamePhase Phase,
    int Turn,
    int SchemaVersion,
    IReadOnlyDictionary<string, string> Tiles,
    IReadOnlyDictionary<string, string> Units,
    IReadOnlyList<string> Actions)
{
    public static ReplayStateSnapshot Capture(GameSession session, ReplayRecord? record = null)
    {
        var encounter = session.Encounter;
        var tiles = encounter.Grid.Tiles
            .OrderBy(tile => tile.Position.Y).ThenBy(tile => tile.Position.X)
            .ToDictionary(tile => tile.Position.ToString(), DescribeTile);
        var units = encounter.Units
            .OrderBy(unit => unit.Id, StringComparer.Ordinal)
            .ToDictionary(unit => unit.Id, DescribeUnit, StringComparer.Ordinal);
        var actions = session.Actions.Select(DescribeAction).ToList();
        return new ReplayStateSnapshot(encounter.Id, encounter.Seed, encounter.Mode, encounter.Difficulty, session.Phase, session.Turn, record?.SchemaVersion ?? 0, tiles, units, actions);
    }

    public string CanonicalPayload()
    {
        var payload = new StringBuilder();
        payload.Append(EncounterId).Append('|').Append(Seed).Append('|').Append(Mode).Append('|').Append(Difficulty).Append('|').Append(Phase).Append('|').Append(Turn).Append('|').Append(SchemaVersion);
        foreach (var tile in Tiles.OrderBy(entry => entry.Key, StringComparer.Ordinal)) payload.Append("|t:").Append(tile.Key).Append('=').Append(tile.Value);
        foreach (var unit in Units.OrderBy(entry => entry.Key, StringComparer.Ordinal)) payload.Append("|u:").Append(unit.Key).Append('=').Append(unit.Value);
        foreach (var action in Actions) payload.Append("|a:").Append(action);
        return payload.ToString();
    }

    public string Fingerprint => DeterministicRandom.Hash(CanonicalPayload()).ToString("X8");

    private static string DescribeTile(Tile tile) => $"kind={tile.Kind}; elevation={tile.Elevation}; integrity={tile.Integrity?.ToString() ?? "none"}; link={tile.LinkedTo?.ToString() ?? "none"}; cover={tile.CoverValue}; highGround={tile.IsHighGround}";

    private static string DescribeUnit(UnitState unit)
    {
        var payload = new StringBuilder($"template={unit.Template.Id}; faction={unit.Faction}; position={unit.Position}; health={unit.Health}; energy={unit.Energy}; shield={unit.Shield}; moved={unit.Moved}; acted={unit.Acted}; reservation={unit.ReservedDestination?.ToString() ?? "none"}");
        foreach (var cooldown in unit.Cooldowns.OrderBy(entry => entry.Key, StringComparer.Ordinal)) payload.Append($"; cooldown.{cooldown.Key}={cooldown.Value}");
        foreach (var status in unit.Statuses.OrderBy(entry => entry.Key, StringComparer.Ordinal)) payload.Append($"; status.{status.Key}={status.Value}");
        return payload.ToString();
    }

    private static string DescribeAction(TacticalAction action) => $"turn={action.Turn}; actor={action.ActorId}; type={action.Type}; target={action.Target?.ToString() ?? "none"}; ability={action.AbilityId ?? "none"}";
}

public sealed record ReplayStateDiff(IReadOnlyList<string> Lines)
{
    public bool IsMatch => Lines.Count == 0;

    public string ToHumanReadable()
    {
        if (IsMatch) return "Replay states match exactly.";
        return $"Replay state mismatch ({Lines.Count} difference{(Lines.Count == 1 ? string.Empty : "s")}):{Environment.NewLine}" + string.Join(Environment.NewLine, Lines.Select(line => $" - {line}"));
    }
}

/// <summary>Compares canonical replay snapshots without depending on frame timing or render state.</summary>
public static class ReplayStateDiffGenerator
{
    public static ReplayStateDiff Compare(ReplayStateSnapshot expected, ReplayStateSnapshot actual)
    {
        var lines = new List<string>();
        CompareValue("encounter id", expected.EncounterId, actual.EncounterId, lines);
        CompareValue("seed", expected.Seed, actual.Seed, lines);
        CompareValue("mode", expected.Mode, actual.Mode, lines);
        CompareValue("difficulty", expected.Difficulty, actual.Difficulty, lines);
        CompareValue("phase", expected.Phase, actual.Phase, lines);
        CompareValue("turn", expected.Turn, actual.Turn, lines);
        CompareValue("replay schema", expected.SchemaVersion, actual.SchemaVersion, lines);
        CompareMap("tile", expected.Tiles, actual.Tiles, lines);
        CompareMap("unit", expected.Units, actual.Units, lines);
        CompareActions(expected.Actions, actual.Actions, lines);
        return new ReplayStateDiff(lines);
    }

    private static void CompareValue<T>(string label, T expected, T actual, ICollection<string> lines) where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual)) lines.Add($"{label}: expected {expected}, actual {actual}");
    }

    private static void CompareMap(string label, IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual, ICollection<string> lines)
    {
        foreach (var key in expected.Keys.Union(actual.Keys).OrderBy(key => key, StringComparer.Ordinal))
        {
            var hasExpected = expected.TryGetValue(key, out var expectedValue);
            var hasActual = actual.TryGetValue(key, out var actualValue);
            if (!hasExpected) lines.Add($"{label} {key}: unexpected actual state {actualValue}");
            else if (!hasActual) lines.Add($"{label} {key}: missing actual state; expected {expectedValue}");
            else if (!string.Equals(expectedValue, actualValue, StringComparison.Ordinal)) lines.Add($"{label} {key}: expected [{expectedValue}], actual [{actualValue}]");
        }
    }

    private static void CompareActions(IReadOnlyList<string> expected, IReadOnlyList<string> actual, ICollection<string> lines)
    {
        var maximum = Math.Max(expected.Count, actual.Count);
        for (var index = 0; index < maximum; index++)
        {
            var expectedValue = index < expected.Count ? expected[index] : "<none>";
            var actualValue = index < actual.Count ? actual[index] : "<none>";
            if (!string.Equals(expectedValue, actualValue, StringComparison.Ordinal)) lines.Add($"action[{index}]: expected [{expectedValue}], actual [{actualValue}]");
        }
    }
}
