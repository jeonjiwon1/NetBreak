using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ItemHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text[] slotTexts = Array.Empty<TMP_Text>();
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;

    private int hoveredSlotIndex = -1;

    private void Start()
    {
        HideTooltip();
        Refresh();
    }

    private void Update()
    {
        Refresh();
        if (ToolSlotInput.IsSelectionOrEndBlocked)
        {
            HideTooltip();
            return;
        }

        if (hoveredSlotIndex >= 0)
        {
            RefreshTooltip(hoveredSlotIndex);
        }
    }

    public void ShowTooltip(int slotIndex)
    {
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

    private void Refresh()
    {
        RunItemInventory inventory = GetInventory();
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
    }

    private static RunItemInventory GetInventory() =>
        RunManager.Instance != null && RunManager.Instance.GrowthState != null
            ? RunManager.Instance.GrowthState.ItemInventory
            : null;
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
