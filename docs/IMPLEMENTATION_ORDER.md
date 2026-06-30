# Implementation Order

This document tracks the current modular implementation order for Dusk Village. Each topic should be small enough to review, test, document, and commit independently.

## Current Sequence

1. Character preset, appearance, import/export, and runtime player state.
2. World clock, calendar, save compatibility, and needs simulation.
3. Character animation catalog and animation preview.
4. Data-driven actions and map interaction effects.
5. Local world assets, seasonal asset catalog, and fallback rendering.
6. Starter world map, tile state, planting/watering, and player location.
7. Gameplay map presentation: full-screen camera, continuous player movement, and local asset organization.

## Next Recommended Topics

1. Inventory foundation: item definitions, item stacks, player inventory save data, and a UI theme switch between traveler backpack and village role book.
2. Tool and hotbar layer: equip tool, use selected action from hotbar, and draw hotkey text over inventory slot art.
3. Crop growth: day transition processing, watered-day counters, harvest actions, and seed item consumption.
4. Map objects: trees, rocks, forage, collision objects, and gather/chop/mining actions.
5. First NPC module: NPC definitions, basic spawn positions, dialogue stub, and relationship save data.

## Modularity Rules

- Gameplay rules live in pure modules and do not depend on screens.
- Rendering and asset loading read stable IDs but do not mutate gameplay state.
- Save data stores mutable instances, never definition data.
- Local paid assets remain ignored by Git; JSON definitions may reference them by stable zip file name and entry path.
- Future mod support should extend registries and definitions instead of adding one-off UI logic.
