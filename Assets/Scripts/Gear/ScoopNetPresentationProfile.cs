using UnityEngine;

[CreateAssetMenu(fileName = "ScoopNetPresentation", menuName = "NetBreak/Scoop Net Presentation")]
public sealed class ScoopNetPresentationProfile : ScriptableObject
{
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite[] swingFrames = new Sprite[5];
    [SerializeField] private Sprite[] hitFrames = new Sprite[4];
    [SerializeField] private AudioClip swingClip;
    [Min(.01f)] [SerializeField] private float swingDuration = .26f;
    [Min(.01f)] [SerializeField] private float hitDuration = .2f;
    [Range(0f, 1f)] [SerializeField] private float swingVolume = .27f;

    public Sprite ReadySprite => readySprite;
    public Sprite[] SwingFrames => swingFrames;
    public Sprite[] HitFrames => hitFrames;
    public AudioClip SwingClip => swingClip;
    public float SwingDuration => swingDuration;
    public float HitDuration => hitDuration;
    public float SwingVolume => swingVolume;
    public bool HasSwing => HasFrames(swingFrames);
    public bool HasHit => HasFrames(hitFrames);

    private static bool HasFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return false;
        foreach (Sprite frame in frames)
            if (frame == null) return false;
        return true;
    }
}
