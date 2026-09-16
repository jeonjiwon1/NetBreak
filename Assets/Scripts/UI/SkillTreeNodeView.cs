using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillTreeNodeView : MonoBehaviour
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

    public void Refresh()
    {
        if (manager == null || definition == null || node == null ||
            RunManager.Instance == null)
        {
            return;
        }

        RunSkillTreeProgress progress = RunManager.Instance.GrowthState.GetTree(role);
        int rank = progress.GetNodeRank(node.NodeId);
        bool completed = rank >= node.MaxRank;
        SkillTreeRankDefinition next = completed ? null : node.GetRank(rank);
        string lockReason = manager.GetNodeLockReason(role, definition, node);
        bool available = manager.CanPurchaseNode(role, definition, node);

        if (titleText != null) titleText.text = node.DisplayName;
        if (descriptionText != null)
        {
            string nextEffect = SkillTreeManager.GetEffectDescription(next);
            descriptionText.text = string.IsNullOrEmpty(nextEffect)
                ? node.Description
                : $"{node.Description}\n\n다음 효과: {nextEffect}";
        }
        if (rankText != null) rankText.text = $"현재 랭크 {rank}/{node.MaxRank}";
        if (costText != null)
            costText.text = next != null ? $"다음 비용: {next.MasteryPointCost}P" : "구매 완료";
        if (lockText != null) lockText.text = lockReason;
        if (purchaseButton != null) purchaseButton.interactable = available;
        if (background != null)
            background.color = completed ? completedColor : available ? availableColor : unavailableColor;
    }

    private void HandlePurchase()
    {
        if (manager != null && node != null)
        {
            manager.TryPurchaseNode(role, node.NodeId);
            Refresh();
        }
    }
}
