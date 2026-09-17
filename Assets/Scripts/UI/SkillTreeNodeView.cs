using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SkillTreeNodeView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text lockText;
    [SerializeField] private Color availableColor = new(0.25f, 0.55f, 0.75f, 1f);
    [SerializeField] private Color unavailableColor = new(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color completedColor = new(0.65f, 0.5f, 0.15f, 1f);

    private SkillTreeManager manager;
    private GrowthToolRole role;
    private SkillTreeDefinition definition;
    private SkillTreeNodeDefinition node;
    private GrowthAbilitySlot abilitySlot;
    private bool isAbilityNode;
    private bool isHovered;

    public void Bind(
        SkillTreeManager treeManager,
        GrowthToolRole treeRole,
        SkillTreeDefinition treeDefinition,
        SkillTreeNodeDefinition nodeDefinition)
    {
        manager = treeManager;
        role = treeRole;
        definition = treeDefinition;
        node = nodeDefinition;

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(HandlePurchase);
        }

        Refresh();
    }

    public void BindAbility(
        SkillTreeManager treeManager,
        GrowthAbilitySlot slot,
        SkillTreeDefinition treeDefinition,
        SkillTreeNodeDefinition nodeDefinition)
    {
        manager = treeManager;
        abilitySlot = slot;
        definition = treeDefinition;
        node = nodeDefinition;
        isAbilityNode = true;

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(HandlePurchase);
        }
        Refresh();
    }

    public void Refresh()
    {
        if (manager == null || definition == null || node == null ||
            RunManager.Instance == null)
        {
            return;
        }

        int rank = isAbilityNode
            ? RunManager.Instance.GrowthState.GetAbility(abilitySlot)
                .GetNodeRank(node.NodeId)
            : RunManager.Instance.GrowthState.GetTree(role).GetNodeRank(node.NodeId);
        bool completed = rank >= node.MaxRank;
        SkillTreeRankDefinition next = completed ? null : node.GetRank(rank);
        string lockReason = isAbilityNode
            ? manager.GetAbilityNodeLockReason(abilitySlot, definition, node)
            : manager.GetNodeLockReason(role, definition, node);
        bool available = isAbilityNode
            ? manager.CanPurchaseAbilityNode(abilitySlot, definition, node)
            : manager.CanPurchaseNode(role, definition, node);

        if (titleText != null) titleText.text = node.DisplayName;
        if (descriptionText != null)
        {
            descriptionText.text = string.Empty;
            descriptionText.gameObject.SetActive(false);
        }
        if (rankText != null) rankText.text = $"{rank}/{node.MaxRank}";
        if (costText != null)
        {
            costText.text = next != null ? $"{next.MasteryPointCost}P" : "MAX";
            costText.gameObject.SetActive(false);
        }
        if (lockText != null)
        {
            lockText.text = completed ? "MAX" : available ? "+" : "잠김";
        }
        if (purchaseButton != null) purchaseButton.interactable = available;
        if (background != null)
            background.color = completed ? completedColor : available ? availableColor : unavailableColor;

        if (isHovered)
        {
            ShowTooltip(rank, next, lockReason);
        }
    }

    private void HandlePurchase()
    {
        if (manager != null && node != null && !SkillTreePanZoom.IsDragging)
        {
            if (isAbilityNode)
                manager.TryPurchaseAbilityNode(abilitySlot, node.NodeId);
            else
                manager.TryPurchaseNode(role, node.NodeId);
            Refresh();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        Refresh();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        SkillTreeTooltip.Instance?.Hide(this);
    }

    private void ShowTooltip(
        int rank,
        SkillTreeRankDefinition next,
        string lockReason)
    {
        if (SkillTreeTooltip.Instance == null) return;

        string currentEffect = isAbilityNode
            ? manager.GetAbilityGameplayEffectDescription(
                abilitySlot, rank > 0 ? node.GetRank(rank - 1) : node.GetRank(0), rank == 0)
            : rank > 0
                ? SkillTreeManager.GetEffectDescription(node.GetRank(rank - 1))
                : "기본 효과";
        string nextEffect = next != null
            ? isAbilityNode
                ? manager.GetAbilityGameplayEffectDescription(abilitySlot, next, false)
                : SkillTreeManager.GetEffectDescription(next)
            : "최대 랭크";
        string prerequisites = GetPrerequisiteText();
        string text = $"<size=21><b>{node.DisplayName}</b></size>\n" +
            $"<size=17>{node.Description}</size>\n\n" +
            $"<size=15><b>랭크</b>  {rank}/{node.MaxRank}\n" +
            $"<b>현재</b>  {currentEffect}\n<b>다음</b>  {nextEffect}";
        if (next != null) text += $"\n<b>비용</b>  {next.MasteryPointCost} 숙련 포인트";
        if (!string.IsNullOrEmpty(prerequisites)) text += $"\n<b>선행</b>  {prerequisites}";
        if (!string.IsNullOrEmpty(lockReason))
            text += $"\n<color=#F2B0A0><b>상태</b>  {lockReason}</color>";
        text += "</size>";
        SkillTreeTooltip.Instance.Show(this, transform as RectTransform, text);
    }

    private string GetPrerequisiteText()
    {
        if (node.Prerequisites.Count == 0) return "없음";
        System.Collections.Generic.List<string> labels = new();
        for (int i = 0; i < node.Prerequisites.Count; i++)
        {
            SkillTreeNodePrerequisite prerequisite = node.Prerequisites[i];
            if (prerequisite == null) continue;
            labels.Add(definition.TryGetNode(prerequisite.NodeId, out SkillTreeNodeDefinition source)
                ? $"{source.DisplayName} {prerequisite.RequiredRank}랭크"
                : prerequisite.NodeId);
        }
        return string.Join(", ", labels);
    }
}
