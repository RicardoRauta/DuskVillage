using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DuskVillage.Items;

public sealed class ItemDefinitionRegistry
{
    private readonly Dictionary<string, ItemDefinition> _definitionsById;

    private ItemDefinitionRegistry(IEnumerable<ItemDefinition> definitions)
    {
        var ordered = definitions.OrderBy(definition => definition.Id, StringComparer.OrdinalIgnoreCase).ToArray();
        Definitions = ordered;
        _definitionsById = ordered.ToDictionary(definition => definition.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ItemDefinition> Definitions { get; }

    public static ItemDefinitionRegistry Empty { get; } = new(Array.Empty<ItemDefinition>());

    public static ItemDefinitionRegistry FromDefinitions(IEnumerable<ItemDefinition> definitions)
    {
        var loaded = new List<ItemDefinition>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions ?? Array.Empty<ItemDefinition>())
        {
            ValidateDefinition(definition, "in-memory");
            if (!ids.Add(definition.Id))
            {
                throw new InvalidDataException($"Duplicate item id '{definition.Id}'.");
            }

            loaded.Add(definition);
        }

        return new ItemDefinitionRegistry(loaded);
    }

    public static ItemDefinitionRegistry LoadFromDirectories(params string[] directories)
    {
        var loaded = new List<ItemDefinition>();
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
                        throw new InvalidDataException($"Duplicate item id '{definition.Id}' in '{filePath}'.");
                    }

                    loaded.Add(definition);
                }
            }
        }

        return new ItemDefinitionRegistry(loaded);
    }

    public bool TryGet(string itemId, out ItemDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            definition = null;
            return false;
        }

        return _definitionsById.TryGetValue(itemId, out definition);
    }

    public ItemDefinition Find(string itemId)
    {
        return TryGet(itemId, out var definition) ? definition : null;
    }

    public int MaxStackFor(string itemId)
    {
        return TryGet(itemId, out var definition) ? Math.Max(1, definition.MaxStack) : 99;
    }

    private static IReadOnlyList<ItemDefinition> ReadDefinitions(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var definitions = JsonSerializer.Deserialize<List<ItemDefinition>>(json, JsonOptions);
            return definitions ?? new List<ItemDefinition>();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Item definition file '{filePath}' is not valid JSON.", exception);
        }
    }

    private static void ValidateDefinition(ItemDefinition definition, string source)
    {
        if (definition == null)
        {
            throw new InvalidDataException($"Item definition in '{source}' is null.");
        }

        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new InvalidDataException($"Item definition in '{source}' is missing an id.");
        }

        if (string.IsNullOrWhiteSpace(definition.LabelKey))
        {
            throw new InvalidDataException($"Item '{definition.Id}' is missing a label key.");
        }

        if (definition.MaxStack <= 0)
        {
            throw new InvalidDataException($"Item '{definition.Id}' must have a positive max stack.");
        }

        definition.Id = definition.Id.Trim();
        definition.LabelKey = definition.LabelKey.Trim();
        definition.DescriptionKey = definition.DescriptionKey?.Trim() ?? string.Empty;
        definition.IconAssetId = definition.IconAssetId?.Trim() ?? string.Empty;
        definition.Category = definition.Category?.Trim() ?? string.Empty;
        definition.Tags ??= new List<string>();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
