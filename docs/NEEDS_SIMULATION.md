# Dusk Village - Needs Simulation

## Purpose

The needs system updates mutable runtime needs without changing character presets.

The module lives in `DuskVillage.Needs` and is intentionally pure: it receives a `CharacterNeedsBlock`, returns a new result, and does not know about screens, saves, input, or the world clock.

## Current MVP Rules

Elapsed time currently applies small hourly decay:

```text
Hunger: -3 per hour
Energy: -2 per hour
```

If hunger reaches the low threshold, mood decreases while time passes. If hunger reaches zero, health decreases. If energy reaches zero, health also decreases while the player continues pushing.

Sleep currently:

```text
Energy: restored to max
Hunger: -8
Mood: +6
Health: +4 if hunger is above the low hunger threshold
```

These numbers are intentionally simple and should remain easy to tune.

## Integration

`GameplayPlaceholderScreen` uses `NeedsSystem` only as a temporary test bench:

```text
Advance 1h -> WorldClock.Advance + NeedsSystem.ApplyElapsedTime
Sleep      -> WorldClock.SleepToNextDay + NeedsSystem.ApplySleep
```

Future action systems should call `NeedsSystem` or related pure services from their own gameplay layer instead of putting costs directly in UI code.
