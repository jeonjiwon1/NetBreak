using System.Collections.Generic;
using UnityEngine;

// Receives authoritative aim, cast radius and damage positions from CastNetController.
// It never searches for fish or starts gameplay damage.
public sealed class CastNetPresentation : MonoBehaviour
{
    [SerializeField] private CastNetPresentationProfile profile;

    private SpriteRenderer previewArea;
    private Color previewAreaOriginalColor;
    private SpriteRenderer ghost;
    private SpriteRenderer opening;
    private float openingRemaining;
    private float impactDelayRemaining;
    private bool impactPending;
    private Vector2 impactPosition;
    private float impactRadius;
    private readonly List<Vector2> impactHits = new();
    private bool aiming;
    private Vector2 aimPosition;
    private float aimRadius;

    public CastNetPresentationProfile Profile => profile;
    public bool IsAiming => aiming;
    public bool IsOpening => openingRemaining > 0f;
    public Vector2 AimPosition => aimPosition;
    public float AimRadius => aimRadius;

    private void Awake()
    {
        if (profile == null)
            profile = Resources.Load<CastNetPresentationProfile>("CastNetPresentation");
    }

    public void Bind(Transform existingArea)
    {
        if (profile == null)
            profile = Resources.Load<CastNetPresentationProfile>("CastNetPresentation");
        previewArea = existingArea != null ? existingArea.GetComponent<SpriteRenderer>() : null;
        if (previewArea != null) previewAreaOriginalColor = previewArea.color;
        ghost = EnsureRenderer("CastNetGhost", ghost, 30);
        opening = EnsureRenderer("CastNetOpening", opening, 31);
        ResetVisual();
    }

    private SpriteRenderer EnsureRenderer(string name, SpriteRenderer current, int order)
    {
        if (current != null) return current;
        Transform child = transform.Find(name);
        GameObject visual = child != null ? child.gameObject : new GameObject(name);
        visual.transform.SetParent(transform, false);
        current = visual.GetComponent<SpriteRenderer>();
        if (current == null) current = visual.AddComponent<SpriteRenderer>();
        current.sortingOrder = order;
        return current;
    }

    public void SetAim(Vector2 position, float radius)
    {
        aiming = true;
        aimPosition = position;
        aimRadius = radius;
        if (ghost != null)
        {
            ghost.transform.position = position;
            ghost.sprite = profile != null ? profile.FoldedSprite : null;
            ghost.color = new Color(1f, 1f, 1f, .7f);
            ghost.enabled = ghost.sprite != null;
        }
        // The existing scene circle is the area indicator. Its transform is
        // sized by the controller from the same radius used for overlap damage.
        if (previewArea != null)
        {
            Color color = previewAreaOriginalColor;
            color.a = Mathf.Max(color.a, .12f);
            previewArea.color = color;
            previewArea.enabled = true;
        }
    }

    public void EndAim()
    {
        aiming = false;
        aimRadius = 0f;
        if (ghost != null) ghost.enabled = false;
        if (previewArea != null)
        {
            previewArea.enabled = false;
            previewArea.color = previewAreaOriginalColor;
        }
    }

    public void ShowCast(Vector2 position, float radius, IReadOnlyList<Vector2> hitPositions)
    {
        EndAim();
        if (profile == null || Time.timeScale <= 0f)
        {
            ResetVisual();
            return;
        }
        if (profile.HasOpening && opening != null)
        {
            opening.transform.position = position;
            opening.transform.localScale = Vector3.one * (radius * 2f);
            opening.sprite = profile.OpeningFrames[0];
            opening.enabled = true;
            openingRemaining = profile.OpeningDuration;
        }

        impactPosition = position;
        impactRadius = radius;
        impactHits.Clear();
        if (hitPositions != null)
            for (int i = 0; i < hitPositions.Count; i++)
                impactHits.Add(hitPositions[i]);
        impactPending = true;
        impactDelayRemaining = profile.HasOpening ? profile.OpeningDuration * .6f : 0f;
        if (impactDelayRemaining <= 0f) EmitImpact();
        ItemEffectManager.Instance?.PlayCastNetSound(profile);
    }

    private void Update()
    {
        if (profile == null || Time.timeScale <= 0f ||
            ToolSlotInput.IsSelectionOrEndBlocked)
        {
            ResetVisual();
            return;
        }
        Tick(Time.deltaTime);
    }

    public void Tick(float scaledDeltaTime)
    {
        if (scaledDeltaTime <= 0f) return;
        if (impactPending)
        {
            impactDelayRemaining -= scaledDeltaTime;
            if (impactDelayRemaining <= 0f) EmitImpact();
        }
        if (openingRemaining > 0f && profile != null && profile.HasOpening)
        {
            openingRemaining = Mathf.Max(0f, openingRemaining - scaledDeltaTime);
            if (openingRemaining <= 0f) opening.enabled = false;
            else
            {
                float progress = 1f - openingRemaining / profile.OpeningDuration;
                int frame = Mathf.Min(profile.OpeningFrames.Length - 1,
                    Mathf.FloorToInt(progress * profile.OpeningFrames.Length));
                opening.sprite = profile.OpeningFrames[frame];
            }
        }
    }

    private void EmitImpact()
    {
        impactPending = false;
        ItemEffectManager effects = ItemEffectManager.Instance;
        effects?.ShowCastNetArea(impactPosition, impactRadius, profile);
        for (int i = 0; i < impactHits.Count; i++)
            effects?.ShowCastNetHit(impactHits[i], profile);
        impactHits.Clear();
    }

    public void ResetVisual()
    {
        EndAim();
        openingRemaining = 0f;
        impactPending = false;
        impactDelayRemaining = 0f;
        impactHits.Clear();
        if (opening != null) opening.enabled = false;
    }

    private void OnDisable() => ResetVisual();
}
