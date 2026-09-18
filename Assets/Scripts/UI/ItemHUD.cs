using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ItemHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text[] slotTexts = Array.Empty<TMP_Text>();
    [SerializeField] private TMP_Text elementLevelText;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private GameObject elementSynergyTooltipPanel;
    [SerializeField] private TMP_Text elementSynergyTooltipText;

    private int hoveredSlotIndex = -1;
    private ItemElement? hoveredElement;
    private RunItemInventory boundInventory;

    private void OnEnable()
    {
        BindInventory();
    }

    private void Start()
    {
        HideTooltip();
        HideElementSynergyTooltip();
        Refresh();
    }

    private void Update()
    {
        BindInventory();
        Refresh();
        if (ToolSlotInput.IsSelectionOrEndBlocked)
        {
            HideTooltip();
            HideElementSynergyTooltip();
            return;
        }

        if (hoveredSlotIndex >= 0)
        {
            RefreshTooltip(hoveredSlotIndex);
        }

        if (hoveredElement.HasValue)
        {
            RefreshElementSynergyTooltip(hoveredElement.Value);
        }
    }

    public void ShowTooltip(int slotIndex)
    {
        HideElementSynergyTooltip();
        hoveredSlotIndex = slotIndex;
        RefreshTooltip(slotIndex);
    }

    private void RefreshTooltip(int slotIndex)
    {
        RunItemInventory inventory = GetInventory();
        if (inventory == null || slotIndex < 0 || slotIndex >= inventory.OwnedItems.Count)
        {
            HideTooltip();
            return;
        }

        RunItemInstance owned = inventory.OwnedItems[slotIndex];
        if (!ItemCatalog.TryGet(owned.ItemId, out ItemDefinition definition))
        {
            HideTooltip();
            return;
        }

        if (tooltipText != null)
        {
            string status = ItemEffectManager.Instance != null
                ? ItemEffectManager.Instance.GetStatusText(owned.ItemId)
                : string.Empty;
            tooltipText.text =
                $"<b>{definition.DisplayName}</b>\n" +
                $"속성: {definition.ElementDisplayName} · Lv.{owned.Level}\n\n" +
                $"{definition.PlannedEffectDescription}" +
                (string.IsNullOrEmpty(status)
                    ? string.Empty
                    : $"\n\n<color=#9DDEF2>{status}</color>");
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            tooltipPanel.transform.SetAsLastSibling();
        }
    }

    public void HideTooltip()
    {
        hoveredSlotIndex = -1;
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void ShowElementSynergyTooltip(ItemElement element)
    {
        HideTooltip();
        hoveredElement = element;
        RefreshElementSynergyTooltip(element);
    }

    public void HideElementSynergyTooltip()
    {
        hoveredElement = null;
        if (elementSynergyTooltipPanel != null)
        {
            elementSynergyTooltipPanel.SetActive(false);
        }
    }

    private void RefreshElementSynergyTooltip(ItemElement element)
    {
        RunItemInventory inventory = boundInventory ?? GetInventory();
        ItemEffectManager manager = ItemEffectManager.Instance;
        if (inventory == null ||
            inventory.GetElementLevel(element) <= 0 ||
            manager == null ||
            elementSynergyTooltipPanel == null ||
            elementSynergyTooltipText == null)
        {
            HideElementSynergyTooltip();
            return;
        }

        elementSynergyTooltipText.text =
            manager.BuildElementSynergyTooltipText(element, inventory);
        elementSynergyTooltipPanel.SetActive(true);
        elementSynergyTooltipPanel.transform.SetAsLastSibling();
    }

    private void Refresh()
    {
        RunItemInventory inventory = boundInventory ?? GetInventory();
        for (int i = 0; i < slotTexts.Length; i++)
        {
            TMP_Text text = slotTexts[i];
            if (text == null)
            {
                continue;
            }

            if (inventory == null || i >= inventory.OwnedItems.Count)
            {
                text.text = $"{i + 1}\n비어 있음";
                continue;
            }

            RunItemInstance owned = inventory.OwnedItems[i];
            text.text = ItemCatalog.TryGet(owned.ItemId, out ItemDefinition definition)
                ? $"{definition.ShortLabel}\n{definition.ElementDisplayName} · Lv.{owned.Level}"
                : $"알 수 없음\nLv.{owned.Level}";
        }

        if (elementLevelText != null)
        {
            string presentation = BuildElementLevelRichText(inventory);
            if (!string.Equals(elementLevelText.text, presentation, StringComparison.Ordinal))
            {
                elementLevelText.text = presentation;
            }
            elementLevelText.gameObject.SetActive(!string.IsNullOrEmpty(presentation));
        }
    }

    public static string BuildElementLevelText(RunItemInventory inventory)
    {
        return BuildElementLevelText(inventory, false);
    }

    private static string BuildElementLevelRichText(RunItemInventory inventory)
    {
        return BuildElementLevelText(inventory, true);
    }

    private static string BuildElementLevelText(
        RunItemInventory inventory,
        bool includeHoverLinks)
    {
        if (inventory == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        AppendOwnedElement(
            builder, inventory, ItemElement.Electric, "전기", includeHoverLinks);
        AppendOwnedElement(
            builder, inventory, ItemElement.Sword, "검", includeHoverLinks);
        AppendOwnedElement(
            builder, inventory, ItemElement.Ice, "얼음", includeHoverLinks);
        return builder.ToString();
    }

    private static void AppendOwnedElement(
        StringBuilder builder,
        RunItemInventory inventory,
        ItemElement element,
        string displayName,
        bool includeHoverLink)
    {
        int level = inventory.GetElementLevel(element);
        if (level <= 0)
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append('\n');
        }

        if (includeHoverLink)
        {
            builder.Append("<link=\"");
            builder.Append(element);
            builder.Append("\">");
        }

        builder.Append(displayName);
        builder.Append(" Lv.");
        builder.Append(level);
        if (includeHoverLink)
        {
            builder.Append("</link>");
        }
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

        Refresh();
    }

    private void OnInventoryChanged()
    {
        Refresh();
        if (hoveredSlotIndex >= 0)
        {
            RefreshTooltip(hoveredSlotIndex);
        }
        if (hoveredElement.HasValue)
        {
            RefreshElementSynergyTooltip(hoveredElement.Value);
        }
    }

    private void OnDisable()
    {
        HideTooltip();
        HideElementSynergyTooltip();
        if (boundInventory != null)
        {
            boundInventory.Changed -= OnInventoryChanged;
            boundInventory = null;
        }
    }

    private static RunItemInventory GetInventory() =>
        RunManager.Instance != null && RunManager.Instance.GrowthState != null
            ? RunManager.Instance.GrowthState.ItemInventory
            : null;
}

public sealed class ItemElementLevelHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerMoveHandler,
    IPointerExitHandler
{
    [SerializeField] private ItemHUD itemHUD;
    [SerializeField] private TMP_Text elementLevelText;

    public void Configure(ItemHUD owner, TMP_Text label)
    {
        itemHUD = owner;
        elementLevelText = label;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RefreshHoveredElement(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RefreshHoveredElement(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemHUD?.HideElementSynergyTooltip();
    }

    private void RefreshHoveredElement(PointerEventData eventData)
    {
        if (itemHUD == null || elementLevelText == null || eventData == null)
        {
            itemHUD?.HideElementSynergyTooltip();
            return;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            elementLevelText,
            eventData.position,
            eventData.pressEventCamera);
        if (linkIndex < 0 || linkIndex >= elementLevelText.textInfo.linkCount)
        {
            itemHUD.HideElementSynergyTooltip();
            return;
        }

        string linkId = elementLevelText.textInfo.linkInfo[linkIndex].GetLinkID();
        if (Enum.TryParse(linkId, out ItemElement element))
        {
            itemHUD.ShowElementSynergyTooltip(element);
        }
        else
        {
            itemHUD.HideElementSynergyTooltip();
        }
    }
}

public sealed class ItemHUDSlotHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private ItemHUD itemHUD;
    [SerializeField] private int slotIndex;

    public void Configure(ItemHUD owner, int index)
    {
        itemHUD = owner;
        slotIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemHUD?.ShowTooltip(slotIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemHUD?.HideTooltip();
    }
}
