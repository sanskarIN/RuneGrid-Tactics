namespace RuneGrid.Tactics.Core;

public sealed class ProgressionService
{
    public PlayerProfile Profile { get; }
    public ProgressionService(PlayerProfile profile) => Profile = profile;

    public void RecordHeroUse(string templateId) => Profile.Statistics.MostUsedHeroes[templateId] = Profile.Statistics.MostUsedHeroes.GetValueOrDefault(templateId) + 1;
    public void RecordAbilityUse(string abilityId) => Profile.Statistics.MostUsedAbilities[abilityId] = Profile.Statistics.MostUsedAbilities.GetValueOrDefault(abilityId) + 1;

    public IReadOnlyList<string> RecordOutcome(GameSession session)
    {
        var profile = Profile;
        var statistics = profile.Statistics;
        var victory = session.Phase == GamePhase.Victory;
        statistics.BattlesPlayed++;
        statistics.TurnsPlayed += session.Turn;
        if (!victory) { statistics.Defeats++; return Array.Empty<string>(); }

        statistics.Victories++;
        profile.Shards += session.Encounter.RewardShards;
        profile.PlayerLevel = Math.Max(1, statistics.Victories / 3 + 1);
        if (session.Encounter.Mode is GameMode.Expedition or GameMode.Endless) statistics.LongestExpedition = Math.Max(statistics.LongestExpedition, statistics.Victories);
        if (session.Encounter.Mode == GameMode.Daily && !statistics.DailyHistory.Contains(session.Encounter.Seed)) statistics.DailyHistory.Add(session.Encounter.Seed);
        if (session.Encounter.Mode is GameMode.Daily or GameMode.Weekly or GameMode.Puzzle) statistics.ChallengesCompleted++;
        if (session.Encounter.Mode == GameMode.BossRush) statistics.BossesDefeated++;
        if (session.Encounter.Mode == GameMode.Campaign && !profile.CampaignCleared.Contains(session.Encounter.Id)) profile.CampaignCleared.Add(session.Encounter.Id);
        if (session.Encounter.Mode == GameMode.Campaign && !profile.UnlockedHeroes.Contains("guardian")) profile.UnlockedHeroes.Add("guardian");
        profile.Replays.Insert(0, session.CreateReplay());
        if (profile.Replays.Count > 25) profile.Replays.RemoveRange(25, profile.Replays.Count - 25);
        return EvaluateAchievements(session);
    }

    private IReadOnlyList<string> EvaluateAchievements(GameSession session)
    {
        var result = new List<string>();
        var definitions = new[]
        {
            ("first-victory", "First Mark", Profile.Statistics.Victories, 1),
            ("expedition-master", "Expedition Master", Profile.Statistics.LongestExpedition, 3),
            ("boss-breaker", "Boss Breaker", Profile.Statistics.BossesDefeated, 1),
            ("tactical-genius", "Tactical Genius", session.Encounter.Mode == GameMode.Puzzle ? 1 : 0, 1)
        };
        foreach (var (id, name, progress, goal) in definitions)
        {
            Profile.AchievementProgress[id] = Math.Max(Profile.AchievementProgress.GetValueOrDefault(id), progress);
            if (progress >= goal && !Profile.UnlockedAchievements.Contains(id)) { Profile.UnlockedAchievements.Add(id); result.Add(name); }
        }
        return result;
    }
}
