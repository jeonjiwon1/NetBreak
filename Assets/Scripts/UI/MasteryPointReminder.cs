using TMPro;
using UnityEngine;

public sealed class MasteryPointReminder : MonoBehaviour
{
    [SerializeField] private GameObject reminderRoot;
    [SerializeField] private TMP_Text pointText;
    [SerializeField] private TMP_Text hintText;

    private RunGrowthState growthState;
    private SkillTreeManager skillTreeManager;

    private void OnEnable()
    {
        BindState();
        Refresh();
    }

    private void BindState()
    {
        UnbindState();
        if (RunManager.Instance == null)
        {
            SetVisible(false);
            return;
        }

        growthState = RunManager.Instance.GrowthState;
        skillTreeManager = RunManager.Instance.SkillTree != null
            ? RunManager.Instance.SkillTree
            : SkillTreeManager.Instance;

        if (growthState != null)
        {
            growthState.AvailableMasteryPointsChanged += HandlePointsChanged;
        }

        if (skillTreeManager != null)
        {
            skillTreeManager.OpenStateChanged += HandleTreeOpenChanged;
        }
    }

    private void HandlePointsChanged(int availablePoints)
    {
        Refresh(availablePoints);
    }

    private void HandleTreeOpenChanged(bool isOpen)
    {
        Refresh();
    }

    private void Refresh()
    {
        int availablePoints = growthState != null
            ? growthState.AvailableMasteryPoints
            : 0;
        Refresh(availablePoints);
    }

    private void Refresh(int availablePoints)
    {
        bool treeOpen = skillTreeManager != null && skillTreeManager.IsOpen;
        bool visible = availablePoints > 0 && !treeOpen;
        if (pointText != null)
        {
            pointText.text = $"숙련 포인트 {availablePoints}개 사용 가능!";
        }

        if (hintText != null)
        {
            hintText.text = "Tab — 성장 관리 열기";
        }

        SetVisible(visible);
    }

    private void SetVisible(bool visible)
    {
        if (reminderRoot != null && reminderRoot.activeSelf != visible)
        {
            reminderRoot.SetActive(visible);
        }
    }

    private void UnbindState()
    {
        if (growthState != null)
        {
            growthState.AvailableMasteryPointsChanged -= HandlePointsChanged;
        }

        if (skillTreeManager != null)
        {
            skillTreeManager.OpenStateChanged -= HandleTreeOpenChanged;
        }

        growthState = null;
        skillTreeManager = null;
    }

    private void OnDisable()
    {
        UnbindState();
    }
}
