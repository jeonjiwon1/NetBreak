using UnityEngine;

// Camera-attached, presentation-only water light for the Area 1 prototype.
public sealed class Area1WaterOverlay : MonoBehaviour
{
    public const int SortingOrder = Area1BackgroundController.BackgroundSortingOrder + 1;

    [SerializeField, Range(0f, 1f)] private float alpha = 0.40f;
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.035f, 0.018f);

    private Camera targetCamera;
    private Sprite sprite;
    private SpriteRenderer backgroundRenderer;
    private Transform tileRoot;
    private Vector2 tileSize;
    private Vector2 displacement;
    private float lastSize = -1f;
    private float lastAspect = -1f;

    public void Initialize(Camera camera, Sprite waterSprite, SpriteRenderer background)
    {
        if (camera == null || waterSprite == null || background == null) return;

        targetCamera = camera;
        sprite = waterSprite;
        backgroundRenderer = background;
        tileSize = sprite.bounds.size;

        if (tileRoot == null)
        {
            Transform existing = transform.Find("Area1WaterOverlay");
            tileRoot = existing != null ? existing : new GameObject("Area1WaterOverlay").transform;
            tileRoot.SetParent(transform, false);
        }

        lastSize = -1f;
        FitToCamera();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || tileRoot == null) return;
        FitToCamera();
        Advance(Time.unscaledDeltaTime);
    }

    public void Advance(float unscaledDeltaTime)
    {
        if (tileRoot == null || tileSize.x <= 0f || tileSize.y <= 0f) return;

        displacement += scrollSpeed * Mathf.Max(0f, unscaledDeltaTime);
        displacement.x = Mathf.Repeat(displacement.x + tileSize.x * 0.5f, tileSize.x) - tileSize.x * 0.5f;
        displacement.y = Mathf.Repeat(displacement.y + tileSize.y * 0.5f, tileSize.y) - tileSize.y * 0.5f;
        tileRoot.localPosition = new Vector3(displacement.x, displacement.y, 8f);
    }

    public void FitToCamera()
    {
        if (targetCamera == null || tileRoot == null || sprite == null ||
            !targetCamera.orthographic || tileSize.x <= 0f || tileSize.y <= 0f)
            return;

        float size = targetCamera.orthographicSize;
        float aspect = targetCamera.aspect;
        if (Mathf.Approximately(size, lastSize) && Mathf.Approximately(aspect, lastAspect))
            return;

        // The centered phase is at most half a tile in either direction.
        int halfColumns = Mathf.CeilToInt(size * aspect / tileSize.x);
        int halfRows = Mathf.CeilToInt(size / tileSize.y);
        int columns = 2 * halfColumns + 1;
        int rows = 2 * halfRows + 1;
        int required = columns * rows;

        while (tileRoot.childCount < required)
        {
            GameObject tile = new GameObject("WaterLightTile");
            tile.transform.SetParent(tileRoot, false);
            tile.AddComponent<SpriteRenderer>();
        }

        Color tint = new Color(1f, 1f, 1f, alpha);
        for (int i = 0; i < tileRoot.childCount; i++)
        {
            Transform tile = tileRoot.GetChild(i);
            bool active = i < required;
            tile.gameObject.SetActive(active);
            if (!active) continue;

            int column = i % columns - halfColumns;
            int row = i / columns - halfRows;
            tile.localPosition = new Vector3(column * tileSize.x, row * tileSize.y, 0f);
            SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = backgroundRenderer.sharedMaterial;
            renderer.sortingLayerID = backgroundRenderer.sortingLayerID;
            renderer.sortingOrder = SortingOrder;
            renderer.color = tint;
        }

        lastSize = size;
        lastAspect = aspect;
        Advance(0f);
    }
}
