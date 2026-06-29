using System;
using System.Collections.Generic;

namespace DuskVillage.Animations;

public static class CharacterAnimationCatalog
{
    public const int CellSize = 64;
    public const int CellsPerRow = 16;

    private static readonly Dictionary<(string AnimationId, CharacterFacingDirection Facing), CharacterAnimationClip> Clips = CreateClips();

    public static CharacterAnimationClip GetClip(string animationId, CharacterFacingDirection facingDirection)
    {
        if (Clips.TryGetValue((animationId, facingDirection), out var clip))
        {
            return clip;
        }

        throw new InvalidOperationException($"Missing character animation clip '{animationId}' for '{facingDirection}'.");
    }

    public static int CellColumn(int cellIndex)
    {
        return cellIndex % CellsPerRow;
    }

    public static int CellRow(int cellIndex)
    {
        return cellIndex / CellsPerRow;
    }

    private static Dictionary<(string AnimationId, CharacterFacingDirection Facing), CharacterAnimationClip> CreateClips()
    {
        var clips = new Dictionary<(string AnimationId, CharacterFacingDirection Facing), CharacterAnimationClip>();

        Add(clips, CharacterAnimationIds.Idle, CharacterFacingDirection.Down, true, Frame(0, 400));
        Add(clips, CharacterAnimationIds.Idle, CharacterFacingDirection.Up, true, Frame(16, 400));
        Add(clips, CharacterAnimationIds.Idle, CharacterFacingDirection.Right, true, Frame(32, 400));
        Add(clips, CharacterAnimationIds.Idle, CharacterFacingDirection.Left, true, Frame(32, 400, flipX: true));

        Add(
            clips,
            CharacterAnimationIds.Walk,
            CharacterFacingDirection.Down,
            true,
            Frame(48, 80),
            Frame(49, 55),
            Frame(51, 115),
            Frame(48, 80, flipX: true),
            Frame(49, 55, flipX: true),
            Frame(51, 115, flipX: true));

        Add(
            clips,
            CharacterAnimationIds.Walk,
            CharacterFacingDirection.Up,
            true,
            Frame(52, 80),
            Frame(53, 55),
            Frame(54, 115),
            Frame(52, 80, flipX: true),
            Frame(53, 55, flipX: true),
            Frame(54, 115, flipX: true));

        Add(
            clips,
            CharacterAnimationIds.Walk,
            CharacterFacingDirection.Right,
            true,
            Frame(64, 145),
            Frame(65, 55),
            Frame(66, 115),
            Frame(67, 165),
            Frame(68, 55),
            Frame(69, 115));

        Add(
            clips,
            CharacterAnimationIds.Walk,
            CharacterFacingDirection.Left,
            true,
            Frame(64, 145, flipX: true),
            Frame(65, 55, flipX: true),
            Frame(66, 115, flipX: true),
            Frame(67, 165, flipX: true),
            Frame(68, 55, flipX: true),
            Frame(69, 115, flipX: true));

        return clips;
    }

    private static void Add(
        Dictionary<(string AnimationId, CharacterFacingDirection Facing), CharacterAnimationClip> clips,
        string animationId,
        CharacterFacingDirection facingDirection,
        bool isLooping,
        params CharacterAnimationFrame[] frames)
    {
        clips[(animationId, facingDirection)] = new CharacterAnimationClip(animationId, facingDirection, frames, isLooping);
    }

    private static CharacterAnimationFrame Frame(int cellIndex, int durationMilliseconds, bool flipX = false)
    {
        return new CharacterAnimationFrame(cellIndex, durationMilliseconds, flipX);
    }
}
