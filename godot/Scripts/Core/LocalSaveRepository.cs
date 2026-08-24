using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using GodotFileAccess = Godot.FileAccess;

namespace RuneGrid.Tactics.Core;

public sealed class Statistics
{
    public int BattlesPlayed { get; set; }
    public int Victories { get; set; }
    public int Defeats { get; set; }
    public int TurnsPlayed { get; set; }
    public int DamageDealt { get; set; }
    public int HealingDone { get; set; }
    public int BossesDefeated { get; set; }
    public int ChallengesCompleted { get; set; }
    public int LongestExpedition { get; set; }
    public int PerfectVictories { get; set; }
    public Dictionary<string, int> MostUsedHeroes { get; set; } = new();
    public Dictionary<string, int> MostUsedAbilities { get; set; } = new();
    public List<string> DailyHistory { get; set; } = [];
}

public sealed class AccessibilitySettings
{
    public string TextScale { get; set; } = "standard";
    public bool HighContrast { get; set; }
    public bool ReducedMotion { get; set; }
    public bool ReducedFlashing { get; set; } = true;
    public float MusicVolume { get; set; } = 0.45f;
    public float EffectsVolume { get; set; } = 0.55f;
    public bool Vibration { get; set; }
    public bool ConfirmActions { get; set; }
    public string Handedness { get; set; } = "right";
}

public sealed class PlayerProfile
{
    public int PlayerLevel { get; set; } = 1;
    public int Shards { get; set; }
    public List<string> CampaignCleared { get; set; } = [];
    public List<string> UnlockedHeroes { get; set; } = ["vanguard", "runemage", "ranger"];
    public List<string> Relics { get; set; } = [];
    public Dictionary<string, int> AchievementProgress { get; set; } = new();
    public List<string> UnlockedAchievements { get; set; } = [];
    public Statistics Statistics { get; set; } = new();
    public List<ReplayRecord> Replays { get; set; } = [];
}

public sealed class SaveEnvelope
{
    public int SchemaVersion { get; set; } = 1;
    public string Checksum { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public PlayerProfile Profile { get; set; } = new();
    public AccessibilitySettings Accessibility { get; set; } = new();
    public Difficulty Difficulty { get; set; } = Difficulty.Field;
}

public sealed class LocalSaveRepository
{
    private const string PrimaryPath = "user://runegrid-save.json";
    private const string BackupPath = "user://runegrid-save.backup.json";
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

    public SaveEnvelope Load()
    {
        var primary = Read(PrimaryPath);
        if (primary is not null) return primary;
        var backup = Read(BackupPath);
        if (backup is not null) { Save(backup); return backup; }
        return Seal(new SaveEnvelope());
    }

    public SaveEnvelope Save(SaveEnvelope value)
    {
        var sealedValue = Seal(value);
        if (GodotFileAccess.FileExists(PrimaryPath))
        {
            using var old = GodotFileAccess.Open(PrimaryPath, GodotFileAccess.ModeFlags.Read);
            using var backup = GodotFileAccess.Open(BackupPath, GodotFileAccess.ModeFlags.Write);
            backup.StoreString(old.GetAsText());
        }
        using var file = GodotFileAccess.Open(PrimaryPath, GodotFileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(sealedValue, _options));
        return sealedValue;
    }

    public string Export(SaveEnvelope value) => JsonSerializer.Serialize(Seal(value), _options);

    public SaveEnvelope Import(string json)
    {
        var value = JsonSerializer.Deserialize<SaveEnvelope>(json, _options) ?? throw new InvalidOperationException("The imported field record has no save envelope.");
        if (!IsValid(value)) throw new InvalidOperationException("The imported field record failed integrity validation.");
        return Save(value);
    }

    private SaveEnvelope? Read(string path)
    {
        if (!GodotFileAccess.FileExists(path)) return null;
        try
        {
            using var file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Read);
            var value = JsonSerializer.Deserialize<SaveEnvelope>(file.GetAsText(), _options);
            return value is not null && IsValid(value) ? value : null;
        }
        catch (JsonException) { return null; }
    }

    private SaveEnvelope Seal(SaveEnvelope value)
    {
        value.SchemaVersion = 1;
        value.UpdatedAt = DateTimeOffset.UtcNow;
        value.Checksum = string.Empty;
        value.Checksum = ComputeChecksum(value);
        return value;
    }

    private bool IsValid(SaveEnvelope value)
    {
        if (value.SchemaVersion != 1 || value.Profile is null || value.Accessibility is null || string.IsNullOrWhiteSpace(value.Checksum)) return false;
        var known = value.Checksum;
        value.Checksum = string.Empty;
        var expected = ComputeChecksum(value);
        value.Checksum = known;
        return known == expected;
    }

    private string ComputeChecksum(SaveEnvelope value) => DeterministicRandom.Hash(JsonSerializer.Serialize(value, _options)).ToString("X8");
}
