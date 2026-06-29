using System;

namespace DuskVillage.Animations;

public sealed class CharacterAnimationFrame
{
    public CharacterAnimationFrame(int cellIndex, int durationMilliseconds, bool flipX)
    {
        if (cellIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellIndex), "Cell index must be zero or greater.");
        }

        if (durationMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationMilliseconds), "Frame duration must be greater than zero.");
        }

        CellIndex = cellIndex;
        DurationMilliseconds = durationMilliseconds;
        FlipX = flipX;
    }

    public int CellIndex { get; }

    public int DurationMilliseconds { get; }

    public bool FlipX { get; }
}
