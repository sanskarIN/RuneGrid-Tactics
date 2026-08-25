namespace RuneGrid.Tactics.Core;

/// <summary>Local first-time guidance state for the replay inspector.</summary>
public sealed class ReplayInspectorOnboarding
{
    public bool HasSeenReplayInspectorIntro { get; set; }
    public List<string> AcknowledgedMismatchWarningKeys { get; set; } = [];

    public bool ShouldShowIntro => !HasSeenReplayInspectorIntro;

    public void DismissIntro() => HasSeenReplayInspectorIntro = true;

    public bool ShouldShowMismatchWarning(string warningKey) => !string.IsNullOrWhiteSpace(warningKey) && !AcknowledgedMismatchWarningKeys.Contains(warningKey, StringComparer.Ordinal);

    public void AcknowledgeMismatchWarning(string warningKey)
    {
        if (string.IsNullOrWhiteSpace(warningKey) || !ShouldShowMismatchWarning(warningKey)) return;
        AcknowledgedMismatchWarningKeys.Add(warningKey);
    }
}

public static class ReplayInspectorMismatchWarning
{
    public static string BuildKey(int actionIndex, string expectedFingerprint, string currentFingerprint) => $"{actionIndex}:{expectedFingerprint}:{currentFingerprint}";

    public static string Summarize(ReplayStateDiff difference)
    {
        if (difference.IsMatch) return "No replay determinism mismatch is present.";
        return difference.Lines.FirstOrDefault() ?? "The reconstructed replay state differs from the visible playback state.";
    }
}
