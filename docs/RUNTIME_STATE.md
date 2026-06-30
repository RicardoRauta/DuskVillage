# Dusk Village - Runtime State

## Purpose

Runtime state stores mutable gameplay data for a running save. It is separate from character presets.

Character presets describe identity, appearance, starting attributes, starting needs, and initial skills. Runtime state stores what can change after the game begins, such as current needs, money, and location.

## Player Runtime State

The player runtime module lives in `DuskVillage.Players`.

Current MVP fields:

```text
entityId
characterPreset
needs
money
location
```

`characterPreset` remains embedded so the save keeps the character identity and appearance used by the player. `needs` is stored separately from `characterPreset.needs` because live gameplay should modify runtime needs without changing the exported preset.

Runtime needs are updated by the pure `DuskVillage.Needs` module. The save only stores the current values.

`location` is intentionally simple for now:

```text
areaId
positionX
positionY
tileX
tileY
```

`positionX` and `positionY` are continuous tile-space coordinates used for gameplay movement and save/load. `tileX` and `tileY` are derived from the continuous position and kept for old saves, targeting, and simple tile interactions.

The map module can later replace tile-space coordinates with real world coordinates without changing the preset format.

## Save Compatibility

Old saves that only contain `entityId` and `characterPreset` normalize into a complete runtime state when loaded. Missing needs are copied from the character preset, money defaults to `0`, and location defaults to `starter_farm (7.00, 6.00)`.
