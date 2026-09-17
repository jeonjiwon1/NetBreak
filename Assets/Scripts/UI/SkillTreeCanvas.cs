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
    private bool movedBehindExternalModal;

    private void Start()
    {
        originalSiblingIndex = transform.GetSiblingIndex();
        manager = SkillTreeManager.Instance;
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleClose);
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

        if (externalModal && !movedBehindExternalModal)
        {
            transform.SetAsFirstSibling();
            movedBehindExternalModal = true;
        }
        else if (!externalModal && movedBehindExternalModal)
        {
            transform.SetSiblingIndex(Mathf.Min(
                originalSiblingIndex,
                transform.parent != null ? transform.parent.childCount - 1 : 0));
            movedBehindExternalModal = false;
        }
    }

    public void ToggleTreeFromUI()
    {
        manager?.ToggleFromUI();
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

        coreBranch?.Refresh(manager, GrowthToolRole.Core);
        partnerBranch?.Refresh(manager, GrowthToolRole.Partner);
        tacticalBranch?.RefreshAbility(manager, GrowthAbilitySlot.TacticalE);
        signatureBranch?.RefreshAbility(manager, GrowthAbilitySlot.SignatureR);
        RefreshAcquisition();
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
        SkillTreeTooltip.HideShared();
    }
}
