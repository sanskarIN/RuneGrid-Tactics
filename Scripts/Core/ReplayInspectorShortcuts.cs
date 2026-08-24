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
        shortcut = default;
        if (hasModifier || string.IsNullOrWhiteSpace(key)) return false;
        switch (key.Trim().ToUpperInvariant())
        {
            case "LEFT": shortcut = ReplayInspectorShortcut.Previous; return true;
            case "RIGHT":
            case "SPACE": shortcut = ReplayInspectorShortcut.Next; return true;
            case "HOME": shortcut = ReplayInspectorShortcut.Start; return true;
            case "END": shortcut = ReplayInspectorShortcut.End; return true;
            case "P": shortcut = ReplayInspectorShortcut.PlayToEnd; return true;
            default: return false;
        }
    }
}
