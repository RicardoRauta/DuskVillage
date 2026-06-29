# World Map

`WorldMap` is the first playable world module. It owns grid state and tile interaction rules, but does not own UI, rendering, save files, character data, or asset loading.

## Current MVP

The default map is a small `starter_farm` grid:

```text
16 x 12 tiles
water border
grass field
path tiles
soil plot for early farming
```

The player starts at `starter_farm (7, 6)`, just below the starter soil plot. Movement remains tile-based in world state and save data, but the gameplay screen interpolates the rendered character between tiles so walking feels smooth. Non-passable tiles such as water still block movement.

## Tile Data

Each tile stores stable string IDs:

```text
typeId:  grass | soil | path | water
stateId: empty | tilled | planted | watered
cropId:  optional crop identifier
```

The first crop placeholder is:

```text
starter_seeds
```

## Actions

World map interactions are applied by `WorldMapActionSystem`. It composes the existing data-driven `Actions` module with map-specific validation and effects.

Supported map effects:

```text
plantCrop
waterTile
setTileState
```

The base `GameActionSystem` still handles actor state such as time, energy, hunger, money, and animation. `WorldMapActionSystem` handles tile validation and tile mutation after a valid action succeeds.

## Save Data

`SaveWorldState` now stores `Map`. Old saves without map data normalize to the default starter farm, so the save format remains backward-compatible for current development builds.

## Rendering

`WorldMapRenderer` draws the map from pure `WorldMapState`. It supports a fitted preview mode and a full-screen camera mode for gameplay. The camera mode fills the viewport, follows the visual player tile, and can crop map edges without changing the underlying map data.

The renderer uses `WorldAssets` for seasonal terrain when local zips are present and falls back to generated colors when paid/local assets are missing.

The renderer is intentionally separate from `WorldMap` so maps can later be rendered by different cameras, editors, debug screens, or mod tools without changing gameplay rules.
