# Dusk Village — Data Schema

## 1. Purpose

This document defines the initial data schema for **Dusk Village**.

The game should be built with a data-driven architecture. Gameplay content such as items, NPCs, dialogue, schedules, furniture, crops, jobs, enemies, and events should be loaded from external JSON files whenever practical.

This document is intended to guide implementation for:

* Content definitions.
* Runtime instances.
* Save data.
* Future mod support.
* Future multiplayer compatibility.
* Stable IDs and references between systems.

This document should be read together with:

```text
/docs/GDD.md
/docs/MVP_SCOPE.md
/docs/TECHNICAL_DESIGN.md
```

## 2. Core Principles

### 2.1 Data-Driven Content

Game content should be defined in external data files when possible.

Examples:

* Items
* NPCs
* Dialogue
* Schedules
* Furniture
* Crops
* Jobs
* Enemies
* Maps
* Events
* Origins
* Skills

Gameplay systems should use content definitions by ID instead of hardcoded names.

### 2.2 Stable IDs

Every definition must have a stable string ID.

IDs should be:

* Unique within their registry.
* Lowercase.
* Snake case.
* Descriptive.
* Stable across saves.

Good examples:

```text
food_bread
resource_wood
npc_mara_woodfall
schedule_shopkeeper_basic
dialogue_mara_woodfall
furniture_wooden_bed
crop_turnip
enemy_goblin_scout
event_goblin_food_missing
```

Bad examples:

```text
Bread
Item001
NPC1
Mara
New Item
goblin scout
```

### 2.3 Definitions vs Instances vs Save Data

The game should clearly separate three types of data.

#### Definition Data

Static data loaded from JSON.

Example:

```text
"food_bread" defines what bread is.
```

#### Instance Data

Runtime data for a specific object.

Example:

```text
The player has 3 units of "food_bread" in inventory.
```

#### Save Data

Persistent mutable state written to save files.

Example:

```text
The player owns 3 bread, has 52 Energy, and is on Day 4 at 14:20.
```

Definitions should not store mutable save state.

## 3. Recommended Data Folder Structure

Initial folder structure:

```text
content/
    data/
        items/
            food.json
            resources.json
            tools.json
            furniture_items.json

        npcs/
            dusk_village_npcs.json

        dialogue/
            mara_woodfall.json
            guard_basic.json
            priestess_light.json

        schedules/
            basic_schedules.json

        furniture/
            basic_furniture.json

        crops/
            basic_crops.json

        jobs/
            basic_jobs.json

        enemies/
            forest_enemies.json

        events/
            goblin_event_chain.json

        origins/
            starting_origins.json

        skills/
            skills.json

        maps/
            dusk_village.json
            player_home.json
            forest_edge.json
```

The exact file split may change, but registries should load definitions from folders, not from a single hardcoded file.

## 4. Common Field Types

### 4.1 ID

```json
"id": "food_bread"
```

All IDs are strings.

### 4.2 Display Name

```json
"displayName": "Bread"
```

The human-readable name shown to the player.

### 4.3 Description

```json
"description": "A simple loaf of bread."
```

Short description for UI.

### 4.4 Tags

```json
"tags": ["Food", "Cooked", "Common"]
```

Tags are optional strings used for filtering, conditions, and future mod support.

### 4.5 Position

Tile position:

```json
"position": [12, 18]
```

World position may also use two numeric values:

```json
"worldPosition": [192, 288]
```

The game should centralize tile size in code.

### 4.6 Time

Use 24-hour time strings for data definitions:

```json
"startTime": "08:00"
```

Internally, the game may convert this into minutes after midnight.

## 5. Item Definition

Items are static definitions loaded into `ItemRegistry`.

### 5.1 Schema

```json
{
  "id": "food_bread",
  "displayName": "Bread",
  "description": "A simple loaf of bread.",
  "category": "Food",
  "maxStack": 10,
  "baseBuyPrice": 12,
  "baseSellPrice": 8,
  "spriteId": "sprite_item_bread",
  "tags": ["Food", "Cooked"],
  "useEffects": []
}
```

### 5.2 Fields

| Field         |     Type | Required | Notes                                      |
| ------------- | -------: | -------: | ------------------------------------------ |
| id            |   string |      Yes | Stable item ID                             |
| displayName   |   string |      Yes | Name shown in UI                           |
| description   |   string |      Yes | UI description                             |
| category      |   string |      Yes | Food, Resource, Tool, Furniture, QuestItem |
| maxStack      |      int |      Yes | Maximum stack size                         |
| baseBuyPrice  |      int |       No | Shop price                                 |
| baseSellPrice |      int |      Yes | Sell price                                 |
| spriteId      |   string |       No | Sprite reference                           |
| tags          | string[] |       No | Optional tags                              |
| useEffects    |    array |       No | Effects when item is used                  |

### 5.3 MVP Item Categories

```text
Food
Resource
Tool
Furniture
QuestItem
```

### 5.4 Food Item Example

```json
{
  "id": "food_bread",
  "displayName": "Bread",
  "description": "A simple loaf of bread.",
  "category": "Food",
  "maxStack": 10,
  "baseBuyPrice": 12,
  "baseSellPrice": 8,
  "spriteId": "sprite_item_bread",
  "tags": ["Food", "Cooked"],
  "useEffects": [
    {
      "type": "RestoreNeed",
      "need": "Hunger",
      "amount": 25
    },
    {
      "type": "RestoreNeed",
      "need": "Mood",
      "amount": 3
    }
  ]
}
```

### 5.5 Resource Item Example

```json
{
  "id": "resource_wood",
  "displayName": "Wood",
  "description": "Usable timber gathered from the forest.",
  "category": "Resource",
  "maxStack": 99,
  "baseSellPrice": 2,
  "spriteId": "sprite_item_wood",
  "tags": ["Resource", "BuildingMaterial"]
}
```

### 5.6 Tool Item Example

```json
{
  "id": "tool_axe_basic",
  "displayName": "Basic Axe",
  "description": "A worn but usable axe.",
  "category": "Tool",
  "maxStack": 1,
  "baseBuyPrice": 120,
  "baseSellPrice": 40,
  "spriteId": "sprite_item_basic_axe",
  "tags": ["Tool", "Axe", "Woodcutting"],
  "toolData": {
    "toolType": "Axe",
    "power": 1,
    "energyCostModifier": 1.0
  }
}
```

## 6. Use Effects

Use effects are reusable data-driven effects triggered by item use, dialogue, events, or jobs.

### 6.1 Restore Need

```json
{
  "type": "RestoreNeed",
  "need": "Hunger",
  "amount": 25
}
```

### 6.2 Change Relationship

```json
{
  "type": "ChangeRelationship",
  "target": "npc_mara_woodfall",
  "amount": 5
}
```

### 6.3 Give Item

```json
{
  "type": "GiveItem",
  "itemId": "resource_wood",
  "quantity": 5
}
```

### 6.4 Add Money

```json
{
  "type": "AddMoney",
  "amount": 20
}
```

### 6.5 Set Event Flag

```json
{
  "type": "SetEventFlag",
  "flag": "goblin_tracks_found",
  "value": true
}
```

The MVP should implement only the effects needed by current systems.

## 7. NPC Definition

NPCs are static definitions loaded into `NpcRegistry`.

NPC definitions should not store mutable relationship values or current position during gameplay. Those belong in save data.

### 7.1 Schema

```json
{
  "id": "npc_mara_woodfall",
  "displayName": "Mara Woodfall",
  "ageGroup": "Adult",
  "role": "Shopkeeper",
  "homeMapId": "map_dusk_village",
  "homePosition": [12, 18],
  "workMapId": "map_general_store",
  "workPosition": [5, 6],
  "scheduleId": "schedule_shopkeeper_basic",
  "dialogueSetId": "dialogue_mara_woodfall",
  "spriteId": "sprite_npc_mara",
  "attributes": {
    "strength": 3,
    "agility": 4,
    "constitution": 4,
    "intelligence": 5,
    "charisma": 6,
    "wisdom": 5
  },
  "tags": ["Villager", "Shopkeeper", "RomanceCandidate"]
}
```

### 7.2 Fields

| Field         |     Type | Required | Notes                                |
| ------------- | -------: | -------: | ------------------------------------ |
| id            |   string |      Yes | Stable NPC ID                        |
| displayName   |   string |      Yes | NPC name                             |
| ageGroup      |   string |      Yes | YoungAdult, Adult, OlderAdult, Elder |
| role          |   string |      Yes | Social/professional role             |
| homeMapId     |   string |      Yes | Home map                             |
| homePosition  |   int[2] |      Yes | Home tile position                   |
| workMapId     |   string |       No | Work map                             |
| workPosition  |   int[2] |       No | Work tile position                   |
| scheduleId    |   string |      Yes | Schedule definition                  |
| dialogueSetId |   string |      Yes | Dialogue definition                  |
| spriteId      |   string |       No | Sprite reference                     |
| attributes    |   object |       No | Optional attribute block             |
| tags          | string[] |       No | Optional tags                        |

### 7.3 Age Groups

Initial age groups:

```text
YoungAdult
Adult
OlderAdult
Elder
```

Future age groups:

```text
Baby
Child
Teenager
AncientElder
```

## 8. Attribute Block

Characters may use the following attribute block:

```json
{
  "strength": 4,
  "agility": 5,
  "constitution": 6,
  "intelligence": 3,
  "charisma": 4,
  "wisdom": 5
}
```

Attributes should use a simple numeric range during the MVP.

Recommended MVP range:

```text
1 to 10
```

## 9. Needs Block

Characters with needs may use:

```json
{
  "energy": 100,
  "maxEnergy": 100,
  "hunger": 100,
  "maxHunger": 100,
  "health": 100,
  "maxHealth": 100,
  "mood": 75,
  "maxMood": 100
}
```

The MVP uses these needs:

```text
Energy
Hunger
Health
Mood
```

## 10. Schedule Definition

Schedules are static definitions loaded into `ScheduleRegistry`.

### 10.1 Schema

```json
{
  "id": "schedule_shopkeeper_basic",
  "blocks": [
    {
      "startTime": "06:00",
      "endTime": "08:00",
      "mapId": "map_dusk_village",
      "position": [12, 18],
      "activity": "Home"
    },
    {
      "startTime": "08:00",
      "endTime": "18:00",
      "mapId": "map_general_store",
      "position": [5, 6],
      "activity": "Work"
    },
    {
      "startTime": "18:00",
      "endTime": "22:00",
      "mapId": "map_tavern",
      "position": [8, 4],
      "activity": "Social"
    },
    {
      "startTime": "22:00",
      "endTime": "06:00",
      "mapId": "map_dusk_village",
      "position": [12, 18],
      "activity": "Sleep"
    }
  ]
}
```

### 10.2 Schedule Block Fields

| Field     |   Type | Required | Notes                             |
| --------- | -----: | -------: | --------------------------------- |
| startTime | string |      Yes | 24-hour time                      |
| endTime   | string |      Yes | 24-hour time                      |
| mapId     | string |      Yes | Target map                        |
| position  | int[2] |      Yes | Target tile position              |
| activity  | string |      Yes | Home, Work, Social, Sleep, Wander |

Schedules may wrap past midnight.

Example:

```json
{
  "startTime": "22:00",
  "endTime": "06:00",
  "activity": "Sleep"
}
```

## 11. Dialogue Definition

Dialogue is loaded into `DialogueRegistry`.

### 11.1 Schema

```json
{
  "id": "dialogue_mara_woodfall",
  "lines": [
    {
      "id": "mara_intro_001",
      "conditions": {
        "minRelationship": 0,
        "requiredFlags": [],
        "forbiddenFlags": []
      },
      "text": "New face in Dusk Village? Then learn quickly: the forest gives, but it always takes back.",
      "effects": [
        {
          "type": "SetEventFlag",
          "flag": "met_mara_woodfall",
          "value": true
        }
      ]
    }
  ]
}
```

### 11.2 Dialogue Line Fields

| Field      |   Type | Required | Notes                        |
| ---------- | -----: | -------: | ---------------------------- |
| id         | string |      Yes | Stable dialogue line ID      |
| conditions | object |       No | Conditions for availability  |
| text       | string |      Yes | Dialogue text                |
| effects    |  array |       No | Effects triggered after line |

### 11.3 Dialogue Conditions

Initial supported conditions:

```json
{
  "minRelationship": 20,
  "maxRelationship": 80,
  "requiredFlags": ["goblin_food_missing_reported"],
  "forbiddenFlags": ["goblin_tracks_found"],
  "timeOfDay": ["Morning", "Afternoon"],
  "requiredItemIds": ["item_letter_from_guard"]
}
```

The MVP does not need to implement every condition immediately, but the structure should allow expansion.

### 11.4 Dialogue Example with Relationship

```json
{
  "id": "mara_friend_001",
  "conditions": {
    "minRelationship": 40
  },
  "text": "You have lasted longer than most newcomers. That means something here.",
  "effects": []
}
```

## 12. Furniture Definition

Furniture definitions are loaded into `FurnitureRegistry`.

### 12.1 Schema

```json
{
  "id": "furniture_wooden_bed",
  "displayName": "Wooden Bed",
  "description": "A simple bed made from local timber.",
  "size": [2, 1],
  "comfortValue": 5,
  "canRotate": true,
  "spriteId": "sprite_furniture_wooden_bed",
  "tags": ["Bed", "Sleep"]
}
```

### 12.2 Fields

| Field        |     Type | Required | Notes                           |
| ------------ | -------: | -------: | ------------------------------- |
| id           |   string |      Yes | Stable furniture ID             |
| displayName  |   string |      Yes | UI name                         |
| description  |   string |      Yes | UI description                  |
| size         |   int[2] |      Yes | Width/height in tiles           |
| comfortValue |      int |      Yes | Home comfort contribution       |
| canRotate    |     bool |      Yes | Placement support               |
| spriteId     |   string |       No | Sprite reference                |
| tags         | string[] |       No | Bed, Storage, Light, Decoration |

## 13. Crop Definition

Crop definitions are loaded into `CropRegistry`.

### 13.1 Schema

```json
{
  "id": "crop_turnip",
  "displayName": "Turnip",
  "seedItemId": "seed_turnip",
  "harvestItemId": "food_turnip",
  "daysToGrow": 4,
  "requiredWateredDays": 4,
  "regrows": false,
  "spriteId": "sprite_crop_turnip",
  "tags": ["Crop", "Food"]
}
```

### 13.2 Fields

| Field               |     Type | Required | Notes                              |
| ------------------- | -------: | -------: | ---------------------------------- |
| id                  |   string |      Yes | Stable crop ID                     |
| displayName         |   string |      Yes | Crop name                          |
| seedItemId          |   string |      Yes | Item needed to plant               |
| harvestItemId       |   string |      Yes | Item produced                      |
| daysToGrow          |      int |      Yes | Minimum days to mature             |
| requiredWateredDays |      int |      Yes | Required watered days              |
| regrows             |     bool |      Yes | Whether crop regrows after harvest |
| regrowDays          |      int |       No | Required if regrows is true        |
| spriteId            |   string |       No | Sprite reference                   |
| tags                | string[] |       No | Optional tags                      |

### 13.3 MVP Crops

```text
crop_turnip
crop_cabbage
crop_nightroot_herb
```

## 14. Skill Definition

Skills are loaded into `SkillRegistry`.

### 14.1 Schema

```json
{
  "id": "skill_woodcutting",
  "displayName": "Woodcutting",
  "description": "Improves efficiency when cutting trees and gathering timber.",
  "maxLevel": 10,
  "tags": ["Work", "Gathering"]
}
```

### 14.2 MVP Skills

```text
skill_farming
skill_woodcutting
skill_crafting
skill_social
skill_survival
skill_combat
```

## 15. Job and Work Action Definition

Jobs and work actions are loaded into `JobRegistry`.

A job represents an activity that consumes time and/or energy and provides rewards.

### 15.1 Schema

```json
{
  "id": "work_chop_wood",
  "displayName": "Chop Wood",
  "description": "Cut nearby trees into usable timber.",
  "requiredToolId": "tool_axe_basic",
  "energyCost": 8,
  "timeCostMinutes": 20,
  "skillId": "skill_woodcutting",
  "skillXpReward": 5,
  "moneyReward": 0,
  "itemRewards": [
    {
      "itemId": "resource_wood",
      "quantity": 3
    }
  ],
  "tags": ["Work", "Gathering", "Forest"]
}
```

### 15.2 Paid Job Example

```json
{
  "id": "job_repair_fence",
  "displayName": "Repair Fence",
  "description": "Help repair damaged village fencing.",
  "requiredToolId": null,
  "energyCost": 12,
  "timeCostMinutes": 60,
  "skillId": "skill_crafting",
  "skillXpReward": 8,
  "moneyReward": 25,
  "itemRewards": [],
  "relationshipRewards": [
    {
      "npcId": "npc_guard_captain",
      "amount": 3
    }
  ],
  "tags": ["Job", "Village", "Crafting"]
}
```

## 16. Enemy Definition

Enemies are loaded into `EnemyRegistry`.

### 16.1 Schema

```json
{
  "id": "enemy_wolf",
  "displayName": "Wolf",
  "description": "A hungry wolf from the dark forest.",
  "maxHealth": 30,
  "attackPower": 6,
  "defense": 1,
  "moveSpeed": 60,
  "aggroRange": 5,
  "attackRange": 1,
  "attackCooldownSeconds": 1.5,
  "spriteId": "sprite_enemy_wolf",
  "lootTable": [
    {
      "itemId": "resource_hide",
      "minQuantity": 1,
      "maxQuantity": 2,
      "dropChance": 0.7
    }
  ],
  "tags": ["Beast", "Forest"]
}
```

### 16.2 Goblin Scout Example

```json
{
  "id": "enemy_goblin_scout",
  "displayName": "Goblin Scout",
  "description": "A small goblin watching the village edge.",
  "maxHealth": 24,
  "attackPower": 5,
  "defense": 0,
  "moveSpeed": 55,
  "aggroRange": 6,
  "attackRange": 1,
  "attackCooldownSeconds": 1.2,
  "spriteId": "sprite_enemy_goblin_scout",
  "lootTable": [
    {
      "itemId": "resource_scrap",
      "minQuantity": 1,
      "maxQuantity": 2,
      "dropChance": 0.5
    }
  ],
  "tags": ["Goblin", "Forest", "Humanoid"]
}
```

## 17. Origin Definition

Origins are loaded into `OriginRegistry`.

### 17.1 Schema

```json
{
  "id": "origin_newcomer",
  "displayName": "Newcomer",
  "description": "You recently arrived in Dusk Village with little more than your name and a chance to begin again.",
  "startingMoney": 50,
  "startingItems": [
    {
      "itemId": "food_bread",
      "quantity": 2
    },
    {
      "itemId": "tool_axe_basic",
      "quantity": 1
    }
  ],
  "attributeModifiers": {
    "strength": 0,
    "agility": 0,
    "constitution": 0,
    "intelligence": 0,
    "charisma": 0,
    "wisdom": 0
  },
  "startingRelationshipModifiers": [],
  "tags": ["Default", "Flexible"]
}
```

### 17.2 MVP Origins

```text
origin_newcomer
origin_local_villager
origin_former_laborer
origin_poor_wanderer
```

## 18. Map Definition

Map definitions are loaded into `MapRegistry`.

The exact tilemap format may depend on the chosen tool. However, map metadata should follow stable IDs.

### 18.1 Schema

```json
{
  "id": "map_dusk_village",
  "displayName": "Dusk Village",
  "tilesetId": "tileset_dusk_village",
  "width": 80,
  "height": 60,
  "tileSize": 16,
  "layers": [
    {
      "id": "ground",
      "type": "TileLayer",
      "dataFile": "maps/dusk_village_ground.csv"
    },
    {
      "id": "collision",
      "type": "CollisionLayer",
      "dataFile": "maps/dusk_village_collision.csv"
    },
    {
      "id": "objects",
      "type": "ObjectLayer",
      "dataFile": "maps/dusk_village_objects.json"
    }
  ],
  "spawnPoints": [
    {
      "id": "spawn_default",
      "position": [20, 30]
    }
  ],
  "transitions": [
    {
      "id": "to_forest_edge",
      "fromPosition": [78, 32],
      "targetMapId": "map_forest_edge",
      "targetSpawnPointId": "spawn_from_village"
    }
  ]
}
```

### 18.2 Required MVP Maps

```text
map_player_home
map_dusk_village
map_general_store
map_tavern
map_church_of_light
map_guard_post
map_forest_edge
```

## 19. Event Definition

Events are loaded into `EventRegistry`.

The MVP event system should be simple and flag-based.

### 19.1 Schema

```json
{
  "id": "event_goblin_food_missing",
  "displayName": "Missing Food",
  "description": "Villagers have reported missing food near the forest edge.",
  "trigger": {
    "type": "Dialogue",
    "npcId": "npc_guard_captain"
  },
  "conditions": {
    "requiredFlags": [],
    "forbiddenFlags": ["goblin_food_missing_reported"]
  },
  "effects": [
    {
      "type": "SetEventFlag",
      "flag": "goblin_food_missing_reported",
      "value": true
    }
  ]
}
```

### 19.2 Goblin MVP Event Flags

```text
goblin_food_missing_reported
goblin_tracks_found
goblin_scout_encountered
goblin_threat_reported_to_guard
```

## 20. Shop Definition

Shops may be loaded into `ShopRegistry`.

### 20.1 Schema

```json
{
  "id": "shop_general_store",
  "displayName": "General Store",
  "ownerNpcId": "npc_mara_woodfall",
  "inventory": [
    {
      "itemId": "food_bread",
      "quantity": 10,
      "buyPriceOverride": null,
      "sellPriceMultiplier": 1.0
    },
    {
      "itemId": "seed_turnip",
      "quantity": 20,
      "buyPriceOverride": null,
      "sellPriceMultiplier": 1.0
    }
  ],
  "tags": ["Village", "Store"]
}
```

The MVP can use static shop inventories.

Future versions may support restocking, shortages, taxes, supply and demand, and NPC-owned businesses.

## 21. Save Game Schema

Save data stores mutable world state.

### 21.1 Save Root

```json
{
  "metadata": {},
  "worldState": {},
  "playerState": {},
  "entityStates": [],
  "relationshipStates": [],
  "mapStates": [],
  "eventFlags": {}
}
```

## 22. Save Metadata

```json
{
  "saveVersion": 1,
  "gameVersion": "0.1.0",
  "createdAt": "2026-06-26T12:00:00Z",
  "lastPlayedAt": "2026-06-26T12:30:00Z",
  "playerName": "Alden",
  "currentDay": 4,
  "currentTime": "14:20"
}
```

## 23. World State Save Data

```json
{
  "day": 4,
  "timeMinutes": 860,
  "currentSeason": "None",
  "worldSeed": 123456,
  "startingLordProfile": "JustAndTired"
}
```

The MVP does not need full seasons, but the field may be reserved for future use.

## 24. Player State Save Data

```json
{
  "entityId": "player_main",
  "name": "Alden",
  "originId": "origin_newcomer",
  "ageGroup": "YoungAdult",
  "currentMapId": "map_dusk_village",
  "position": [20, 30],
  "attributes": {
    "strength": 5,
    "agility": 4,
    "constitution": 6,
    "intelligence": 4,
    "charisma": 3,
    "wisdom": 4
  },
  "needs": {
    "energy": 72,
    "maxEnergy": 100,
    "hunger": 65,
    "maxHunger": 100,
    "health": 91,
    "maxHealth": 100,
    "mood": 58,
    "maxMood": 100
  },
  "money": 84,
  "inventory": {
    "slots": [
      {
        "itemId": "food_bread",
        "quantity": 2
      },
      {
        "itemId": "resource_wood",
        "quantity": 12
      }
    ]
  },
  "skills": [
    {
      "skillId": "skill_woodcutting",
      "level": 2,
      "experience": 35
    }
  ]
}
```

## 25. Entity State Save Data

Entity save data is used for persistent world entities.

Examples:

* NPCs
* Furniture
* Crops
* Chests
* Dropped items
* Enemies if persistent

### 25.1 NPC State Example

```json
{
  "entityId": "npc_mara_woodfall",
  "definitionId": "npc_mara_woodfall",
  "entityType": "NPC",
  "currentMapId": "map_general_store",
  "position": [5, 6],
  "currentScheduleBlock": "Work",
  "stateFlags": {
    "metPlayer": true
  }
}
```

### 25.2 Furniture State Example

```json
{
  "entityId": "furniture_instance_001",
  "definitionId": "furniture_wooden_bed",
  "entityType": "Furniture",
  "currentMapId": "map_player_home",
  "position": [4, 5],
  "rotation": 0
}
```

### 25.3 Crop State Example

```json
{
  "entityId": "crop_instance_001",
  "definitionId": "crop_turnip",
  "entityType": "Crop",
  "currentMapId": "map_player_home",
  "position": [8, 10],
  "growthStage": 2,
  "daysWatered": 2,
  "isWateredToday": false
}
```

## 26. Relationship State Save Data

Relationships are mutable and should be saved separately from NPC definitions.

```json
{
  "sourceEntityId": "player_main",
  "targetEntityId": "npc_mara_woodfall",
  "value": 25,
  "tags": ["Acquaintance"]
}
```

For the MVP, only player-to-NPC relationships are required.

Future versions may support NPC-to-NPC relationships.

## 27. Map State Save Data

Map states store mutable tile and object data.

Example:

```json
{
  "mapId": "map_player_home",
  "modifiedTiles": [
    {
      "position": [8, 10],
      "layerId": "soil",
      "tileId": "tilled_soil"
    }
  ],
  "removedObjects": [],
  "placedEntityIds": [
    "furniture_instance_001",
    "crop_instance_001"
  ]
}
```

The MVP should save at least:

* Tilled soil.
* Crop state.
* Placed furniture.
* Removed/gathered objects if needed.

## 28. Event Flags Save Data

Event flags are simple key-value values.

```json
{
  "goblin_food_missing_reported": true,
  "goblin_tracks_found": true,
  "goblin_scout_encountered": false,
  "goblin_threat_reported_to_guard": false
}
```

Event flags should use stable string keys.

## 29. Validation Rules

The data loader should validate content before gameplay starts.

### 29.1 General Validation

Check for:

* Duplicate IDs.
* Missing required fields.
* Invalid field types.
* Empty display names.
* Invalid references.
* Invalid enum values.

### 29.2 Item Validation

Check that:

* `id` is unique.
* `maxStack` is greater than 0.
* `baseSellPrice` is not negative.
* Use effects are valid.
* Tool data exists for tool items when required.

### 29.3 NPC Validation

Check that:

* `scheduleId` exists.
* `dialogueSetId` exists.
* `homeMapId` exists.
* `workMapId` exists if provided.
* Attribute values are valid.

### 29.4 Schedule Validation

Check that:

* Time strings are valid.
* Map IDs exist.
* Positions are valid or within expected bounds.
* Each schedule has at least one block.

### 29.5 Furniture Validation

Check that:

* Size is positive.
* Comfort value is not negative.
* Sprite ID exists if sprite validation is implemented.

### 29.6 Crop Validation

Check that:

* Seed item exists.
* Harvest item exists.
* Days to grow is greater than 0.
* Required watered days is not negative.

### 29.7 Dialogue Validation

Check that:

* Dialogue line IDs are unique inside the dialogue set.
* Conditions reference valid flags or known systems when possible.
* Effects are valid.

## 30. Versioning

Definitions may include a schema version later.

Example:

```json
{
  "schemaVersion": 1,
  "id": "food_bread"
}
```

For MVP content, this is optional.

Save files must include a version from the start:

```json
{
  "saveVersion": 1
}
```

Future save migrations should use this field.

## 31. Missing Content Handling

The game should handle missing content carefully.

During development:

* Missing required content should produce a clear error.
* Invalid data should fail fast.
* Validation errors should mention file path and ID.

In future modded saves:

* Missing optional mod content should degrade gracefully where possible.
* Missing items may become placeholder items.
* Missing NPCs may be marked unavailable.
* Missing furniture may become placeholder furniture.

The MVP does not need full modded save recovery, but should avoid fragile assumptions.

## 32. Minimal MVP Data Set

The MVP should include at least:

### Items

```text
food_bread
food_apple
food_turnip
seed_turnip
seed_cabbage
seed_nightroot_herb
resource_wood
resource_stone
resource_herb
resource_hide
resource_scrap
tool_axe_basic
tool_hoe_basic
tool_watering_can_basic
tool_sword_basic
furniture_wooden_bed_item
furniture_chair_item
furniture_table_item
```

### Furniture

```text
furniture_wooden_bed
furniture_wooden_chair
furniture_wooden_table
furniture_storage_chest
furniture_candle
```

### NPCs

```text
8 to 12 fixed MVP NPCs
```

### Skills

```text
skill_farming
skill_woodcutting
skill_crafting
skill_social
skill_survival
skill_combat
```

### Crops

```text
crop_turnip
crop_cabbage
crop_nightroot_herb
```

### Enemies

```text
enemy_wolf
enemy_goblin_scout
```

### Events

```text
event_goblin_food_missing
event_goblin_tracks_found
event_goblin_scout_encountered
event_goblin_report_to_guard
```

## 33. Implementation Notes for Codex

When implementing this schema:

1. Create definition classes separately from save-state classes.
2. Do not put mutable state inside definition classes.
3. Load JSON files into registries.
4. Reference definitions by stable string IDs.
5. Validate all loaded data before starting gameplay.
6. Keep save files separate from content files.
7. Use clear errors for missing references.
8. Avoid hardcoding item, NPC, or dialogue behavior into UI classes.
9. Implement only the schema needed for the MVP first.
10. Leave room for future fields without requiring all future systems now.

## 34. Summary

The data schema for Dusk Village should support a modular, persistent, data-driven life simulation game.

The most important rules are:

* Definitions are static.
* Instances are runtime objects.
* Save data stores mutable state.
* IDs must be stable.
* Content should be loaded from JSON.
* Registries should own lookups.
* Systems should reference definitions by ID.
* The structure should prepare for future mods and multiplayer without requiring them in the MVP.
