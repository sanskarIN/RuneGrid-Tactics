namespace RuneGrid.Tactics.Core;

public enum ReplayInspectorShortcut
{
    Previous,
    Next,
    Start,
    End,
    PlayToEnd
}

/// <summary>Maps unmodified physical key names to replay inspector navigation intents.</summary>
public static class ReplayInspectorShortcutMap
{
    public static bool TryParse(string key, bool hasModifier, out ReplayInspectorShortcut shortcut)
    {
        return ReplayInspectorKeyBindings.CreateDefault().TryResolve(key, hasModifier, out shortcut);
    }
}

/// <summary>Local, conflict-free key assignments for replay timeline commands.</summary>
public sealed class ReplayInspectorKeyBindings
{
    private static readonly ReplayInspectorShortcut[] Shortcuts = Enum.GetValues<ReplayInspectorShortcut>();
    private static readonly Dictionary<ReplayInspectorShortcut, string> Defaults = new()
    {
        [ReplayInspectorShortcut.Previous] = "Left",
        [ReplayInspectorShortcut.Next] = "Right",
        [ReplayInspectorShortcut.Start] = "Home",
        [ReplayInspectorShortcut.End] = "End",
        [ReplayInspectorShortcut.PlayToEnd] = "P"
    };

    public string Previous { get; set; } = Defaults[ReplayInspectorShortcut.Previous];
    public string Next { get; set; } = Defaults[ReplayInspectorShortcut.Next];
    public string Start { get; set; } = Defaults[ReplayInspectorShortcut.Start];
    public string End { get; set; } = Defaults[ReplayInspectorShortcut.End];
    public string PlayToEnd { get; set; } = Defaults[ReplayInspectorShortcut.PlayToEnd];

    public static ReplayInspectorKeyBindings CreateDefault() => new();

    public string Get(ReplayInspectorShortcut shortcut) => shortcut switch
    {
        ReplayInspectorShortcut.Previous => Previous,
        ReplayInspectorShortcut.Next => Next,
        ReplayInspectorShortcut.Start => Start,
        ReplayInspectorShortcut.End => End,
        ReplayInspectorShortcut.PlayToEnd => PlayToEnd,
        _ => throw new ArgumentOutOfRangeException(nameof(shortcut))
    };

    public void RestoreDefaults()
    {
        foreach (var shortcut in Shortcuts) Set(shortcut, Defaults[shortcut]);
    }

    public void Normalize()
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var shortcut in Shortcuts)
        {
            if (!TryNormalizeKey(Get(shortcut), out var normalized) || !used.Add(normalized))
            {
                normalized = Defaults[shortcut];
                if (used.Contains(normalized))
                {
                    normalized = Defaults.First(pair => !used.Contains(pair.Value)).Value;
                }
                used.Add(normalized);
            }
            Set(shortcut, normalized);
        }
    }

    public bool TryAssign(ReplayInspectorShortcut shortcut, string? key, out string error)
    {
        error = string.Empty;
        if (!TryNormalizeKey(key, out var normalized))
        {
            error = "Choose a letter, arrow, Space, Home, End, Page Up, or Page Down.";
            return false;
        }
        if (shortcut != ReplayInspectorShortcut.Next && IsDefaultNextBinding && string.Equals(normalized, "Space", StringComparison.OrdinalIgnoreCase))
        {
            error = "Space is available as the default alternate binding for Next.";
            return false;
        }
        if (Shortcuts.Any(other => other != shortcut && string.Equals(Get(other), normalized, StringComparison.OrdinalIgnoreCase)))
        {
            error = $"{normalized} is already assigned to {LabelFor(Shortcuts.First(other => other != shortcut && string.Equals(Get(other), normalized, StringComparison.OrdinalIgnoreCase)))}.";
            return false;
        }
        Set(shortcut, normalized);
        return true;
    }

    public bool TryResolve(string? key, bool hasModifier, out ReplayInspectorShortcut shortcut)
    {
        shortcut = default;
        if (hasModifier || !TryNormalizeKey(key, out var normalized)) return false;
        foreach (var candidate in Shortcuts)
        {
            if (!string.Equals(Get(candidate), normalized, StringComparison.OrdinalIgnoreCase)) continue;
            shortcut = candidate;
            return true;
        }
        if (IsDefaultNextBinding && string.Equals(normalized, "Space", StringComparison.OrdinalIgnoreCase))
        {
            shortcut = ReplayInspectorShortcut.Next;
            return true;
        }
        return false;
    }

    public static string LabelFor(ReplayInspectorShortcut shortcut) => shortcut switch
    {
        ReplayInspectorShortcut.Previous => "Previous",
        ReplayInspectorShortcut.Next => "Next",
        ReplayInspectorShortcut.Start => "Opening state",
        ReplayInspectorShortcut.End => "Final state",
        ReplayInspectorShortcut.PlayToEnd => "Play to end",
        _ => shortcut.ToString()
    };

    public static bool TryNormalizeKey(string? key, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(key)) return false;
        var compact = key.Trim().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
        normalized = compact switch
        {
            "LEFT" => "Left",
            "RIGHT" => "Right",
            "UP" => "Up",
            "DOWN" => "Down",
            "SPACE" => "Space",
            "HOME" => "Home",
            "END" => "End",
            "PAGEUP" => "Page Up",
            "PAGEDOWN" => "Page Down",
            _ when compact.Length == 1 && compact[0] is >= 'A' and <= 'Z' => compact,
            _ => string.Empty
        };
        return normalized.Length > 0;
    }

    private void Set(ReplayInspectorShortcut shortcut, string value)
    {
        switch (shortcut)
        {
            case ReplayInspectorShortcut.Previous: Previous = value; break;
            case ReplayInspectorShortcut.Next: Next = value; break;
            case ReplayInspectorShortcut.Start: Start = value; break;
            case ReplayInspectorShortcut.End: End = value; break;
            case ReplayInspectorShortcut.PlayToEnd: PlayToEnd = value; break;
        }
    }

    private bool IsDefaultNextBinding => string.Equals(Next, Defaults[ReplayInspectorShortcut.Next], StringComparison.OrdinalIgnoreCase);
}
