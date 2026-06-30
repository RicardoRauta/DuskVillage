using DuskVillage.Animations;
using DuskVillage.Actions;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.Inventory;
using DuskVillage.InventoryAssets;
using DuskVillage.Items;
using DuskVillage.Needs;
using DuskVillage.Players;
using DuskVillage.Saving;
using DuskVillage.Settings;
using DuskVillage.World;
using DuskVillage.WorldAssets;
using DuskVillage.WorldMap;
using System.IO.Compression;
using Microsoft.Xna.Framework.Input;

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
    ("Seasonal world asset catalog loads stable IDs", SeasonalWorldAssetCatalogLoadsStableIds),
    ("Content resolver finds nested local packs", ContentResolverFindsNestedLocalPacks),
    ("Seasonal world asset catalog supports missing zips", SeasonalWorldAssetCatalogSupportsMissingZips),
    ("Inventory UI asset catalog loads stable IDs", InventoryUiAssetCatalogLoadsStableIds),
    ("Inventory UI asset catalog supports missing zips", InventoryUiAssetCatalogSupportsMissingZips),
    ("Input defaults open backpack with E", InputDefaultsOpenBackpackWithE),
    ("World map default has farm plot", WorldMapDefaultHasFarmPlot),
    ("World map movement respects passability", WorldMapMovementRespectsPassability),
    ("World map continuous movement supports fractional position", WorldMapContinuousMovementSupportsFractionalPosition),
    ("World map continuous movement blocks impassable tiles", WorldMapContinuousMovementBlocksImpassableTiles),
    ("World map target resolver uses facing", WorldMapTargetResolverUsesFacing),
    ("World map target resolver uses fractional position", WorldMapTargetResolverUsesFractionalPosition),
    ("World map actions change tile state", WorldMapActionsChangeTileState),
    ("World map action rejects invalid tile", WorldMapActionRejectsInvalidTile),
    ("Inventory stacks and removes items", InventoryStacksAndRemovesItems),
    ("Inventory operations keep owner inventories isolated", InventoryOperationsKeepOwnerInventoriesIsolated),
    ("Player runtime starts from preset needs", PlayerRuntimeStartsFromPresetNeeds),
    ("Old save player runtime normalizes", OldSavePlayerRuntimeNormalizes),
    ("Needs elapsed time changes runtime needs", NeedsElapsedTimeChangesRuntimeNeeds),
    ("Needs low hunger affects mood and health", NeedsLowHungerAffectsMoodAndHealth),
    ("Needs sleep restores energy", NeedsSleepRestoresEnergy),
    ("Character walk guide uses flip and durations", CharacterWalkGuideUsesFlipAndDurations),
    ("Character run guide stays separate from walk", CharacterRunGuideStaysSeparateFromWalk),
    ("Expanded character clips are four way", ExpandedCharacterClipsAreFourWay),
    ("Character animation timeline honors variable durations", CharacterAnimationTimelineHonorsVariableDurations),
    ("Farming action clips advance beyond windup", FarmingActionClipsAdvanceBeyondWindup),
    ("Character animation keeps timeline across repeated walk motion", CharacterAnimationKeepsTimelineAcrossRepeatedWalkMotion),
    ("Character animation cell coordinates are stable", CharacterAnimationCellCoordinatesAreStable),
    ("Action registry validates definitions", ActionRegistryValidatesDefinitions),
    ("Action execution applies effects and time", ActionExecutionAppliesEffectsAndTime),
    ("Action execution rejects invalid target", ActionExecutionRejectsInvalidTarget),
    ("Action execution consumes inventory items", ActionExecutionConsumesInventoryItems),
    ("Action execution rejects missing inventory item", ActionExecutionRejectsMissingInventoryItem),
    ("Action sleep advances to next day", ActionSleepAdvancesToNextDay),
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

static void SeasonalWorldAssetCatalogLoadsStableIds()
{
    var root = CreateTempDirectory();
    try
    {
        var definitionsDirectory = Path.Combine(root, "definitions");
        var contentDirectory = Path.Combine(root, "content");
        var seasonalPacksDirectory = Path.Combine(contentDirectory, "Packs", "World", "Seasonal");
        Directory.CreateDirectory(definitionsDirectory);
        Directory.CreateDirectory(seasonalPacksDirectory);

        File.WriteAllText(Path.Combine(definitionsDirectory, "seasonal_assets.json"), """
        [
          {
            "seasonId": "spring",
            "zipFileName": "spring.zip",
            "variantId": "default",
            "assets": [
              {
                "id": "terrain_wang",
                "entryPath": "spring sheets/spring forest wang tiles.png",
                "tileWidth": 16,
                "tileHeight": 16
              }
            ]
          }
        ]
        """);
        CreateZipWithEntries(
            Path.Combine(seasonalPacksDirectory, "spring.zip"),
            "spring sheets/spring forest wang tiles.png");

        var catalog = SeasonalWorldAssetCatalog.LoadFromDirectories([definitionsDirectory], [contentDirectory]);

        Assert(catalog.TryGetPack(WorldCalendarRules.Spring, out var pack), "Catalog should load spring pack.");
        AssertEqual("default", pack.VariantId, "Spring variant should load.");
        Assert(pack.IsAvailable, "Pack should be available when the zip entry exists.");

        var asset = pack.FindAsset(SeasonalWorldAssetIds.TerrainWang);
        Assert(asset != null, "Pack should find the stable terrain asset ID.");
        AssertEqual("spring/terrain_wang", asset.StableId, "Stable asset ID should combine season and asset ID.");
        AssertEqual(16, asset.TileWidth, "Tile width should load.");
        AssertEqual(16, asset.TileHeight, "Tile height should load.");
        Assert(asset.ZipExists, "Asset should report existing zip.");
        Assert(asset.EntryExists, "Asset should report existing zip entry.");
        Assert(asset.IsAvailable, "Asset should be available when zip and entry exist.");
    }
    finally
    {
        DeleteTempDirectory(root);
    }
}

static void ContentResolverFindsNestedLocalPacks()
{
    var root = CreateTempDirectory();
    try
    {
        var contentDirectory = Path.Combine(root, "content");
        var nestedDirectory = Path.Combine(contentDirectory, "Packs", "World", "Seasonal");
        Directory.CreateDirectory(nestedDirectory);
        File.WriteAllBytes(Path.Combine(nestedDirectory, "spring.zip"), [0, 1, 2, 3]);

        var resolved = GameDirectories.ResolveContentFile("spring.zip", [contentDirectory]);

        AssertEqual(Path.Combine(nestedDirectory, "spring.zip"), resolved, "Resolver should find packs organized in nested folders.");
    }
    finally
    {
        DeleteTempDirectory(root);
    }
}

static void SeasonalWorldAssetCatalogSupportsMissingZips()
{
    var root = CreateTempDirectory();
    try
    {
        var definitionsDirectory = Path.Combine(root, "definitions");
        var contentDirectory = Path.Combine(root, "content");
        Directory.CreateDirectory(definitionsDirectory);
        Directory.CreateDirectory(contentDirectory);

        File.WriteAllText(Path.Combine(definitionsDirectory, "seasonal_assets.json"), """
        [
          {
            "seasonId": "winter",
            "zipFileName": "missing.zip",
            "variantId": "snowy",
            "assets": [
              {
                "id": "trees",
                "entryPath": "winter sheets/winter trees (snowy) 80x112.png",
                "tileWidth": 80,
                "tileHeight": 112
              }
            ]
          }
        ]
        """);

        var catalog = SeasonalWorldAssetCatalog.LoadFromDirectories([definitionsDirectory], [contentDirectory]);

        Assert(catalog.TryGetPack(WorldCalendarRules.Winter, out var pack), "Catalog should load definition even when zip is missing.");
        Assert(!pack.IsAvailable, "Pack should be unavailable when its zip is missing.");

        var asset = pack.FindAsset(SeasonalWorldAssetIds.Trees);
        Assert(asset != null, "Missing zip should not remove the asset definition.");
        AssertEqual("winter/trees", asset.StableId, "Missing assets should keep stable IDs.");
        Assert(!asset.ZipExists, "Asset should report missing zip.");
        Assert(!asset.EntryExists, "Asset should report missing zip entry.");
    Assert(!asset.IsAvailable, "Asset should be unavailable when zip is missing.");
    }
    finally
    {
        DeleteTempDirectory(root);
    }
}

static void InventoryUiAssetCatalogLoadsStableIds()
{
    var root = CreateTempDirectory();
    try
    {
        var definitionsDirectory = Path.Combine(root, "definitions");
        var contentDirectory = Path.Combine(root, "content");
        var inventoryPacksDirectory = Path.Combine(contentDirectory, "Packs", "UI", "Inventory");
        Directory.CreateDirectory(definitionsDirectory);
        Directory.CreateDirectory(inventoryPacksDirectory);

        File.WriteAllText(Path.Combine(definitionsDirectory, "inventory_assets.json"), """
        [
          {
            "id": "traveler_backpack",
            "labelKey": "inventory.skin.traveler_backpack",
            "zipFileName": "backpack.zip",
            "variantId": "green",
            "assets": [
              {
                "id": "slot_holder",
                "entryPath": "Sprites/Green/Categories/1 - Inventory/Sprites/Holders/0.png",
                "width": 47,
                "height": 48
              }
            ]
          }
        ]
        """);
        CreateZipWithEntries(
            Path.Combine(inventoryPacksDirectory, "backpack.zip"),
            "Sprites/Green/Categories/1 - Inventory/Sprites/Holders/0.png");

        var catalog = InventoryUiAssetCatalog.LoadFromDirectories([definitionsDirectory], [contentDirectory]);

        Assert(catalog.TryGetSkin(InventoryUiAssetIds.TravelerBackpackSkin, out var skin), "Catalog should load traveler backpack skin.");
        AssertEqual("green", skin.VariantId, "Skin variant should load.");
        Assert(skin.IsAvailable, "Skin should be available when a referenced zip entry exists.");

        var asset = skin.FindAsset(InventoryUiAssetIds.SlotHolder);
        Assert(asset != null, "Skin should find slot holder by stable ID.");
        AssertEqual("traveler_backpack/slot_holder", asset.StableId, "Stable asset ID should combine skin and asset ID.");
        AssertEqual(47, asset.Width, "Asset width should load.");
        AssertEqual(48, asset.Height, "Asset height should load.");
        Assert(asset.ZipExists, "Asset should report existing zip.");
        Assert(asset.EntryExists, "Asset should report existing entry.");
        Assert(asset.IsAvailable, "Asset should be available when zip and entry exist.");
    }
    finally
    {
        DeleteTempDirectory(root);
    }
}

static void InventoryUiAssetCatalogSupportsMissingZips()
{
    var root = CreateTempDirectory();
    try
    {
        var definitionsDirectory = Path.Combine(root, "definitions");
        var contentDirectory = Path.Combine(root, "content");
        Directory.CreateDirectory(definitionsDirectory);
        Directory.CreateDirectory(contentDirectory);

        File.WriteAllText(Path.Combine(definitionsDirectory, "inventory_assets.json"), """
        [
          {
            "id": "traveler_backpack",
            "labelKey": "inventory.skin.traveler_backpack",
            "zipFileName": "missing.zip",
            "variantId": "green",
            "assets": [
              {
                "id": "slot_highlight",
                "entryPath": "Sprites/Green/Categories/1 - Inventory/Sprites/HighLighter/0.png",
                "width": 61,
                "height": 59
              }
            ]
          }
        ]
        """);

        var catalog = InventoryUiAssetCatalog.LoadFromDirectories([definitionsDirectory], [contentDirectory]);

        Assert(catalog.TryGetSkin(InventoryUiAssetIds.TravelerBackpackSkin, out var skin), "Catalog should load definition even when zip is missing.");
        Assert(!skin.IsAvailable, "Skin should be unavailable when the zip is missing.");

        var asset = skin.FindAsset(InventoryUiAssetIds.SlotHighlight);
        Assert(asset != null, "Missing zip should not remove the asset definition.");
        AssertEqual("traveler_backpack/slot_highlight", asset.StableId, "Missing assets should keep stable IDs.");
        Assert(!asset.ZipExists, "Asset should report missing zip.");
        Assert(!asset.EntryExists, "Asset should report missing entry.");
        Assert(!asset.IsAvailable, "Asset should be unavailable when zip is missing.");
    }
    finally
    {
        DeleteTempDirectory(root);
    }
}

static void InputDefaultsOpenBackpackWithE()
{
    var defaults = GameSettings.CreateDefault();

    AssertEqual(Keys.E, defaults.Input.Inventory, "Default inventory key should open the backpack with E.");
    AssertEqual(Keys.F, defaults.Input.Interact, "Default use-selected key should avoid conflicting with backpack.");

    var legacy = GameSettings.CreateDefault();
    legacy.Input.Inventory = Keys.I;
    legacy.Input.Interact = Keys.E;
    legacy.Normalize();

    AssertEqual(Keys.E, legacy.Input.Inventory, "Legacy inventory key should migrate to E.");
    AssertEqual(Keys.F, legacy.Input.Interact, "Legacy interact key should migrate away from E.");
}

static void WorldMapDefaultHasFarmPlot()
{
    var map = WorldMapFactory.CreateDefault();

    AssertEqual(WorldMapFactory.DefaultWidth, map.Width, "Default map width should be stable.");
    AssertEqual(WorldMapFactory.DefaultHeight, map.Height, "Default map height should be stable.");
    AssertEqual(WorldMapTileTypeIds.Water, WorldMapRules.GetTile(map, 0, 0).TypeId, "Default map should block the border with water.");
    AssertEqual(WorldMapTileTypeIds.Soil, WorldMapRules.GetTile(map, 4, 3).TypeId, "Default map should include a farmable soil plot.");
    AssertEqual(WorldMapTileTypeIds.Path, WorldMapRules.GetTile(map, 2, 6).TypeId, "Default map should include a path.");
}

static void WorldMapMovementRespectsPassability()
{
    var map = WorldMapFactory.CreateDefault();
    var location = new PlayerLocationState
    {
        AreaId = map.AreaId
    };
    location.SetTile(1, 1);

    var blocked = WorldMapTargetResolver.TryMove(map, location, CharacterFacingDirection.Left);
    var moved = WorldMapTargetResolver.TryMove(map, location, CharacterFacingDirection.Right);

    Assert(!blocked.Moved, "Movement into water border should be blocked.");
    AssertEqual(1, blocked.Location.TileX, "Blocked movement should keep X.");
    Assert(moved.Moved, "Movement into passable tile should succeed.");
    AssertEqual(2, moved.Location.TileX, "Movement should update X.");
}

static void WorldMapContinuousMovementSupportsFractionalPosition()
{
    var map = WorldMapFactory.CreateDefault();
    var location = new PlayerLocationState
    {
        AreaId = map.AreaId
    };
    location.SetTile(7, 6);

    var result = WorldMapContinuousMovementSystem.Move(map, location, 1, 0, 0.1, 4.5);

    Assert(result.Moved, "Continuous movement should move inside passable tiles.");
    Assert(result.Location.GetPositionX() > 7.5, "Continuous movement should advance X beyond the starting foot position.");
    Assert(result.Location.GetPositionX() < 8, "Short continuous movement should be able to stop between tiles.");
    AssertEqual(7, result.Location.TileX, "Small fractional movement should keep the current tile until crossing the tile edge.");
    AssertApprox(7.95, result.Location.GetPositionX(), 0.001, "Continuous movement should preserve fractional X.");
    AssertApprox(6.5, result.Location.GetPositionY(), 0.001, "Continuous movement should preserve Y.");
}

static void WorldMapContinuousMovementBlocksImpassableTiles()
{
    var map = WorldMapFactory.CreateDefault();
    var location = new PlayerLocationState
    {
        AreaId = map.AreaId
    };
    location.SetTile(1, 1);

    var result = WorldMapContinuousMovementSystem.Move(map, location, -1, 0, 1, 4.5);

    Assert(!result.Moved, "Continuous movement should not enter impassable water.");
    Assert(result.Blocked, "Continuous movement should report blocked movement.");
    AssertApprox(1.5, result.Location.GetPositionX(), 0.001, "Blocked movement should keep X.");
    AssertApprox(1.5, result.Location.GetPositionY(), 0.001, "Blocked movement should keep Y.");
}

static void WorldMapTargetResolverUsesFacing()
{
    var location = new PlayerLocationState
    {
        AreaId = WorldMapFactory.DefaultAreaId
    };
    location.SetTile(7, 7);

    var up = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Up);
    var right = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Right);
    var left = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Left);

    AssertEqual((7, 5), up, "Up should target the tile at the head anchor.");
    AssertEqual((8, 7), right, "Right should target the tile in front of the side-facing feet.");
    AssertEqual((6, 7), left, "Left should target the tile in front of the side-facing feet.");
}

static void WorldMapTargetResolverUsesFractionalPosition()
{
    var location = new PlayerLocationState
    {
        AreaId = WorldMapFactory.DefaultAreaId
    };
    location.SetPosition(6.5, 6.5);

    var down = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Down);
    var right = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Right);
    var left = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Left);
    var up = WorldMapTargetResolver.ResolveAdjacentTile(location, CharacterFacingDirection.Up);

    AssertEqual((6, 7), down, "Down should target the tile in front of the toe tips.");
    AssertEqual((7, 6), right, "Side targeting should target the tile in front of the side-facing feet.");
    AssertEqual((5, 6), left, "Side targeting should target the tile in front of the side-facing feet.");
    AssertEqual((6, 4), up, "Up should target the tile at the head anchor.");
}

static void WorldMapActionsChangeTileState()
{
    var registry = CreateTestActionRegistry();
    var map = WorldMapFactory.CreateDefault();
    var player = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());
    player.Location.AreaId = map.AreaId;
    player.Location.SetTile(4, 6);
    player.Needs.Energy = 80;
    player.Needs.Hunger = 70;

    var plant = WorldMapActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_plant",
            ActorEntityId = player.EntityId,
            FacingDirection = CharacterFacingDirection.Up,
            Target = GameActionTarget.Tile(map.AreaId, 4, 5)
        },
        WorldClock.CreateDefault(),
        player,
        map);

    Assert(plant.Succeeded, "Planting on empty soil should succeed.");
    AssertEqual(WorldMapTileStateIds.Planted, WorldMapRules.GetTile(plant.MapState, 4, 5).StateId, "Planting should update tile state.");
    AssertEqual(WorldMapCropIds.StarterSeeds, WorldMapRules.GetTile(plant.MapState, 4, 5).CropId, "Planting should set crop id.");
    AssertEqual(WorldMapTileStateIds.Empty, WorldMapRules.GetTile(map, 4, 5).StateId, "Planting should not mutate original map.");
    AssertEqual("06:10", plant.ActionResult.WorldTime.CurrentTime, "Planting should use action time cost.");
    AssertEqual(77, plant.ActionResult.PlayerState.Needs.Energy, "Planting should apply energy cost.");
    AssertEqual(14, InventoryQuantity(plant.ActionResult.PlayerState.Inventory, ItemIds.StarterSeeds), "Planting should consume one seed.");

    var water = WorldMapActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_water",
            ActorEntityId = plant.ActionResult.PlayerState.EntityId,
            FacingDirection = CharacterFacingDirection.Up,
            Target = GameActionTarget.Tile(plant.MapState.AreaId, 4, 5)
        },
        plant.ActionResult.WorldTime,
        plant.ActionResult.PlayerState,
        plant.MapState);

    Assert(water.Succeeded, "Watering a planted crop should succeed.");
    AssertEqual(WorldMapTileStateIds.Watered, WorldMapRules.GetTile(water.MapState, 4, 5).StateId, "Watering should update tile state.");
    AssertEqual("06:20", water.ActionResult.WorldTime.CurrentTime, "Watering should use action time cost.");
    AssertEqual(73, water.ActionResult.PlayerState.Needs.Energy, "Watering should apply energy cost.");
    AssertEqual(1, InventoryQuantity(water.ActionResult.PlayerState.Inventory, ItemIds.WateringCanBasic), "Watering should require but not consume the watering can.");
}

static void WorldMapActionRejectsInvalidTile()
{
    var registry = CreateTestActionRegistry();
    var map = WorldMapFactory.CreateDefault();
    var player = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());
    player.Needs.Energy = 80;

    var result = WorldMapActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_plant",
            ActorEntityId = player.EntityId,
            FacingDirection = CharacterFacingDirection.Right,
            Target = GameActionTarget.Tile(map.AreaId, 3, 3)
        },
        WorldClock.CreateDefault(),
        player,
        map);

    Assert(!result.Succeeded, "Planting on grass should fail.");
    AssertEqual("world.map.not_plantable", result.MessageKey, "Invalid plant target should explain the failure.");
    AssertEqual("06:00", result.ActionResult.WorldTime.CurrentTime, "Invalid target should not spend time.");
    AssertEqual(80, result.ActionResult.PlayerState.Needs.Energy, "Invalid target should not spend energy.");
}

static void InventoryStacksAndRemovesItems()
{
    var items = CreateTestItemRegistry();
    var inventory = InventorySystem.CreateDefault();

    AssertEqual("backpack_icon_seeds", items.Find(ItemIds.StarterSeeds).IconAssetId, "Item definitions should keep stable icon asset IDs.");

    var added = InventorySystem.AddItem(inventory, ItemIds.StarterSeeds, 120, items);
    Assert(added.Succeeded, "Adding seeds should succeed.");
    AssertEqual(120, InventoryQuantity(added.Inventory, ItemIds.StarterSeeds), "Inventory should keep the full seed quantity.");
    Assert(added.Inventory.Slots.Count(slot => !slot.IsEmpty && slot.ItemId == ItemIds.StarterSeeds) > 1, "Seed overflow should use a second stack.");

    var removed = InventorySystem.RemoveItem(added.Inventory, ItemIds.StarterSeeds, 21, items);
    Assert(removed.Succeeded, "Removing seeds should succeed.");
    AssertEqual(99, InventoryQuantity(removed.Inventory, ItemIds.StarterSeeds), "Inventory should subtract removed quantity.");

    var selected = InventorySystem.SelectHotbarSlot(removed.Inventory, 0, items);
    AssertEqual(ItemIds.StarterSeeds, InventorySystem.GetSelectedHotbarSlot(selected, items).ItemId, "Hotbar selection should expose selected slot.");
}

static void InventoryOperationsKeepOwnerInventoriesIsolated()
{
    var items = CreateTestItemRegistry();
    var playerInventory = InventorySystem.CreateStarterInventory(items);
    var npcInventory = InventorySystem.CreateDefault();

    var updatedNpc = InventorySystem.AddItem(npcInventory, ItemIds.StarterSeeds, 3, items);

    Assert(updatedNpc.Succeeded, "NPC inventory operation should succeed.");
    AssertEqual(15, InventoryQuantity(playerInventory, ItemIds.StarterSeeds), "Player inventory should not change when an NPC inventory changes.");
    AssertEqual(0, InventoryQuantity(npcInventory, ItemIds.StarterSeeds), "Original NPC inventory state should remain unchanged.");
    AssertEqual(3, InventoryQuantity(updatedNpc.Inventory, ItemIds.StarterSeeds), "Updated NPC inventory should receive the item stack.");
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
    AssertEqual(WorldMapFactory.DefaultWidth, loaded.WorldState.Map.Width, "Legacy save should receive default map width.");
    AssertEqual(WorldMapFactory.DefaultAreaId, loaded.WorldState.Map.AreaId, "Legacy save should receive default map area.");
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
    AssertApprox(WorldMapFactory.DefaultPlayerTileX + 0.5, runtime.Location.GetPositionX(), 0.001, "Runtime X position should start between the feet at the default spawn.");
    AssertApprox(WorldMapFactory.DefaultPlayerTileY + 0.5, runtime.Location.GetPositionY(), 0.001, "Runtime Y position should start between the feet at the default spawn.");

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

static void NeedsElapsedTimeChangesRuntimeNeeds()
{
    var needs = new CharacterNeedsBlock
    {
        Energy = 80,
        MaxEnergy = 100,
        Hunger = 90,
        MaxHunger = 100,
        Health = 100,
        MaxHealth = 100,
        Mood = 75,
        MaxMood = 100
    };

    var result = NeedsSystem.ApplyElapsedTime(needs, 60);

    AssertEqual(80, needs.Energy, "Elapsed time should not mutate the original needs block.");
    AssertEqual(90, needs.Hunger, "Elapsed time should not mutate original hunger.");
    AssertEqual(78, result.Needs.Energy, "Elapsed time should reduce energy.");
    AssertEqual(87, result.Needs.Hunger, "Elapsed time should reduce hunger.");
    Assert(result.EnergyChanged, "Elapsed time result should report energy change.");
    Assert(result.HungerChanged, "Elapsed time result should report hunger change.");
}

static void NeedsLowHungerAffectsMoodAndHealth()
{
    var lowHunger = new CharacterNeedsBlock
    {
        Energy = 30,
        MaxEnergy = 100,
        Hunger = 2,
        MaxHunger = 100,
        Health = 50,
        MaxHealth = 100,
        Mood = 40,
        MaxMood = 100
    };

    var starving = NeedsSystem.ApplyElapsedTime(lowHunger, 60);

    Assert(starving.IsHungry, "Low hunger should be flagged.");
    Assert(starving.IsStarving, "Zero hunger should be flagged as starving.");
    AssertEqual(0, starving.Needs.Hunger, "Hunger should clamp at zero.");
    AssertEqual(47, starving.Needs.Health, "Starving should reduce health.");
    AssertEqual(38, starving.Needs.Mood, "Low hunger should reduce mood.");
}

static void NeedsSleepRestoresEnergy()
{
    var tired = new CharacterNeedsBlock
    {
        Energy = 12,
        MaxEnergy = 90,
        Hunger = 70,
        MaxHunger = 100,
        Health = 80,
        MaxHealth = 100,
        Mood = 50,
        MaxMood = 100
    };

    var slept = NeedsSystem.ApplySleep(tired);

    AssertEqual(90, slept.Needs.Energy, "Sleep should restore energy to max.");
    AssertEqual(62, slept.Needs.Hunger, "Sleep should consume hunger.");
    AssertEqual(84, slept.Needs.Health, "Sleep should recover health if hunger is not low.");
    AssertEqual(56, slept.Needs.Mood, "Sleep should recover mood.");
}

static void CharacterWalkGuideUsesFlipAndDurations()
{
    var clip = CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Walk, CharacterFacingDirection.Down);
    var sideClip = CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Walk, CharacterFacingDirection.Right);
    var leftClip = CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Walk, CharacterFacingDirection.Left);

    AssertEqual(6, clip.Frames.Count, "Down walk should use the six guide frames.");
    AssertEqual(48, clip.Frames[0].CellIndex, "First down walk frame should use cell 048.");
    AssertEqual(135, clip.Frames[0].DurationMilliseconds, "First down walk frame should use guide duration 135.");
    AssertEqual(49, clip.Frames[1].CellIndex, "Second down walk frame should use cell 049.");
    AssertEqual(135, clip.Frames[1].DurationMilliseconds, "Second down walk frame should use guide duration 135.");
    AssertEqual(50, clip.Frames[2].CellIndex, "Third down walk frame should use cell 050.");
    AssertEqual(135, clip.Frames[2].DurationMilliseconds, "Third down walk frame should use guide duration 135.");
    Assert(clip.Frames[3].FlipX, "Guide reverse marker should become flipX on repeated down walk frames.");

    AssertEqual(64, sideClip.Frames[0].CellIndex, "First side walk frame should use cell 064.");
    AssertEqual(135, sideClip.Frames[0].DurationMilliseconds, "First side walk frame should use guide duration 135.");
    AssertEqual(66, sideClip.Frames[2].CellIndex, "Third side walk frame should use cell 066.");
    AssertEqual(135, sideClip.Frames[2].DurationMilliseconds, "Third side walk frame should use guide duration 135.");
    AssertEqual(69, sideClip.Frames[5].CellIndex, "Last side walk frame should use cell 069.");
    AssertEqual(135, sideClip.Frames[5].DurationMilliseconds, "Last side walk frame should use guide duration 135.");
    Assert(leftClip.Frames.All(frame => frame.FlipX), "Left walk should reuse right-facing cells with flipX.");
}

static void CharacterRunGuideStaysSeparateFromWalk()
{
    var downRun = CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Run, CharacterFacingDirection.Down);
    var upRun = CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Run, CharacterFacingDirection.Up);
    var sideRun = CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Run, CharacterFacingDirection.Right);

    AssertEqual(51, downRun.Frames[2].CellIndex, "Down run should use cell 051, unlike walk.");
    AssertEqual(115, downRun.Frames[2].DurationMilliseconds, "Down run should keep guide duration 115.");
    AssertEqual(55, upRun.Frames[2].CellIndex, "Up run should use cell 055.");
    AssertEqual(70, sideRun.Frames[2].CellIndex, "Side run should use cell 070.");
    AssertEqual(71, sideRun.Frames[5].CellIndex, "Side run should use cell 071.");
}

static void ExpandedCharacterClipsAreFourWay()
{
    var animationIds = new[]
    {
        CharacterAnimationIds.Idle,
        CharacterAnimationIds.Walk,
        CharacterAnimationIds.Run,
        CharacterAnimationIds.Jump,
        CharacterAnimationIds.WalkCarry,
        CharacterAnimationIds.RunCarry,
        CharacterAnimationIds.JumpCarry,
        CharacterAnimationIds.Push,
        CharacterAnimationIds.Pull,
        CharacterAnimationIds.PickUpCarry,
        CharacterAnimationIds.ThrowCarry,
        CharacterAnimationIds.PlantSeeds,
        CharacterAnimationIds.Water,
        CharacterAnimationIds.WorkStation,
        CharacterAnimationIds.Wave,
        CharacterAnimationIds.Hug,
        CharacterAnimationIds.Sing,
        CharacterAnimationIds.LuteGuitar,
        CharacterAnimationIds.FluteOcarina,
        CharacterAnimationIds.Drums,
        CharacterAnimationIds.SitThrone,
        CharacterAnimationIds.LookAround,
        CharacterAnimationIds.SitLedge,
        CharacterAnimationIds.SitChair,
        CharacterAnimationIds.Meditate,
        CharacterAnimationIds.Sleep,
        CharacterAnimationIds.SleepSit,
        CharacterAnimationIds.ThumbsUp,
        CharacterAnimationIds.MadStomp,
        CharacterAnimationIds.Shocked,
        CharacterAnimationIds.Laugh,
        CharacterAnimationIds.DrinkStanding,
        CharacterAnimationIds.SitFloor,
        CharacterAnimationIds.Impatient
    };

    foreach (var animationId in animationIds)
    {
        foreach (var direction in Enum.GetValues<CharacterFacingDirection>())
        {
            var clip = CharacterAnimationCatalog.GetClip(animationId, direction);
            Assert(clip.Frames.Count > 0, $"{animationId}/{direction} should have frames.");
        }
    }
}

static void CharacterAnimationTimelineHonorsVariableDurations()
{
    var state = new CharacterAnimationState();
    CharacterAnimationSystem.SetMotion(state, CharacterAnimationIds.Walk, CharacterFacingDirection.Down);

    AssertEqual(48, CharacterAnimationSystem.GetCurrentFrame(state).CellIndex, "Walk should start on first frame.");

    CharacterAnimationSystem.Advance(state, TimeSpan.FromMilliseconds(134));
    AssertEqual(48, CharacterAnimationSystem.GetCurrentFrame(state).CellIndex, "Frame 048 should last through 134ms.");

    CharacterAnimationSystem.Advance(state, TimeSpan.FromMilliseconds(1));
    AssertEqual(49, CharacterAnimationSystem.GetCurrentFrame(state).CellIndex, "Frame 049 should start at 135ms.");

    CharacterAnimationSystem.Advance(state, TimeSpan.FromMilliseconds(135));
    AssertEqual(50, CharacterAnimationSystem.GetCurrentFrame(state).CellIndex, "Frame 050 should start after the second 135ms frame.");
}

static void FarmingActionClipsAdvanceBeyondWindup()
{
    var plant = new CharacterAnimationState();
    CharacterAnimationSystem.SetMotion(plant, CharacterAnimationIds.PlantSeeds, CharacterFacingDirection.Down);
    CharacterAnimationSystem.Advance(plant, TimeSpan.FromMilliseconds(400));

    var water = new CharacterAnimationState();
    CharacterAnimationSystem.SetMotion(water, CharacterAnimationIds.Water, CharacterFacingDirection.Down);
    CharacterAnimationSystem.Advance(water, TimeSpan.FromMilliseconds(350));

    AssertEqual(12, CharacterAnimationSystem.GetCurrentFrame(plant).CellIndex, "Planting should reach the seed-drop frame after the windup.");
    AssertEqual(133, CharacterAnimationSystem.GetCurrentFrame(water).CellIndex, "Watering should reach the pour frame after the windup.");
    Assert(CharacterAnimationCatalog.GetClip(CharacterAnimationIds.PlantSeeds, CharacterFacingDirection.Down).DurationMilliseconds > 400, "Planting must stay active long enough to show more than its first frame.");
    Assert(CharacterAnimationCatalog.GetClip(CharacterAnimationIds.Water, CharacterFacingDirection.Down).DurationMilliseconds > 350, "Watering must stay active long enough to show more than its first frame.");
}

static void CharacterAnimationKeepsTimelineAcrossRepeatedWalkMotion()
{
    var state = new CharacterAnimationState();
    CharacterAnimationSystem.SetMotion(state, CharacterAnimationIds.Walk, CharacterFacingDirection.Down);
    CharacterAnimationSystem.Advance(state, TimeSpan.FromMilliseconds(150));

    CharacterAnimationSystem.SetMotion(state, CharacterAnimationIds.Walk, CharacterFacingDirection.Down);

    AssertEqual(150, state.ElapsedMilliseconds, "Repeating the same walk motion should not reset elapsed animation time.");
    AssertEqual(49, CharacterAnimationSystem.GetCurrentFrame(state).CellIndex, "Walk should keep its current frame after repeated motion.");
}

static void CharacterAnimationCellCoordinatesAreStable()
{
    AssertEqual(1, CharacterAnimationCatalog.CellColumn(49), "Cell 049 should be in column 1.");
    AssertEqual(3, CharacterAnimationCatalog.CellRow(49), "Cell 049 should be in row 3.");
    AssertEqual(0, CharacterAnimationCatalog.CellColumn(64), "Cell 064 should start a new row.");
    AssertEqual(4, CharacterAnimationCatalog.CellRow(64), "Cell 064 should be in row 4.");
}

static void ActionRegistryValidatesDefinitions()
{
    var registry = CreateTestActionRegistry();

    Assert(registry.TryGet("test_work", out var definition), "Registry should find action by stable ID.");
    AssertEqual("animation.work_station", "animation." + definition.AnimationId, "Action should reference animation by ID.");
    AssertEqual(2, definition.Effects.Count, "Action should keep configured effects.");
}

static void ActionExecutionAppliesEffectsAndTime()
{
    var registry = CreateTestActionRegistry();
    var preset = CharacterPresetFactory.CreateDefault();
    var player = PlayerRuntimeFactory.CreateNew(preset);
    player.Needs.Energy = 80;
    player.Needs.Hunger = 70;

    var result = GameActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_work",
            ActorEntityId = player.EntityId,
            FacingDirection = CharacterFacingDirection.Right,
            Target = GameActionTarget.Self(player.EntityId)
        },
        WorldClock.CreateDefault(),
        player);

    Assert(result.Succeeded, "Valid action should succeed.");
    AssertEqual("06:30", result.WorldTime.CurrentTime, "Action time cost should advance world time.");
    AssertEqual(72, result.PlayerState.Needs.Energy, "Action should apply energy effect.");
    AssertEqual(70, result.PlayerState.Needs.Hunger, "Action should not apply passive elapsed needs by itself.");
    AssertEqual(5, result.PlayerState.Money, "Action should apply money effect.");
    AssertEqual(CharacterAnimationIds.WorkStation, result.AnimationId, "Action should expose animation ID.");
    AssertEqual(CharacterFacingDirection.Right, result.FacingDirection, "Action should preserve facing direction.");
    AssertEqual(80, player.Needs.Energy, "Action execution should not mutate original runtime state.");
}

static void ActionExecutionRejectsInvalidTarget()
{
    var registry = CreateTestActionRegistry();
    var player = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());

    var result = GameActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_tile",
            ActorEntityId = player.EntityId,
            Target = GameActionTarget.Self(player.EntityId)
        },
        WorldClock.CreateDefault(),
        player);

    Assert(!result.Succeeded, "Tile action should reject a self target.");
    AssertEqual("action.result.invalid_target", result.MessageKey, "Invalid target should return a clear message key.");
}

static void ActionExecutionConsumesInventoryItems()
{
    var registry = CreateTestActionRegistry();
    var player = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());
    player.Needs.Energy = 80;

    var result = GameActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_plant",
            ActorEntityId = player.EntityId,
            FacingDirection = CharacterFacingDirection.Down,
            Target = GameActionTarget.Tile(WorldMapFactory.DefaultAreaId, 4, 5)
        },
        WorldClock.CreateDefault(),
        player);

    Assert(result.Succeeded, "Plant action should succeed while seeds are present.");
    AssertEqual(14, InventoryQuantity(result.PlayerState.Inventory, ItemIds.StarterSeeds), "Plant action should consume one seed.");
    AssertEqual(77, result.PlayerState.Needs.Energy, "Plant action should still apply non-inventory effects.");
    AssertEqual("06:10", result.WorldTime.CurrentTime, "Plant action should spend time after item validation succeeds.");
}

static void ActionExecutionRejectsMissingInventoryItem()
{
    var registry = CreateTestActionRegistry();
    var player = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());
    player.Inventory = InventorySystem.CreateDefault();
    player.Needs.Energy = 80;

    var result = GameActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_plant",
            ActorEntityId = player.EntityId,
            FacingDirection = CharacterFacingDirection.Down,
            Target = GameActionTarget.Tile(WorldMapFactory.DefaultAreaId, 4, 5)
        },
        WorldClock.CreateDefault(),
        player);

    Assert(!result.Succeeded, "Plant action should fail without seeds.");
    AssertEqual("action.result.missing_item", result.MessageKey, "Missing item should return a clear message key.");
    AssertEqual(0, InventoryQuantity(result.PlayerState.Inventory, ItemIds.StarterSeeds), "Failed action should not create or consume seeds.");
    AssertEqual(80, result.PlayerState.Needs.Energy, "Failed action should not spend energy.");
    AssertEqual("06:00", result.WorldTime.CurrentTime, "Failed action should not spend time.");
}

static void ActionSleepAdvancesToNextDay()
{
    var registry = CreateTestActionRegistry();
    var player = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());
    player.Needs.Energy = 10;
    player.Needs.Hunger = 80;

    var result = GameActionSystem.Execute(
        registry,
        new GameActionRequest
        {
            ActionId = "test_sleep",
            ActorEntityId = player.EntityId,
            Target = GameActionTarget.Self(player.EntityId)
        },
        new WorldTime
        {
            Day = 7,
            TimeMinutes = 22 * 60
        },
        player);

    Assert(result.Succeeded, "Sleep action should succeed.");
    Assert(result.StartedNewDay, "Sleep action should start a new day.");
    AssertEqual(8, result.WorldTime.Day, "Sleep should advance to next day.");
    AssertEqual("06:00", result.WorldTime.CurrentTime, "Sleep should wake at morning.");
    AssertEqual(100, result.PlayerState.Needs.Energy, "Sleep should restore energy.");
    AssertEqual(72, result.PlayerState.Needs.Hunger, "Sleep should consume hunger.");
}

static GameActionRegistry CreateTestActionRegistry()
{
    return GameActionRegistry.FromDefinitions(
    [
        new GameActionDefinition
        {
            Id = "test_work",
            LabelKey = "action.work_station",
            TargetKind = GameActionTargetKinds.Self,
            AnimationId = CharacterAnimationIds.WorkStation,
            TimeCostMinutes = 30,
            Effects =
            [
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.ChangeNeed,
                    NeedId = GameActionNeedIds.Energy,
                    Amount = -8
                },
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.AddMoney,
                    Amount = 5
                }
            ]
        },
        new GameActionDefinition
        {
            Id = "test_tile",
            LabelKey = "action.water",
            TargetKind = GameActionTargetKinds.Tile,
            AnimationId = CharacterAnimationIds.Water
        },
        new GameActionDefinition
        {
            Id = "test_plant",
            LabelKey = "action.plant_seeds",
            TargetKind = GameActionTargetKinds.Tile,
            AnimationId = CharacterAnimationIds.PlantSeeds,
            TimeCostMinutes = 10,
            Effects =
            [
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.ConsumeItem,
                    ItemId = ItemIds.StarterSeeds,
                    Amount = 1
                },
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.PlantCrop,
                    CropId = WorldMapCropIds.StarterSeeds
                },
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.ChangeNeed,
                    NeedId = GameActionNeedIds.Energy,
                    Amount = -3
                }
            ]
        },
        new GameActionDefinition
        {
            Id = "test_water",
            LabelKey = "action.water",
            TargetKind = GameActionTargetKinds.Tile,
            AnimationId = CharacterAnimationIds.Water,
            TimeCostMinutes = 10,
            Effects =
            [
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.RequireItem,
                    ItemId = ItemIds.WateringCanBasic,
                    Amount = 1
                },
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.WaterTile
                },
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.ChangeNeed,
                    NeedId = GameActionNeedIds.Energy,
                    Amount = -4
                }
            ]
        },
        new GameActionDefinition
        {
            Id = "test_sleep",
            LabelKey = "action.sleep",
            TargetKind = GameActionTargetKinds.Self,
            AnimationId = CharacterAnimationIds.Sleep,
            Effects =
            [
                new GameActionEffectDefinition
                {
                    Type = GameActionEffectTypes.SleepToNextDay
                }
            ]
        }
    ]);
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
    saveGame.PlayerState.Location.SetPosition(7.35, 8.75);
    saveGame.PlayerState.Inventory = InventorySystem.SelectHotbarSlot(saveGame.PlayerState.Inventory, 1, CreateTestItemRegistry());
    saveGame.PlayerState.Inventory = InventorySystem.RemoveItem(saveGame.PlayerState.Inventory, ItemIds.StarterSeeds, 3, CreateTestItemRegistry()).Inventory;
    saveGame.WorldState.Map = WorldMapFactory.CreateDefault();
    var savedTile = WorldMapRules.GetTile(saveGame.WorldState.Map, 4, 5);
    savedTile.StateId = WorldMapTileStateIds.Watered;
    savedTile.CropId = WorldMapCropIds.StarterSeeds;

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
    AssertEqual(8, loaded.PlayerState.Location.TileY, "Save should keep runtime tile Y.");
    AssertApprox(7.35, loaded.PlayerState.Location.GetPositionX(), 0.001, "Save should keep fractional runtime X position.");
    AssertApprox(8.75, loaded.PlayerState.Location.GetPositionY(), 0.001, "Save should keep fractional runtime Y position.");
    AssertEqual(12, InventoryQuantity(loaded.PlayerState.Inventory, ItemIds.StarterSeeds), "Save should keep inventory seed quantity.");
    AssertEqual(1, InventoryQuantity(loaded.PlayerState.Inventory, ItemIds.WateringCanBasic), "Save should keep inventory tool quantity.");
    AssertEqual(1, loaded.PlayerState.Inventory.SelectedHotbarIndex, "Save should keep selected hotbar slot.");
    AssertEqual(WorldMapTileStateIds.Watered, WorldMapRules.GetTile(loaded.WorldState.Map, 4, 5).StateId, "Save should keep map tile state.");
    AssertEqual(WorldMapCropIds.StarterSeeds, WorldMapRules.GetTile(loaded.WorldState.Map, 4, 5).CropId, "Save should keep map crop id.");
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

static void AssertApprox(double expected, double actual, double tolerance, string message)
{
    if (Math.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException($"{message} Expected '{expected}', got '{actual}'.");
    }
}

static ItemDefinitionRegistry CreateTestItemRegistry()
{
    return ItemDefinitionRegistry.FromDefinitions(
    [
        new ItemDefinition
        {
            Id = ItemIds.StarterSeeds,
            LabelKey = "item.starter_seeds",
            IconAssetId = "backpack_icon_seeds",
            MaxStack = 99
        },
        new ItemDefinition
        {
            Id = ItemIds.WateringCanBasic,
            LabelKey = "item.watering_can_basic",
            IconAssetId = "backpack_icon_water",
            MaxStack = 1
        }
    ]);
}

static int InventoryQuantity(InventoryState inventory, string itemId)
{
    return InventorySystem.Normalize(inventory, CreateTestItemRegistry()).Slots
        .Where(slot => !slot.IsEmpty && slot.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase))
        .Sum(slot => slot.Quantity);
}

static string CreateTempDirectory()
{
    var directory = Path.Combine(Path.GetTempPath(), "DuskVillageTests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    return directory;
}

static void DeleteTempDirectory(string directory)
{
    if (Directory.Exists(directory))
    {
        Directory.Delete(directory, recursive: true);
    }
}

static void CreateZipWithEntries(string zipPath, params string[] entries)
{
    using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
    foreach (var entryPath in entries)
    {
        var entry = archive.CreateEntry(entryPath);
        using var stream = entry.Open();
        stream.Write([0, 1, 2, 3]);
    }
}
