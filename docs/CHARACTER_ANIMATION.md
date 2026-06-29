# Dusk Village — Character Animation

## Purpose

Character animation is split from character presets, save data, input, and rendering.

The `DuskVillage.Animations` module stores pure animation data:

```text
animation id
facing direction
frame cell index
frame duration in milliseconds
flipX flag
```

Renderers consume the current animation frame, but they do not decide how time advances or which animation should play.

## Mana Seed Cell Layout

The Mana Seed Farmer Base sheets are read from the local zip in `DuskVillage/Content`. The sheets are not committed to Git.

Current assumptions from the included animation guide:

```text
sheet size: 1024x1024
cell size: 64x64
grid: 16 columns x 16 rows
cell index: row * 16 + column
```

The purple number in the guide is the cell index. The yellow number is the suggested frame duration in milliseconds. The reverse marker means the same cell should be drawn with `flipX`.

Example down-walk sequence:

```text
048 105
049 105
050 105
048 105 flipX
049 105 flipX
050 105 flipX
```

## MVP Clips

Implemented clips:

```text
idle: down, up, right, left
walk: down, up, right, left
```

The gameplay placeholder uses this only as an animation preview. Pressing movement input changes the player preview between idle and walk, but map movement, collision, camera, and tile interaction belong to later modules.

The gameplay placeholder also exposes an animation preview screen. It is a temporary testing surface for this module and lets the developer choose clip, direction, playback, reset, and inspect the current cell, frame duration, `flipX`, and timeline position.

## Rendering

`CharacterSpriteRenderer` draws the active animation cell for every selected appearance layer in paper-doll order.

Texture loading and recoloring are centralized in `ManaSeedCharacterTextureProvider`, so portrait rendering and sprite rendering share the same cache. If the paid zip is missing, the fallback renderer keeps the game usable.

## Git and Assets

The Mana Seed zip remains local/private and ignored by Git. To enable real animated sprites, download the Farmer Base pack from:

```text
https://seliel-the-shaper.itch.io/farmer-base
```

Place the zip at:

```text
DuskVillage/Content/22.10a - Mana Seed Farmer Sprite System v1.6.zip
```
