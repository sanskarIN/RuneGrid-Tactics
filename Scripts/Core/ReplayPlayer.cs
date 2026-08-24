namespace RuneGrid.Tactics.Core;

/// <summary>
/// Rebuilds a compatible seeded encounter and replays recorded player choices.
/// Enemy records are regenerated deterministically by GameSession, preserving the compact replay format.
/// </summary>
public sealed class ReplayPlayer
{
    private readonly IReadOnlyDictionary<string, AbilityDefinition> _abilities;
    private readonly Func<string, GameMode, Difficulty, EncounterState> _buildEncounter;
    private readonly ReplayRecord _record;
    private int _index;

    public GameSession Session { get; private set; } = null!;
    public bool IsPaused { get; private set; } = true;
    public float Speed { get; private set; } = 1f;
    public int CurrentActionIndex => _index;
    public int ActionCount => _record.Actions.Count;
    public bool IsComplete => _index >= _record.Actions.Count;
    public string? LastError { get; private set; }
    public bool IsInvalid => LastError is not null;
    public string Fingerprint => ReplayFingerprint.Create(Session, _record);

    public ReplayPlayer(ReplayRecord record, Func<string, GameMode, Difficulty, EncounterState> buildEncounter, IReadOnlyDictionary<string, AbilityDefinition> abilities)
    {
        _record = record;
        _buildEncounter = buildEncounter;
        _abilities = abilities;
        Session = CreateSession();
    }

    public void Play() => IsPaused = false;
    public void Pause() => IsPaused = true;
    public void SetSpeed(float speed) => Speed = Math.Clamp(speed, 0.5f, 4f);

    public bool Step()
    {
        if (IsComplete || IsInvalid || Session.Phase is GamePhase.Victory or GamePhase.Defeat) return false;
        var action = _record.Actions[_index];
        if (action.Turn != Session.Turn) return Reject($"Replay turn mismatch at action {_index}: record {action.Turn}, session {Session.Turn}.");

        var actionCount = Session.Actions.Count;
        var accepted = action.Type switch
        {
            "enemy" => ReplayEnemyAction(action, actionCount),
            "end-turn" => Session.EndTurn(),
            "move" or "ability" => ReplayPlayerAction(action),
            _ => Reject($"Replay action {_index} has an unknown type: {action.Type}.")
        };
        if (!accepted) return false;

        var produced = Session.Actions.Skip(actionCount).LastOrDefault();
        if (produced is null || !SameAction(action, produced)) return Reject($"Replay action {_index} did not reproduce the recorded tactical event.");
        _index++;
        return true;
    }

    public void Reset()
    {
        _index = 0;
        IsPaused = true;
        LastError = null;
        Session = CreateSession();
    }

    private GameSession CreateSession()
    {
        var session = new GameSession(_buildEncounter(_record.Seed, _record.Mode, _record.Difficulty), _abilities);
        session.Start();
        return session;
    }

    private bool ReplayEnemyAction(TacticalAction action, int actionCount)
    {
        if (Session.Phase != GamePhase.Enemy) return Reject($"Replay action {_index} expected an enemy phase.");
        Session.ResolveNextEnemy();
        if (Session.Phase == GamePhase.Enemy && !Session.LivingEnemies.Any(enemy => !enemy.Acted)) Session.ResolveNextEnemy();
        return Session.Actions.Count > actionCount;
    }

    private bool ReplayPlayerAction(TacticalAction action)
    {
        if (action.Target is not { } target) return Reject($"Replay action {_index} has no tactical target.");
        if (!Session.SelectUnit(action.ActorId)) return Reject($"Replay action {_index} could not select {action.ActorId}.");
        if (action.Type == "ability" && (action.AbilityId is null || !Session.SelectAbility(action.AbilityId))) return Reject($"Replay action {_index} could not select its ability.");
        return Session.ChooseTile(target) || Reject($"Replay action {_index} could not resolve its target.");
    }

    private bool Reject(string reason)
    {
        LastError = reason;
        return false;
    }

    private static bool SameAction(TacticalAction expected, TacticalAction actual) =>
        expected.Turn == actual.Turn && expected.ActorId == actual.ActorId && expected.Type == actual.Type && expected.Target == actual.Target && expected.AbilityId == actual.AbilityId;
}
