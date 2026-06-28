# Dusk Village — Character Presets

## Purpose

Character presets are reusable character files for the player creator and future NPC authoring.

They are not save games. A preset stores character identity, appearance choices, attributes, needs, and initial skills. It does not store world progress, day/time progression, inventory, map position, relationships, event flags, crops, furniture, or any other mutable world state.

## File Format

Preset files use JSON and the extension:

```text
.dvchar.json
```

The current schema version is `1`.

Example:

```json
{
  "schemaVersion": 1,
  "presetId": "character_7f3a9c",
  "name": "Alden",
  "familyName": "Vale",
  "ageCategoryId": "young_adult",
  "originId": "newcomer",
  "birthdaySeasonId": "spring",
  "birthdayDay": 1,
  "motivationId": "fresh_start",
  "appearance": {
    "layers": {
      "01body": "fbas_01body_human_00",
      "03fot1": "fbas_03fot1_boots_00a",
      "04lwr1": "fbas_04lwr1_longpants_00a",
      "05shrt": "fbas_05shrt_longshirt_00a",
      "13hair": "fbas_13hair_bob1_00"
    },
    "palettes": {
      "01body": "skin_brown",
      "05shrt": "cloth_blue",
      "13hair": "hair_brown"
    }
  },
  "attributes": {
    "strength": 4,
    "agility": 5,
    "constitution": 6,
    "intelligence": 4,
    "charisma": 3,
    "wisdom": 3
  },
  "needs": {
    "energy": 100,
    "maxEnergy": 100,
    "hunger": 100,
    "maxHunger": 100,
    "health": 100,
    "maxHealth": 100,
    "mood": 78,
    "maxMood": 100
  },
  "skills": [
    {
      "skillId": "skill_farming",
      "level": 0,
      "experience": 0
    }
  ]
}
```

## Appearance Slots

Mana Seed Farmer Sprite System layers are stored by stable slot ID:

```text
00undr, 01body, 02sock, 03fot1, 04lwr1, 05shrt, 06lwr2, 07fot2,
08lwr3, 09hand, 10outr, 11neck, 12face, 13hair, 14head
```

For playable characters, these slots are required and cannot be omitted or set to `none`:

```text
01body, 03fot1, 04lwr1, 05shrt
```

Other slots can be omitted or set to `none`.

Each slot can also store a palette/tint ID in `appearance.palettes`. The current renderer uses layer tints for skin, hair, and clothing colors. The IDs are stored separately so the system can later move to exact Mana Seed palette swapping without changing preset files.

The player creator intentionally hides technical layers that are not useful as normal choices. `01body` is fixed to the human body for the MVP and is represented by the skin color control. `00undr` is reserved for future paired cloak/wing-style assets and is not exposed in the first UI pass.

The renderer applies the Mana Seed `_e` rule:

* Headwear ending in `_e` hides hair.
* Hair ending in `_e` hides itself when any headwear is equipped.

## Status Rules

Attributes use a balanced budget:

```text
Minimum per attribute: 1
Maximum per attribute: 10
Total budget: 30
```

The player-facing creator edits only the six core attributes with point buy. Needs and skill levels remain in the preset schema so future NPC authoring and data tools can use them, but the MVP player creator derives them from age/origin and does not expose them as editable fields.

## Identity Fields

Identity data is part of the preset so player characters and future NPC presets can share the same authoring format.

```text
name
familyName
ageCategoryId
originId
birthdaySeasonId
birthdayDay
motivationId
```

Birthdays currently use four season IDs and a day from `1` to `28`.

Motivations are stable IDs intended to become hooks for future traits, dialogue, events, and starting goals. Current MVP motivation IDs:

```text
fresh_start
family_legacy
honest_work
village_guardian
lost_knowledge
wealth_security
belonging
debt_escape
```

## Assets

The Mana Seed zip remains a local paid/private asset under `DuskVillage/Content/` and is intentionally ignored by Git. The code reads the zip at runtime when it is present and falls back to a simple placeholder preview when it is missing.

To enable the real paper-doll preview, download the Mana Seed Farmer Sprite System / Farmer Base pack from:

```text
https://seliel-the-shaper.itch.io/farmer-base
```

Place the downloaded zip at:

```text
DuskVillage/Content/22.10a - Mana Seed Farmer Sprite System v1.6.zip
```

Do not extract or commit the PNG files. The runtime catalog reads the zip directly and the repository intentionally keeps paid/licensed sprites out of Git.
