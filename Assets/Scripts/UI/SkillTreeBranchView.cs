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
    [Header("Compact Graph")]
    [SerializeField] private Color connectionLockedColor = new(0.22f, 0.25f, 0.3f, 1f);
    [SerializeField] private Color connectionReadyColor = new(0.25f, 0.65f, 0.85f, 1f);
    [SerializeField] private float connectionThickness = 4f;
    [SerializeField] private Vector2 automaticSpacing = new(150f, 125f);

    private readonly List<SkillTreeNodeView> nodeViews = new();
    private readonly List<Connection> connections = new();
    private SkillTreeDefinition shownDefinition;
    private GrowthToolRole shownRole;
    private GrowthAbilitySlot shownAbilitySlot;
    private bool showingAbility;

    private sealed class Connection
    {
        public Image Image;
        public string PrerequisiteId;
        public int RequiredRank;
    }

    public void Refresh(SkillTreeManager manager, GrowthToolRole role)
    {
        if (manager == null || RunManager.Instance == null) return;
        ConfigureCompactRoot();
        RunSkillTreeProgress progress = RunManager.Instance.GrowthState.GetTree(role);
        SkillTreeDefinition definition = manager.GetDefinition(role);
        if (branchTitleText != null)
            branchTitleText.text = role == GrowthToolRole.Core
                ? "주력 스킬 트리 (Q)" : "보조 스킬 트리 (W)";
        RefreshRoot(manager, role, progress);
        if (shownDefinition != definition || shownRole != role || showingAbility)
            RebuildNodes(manager, role, definition);
        RefreshViews();
    }

    public void RefreshAbility(SkillTreeManager manager, GrowthAbilitySlot slot)
    {
        if (manager == null || RunManager.Instance == null) return;
        ConfigureCompactRoot();
        RunGrowthAbilityState progress = RunManager.Instance.GrowthState.GetAbility(slot);
        SkillTreeDefinition definition = manager.GetAbilityDefinition(slot);
        if (branchTitleText != null)
            branchTitleText.text = slot == GrowthAbilitySlot.TacticalE
                ? "전술 스킬 강화 (E)" : "궁극기 강화 (R)";
        if (rootButton != null)
        {
            rootButton.onClick.RemoveAllListeners();
            rootButton.interactable = false;
        }
        if (rootTitleText != null)
        {
            string shownAbilityId = !string.IsNullOrEmpty(progress.EquippedAbilityId)
                ? progress.EquippedAbilityId
                : definition != null ? definition.AbilityId : string.Empty;
            rootTitleText.text = progress.IsUnlocked && !string.IsNullOrEmpty(shownAbilityId)
                ? GetAbilityName(shownAbilityId)
                : "잠금";
        }
        if (rootStatusText != null)
            rootStatusText.text = progress.IsUnlocked ? "해금 완료"
                : slot == GrowthAbilitySlot.TacticalE
                    ? "미니보스 포획 후 선택" : "보스 포획 후 해금";
        if (shownDefinition != definition || shownAbilitySlot != slot || !showingAbility)
            RebuildAbilityNodes(manager, slot, definition);
        RefreshViews();
    }

    private void RefreshRoot(
        SkillTreeManager manager, GrowthToolRole role, RunSkillTreeProgress progress)
    {
        bool acquired = progress.Tool != ToolId.None;
        bool active = manager.IsMandatoryAcquisition && manager.MandatoryRole == role;
        if (rootTitleText != null)
            rootTitleText.text = acquired
                ? SkillTreeManager.GetToolName(progress.Tool)
                : active ? "선택" : "잠금";
        if (rootStatusText != null)
            rootStatusText.text = acquired ? "획득 완료" : active
                ? $"{manager.AcquisitionMasteryCost}P · 선택"
                : role == GrowthToolRole.Core ? "Lv2에서 해금" : "Lv3에서 해금";
        if (rootButton != null)
        {
            rootButton.onClick.RemoveAllListeners();
            rootButton.onClick.AddListener(() => manager.FocusAcquisitionRoot(role));
            rootButton.interactable = active && manager.CanInteract;
        }
    }

    private void ConfigureCompactRoot()
    {
        if (rootButton == null) return;
        RectTransform rootRect = rootButton.transform as RectTransform;
        rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(112f, 88f);
        LayoutElement element = rootButton.GetComponent<LayoutElement>() ??
            rootButton.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = 112f;
        element.preferredHeight = 88f;
        element.minWidth = 96f;
        element.minHeight = 80f;
        element.flexibleWidth = 0f;
        element.flexibleHeight = 0f;

        if (rootTitleText != null)
        {
            rootTitleText.fontSize = 20f;
            rootTitleText.enableAutoSizing = false;
            rootTitleText.alignment = TextAlignmentOptions.Center;
        }
        if (rootStatusText != null)
        {
            rootStatusText.fontSize = 13f;
            rootStatusText.enableAutoSizing = false;
            rootStatusText.alignment = TextAlignmentOptions.Center;
        }

        if (rootButton.transform.parent != null &&
            rootButton.transform.parent.TryGetComponent(out VerticalLayoutGroup layout))
        {
            layout.childForceExpandWidth = false;
            layout.childAlignment = TextAnchor.UpperCenter;
        }
    }

    private void RebuildNodes(
        SkillTreeManager manager, GrowthToolRole role, SkillTreeDefinition definition)
    {
        ClearGraph();
        shownDefinition = definition;
        shownRole = role;
        showingAbility = false;
        if (definition == null || nodeContainer == null || nodePrefab == null) return;
        PrepareContainer();
        IReadOnlyList<SkillTreeNodeDefinition> nodes = definition.Nodes;
        Dictionary<string, RectTransform> rects = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            SkillTreeNodeView view = Instantiate(nodePrefab, nodeContainer);
            PrepareNode(view, nodes, i);
            view.Bind(manager, role, definition, nodes[i]);
            nodeViews.Add(view);
            rects[nodes[i].NodeId] = view.transform as RectTransform;
        }
        BuildConnections(definition, rects);
    }

    private void RebuildAbilityNodes(
        SkillTreeManager manager, GrowthAbilitySlot slot, SkillTreeDefinition definition)
    {
        ClearGraph();
        shownDefinition = definition;
        shownAbilitySlot = slot;
        showingAbility = true;
        if (definition == null || nodeContainer == null || nodePrefab == null) return;
        PrepareContainer();
        IReadOnlyList<SkillTreeNodeDefinition> nodes = definition.Nodes;
        Dictionary<string, RectTransform> rects = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            SkillTreeNodeView view = Instantiate(nodePrefab, nodeContainer);
            PrepareNode(view, nodes, i);
            view.BindAbility(manager, slot, definition, nodes[i]);
            nodeViews.Add(view);
            rects[nodes[i].NodeId] = view.transform as RectTransform;
        }
        BuildConnections(definition, rects);
    }

    private void PrepareContainer()
    {
        foreach (LayoutGroup layout in nodeContainer.GetComponents<LayoutGroup>())
            layout.enabled = false;
        ContentSizeFitter fitter = nodeContainer.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;
        LayoutElement element = nodeContainer.GetComponent<LayoutElement>() ??
            nodeContainer.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 330f;
        element.minHeight = 260f;
        element.flexibleHeight = 0f;
        nodeContainer.sizeDelta = new Vector2(520f, 330f);
        nodeContainer.anchorMin = nodeContainer.anchorMax = new Vector2(0.5f, 0.5f);
        nodeContainer.pivot = new Vector2(0.5f, 0.5f);
    }

    private void PrepareNode(
        SkillTreeNodeView view, IReadOnlyList<SkillTreeNodeDefinition> nodes, int index)
    {
        RectTransform rect = view.transform as RectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(112f, 112f);
        LayoutElement element = view.GetComponent<LayoutElement>();
        if (element != null) element.enabled = false;
        Vector2 position = nodes[index].GraphPosition;
        if (position == Vector2.zero && nodes.Count > 1)
            position = CalculateAutomaticPosition(nodes, index);
        rect.anchoredPosition = position;
        view.gameObject.SetActive(true);
    }

    private Vector2 CalculateAutomaticPosition(
        IReadOnlyList<SkillTreeNodeDefinition> nodes, int index)
    {
        int depth = GetDepth(nodes, nodes[index]);
        int sameDepthBefore = 0;
        for (int i = 0; i < index; i++)
            if (GetDepth(nodes, nodes[i]) == depth) sameDepthBefore++;
        return new Vector2(
            (sameDepthBefore - 0.5f) * automaticSpacing.x,
            95f - depth * automaticSpacing.y);
    }

    private static int GetDepth(
        IReadOnlyList<SkillTreeNodeDefinition> nodes, SkillTreeNodeDefinition node)
    {
        int depth = 0;
        HashSet<string> visited = new();
        while (node != null && node.Prerequisites.Count > 0 && visited.Add(node.NodeId))
        {
            string id = node.Prerequisites[0].NodeId;
            node = null;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].NodeId == id) { node = nodes[i]; break; }
            if (node != null) depth++;
        }
        return depth;
    }

    private void BuildConnections(
        SkillTreeDefinition definition, Dictionary<string, RectTransform> rects)
    {
        IReadOnlyList<SkillTreeNodeDefinition> nodes = definition.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (!rects.TryGetValue(nodes[i].NodeId, out RectTransform child)) continue;
            for (int p = 0; p < nodes[i].Prerequisites.Count; p++)
            {
                SkillTreeNodePrerequisite prerequisite = nodes[i].Prerequisites[p];
                if (prerequisite == null ||
                    !rects.TryGetValue(prerequisite.NodeId, out RectTransform parent)) continue;
                Image line = CreateLine(parent.anchoredPosition, child.anchoredPosition);
                connections.Add(new Connection
                {
                    Image = line,
                    PrerequisiteId = prerequisite.NodeId,
                    RequiredRank = prerequisite.RequiredRank
                });
            }
        }
        for (int i = 0; i < nodeViews.Count; i++) nodeViews[i].transform.SetAsLastSibling();
    }

    private Image CreateLine(Vector2 from, Vector2 to)
    {
        GameObject lineObject = new("PrerequisiteConnection", typeof(RectTransform), typeof(Image));
        lineObject.transform.SetParent(nodeContainer, false);
        RectTransform rect = (RectTransform)lineObject.transform;
        Vector2 delta = to - from;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = from;
        rect.sizeDelta = new Vector2(delta.magnitude, connectionThickness);
        rect.localEulerAngles = new Vector3(0f, 0f,
            Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        Image image = lineObject.GetComponent<Image>();
        image.color = connectionLockedColor;
        image.raycastTarget = false;
        return image;
    }

    private void RefreshViews()
    {
        for (int i = 0; i < nodeViews.Count; i++) nodeViews[i]?.Refresh();
        if (RunManager.Instance == null) return;
        for (int i = 0; i < connections.Count; i++)
        {
            Connection connection = connections[i];
            int rank = showingAbility
                ? RunManager.Instance.GrowthState.GetAbility(shownAbilitySlot)
                    .GetNodeRank(connection.PrerequisiteId)
                : RunManager.Instance.GrowthState.GetTree(shownRole)
                    .GetNodeRank(connection.PrerequisiteId);
            if (connection.Image != null)
                connection.Image.color = rank >= connection.RequiredRank
                    ? connectionReadyColor : connectionLockedColor;
        }
    }

    private void ClearGraph()
    {
        for (int i = 0; i < nodeViews.Count; i++)
            if (nodeViews[i] != null) Destroy(nodeViews[i].gameObject);
        for (int i = 0; i < connections.Count; i++)
            if (connections[i].Image != null) Destroy(connections[i].Image.gameObject);
        nodeViews.Clear();
        connections.Clear();
    }

    private static string GetAbilityName(string abilityId) => abilityId switch
    {
        TacticalSkillManager.RapidReelingAbilityId => "급속 릴링",
        TacticalSkillManager.EmergencyLockdownAbilityId => "긴급 봉쇄",
        TacticalSkillManager.EmergencyCastNetAbilityId => "비상 투망",
        TacticalSkillManager.OverbaitingAbilityId => "과잉 집어",
        TacticalSkillManager.FocusedOperationAbilityId => "집중 조업",
        SignatureSkillManager.FishingGroundCrossingAbilityId => "어장 대횡단",
        SignatureSkillManager.CrossLockdownAbilityId => "교차 봉쇄",
        SignatureSkillManager.HeavenlyNetAbilityId => "천망",
        _ => "?"
    };
}
