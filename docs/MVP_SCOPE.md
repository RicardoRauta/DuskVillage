# Dusk Village — MVP Scope

## 1. Purpose

This document defines the scope of the first playable MVP for **Dusk Village**.

The MVP is not meant to represent the full game. Its purpose is to prove the core life simulation loop:

> Create a character, live daily routines in Dusk Village, manage basic needs, work, earn money, interact with NPCs, improve a home, save progress, and experience the village as a persistent world.

Adventure, deep politics, advanced generations, magic, full modding, and multiplayer are important long-term goals, but they are not the main focus of the MVP.

## 2. MVP Design Goal

The MVP should prove that the game is interesting even without a large adventure system.

The player should be able to:

1. Create a human character.
2. Start in Dusk Village.
3. Experience a fast day/night cycle.
4. Manage basic needs.
5. Perform simple work activities.
6. Earn and spend money.
7. Interact with NPCs.
8. Improve or decorate a basic home.
9. See the world persist through save/load.
10. Experience a small threat from the dark forest.

The MVP should answer this question:

> Is the daily life simulation of Dusk Village compelling enough to build the rest of the game around it?

## 3. Target Platform

Initial target:

* PC
* C#
* MonoGame
* 2D pixel art

The MVP does not need console, mobile, Steam integration, achievements, workshop support, cloud saves, or multiplayer.

## 4. Core MVP Loop

The main MVP gameplay loop is:

1. Wake up at home.
2. Check needs, time, money, and current tasks.
3. Choose daily activity:

   * Work
   * Gather resources
   * Socialize
   * Buy/sell items
   * Improve home
   * Explore nearby forest
4. Spend time and energy.
5. Gain resources, money, relationship points, or skill progress.
6. Return home.
7. Sleep.
8. Save world progress.
9. Advance to next day.

The loop should work even if the player never enters combat.

## 5. MVP Content Scale

The MVP should use a reduced but representative version of Dusk Village.

### Village Content

MVP Dusk Village should include:

* Player home
* Village square
* General store
* Tavern or inn
* Blacksmith or workshop
* Small church of the Archangel of Light
* Guard post
* Forest entrance
* Basic build/decorate area inside the player home

### NPC Count

The full game may start with 40–50 fixed NPCs.

The MVP should start smaller:

* 8 to 12 fixed NPCs
* Each NPC should have:

  * Name
  * Age group
  * Profession or role
  * Basic schedule
  * Basic dialogue
  * Relationship value with the player

Recommended MVP NPC roles:

* Local lord or village authority representative
* Guard
* Shopkeeper
* Tavern keeper
* Blacksmith or carpenter
* Priest/priestess of the Archangel of Light
* Farmer
* Hunter or woodcutter
* Healer
* One or two romance-focused NPCs
* One suspicious or mysterious NPC

## 6. Character Creation

The MVP character creator should support:

* Character name
* Human ancestry only
* Basic appearance selection
* Starting age category
* Starting origin
* Attribute allocation

### Starting Age Categories

The MVP should support three starting age categories:

* Young Adult
* Adult
* Older Adult

Age affects starting attributes.

Example direction:

* Young Adult: higher Constitution and Energy
* Adult: balanced attributes
* Older Adult: higher Wisdom, lower Constitution

### Origins

The MVP should include a small set of starting origins:

* Newcomer
* Local villager
* Former laborer
* Poor wanderer

Origins may affect:

* Starting money
* Starting relationships
* Starting tools
* Starting reputation

The **Newcomer** origin should be the most customizable and default-friendly option.

## 7. Attributes

The MVP should implement the six core attributes:

| Attribute    | Purpose                                           |
| ------------ | ------------------------------------------------- |
| Strength     | Physical labor, carrying, melee work              |
| Agility      | Movement, precision, gathering, evasion           |
| Constitution | Health, illness resistance, hunger/cold tolerance |
| Intelligence | Learning, crafting, future magic support          |
| Charisma     | Relationships, trade, persuasion                  |
| Wisdom       | Survival, perception, judgment                    |

Attributes do not need complex formulas in the MVP, but they should influence at least some actions.

Examples:

* Strength affects woodcutting efficiency.
* Constitution affects maximum Health or Energy.
* Charisma affects relationship gains or shop prices.
* Wisdom affects foraging or survival outcomes.
* Intelligence affects learning speed or crafting success.

## 8. Skills

The MVP should include a small set of practical skills.

Recommended MVP skills:

* Farming
* Woodcutting
* Crafting
* Social
* Survival
* Combat

Skills should improve through use.

The MVP does not need a full skill tree. A simple experience and level system is enough.

Example:

```text
Woodcutting XP increases when cutting trees.
Social XP increases when talking, gifting, or completing social tasks.
Combat XP increases when defeating hostile creatures.
```

## 9. Time System

The MVP should implement a fast day structure inspired by Stardew Valley.

### Requirements

* Time advances during gameplay.
* The player wakes up in the morning.
* The day has visible time progression.
* The player can sleep to end the day.
* Staying awake too late causes collapse or forced day end.
* NPC schedules use the time system.

### Suggested Day Length

Initial tuning target:

* 1 in-game day = 15 to 20 real-time minutes

This can be adjusted during testing.

## 10. Needs System

The MVP should include a simple version of the needs system.

### Required MVP Needs

#### Energy

Used by work, gathering, crafting, combat, and exploration.

If Energy reaches zero:

* The player becomes inefficient.
* The player may collapse if they continue pushing.

#### Hunger

Represents food requirement.

If Hunger becomes too low:

* Energy recovery is reduced.
* Mood decreases.
* Health may slowly decrease.

#### Health

Represents physical condition.

Health is reduced by:

* Combat
* Accidents
* Severe hunger
* Dangerous events

If Health reaches zero:

* The player collapses.
* Death does not need to be fully implemented in the MVP.

#### Mood

Represents emotional state.

Affected by:

* Good food
* Bad food
* Social interaction
* Poor sleep
* Hunger
* Home comfort

Mood can affect:

* Work efficiency
* Relationship gains
* Optional dialogue tone

### Not Required in MVP

The following needs are not required for the MVP:

* Hygiene
* Sanity
* Corruption
* Faith
* Stress
* Complex temperature simulation

Temperature may be added later.

## 11. NPC System

The MVP should implement basic NPC simulation.

### Required

Each NPC should support:

* Persistent ID
* Name
* Age group
* Role/profession
* Home location
* Work location
* Basic schedule
* Basic dialogue
* Relationship value with the player

### NPC Schedules

NPCs should move between simple schedule points.

Example:

```text
Morning: home
Day: work location
Evening: tavern or home
Night: home
```

The MVP does not need complex autonomous decision-making.

## 12. Relationship System

The MVP should include a basic relationship system.

### Required

The player can:

* Talk to NPCs
* Give simple gifts
* Gain or lose relationship points
* Unlock simple dialogue based on relationship level

### Relationship Levels

Example levels:

* Stranger
* Acquaintance
* Friend
* Close Friend
* Romantic Interest

Marriage, children, family simulation, and inheritance are not required for the MVP.

However, the data model should not prevent these systems from being added later.

## 13. Work and Economy

The MVP should include simple work and money systems.

### Required Work Activities

At least three work activities should be implemented:

* Farming task
* Woodcutting/gathering task
* Paid village job or errand

Examples:

* Chop wood and sell logs.
* Harvest crops.
* Deliver an item to an NPC.
* Help repair a fence.
* Gather herbs for the healer.
* Patrol a forest edge for the guard.

### Economy Requirements

The MVP should support:

* Player money
* Item buying
* Item selling
* Basic shop inventory
* Basic item prices

The economy does not need supply/demand simulation in the MVP.

## 14. Inventory and Items

The MVP should include a basic inventory system.

### Required

Items should support:

* ID
* Display name
* Description
* Stack size
* Type/category
* Sell price
* Optional use effect

MVP item categories:

* Food
* Resource
* Tool
* Furniture
* Quest item

Example MVP items:

* Bread
* Apple
* Wood
* Stone
* Iron scrap
* Herb
* Simple sword
* Axe
* Bed
* Chair
* Table

## 15. Home, Building, and Decoration

The MVP should include a basic home system.

### Required

The player should have:

* A small starting home or room
* A placeable furniture system
* At least a few furniture items
* Basic comfort value

Furniture should be placeable on a grid.

Example MVP furniture:

* Bed
* Chair
* Table
* Chest
* Rug
* Candle or lamp

### Home Effects

The MVP should include at least one gameplay effect from the home.

Examples:

* Better bed restores more Energy.
* Furniture increases Comfort.
* Higher Comfort improves Mood recovery after sleeping.

The MVP does not need full house expansion or exterior building.

## 16. Farming and Resource Gathering

The MVP should include a minimal farming and gathering system.

### Farming Requirements

The player can:

* Till soil
* Plant seeds
* Water crops
* Wait for crops to grow
* Harvest crops
* Sell crops or use them as food

Only a small number of crops are required.

Recommended MVP crops:

* Turnip
* Cabbage
* Nightroot herb

### Gathering Requirements

The player can:

* Chop small trees
* Gather herbs
* Pick berries or mushrooms
* Collect stone or basic resources

Gathering should consume Energy.

## 17. Forest Threat MVP

The MVP should include a small version of the dark forest threat.

### Required

The forest should contain:

* A small explorable forest area
* Wolves or hostile beasts
* Goblin scout or goblin raider
* Basic combat encounter
* Simple danger warning from villagers

### Goblin Event Chain

The MVP should include a short event chain:

1. Villagers report missing food.
2. The player can investigate the forest edge.
3. The player finds signs of goblins.
4. The player fights or avoids a goblin scout.
5. The village reacts to the discovery.

The full goblin cave and goblin leader boss are not required for the first MVP, but can be planned as the next vertical slice.

## 18. Combat

Combat should be simple in the MVP.

### Required

The MVP should support:

* Player attack
* Enemy attack
* Health damage
* Enemy death
* Player collapse at zero Health
* Basic melee weapon
* At least two enemy types

Recommended MVP enemies:

* Wolf
* Goblin scout

The MVP does not need:

* Magic combat
* Ranged combat
* Boss fights
* Armor systems
* Complex enemy AI
* Party combat
* Permadeath

## 19. Save and Load

Save/load is mandatory for the MVP.

The MVP should persist:

* Player character
* Player attributes
* Player skills
* Player inventory
* Player money
* Player home furniture
* Current day and time
* NPC relationship values
* Basic NPC states
* World event flags
* Crop growth state

Persistent IDs should be used for important entities.

## 20. Data-Driven Content

The MVP should begin using data-driven content where practical.

At minimum, the following should be loadable from external data files:

* Items
* NPC definitions
* Dialogue lines
* Basic schedules
* Furniture definitions

Recommended format:

* JSON for early development

The MVP does not need a full mod loader, but the project should avoid hardcoding all content directly into gameplay systems.

## 21. Modding Preparation

Full mod support is not required for the MVP.

However, the codebase should be structured so modding can be added later.

### Required Principles

* Content definitions should be separated from code.
* Data should use stable IDs.
* Game systems should not depend on hardcoded NPC names.
* Items, NPCs, dialogue, furniture, and jobs should be extendable later.
* Save files should handle missing or changed content gracefully when possible.

## 22. Multiplayer Preparation

Multiplayer is not required for the MVP.

However, the architecture should avoid decisions that would make future multiplayer impossible.

### Required Principles

* Separate simulation logic from rendering where possible.
* Keep world state serializable.
* Use stable entity IDs.
* Avoid storing important state only in UI classes.
* Avoid direct dependencies between player input and all world simulation.
* Design characters and NPCs as entities using similar data structures.

Offline player-as-NPC behavior is not required for the MVP.

## 23. Out of Scope for MVP

The following features are explicitly out of scope for the MVP:

* Full multiplayer
* Online hosting
* Steam Workshop
* Full mod loader
* Full procedural world map
* Multiple villages
* Full 40–50 NPC population
* Marriage
* Children
* Full aging and death cycle
* Inheritance
* Advanced politics
* Player becoming lord
* Full council simulation
* Magic system
* Demon/devil/angel questlines
* Full goblin cave dungeon
* Goblin leader boss
* Complex economy simulation
* Seasonal festivals
* Large quest system
* Advanced combat
* Ranged weapons
* Armor system
* Large-scale construction
* House expansion
* Animal husbandry
* Complex weather and temperature
* Sanity/corruption systems

These systems are important for the full game but should not block the first playable MVP.

## 24. MVP Acceptance Criteria

The MVP can be considered successful when the following are true:

### Core Life Loop

* The player can create a character.
* The player can wake up, spend a day, sleep, and advance to the next day.
* Time progression works.
* Needs change over time and through actions.

### Village Loop

* The player can walk around a small version of Dusk Village.
* NPCs follow simple schedules.
* The player can talk to NPCs.
* Relationship values can change.

### Work Loop

* The player can perform at least three useful daily activities.
* The player can earn money.
* The player can spend money at a shop.

### Home Loop

* The player has a home.
* The player can place furniture.
* Furniture has at least one gameplay effect.

### Survival Loop

* Energy, Hunger, Health, and Mood affect gameplay.
* Food can restore Hunger.
* Sleep restores Energy.
* Poor condition creates penalties.

### Threat Loop

* The player can enter a forest area.
* The player can encounter at least one hostile creature.
* Basic combat works.
* A small goblin-related event chain exists.

### Persistence

* The player can save the game.
* The player can load the game.
* Important state persists correctly.

## 25. Recommended MVP Milestones

### Milestone 1 — Project Foundation

* MonoGame project setup
* Game state management
* Input handling
* Basic rendering
* Tilemap loading or simple map system
* Player movement
* Camera

### Milestone 2 — Time and Character State

* Time system
* Day/night cycle
* Sleep and day transition
* Player attributes
* Needs system
* Basic UI for time, needs, money, and inventory

### Milestone 3 — Village and NPCs

* Small Dusk Village map
* NPC entity system
* NPC schedules
* Dialogue system
* Relationship values

### Milestone 4 — Items, Inventory, and Economy

* Item definitions
* Inventory
* Shop buying/selling
* Money
* Basic food usage

### Milestone 5 — Work and Skills

* Farming prototype
* Woodcutting/gathering
* Simple paid job
* Skill XP and levels

### Milestone 6 — Home and Furniture

* Player home
* Furniture definitions
* Furniture placement
* Comfort or sleep bonus

### Milestone 7 — Forest and Threat

* Forest area
* Basic enemy AI
* Combat
* Wolf enemy
* Goblin scout enemy
* Small goblin event chain

### Milestone 8 — Save/Load

* Save player state
* Save inventory
* Save world time
* Save NPC relationships
* Save crops
* Save furniture placement
* Save event flags

### Milestone 9 — MVP Polish Pass

* Basic UI polish
* Interaction feedback
* Balancing pass
* Bug fixing
* Playtest loop
* Documentation update

## 26. MVP Development Rule

When in doubt, prioritize systems that support daily life simulation over systems that support adventure.

The MVP should prove this hierarchy:

1. Life simulation
2. Work and survival
3. Home and relationships
4. World persistence
5. Light danger and adventure

Adventure should support the life sim, not replace it.
