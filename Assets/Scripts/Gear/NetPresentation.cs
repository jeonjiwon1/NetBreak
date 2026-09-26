using UnityEngine;

// Draws repeating tiles in world units while the gameplay root keeps its
// length/thickness scale for the BoxCollider2D. No child has a collider.
public sealed class NetPresentation : MonoBehaviour
{
    [SerializeField] private NetPresentationProfile profile;

    private SpriteRenderer source;
    private SpriteRenderer mesh;
    private SpriteRenderer upperRope;
    private SpriteRenderer lowerRope;
    private NetController net;
    private bool preview;

    public NetPresentationProfile Profile => profile;

    public void Configure(bool isPreview, NetPresentationProfile value = null)
    {
        preview = isPreview;
        if (value != null) profile = value;
        Initialize();
        RefreshVisual();
    }

    private void Awake() => Initialize();
    private void OnEnable() => Initialize();

    private void Initialize()
    {
        if (profile == null) profile = Resources.Load<NetPresentationProfile>("NetPresentation");
        if (source == null) source = GetComponent<SpriteRenderer>();
        if (net == null) net = GetComponent<NetController>();
        if (profile == null || profile.MeshTile == null || profile.RopeTile == null || source == null)
            return;

        mesh = EnsureTile("NetMeshVisual", mesh, profile.MeshTile, source.sortingOrder);
        upperRope = EnsureTile("NetUpperRopeVisual", upperRope, profile.RopeTile, source.sortingOrder + 1);
        lowerRope = EnsureTile("NetLowerRopeVisual", lowerRope, profile.RopeTile, source.sortingOrder + 1);
        source.enabled = false;
    }

    private SpriteRenderer EnsureTile(string objectName, SpriteRenderer current,
        Sprite sprite, int sortingOrder)
    {
        if (current == null)
        {
            Transform existing = transform.Find(objectName);
            GameObject visual = existing != null ? existing.gameObject : new GameObject(objectName);
            visual.transform.SetParent(transform, false);
            current = visual.GetComponent<SpriteRenderer>();
            if (current == null) current = visual.AddComponent<SpriteRenderer>();
        }
        current.sprite = sprite;
        current.drawMode = SpriteDrawMode.Tiled;
        current.sortingLayerID = source.sortingLayerID;
        current.sortingOrder = sortingOrder;
        return current;
    }

    private void LateUpdate() => RefreshVisual();

    public void RefreshVisual()
    {
        if (mesh == null || source == null) return;
        float length = Mathf.Abs(transform.localScale.x);
        float thickness = Mathf.Abs(transform.localScale.y);
        if (length < 0.001f || thickness < 0.001f) return;

        Vector3 inverse = new(1f / length, 1f / thickness, 1f);
        mesh.transform.localScale = inverse;
        mesh.transform.localPosition = Vector3.zero;
        mesh.size = new Vector2(length, thickness);

        float ropeHeight = profile.RopeTile.rect.height / profile.RopeTile.pixelsPerUnit;
        SetRope(upperRope, inverse, length, ropeHeight, thickness * .5f - ropeHeight * .5f);
        SetRope(lowerRope, inverse, length, ropeHeight, -thickness * .5f + ropeHeight * .5f);

        Color tint = source.color;
        if (preview)
            tint.a = Mathf.Max(tint.a, .7f);
        else if (net == null || net.IsOperational)
            tint.a *= 0.93f + 0.04f * Mathf.Sin(Time.time * 3f + GetInstanceID());
        mesh.color = tint;
        Color ropeTint = source.color;
        if (preview) ropeTint.a = Mathf.Max(ropeTint.a, .7f);
        upperRope.color = ropeTint;
        lowerRope.color = ropeTint;
    }

    private static void SetRope(SpriteRenderer renderer, Vector3 inverse,
        float length, float height, float localY)
    {
        renderer.transform.localScale = inverse;
        renderer.transform.localPosition = new Vector3(0f, localY * inverse.y, 0f);
        renderer.size = new Vector2(length, height);
    }
}
