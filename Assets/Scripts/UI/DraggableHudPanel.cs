using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Attach to a small raycastable title bar, not the information panel body.
public sealed class DraggableHudPanel : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    private const float EdgeMargin = 8f;
    private static readonly HashSet<DraggableHudPanel> Handles =
        new HashSet<DraggableHudPanel>();
    private static DraggableHudPanel activeHandle;

    private RectTransform panel;
    private RectTransform parent;
    private RectTransform handle;
    private readonly Vector3[] corners = new Vector3[4];
    private Vector2 pointerAtStart;
    private Vector2 panelAtStart;

    // A direct pointer check also covers the click frame before EventSystem dispatch.
    public static bool BlocksWorldPointer => activeHandle != null ||
        (Mouse.current != null && Mouse.current.leftButton.isPressed &&
         IsPointerOverHandle());

    public static bool IsPointerOverHandle()
    {
        if (Mouse.current == null) return false;
        Vector2 pointer = Mouse.current.position.ReadValue();
        foreach (DraggableHudPanel candidate in Handles)
        {
            if (candidate == null || !candidate.isActiveAndEnabled ||
                candidate.handle == null) continue;
            Canvas canvas = candidate.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    candidate.handle, pointer, camera)) return true;
        }
        return false;
    }

    public void Configure(RectTransform target)
    {
        panel = target;
        parent = target != null ? target.parent as RectTransform : null;
        handle = transform as RectTransform;
        Handles.Add(this);
        ClampToParent();
    }

    private void OnEnable()
    {
        if (panel != null) Handles.Add(this);
    }

    private void Update()
    {
        ClampToParent();
        if (activeHandle == this &&
            (Mouse.current == null || !Mouse.current.leftButton.isPressed))
            activeHandle = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            activeHandle = this;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activeHandle == this) activeHandle = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left ||
            panel == null || parent == null ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera,
                out pointerAtStart)) return;
        panelAtStart = panel.anchoredPosition;
        activeHandle = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeHandle != this || panel == null || parent == null ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, eventData.position, eventData.pressEventCamera,
                out Vector2 pointerNow)) return;
        panel.anchoredPosition = panelAtStart + pointerNow - pointerAtStart;
        ClampToParent();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (activeHandle == this) activeHandle = null;
    }

    private void ClampToParent()
    {
        if (panel == null || parent == null) return;
        panel.GetWorldCorners(corners);
        Vector3 lower = parent.InverseTransformPoint(corners[0]);
        Vector3 upper = parent.InverseTransformPoint(corners[2]);
        Rect area = parent.rect;
        Vector2 correction = Vector2.zero;
        if (lower.x < area.xMin + EdgeMargin)
            correction.x = area.xMin + EdgeMargin - lower.x;
        else if (upper.x > area.xMax - EdgeMargin)
            correction.x = area.xMax - EdgeMargin - upper.x;
        if (lower.y < area.yMin + EdgeMargin)
            correction.y = area.yMin + EdgeMargin - lower.y;
        else if (upper.y > area.yMax - EdgeMargin)
            correction.y = area.yMax - EdgeMargin - upper.y;
        panel.anchoredPosition += correction;
    }

    private void OnDisable()
    {
        Handles.Remove(this);
        if (activeHandle == this) activeHandle = null;
    }
}
