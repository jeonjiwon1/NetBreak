using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GrowthItemPage : MonoBehaviour
{
    [Serializable]
    private sealed class ItemSlotView
    {
        [SerializeField] private TMP_Text label;

        public void Refresh(int slotIndex, RunItemInventory inventory)
        {
            if (label != null)
            {
                label.text = BuildItemSlotText(slotIndex, inventory);
            }
        }
    }

    [Serializable]
    private sealed class ElementEntryView
    {
        [SerializeField] private ItemElement element;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;

        public void Refresh(RunItemInventory inventory)
        {
            int level = inventory != null ? inventory.GetElementLevel(element) : 0;
            root?.SetActive(level > 0);
            if (label != null)
            {
                label.text = level > 0
                    ? $"{GetElementDisplayName(element)} Lv.{level}"
                    : string.Empty;
            }
        }
    }

    [Serializable]
    private sealed class CombinedCardView
    {
        [SerializeField] private CombinedSynergyId id;
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;

        public CombinedSynergyId Id => id;

        public void Bind(GrowthItemPage owner)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => owner.PreviewCombinedSynergy(id));
        }

        public void Refresh(
            RunItemInventory inventory,
            RunCombinedSynergyState state,
            bool previewed)
        {
            if (!CombinedSynergyCatalog.TryGet(id, out CombinedSynergyDefinition definition))
            {
                button?.gameObject.SetActive(false);
                return;
            }

            button?.gameObject.SetActive(true);
            int firstLevel = inventory?.GetElementLevel(definition.FirstElement) ?? 0;
            int secondLevel = inventory?.GetElementLevel(definition.SecondElement) ?? 0;
            bool eligible = state != null && state.IsEligible(id, inventory);
            int firstRequirement = state != null
                ? state.GetRequiredFirstLevel(definition)
                : definition.RequiredFirstLevel;
            int secondRequirement = state != null
                ? state.GetRequiredSecondLevel(definition)
                : definition.RequiredSecondLevel;
            if (label != null)
            {
                label.text =
                    $"<b>{definition.DisplayName}</b>\n" +
                    $"{GetElementDisplayName(definition.FirstElement)} Lv.{firstRequirement} + " +
                    $"{GetElementDisplayName(definition.SecondElement)} Lv.{secondRequirement}\n" +
                    $"현재 {firstLevel} / {secondLevel}\n" +
                    $"{definition.ShortDescription}\n" +
                    (eligible ? "<color=#79E6B2>해금됨 · 구현 예정</color>" :
                        "<color=#9AA7AD>미해금 · 구현 예정</color>");
            }

            if (background != null)
            {
                Color baseColor = eligible
                    ? new Color(0.09f, 0.25f, 0.31f, 0.98f)
                    : new Color(0.07f, 0.09f, 0.11f, 0.96f);
                background.color = previewed
                    ? Color.Lerp(baseColor, new Color(0.2f, 0.62f, 0.72f, 1f), 0.35f)
                    : baseColor;
            }
        }
    }

    [Header("Inventory")]
    [SerializeField] private ItemSlotView[] itemSlots = Array.Empty<ItemSlotView>();

    [Header("Owned Elements")]
    [SerializeField] private ElementEntryView[] elementEntries =
        Array.Empty<ElementEntryView>();
    [SerializeField] private TMP_Text emptyElementText;

    [Header("Combined Synergy")]
    [SerializeField] private CombinedCardView[] combinedCards =
        Array.Empty<CombinedCardView>();
    [SerializeField] private TMP_Text combinedDetailText;
    [SerializeField] private TMP_Text combinedCooldownText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text confirmButtonLabel;

    [Header("Shared Tooltips")]
    [SerializeField] private GameObject itemTooltipPanel;
    [SerializeField] private TMP_Text itemTooltipText;
    [SerializeField] private GameObject elementTooltipPanel;
    [SerializeField] private TMP_Text elementTooltipText;
    [SerializeField] private RectTransform tooltipBounds;

    private RunItemInventory boundInventory;
    private int hoveredSlot = -1;
    private ItemElement? hoveredElement;
    private CombinedSynergyId previewedCombination =
        CombinedSynergyId.ThunderSwordResonance;
    private bool pageVisible;
    private int lastDisplayedCooldownSecond = -1;

    private void Start()
    {
        for (int i = 0; i < combinedCards.Length; i++)
        {
            combinedCards[i]?.Bind(this);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmCombinedSynergy);
        }

        HideTooltips();
        BindInventory();
        RefreshAll();
    }

    private void Update()
    {
        BindInventory();
        if (!pageVisible)
        {
            return;
        }

        if (IsBlockingExternalModalOpen())
        {
            HideTooltips();
        }

        RunCombinedSynergyState state = GetCombinedState();
        int cooldownSecond = state != null
            ? Mathf.CeilToInt(state.GetRemainingSwitchCooldown(Time.time))
            : 0;
        if (cooldownSecond != lastDisplayedCooldownSecond)
        {
            lastDisplayedCooldownSecond = cooldownSecond;
            RefreshCombinedDetails();
        }
    }

    public void SetPageVisible(bool visible)
    {
        if (pageVisible == visible)
        {
            if (visible)
            {
                BindInventory();
            }
            return;
        }

        pageVisible = visible;
        if (!visible)
        {
            HideTooltips();
            return;
        }

        BindInventory();
        RefreshAll();
    }

    public void ShowItemTooltip(int slotIndex, PointerEventData eventData)
    {
        HideElementTooltip();
        hoveredSlot = slotIndex;
        RefreshItemTooltip(eventData);
    }

    public void HideItemTooltip()
    {
        hoveredSlot = -1;
        itemTooltipPanel?.SetActive(false);
    }

    public void ShowElementTooltip(ItemElement element, PointerEventData eventData)
    {
        HideItemTooltip();
        hoveredElement = element;
        RefreshElementTooltip(eventData);
    }

    public void HideElementTooltip()
    {
        hoveredElement = null;
        elementTooltipPanel?.SetActive(false);
    }

    public void PreviewCombinedSynergy(CombinedSynergyId id)
    {
        if (!CombinedSynergyCatalog.TryGet(id, out _))
        {
            return;
        }

        previewedCombination = id;
        GetCombinedState()?.TrySelect(id, boundInventory);
        RefreshCombinedCards();
        RefreshCombinedDetails();
    }

    public static string BuildItemSlotText(
        int slotIndex,
        RunItemInventory inventory)
    {
        if (slotIndex < 0 || slotIndex >= RunItemInventory.Capacity)
        {
            return string.Empty;
        }

        if (inventory == null || slotIndex >= inventory.OwnedItems.Count)
        {
            return $"슬롯 {slotIndex + 1}\n비어 있음";
        }

        RunItemInstance owned = inventory.OwnedItems[slotIndex];
        return ItemCatalog.TryGet(owned.ItemId, out ItemDefinition definition)
            ? $"<b>{definition.DisplayName}</b>\n{definition.ElementDisplayName} · Lv.{owned.Level}"
            : $"알 수 없는 아이템\nLv.{owned.Level}";
    }

    private void ConfirmCombinedSynergy()
    {
        RunCombinedSynergyState state = GetCombinedState();
        if (state == null)
        {
            return;
        }

        state.TryActivateSelected(
            boundInventory,
            Time.time,
            CombinedSynergyCatalog.CombatEffectsImplemented);
        RefreshCombinedCards();
        RefreshCombinedDetails();
    }

    private void BindInventory()
    {
        RunItemInventory current = GetInventory();
        if (ReferenceEquals(current, boundInventory))
        {
            return;
        }

        if (boundInventory != null)
        {
            boundInventory.Changed -= OnInventoryChanged;
        }

        boundInventory = current;
        if (boundInventory != null)
        {
            boundInventory.Changed += OnInventoryChanged;
        }

        RefreshAll();
    }

    private void OnInventoryChanged()
    {
        RefreshAll();
        if (hoveredSlot >= 0)
        {
            RefreshItemTooltip(null);
        }
        if (hoveredElement.HasValue)
        {
            RefreshElementTooltip(null);
        }
    }

    private void RefreshAll()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i]?.Refresh(i, boundInventory);
        }

        int ownedElementCount = 0;
        for (int i = 0; i < elementEntries.Length; i++)
        {
            elementEntries[i]?.Refresh(boundInventory);
        }
        foreach (ItemElement element in Enum.GetValues(typeof(ItemElement)))
        {
            if (boundInventory != null && boundInventory.GetElementLevel(element) > 0)
            {
                ownedElementCount++;
            }
        }

        if (emptyElementText != null)
        {
            emptyElementText.gameObject.SetActive(ownedElementCount == 0);
            emptyElementText.text = ownedElementCount == 0
                ? "보유한 아이템이 없습니다."
                : string.Empty;
        }

        RefreshCombinedCards();
        RefreshCombinedDetails();
    }

    private void RefreshCombinedCards()
    {
        RunCombinedSynergyState state = GetCombinedState();
        for (int i = 0; i < combinedCards.Length; i++)
        {
            combinedCards[i]?.Refresh(
                boundInventory,
                state,
                combinedCards[i] != null &&
                combinedCards[i].Id == previewedCombination);
        }
    }

    private void RefreshCombinedDetails()
    {
        if (!CombinedSynergyCatalog.TryGet(
                previewedCombination,
                out CombinedSynergyDefinition definition))
        {
            return;
        }

        RunCombinedSynergyState state = GetCombinedState();
        bool eligible = state != null &&
            state.IsEligible(previewedCombination, boundInventory);
        int firstRequirement = state != null
            ? state.GetRequiredFirstLevel(definition)
            : definition.RequiredFirstLevel;
        int secondRequirement = state != null
            ? state.GetRequiredSecondLevel(definition)
            : definition.RequiredSecondLevel;
        int firstLevel = boundInventory?.GetElementLevel(definition.FirstElement) ?? 0;
        int secondLevel = boundInventory?.GetElementLevel(definition.SecondElement) ?? 0;
        if (combinedDetailText != null)
        {
            combinedDetailText.text =
                $"<b>{definition.DisplayName}</b> · " +
                $"{GetElementDisplayName(definition.FirstElement)} {firstLevel}/{firstRequirement}, " +
                $"{GetElementDisplayName(definition.SecondElement)} {secondLevel}/{secondRequirement}\n" +
                definition.DetailedDescription + "\n" +
                (eligible
                    ? "<color=#79E6B2>해금됨 · 전투 활성화는 G6-C2 구현 예정</color>"
                    : "<color=#9AA7AD>미해금 · 요구 속성 레벨을 달성하세요.</color>");
        }

        float remaining = state?.GetRemainingSwitchCooldown(Time.time) ?? 0f;
        if (combinedCooldownText != null)
        {
            combinedCooldownText.text = remaining > 0f
                ? $"변경 대기: {remaining:0.0}초"
                : "변경 대기 없음";
        }

        bool currentlyActive = state != null &&
            state.ActiveId == previewedCombination &&
            CombinedSynergyCatalog.CombatEffectsImplemented;
        if (confirmButtonLabel != null)
        {
            confirmButtonLabel.text = currentlyActive
                ? "현재 사용 중"
                : "이 복합 시너지로 변경";
        }
        if (confirmButton != null)
        {
            confirmButton.interactable =
                CombinedSynergyCatalog.CombatEffectsImplemented &&
                eligible && !currentlyActive && remaining <= 0f;
        }
    }

    private void RefreshItemTooltip(PointerEventData eventData)
    {
        if (!pageVisible || IsBlockingExternalModalOpen() ||
            boundInventory == null || hoveredSlot < 0 ||
            hoveredSlot >= boundInventory.OwnedItems.Count ||
            itemTooltipPanel == null || itemTooltipText == null)
        {
            HideItemTooltip();
            return;
        }

        ItemEffectManager manager = ItemEffectManager.Instance;
        RunItemInstance owned = boundInventory.OwnedItems[hoveredSlot];
        if (manager == null)
        {
            HideItemTooltip();
            return;
        }

        itemTooltipText.text = manager.BuildItemTooltipText(owned, boundInventory);
        itemTooltipPanel.SetActive(true);
        BringTooltipToFront(itemTooltipPanel);
        PositionTooltip(itemTooltipPanel.transform as RectTransform, eventData);
    }

    private void RefreshElementTooltip(PointerEventData eventData)
    {
        if (!pageVisible || IsBlockingExternalModalOpen() ||
            !hoveredElement.HasValue || boundInventory == null ||
            boundInventory.GetElementLevel(hoveredElement.Value) <= 0 ||
            ItemEffectManager.Instance == null || elementTooltipPanel == null ||
            elementTooltipText == null)
        {
            HideElementTooltip();
            return;
        }

        elementTooltipText.text = ItemEffectManager.Instance
            .BuildElementSynergyTooltipText(hoveredElement.Value, boundInventory);
        elementTooltipPanel.SetActive(true);
        BringTooltipToFront(elementTooltipPanel);
        PositionTooltip(elementTooltipPanel.transform as RectTransform, eventData);
    }

    private void BringTooltipToFront(GameObject tooltipPanel)
    {
        if (tooltipPanel == null)
        {
            return;
        }

        Transform overlayParent = transform.parent;
        if (overlayParent != null && tooltipPanel.transform.parent != overlayParent)
        {
            tooltipPanel.transform.SetParent(overlayParent, false);
        }

        tooltipPanel.transform.SetAsLastSibling();
    }

    private void PositionTooltip(RectTransform tooltip, PointerEventData eventData)
    {
        if (tooltip == null || tooltipBounds == null || eventData == null ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipBounds,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        Rect bounds = tooltipBounds.rect;
        Vector2 size = tooltip.rect.size;
        Vector2 desired = localPoint + new Vector2(24f, -24f);
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        desired.x = Mathf.Clamp(
            desired.x,
            bounds.xMin + halfWidth,
            bounds.xMax - halfWidth);
        desired.y = Mathf.Clamp(
            desired.y,
            bounds.yMin + halfHeight,
            bounds.yMax - halfHeight);
        tooltip.anchoredPosition = desired;
    }

    private void HideTooltips()
    {
        HideItemTooltip();
        HideElementTooltip();
    }

    private void OnDisable()
    {
        HideTooltips();
        if (boundInventory != null)
        {
            boundInventory.Changed -= OnInventoryChanged;
            boundInventory = null;
        }
    }

    private static RunItemInventory GetInventory() =>
        RunManager.Instance?.GrowthState?.ItemInventory;

    private static RunCombinedSynergyState GetCombinedState() =>
        RunManager.Instance?.GrowthState?.CombinedSynergy;

    private static string GetElementDisplayName(ItemElement element) => element switch
    {
        ItemElement.Electric => "전기",
        ItemElement.Sword => "검",
        ItemElement.Ice => "얼음",
        _ => "알 수 없음"
    };

    private static bool IsBlockingExternalModalOpen() =>
        (TacticalSkillManager.Instance != null && TacticalSkillManager.Instance.IsChoosing) ||
        (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsChoosingTool) ||
        (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
        (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
        ItemRewardManager.IsSelectionPendingOrActive;
}

public sealed class GrowthElementHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerMoveHandler,
    IPointerExitHandler
{
    [SerializeField] private GrowthItemPage owner;
    [SerializeField] private ItemElement element;

    public void Configure(GrowthItemPage page, ItemElement itemElement)
    {
        owner = page;
        element = itemElement;
    }

    public void OnPointerEnter(PointerEventData eventData) =>
        owner?.ShowElementTooltip(element, eventData);

    public void OnPointerMove(PointerEventData eventData) =>
        owner?.ShowElementTooltip(element, eventData);

    public void OnPointerExit(PointerEventData eventData) =>
        owner?.HideElementTooltip();
}
