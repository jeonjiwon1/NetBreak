using UnityEngine;

public interface IFishSpecialAnimation
{
    bool HasAnimation { get; }
    float FramesPerSecond { get; }
    Sprite[] GetFrames(FishVisualSet set);
}
