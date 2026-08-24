using System.Text;

namespace RuneGrid.Tactics.Core;

/// <summary>Produces a canonical, order-stable state digest for replay and saved-encounter verification.</summary>
public static class ReplayFingerprint
{
    public static string Create(GameSession session, ReplayRecord? record = null)
    {
        var payload = new StringBuilder();
        var encounter = session.Encounter;
        payload.Append(encounter.Id).Append('|').Append(encounter.Seed).Append('|').Append(encounter.Mode).Append('|').Append(encounter.Difficulty)
            .Append('|').Append(session.Phase).Append('|').Append(session.Turn).Append('|').Append(record?.SchemaVersion ?? 0);

        foreach (var tile in encounter.Grid.Tiles.OrderBy(tile => tile.Position.Y).ThenBy(tile => tile.Position.X))
        {
            payload.Append("|t:").Append(tile.Position).Append(':').Append(tile.Kind).Append(':').Append(tile.Elevation).Append(':').Append(tile.Integrity)
                .Append(':').Append(tile.LinkedTo).Append(':').Append(tile.CoverValue).Append(':').Append(tile.IsHighGround);
        }

        foreach (var unit in encounter.Units.OrderBy(unit => unit.Id, StringComparer.Ordinal))
        {
            payload.Append("|u:").Append(unit.Id).Append(':').Append(unit.Template.Id).Append(':').Append(unit.Position).Append(':').Append(unit.Health)
                .Append(':').Append(unit.Energy).Append(':').Append(unit.Shield).Append(':').Append(unit.Moved).Append(':').Append(unit.Acted).Append(':').Append(unit.ReservedDestination);
            foreach (var cooldown in unit.Cooldowns.OrderBy(entry => entry.Key, StringComparer.Ordinal)) payload.Append(":c-").Append(cooldown.Key).Append('=').Append(cooldown.Value);
            foreach (var status in unit.Statuses.OrderBy(entry => entry.Key, StringComparer.Ordinal)) payload.Append(":s-").Append(status.Key).Append('=').Append(status.Value);
        }

        foreach (var action in session.Actions) payload.Append("|a:").Append(action.Turn).Append(':').Append(action.ActorId).Append(':').Append(action.Type).Append(':').Append(action.Target).Append(':').Append(action.AbilityId);
        return DeterministicRandom.Hash(payload.ToString()).ToString("X8");
    }
}
