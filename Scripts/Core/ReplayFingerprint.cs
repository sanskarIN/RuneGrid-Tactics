namespace RuneGrid.Tactics.Core;

/// <summary>Produces a canonical, order-stable state digest for replay and saved-encounter verification.</summary>
public static class ReplayFingerprint
{
    public static string Create(GameSession session, ReplayRecord? record = null) => ReplayStateSnapshot.Capture(session, record).Fingerprint;
}
