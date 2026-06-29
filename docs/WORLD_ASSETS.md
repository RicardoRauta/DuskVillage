# World Assets

World visuals are loaded through a data-driven catalog instead of being committed to Git.

## Seasonal Forest Packs

The initial world asset catalog lives in:

```text
DuskVillage/Data/WorldAssets/seasonal_assets.json
```

It maps each `WorldClock` season to a local Mana Seed forest zip in `DuskVillage/Content`:

```text
DuskVillage/Content/20.05c - Spring Forest 4.3.zip
DuskVillage/Content/20.04c - Summer Forest 4.3.zip
DuskVillage/Content/20.06a - Autumn Forest 4.3.zip
DuskVillage/Content/20.07a - Winter Forest 4.3.zip
```

These files are paid/licensed local assets and remain ignored by Git. The repository stores only stable IDs, zip file names, zip entry paths, tile sizes, and animation timing hints.

## Runtime Loading

`SeasonalWorldAssetCatalog` reads JSON definitions from `Data/WorldAssets` and resolves zip files from known `Content` locations. Missing zips or missing entries do not crash catalog loading; the affected assets are marked unavailable.

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
