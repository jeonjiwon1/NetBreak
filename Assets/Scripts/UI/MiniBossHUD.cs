using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniBossHUD : MonoBehaviour
{
    [Header("Mini Boss Panel")]
    [SerializeField] private GameObject miniBossPanel;

    [SerializeField] private TMP_Text miniBossTitleText;

    [SerializeField] private Image resistanceFill;

    [SerializeField] private TMP_Text resistanceText;

    [Header("Reward")]
    [SerializeField] private GameObject rewardPanel;

    [SerializeField] private TMP_Text rewardText;

    [SerializeField] private float rewardMessageDuration = 2.5f;

    private float rewardMessageEndTime;

    private void OnEnable()
    {
        MiniBossController.MiniBossCaptured +=
            HandleMiniBossCaptured;
    }

    private void Start()
    {
        if (miniBossPanel != null)
        {
            miniBossPanel.SetActive(false);
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateMiniBossPanel();
        UpdateRewardPanel();
    }

    // =========================================================
    // MINI BOSS PANEL
    // =========================================================

    private void UpdateMiniBossPanel()
    {
        MiniBossController miniBoss =
            MiniBossController.ActiveMiniBoss;

        bool shouldShow =
            miniBoss != null &&
            miniBoss.gameObject.activeInHierarchy;

        if (miniBossPanel != null &&
            miniBossPanel.activeSelf != shouldShow)
        {
            miniBossPanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow ||
            miniBoss == null)
        {
            return;
        }

        UpdateMiniBossTitle(
            miniBoss
        );

        UpdateResistance(
            miniBoss
        );
    }

    private void UpdateMiniBossTitle(
        MiniBossController miniBoss)
    {
        if (miniBossTitleText == null)
        {
            return;
        }

        string stateText = "";

        if (miniBoss.IsTelegraphing)
        {
            stateText =
                " - 돌진 준비!";
        }
        else if (miniBoss.IsDashing)
        {
            stateText =
                " - 돌진!";
        }

        miniBossTitleText.text =
            $"{miniBoss.DisplayName}{stateText}";
    }

    private void UpdateResistance(
        MiniBossController miniBoss)
    {
        if (resistanceFill != null)
        {
            resistanceFill.fillAmount =
                Mathf.Clamp01(
                    miniBoss.ResistanceRatio
                );
        }

        if (resistanceText != null)
        {
            resistanceText.text =
                $"저항력 " +
                $"{miniBoss.CurrentResistance:F0} / " +
                $"{miniBoss.MaxResistance:F0}";
        }
    }

    // =========================================================
    // REWARD
    // =========================================================

    private void UpdateRewardPanel()
    {
        if (rewardPanel == null)
        {
            return;
        }

        bool shouldShow =
            Time.unscaledTime <
            rewardMessageEndTime;

        if (rewardPanel.activeSelf != shouldShow)
        {
            rewardPanel.SetActive(
                shouldShow
            );
        }
    }

    private void HandleMiniBossCaptured(
        string fishName,
        int gold,
        int exp)
    {
        if (rewardText != null)
        {
            rewardText.text =
                $"{fishName} 포획!  " +
                $"추가 보상 +{gold}G / +{exp} EXP";
        }

        rewardMessageEndTime =
            Time.unscaledTime +
            rewardMessageDuration;

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void OnDisable()
    {
        MiniBossController.MiniBossCaptured -=
            HandleMiniBossCaptured;
    }
}