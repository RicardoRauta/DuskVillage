# World Map

`WorldMap` is the first playable world module. It owns grid state and tile interaction rules, but does not own UI, rendering, save files, character data, or asset loading.

## Current MVP

The default map is a small `starter_farm` grid:

```text
24 x 18 tiles
water border
grass field
path tiles
soil plot for early farming
```

The player starts at `starter_farm (7, 6)`, just below the starter soil plot. Movement uses continuous tile-space coordinates, so the player can stop between tiles. `positionX` and `positionY` represent the gameplay anchor between the visible feet; `tileX` and `tileY` are derived with `floor(position)` and remain compatibility fields for targeting, actions, and old saves. Non-passable tiles such as water still block movement.

Targeting starts from direction-specific anchors around the same feet point, matching the Mana Seed sprite footprint:

```text
down: tile in front of the visible toe tips
left/right: adjacent tile in front of the side-facing feet
up: tile around the visible head/upper sprite area
```

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

Gameplay draws the Mana Seed player sprite at the same pixel scale as the seasonal tiles: `64px` character cells over `16px` terrain tiles, so the full character frame occupies four terrain tiles. The visible body is smaller because the source frame includes transparent padding. The gameplay renderer anchors the character by the point between the visible feet inside the 64x64 frame, currently around source `y=44`, rather than by the bottom of the transparent cell. Targeting uses toe/head offsets from that same point so the highlighted tile follows the visible sprite instead of the transparent frame. The starter map is intentionally larger than the first prototype grid so the full-screen camera can show smaller world sprites without letterboxing.

The renderer is intentionally separate from `WorldMap` so maps can later be rendered by different cameras, editors, debug screens, or mod tools without changing gameplay rules.
