# World Assets

World visuals are loaded through a data-driven catalog instead of being committed to Git.

## Seasonal Forest Packs

The initial world asset catalog lives in:

```text
DuskVillage/Data/WorldAssets/seasonal_assets.json
```

It maps each `WorldClock` season to a local Mana Seed forest zip under `DuskVillage/Content`:

```text
DuskVillage/Content/Packs/World/Seasonal/20.05c - Spring Forest 4.3.zip
DuskVillage/Content/Packs/World/Seasonal/20.04c - Summer Forest 4.3.zip
DuskVillage/Content/Packs/World/Seasonal/20.06a - Autumn Forest 4.3.zip
DuskVillage/Content/Packs/World/Seasonal/20.07a - Winter Forest 4.3.zip
```

These files are paid/licensed local assets and remain ignored by Git. The repository stores only stable IDs, zip file names, zip entry paths, tile sizes, and animation timing hints.

## Local Pack Organization

Local paid or prototype packs should stay under `DuskVillage/Content/Packs`:

```text
Packs/Characters
Packs/Farming
Packs/Crafting
Packs/Creatures
Packs/UI
Packs/UI/Inventory
Packs/World/Buildings
Packs/World/Props
Packs/World/Seasonal
```

`DuskVillage/Content/**` is ignored by Git except for `Content.mgcb` and spritefont files, so these zips are not versioned. Runtime catalogs resolve packs recursively by zip file name, which keeps data definitions stable even if local folders are reorganized.

The current UI inventory candidates are:

```text
Pocket Inventory Series #8 BackPack v1.1.zip - good fit for traveler inventory.
Pocket Inventory Series #1 Adventure Book.zip - good fit for village role/job records.
Pocket Inventory Series #7 Gems of Status.zip - useful for status/equipment UI pieces.
```

The inspected UI packs expose buttons, slots, holders, icons, and category art, but no dedicated keyboard hotkey glyph set. Hotkeys should be rendered as text labels over button or slot sprites until a dedicated key glyph pack is added.

## Runtime Loading

`SeasonalWorldAssetCatalog` reads JSON definitions from `Data/WorldAssets` and resolves zip files from known `Content` locations, including nested pack folders. Missing zips or missing entries do not crash catalog loading; the affected assets are marked unavailable.

`SeasonalWorldTextureProvider` loads textures from zip entries on demand and caches them. If a texture cannot be loaded, it can return a small seasonal fallback texture so the map can still render during development or on machines without the paid packs.

## Built-in Asset IDs

The first map systems should use these stable IDs:

```text
terrain_wang
terrain_objects
trees
tall_grass_effect
```

The current default variants are:

```text
spring: default
summer: default
autumn: leaves
winter: snowy
```

## Scope

This module does not import Tiled `.tmx` or `.tsx` files yet. The next map implementation should keep playable world state in Dusk Village data structures and use this catalog only as the visual asset layer.
