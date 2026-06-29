using System;
using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.Animations;

public sealed class CharacterAnimationClip
{
    public CharacterAnimationClip(
        string animationId,
        CharacterFacingDirection facingDirection,
        IEnumerable<CharacterAnimationFrame> frames,
        bool isLooping)
    {
        if (string.IsNullOrWhiteSpace(animationId))
        {
            throw new ArgumentException("Animation ID is required.", nameof(animationId));
        }

        AnimationId = animationId;
        FacingDirection = facingDirection;
        Frames = frames?.ToArray() ?? throw new ArgumentNullException(nameof(frames));
        if (Frames.Count == 0)
        {
            throw new ArgumentException("Animation clip must have at least one frame.", nameof(frames));
        }

        IsLooping = isLooping;
        DurationMilliseconds = Frames.Sum(frame => frame.DurationMilliseconds);
    }

    public string AnimationId { get; }

    public CharacterFacingDirection FacingDirection { get; }

    public IReadOnlyList<CharacterAnimationFrame> Frames { get; }

    public bool IsLooping { get; }

    public int DurationMilliseconds { get; }

    public CharacterAnimationFrame GetFrameAt(int elapsedMilliseconds)
    {
        if (Frames.Count == 1)
        {
            return Frames[0];
        }

        var time = Math.Max(0, elapsedMilliseconds);
        if (IsLooping && DurationMilliseconds > 0)
        {
            time %= DurationMilliseconds;
        }

        if (!IsLooping && time >= DurationMilliseconds)
        {
            return Frames[^1];
        }

        var cursor = 0;
        foreach (var frame in Frames)
        {
            cursor += frame.DurationMilliseconds;
            if (time < cursor)
            {
                return frame;
            }
        }

        return Frames[^1];
    }
}
