using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillTreeBranchView : MonoBehaviour
{
    [SerializeField] private TMP_Text branchTitleText;
    [SerializeField] private Button rootButton;
    [SerializeField] private TMP_Text rootTitleText;
    [SerializeField] private TMP_Text rootStatusText;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private SkillTreeNodeView nodePrefab;

    private readonly List<SkillTreeNodeView> nodeViews = new();
    private SkillTreeDefinition shownDefinition;
    private GrowthToolRole shownRole;

    public void Refresh(SkillTreeManager manager, GrowthToolRole role)
    {
        if (manager == null || RunManager.Instance == null)
        {
            return;
        }

        RunSkillTreeProgress progress = RunManager.Instance.GrowthState.GetTree(role);
        SkillTreeDefinition definition = manager.GetDefinition(role);

        if (branchTitleText != null)
        {
            branchTitleText.text = role == GrowthToolRole.Core
                ? "주력 스킬 트리"
                : "보조 스킬 트리";
        }

        RefreshRoot(manager, role, progress);
        if (shownDefinition != definition || shownRole != role)
        {
            RebuildNodes(manager, role, definition);
        }

        for (int i = 0; i < nodeViews.Count; i++)
        {
            nodeViews[i]?.Refresh();
        }
    }

    private void RefreshRoot(
        SkillTreeManager manager,
        GrowthToolRole role,
        RunSkillTreeProgress progress)
    {
        bool acquired = progress.Tool != ToolId.None;
        bool active = manager.IsMandatoryAcquisition && manager.MandatoryRole == role;
        if (rootTitleText != null)
        {
            rootTitleText.text = acquired
                ? SkillTreeManager.GetToolName(progress.Tool)
                : "?";
        }

        if (rootStatusText != null)
        {
            rootStatusText.text = acquired
                ? "획득 완료"
                : active
                    ? $"해금 가능 · 비용 {manager.AcquisitionMasteryCost}P"
                    : role == GrowthToolRole.Core
                        ? "Lv2에서 해금"
                        : "Lv3에서 해금";
        }

        if (rootButton != null)
        {
            rootButton.onClick.RemoveAllListeners();
            rootButton.onClick.AddListener(() => manager.FocusAcquisitionRoot(role));
            rootButton.interactable = active && manager.CanInteract;
        }
    }

    private void RebuildNodes(
        SkillTreeManager manager,
        GrowthToolRole role,
        SkillTreeDefinition definition)
    {
        for (int i = 0; i < nodeViews.Count; i++)
        {
            if (nodeViews[i] != null)
            {
                Destroy(nodeViews[i].gameObject);
            }
        }

        nodeViews.Clear();
        shownDefinition = definition;
        shownRole = role;
        if (definition == null || nodeContainer == null || nodePrefab == null)
        {
            return;
        }

        IReadOnlyList<SkillTreeNodeDefinition> nodes = definition.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            SkillTreeNodeView view = Instantiate(nodePrefab, nodeContainer);
            view.Bind(manager, role, definition, nodes[i]);
            nodeViews.Add(view);
        }
    }
}
