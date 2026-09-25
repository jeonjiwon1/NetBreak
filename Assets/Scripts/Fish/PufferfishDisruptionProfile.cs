using UnityEngine;

[CreateAssetMenu(fileName = "PufferfishDisruption", menuName = "NetBreak/Pufferfish Disruption")]
public sealed class PufferfishDisruptionProfile : ScriptableObject, IFishSpecialAnimation
{
    [SerializeField] private Sprite[] horizontalFrames = new Sprite[4];
    [SerializeField] private Sprite[] verticalFrames = new Sprite[4];
    [SerializeField] private Sprite[] diagonalFrames = new Sprite[4];
    [SerializeField] private Sprite[] diagonalNorthWestFrames = new Sprite[4];
    [SerializeField] private Sprite[] impactFrames = new Sprite[4];
    [Min(0.1f)] [SerializeField] private float framesPerSecond = 12f;
    [Range(0f, 1f)] [SerializeField] private float soundVolume = 0.34f;
    [SerializeField] private AudioClip soundClip;

    public bool HasAnimation => HasFrames(horizontalFrames) && HasFrames(verticalFrames) &&
                                HasFrames(diagonalFrames) && HasFrames(diagonalNorthWestFrames);
    public bool HasImpact => HasFrames(impactFrames);
    public Sprite[] ImpactFrames => impactFrames;
    public float FramesPerSecond => Mathf.Max(0.1f, framesPerSecond);
    public float SoundVolume => Mathf.Clamp01(soundVolume);
    public AudioClip SoundClip => soundClip;

    public Sprite[] GetFrames(FishVisualSet set)
    {
        switch (set)
        {
            case FishVisualSet.Vertical: return verticalFrames;
            case FishVisualSet.Diagonal: return diagonalFrames;
            case FishVisualSet.DiagonalNorthWest: return diagonalNorthWestFrames;
            default: return horizontalFrames;
        }
    }

    private static bool HasFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length != 4) return false;
        for (int i = 0; i < frames.Length; i++)
            if (frames[i] == null) return false;
        return true;
    }
}
