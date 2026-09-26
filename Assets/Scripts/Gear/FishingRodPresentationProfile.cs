using UnityEngine;

[CreateAssetMenu(fileName = "FishingRodPresentation", menuName = "NetBreak/Fishing Rod Presentation")]
public sealed class FishingRodPresentationProfile : ScriptableObject
{
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] attackFrames = new Sprite[3];
    [SerializeField] private Sprite[] hitFrames = new Sprite[4];
    [SerializeField] private AudioClip hitClip;
    [Min(0.01f)] [SerializeField] private float attackDuration = 0.27f;
    [Min(0.01f)] [SerializeField] private float lineDuration = 0.18f;
    [Min(0.01f)] [SerializeField] private float hitDuration = 0.22f;
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 0.28f;

    public Sprite IdleSprite => idleSprite;
    public Sprite[] AttackFrames => attackFrames;
    public Sprite[] HitFrames => hitFrames;
    public AudioClip HitClip => hitClip;
    public float AttackDuration => attackDuration;
    public float LineDuration => lineDuration;
    public float HitDuration => hitDuration;
    public float HitVolume => hitVolume;
    public bool HasAttack => HasFrames(attackFrames);
    public bool HasHit => HasFrames(hitFrames);

    private static bool HasFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return false;
        foreach (Sprite frame in frames)
            if (frame == null) return false;
        return true;
    }
}
