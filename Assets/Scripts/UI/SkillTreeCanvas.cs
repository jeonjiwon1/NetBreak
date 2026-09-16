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

    [Header("Acquisition")]
    [SerializeField] private GameObject acquisitionPanel;
    [SerializeField] private TMP_Text acquisitionTitleText;
    [SerializeField] private AcquisitionChoiceView[] acquisitionChoices =
        Array.Empty<AcquisitionChoiceView>();

    private SkillTreeManager manager;

    private void Start()
    {
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

        Refresh();
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
        RefreshAcquisition();
    }

    private void RefreshAcquisition()
    {
        bool show = manager.IsMandatoryAcquisition;
        if (acquisitionPanel != null && acquisitionPanel.activeSelf != show)
        {
            acquisitionPanel.SetActive(show);
        }

        if (!show)
        {
            return;
        }

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
}
