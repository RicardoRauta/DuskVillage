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
E/Y: open backpack
F/A: use selected item action
R/controller X: debug shortcut for watering
```

The gameplay hotbar uses the local Backpack UI pack when it is present:

```text
DuskVillage/Content/Packs/UI/Inventory/Pocket Inventory Series #8 BackPack v1.1.zip
```

Only JSON references to the zip file and entry paths are versioned. The paid/local PNG files remain ignored by Git.

If the zip is missing, the hotbar falls back to simple rectangles and text.

The current Backpack skin uses:

```text
BackPack idle frame as the open backpack background
Inventory button frame for the hotbar backpack icon
Inventory holder
Inventory highlighter
Inventory side tab
Inventory paper tileset for the item description panel
small item icons
```

The open backpack screen is drawn by `DuskVillage.Rendering.InventoryHotbarRenderer`, not by the gameplay screen. It shows a smaller storage grid inside the backpack, keeps the gameplay hotbar in its normal bottom position, and uses mouse hit-testing from the same layout geometry that draws the visible slots. The gameplay screen only decides when the backpack is open and passes the current owner inventory into the renderer. This keeps presentation separate from player, NPC, chest, and future multiplayer inventory state.

The pack also includes open/close pouch animations and inventory appear/disappear frames. Those animation folders were inspected and should drive the next presentation topic; the current gameplay inventory uses static frames so action readability stays stable while the layout is still evolving.

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

## NPC and Multiplayer Compatibility

`InventoryState` is intentionally owner-agnostic. It does not store "player" inside the inventory itself. A player, NPC, chest, or remote multiplayer entity can each own an `InventoryState` from its own runtime/save component.

Inventory operations clone and return a new state instead of mutating unrelated owners. Rendering receives an `InventoryState` as input, so the same hotbar/inventory renderer can later draw:

```text
local player inventory
NPC inventory
trade partner inventory
chest inventory
remote player inspection inventory
```

Texture caches are client-side rendering data and should not be synchronized over the network. Multiplayer sync should send item IDs, quantities, selected hotbar index, and authoritative inventory operation results.

## Next Work

The next inventory-related topic should focus on presentation and item use depth:

```text
play backpack open/close and category appear/disappear animations
add Adventure Book skin for village roles/jobs
add item icons once icon catalogs are mapped
add harvest items when crop growth is implemented
add tool tiers when equipment progression starts
```
