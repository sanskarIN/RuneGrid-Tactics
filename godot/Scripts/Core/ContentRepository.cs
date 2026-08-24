using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using GodotFileAccess = Godot.FileAccess;

namespace RuneGrid.Tactics.Core;

public sealed class ContentRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyDictionary<string, UnitTemplate> Heroes { get; }
    public IReadOnlyDictionary<string, UnitTemplate> Enemies { get; }
    public IReadOnlyDictionary<string, AbilityDefinition> Abilities { get; }
    public IReadOnlyDictionary<string, ItemDefinition> Items { get; }
    public IReadOnlyDictionary<string, LevelDefinition> Levels { get; }
    public BalanceDefinition Balance { get; }

    public ContentRepository()
    {
        Heroes = LoadCollection<UnitTemplate>("res://Data/heroes.json").ToDictionary(item => item.Id, StringComparer.Ordinal);
        Enemies = LoadCollection<UnitTemplate>("res://Data/enemies.json").ToDictionary(item => item.Id, StringComparer.Ordinal);
        Abilities = LoadCollection<AbilityDefinition>("res://Data/abilities.json").ToDictionary(item => item.Id, StringComparer.Ordinal);
        Items = LoadCollection<ItemDefinition>("res://Data/items.json").ToDictionary(item => item.Id, StringComparer.Ordinal);
        Levels = LoadCollection<LevelDefinition>("res://Data/levels.json").ToDictionary(item => item.Id, StringComparer.Ordinal);
        Balance = LoadObject<BalanceDefinition>("res://Data/balance.json");
    }

    private IReadOnlyList<T> LoadCollection<T>(string path)
    {
        using var file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Read);
        if (file is null)
            throw new InvalidOperationException($"RuneGrid content file could not be opened: {path}");

        var content = file.GetAsText();
        return JsonSerializer.Deserialize<List<T>>(content, _jsonOptions)
               ?? throw new InvalidOperationException($"RuneGrid content file contains no valid entries: {path}");
    }

    private T LoadObject<T>(string path)
    {
        using var file = GodotFileAccess.Open(path, GodotFileAccess.ModeFlags.Read);
        if (file is null) throw new InvalidOperationException($"RuneGrid content file could not be opened: {path}");
        return JsonSerializer.Deserialize<T>(file.GetAsText(), _jsonOptions)
               ?? throw new InvalidOperationException($"RuneGrid content file has no valid object: {path}");
    }
}
