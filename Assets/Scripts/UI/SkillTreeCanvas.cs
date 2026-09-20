using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SkillTreeCanvas : MonoBehaviour
{
    [Serializable]
    private sealed class AcquisitionChoiceView
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;

        public void Bind(int index, SkillTreeManager manager)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => manager.SelectAcquisitionChoice(index));
            }
        }

        public void Refresh(int index, SkillTreeManager manager)
        {
            ToolId tool = manager.GetAcquisitionChoice(index);
            bool visible = tool != ToolId.None;
            if (button != null)
            {
                button.gameObject.SetActive(visible);
                button.interactable = visible && manager.CanInteract;
            }

            if (label != null)
            {
                label.text = visible
                    ? $"{SkillTreeManager.GetToolName(tool)}\n\n이 도구를 선택하고 {manager.AcquisitionMasteryCost} 숙련 포인트를 사용합니다."
                    : string.Empty;
            }
        }
    }

    [Header("Root")]
    [SerializeField] private GameObject treePanel;
    [SerializeField] private TMP_Text masteryPointText;
    [SerializeField] private TMP_Text noticeText;
    [SerializeField] private Button closeButton;

    [Header("Growth Management Pages")]
    [SerializeField] private GameObject navigationRoot;
    [SerializeField] private Button skillTreeTabButton;
    [SerializeField] private Button itemTabButton;
    [SerializeField] private GameObject skillTreePage;
    [SerializeField] private GameObject itemPage;
    [SerializeField] private GrowthItemPage itemPageView;

    [Header("Branches")]
    [SerializeField] private SkillTreeBranchView coreBranch;
    [SerializeField] private SkillTreeBranchView partnerBranch;
    [SerializeField] private SkillTreeBranchView tacticalBranch;
    [SerializeField] private SkillTreeBranchView signatureBranch;

    [Header("Acquisition")]
    [SerializeField] private GameObject acquisitionBlocker;
    [SerializeField] private GameObject acquisitionPanel;
    [SerializeField] private TMP_Text acquisitionTitleText;
    [SerializeField] private AcquisitionChoiceView[] acquisitionChoices =
        Array.Empty<AcquisitionChoiceView>();

    private SkillTreeManager manager;
    private int originalSiblingIndex;
    private bool hasRuntimeSiblingOverride;
    private GrowthManagementPage currentPage = GrowthManagementPage.SkillTree;

    public GrowthManagementPage CurrentPage => currentPage;

    private void Start()
    {
        originalSiblingIndex = transform.GetSiblingIndex();
        manager = SkillTreeManager.Instance;
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleClose);
        }
        if (skillTreeTabButton != null)
        {
            skillTreeTabButton.onClick.AddListener(ShowSkillTreePage);
        }
        if (itemTabButton != null)
        {
            itemTabButton.onClick.AddListener(ShowItemPage);
        }

        BindChoiceButtons();
        Refresh();
    }

    private void Update()
    {
        if (manager == null)
        {
            manager = SkillTreeManager.Instance;
            BindChoiceButtons();
        }

        UpdateExternalModalSorting();
        Refresh();
    }

    private void UpdateExternalModalSorting()
    {
        bool externalModal =
            (TacticalSkillManager.Instance != null && TacticalSkillManager.Instance.IsChoosing) ||
            (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsChoosingTool) ||
            (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
            (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
            ItemRewardManager.IsSelectionPendingOrActive;

        if (transform.parent == null)
        {
            return;
        }

        bool treeOpen = manager != null && manager.IsOpen;
        if (externalModal)
        {
            SetRuntimeSiblingIndex(0);
            return;
        }

        if (treeOpen)
        {
            SetRuntimeSiblingIndex(transform.parent.childCount - 1);
            return;
        }

        RestoreOriginalSiblingIndex();
    }

    private void SetRuntimeSiblingIndex(int siblingIndex)
    {
        if (transform.GetSiblingIndex() != siblingIndex)
        {
            transform.SetSiblingIndex(siblingIndex);
        }

        hasRuntimeSiblingOverride = true;
    }

    private void RestoreOriginalSiblingIndex()
    {
        if (!hasRuntimeSiblingOverride || transform.parent == null)
        {
            return;
        }

        transform.SetSiblingIndex(Mathf.Min(
            originalSiblingIndex,
            transform.parent.childCount - 1));
        hasRuntimeSiblingOverride = false;
    }

    public void ToggleTreeFromUI()
    {
        manager?.ToggleFromUI();
    }

    public void ShowSkillTreePage()
    {
        TryShowPage(GrowthManagementPage.SkillTree);
    }

    public void ShowItemPage()
    {
        TryShowPage(GrowthManagementPage.Item);
    }

    private void HandleClose()
    {
        manager?.TryClose();
    }

    private void BindChoiceButtons()
    {
        if (manager == null)
        {
            return;
        }

        for (int i = 0; i < acquisitionChoices.Length; i++)
        {
            acquisitionChoices[i]?.Bind(i, manager);
        }
    }

    private void Refresh()
    {
        bool show = manager != null && manager.IsOpen;
        if (treePanel != null && treePanel.activeSelf != show)
        {
            treePanel.SetActive(show);
        }

        if (!show || manager == null)
        {
            SkillTreeTooltip.HideShared();
            itemPageView?.SetPageVisible(false);
            return;
        }

        RunGrowthState growth = RunManager.Instance != null
            ? RunManager.Instance.GrowthState
            : null;
        if (masteryPointText != null)
        {
            masteryPointText.text = growth != null
                ? $"숙련 포인트: {growth.AvailableMasteryPoints}"
                : "숙련 포인트: -";
        }

        if (noticeText != null)
        {
            noticeText.text = manager.Notice;
        }

        if (closeButton != null)
        {
            closeButton.interactable = !manager.IsMandatoryAcquisition;
        }

        RefreshPages(growth);

        if (currentPage == GrowthManagementPage.SkillTree)
        {
            coreBranch?.Refresh(manager, GrowthToolRole.Core);
            partnerBranch?.Refresh(manager, GrowthToolRole.Partner);
            tacticalBranch?.RefreshAbility(manager, GrowthAbilitySlot.TacticalE);
            signatureBranch?.RefreshAbility(manager, GrowthAbilitySlot.SignatureR);
        }
        RefreshAcquisition();
    }

    private void TryShowPage(GrowthManagementPage page)
    {
        if (manager == null || !manager.CanInteract ||
            manager.IsMandatoryAcquisition)
        {
            return;
        }

        RunGrowthState growth = RunManager.Instance?.GrowthState;
        if (growth == null || !growth.TrySetGrowthManagementPage(page))
        {
            return;
        }

        currentPage = page;
        SkillTreeTooltip.HideShared();
        RefreshPages(growth);
    }

    private void RefreshPages(RunGrowthState growth)
    {
        GrowthManagementPage requested = manager.IsMandatoryAcquisition
            ? GrowthManagementPage.SkillTree
            : growth?.LastGrowthManagementPage ?? GrowthManagementPage.SkillTree;
        currentPage = requested;
        bool showSkillTree = requested == GrowthManagementPage.SkillTree;

        if (navigationRoot != null)
        {
            navigationRoot.SetActive(true);
        }
        if (skillTreePage != null && skillTreePage.activeSelf != showSkillTree)
        {
            skillTreePage.SetActive(showSkillTree);
        }
        if (itemPage != null && itemPage.activeSelf == showSkillTree)
        {
            itemPage.SetActive(!showSkillTree);
        }
        if (skillTreeTabButton != null)
        {
            skillTreeTabButton.interactable =
                !showSkillTree && !manager.IsMandatoryAcquisition && manager.CanInteract;
        }
        if (itemTabButton != null)
        {
            itemTabButton.interactable =
                showSkillTree && !manager.IsMandatoryAcquisition && manager.CanInteract;
        }

        itemPageView?.SetPageVisible(!showSkillTree);
        if (!showSkillTree)
        {
            SkillTreeTooltip.HideShared();
        }
    }

    private void RefreshAcquisition()
    {
        bool show = manager.IsMandatoryAcquisition;
        if (acquisitionBlocker != null && acquisitionBlocker.activeSelf != show)
        {
            acquisitionBlocker.SetActive(show);
        }
        if (acquisitionPanel != null && acquisitionPanel.activeSelf != show)
        {
            acquisitionPanel.SetActive(show);
        }

        SetBranchInteraction(!show);
        if (!show)
        {
            return;
        }

        SkillTreeTooltip.HideShared();
        acquisitionBlocker?.transform.SetAsLastSibling();
        acquisitionPanel?.transform.SetAsLastSibling();

        if (acquisitionTitleText != null)
        {
            acquisitionTitleText.text = manager.MandatoryRole == GrowthToolRole.Core
                ? "주력 도구 선택"
                : "보조 도구 선택";
        }

        for (int i = 0; i < acquisitionChoices.Length; i++)
        {
            acquisitionChoices[i]?.Refresh(i, manager);
        }
    }

    private void SetBranchInteraction(bool enabled)
    {
        SetInteraction(coreBranch, enabled);
        SetInteraction(partnerBranch, enabled);
        SetInteraction(tacticalBranch, enabled);
        SetInteraction(signatureBranch, enabled);
    }

    private static void SetInteraction(SkillTreeBranchView branch, bool enabled)
    {
        if (branch == null) return;
        CanvasGroup group = branch.GetComponent<CanvasGroup>() ??
            branch.gameObject.AddComponent<CanvasGroup>();
        group.interactable = enabled;
        group.blocksRaycasts = enabled;
    }

    private void OnDisable()
    {
        RestoreOriginalSiblingIndex();
        SkillTreeTooltip.HideShared();
        itemPageView?.SetPageVisible(false);
    }
}
