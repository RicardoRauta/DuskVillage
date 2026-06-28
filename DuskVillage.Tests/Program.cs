using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.Saving;

var tests = new (string Name, Action Run)[]
{
    ("Default preset validates", DefaultPresetValidates),
    ("Preset JSON round-trips", PresetJsonRoundTrips),
    ("Attribute budget rejects invalid preset", AttributeBudgetRejectsInvalidPreset),
    ("Attribute randomizer respects point buy", AttributeRandomizerRespectsPointBuy),
    ("Identity fields validate", IdentityFieldsValidate),
    ("Required appearance slots reject none", RequiredAppearanceSlotsRejectNone),
    ("Mana Seed asset parser keeps stable IDs", ManaSeedAssetParserKeepsStableIds),
    ("Save game round-trips character preset", SaveGameRoundTripsCharacterPreset)
};

var failures = 0;
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception exception)
    {
        failures++;
        Console.WriteLine($"FAIL {test.Name}: {exception.Message}");
    }
}

if (failures > 0)
{
    Environment.Exit(1);
}

static void DefaultPresetValidates()
{
    var preset = CharacterPresetFactory.CreateDefault();
    var validation = CharacterPresetValidator.Validate(preset);
    Assert(validation.IsValid, "Default preset should be valid.");
}

static void PresetJsonRoundTrips()
{
    var preset = CharacterPresetFactory.CreateDefault();
    preset.Name = "Elena";
    preset.FamilyName = "Ashford";
    preset.BirthdaySeasonId = CharacterOptionCatalog.Autumn;
    preset.BirthdayDay = 17;
    preset.MotivationId = CharacterOptionCatalog.MotivationLostKnowledge;
    preset.Appearance.SetLayer(CharacterAppearanceSlotIds.Head, "fbas_14head_headscarf_00b_e");
    preset.Appearance.SetPalette(CharacterAppearanceSlotIds.Body, "skin_brown");
    preset.Appearance.SetPalette(CharacterAppearanceSlotIds.Shirt, "cloth_blue");
    preset.Skills.First(skill => skill.SkillId == "skill_survival").Level = 3;

    var json = CharacterPresetSerializer.Serialize(preset);
    var loaded = CharacterPresetSerializer.Deserialize(json);

    AssertEqual("Elena", loaded.Name, "Name should round-trip.");
    AssertEqual("Ashford", loaded.FamilyName, "Family name should round-trip.");
    AssertEqual(CharacterOptionCatalog.Autumn, loaded.BirthdaySeasonId, "Birthday season should round-trip.");
    AssertEqual(17, loaded.BirthdayDay, "Birthday day should round-trip.");
    AssertEqual(CharacterOptionCatalog.MotivationLostKnowledge, loaded.MotivationId, "Motivation should round-trip.");
    AssertEqual("fbas_14head_headscarf_00b_e", loaded.Appearance.GetLayer(CharacterAppearanceSlotIds.Head), "Headwear should round-trip.");
    AssertEqual("skin_brown", loaded.Appearance.GetPalette(CharacterAppearanceSlotIds.Body), "Skin palette should round-trip.");
    AssertEqual("cloth_blue", loaded.Appearance.GetPalette(CharacterAppearanceSlotIds.Shirt), "Clothing palette should round-trip.");
    AssertEqual(3, loaded.Skills.First(skill => skill.SkillId == "skill_survival").Level, "Skill level should round-trip.");
}

static void AttributeBudgetRejectsInvalidPreset()
{
    var preset = CharacterPresetFactory.CreateDefault();
    foreach (var attributeId in CharacterAttributeIds.All)
    {
        preset.Attributes.SetValue(attributeId, 10);
    }

    var validation = CharacterPresetValidator.Validate(preset);
    Assert(!validation.IsValid, "Over-budget preset should be invalid.");
    Assert(validation.Errors.Any(error => error.MessageKey == "character.validation.attribute_budget"), "Validation should include budget error.");
}

static void AttributeRandomizerRespectsPointBuy()
{
    var attributes = CharacterAttributePointBuy.Randomize(new Random(1234));

    AssertEqual(CharacterPresetValidator.AttributePointBudget, attributes.Total, "Random attributes should spend the point budget.");
    foreach (var attributeId in CharacterAttributeIds.All)
    {
        var value = attributes.GetValue(attributeId);
        Assert(value >= CharacterPresetValidator.AttributeMinimum, $"{attributeId} should respect minimum.");
        Assert(value <= CharacterPresetValidator.AttributeMaximum, $"{attributeId} should respect maximum.");
    }
}

static void IdentityFieldsValidate()
{
    var preset = CharacterPresetFactory.CreateDefault();
    preset.BirthdayDay = 42;
    preset.MotivationId = "missing_motivation";

    var validation = CharacterPresetValidator.Validate(preset);
    Assert(!validation.IsValid, "Invalid identity fields should be rejected.");
    Assert(validation.Errors.Any(error => error.MessageKey == "character.validation.birthday_day"), "Validation should include birthday day error.");
    Assert(validation.Errors.Any(error => error.MessageKey == "character.validation.motivation"), "Validation should include motivation error.");
}

static void RequiredAppearanceSlotsRejectNone()
{
    var preset = CharacterPresetFactory.CreateDefault();
    preset.Appearance.SetLayer(CharacterAppearanceSlotIds.LowerOne, CharacterAppearanceData.NoneAssetId);

    var validation = CharacterPresetValidator.Validate(preset);
    Assert(!validation.IsValid, "Preset with no pants/lower layer should be invalid.");
    Assert(validation.Errors.Any(error => error.MessageKey == "character.validation.appearance_required"), "Validation should include required appearance error.");
}

static void ManaSeedAssetParserKeepsStableIds()
{
    var parsed = ManaSeedAssetNameParser.TryParse(
        "farmer_base_sheets/14head/fbas_14head_headscarf_00b_e.png",
        out var parts);

    Assert(parsed, "Expected parser to accept Mana Seed filename.");
    AssertEqual("fbas_14head_headscarf_00b_e", parts.AssetId, "Asset ID should be filename without extension.");
    AssertEqual("14head", parts.SlotId, "Slot ID should be parsed.");
    AssertEqual("e", parts.SpecialId, "Special ID should be parsed.");
}

static void SaveGameRoundTripsCharacterPreset()
{
    var preset = CharacterPresetFactory.CreateDefault();
    preset.Name = "Rook";
    preset.Appearance.SetLayer(CharacterAppearanceSlotIds.Hair, "fbas_13hair_mohawk_00_e");
    preset.Appearance.SetPalette(CharacterAppearanceSlotIds.Hair, "hair_red");

    var saveGame = SaveGame.CreateNew(preset);
    var json = SaveGameSerializer.Serialize(saveGame);
    var loaded = SaveGameSerializer.Deserialize(json);

    AssertEqual("Rook", loaded.PlayerState.CharacterPreset.Name, "Save should keep player preset name.");
    AssertEqual("fbas_13hair_mohawk_00_e", loaded.PlayerState.CharacterPreset.Appearance.GetLayer(CharacterAppearanceSlotIds.Hair), "Save should keep appearance.");
    AssertEqual("hair_red", loaded.PlayerState.CharacterPreset.Appearance.GetPalette(CharacterAppearanceSlotIds.Hair), "Save should keep palette.");
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
    }
}
