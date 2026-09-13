using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeHUDCanvas : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text captureText;
    [SerializeField] private TMP_Text catchRateText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;

    [Header("Encounter")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Gear")]
    [SerializeField] private TMP_Text castNetText;

    [Header("Job")]
    [SerializeField] private TMP_Text jobText;

    [Header("Preparation")]
    [SerializeField] private GameObject preparationPanel;
    [SerializeField] private Button startFishingButton;

    [Header("Announcement")]
    [SerializeField] private GameObject announcementPanel;
    [SerializeField] private TMP_Text announcementText;

    [Header("References")]
    [SerializeField] private FishSpawner fishSpawner;
    [SerializeField] private CastNetController castNet;

    private void Awake()
    {
        if (startFishingButton != null)
        {
            startFishingButton.onClick.AddListener(
                HandleStartFishingClicked
            );
        }
    }

    private void Start()
    {
        RefreshImmediateState();
    }

    private void Update()
    {
        UpdateRunInfo();
        UpdateEncounterInfo();
        UpdateCastNetInfo();
        UpdateJobInfo();

        UpdatePreparationUI();
        UpdateAnnouncementUI();
    }

    // =========================================================
    // RUN INFO
    // =========================================================

    private void UpdateRunInfo()
    {
        RunManager run =
            RunManager.Instance;

        if (run == null)
        {
            return;
        }

        if (goldText != null)
        {
            goldText.text =
                $"골드: {run.CurrentGold}";
        }

        if (captureText != null)
        {
            captureText.text =
                $"포획 수: {run.CapturedFishCount}";
        }

        if (catchRateText != null)
        {
            catchRateText.text =
                $"어획률: {run.CatchRate * 100f:F1}%";
        }

        if (levelText != null)
        {
            levelText.text =
                $"레벨: {run.CurrentLevel}";
        }

        if (expText != null)
        {
            expText.text =
                $"경험치: {run.CurrentExp} / " +
                $"{run.ExpToNextLevel}";
        }
    }

    // =========================================================
    // ENCOUNTER INFO
    // =========================================================

    private void UpdateEncounterInfo()
    {
        if (fishSpawner == null)
        {
            return;
        }

        if (stageText != null)
        {
            if (fishSpawner.HasStarted)
            {
                stageText.text =
                    $"조업 단계: " +
                    $"{fishSpawner.CurrentStageIndex} / " +
                    $"{fishSpawner.TotalStageCount}";
            }
            else
            {
                stageText.text =
                    "조업 단계: 준비";
            }
        }

        if (phaseText != null)
        {
            phaseText.text =
                $"현재 구간: " +
                $"{fishSpawner.CurrentPhaseName}";
        }
    }

    // =========================================================
    // CAST NET
    // =========================================================

    private void UpdateCastNetInfo()
    {
        if (castNetText == null ||
            castNet == null)
        {
            return;
        }

        if (castNet.MaxCharges > 1)
        {
            if (castNet.CurrentCharges ==
                castNet.MaxCharges)
            {
                castNetText.text =
                    $"투망 [E]: " +
                    $"{castNet.CurrentCharges}/" +
                    $"{castNet.MaxCharges}";

                return;
            }

            castNetText.text =
                $"투망 [E]: " +
                $"{castNet.CurrentCharges}/" +
                $"{castNet.MaxCharges} " +
                $"(충전 {castNet.CooldownTimer:F1}초)";

            return;
        }

        if (castNet.IsReady)
        {
            castNetText.text =
                "투망 [E]: 준비 완료";
        }
        else
        {
            castNetText.text =
                $"투망 [E]: " +
                $"{castNet.CooldownTimer:F1}초";
        }
    }

    // =========================================================
    // JOB
    // =========================================================

    private void UpdateJobInfo()
    {
        if (jobText == null)
        {
            return;
        }

        PrototypeJobManager jobManager =
            PrototypeJobManager.Instance;

        if (jobManager == null)
        {
            jobText.text =
                "전직: 초보 어부";

            return;
        }

        jobText.text =
            $"전직: {jobManager.CurrentJobName}";
    }

    // =========================================================
    // PREPARATION
    // =========================================================

    private void UpdatePreparationUI()
    {
        if (preparationPanel == null)
        {
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        bool shouldShow =
            flow != null &&
            flow.IsPreparation;

        if (preparationPanel.activeSelf !=
            shouldShow)
        {
            preparationPanel.SetActive(
                shouldShow
            );
        }
    }

    private void HandleStartFishingClicked()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            !flow.IsPreparation)
        {
            return;
        }

        flow.StartFishing();

        if (preparationPanel != null)
        {
            preparationPanel.SetActive(
                false
            );
        }
    }

    // =========================================================
    // ANNOUNCEMENT
    // =========================================================

    private void UpdateAnnouncementUI()
    {
        if (announcementPanel == null ||
            announcementText == null)
        {
            return;
        }

        bool shouldShow =
            fishSpawner != null &&
            fishSpawner.IsShowingAnnouncement;

        if (announcementPanel.activeSelf !=
            shouldShow)
        {
            announcementPanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow)
        {
            return;
        }

        announcementText.text =
            fishSpawner.AnnouncementText;
    }

    // =========================================================
    // INITIAL STATE
    // =========================================================

    private void RefreshImmediateState()
    {
        UpdatePreparationUI();
        UpdateAnnouncementUI();
    }

    private void OnDestroy()
    {
        if (startFishingButton != null)
        {
            startFishingButton.onClick.RemoveListener(
                HandleStartFishingClicked
            );
        }
    }
}