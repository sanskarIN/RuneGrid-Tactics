using Godot;
using RuneGrid.Tactics.Core;

namespace RuneGrid.Tactics.Godot;

public partial class GameServices : Node
{
    public ContentRepository Content { get; private set; } = null!;
    public EncounterFactory Encounters { get; private set; } = null!;
    public LocalSaveRepository Saves { get; private set; } = null!;
    public SaveEnvelope SaveData { get; private set; } = null!;
    public ProgressionService Progression { get; private set; } = null!;
    public GameSession? CurrentSession { get; private set; }

    public override void _Ready()
    {
        Content = new ContentRepository();
        Encounters = new EncounterFactory(Content);
        Saves = new LocalSaveRepository();
        SaveData = Saves.Load();
        SaveData.Accessibility.ReplayKeyBindings ??= ReplayInspectorKeyBindings.CreateDefault();
        SaveData.Accessibility.ReplayKeyBindings.Normalize();
        Progression = new ProgressionService(SaveData.Profile);
        Progression.EnsureNativeRoster();
        Persist();
    }

    public GameSession StartEncounter(GameMode mode, string seed)
    {
        CurrentSession = new GameSession(Encounters.Create(seed, mode, SaveData.Difficulty), Content.Abilities);
        foreach (var hero in CurrentSession.LivingHeroes) Progression.RecordHeroUse(hero.Template.Id);
        CurrentSession.StateChanged += Persist;
        CurrentSession.Start();
        Persist();
        return CurrentSession;
    }

    public ReplayInspector InspectReplay(ReplayRecord record) => new(record, (seed, mode, difficulty) => Encounters.Create(seed, mode, difficulty), Content.Abilities);

    public void CompleteCurrentSession()
    {
        if (CurrentSession is null || CurrentSession.Phase is not (GamePhase.Victory or GamePhase.Defeat)) return;
        Progression.RecordOutcome(CurrentSession);
        Persist();
    }

    public void ReplaceSave(SaveEnvelope imported)
    {
        SaveData = imported;
        SaveData.Accessibility.ReplayKeyBindings ??= ReplayInspectorKeyBindings.CreateDefault();
        SaveData.Accessibility.ReplayKeyBindings.Normalize();
        Progression = new ProgressionService(SaveData.Profile);
        Progression.EnsureNativeRoster();
        Persist();
    }

    public void Persist()
    {
        SaveData.Profile = Progression.Profile;
        SaveData = Saves.Save(SaveData);
    }
}
