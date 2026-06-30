# Dusk Village - Inventory and Items

## Purpose

Inventory is a pure gameplay module. It owns item stacks, slot operations, hotbar selection, and saveable player inventory state. It does not own rendering, input, map rules, action definitions, or local asset loading.

The module is split into:

```text
DuskVillage.Items      static item definitions and registry
DuskVillage.Inventory  mutable player inventory state and operations
```

## Item Definitions

Built-in item definitions are loaded from:

```text
DuskVillage/Data/Items/items.json
```

Each item is static data:

```json
{
  "id": "starter_seeds",
  "labelKey": "item.starter_seeds",
  "descriptionKey": "item.starter_seeds.description",
  "category": "seed",
  "maxStack": 99,
  "tags": [ "seed", "farming" ]
}
```

Definitions are referenced by stable string IDs. Save files store item IDs and quantities, not copied definition data.

## Inventory State

`InventoryState` stores:

```text
capacity
hotbarSize
selectedHotbarIndex
slots: itemId + quantity
```

The default player inventory currently starts with:

```text
starter_seeds x15
watering_can_basic x1
```

Old saves without an inventory receive the starter inventory during normalization so gameplay remains testable while saves are still evolving.

## Hotbar

The gameplay placeholder draws a temporary 8-slot hotbar at the bottom of the screen.

Current controls:

```text
1-8: select hotbar slot
Interact/E: use selected item action
R/controller X: debug shortcut for watering
```

The hotbar UI currently uses simple rectangles and text. Local UI packs under `DuskVillage/Content/Packs/UI/Inventory` should be used in a later UI skinning topic. Those paid/local assets stay ignored by Git.

## Action Integration

Actions can gate gameplay through item effects:

```text
requireItem  needs an item, does not consume it
consumeItem  needs and removes item quantity
```

Item effects are validated before time, needs, or map state change. This prevents partial actions such as spending energy or planting a crop when the required seed is missing.

Current examples:

```text
Plant Seeds -> consume starter_seeds x1
Water       -> require watering_can_basic x1
```

## Mod Compatibility

This layer is data-driven and registry-based:

```text
items are loaded from JSON folders
actions reference item IDs by string
save data stores item IDs and quantities
systems validate effects through whitelisted effect types
UI reads labels through localization keys
```

That means future mods can add item definition JSON and action JSON without changing save state shapes. A later mod loader can add mod directories to the item and action registries.

## Next Work

The next inventory-related topic should focus on presentation and item use depth:

```text
draw the hotbar with backpack/book asset skins
add an inventory screen
add item icons once icon catalogs are mapped
add harvest items when crop growth is implemented
add tool tiers when equipment progression starts
```
