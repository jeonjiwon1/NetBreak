using UnityEngine;

// The gameplay controller owns targeting and damage. This component only displays a confirmed hit.
public sealed class FishingRodPresentation : MonoBehaviour
{
    [SerializeField] private FishingRodPresentationProfile profile;

    private SpriteRenderer body;
    private LineRenderer line;
    private float attackRemaining;
    private float lineRemaining;

    public FishingRodPresentationProfile Profile => profile;
    public bool IsAttacking => attackRemaining > 0f;
    public bool IsLineVisible => line != null && line.enabled;

    private void Awake()
    {
        if (profile == null)
            profile = Resources.Load<FishingRodPresentationProfile>("FishingRodPresentation");
        body = GetComponent<SpriteRenderer>();
        line = GetComponent<LineRenderer>();
        ResetVisual();
    }

    private void OnEnable()
    {
        if (profile == null)
            profile = Resources.Load<FishingRodPresentationProfile>("FishingRodPresentation");
        ResetVisual();
    }
    private void OnDisable() => ResetVisual();

    private void Update()
    {
        if (Time.deltaTime <= 0f) return;
        if (attackRemaining > 0f)
        {
            attackRemaining = Mathf.Max(0f, attackRemaining - Time.deltaTime);
            if (body != null && profile != null && profile.HasAttack)
            {
                if (attackRemaining <= 0f) body.sprite = profile.IdleSprite;
                else
                {
                    float progress = 1f - attackRemaining / profile.AttackDuration;
                    int frame = Mathf.Min(profile.AttackFrames.Length - 1,
                        Mathf.FloorToInt(progress * profile.AttackFrames.Length));
                    body.sprite = profile.AttackFrames[frame];
                }
            }
        }

        if (lineRemaining > 0f)
        {
            lineRemaining = Mathf.Max(0f, lineRemaining - Time.deltaTime);
            if (lineRemaining <= 0f && line != null) line.enabled = false;
        }
    }

    public void ShowHit(Vector2 targetPosition, bool showLine)
    {
        if (profile == null || Time.timeScale <= 0f) return;
        if (profile.HasAttack && body != null)
        {
            attackRemaining = profile.AttackDuration;
            body.sprite = profile.AttackFrames[0];
        }

        if (showLine && line != null)
        {
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = 0.015f;
            line.startColor = line.endColor = new Color(0.94f, 0.98f, 0.87f, 0.9f);
            line.SetPosition(0, transform.position + new Vector3(0.07f, 0.11f));
            line.SetPosition(1, targetPosition);
            line.enabled = true;
            lineRemaining = profile.LineDuration;
        }

        ItemEffectManager.Instance?.ShowFishingRodHit(targetPosition, profile);
        ItemEffectManager.Instance?.PlayFishingRodHitSound(profile);
    }

    public void ResetVisual()
    {
        attackRemaining = 0f;
        lineRemaining = 0f;
        if (line == null) line = GetComponent<LineRenderer>();
        if (line != null) line.enabled = false;
        if (body == null) body = GetComponent<SpriteRenderer>();
        if (body != null && profile != null && profile.IdleSprite != null)
            body.sprite = profile.IdleSprite;
    }
}
