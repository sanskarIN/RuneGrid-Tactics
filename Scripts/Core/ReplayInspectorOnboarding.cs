namespace RuneGrid.Tactics.Core;

/// <summary>Local first-time guidance state for the replay inspector.</summary>
public sealed class ReplayInspectorOnboarding
{
    public bool HasSeenReplayInspectorIntro { get; set; }

    public bool ShouldShowIntro => !HasSeenReplayInspectorIntro;

    public void DismissIntro() => HasSeenReplayInspectorIntro = true;
}
