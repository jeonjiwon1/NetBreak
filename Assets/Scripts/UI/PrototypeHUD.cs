using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeHUD : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;

    [Header("World Feedback Canvas")]
    [SerializeField] private RectTransform worldFeedbackRoot;

    [SerializeField] private TMP_Text netCostText;
    [SerializeField] private TMP_Text castNetTargetText;
    [SerializeField] private TMP_Text castNetCatchText;

    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;

    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultDescriptionText;
    [SerializeField] private TMP_Text resultRankText;

    [SerializeField] private TMP_Text resultCatchRateText;
    [SerializeField] private TMP_Text resultCaptureCountText;
    [SerializeField] private TMP_Text resultGoldText;
    [SerializeField] private TMP_Text resultJobText;
    [SerializeField] private TMP_Text resultBossText;

    [SerializeField] private Button restartButton;

    private bool resultShown;

    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(
                HandleRestartClicked
            );
        }
    }

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        SetWorldFeedbackVisible(
            false,
            false,
            false
        );
    }

    private void Update()
    {
        UpdateResultPanel();
        UpdateWorldFeedback();
    }

    // =========================================================
    // WORLD FEEDBACK
    // =========================================================

    private void UpdateWorldFeedback()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow != null &&
            flow.IsGameEnded)
        {
            SetWorldFeedbackVisible(
                false,
                false,
                false
            );

            return;
        }

        UpdateNetCost();
        UpdateCastNetTarget();
        UpdateCastNetCatch();
    }

    private void UpdateNetCost()
    {
        bool shouldShow =
            netPlacement != null &&
            netPlacement.IsDragging;

        SetTextActive(
            netCostText,
            shouldShow
        );

        if (!shouldShow ||
            netCostText == null)
        {
            return;
        }

        netCostText.text =
            $"그물 설치 비용: " +
            $"{netPlacement.CurrentPlacementCost}G";
    }

    private void UpdateCastNetTarget()
    {
        bool shouldShow =
            castNet != null &&
            castNet.IsAiming;

        SetTextActive(
            castNetTargetText,
            shouldShow
        );

        if (!shouldShow ||
            castNetTargetText == null)
        {
            return;
        }

        castNetTargetText.text =
            $"범위 내: " +
            $"{castNet.CurrentTargetCount}마리";

        SetWorldFeedbackPosition(
            castNetTargetText.rectTransform,
            castNet.CurrentAimPosition,
            new Vector2(
                0f,
                -45f
            )
        );
    }

    private void UpdateCastNetCatch()
    {
        bool shouldShow =
            castNet != null &&
            castNet.IsShowingCatchFeedback;

        SetTextActive(
            castNetCatchText,
            shouldShow
        );

        if (!shouldShow ||
            castNetCatchText == null)
        {
            return;
        }

        castNetCatchText.text =
            $"+{castNet.LastCapturedCount}마리 포획!";

        SetWorldFeedbackPosition(
            castNetCatchText.rectTransform,
            castNet.LastCastPosition,
            new Vector2(
                0f,
                50f
            )
        );
    }

    private void SetWorldFeedbackPosition(
        RectTransform target,
        Vector3 worldPosition,
        Vector2 offset)
    {
        if (target == null ||
            worldFeedbackRoot == null)
        {
            return;
        }

        Camera mainCamera =
            Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(
                worldPosition
            );

        if (screenPosition.z < 0f)
        {
            return;
        }

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                worldFeedbackRoot,
                screenPosition,
                null,
                out Vector2 localPoint
            ))
        {
            return;
        }

        target.anchoredPosition =
            localPoint +
            offset;
    }

    private void SetWorldFeedbackVisible(
        bool showNetCost,
        bool showTarget,
        bool showCatch)
    {
        SetTextActive(
            netCostText,
            showNetCost
        );

        SetTextActive(
            castNetTargetText,
            showTarget
        );

        SetTextActive(
            castNetCatchText,
            showCatch
        );
    }

    private void SetTextActive(
        TMP_Text target,
        bool shouldShow)
    {
        if (target == null)
        {
            return;
        }

        if (target.gameObject.activeSelf !=
            shouldShow)
        {
            target.gameObject.SetActive(
                shouldShow
            );
        }
    }

    // =========================================================
    // RESULT CANVAS
    // =========================================================

    private void UpdateResultPanel()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        bool shouldShow =
            flow != null &&
            flow.IsGameEnded;

        if (resultPanel != null &&
            resultPanel.activeSelf != shouldShow)
        {
            resultPanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow ||
            flow == null)
        {
            resultShown = false;
            return;
        }

        if (resultShown)
        {
            return;
        }

        RefreshResultText(
            flow
        );

        resultShown = true;
    }

    private void RefreshResultText(
        PrototypeGameFlowManager flow)
    {
        RunManager run =
            RunManager.Instance;

        if (run == null)
        {
            return;
        }

        if (resultTitleText != null)
        {
            resultTitleText.text =
                flow.ResultTitle;
        }

        if (resultDescriptionText != null)
        {
            resultDescriptionText.text =
                flow.ResultDescription;
        }

        if (resultRankText != null)
        {
            resultRankText.text =
                $"어획 등급  {flow.CatchRank}";
        }

        if (resultCatchRateText != null)
        {
            resultCatchRateText.text =
                $"최종 어획률: " +
                $"{run.CatchRate * 100f:F1}%";
        }

        if (resultCaptureCountText != null)
        {
            resultCaptureCountText.text =
                $"포획 수: " +
                $"{run.CapturedFishCount}마리";
        }

        if (resultGoldText != null)
        {
            resultGoldText.text =
                $"보유 골드: " +
                $"{run.CurrentGold}G";
        }

        string jobName =
            "초보 어부";

        if (PrototypeJobManager.Instance != null)
        {
            jobName =
                PrototypeJobManager.Instance
                    .CurrentJobName;
        }

        if (resultJobText != null)
        {
            resultJobText.text =
                $"전직: {jobName}";
        }

        if (resultBossText != null)
        {
            BossEncounterController boss =
                BossEncounterController.Instance;

            if (boss == null)
            {
                resultBossText.text = "";
            }
            else if (flow.IsSuccess)
            {
                resultBossText.text =
                    $"보스 포획: " +
                    $"{boss.CurrentPass}차 회유";
            }
            else
            {
                resultBossText.text =
                    $"보스 도주: " +
                    $"{boss.CurrentPass}차 회유";
            }
        }
    }

    // =========================================================
    // RESTART
    // =========================================================

    private void HandleRestartClicked()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null)
        {
            return;
        }

        flow.RestartPrototype();
    }

    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(
                HandleRestartClicked
            );
        }
    }
}