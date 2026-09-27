using UnityEngine;
using UnityEngine.SceneManagement;

// A camera-attached visual for the current single-scene Area 1 prototype.
// It has no collider, input, or encounter state.
public sealed class Area1BackgroundController : MonoBehaviour
{
    public const string ResourcePath = "Area1/CoastBackground";
    public const int BackgroundSortingOrder = -1000;

    private Camera targetCamera;
    private SpriteRenderer backgroundRenderer;
    private float lastSize = -1f;
    private float lastAspect = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneLoad()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main") return;

        Camera camera = Camera.main;
        Sprite sprite = Resources.Load<Sprite>(ResourcePath);
        if (camera == null || !camera.orthographic || sprite == null)
        {
            Debug.LogWarning("Area 1 background: camera or sprite is unavailable.");
            return;
        }

        Area1BackgroundController controller =
            camera.GetComponent<Area1BackgroundController>();
        if (controller == null)
            controller = camera.gameObject.AddComponent<Area1BackgroundController>();
        controller.Initialize(camera, sprite);
    }

    public void Initialize(Camera camera, Sprite sprite)
    {
        if (camera == null || sprite == null) return;

        targetCamera = camera;
        if (backgroundRenderer == null)
        {
            Transform existing = transform.Find("Area1CoastBackground");
            GameObject visual = existing != null
                ? existing.gameObject : new GameObject("Area1CoastBackground");
            visual.transform.SetParent(transform, false);
            backgroundRenderer = visual.GetComponent<SpriteRenderer>();
            if (backgroundRenderer == null)
                backgroundRenderer = visual.AddComponent<SpriteRenderer>();
        }

        backgroundRenderer.sprite = sprite;
        backgroundRenderer.color = Color.white;
        backgroundRenderer.sortingOrder = BackgroundSortingOrder;
        backgroundRenderer.transform.localPosition = new Vector3(0f, 0f, 9f);

        lastSize = -1f;
        FitToCamera();
    }

    private void LateUpdate()
    {
        FitToCamera();
    }

    public void FitToCamera()
    {
        if (targetCamera == null || backgroundRenderer == null ||
            backgroundRenderer.sprite == null || !targetCamera.orthographic)
            return;

        float size = targetCamera.orthographicSize;
        float aspect = targetCamera.aspect;
        if (Mathf.Approximately(size, lastSize) &&
            Mathf.Approximately(aspect, lastAspect)) return;

        Vector2 spriteSize = backgroundRenderer.sprite.bounds.size;
        backgroundRenderer.transform.localScale = new Vector3(
            2f * size * aspect / spriteSize.x,
            2f * size / spriteSize.y,
            1f);
        lastSize = size;
        lastAspect = aspect;
    }
}
