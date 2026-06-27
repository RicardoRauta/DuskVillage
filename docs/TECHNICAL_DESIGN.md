# Dusk Village — Technical Design Document

## 1. Purpose

This document defines the initial technical direction for **Dusk Village**, a 2D pixel art dark fantasy life simulation sandbox RPG built in **C# with MonoGame**.

The goal is to guide implementation in a way that supports:

* A playable MVP.
* Data-driven content.
* Persistent world simulation.
* Future mod support.
* Future multiplayer support.
* Long-term generational gameplay.

This document should be read together with:

```text
/docs/GDD.md
/docs/MVP_SCOPE.md
```

The MVP should remain focused on the core life simulation loop. This technical design should prepare the architecture for future systems without requiring those systems to be fully implemented immediately.

## 2. Technical Goals

The codebase should prioritize:

1. Clear separation between simulation, rendering, input, and UI.
2. Persistent world state with stable entity identifiers.
3. Data-driven definitions for content such as items, NPCs, dialogue, furniture, jobs, and schedules.
4. Modular gameplay systems that can be expanded without rewriting core logic.
5. Save/load support from early development.
6. Future modding support through external content files.
7. Future multiplayer support by keeping simulation state serializable and not tied directly to local input.

## 3. Technology Stack

Initial technology stack:

* Language: C#
* Framework: MonoGame
* Target platform: PC
* Graphics: 2D pixel art
* Content format: External JSON files for gameplay data
* Save format: JSON during early development
* Audio: MonoGame-compatible audio assets
* Maps: Internal tilemap system or external tilemap import, depending on tooling decision

The project should avoid unnecessary engine complexity during the MVP. The architecture should be clean, but not over-engineered.

## 4. High-Level Architecture

The game should be divided into these major layers:

```text
Game Application
    ├── Platform / MonoGame Layer
    ├── Screen and State Management
    ├── Input Layer
    ├── Rendering Layer
    ├── UI Layer
    ├── Simulation Layer
    ├── Data Loading Layer
    ├── Save/Load Layer
    └── Content / Asset Layer
```

### 4.1 MonoGame Layer

Responsible for:

* Game initialization
* Game loop
* Graphics device setup
* Content loading
* Window configuration
* Input polling
* Delegating update and draw calls

The MonoGame layer should not contain game-specific simulation logic.

### 4.2 Simulation Layer

Responsible for:

* Time
* Entities
* NPCs
* Needs
* Inventory
* Relationships
* Work
* Farming
* Combat
* Events
* World state

Simulation systems should not depend directly on rendering classes.

### 4.3 Rendering Layer

Responsible for:

* Sprite drawing
* Tilemap rendering
* Animation drawing
* Camera
* Draw ordering
* Lighting or visual effects later

Rendering should read state from the simulation, not own the simulation.

### 4.4 UI Layer

Responsible for:

* HUD
* Dialogue windows
* Inventory screens
* Shop screens
* Character creation
* Pause menu
* Save/load menu

UI should call commands or services instead of directly mutating world state whenever possible.

## 5. Recommended Project Structure

Initial folder structure:

```text
DuskVillage/
    src/
        DuskVillage.Game/
            Core/
            Screens/
            Input/
            Rendering/
            UI/
            Simulation/
                Entities/
                Components/
                Systems/
                World/
                Time/
                Characters/
                Needs/
                Inventory/
                Economy/
                Dialogue/
                Relationships/
                Jobs/
                Farming/
                Combat/
                Events/
            Data/
                Definitions/
                Loading/
                Registries/
                Validation/
            Saving/
            Audio/
            Utilities/

    content/
        assets/
            textures/
            sprites/
            tilesets/
            ui/
            fonts/
            audio/
        data/
            items/
            npcs/
            dialogue/
            schedules/
            maps/
            furniture/
            jobs/
            crops/
            enemies/
            events/

    saves/

    docs/
        GDD.md
        MVP_SCOPE.md
        TECHNICAL_DESIGN.md
```

This structure can evolve, but the separation between source code, assets, data, saves, and documentation should remain.

## 6. Core Runtime Flow

The main game loop should follow the MonoGame pattern:

```text
Initialize
LoadContent

Loop:
    Handle Input
    Update Active Screen
    Update Simulation if gameplay is active
    Draw Active Screen
```

During gameplay:

```text
Input
    ↓
Player Commands
    ↓
Simulation Systems
    ↓
World State Changes
    ↓
Rendering and UI read updated state
```

The simulation should be updated through explicit systems and services, not through scattered logic inside UI or rendering classes.

## 7. Game States and Screens

The game should use a screen/state system.

Recommended MVP screens:

```text
BootScreen
MainMenuScreen
NewGameScreen
CharacterCreationScreen
LoadGameScreen
GameplayScreen
PauseScreen
InventoryScreen
DialogueScreen
ShopScreen
SettingsScreen
```

### 7.1 GameplayScreen

The main gameplay screen owns or references:

* Current world
* Current map
* Player entity
* Camera
* Simulation systems
* HUD
* Interaction controller

GameplayScreen should coordinate gameplay but should not become a large class that implements all game logic.

## 8. Entity Architecture

The game should use a lightweight entity-component-system-inspired architecture.

A full complex ECS is not required for the MVP, but the architecture should avoid deep inheritance hierarchies.

### 8.1 Entity

An entity represents anything persistent or interactive in the world.

Examples:

* Player character
* NPC
* Animal
* Enemy
* Dropped item
* Furniture
* Crop
* Door
* Chest
* Workstation
* Interactive object

Each entity should have:

```text
EntityId
EntityType
Components
Tags
```

### 8.2 Entity ID

Every persistent entity should have a stable ID.

Recommended format:

```text
string EntityId
```

Examples:

```text
player_main
npc_elena_ashford
item_instance_7f3a9c
furniture_instance_2d91aa
crop_instance_882bd1
```

For generated runtime entities, a GUID or deterministic ID generator can be used.

Stable IDs are important for:

* Save/load
* Relationships
* Dialogue flags
* Quest/event flags
* Future modding
* Future multiplayer sync

## 9. Components

Components should hold data. They should not contain large gameplay logic.

Recommended MVP components:

### TransformComponent

Stores world position.

```text
Position
Direction
MapId
```

### SpriteComponent

Stores sprite reference and animation state.

```text
SpriteId
AnimationId
FacingDirection
```

### ColliderComponent

Stores collision information.

```text
Bounds
BlocksMovement
```

### CharacterComponent

Marks an entity as a character.

```text
DisplayName
AgeGroup
OriginId
IsPlayerControlled
```

### AttributeComponent

Stores RPG attributes.

```text
Strength
Agility
Constitution
Intelligence
Charisma
Wisdom
```

### NeedsComponent

Stores character needs.

```text
Energy
MaxEnergy
Hunger
MaxHunger
Health
MaxHealth
Mood
MaxMood
```

### SkillComponent

Stores skill levels and XP.

```text
SkillId
Level
Experience
```

### InventoryComponent

Stores items carried by an entity.

```text
InventorySlots
MaxSlots
```

### WalletComponent

Stores money.

```text
CurrencyAmount
```

### RelationshipComponent

Stores relationships with other characters.

```text
TargetEntityId
RelationshipValue
RelationshipTags
```

### ScheduleComponent

Stores NPC schedule reference and current schedule state.

```text
ScheduleId
CurrentScheduleBlock
```

### InteractableComponent

Allows an entity or object to be interacted with.

```text
InteractionType
InteractionPrompt
TargetActionId
```

### FurnitureComponent

Marks an entity as furniture.

```text
FurnitureDefinitionId
ComfortValue
CanBeMoved
```

### CropComponent

Stores crop state.

```text
CropDefinitionId
GrowthStage
DaysWatered
IsWateredToday
```

### CombatComponent

Stores basic combat state.

```text
AttackPower
Defense
AttackCooldown
FactionId
```

## 10. Systems

Systems contain gameplay logic and operate on world state.

Recommended MVP systems:

```text
TimeSystem
InputCommandSystem
MovementSystem
CollisionSystem
InteractionSystem
NeedsSystem
InventorySystem
EconomySystem
DialogueSystem
RelationshipSystem
ScheduleSystem
WorkSystem
SkillSystem
FarmingSystem
FurnitureSystem
CombatSystem
EnemyAISystem
EventSystem
SaveSystem
```

### 10.1 TimeSystem

Responsible for:

* Current day
* Current time
* Time scale
* Day transition
* Sleep transition
* Calendar progression later

MVP requirements:

* Advance time during gameplay.
* Allow sleeping to end the day.
* Trigger daily reset events.
* Notify systems when a new day starts.

### 10.2 NeedsSystem

Responsible for:

* Reducing hunger over time.
* Reducing energy through actions.
* Applying penalties for low hunger, low energy, low health, or low mood.
* Restoring energy through sleep.
* Restoring hunger through food.

The MVP should keep formulas simple.

### 10.3 ScheduleSystem

Responsible for:

* Moving NPCs between scheduled locations.
* Selecting current NPC behavior based on time.
* Updating NPC location when the player enters a map.

The MVP does not require complex autonomous AI.

### 10.4 DialogueSystem

Responsible for:

* Loading dialogue lines from data.
* Selecting valid dialogue based on NPC, relationship, time, and flags.
* Showing dialogue through UI.
* Triggering simple dialogue effects.

### 10.5 RelationshipSystem

Responsible for:

* Tracking relationship values.
* Applying relationship changes from talking, gifting, events, and choices.
* Returning relationship level labels.

### 10.6 WorkSystem

Responsible for:

* Simple paid tasks.
* Work action validation.
* Rewards.
* Skill XP.
* Energy costs.

### 10.7 FarmingSystem

Responsible for:

* Soil state.
* Planting.
* Watering.
* Crop growth.
* Harvesting.
* Daily crop update.

### 10.8 FurnitureSystem

Responsible for:

* Placing furniture.
* Removing furniture.
* Moving furniture.
* Validating placement.
* Applying comfort or sleep bonuses.

### 10.9 CombatSystem

Responsible for:

* Player attacks.
* Enemy attacks.
* Damage.
* Health changes.
* Collapse at zero health.
* Enemy defeat.

Combat should stay simple in the MVP.

### 10.10 EventSystem

Responsible for:

* World event flags.
* Simple event chains.
* Triggering events based on time, location, or player actions.

The MVP goblin event chain should use this system.

## 11. Data-Driven Content

The game should distinguish between:

```text
Definition Data
Instance Data
Save Data
```

### 11.1 Definition Data

Definition data is static content loaded from external files.

Examples:

* Item definitions
* NPC definitions
* Furniture definitions
* Crop definitions
* Dialogue definitions
* Job definitions
* Enemy definitions

Definition data should not store mutable save state.

### 11.2 Instance Data

Instance data represents a specific object in the world.

Example:

```text
Item definition: wood
Item instance: 24 pieces of wood in the player inventory
```

### 11.3 Save Data

Save data stores the mutable state of the world.

Examples:

* Player position
* Inventory contents
* NPC relationship values
* Furniture placement
* Crop growth
* Current day
* Event flags

## 12. Data Registries

Loaded definitions should be stored in registries.

Recommended registries:

```text
ItemRegistry
NpcRegistry
DialogueRegistry
ScheduleRegistry
FurnitureRegistry
CropRegistry
JobRegistry
EnemyRegistry
MapRegistry
EventRegistry
```

Registries should allow lookup by stable string ID.

Example:

```text
ItemDefinition bread = ItemRegistry.Get("food_bread");
```

If a definition is missing, the game should fail gracefully where possible and report a clear validation error.

## 13. Example Data Formats

The MVP should use JSON for early data files.

### 13.1 Item Definition Example

```json
{
  "id": "food_bread",
  "displayName": "Bread",
  "description": "A simple loaf of bread.",
  "category": "Food",
  "maxStack": 10,
  "baseSellPrice": 8,
  "useEffects": [
    {
      "type": "RestoreNeed",
      "need": "Hunger",
      "amount": 25
    }
  ]
}
```

### 13.2 NPC Definition Example

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
  "initialRelationships": []
}
```

### 13.3 Schedule Definition Example

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

### 13.4 Dialogue Definition Example

```json
{
  "id": "dialogue_mara_woodfall",
  "lines": [
    {
      "id": "mara_intro_001",
      "conditions": {
        "minRelationship": 0
      },
      "text": "New face in Dusk Village? Then learn quickly: the forest gives, but it always takes back."
    },
    {
      "id": "mara_friend_001",
      "conditions": {
        "minRelationship": 40
      },
      "text": "You have lasted longer than most newcomers. That means something here."
    }
  ]
}
```

### 13.5 Furniture Definition Example

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

## 14. Save System

Save/load is mandatory for the MVP.

The save system should persist the current world state.

### 14.1 Save File Structure

Recommended early structure:

```text
SaveGame
    Metadata
    WorldState
    PlayerState
    EntityStates
    MapStates
    RelationshipStates
    EventFlags
    InventoryStates
```

### 14.2 Save Metadata

Should include:

```text
SaveVersion
GameVersion
CreatedAt
LastPlayedAt
PlayerName
CurrentDay
CurrentTime
```

### 14.3 Required MVP Save Data

The MVP must save:

* Player character
* Player attributes
* Player skills
* Player needs
* Player inventory
* Player money
* Player position
* Current day and time
* NPC relationship values
* Basic NPC states
* Crop growth state
* Furniture placement
* World event flags

### 14.4 Stable Save IDs

Save data should reference definitions by ID.

Example:

```json
{
  "itemDefinitionId": "food_bread",
  "quantity": 3
}
```

Do not save display names as primary identifiers.

### 14.5 Save Compatibility

The MVP does not need advanced save migration, but the structure should include a save version.

Example:

```json
{
  "saveVersion": 1
}
```

Future versions can use this to migrate old saves.

## 15. Maps and World Representation

The MVP should include a small set of maps.

Recommended MVP maps:

```text
map_player_home
map_dusk_village
map_general_store
map_tavern
map_church_of_light
map_guard_post
map_forest_edge
```

### 15.1 Tilemap Requirements

Maps should support:

* Tile layers
* Collision layer
* Object layer
* Spawn points
* Interaction points
* Exit/transition zones

### 15.2 Map Transitions

Map transitions should be data-driven where possible.

Example:

```text
From map_dusk_village at forest gate
    → map_forest_edge at village entrance
```

### 15.3 Coordinates

Use tile coordinates for map layout and world coordinates for rendering.

Recommended:

```text
Tile position: Point or Vector2Int-like structure
World position: Vector2
```

Tile size should be centralized in one configuration value.

## 16. Input and Commands

Player input should be converted into gameplay commands.

Examples:

```text
MoveCommand
InteractCommand
UseItemCommand
AttackCommand
OpenInventoryCommand
PlaceFurnitureCommand
TalkCommand
SleepCommand
```

This helps separate raw keyboard/controller input from gameplay simulation.

Future multiplayer will also benefit from command-style input processing.

## 17. UI Architecture

The UI should be separated from gameplay systems.

Recommended UI elements for MVP:

* Time display
* Day display
* Money display
* Need bars
* Inventory window
* Dialogue box
* Shop window
* Interaction prompt
* Pause menu
* Character creation UI

UI should read from state and send commands or requests to systems.

Avoid storing important gameplay state only inside UI classes.

## 18. Rendering

Rendering should be organized around:

* Tilemap rendering
* Entity sprite rendering
* UI rendering
* Camera
* Draw order

### 18.1 Pixel Art Requirements

The renderer should support:

* Pixel-perfect scaling where possible.
* Integer scaling options.
* Texture filtering appropriate for pixel art.
* Clear separation between world rendering and UI rendering.

### 18.2 Draw Order

Draw order should account for Y-position sorting for characters and objects.

Basic rule:

```text
Lower Y positions draw behind higher Y positions where appropriate.
```

Static tile layers may draw first, followed by entities and foreground layers.

## 19. Audio

Audio is not the main focus of the MVP, but the architecture should support:

* Music tracks
* Ambient loops
* Sound effects
* UI sounds
* Location-based music later

Recommended MVP audio systems:

```text
AudioManager
MusicPlayer
SoundEffectPlayer
```

## 20. Character Creation Implementation

The MVP character creation should produce a `PlayerCharacterState`.

Required output:

```text
Name
AppearanceData
StartingAgeCategory
OriginId
Attributes
StartingNeeds
StartingInventory
StartingMoney
StartingPosition
```

Character creation should not hardcode all options into UI. Age categories, origins, and starting options should eventually come from data.

## 21. Needs Implementation

Needs should be updated through `NeedsSystem`.

### 21.1 MVP Need Values

Use simple numeric ranges:

```text
Energy: 0–100
Hunger: 0–100
Health: 0–100
Mood: 0–100
```

### 21.2 Daily Reset

When the player sleeps:

* Energy restores based on bed quality.
* Hunger may decrease slightly overnight.
* Mood may recover based on home comfort.
* Health may recover if hunger is not critically low.

### 21.3 Action Costs

Actions may define costs.

Example:

```text
ChopTree:
    EnergyCost: 8
    Skill: Woodcutting
    SkillXp: 5
```

## 22. Inventory Implementation

Inventory should support:

* Stackable items.
* Non-stackable items later.
* Item definitions by ID.
* Quantity.
* Basic item use.
* Basic item transfer.
* Basic item selling.

Recommended MVP structure:

```text
Inventory
    Slots: List<InventorySlot>

InventorySlot
    ItemDefinitionId
    Quantity
```

Do not store full item definition data inside inventory slots.

## 23. Economy Implementation

The MVP economy should be simple.

Required:

* Player money.
* Shop inventory.
* Buy item.
* Sell item.
* Static prices.

Future economy systems may add:

* Supply and demand.
* Seasonal prices.
* Shortages.
* Taxes.
* Regional markets.
* NPC-owned businesses.

Do not implement these advanced systems in the MVP unless needed for testing.

## 24. NPC Implementation

NPCs should be entity-based.

Each NPC should have:

```text
EntityId
NpcDefinitionId
TransformComponent
CharacterComponent
ScheduleComponent
Relationship state with player
DialogueSetId
```

NPC state should be persistent where necessary.

### 24.1 MVP NPC Behavior

MVP NPC behavior can be simple:

* Follow schedule.
* Face player when talked to.
* Show dialogue.
* Accept gift.
* Modify relationship.
* Sell items if shopkeeper.

The MVP does not need deep AI.

## 25. Relationship Implementation

Relationships should be stored separately from NPC definitions.

Example:

```json
{
  "sourceEntityId": "player_main",
  "targetEntityId": "npc_mara_woodfall",
  "value": 25,
  "tags": ["Acquaintance"]
}
```

Relationship values can be interpreted as levels:

```text
0–19: Stranger
20–39: Acquaintance
40–59: Friend
60–79: Close Friend
80–100: Romantic Interest
```

These ranges are initial tuning values and may change.

## 26. Dialogue Implementation

Dialogue should be data-driven.

Dialogue selection may use:

* NPC ID
* Relationship value
* Time of day
* Current event flags
* First meeting flag
* Gift reaction
* Current weather later
* Current season later

The MVP only requires simple conditions.

Dialogue effects may include:

* Change relationship
* Set event flag
* Give item
* Start shop
* Start simple task

## 27. Work and Skill Implementation

Work activities should be implemented as reusable action definitions where possible.

Example structure:

```text
WorkActionDefinition
    Id
    DisplayName
    RequiredToolId
    EnergyCost
    TimeCost
    SkillId
    SkillXpReward
    MoneyReward
    ItemRewards
```

This allows jobs and gathering actions to share implementation patterns.

## 28. Farming Implementation

The MVP farming system should support:

* Till soil.
* Plant seed.
* Water crop.
* Advance crop growth on new day.
* Harvest crop.

Crop state should be stored per tile or crop entity.

Recommended crop data:

```text
CropDefinition
    Id
    DisplayName
    SeedItemId
    HarvestItemId
    DaysToGrow
    RequiredWateredDays
```

MVP crops:

```text
turnip
cabbage
nightroot_herb
```

## 29. Furniture and Home Implementation

Furniture should be placeable on a grid inside the player home.

Furniture placement should validate:

* Tile is inside allowed placement area.
* Tile is not blocked.
* Furniture does not overlap another object.
* Furniture fits within the room.

Furniture should be stored as persistent entities.

Required furniture data:

```text
FurnitureDefinitionId
MapId
TilePosition
Rotation
```

Home comfort can be calculated from placed furniture.

Example:

```text
HomeComfort = Sum(ComfortValue of furniture)
```

Sleep can use HomeComfort or bed quality to restore Energy and Mood.

## 30. Combat Implementation

Combat should remain simple in the MVP.

Required:

* Player attack.
* Enemy attack.
* Health damage.
* Enemy defeat.
* Player collapse at zero Health.
* Basic attack cooldown.
* Basic enemy chase/attack behavior.

Enemy state:

```text
EnemyDefinitionId
Health
AttackPower
MoveSpeed
AggroRange
AttackRange
```

MVP enemies:

```text
wolf
goblin_scout
```

The MVP does not need:

* Magic combat.
* Ranged combat.
* Armor.
* Complex AI.
* Bosses.
* Party combat.
* Permadeath.

## 31. Event System Implementation

The MVP should include a simple event flag system.

Example event flags:

```text
goblin_food_missing_reported
goblin_tracks_found
goblin_scout_encountered
goblin_threat_reported_to_guard
```

Events may be triggered by:

* Talking to an NPC.
* Entering a location.
* Interacting with an object.
* Defeating an enemy.
* Advancing to a new day.

The event system should be simple but extensible.

## 32. Modding Preparation

Full mod loading is not required for the MVP.

However, the project should follow mod-friendly rules:

* Content definitions should be external.
* Content IDs should be stable strings.
* Registries should load from folders, not only single files.
* Game systems should reference definitions by ID.
* Avoid hardcoded NPC names in systems.
* Avoid hardcoded item behavior when a data-driven effect can handle it.
* Save files should reference definition IDs, not asset paths directly.

Future mod folder structure may look like:

```text
mods/
    ExampleMod/
        mod.json
        data/
            items/
            npcs/
            dialogue/
            furniture/
        assets/
            textures/
            audio/
```

The MVP does not need to implement this folder yet, but the base content folder should be structured similarly.

## 33. Multiplayer Preparation

Multiplayer is not part of the MVP.

However, the architecture should avoid blocking future multiplayer.

Important rules:

* Simulation state should be serializable.
* Player input should become commands.
* World state should not depend directly on local UI.
* Entities should use stable IDs.
* Player characters and NPCs should share similar character structures.
* Systems should operate on world state, not on global static state.
* Avoid hidden state inside rendering classes.

Future multiplayer may use:

```text
Host-authoritative world simulation
Client input commands
World state synchronization
Offline player characters as NPCs
```

Do not implement multiplayer in the MVP.

## 34. Error Handling and Validation

Data loading should validate content at startup.

Validation should check:

* Duplicate IDs.
* Missing referenced IDs.
* Invalid item categories.
* Invalid schedule times.
* Missing dialogue sets.
* Invalid map IDs.
* Invalid sprite IDs.
* Invalid furniture sizes.
* Invalid crop definitions.

Errors should be clear and actionable.

Example:

```text
Data Error: NPC 'npc_mara_woodfall' references missing schedule 'schedule_shopkeeper_basic'.
```

## 35. Logging

The project should include a simple logging utility.

Recommended log categories:

```text
Info
Warning
Error
DataValidation
SaveLoad
Gameplay
```

Logging is especially important for:

* Data loading.
* Save/load.
* Missing assets.
* Invalid definitions.
* Event triggers.

## 36. Testing Strategy

The MVP should support basic testing where practical.

Priority test areas:

* Data loading.
* Registry lookups.
* Save/load serialization.
* Needs calculations.
* Inventory operations.
* Relationship changes.
* Crop growth.
* Furniture placement validation.

Not every MonoGame rendering feature needs automated testing, but pure logic systems should be testable without launching the full game.

## 37. Coding Style

Recommended C# style:

* Use clear, descriptive names.
* Keep systems small and focused.
* Prefer composition over inheritance.
* Avoid global mutable state.
* Avoid large manager classes that control unrelated systems.
* Use interfaces where they make testing or swapping implementations easier.
* Keep rendering and simulation separate.
* Keep data definitions immutable after loading where possible.

## 38. Implementation Order

The implementation order should follow the MVP milestones.

### Phase 1 — Foundation

Implement:

* MonoGame project setup.
* Screen system.
* Input handling.
* Basic rendering.
* Basic player movement.
* Camera.
* Basic tilemap or map loading.

### Phase 2 — World and Time

Implement:

* GameplayScreen.
* WorldState.
* TimeSystem.
* Day/night progression.
* Sleep and day transition.
* Basic HUD.

### Phase 3 — Character State

Implement:

* Player state.
* Attributes.
* Needs.
* Inventory.
* Money.
* Character creation output.

### Phase 4 — Data Loading

Implement:

* JSON loading.
* Registries.
* Item definitions.
* NPC definitions.
* Dialogue definitions.
* Furniture definitions.
* Basic validation.

### Phase 5 — NPC and Dialogue

Implement:

* NPC entities.
* Schedules.
* Dialogue UI.
* Relationship values.
* Basic gift interaction.

### Phase 6 — Work and Economy

Implement:

* Work actions.
* Skill XP.
* Shop buying/selling.
* Food use.
* Gathering.

### Phase 7 — Farming and Home

Implement:

* Soil.
* Seeds.
* Watering.
* Crop growth.
* Harvesting.
* Furniture placement.
* Comfort and sleep effects.

### Phase 8 — Forest and Combat

Implement:

* Forest map.
* Enemy entities.
* Basic combat.
* Wolf.
* Goblin scout.
* Goblin event flags.

### Phase 9 — Save/Load

Implement:

* Save file structure.
* Player save.
* World time save.
* Inventory save.
* NPC relationship save.
* Crop save.
* Furniture save.
* Event flag save.

### Phase 10 — MVP Polish

Implement:

* UI improvements.
* Interaction feedback.
* Balance pass.
* Bug fixing.
* Data cleanup.
* Documentation update.

## 39. Explicit Non-Goals for Technical MVP

Do not implement these systems during the technical MVP unless explicitly promoted into scope:

* Full multiplayer.
* Full mod loader.
* Procedural world map.
* Multiple villages.
* Advanced generational simulation.
* Marriage and children.
* Full inheritance.
* Advanced political simulation.
* Player becoming lord.
* Magic system.
* Boss fights.
* Goblin cave dungeon.
* Complex economy simulation.
* Complex weather.
* Complex temperature.
* Sanity/corruption.
* Large quest framework.
* Advanced AI.
* Steam Workshop.
* Cloud saves.

The architecture should prepare for these features, but they should not delay the first playable MVP.

## 40. Technical Design Summary

Dusk Village should be built as a modular, data-driven MonoGame project.

The most important technical decisions are:

* Separate simulation from rendering.
* Use stable IDs for persistent entities.
* Load gameplay content from external data files.
* Save mutable world state clearly.
* Keep player characters and NPCs structurally similar.
* Use simple systems first, then expand.
* Avoid implementing full future features before the MVP works.

The MVP should prove that the core daily life simulation works before expanding into advanced adventure, politics, generations, magic, modding, or multiplayer.
