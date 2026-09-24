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

    public bool UsesCustomVisual => profile != null;
    public int FrameIndex => frameIndex;

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
}
