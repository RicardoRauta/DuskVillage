# Dusk Village — Actions

## Purpose

Actions are the gameplay command layer between UI/input and simulation systems.

The module lives in `DuskVillage.Actions` and keeps action rules separate from screens, rendering, animation playback, save files, and input handling.

An action can:

```text
reference an animation by ID
target none, self, tile, or entity
advance world time
apply safe data-driven effects
return a message key for UI feedback
```

## Current Flow

```text
Screen/Input
  -> GameActionRequest
  -> GameActionSystem.Execute
  -> GameActionResult
  -> screen applies returned runtime/world state and plays returned animation
```

The screen does not know action rules. It only selects a definition ID, builds a request, applies the result, and displays feedback.

## Data Format

Built-in actions are loaded from:

```text
DuskVillage/Data/Actions/actions.json
```

Each action is a static definition:

```json
{
  "id": "action_water",
  "labelKey": "action.water",
  "descriptionKey": "action.water.description",
  "targetKind": "tile",
  "animationId": "water",
  "timeCostMinutes": 10,
  "successMessageKey": "action.result.watered",
  "tags": [ "farming", "tile" ],
  "effects": [
    { "type": "changeNeed", "needId": "energy", "amount": -4 },
    { "type": "changeNeed", "needId": "hunger", "amount": -1 }
  ]
}
```

## Effect Types

Current safe effect types:

```text
changeNeed
addMoney
sleepToNextDay
```

Actions advance the clock through `timeCostMinutes`. Need costs are explicit effects, so a short 10-minute action does not accidentally consume a full hour of hunger or energy.

## Mod Compatibility

The registry loads definitions from folders and uses stable string IDs. This prepares the system for future mod directories without requiring a full mod loader in the MVP.

The important mod-friendly constraints are:

```text
definitions are static JSON
runtime state stays in saves
systems reference definitions by ID
effects are whitelisted and validated
UI does not hardcode action behavior
```

## Lua Support

Lua is not implemented in this topic.

The action/effect shape leaves room for a later `script` effect or condition type, but Lua should be added only after the action, target, entity, map, inventory, and effect APIs are stable enough to sandbox. The first modding layer should stay data-only JSON.
