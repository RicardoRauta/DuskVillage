using System;
using System.Collections.Generic;

namespace DuskVillage.Animations;

public static class CharacterAnimationCatalog
{
    public const int CellSize = 64;
    public const int CellsPerRow = 16;
    private const int HoldFrameMilliseconds = 600;

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

        AddFourWay(
            clips,
            CharacterAnimationIds.Idle,
            true,
            [Frame(0, 400)],
            [Frame(16, 400)],
            [Frame(32, 400)],
            [Frame(32, 400, flipX: true)]);

        AddFourWay(
            clips,
            CharacterAnimationIds.Walk,
            true,
            [Frame(48, 135), Frame(49, 135), Frame(50, 135), Frame(48, 135, flipX: true), Frame(49, 135, flipX: true), Frame(50, 135, flipX: true)],
            [Frame(52, 135), Frame(53, 135), Frame(54, 135), Frame(52, 135, flipX: true), Frame(53, 135, flipX: true), Frame(54, 135, flipX: true)],
            [Frame(64, 135), Frame(65, 135), Frame(66, 135), Frame(67, 135), Frame(68, 135), Frame(69, 135)],
            Mirrored(Frame(64, 135), Frame(65, 135), Frame(66, 135), Frame(67, 135), Frame(68, 135), Frame(69, 135)));

        AddFourWay(
            clips,
            CharacterAnimationIds.Run,
            true,
            [Frame(48, 80), Frame(49, 55), Frame(51, 115), Frame(48, 80, flipX: true), Frame(49, 55, flipX: true), Frame(51, 115, flipX: true)],
            [Frame(52, 80), Frame(53, 55), Frame(55, 115), Frame(52, 80, flipX: true), Frame(53, 55, flipX: true), Frame(55, 115, flipX: true)],
            [Frame(64, 80), Frame(65, 55), Frame(70, 115), Frame(67, 80), Frame(68, 55), Frame(71, 115)],
            Mirrored(Frame(64, 80), Frame(65, 55), Frame(70, 115), Frame(67, 80), Frame(68, 55), Frame(71, 115)));

        AddFourWay(
            clips,
            CharacterAnimationIds.Jump,
            false,
            [Frame(1, 200), Frame(2, 350), Frame(1, 300)],
            [Frame(17, 200), Frame(18, 350), Frame(17, 300)],
            [Frame(33, 200), Frame(34, 350), Frame(33, 300)],
            Mirrored(Frame(33, 200), Frame(34, 350), Frame(33, 300)));

        AddFourWay(
            clips,
            CharacterAnimationIds.WalkCarry,
            true,
            [Frame(80, 135), Frame(81, 135), Frame(82, 135), Frame(80, 135, flipX: true), Frame(81, 135, flipX: true), Frame(82, 135, flipX: true)],
            [Frame(84, 135), Frame(85, 135), Frame(86, 135), Frame(84, 135, flipX: true), Frame(85, 135, flipX: true), Frame(86, 135, flipX: true)],
            [Frame(96, 135), Frame(97, 135), Frame(98, 135), Frame(99, 135), Frame(100, 135), Frame(101, 135)],
            Mirrored(Frame(96, 135), Frame(97, 135), Frame(98, 135), Frame(99, 135), Frame(100, 135), Frame(101, 135)));

        AddFourWay(
            clips,
            CharacterAnimationIds.RunCarry,
            true,
            [Frame(80, 80), Frame(81, 55), Frame(83, 115), Frame(80, 80, flipX: true), Frame(81, 55, flipX: true), Frame(83, 115, flipX: true)],
            [Frame(84, 80), Frame(85, 55), Frame(87, 115), Frame(84, 80, flipX: true), Frame(85, 55, flipX: true), Frame(87, 115, flipX: true)],
            [Frame(96, 80), Frame(97, 55), Frame(102, 115), Frame(99, 80), Frame(100, 55), Frame(103, 115)],
            Mirrored(Frame(96, 80), Frame(97, 55), Frame(102, 115), Frame(99, 80), Frame(100, 55), Frame(103, 115)));

        AddFourWay(
            clips,
            CharacterAnimationIds.JumpCarry,
            false,
            [Frame(4, 200), Frame(5, 350), Frame(4, 300)],
            [Frame(20, 200), Frame(21, 350), Frame(20, 300)],
            [Frame(36, 200), Frame(37, 350), Frame(36, 300)],
            Mirrored(Frame(36, 200), Frame(37, 350), Frame(36, 300)));

        AddFourWay(
            clips,
            CharacterAnimationIds.Push,
            true,
            [Frame(8, 300), Frame(9, 300)],
            [Frame(24, 300), Frame(25, 300)],
            [Frame(40, 300), Frame(41, 300)],
            Mirrored(Frame(40, 300), Frame(41, 300)));

        AddFourWay(
            clips,
            CharacterAnimationIds.Pull,
            true,
            [Frame(10, 400), Frame(11, 400)],
            [Frame(26, 400), Frame(27, 400)],
            [Frame(42, 400), Frame(43, 400)],
            Mirrored(Frame(42, 400), Frame(43, 400)));

        AddFourWay(
            clips,
            CharacterAnimationIds.PickUpCarry,
            false,
            [Frame(130, 400), Frame(3, HoldFrameMilliseconds)],
            [Frame(146, 400), Frame(19, HoldFrameMilliseconds)],
            [Frame(162, 400), Frame(35, HoldFrameMilliseconds)],
            Mirrored(Frame(162, 400), Frame(35, HoldFrameMilliseconds)));

        AddFourWay(
            clips,
            CharacterAnimationIds.ThrowCarry,
            false,
            [Frame(6, 120), Frame(7, 400)],
            [Frame(22, 120), Frame(23, 400)],
            [Frame(38, 120), Frame(39, 400)],
            Mirrored(Frame(38, 120), Frame(39, 400)));

        AddFourWay(
            clips,
            CharacterAnimationIds.PlantSeeds,
            false,
            [Frame(131, 400), Frame(12, 100), Frame(12, 100), Frame(12, 100)],
            [Frame(147, 400), Frame(28, 100), Frame(28, 100), Frame(28, 100)],
            [Frame(163, 400), Frame(44, 100), Frame(44, 100), Frame(44, 100)],
            Mirrored(Frame(163, 400), Frame(44, 100), Frame(44, 100), Frame(44, 100)));

        AddFourWay(
            clips,
            CharacterAnimationIds.Water,
            false,
            [Frame(131, 350), Frame(133, 100), Frame(133, 100), Frame(133, 100)],
            [Frame(147, 350), Frame(149, 100), Frame(149, 100), Frame(149, 100)],
            [Frame(163, 350), Frame(165, 100), Frame(165, 100), Frame(165, 100)],
            Mirrored(Frame(163, 350), Frame(165, 100), Frame(165, 100), Frame(165, 100)));

        AddFourWay(
            clips,
            CharacterAnimationIds.WorkStation,
            true,
            [Frame(126, 200), Frame(126, 200, flipX: true)],
            [Frame(126, 200), Frame(126, 200, flipX: true)],
            [Frame(127, 200), Frame(127, 200, flipX: true)],
            [Frame(127, 200, flipX: true), Frame(127, 200)]);

        AddSameAllDirections(clips, CharacterAnimationIds.Wave, true, Frame(92, 200), Frame(93, 200));
        AddSameAllDirections(clips, CharacterAnimationIds.Hug, true, Frame(104, HoldFrameMilliseconds), Frame(105, HoldFrameMilliseconds));
        AddSameAllDirections(clips, CharacterAnimationIds.Sing, true, Frame(60, 180), Frame(61, 180));
        AddSameAllDirections(clips, CharacterAnimationIds.LuteGuitar, true, Frame(76, 180), Frame(77, 180));
        AddSameAllDirections(clips, CharacterAnimationIds.FluteOcarina, true, Frame(62, 180), Frame(63, 180));
        AddSameAllDirections(clips, CharacterAnimationIds.Drums, true, Frame(78, 180), Frame(79, 180));
        AddSameAllDirections(clips, CharacterAnimationIds.SitThrone, true, Frame(116, HoldFrameMilliseconds));
        AddSameAllDirections(clips, CharacterAnimationIds.LookAround, true, Frame(240, HoldFrameMilliseconds), Frame(241, HoldFrameMilliseconds));
        AddSameAllDirections(clips, CharacterAnimationIds.SitLedge, true, Frame(120, HoldFrameMilliseconds), Frame(121, 200), Frame(122, 200));
        AddFourWay(
            clips,
            CharacterAnimationIds.SitChair,
            true,
            [Frame(195, HoldFrameMilliseconds)],
            [Frame(211, HoldFrameMilliseconds)],
            [Frame(227, HoldFrameMilliseconds)],
            [Frame(227, HoldFrameMilliseconds, flipX: true)]);
        AddFourWay(
            clips,
            CharacterAnimationIds.Meditate,
            true,
            [Frame(15, HoldFrameMilliseconds)],
            [Frame(31, HoldFrameMilliseconds)],
            [Frame(47, HoldFrameMilliseconds)],
            [Frame(47, HoldFrameMilliseconds, flipX: true)]);
        AddFourWay(
            clips,
            CharacterAnimationIds.Sleep,
            true,
            [Frame(199, HoldFrameMilliseconds)],
            [Frame(215, HoldFrameMilliseconds)],
            [Frame(231, HoldFrameMilliseconds)],
            [Frame(231, HoldFrameMilliseconds, flipX: true)]);
        AddFourWay(
            clips,
            CharacterAnimationIds.SleepSit,
            true,
            [Frame(196, HoldFrameMilliseconds)],
            [Frame(212, HoldFrameMilliseconds)],
            [Frame(228, HoldFrameMilliseconds)],
            [Frame(228, HoldFrameMilliseconds, flipX: true)]);
        AddSameAllDirections(clips, CharacterAnimationIds.ThumbsUp, true, Frame(243, HoldFrameMilliseconds));
        AddSameAllDirections(clips, CharacterAnimationIds.MadStomp, true, Frame(245, 160), Frame(245, 160, flipX: true));
        AddSameAllDirections(clips, CharacterAnimationIds.Shocked, true, Frame(244, HoldFrameMilliseconds));
        AddSameAllDirections(clips, CharacterAnimationIds.Laugh, true, Frame(246, 160), Frame(247, 160));
        AddSameAllDirections(clips, CharacterAnimationIds.DrinkStanding, true, Frame(112, HoldFrameMilliseconds), Frame(113, 800), Frame(114, HoldFrameMilliseconds), Frame(115, 800));
        AddSameAllDirections(clips, CharacterAnimationIds.SitFloor, true, Frame(197, HoldFrameMilliseconds), Frame(198, HoldFrameMilliseconds));
        AddSameAllDirections(clips, CharacterAnimationIds.Impatient, true, Frame(193, 300), Frame(194, 300));

        return clips;
    }

    private static void AddFourWay(
        Dictionary<(string AnimationId, CharacterFacingDirection Facing), CharacterAnimationClip> clips,
        string animationId,
        bool isLooping,
        CharacterAnimationFrame[] downFrames,
        CharacterAnimationFrame[] upFrames,
        CharacterAnimationFrame[] rightFrames,
        CharacterAnimationFrame[] leftFrames)
    {
        Add(clips, animationId, CharacterFacingDirection.Down, isLooping, downFrames);
        Add(clips, animationId, CharacterFacingDirection.Up, isLooping, upFrames);
        Add(clips, animationId, CharacterFacingDirection.Right, isLooping, rightFrames);
        Add(clips, animationId, CharacterFacingDirection.Left, isLooping, leftFrames);
    }

    private static void AddSameAllDirections(
        Dictionary<(string AnimationId, CharacterFacingDirection Facing), CharacterAnimationClip> clips,
        string animationId,
        bool isLooping,
        params CharacterAnimationFrame[] frames)
    {
        AddFourWay(clips, animationId, isLooping, frames, frames, frames, frames);
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

    private static CharacterAnimationFrame[] Mirrored(params CharacterAnimationFrame[] frames)
    {
        var mirrored = new CharacterAnimationFrame[frames.Length];
        for (var i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            mirrored[i] = Frame(frame.CellIndex, frame.DurationMilliseconds, !frame.FlipX);
        }

        return mirrored;
    }

    private static CharacterAnimationFrame Frame(int cellIndex, int durationMilliseconds, bool flipX = false)
    {
        return new CharacterAnimationFrame(cellIndex, durationMilliseconds, flipX);
    }
}
