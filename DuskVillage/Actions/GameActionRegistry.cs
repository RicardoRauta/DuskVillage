using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DuskVillage.Animations;

namespace DuskVillage.Actions;

public sealed class GameActionRegistry
{
    private readonly Dictionary<string, GameActionDefinition> _definitionsById;

    private GameActionRegistry(IEnumerable<GameActionDefinition> definitions)
    {
        var ordered = definitions.OrderBy(definition => definition.Id, StringComparer.OrdinalIgnoreCase).ToArray();
        Definitions = ordered;
        _definitionsById = ordered.ToDictionary(definition => definition.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<GameActionDefinition> Definitions { get; }

    public static GameActionRegistry Empty { get; } = new(Array.Empty<GameActionDefinition>());

    public static GameActionRegistry FromDefinitions(IEnumerable<GameActionDefinition> definitions)
    {
        var loaded = new List<GameActionDefinition>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            ValidateDefinition(definition, "in-memory");
            if (!ids.Add(definition.Id))
            {
                throw new InvalidDataException($"Duplicate action id '{definition.Id}'.");
            }

            loaded.Add(definition);
        }

        return new GameActionRegistry(loaded);
    }

    public static GameActionRegistry LoadFromDirectories(params string[] directories)
    {
        var loaded = new List<GameActionDefinition>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var directory in directories)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var definitions = ReadDefinitions(filePath);
                foreach (var definition in definitions)
                {
                    ValidateDefinition(definition, filePath);
                    if (!ids.Add(definition.Id))
                    {
                        throw new InvalidDataException($"Duplicate action id '{definition.Id}' in '{filePath}'.");
                    }

                    loaded.Add(definition);
                }
            }
        }

        return new GameActionRegistry(loaded);
    }

    public bool TryGet(string actionId, out GameActionDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(actionId))
        {
            definition = null;
            return false;
        }

        return _definitionsById.TryGetValue(actionId, out definition);
    }

    public GameActionDefinition Find(string actionId)
    {
        return TryGet(actionId, out var definition) ? definition : null;
    }

    private static IReadOnlyList<GameActionDefinition> ReadDefinitions(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var definitions = JsonSerializer.Deserialize<List<GameActionDefinition>>(json, JsonOptions);
            return definitions ?? new List<GameActionDefinition>();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Action definition file '{filePath}' is not valid JSON.", exception);
        }
    }

    private static void ValidateDefinition(GameActionDefinition definition, string source)
    {
        if (definition == null)
        {
            throw new InvalidDataException($"Action definition in '{source}' is null.");
        }

        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new InvalidDataException($"Action definition in '{source}' is missing an id.");
        }

        if (string.IsNullOrWhiteSpace(definition.LabelKey))
        {
            throw new InvalidDataException($"Action '{definition.Id}' is missing a label key.");
        }

        if (!GameActionTargetKinds.IsKnown(definition.TargetKind))
        {
            throw new InvalidDataException($"Action '{definition.Id}' has unknown target kind '{definition.TargetKind}'.");
        }

        if (definition.TimeCostMinutes < 0)
        {
            throw new InvalidDataException($"Action '{definition.Id}' cannot have negative time cost.");
        }

        CharacterAnimationCatalog.GetClip(definition.AnimationId, CharacterFacingDirection.Down);

        definition.Tags ??= new List<string>();
        definition.Effects ??= new List<GameActionEffectDefinition>();
        foreach (var effect in definition.Effects)
        {
            ValidateEffect(definition.Id, effect);
        }
    }

    private static void ValidateEffect(string actionId, GameActionEffectDefinition effect)
    {
        if (effect == null)
        {
            throw new InvalidDataException($"Action '{actionId}' has a null effect.");
        }

        if (!GameActionEffectTypes.IsKnown(effect.Type))
        {
            throw new InvalidDataException($"Action '{actionId}' has unknown effect type '{effect.Type}'.");
        }

        if (effect.Type.Equals(GameActionEffectTypes.ChangeNeed, StringComparison.OrdinalIgnoreCase) &&
            !GameActionNeedIds.IsKnown(effect.NeedId))
        {
            throw new InvalidDataException($"Action '{actionId}' has unknown need id '{effect.NeedId}'.");
        }

        if ((effect.Type.Equals(GameActionEffectTypes.RequireItem, StringComparison.OrdinalIgnoreCase) ||
            effect.Type.Equals(GameActionEffectTypes.ConsumeItem, StringComparison.OrdinalIgnoreCase)) &&
            string.IsNullOrWhiteSpace(effect.ItemId))
        {
            throw new InvalidDataException($"Action '{actionId}' has an item effect without an item id.");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
