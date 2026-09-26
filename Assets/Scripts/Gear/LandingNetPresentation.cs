using System.Collections.Generic;
using UnityEngine;

public readonly struct ScoopNetHit
{
    public FishController Target { get; }
    public Vector2 Position { get; }

    public ScoopNetHit(FishController target, Vector2 position)
    {
        Target = target;
        Position = position;
    }
}

// The controller provides its actual cursor position and confirmed damage positions.
// This component never performs targeting or changes gameplay timing.
public sealed class LandingNetPresentation : MonoBehaviour
{
    [SerializeField] private ScoopNetPresentationProfile profile;

    private LandingNetController controller;
    private SpriteRenderer rangeRenderer;
    private SpriteRenderer scoopRenderer;
    private float swingRemaining;
    private bool aimAvailable;
    private Vector2 aimPosition;

    public ScoopNetPresentationProfile Profile => profile;
    public bool IsSwinging => swingRemaining > 0f;
    public Vector2 AimPosition => aimPosition;

    private void Awake()
    {
        if (profile == null)
            profile = Resources.Load<ScoopNetPresentationProfile>("ScoopNetPresentation");
    }

    public void Bind(LandingNetController owner, Transform rangeVisual)
    {
        controller = owner;
        if (profile == null)
            profile = Resources.Load<ScoopNetPresentationProfile>("ScoopNetPresentation");
        rangeRenderer = rangeVisual != null ? rangeVisual.GetComponent<SpriteRenderer>() : null;
        EnsureScoopRenderer();
        ResetVisual();
    }

    private void EnsureScoopRenderer()
    {
        if (scoopRenderer != null) return;
        Transform existing = transform.Find("ScoopNetVisual");
        GameObject visual;
        if (existing != null) visual = existing.gameObject;
        else
        {
            visual = new GameObject("ScoopNetVisual");
            visual.transform.SetParent(transform, false);
        }
        scoopRenderer = visual.GetComponent<SpriteRenderer>();
        if (scoopRenderer == null) scoopRenderer = visual.AddComponent<SpriteRenderer>();
        scoopRenderer.sortingOrder = 31;
    }

    public void SetAimPosition(Vector2 position)
    {
        aimPosition = position;
        if (scoopRenderer != null && !IsSwinging)
            scoopRenderer.transform.position = position;
    }

    public void SetAimAvailable(bool available)
    {
        aimAvailable = available;
        if (!available)
        {
            swingRemaining = 0f;
            if (rangeRenderer != null) rangeRenderer.enabled = false;
            if (scoopRenderer != null) scoopRenderer.enabled = false;
            return;
        }

        if (rangeRenderer != null)
        {
            rangeRenderer.enabled = true;
            // Keep the existing scene circle and controller's authoritative size.
            Color color = rangeRenderer.color;
            color.a = controller != null && controller.RemainingCooldown > 0f ? .045f : .11f;
            rangeRenderer.color = color;
        }
        if (scoopRenderer != null)
        {
            scoopRenderer.enabled = profile != null &&
                (IsSwinging ? profile.HasSwing : profile.ReadySprite != null);
            if (!IsSwinging) scoopRenderer.sprite = profile != null ? profile.ReadySprite : null;
        }
    }

    public void ShowUse(Vector2 attackPosition, IReadOnlyList<ScoopNetHit> hits)
    {
        // Called once, after the controller has completed all gameplay damage.
        if (profile == null) return;
        EnsureScoopRenderer();
        scoopRenderer.transform.position = attackPosition;
        if (profile.HasSwing)
        {
            swingRemaining = profile.SwingDuration;
            scoopRenderer.sprite = profile.SwingFrames[0];
            scoopRenderer.enabled = true;
        }

        if (hits != null)
            for (int i = 0; i < hits.Count; i++)
                ItemEffectManager.Instance?.ShowScoopNetHit(hits[i].Position, profile);

        ItemEffectManager.Instance?.PlayScoopNetSwingSound(profile);
    }

    private void Update() => Tick(Time.deltaTime);

    public void Tick(float scaledDeltaTime)
    {
        if (swingRemaining <= 0f || profile == null || !profile.HasSwing ||
            scaledDeltaTime <= 0f) return;
        swingRemaining = Mathf.Max(0f, swingRemaining - scaledDeltaTime);
        if (swingRemaining <= 0f)
        {
            scoopRenderer.sprite = profile.ReadySprite;
            scoopRenderer.enabled = aimAvailable && profile.ReadySprite != null;
            scoopRenderer.transform.position = aimPosition;
            return;
        }
        float progress = 1f - swingRemaining / profile.SwingDuration;
        int frame = Mathf.Min(profile.SwingFrames.Length - 1,
            Mathf.FloorToInt(progress * profile.SwingFrames.Length));
        scoopRenderer.sprite = profile.SwingFrames[frame];
    }

    public void ResetVisual()
    {
        swingRemaining = 0f;
        aimAvailable = false;
        if (rangeRenderer != null) rangeRenderer.enabled = false;
        if (scoopRenderer != null)
        {
            scoopRenderer.sprite = profile != null ? profile.ReadySprite : null;
            scoopRenderer.enabled = false;
        }
    }

    private void OnDisable() => ResetVisual();
}
