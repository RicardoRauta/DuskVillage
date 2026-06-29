using System;

namespace DuskVillage.Animations;

public static class CharacterAnimationSystem
{
    public static void SetMotion(CharacterAnimationState state, string animationId, CharacterFacingDirection facingDirection)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.AnimationId == animationId && state.FacingDirection == facingDirection)
        {
            return;
        }

        CharacterAnimationCatalog.GetClip(animationId, facingDirection);
        state.AnimationId = animationId;
        state.FacingDirection = facingDirection;
        state.ElapsedMilliseconds = 0;
    }

    public static void Advance(CharacterAnimationState state, TimeSpan elapsedTime)
    {
        ArgumentNullException.ThrowIfNull(state);

        var clip = CharacterAnimationCatalog.GetClip(state.AnimationId, state.FacingDirection);
        var elapsedMilliseconds = Math.Max(0, (int)Math.Round(elapsedTime.TotalMilliseconds));
        state.ElapsedMilliseconds += elapsedMilliseconds;

        if (clip.IsLooping && clip.DurationMilliseconds > 0)
        {
            state.ElapsedMilliseconds %= clip.DurationMilliseconds;
        }
        else if (state.ElapsedMilliseconds > clip.DurationMilliseconds)
        {
            state.ElapsedMilliseconds = clip.DurationMilliseconds;
        }
    }

    public static CharacterAnimationFrame GetCurrentFrame(CharacterAnimationState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var clip = CharacterAnimationCatalog.GetClip(state.AnimationId, state.FacingDirection);
        return clip.GetFrameAt(state.ElapsedMilliseconds);
    }
}
