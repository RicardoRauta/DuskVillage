# Dusk Village — Game Design Document

## Version 0.5 — Core Vision

## 1. Game Overview

**Dusk Village** is a 2D pixel art dark fantasy life simulation sandbox RPG developed in **C# using the MonoGame framework**.

The game is inspired by **Stardew Valley**, **World Neverland: Elnea Kingdom**, and **The Sims 4**, but its core focus is different: it is a generational life simulation set in a melancholic dark fantasy world where adventure is only one possible path.

The player is not forced to become a hero or adventurer. They may choose to live as a farmer, artisan, guard, noble, hunter, merchant, builder, criminal, occultist, scholar, family head, landowner, or any other role the simulation allows, as long as they have enough skill, reputation, resources, and influence.

The game should support modding from early architecture decisions and may eventually support multiplayer worlds hosted by players.

## 2. Genre

* Sandbox
* Life simulation
* Dark fantasy
* RPG
* Social simulation
* Generational simulation
* Survival
* Building and decoration
* Optional adventure and combat
* Mod-compatible
* Future multiplayer support

## 3. Core Player Fantasy

The central fantasy is:

> Live a full life, survive hardship, build a home, form relationships, raise a family, gain influence, and leave a legacy in a dark fantasy world.

The player may spend an entire lifetime without becoming an adventurer. Combat and exploration exist, but they are optional paths rather than the main required progression.

A valid playthrough could focus on farming, crafting, politics, family, trade, construction, survival, social influence, or generational legacy.

## 4. Tone and Atmosphere

The game should feel like **melancholic dark fantasy with beauty and tragedy**.

The world is not purely grim or hopeless. People fall in love, build homes, raise children, celebrate festivals, and create families. However, tragedy is always present. Characters may die from illness, monsters, famine, violence, accidents, curses, age, poverty, or political conflict.

The emotional tone should come from contrast:

* Warm homes surrounded by dangerous forests.
* Families growing while older generations die.
* Beautiful seasons hiding hunger, cold, and monsters.
* Peaceful routines interrupted by tragedy.
* A village that feels alive, fragile, and worth protecting.

## 5. Design Pillars

### 5.1 Freedom of Life

The player should be free to decide how their character lives.

Possible life paths include:

* Farmer
* Artisan
* Guard
* Hunter
* Merchant
* Noble
* Criminal
* Builder
* Occultist
* Scholar
* Healer
* Adventurer
* Political figure
* Family leader
* Landowner

The game should not force a single main campaign path.

### 5.2 Generational Simulation

NPCs and player characters age, marry, have children, grow old, and die.

The player’s current character is temporary, but their impact may remain through:

* Children
* Family name
* Property
* Reputation
* Wealth
* Businesses
* Political influence
* Enemies
* Debts
* Curses
* Buildings
* Social changes

When a character dies, the world continues.

### 5.3 Work, Professions, and Influence

Work and professions are central systems.

Professions are not just menu choices. They exist inside the village economy and social hierarchy. Important positions may be limited and controlled by local authority.

For example, the village may only have a limited number of official guard positions. A player who wants to become a guard can still train combat, take protection jobs, defend caravans, patrol roads, and gain reputation. When a position opens, they may apply or be invited.

### 5.4 Survival in a Dark World

Survival should matter, but should not become excessive micromanagement.

The player must care about food, energy, health, safety, weather, shelter, money, and social support.

Survival should create meaningful decisions, such as:

* Stockpiling food before winter.
* Owning a safe home.
* Wearing proper clothing in cold weather.
* Avoiding dangerous roads at night.
* Building relationships to receive help in times of crisis.
* Choosing between work, rest, travel, and family.

### 5.5 Home, Building, and Decoration

The player’s home is one of the central emotional and mechanical anchors of the game.

A house represents:

* Safety
* Status
* Family
* Comfort
* Storage
* Production
* Decoration
* Inheritance
* Legacy

The player should be able to build, expand, furnish, decorate, and personalize their home.

### 5.6 A Living and Tragic World

The world should continue to move forward even when the player is not controlling every event.

NPCs should have routines, jobs, relationships, families, ambitions, conflicts, and risks.

Possible world events include:

* Births
* Marriages
* Deaths
* Illness
* Monster attacks
* Food shortages
* Political disputes
* Crimes
* Funerals
* Festivals
* Economic shifts
* Changes in leadership
* Family conflicts

### 5.7 Modding from the Start

The game should be designed with modding in mind from early development.

Systems that should eventually be moddable include:

* Items
* NPCs
* Dialogue
* Jobs
* Events
* Maps
* Furniture
* Recipes
* Clothing
* Origins
* Traits
* Professions
* Quests
* World locations

Content should be data-driven whenever possible.

## 6. Player Character

### 6.1 Playable Ancestry

In the initial version, all playable characters are human.

Other ancestries or fantasy races may be added later through expansions or mods.

This keeps the initial scope focused and reinforces the theme of human lives, families, survival, and mortality in a hostile dark fantasy world.

### 6.2 Character Creation

During character creation, the player defines:

* Name
* Appearance
* Starting age
* Origin
* Attributes
* Traits
* Starting resources
* Starting relationships
* Starting social position
* Starting home or lack of home

Character creation should affect gameplay, not only visuals.

### 6.3 Starting Age

The player may start as:

#### Young Adult

Advantages:

* Higher vitality
* Higher Constitution
* More years before old age
* Greater growth potential

Disadvantages:

* Lower Wisdom
* Lower reputation
* Less experience
* Fewer initial resources

#### Adult

Advantages:

* Balanced attributes
* Some life experience
* More social credibility
* May start with a profession or reputation

Disadvantages:

* Less lifetime remaining than a young adult
* Less growth potential

#### Older Adult

Advantages:

* Higher Wisdom
* More experience
* Better starting reputation
* Possible social influence
* Better access to some positions or contacts

Disadvantages:

* Lower Constitution
* Lower Energy
* Higher illness risk
* Fewer remaining years
* Weaker performance in physical work

## 7. Attributes and Skills

### 7.1 Core Attributes

Each character has a combination of RPG-style attributes.

Recommended attributes:

| Attribute    | Purpose                                                        |
| ------------ | -------------------------------------------------------------- |
| Strength     | Physical labor, melee combat, construction, carrying capacity  |
| Agility      | Movement, precision, hunting, stealth, dodging                 |
| Constitution | Health, illness resistance, hunger resistance, cold resistance |
| Intelligence | Learning, magic, alchemy, engineering, medicine                |
| Charisma     | Relationships, negotiation, romance, leadership, trade         |
| Wisdom       | Survival, perception, judgment, faith, experience              |

### 7.2 Skills

Skills improve through practice, training, or study.

Possible skills:

* Farming
* Construction
* Cooking
* Hunting
* Mining
* Fishing
* Trade
* Guarding
* Combat
* Medicine
* Alchemy
* Tailoring
* Carpentry
* Blacksmithing
* Leadership
* Stealth
* Occultism
* Magic
* Survival

Attributes influence potential and learning speed, while skills represent practical experience.

## 8. Time, Calendar, and Aging

### 8.1 Time System

The game uses a fast day structure inspired by Stardew Valley.

A typical day includes:

* Morning
* Afternoon
* Evening
* Night
* Late night

Each day should be short enough to maintain rhythm, but long enough for meaningful decisions.

Initial direction:

* One in-game day lasts around 15 to 20 real-time minutes.
* The day ends when the character sleeps, collapses, or reaches a late-night limit.
* Important events may happen at specific times.

### 8.2 Calendar and Aging

The calendar is accelerated.

One in-game year represents roughly ten years of a character’s life.

This allows generational gameplay without requiring hundreds of hours for a single generation to pass.

Characters move through life stages:

* Baby
* Child
* Teenager
* Young adult
* Adult
* Older adult
* Elder
* Ancient elder

## 9. Death, Inheritance, and Legacy

Death is part of the core experience.

Characters may die from:

* Old age
* Illness
* Hunger
* Cold
* Accidents
* Combat
* Monsters
* Crime
* Execution
* Curses
* Tragic events
* Consequences of choices

When a player character dies, the player may:

1. Continue as a family member.
2. Choose an heir from descendants or relatives.
3. Create a new character in the same world.

Death does not end the save file. It changes the story.

### 9.1 Inheritance

A dead character’s assets and consequences may be inherited or redistributed.

Possible inherited elements:

* House
* Land
* Money
* Items
* Businesses
* Debts
* Family reputation
* Titles
* Political relationships
* Enemies
* Curses
* Animals
* Employees
* Buildings
* Books and knowledge

Inheritance depends on family structure, law, social status, debts, and local authority.

## 10. Needs System

The game should include basic needs without excessive micromanagement.

Recommended initial needs:

### Energy

Used by work, combat, travel, building, mining, and other demanding activities.

Low Energy reduces efficiency and may cause collapse.

### Hunger

Food matters for survival.

Low Hunger reduces Energy, Health, and Mood. Extreme hunger may cause illness or death.

### Health

Represents physical condition.

Affected by wounds, illness, hunger, cold, poison, curses, aging, accidents, and monster attacks.

### Mood

Represents emotional state.

Affected by relationships, grief, loneliness, comfort, food quality, home quality, family, work, success, trauma, and safety.

### Temperature / Protection

Represents exposure to cold, heat, rain, and hostile weather.

This should be contextual rather than a constantly annoying meter.

Secondary needs that may be added later:

* Hygiene
* Sanity
* Corruption
* Faith
* Security
* Comfort
* Stress

## 11. World Structure

The world uses a hybrid structure.

### 11.1 Fixed Starting Village

The starting village should be fixed and memorable.

It contains the core NPCs, families, politics, jobs, early conflicts, and emotional identity of the game.

### 11.2 Procedural World Map

Outside the village, there is a larger procedural world map.

The world map may contain:

* Other villages
* Forests
* Caves
* Ruins
* Cemeteries
* Swamps
* Roads
* Mines
* Towers
* Cursed places
* Camps
* Faction-controlled regions

The player selects a location on the world map and travels there.

Travel may cost time, energy, food, and may involve risk.

### 11.3 Explorable Locations

World map locations may generate explorable 2D areas.

Examples:

* Hunting forest
* Mining cave
* Ancient ruin
* Abandoned farm
* Destroyed village
* Profaned temple
* Infested mine
* Ambush road
* Cursed swamp

Some locations may be temporary. Others may become permanent.

## 12. Dusk Village

### 12.1 Identity

Dusk Village is the fixed starting village and the emotional center of the game.

It is a small frontier settlement on the edge of a dark forest. The village survives through farming, hunting, logging, trade, crafting, and local protection work.

The village should feel small enough for the player to know everyone, but complex enough to support many years of stories.

### 12.2 Region

Dusk Village is located near a dark forest.

The forest provides resources but also danger.

The region may include:

* Hunting trails
* Dangerous clearings
* Logging camps
* Root-covered ruins
* Natural caves
* Wolf dens
* Goblin camps
* Marked ritual trees
* Missing traveler sites
* Poorly protected trade roads

## 13. Government and Politics

Dusk Village is governed by a local lord and a village council.

The lord controls or influences:

* Land distribution
* Guard hiring
* Village defense
* Taxes
* Trade licenses
* Building permissions
* Official positions
* Important trials
* Relations with other settlements
* Responses to monster attacks

The council represents influential families, professions, and local interests.

### 13.1 Configurable Starting Lord

At world creation, the player may choose or randomize the starting lord’s profile. This works as a narrative difficulty modifier.

Possible lord profiles:

#### Just and Tired

A decent ruler, but worn down by age and crisis.

Gameplay effects:

* Fairer taxes
* More stable society
* Less corruption
* Slower crisis escalation
* Easier access to legal permissions

#### Weak and Manipulated

A lord controlled by council members, rich families, or outside interests.

Gameplay effects:

* Stronger council
* More corruption
* Influence matters more
* More political opportunities
* Greater chance of political change

#### Cruel and Authoritarian

A harsh ruler who values order and power over compassion.

Gameplay effects:

* Higher taxes
* More repression
* Harsher punishments
* Less social freedom
* Greater difficulty for newcomers
* Higher chance of rebellion or conspiracy

#### Well-Meaning but Incompetent

A ruler who wants to help but makes poor decisions.

Gameplay effects:

* Weaker village defense
* Slow response to crises
* Poor resource management
* Higher risk of shortages
* Greater opportunity for the player to gain influence by solving problems

### 13.2 Political Change

Politics may change over generations.

Possible changes:

* The lord dies and an heir takes over.
* The council gains power.
* A player family marries into the ruling family.
* A player becomes a council member.
* A player becomes captain of the guard.
* The Church of Light gains influence.
* A revolt removes the lord.
* A rich family buys power.
* A new form of government emerges.

Politics should affect taxes, jobs, security, trade, land, building permissions, and reputation.

## 14. Religion and Cosmology

The world does not have gods.

Instead, powerful entities exist, such as:

* Angels
* Archangels
* Devils
* Demons

These beings are not traditional gods, but they shape religion, culture, fear, magic, institutions, and conflict.

### 14.1 Church of the Archangel of Light

Dusk Village has a church dedicated to an Archangel of Light.

The church may provide:

* Basic healing
* Illness treatment
* Blessings
* Funeral rites
* Support during grief
* Aid for orphans
* Moral quests
* Marriage records
* Birth records
* Death records

The church may also create conflict through:

* Suspicion of magic
* Conflict with occultists
* Political pressure
* Fear of demonic pacts
* Secrets about angels, devils, and demons

## 15. Professions

Professions are divided between formal positions and informal activities.

### 15.1 Formal Professions

Formal professions are recognized by the village and may have limited positions.

Examples:

* Guard
* Official blacksmith
* Healer
* Priest
* Administrator
* Tax collector
* Licensed hunter
* Teacher
* Councilor
* Captain of the guard
* Lord’s steward

These jobs may require approval, reputation, skill, social trust, or political influence.

### 15.2 Informal Activities

Informal activities can be performed freely if the character has tools, resources, and skill.

Examples:

* Farming
* Logging
* Hunting
* Cooking
* Crafting furniture
* Raising animals
* Mining
* Fishing
* Sewing clothes
* Brewing simple potions
* Building
* Exploring ruins

Some informal activities may be illegal or socially dangerous without permission.

## 16. Magic

Magic exists, but it is not easily accessible at the start.

To learn magic, a character may need:

* High Intelligence
* Books or grimoires
* A mentor
* Time to study
* Money
* Social contacts
* Courage to face risk
* Occult knowledge
* Access to forbidden places

Magic should feel rare, powerful, dangerous, and socially meaningful.

Possible ways to learn magic:

* Study with a mentor
* Find a grimoire
* Join an order
* Make a pact
* Explore ancient ruins
* Become an alchemist’s apprentice
* Inherit family knowledge
* Discover hidden talent
* Be changed by a supernatural event

Society’s view of magic may vary by region. Some communities fear it, regulate it, condemn it, or secretly rely on it.

## 17. Romance, Marriage, and Family

All compatible adult NPCs may become romanceable depending on relationships, context, personality, and marital status.

The game should not restrict romance to only a small list of candidates.

However, some initial NPCs should be designed with more detailed romance content, including:

* More dialogue
* Personal events
* Family conflicts
* Strong preferences
* Personal ambitions
* Emotional arcs
* Connections to important systems

Romance may progress into:

* Dating
* Engagement
* Marriage
* Shared home
* Children
* Inheritance
* Family alliances
* Domestic conflict
* Separation
* Widowhood

Marriage is not an ending. It is part of life, legacy, and generational simulation.

## 18. Initial Threat: The Dark Forest

The main early threat to Dusk Village comes from the dark forest.

Threats include:

* Goblins
* Wolves
* Bears
* Wild beasts
* Minor corrupted creatures

These threats affect the economy, safety, jobs, food, travel, and politics.

### 18.1 Goblins as a Systemic Threat

Goblins should not be generic combat enemies only. They should affect the simulation.

Possible behaviors:

* Steal food
* Steal tools
* Attack travelers
* Raid isolated farms
* Capture animals
* Sabotage trails
* Set ambushes
* Observe the village before attacking
* Retreat if the village is well-defended
* Grow stronger if ignored

### 18.2 Goblin Cave

The forest contains a goblin cave that acts as the first major local adventure threat.

It may contain:

* Goblin patrols
* Simple traps
* Stolen supplies
* Captured animals
* Remains of travelers
* Items from villagers
* Dark tunnels
* Blocked passages
* Signs of growing organization

### 18.3 Goblin Leader

The goblin cave contains a stronger goblin leader.

This leader is smarter, more dangerous, and more organized than ordinary goblins. The leader explains why attacks have become more coordinated.

The goblin leader acts as:

* First local boss
* Source of escalating attacks
* Threat to farms and caravans
* Combat test for adventurer characters
* Reputation opportunity for future guards, hunters, or protectors
* Event that can change village security

### 18.4 Consequences

If goblins are ignored:

* More food is stolen
* Animals die
* Prices rise
* Guards become overwhelmed
* Villagers may be injured
* Trade routes become unsafe
* The council pressures the lord
* Taxes may increase to fund defense
* New guard positions may open

If goblins are defeated or contained:

* The village becomes safer
* The player gains reputation
* Stolen goods may be returned
* The guard recognizes the player’s skill
* The economy improves
* The leader may be killed, captured, or driven away

## 19. Multiplayer Direction

The game may eventually support player-hosted multiplayer worlds.

One player hosts a world. Other players create characters within that world.

Because the game has aging and generations, player characters should not simply disappear when a player logs out.

When offline, a player character may continue existing as an autonomous NPC with safe behavior.

Offline characters may:

* Go to work
* Eat automatically if food is available
* Sleep
* Maintain basic needs
* Interact lightly with family
* Follow domestic routines
* Earn basic income
* Age
* Participate in simple family events

Offline characters should not automatically:

* Marry
* Divorce
* Have children
* Sell important items
* Go on dangerous adventures
* Spend large amounts of money
* Join factions
* Commit serious crimes
* Risk their life
* Move house
* Make irreversible decisions

Major decisions should require player permission or predefined settings.

## 20. Technical Direction

The game is planned for:

* C#
* MonoGame framework
* 2D pixel art
* PC as the initial target platform
* Data-driven content
* Mod-compatible architecture

Recommended early technical principles:

* Keep game content separate from engine code.
* Store items, NPCs, dialogue, jobs, events, maps, and recipes in external data files where possible.
* Use stable IDs for all persistent world objects.
* Design save files to support long-term simulation.
* Treat NPCs, player characters, items, buildings, jobs, and locations as persistent entities.
* Prepare architecture for future multiplayer, even if the MVP is single-player.
* Avoid hardcoding village content directly into gameplay systems.

## 21. MVP Direction

The first playable MVP should prove that the game is interesting without relying on adventure.

Recommended MVP focus:

* Character creation
* Fixed starting village
* Basic time system
* Basic needs
* Basic NPC routines
* Basic relationships
* Simple jobs or work tasks
* Simple money economy
* Home ownership or basic housing
* Basic building or decoration
* Aging framework
* Save/load persistence
* Early monster threat simulation
* Initial goblin event chain

Adventure, magic, complex politics, full multiplayer, and deep modding can be expanded after the core life simulation loop works.

## 22. Current Design Summary

Dusk Village is a generational dark fantasy life simulation sandbox.

It is not just “Stardew Valley with monsters.”

It is not just “The Sims in a medieval world.”

It is a game about living, surviving, aging, forming families, gaining influence, building a home, facing tragedy, and leaving a legacy in a dark fantasy village surrounded by danger.
