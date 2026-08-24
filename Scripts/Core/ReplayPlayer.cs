namespace RuneGrid.Tactics.Core;

/// <summary>
/// Rebuilds a compatible seeded encounter and replays recorded player choices.
/// Enemy records are regenerated deterministically by GameSession, preserving the compact replay format.
/// </summary>
public sealed class ReplayPlayer
{
    private readonly IReadOnlyDictionary<string, AbilityDefinition> _abilities;
    private readonly EncounterFactory _encounters;
    private readonly ReplayRecord _record;
    private int _index;

    public GameSession Session { get; }
    public bool IsPaused { get; private set; } = true;
    public float Speed { get; private set; } = 1f;
    public int CurrentActionIndex => _index;
    public int ActionCount => _record.Actions.Count;
    public bool IsComplete => _index >= _record.Actions.Count;

    public ReplayPlayer(ReplayRecord record, EncounterFactory encounters, IReadOnlyDictionary<string, AbilityDefinition> abilities)
    {
        _record = record;
        _encounters = encounters;
        _abilities = abilities;
        Session = new GameSession(_encounters.Create(record.Seed, record.Mode, record.Difficulty), _abilities);
        Session.Start();
    }

    public void Play() => IsPaused = false;
    public void Pause() => IsPaused = true;
    public void SetSpeed(float speed) => Speed = Math.Clamp(speed, 0.5f, 4f);

    public bool Step()
    {
        if (IsComplete || Session.Phase is GamePhase.Victory or GamePhase.Defeat) return false;
        var action = _record.Actions[_index++];
        if (action.Type == "enemy") return true;
        if (action.Type == "end-turn") return Session.EndTurn();
        if (action.Target is not { } target) return false;
        if (!Session.SelectUnit(action.ActorId)) return false;
        if (action.Type == "ability" && action.AbilityId is not null) Session.SelectAbility(action.AbilityId);
        return Session.ChooseTile(target);
    }

    public void Reset()
    {
        _index = 0;
        IsPaused = true;
    }
}
