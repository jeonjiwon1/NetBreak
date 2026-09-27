using UnityEngine;

[CreateAssetMenu(fileName = "CastNetPresentation", menuName = "NetBreak/Cast Net Presentation")]
public sealed class CastNetPresentationProfile : ScriptableObject
{
    [SerializeField] private Sprite foldedSprite;
    [SerializeField] private Sprite[] openingFrames = new Sprite[5];
    [SerializeField] private Sprite[] areaFrames = new Sprite[4];
    [SerializeField] private Sprite[] hitFrames = new Sprite[4];
    [SerializeField] private AudioClip castClip;
    [Min(.01f)] [SerializeField] private float openingDuration = .42f;
    [Min(.01f)] [SerializeField] private float areaDuration = .28f;
    [Min(.01f)] [SerializeField] private float hitDuration = .18f;
    [Range(0f, 1f)] [SerializeField] private float castVolume = .3f;

    public Sprite FoldedSprite => foldedSprite;
    public Sprite[] OpeningFrames => openingFrames;
    public Sprite[] AreaFrames => areaFrames;
    public Sprite[] HitFrames => hitFrames;
    public AudioClip CastClip => castClip;
    public float OpeningDuration => openingDuration;
    public float AreaDuration => areaDuration;
    public float HitDuration => hitDuration;
    public float CastVolume => castVolume;
    public bool HasOpening => HasFrames(openingFrames);
    public bool HasArea => HasFrames(areaFrames);
    public bool HasHit => HasFrames(hitFrames);

    private static bool HasFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return false;
        foreach (Sprite frame in frames)
            if (frame == null) return false;
        return true;
    }
}
