using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class SkillTreeTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text label;
    [SerializeField] private RectTransform visibleBounds;
    [SerializeField] private Vector2 offset = new(22f, -14f);
    [Min(300f)] [SerializeField] private float tooltipWidth = 430f;
    [Min(120f)] [SerializeField] private float minimumHeight = 220f;
    [Min(160f)] [SerializeField] private float maximumHeight = 430f;
    [Min(8f)] [SerializeField] private float horizontalPadding = 22f;
    [Min(8f)] [SerializeField] private float verticalPadding = 18f;

    private object owner;
    private RectTransform anchor;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private bool initialized;

    private static SkillTreeTooltip instance;
    public static SkillTreeTooltip Instance => instance != null
        ? instance
        : instance = FindFirstObjectByType<SkillTreeTooltip>(FindObjectsInactive.Include);

    public static void HideShared()
    {
        if (instance != null) instance.HideAll();
    }

    private void Awake()
    {
        instance = this;
        EnsureInitialized();
        if (owner == null) HideAll();
    }

    private void LateUpdate()
    {
        if (owner == null || anchor == null || panel == null || !panel.gameObject.activeSelf)
            return;
        PositionPanel();
    }

    public void Show(object tooltipOwner, RectTransform target, string text)
    {
        EnsureInitialized();
        if (tooltipOwner == null || target == null || panel == null || label == null ||
            (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsMandatoryAcquisition))
        {
            HideAll();
            return;
        }
        owner = tooltipOwner;
        anchor = target;
        label.text = text;
        panel.gameObject.SetActive(true);
        panel.SetAsLastSibling();
        UpdateSize();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        PositionPanel();
    }

    public void Hide(object tooltipOwner)
    {
        if (ReferenceEquals(owner, tooltipOwner)) HideAll();
    }

    public void HideAll()
    {
        owner = null;
        anchor = null;
        if (panel != null) panel.gameObject.SetActive(false);
    }

    private void PositionPanel()
    {
        if (!EnsureInitialized() || panel == null || anchor == null ||
            panel.parent is not RectTransform parent)
        {
            HideAll();
            return;
        }

        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, anchor.position);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, screen, camera, out Vector2 local)) return;

        Vector2 position = local + offset;
        Rect bounds = GetBoundsInParent(parent);
        Vector2 size = panel.rect.size;
        position.x = Mathf.Clamp(position.x,
            bounds.xMin + size.x * panel.pivot.x,
            bounds.xMax - size.x * (1f - panel.pivot.x));
        position.y = Mathf.Clamp(position.y,
            bounds.yMin + size.y * panel.pivot.y,
            bounds.yMax - size.y * (1f - panel.pivot.y));
        panel.anchoredPosition = position;
    }

    private bool EnsureInitialized()
    {
        if (initialized && panel != null && label != null) return true;
        panel ??= transform as RectTransform;
        if (panel == null) return false;
        label ??= panel.GetComponentInChildren<TMP_Text>(true);
        if (visibleBounds == null) visibleBounds = panel.parent as RectTransform;
        canvas = panel.GetComponentInParent<Canvas>(true);
        canvasGroup = panel.GetComponent<CanvasGroup>() ??
            panel.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.ignoreParentGroups = false;
        panel.pivot = new Vector2(0f, 1f);
        if (label != null)
        {
            label.enableAutoSizing = false;
            label.fontSize = 17f;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.TopLeft;
        }
        initialized = label != null;
        return initialized;
    }

    private void UpdateSize()
    {
        float contentWidth = Mathf.Max(1f, tooltipWidth - horizontalPadding * 2f);
        Vector2 preferred = label.GetPreferredValues(label.text, contentWidth, 0f);
        float height = Mathf.Clamp(
            preferred.y + verticalPadding * 2f,
            minimumHeight,
            maximumHeight);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tooltipWidth);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(horizontalPadding, verticalPadding);
        labelRect.offsetMax = new Vector2(-horizontalPadding, -verticalPadding);
    }

    private Rect GetBoundsInParent(RectTransform parent)
    {
        if (visibleBounds == null || visibleBounds == parent) return parent.rect;
        Bounds relative = RectTransformUtility.CalculateRelativeRectTransformBounds(
            parent, visibleBounds);
        return new Rect(relative.min.x, relative.min.y, relative.size.x, relative.size.y);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
