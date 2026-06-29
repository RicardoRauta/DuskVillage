namespace DuskVillage.Animations;

public sealed class CharacterAnimationState
{
    public string AnimationId { get; set; } = CharacterAnimationIds.Idle;

    public CharacterFacingDirection FacingDirection { get; set; } = CharacterFacingDirection.Down;

    public int ElapsedMilliseconds { get; set; }

    public CharacterAnimationState Clone()
    {
        return new CharacterAnimationState
        {
            AnimationId = AnimationId,
            FacingDirection = FacingDirection,
            ElapsedMilliseconds = ElapsedMilliseconds
        };
    }
}
