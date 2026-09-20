using UnityEngine;
using UnityEngine.EventSystems;

public sealed class GrowthItemSlotHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerMoveHandler,
    IPointerExitHandler
{
    [SerializeField] private GrowthItemPage owner;
    [SerializeField] private int slotIndex;

    public void Configure(GrowthItemPage page, int index)
    {
        owner = page;
        slotIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData) =>
        owner?.ShowItemTooltip(slotIndex, eventData);

    public void OnPointerMove(PointerEventData eventData) =>
        owner?.ShowItemTooltip(slotIndex, eventData);

    public void OnPointerExit(PointerEventData eventData) =>
        owner?.HideItemTooltip();
}
