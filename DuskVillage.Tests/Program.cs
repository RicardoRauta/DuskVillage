using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.Players;
using DuskVillage.Saving;
using DuskVillage.World;

var tests = new (string Name, Action Run)[]
{
    ("Default preset validates", DefaultPresetValidates),
    ("Preset JSON round-trips", PresetJsonRoundTrips),
    ("Attribute budget rejects invalid preset", AttributeBudgetRejectsInvalidPreset),
    ("Attribute randomizer respects point buy", AttributeRandomizerRespectsPointBuy),
    ("Identity fields validate", IdentityFieldsValidate),
    ("Required appearance slots reject none", RequiredAppearanceSlotsRejectNone),
    ("Mana Seed asset parser keeps stable IDs", ManaSeedAssetParserKeepsStableIds),
    ("World clock default starts at morning", WorldClockDefaultStartsAtMorning),
    ("World clock advances within day", WorldClockAdvancesWithinDay),
    ("World clock formats past midnight", WorldClockFormatsPastMidnight),
    ("World clock forces day end after late night", WorldClockForcesDayEndAfterLateNight),
    ("World calendar rolls season and year", WorldCalendarRollsSeasonAndYear),
    ("Player runtime starts from preset needs", PlayerRuntimeStartsFromPresetNeeds),
    ("Old save player runtime normalizes", OldSavePlayerRuntimeNormalizes),
    ("Old save world time normalizes", OldSaveWorldTimeNormalizes),
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

static void WorldClockDefaultStartsAtMorning()
{
    var time = WorldClock.CreateDefault();

    AssertEqual(1, time.Day, "Default day should start at 1.");
    AssertEqual(WorldCalendarRules.Spring, time.CurrentSeason, "Default season should be spring.");
    AssertEqual(1, time.Year, "Default year should start at 1.");
    AssertEqual("06:00", time.CurrentTime, "Default time should start at 06:00.");
}

static void WorldClockAdvancesWithinDay()
{
    var result = WorldClock.Advance(WorldClock.CreateDefault(), 60);

    Assert(!result.StartedNewDay, "Advancing one hour from morning should not start a new day.");
    Assert(!result.ForcedDayEnd, "Advancing one hour from morning should not force day end.");
    AssertEqual("07:00", result.Time.CurrentTime, "06:00 plus one hour should be 07:00.");
}

static void WorldClockFormatsPastMidnight()
{
    var time = new WorldTime
    {
        Day = 1,
        TimeMinutes = 23 * 60 + 30
    };

    var result = WorldClock.Advance(time, 60);

    Assert(!result.StartedNewDay, "Crossing midnight before the late-night limit should keep the same active day.");
    AssertEqual(1, result.Time.Day, "Midnight should still belong to the active day before forced day end.");
    AssertEqual("00:30", result.Time.CurrentTime, "Past midnight should format as 00:30.");
}

static void WorldClockForcesDayEndAfterLateNight()
{
    var time = new WorldTime
    {
        Day = 1,
        TimeMinutes = 90
    };

    var result = WorldClock.Advance(time, 60);

    Assert(result.StartedNewDay, "Advancing beyond 02:00 should start a new day.");
    Assert(result.ForcedDayEnd, "Advancing beyond 02:00 should be a forced day end.");
    AssertEqual(2, result.Time.Day, "Forced day end should advance the day.");
    AssertEqual("06:00", result.Time.CurrentTime, "Forced day end should wake at 06:00.");
}

static void WorldCalendarRollsSeasonAndYear()
{
    var day29 = WorldClock.Normalize(new WorldTime { Day = 29 });
    var day113 = WorldClock.Normalize(new WorldTime { Day = 113 });

    AssertEqual(WorldCalendarRules.Summer, day29.CurrentSeason, "Day 29 should be summer.");
    AssertEqual(1, day29.DayOfSeason, "Day 29 should be day 1 of summer.");
    AssertEqual(WorldCalendarRules.Spring, day113.CurrentSeason, "Day 113 should roll back to spring.");
    AssertEqual(2, day113.Year, "Day 113 should start year 2.");
}

static void OldSaveWorldTimeNormalizes()
{
    const string json = """
    {
      "metadata": {
        "saveVersion": 1,
        "gameVersion": "0.1.0",
        "playerName": "Legacy"
      },
      "worldState": {
        "day": 29,
        "timeMinutes": 480
      }
    }
    """;

    var loaded = SaveGameSerializer.Deserialize(json);

    AssertEqual(29, loaded.WorldState.Day, "Legacy save day should load.");
    AssertEqual("08:00", loaded.WorldState.CurrentTime, "Legacy save time should load.");
    AssertEqual(WorldCalendarRules.Summer, loaded.WorldState.CurrentSeason, "Legacy save season should be derived.");
    AssertEqual(1, loaded.WorldState.Year, "Legacy save year should be derived.");
}

static void PlayerRuntimeStartsFromPresetNeeds()
{
    var preset = CharacterPresetFactory.CreateDefault();
    preset.Needs.Energy = 72;
    preset.Needs.Hunger = 64;

    var runtime = PlayerRuntimeFactory.CreateNew(preset);

    AssertEqual(PlayerRuntimeFactory.DefaultPlayerEntityId, runtime.EntityId, "Runtime should use the default player entity.");
    AssertEqual(72, runtime.Needs.Energy, "Runtime needs should start from preset needs.");
    AssertEqual(64, runtime.Needs.Hunger, "Runtime hunger should start from preset needs.");
    AssertEqual(0, runtime.Money, "Runtime money should start at zero.");
    AssertEqual(PlayerRuntimeFactory.DefaultAreaId, runtime.Location.AreaId, "Runtime location should start at the default area.");

    runtime.Needs.Energy = 10;
    AssertEqual(72, preset.Needs.Energy, "Changing runtime needs should not mutate preset needs.");
}

static void OldSavePlayerRuntimeNormalizes()
{
    const string json = """
    {
      "metadata": {
        "saveVersion": 1,
        "gameVersion": "0.1.0",
        "playerName": "Legacy"
      },
      "worldState": {
        "day": 1,
        "timeMinutes": 360
      },
      "playerState": {
        "entityId": "player_main",
        "characterPreset": {
          "name": "Legacy",
          "needs": {
            "energy": 81,
            "maxEnergy": 100,
            "hunger": 77,
            "maxHunger": 100,
            "health": 96,
            "maxHealth": 100,
            "mood": 62,
            "maxMood": 100
          }
        }
      }
    }
    """;

    var loaded = SaveGameSerializer.Deserialize(json);

    AssertEqual("Legacy", loaded.PlayerState.CharacterPreset.Name, "Legacy save should keep player name.");
    AssertEqual(81, loaded.PlayerState.Needs.Energy, "Missing runtime needs should be copied from preset needs.");
    AssertEqual(77, loaded.PlayerState.Needs.Hunger, "Missing runtime hunger should be copied from preset needs.");
    AssertEqual(0, loaded.PlayerState.Money, "Missing money should default to zero.");
    AssertEqual(PlayerRuntimeFactory.DefaultAreaId, loaded.PlayerState.Location.AreaId, "Missing location should default.");
}

static void SaveGameRoundTripsCharacterPreset()
{
    var preset = CharacterPresetFactory.CreateDefault();
    preset.Name = "Rook";
    preset.Appearance.SetLayer(CharacterAppearanceSlotIds.Hair, "fbas_13hair_mohawk_00_e");
    preset.Appearance.SetPalette(CharacterAppearanceSlotIds.Hair, "hair_red");

    var saveGame = SaveGame.CreateNew(preset);
    saveGame.WorldState = SaveWorldState.FromWorldTime(new WorldTime
    {
        Day = 113,
        TimeMinutes = 75
    });
    saveGame.PlayerState.Money = 42;
    saveGame.PlayerState.Needs.Energy = 34;
    saveGame.PlayerState.Needs.Hunger = 56;
    saveGame.PlayerState.Location.AreaId = "starter_farm";
    saveGame.PlayerState.Location.TileX = 7;
    saveGame.PlayerState.Location.TileY = 9;

    var json = SaveGameSerializer.Serialize(saveGame);
    var loaded = SaveGameSerializer.Deserialize(json);

    AssertEqual("Rook", loaded.PlayerState.CharacterPreset.Name, "Save should keep player preset name.");
    AssertEqual("fbas_13hair_mohawk_00_e", loaded.PlayerState.CharacterPreset.Appearance.GetLayer(CharacterAppearanceSlotIds.Hair), "Save should keep appearance.");
    AssertEqual("hair_red", loaded.PlayerState.CharacterPreset.Appearance.GetPalette(CharacterAppearanceSlotIds.Hair), "Save should keep palette.");
    AssertEqual(113, loaded.WorldState.Day, "Save should keep world day.");
    AssertEqual("01:15", loaded.WorldState.CurrentTime, "Save should keep world time.");
    AssertEqual(WorldCalendarRules.Spring, loaded.WorldState.CurrentSeason, "Save should keep normalized season.");
    AssertEqual(2, loaded.WorldState.Year, "Save should keep normalized year.");
    AssertEqual(42, loaded.PlayerState.Money, "Save should keep runtime money.");
    AssertEqual(34, loaded.PlayerState.Needs.Energy, "Save should keep runtime energy.");
    AssertEqual(56, loaded.PlayerState.Needs.Hunger, "Save should keep runtime hunger.");
    AssertEqual("starter_farm", loaded.PlayerState.Location.AreaId, "Save should keep runtime area.");
    AssertEqual(7, loaded.PlayerState.Location.TileX, "Save should keep runtime tile X.");
    AssertEqual(9, loaded.PlayerState.Location.TileY, "Save should keep runtime tile Y.");
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
