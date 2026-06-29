# Dusk Village

Dusk Village is a 2D pixel-art dark fantasy generational life simulation sandbox RPG built in C# with MonoGame.

The project is about living a full life in a fragile frontier village: surviving hardship, building a home, forming relationships, raising a family, gaining influence, and leaving a legacy in a dangerous world. Adventure and combat exist, but they are optional paths rather than the only way to play.

## Status

Dusk Village is in early development. The repository currently contains the MonoGame project foundation and design documentation for the MVP, technical architecture, and long-term game systems.

## Core Pillars

- Freedom of life: play as a farmer, artisan, guard, hunter, merchant, scholar, occultist, family head, landowner, or another role the simulation supports.
- Generational simulation: characters age, form families, die, and leave behind property, reputation, debts, enemies, knowledge, and consequences.
- Survival in a dark world: food, energy, health, shelter, weather, safety, money, and social support should matter without becoming excessive micromanagement.
- Home and legacy: the player's home is a place of safety, status, comfort, production, family, storage, decoration, and inheritance.
- Living and tragic world: NPCs have routines, jobs, relationships, families, ambitions, conflicts, and risks.
- Data-driven future: content should be structured for future mod support and long-lived save files.

## MVP Focus

The first playable MVP is intended to prove the daily life simulation loop:

1. Create a human character.
2. Start in Dusk Village.
3. Live through a fast day/night cycle.
4. Manage basic needs.
5. Work, gather resources, earn money, and spend it.
6. Interact with NPCs and build relationships.
7. Improve or decorate a basic home.
8. Save and load persistent world progress.
9. Experience a small threat from the dark forest.

The MVP prioritizes life simulation, work, survival, home, relationships, and persistence before deeper adventure, politics, magic, multiplayer, or full modding.

## Tech Stack

- Language: C#
- Framework: MonoGame
- Target: PC, Windows
- Runtime target: .NET 8 (`net8.0-windows`)
- Content pipeline: MonoGame Content Builder
- Planned data format: JSON for gameplay definitions and save files

## Getting Started

From the repository root:

```powershell
dotnet restore
dotnet tool restore --tool-manifest .\DuskVillage\.config\dotnet-tools.json
dotnet build .\DuskVillage.sln
dotnet run --project .\DuskVillage\DuskVillage.csproj
```

The project currently uses a local MonoGame tool manifest in `DuskVillage/.config/dotnet-tools.json`.

## Assets and Content

The `DuskVillage/Content/Content.mgcb` file is tracked so the MonoGame content project structure is preserved.

Actual game assets under `DuskVillage/Content/` are intentionally ignored by Git. This protects paid, licensed, private, prototype, or placeholder assets that should not be redistributed through the public repository.

The character creator currently uses the Mana Seed Farmer Sprite System / Farmer Base asset pack. The repository does not include this paid/licensed asset. To see the real character sprites locally, download the pack from [Seliel the Shaper - Farmer Base](https://seliel-the-shaper.itch.io/farmer-base) and place the zip here:

```text
DuskVillage/Content/22.10a - Mana Seed Farmer Sprite System v1.6.zip
```

The game can still run without this zip. When it is missing, the character preview uses a simple fallback placeholder instead of the Mana Seed sprites.

## Documentation

- [Game Design Document](docs/GDD.md)
- [MVP Scope](docs/MVP_SCOPE.md)
- [Technical Design](docs/TECHNICAL_DESIGN.md)
- [Game Systems Roadmap](docs/GAME_SYSTEMS_ROADMAP.md)
- [Character Presets](docs/CHARACTER_PRESETS.md)
- [Character Animation](docs/CHARACTER_ANIMATION.md)
- [Actions](docs/ACTIONS.md)
- [Runtime State](docs/RUNTIME_STATE.md)
- [Needs Simulation](docs/NEEDS_SIMULATION.md)

## License

This repository is public but not open source. All rights are reserved.

You may view the repository through GitHub's public repository functionality, but no permission is granted to copy, redistribute, sublicense, sell, publish derivatives, reuse assets, reuse documentation, or use Dusk Village's project identity, lore, names, characters, or creative materials in another project.

See [LICENSE.md](LICENSE.md) for the full notice.
