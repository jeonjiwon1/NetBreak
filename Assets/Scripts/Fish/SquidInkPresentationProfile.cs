using UnityEngine;

[CreateAssetMenu(fileName = "SquidInkPresentation", menuName = "NetBreak/Squid Ink Presentation")]
public sealed class SquidInkPresentationProfile : ScriptableObject
{
    [SerializeField] private Sprite[] horizontalFrames = new Sprite[4];
    [SerializeField] private Sprite[] verticalFrames = new Sprite[4];
    [SerializeField] private Sprite[] diagonalFrames = new Sprite[4];
    [SerializeField] private Sprite[] diagonalNorthWestFrames = new Sprite[4];
    [SerializeField] private Sprite[] puffFrames = new Sprite[4];
    [SerializeField] private Sprite[] projectileFrames = new Sprite[4];
    [SerializeField] private Sprite[] impactFrames = new Sprite[4];
    [Min(0.1f)] [SerializeField] private float framesPerSecond = 8f;
    [Range(0f, 1f)] [SerializeField] private float inkVolume = 0.38f;
    [SerializeField] private AudioClip inkClip;

    public bool HasAnimation => HasFrames(horizontalFrames) && HasFrames(verticalFrames) &&
                                HasFrames(diagonalFrames) && HasFrames(diagonalNorthWestFrames);
    public bool HasPuff => HasFrames(puffFrames);
    public bool HasProjectile => HasFrames(projectileFrames);
    public bool HasImpact => HasFrames(impactFrames);
    public Sprite[] PuffFrames => puffFrames;
    public Sprite[] ProjectileFrames => projectileFrames;
    public Sprite[] ImpactFrames => impactFrames;
    public float FramesPerSecond => Mathf.Max(0.1f, framesPerSecond);
    public float InkVolume => Mathf.Clamp01(inkVolume);
    public AudioClip InkClip => inkClip;

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
