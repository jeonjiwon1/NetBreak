using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SkillTreePanZoom : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IScrollHandler
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Canvas canvas;
    [Header("Navigation")]
    [Min(0.1f)] [SerializeField] private float minimumZoom = 0.75f;
    [Min(0.1f)] [SerializeField] private float maximumZoom = 1.5f;
    [Min(0.01f)] [SerializeField] private float zoomSensitivity = 0.1f;
    [Min(0.01f)] [SerializeField] private float panSensitivity = 1f;

    private Vector2 startPointerPosition;
    private Vector2 startAnchoredPosition;

    private void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        ClampSettings();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (content == null)
        {
            return;
        }

        startPointerPosition = eventData.position;
        startAnchoredPosition = content.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (content == null)
        {
            return;
        }

        float scaleFactor = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
        Vector2 delta = (eventData.position - startPointerPosition) / scaleFactor;
        content.anchoredPosition = startAnchoredPosition + delta * panSensitivity;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (content == null)
        {
            return;
        }

        float current = content.localScale.x;
        float next = Mathf.Clamp(
            current + eventData.scrollDelta.y * zoomSensitivity,
            minimumZoom,
            maximumZoom);
        content.localScale = new Vector3(next, next, 1f);
    }

    public void ResetView()
    {
        if (content == null)
        {
            return;
        }

        content.anchoredPosition = Vector2.zero;
        content.localScale = Vector3.one;
    }

    private void OnValidate()
    {
        ClampSettings();
    }

    private void ClampSettings()
    {
        minimumZoom = Mathf.Max(0.1f, minimumZoom);
        maximumZoom = Mathf.Max(minimumZoom, maximumZoom);
        zoomSensitivity = Mathf.Max(0.01f, zoomSensitivity);
        panSensitivity = Mathf.Max(0.01f, panSensitivity);
    }
}
