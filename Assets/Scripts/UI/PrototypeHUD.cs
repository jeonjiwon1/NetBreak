using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeHUD : MonoBehaviour
{
    [Header("World Feedback")]
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;

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

    private GUIStyle centerStyle;

    private bool resultShown;

    private void Awake()
    {
        centerStyle =
            new GUIStyle();

        centerStyle.fontSize = 30;

        centerStyle.alignment =
            TextAnchor.MiddleCenter;

        centerStyle.normal.textColor =
            Color.white;

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
    }

    private void Update()
    {
        UpdateResultPanel();
    }

    private void OnGUI()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        // 결과 화면은 Canvas가 담당한다.
        if (flow != null &&
            flow.IsGameEnded)
        {
            return;
        }

        DrawNetCost();
        DrawCastNetInfo();
        DrawLegacyFinalFishing();
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

        // -----------------------------------------------------
        // TITLE / DESCRIPTION
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // RANK
        // -----------------------------------------------------

        if (resultRankText != null)
        {
            resultRankText.text =
                $"어획 등급  {flow.CatchRank}";
        }

        // -----------------------------------------------------
        // RUN RESULT
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // JOB
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // BOSS RESULT
        // -----------------------------------------------------

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

    // =========================================================
    // NET COST
    // =========================================================

    private void DrawNetCost()
    {
        if (netPlacement == null ||
            !netPlacement.IsDragging)
        {
            return;
        }

        GUI.Label(
            new Rect(
                20,
                300,
                600,
                40
            ),
            $"그물 설치 비용: " +
            $"{netPlacement.CurrentPlacementCost}G"
        );
    }

    // =========================================================
    // CAST NET WORLD FEEDBACK
    // =========================================================

    private void DrawCastNetInfo()
    {
        if (castNet == null)
        {
            return;
        }

        Camera mainCamera =
            Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        if (castNet.IsAiming)
        {
            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(
                    castNet.CurrentAimPosition
                );

            float guiY =
                Screen.height -
                screenPosition.y;

            GUI.Label(
                new Rect(
                    screenPosition.x - 100f,
                    guiY + 45f,
                    200f,
                    40f
                ),
                $"범위 내: " +
                $"{castNet.CurrentTargetCount}마리",
                centerStyle
            );
        }

        if (castNet.IsShowingCatchFeedback)
        {
            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(
                    castNet.LastCastPosition
                );

            float guiY =
                Screen.height -
                screenPosition.y;

            GUI.Label(
                new Rect(
                    screenPosition.x - 150f,
                    guiY - 50f,
                    300f,
                    50f
                ),
                $"+{castNet.LastCapturedCount}마리 포획!",
                centerStyle
            );
        }
    }

    // =========================================================
    // LEGACY FINAL FISHING
    // =========================================================

    private void DrawLegacyFinalFishing()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            !flow.IsFinalFishing)
        {
            return;
        }

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 250f,
                30f,
                500f,
                60f
            ),
            $"마감 조업: " +
            $"{flow.FinalFishingTimer:F1}초",
            centerStyle
        );
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