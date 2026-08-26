using System.Text;
using System.Text.Json;

namespace RuneGrid.Tactics.Core;

public sealed record ReplayMismatchExportLine(string Category, string Difference);

/// <summary>Portable, deterministic snapshot of the currently focused replay mismatch audit.</summary>
public sealed record ReplayMismatchExport(
    int SchemaVersion,
    string EncounterId,
    string Seed,
    string Mode,
    string Difficulty,
    string ReplayCreatedAt,
    int CurrentActionIndex,
    int ActionCount,
    string Filter,
    string ExpectedFingerprint,
    string CurrentFingerprint,
    bool IsDeterministicMatch,
    IReadOnlyList<string> AffectedTiles,
    IReadOnlyList<string> AffectedUnits,
    IReadOnlyList<ReplayMismatchExportLine> Differences)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public string ToCsv()
    {
        const string header = "record_type,encounter_id,seed,mode,difficulty,replay_created_at,current_action_index,action_count,filter,expected_fingerprint,current_fingerprint,is_deterministic_match,affected_tiles,affected_units,difference_category,difference";
        var rows = Differences.Count == 0
            ? new[] { new ReplayMismatchExportLine(string.Empty, string.Empty) }
            : Differences;
        var builder = new StringBuilder(header);
        foreach (var difference in rows)
        {
            builder.AppendLine();
            builder.AppendJoin(',', new[]
            {
                Escape("filtered_replay_mismatch"), Escape(EncounterId), Escape(Seed), Escape(Mode), Escape(Difficulty), Escape(ReplayCreatedAt),
                Escape(CurrentActionIndex.ToString()), Escape(ActionCount.ToString()), Escape(Filter), Escape(ExpectedFingerprint), Escape(CurrentFingerprint),
                Escape(IsDeterministicMatch.ToString().ToLowerInvariant()), Escape(string.Join(';', AffectedTiles)), Escape(string.Join(';', AffectedUnits)),
                Escape(difference.Category), Escape(difference.Difference)
            });
        }
        return builder.ToString();
    }

    public string BuildFileName(string extension)
    {
        var safeExtension = extension.Trim().TrimStart('.').ToLowerInvariant();
        return $"runegrid-replay-diff-{Safe(EncounterId)}-{Safe(Seed)}-action-{CurrentActionIndex}-{Safe(Filter)}.{safeExtension}";
    }

    private static string Escape(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

    private static string Safe(string value)
    {
        var normalized = new string(value.Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-').ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }
}

public static class ReplayMismatchExportBuilder
{
    public static ReplayMismatchExport Build(ReplayInspectorReport report, ReplayDiffFilterResult filteredDiff) => new(
        SchemaVersion: 1,
        EncounterId: report.Record.EncounterId,
        Seed: report.Record.Seed,
        Mode: report.Record.Mode.ToString(),
        Difficulty: report.Record.Difficulty.ToString(),
        ReplayCreatedAt: report.Record.CreatedAt.ToUniversalTime().ToString("O"),
        CurrentActionIndex: report.CurrentActionIndex,
        ActionCount: report.ActionCount,
        Filter: ReplayDiffFilter.LabelFor(filteredDiff.Category),
        ExpectedFingerprint: report.ExpectedFingerprint,
        CurrentFingerprint: report.CurrentFingerprint,
        IsDeterministicMatch: report.DeterminismDifference.IsMatch,
        AffectedTiles: filteredDiff.AffectedTiles.OrderBy(point => point.Y).ThenBy(point => point.X).Select(point => point.ToString()).ToList(),
        AffectedUnits: filteredDiff.AffectedUnitIds.OrderBy(id => id, StringComparer.Ordinal).ToList(),
        Differences: filteredDiff.Entries.Select(entry => new ReplayMismatchExportLine(ReplayDiffFilter.LabelFor(entry.Category), entry.Line)).ToList());
}

/// <summary>Bounded local record of mismatch signatures already written by automatic export.</summary>
public sealed class ReplayMismatchAutoExportState
{
    private const int MaximumSignatures = 64;
    public List<string> ExportedMismatchSignatures { get; set; } = [];

    public bool HasExported(string? signature) => !string.IsNullOrWhiteSpace(signature) && ExportedMismatchSignatures.Contains(signature, StringComparer.Ordinal);

    public void MarkExported(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature) || HasExported(signature)) return;
        ExportedMismatchSignatures.Add(signature);
        if (ExportedMismatchSignatures.Count > MaximumSignatures) ExportedMismatchSignatures.RemoveRange(0, ExportedMismatchSignatures.Count - MaximumSignatures);
    }
}

public static class ReplayMismatchAutoExport
{
    public static string BuildUserPath(ReplayMismatchExport export, string mismatchSignature)
    {
        var name = export.BuildFileName("json");
        var stem = name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? name[..^5] : name;
        var signatureHash = DeterministicRandom.Hash(mismatchSignature).ToString("X8").ToLowerInvariant();
        return $"user://auto-{stem}-{signatureHash}.json";
    }
}
