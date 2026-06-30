using System.Collections.Generic;

namespace DuskVillage.Items;

public sealed class ItemDefinition
{
    public string Id { get; set; } = string.Empty;

    public string LabelKey { get; set; } = string.Empty;

    public string DescriptionKey { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int MaxStack { get; set; } = 99;

    public List<string> Tags { get; set; } = new();
}
