namespace RuneGrid.Tactics.Core;

/// <summary>Command-table-facing replay inspection model with deterministic expected-state reconstruction.</summary>
public sealed class ReplayInspector
{
    private readonly ReplayRecord _record;
    private readonly Func<string, GameMode, Difficulty, EncounterState> _buildEncounter;
    private readonly IReadOnlyDictionary<string, AbilityDefinition> _abilities;
    private readonly ReplayStateSnapshot _initial;

    public ReplayPlayer Player { get; }
    public event Action? Changed;

    public ReplayInspector(ReplayRecord record, Func<string, GameMode, Difficulty, EncounterState> buildEncounter, IReadOnlyDictionary<string, AbilityDefinition> abilities)
    {
        _record = record;
        _buildEncounter = buildEncounter;
        _abilities = abilities;
        Player = new ReplayPlayer(record, buildEncounter, abilities);
        _initial = ReplayStateSnapshot.Capture(Player.Session, record);
    }

    public ReplayInspectorReport BuildReport()
    {
        var current = ReplayStateSnapshot.Capture(Player.Session, _record);
        var expected = ExpectedSnapshotAt(Player.CurrentActionIndex);
        return new ReplayInspectorReport(
            _record,
            Player.CurrentActionIndex,
            Player.ActionCount,
            Player.IsComplete,
            Player.IsInvalid,
            Player.LastError,
            _initial,
            expected,
            current,
            ReplayStateDiffGenerator.Compare(_initial, current),
            ReplayStateDiffGenerator.Compare(expected, current),
            Player.NextAction);
    }

    public bool Step()
    {
        var advanced = Player.Step();
        Changed?.Invoke();
        return advanced;
    }

    public int StepToEnd()
    {
        var count = 0;
        while (!Player.IsComplete && !Player.IsInvalid && Step()) count++;
        return count;
    }

    public void Reset()
    {
        Player.Reset();
        Changed?.Invoke();
    }

    public IReadOnlyList<ReplayInspectorActionRow> ActionRows() => _record.Actions.Select((action, index) => new ReplayInspectorActionRow(index, index < Player.CurrentActionIndex, index == Player.CurrentActionIndex && !Player.IsComplete, action, DescribeAction(action))).ToList();

    private ReplayStateSnapshot ExpectedSnapshotAt(int actionIndex)
    {
        var expected = new ReplayPlayer(_record, _buildEncounter, _abilities);
        for (var index = 0; index < actionIndex; index++)
        {
            if (!expected.Step()) break;
        }
        return ReplayStateSnapshot.Capture(expected.Session, _record);
    }

    private static string DescribeAction(TacticalAction action) =>
        $"T{action.Turn} · {action.ActorId} · {action.Type}" +
        (action.Target is { } target ? $" → {target}" : string.Empty) +
        (action.AbilityId is { } ability ? $" · {ability}" : string.Empty);
}

public sealed record ReplayInspectorActionRow(int Index, bool IsResolved, bool IsCurrent, TacticalAction Action, string Label);

public sealed record ReplayInspectorReport(
    ReplayRecord Record,
    int CurrentActionIndex,
    int ActionCount,
    bool IsComplete,
    bool IsInvalid,
    string? Error,
    ReplayStateSnapshot Initial,
    ReplayStateSnapshot Expected,
    ReplayStateSnapshot Current,
    ReplayStateDiff DifferenceFromInitial,
    ReplayStateDiff DeterminismDifference,
    TacticalAction? NextAction)
{
    public string CurrentFingerprint => Current.Fingerprint;
    public string ExpectedFingerprint => Expected.Fingerprint;
}
