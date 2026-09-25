using System;
using UnityEngine;

// The root renderer and collider remain the existing prototype/gameplay representation.
public sealed class FishVisualController : MonoBehaviour
{
    private SpriteRenderer prototypeRenderer;
    private SpriteRenderer customRenderer;
    private FishMovement movement;
    private FishVisualProfile profile;
    private FishVisualHeading heading;
    private FishVisualSet currentSet;
    private int frameIndex;
    private float frameTimer;
    private IFishSpecialAnimation specialProfile;
    private Action specialRelease;
    private FishVisualDirection lockedDirection;
    private int specialFrame;
    private float specialTimer;

    public bool UsesCustomVisual => profile != null;
    public int FrameIndex => frameIndex;
    public bool IsPlayingSpecial => specialProfile != null;
    public int SpecialFrame => specialFrame;
    public FishVisualHeading SpecialHeading => lockedDirection.Heading;

    public void Initialize(FishData data, SpriteRenderer rootRenderer)
    {
        prototypeRenderer = rootRenderer;
        movement ??= GetComponent<FishMovement>();
        profile = data != null && data.VisualProfile != null && data.VisualProfile.IsValid
            ? data.VisualProfile : null;
        heading = FishVisualHeading.East;
        currentSet = FishVisualSet.Horizontal;
        frameIndex = 0;
        frameTimer = 0f;
        ResetSpecial();

        if (prototypeRenderer == null) return;

        prototypeRenderer.flipX = false;
        prototypeRenderer.flipY = false;
        prototypeRenderer.enabled = profile == null;

        if (profile == null)
        {
            if (customRenderer != null)
            {
                customRenderer.sprite = null;
                customRenderer.enabled = false;
                customRenderer.flipX = false;
                customRenderer.flipY = false;
                customRenderer.color = Color.white;
                customRenderer.transform.localScale = Vector3.one;
            }
            return;
        }

        EnsureCustomRenderer();
        customRenderer.enabled = true;
        customRenderer.flipX = false;
        customRenderer.flipY = false;
        customRenderer.color = profile.Tint;
        Vector3 rootScale = transform.localScale;
        customRenderer.transform.localScale = new Vector3(
            profile.VisualScale.x / Mathf.Max(Mathf.Abs(rootScale.x), 0.0001f),
            profile.VisualScale.y / Mathf.Max(Mathf.Abs(rootScale.y), 0.0001f), 1f);
        customRenderer.sprite = profile.HorizontalFrames[0];
    }

    private void EnsureCustomRenderer()
    {
        if (customRenderer != null) return;
        GameObject visual = new GameObject("FishPixelVisual");
        visual.transform.SetParent(transform, false);
        customRenderer = visual.AddComponent<SpriteRenderer>();
        customRenderer.sharedMaterial = prototypeRenderer.sharedMaterial;
        customRenderer.sortingLayerID = prototypeRenderer.sortingLayerID;
        customRenderer.sortingOrder = prototypeRenderer.sortingOrder;
        customRenderer.maskInteraction = prototypeRenderer.maskInteraction;
    }

    private void LateUpdate()
    {
        Tick(Time.deltaTime, movement != null ? movement.LastMovementDirection : Vector2.right);
    }

    public void Tick(float scaledDeltaTime, Vector2 movementDirection)
    {
        if (profile == null || customRenderer == null) return;

        if (specialProfile != null)
        {
            TickSpecial(scaledDeltaTime);
            return;
        }

        FishVisualDirection direction = FishVisualDirectionResolver.Resolve(
            movementDirection, heading);
        heading = direction.Heading;
        if (currentSet != direction.Set)
        {
            currentSet = direction.Set;
            customRenderer.sprite = profile.GetFrames(currentSet)[frameIndex];
        }
        customRenderer.flipX = direction.FlipX;
        customRenderer.flipY = direction.FlipY;

        if (scaledDeltaTime <= 0f) return;
        frameTimer += scaledDeltaTime;
        float interval = 1f / Mathf.Max(0.1f, profile.FramesPerSecond);
        if (frameTimer < interval) return;
        int steps = Mathf.FloorToInt(frameTimer / interval);
        frameTimer -= steps * interval;
        frameIndex = (frameIndex + steps) % 4;
        customRenderer.sprite = profile.GetFrames(currentSet)[frameIndex];
    }

    public bool PlaySpecial(IFishSpecialAnimation presentation, Action onRelease = null)
    {
        if (profile == null || customRenderer == null ||
            presentation == null || !presentation.HasAnimation)
            return false;

        Vector2 direction = movement != null ? movement.LastMovementDirection : Vector2.right;
        lockedDirection = FishVisualDirectionResolver.Resolve(direction, heading);
        specialProfile = presentation;
        specialRelease = onRelease;
        specialFrame = 0;
        specialTimer = 0f;
        ShowSpecialFrame();
        return true;
    }

    private void TickSpecial(float scaledDeltaTime)
    {
        if (scaledDeltaTime <= 0f) return;
        specialTimer += scaledDeltaTime;
        float interval = 1f / specialProfile.FramesPerSecond;
        while (specialProfile != null && specialTimer >= interval)
        {
            specialTimer -= interval;
            specialFrame++;
            if (specialFrame == 2)
            {
                Action release = specialRelease;
                specialRelease = null;
                release?.Invoke();
            }
            if (specialFrame >= 4)
            {
                ResetSpecial();
                // Resolve current movement on the same frame as the return to swimming.
                customRenderer.sprite = profile.GetFrames(currentSet)[frameIndex];
                Tick(0f, movement != null ? movement.LastMovementDirection : Vector2.right);
                return;
            }
            ShowSpecialFrame();
        }
    }

    private void ShowSpecialFrame()
    {
        customRenderer.sprite = specialProfile.GetFrames(lockedDirection.Set)[specialFrame];
        customRenderer.flipX = lockedDirection.FlipX;
        customRenderer.flipY = lockedDirection.FlipY;
    }

    private void ResetSpecial()
    {
        specialProfile = null;
        specialRelease = null;
        specialFrame = 0;
        specialTimer = 0f;
        lockedDirection = new FishVisualDirection(FishVisualHeading.East);
    }

    private void OnDisable()
    {
        ResetSpecial();
    }
}
