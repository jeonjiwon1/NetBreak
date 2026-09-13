using UnityEngine;

public class PrototypeHUD : MonoBehaviour
{
    [SerializeField] private FishSpawner fishSpawner;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;

    private GUIStyle style;
    private GUIStyle centerStyle;
    private GUIStyle resultStyle;
    private GUIStyle announcementStyle;
    private GUIStyle resultInfoStyle;
    private GUIStyle rankStyle;

    private void Awake()
    {
        style =
            new GUIStyle();

        style.fontSize = 24;
        style.normal.textColor =
            Color.white;

        centerStyle =
            new GUIStyle(
                style
            );

        centerStyle.alignment =
            TextAnchor.MiddleCenter;

        centerStyle.fontSize = 30;

        resultStyle =
            new GUIStyle(
                centerStyle
            );

        resultStyle.fontSize = 42;
        resultStyle.fontStyle =
            FontStyle.Bold;

        announcementStyle =
            new GUIStyle(
                centerStyle
            );

        announcementStyle.fontSize = 34;

        resultInfoStyle =
            new GUIStyle(
                centerStyle
            );

        resultInfoStyle.fontSize = 22;

        rankStyle =
            new GUIStyle(
                centerStyle
            );

        rankStyle.fontSize = 36;
        rankStyle.fontStyle =
            FontStyle.Bold;
    }

    private void OnGUI()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        RunManager run =
            RunManager.Instance;

        // 결과 화면 중에는 기존 HUD 정보를
        // 뒤에 겹쳐 표시하지 않는다.
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow != null &&
            flow.IsGameEnded)
        {
            DrawGameFlow();
            return;
        }

        GUI.Label(
            new Rect(
                20,
                20,
                500,
                40
            ),
            $"골드: {run.CurrentGold}",
            style
        );

        GUI.Label(
            new Rect(
                20,
                55,
                500,
                40
            ),
            $"포획 수: {run.CapturedFishCount}",
            style
        );

        GUI.Label(
            new Rect(
                20,
                90,
                500,
                40
            ),
            $"어획률: {run.CatchRate * 100f:F1}%",
            style
        );

        GUI.Label(
            new Rect(
                20,
                125,
                500,
                40
            ),
            $"레벨: {run.CurrentLevel}",
            style
        );

        GUI.Label(
            new Rect(
                20,
                160,
                500,
                40
            ),
            $"경험치: " +
            $"{run.CurrentExp} / " +
            $"{run.ExpToNextLevel}",
            style
        );

        if (fishSpawner != null &&
            fishSpawner.HasStarted)
        {
            GUI.Label(
                new Rect(
                    20,
                    195,
                    600,
                    40
                ),
                $"조업 단계: " +
                $"{fishSpawner.CurrentStageIndex} / " +
                $"{fishSpawner.TotalStageCount}",
                style
            );

            GUI.Label(
                new Rect(
                    20,
                    230,
                    600,
                    40
                ),
                $"현재 구간: " +
                $"{fishSpawner.CurrentPhaseName}",
                style
            );
        }

        DrawCastNetStatus();
        DrawNetCost();
        DrawCastNetInfo();
        DrawEncounterAnnouncement();
        DrawGameFlow();
    }

    // =========================================================
    // CAST NET
    // =========================================================

    private void DrawCastNetStatus()
    {
        if (castNet == null)
        {
            return;
        }

        string castNetText;

        if (castNet.MaxCharges > 1)
        {
            if (castNet.CurrentCharges ==
                castNet.MaxCharges)
            {
                castNetText =
                    $"투망 [E]: " +
                    $"{castNet.CurrentCharges}/" +
                    $"{castNet.MaxCharges}";
            }
            else
            {
                castNetText =
                    $"투망 [E]: " +
                    $"{castNet.CurrentCharges}/" +
                    $"{castNet.MaxCharges} " +
                    $"(충전 " +
                    $"{castNet.CooldownTimer:F1}초)";
            }
        }
        else
        {
            if (castNet.IsReady)
            {
                castNetText =
                    "투망 [E]: 준비 완료";
            }
            else
            {
                castNetText =
                    $"투망 [E]: " +
                    $"{castNet.CooldownTimer:F1}초";
            }
        }

        GUI.Label(
            new Rect(
                20,
                265,
                600,
                40
            ),
            castNetText,
            style
        );
    }

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
            $"{netPlacement.CurrentPlacementCost}G",
            style
        );
    }

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
    // ANNOUNCEMENT
    // =========================================================

    private void DrawEncounterAnnouncement()
    {
        if (fishSpawner == null ||
            !fishSpawner.IsShowingAnnouncement)
        {
            return;
        }

        GUI.Box(
            new Rect(
                Screen.width * 0.5f - 300f,
                100f,
                600f,
                70f
            ),
            ""
        );

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 290f,
                105f,
                580f,
                60f
            ),
            fishSpawner.AnnouncementText,
            announcementStyle
        );
    }

    // =========================================================
    // GAME FLOW
    // =========================================================

    private void DrawGameFlow()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null)
        {
            return;
        }

        if (flow.IsPreparation)
        {
            GUI.Box(
                new Rect(
                    Screen.width * 0.5f - 180f,
                    30f,
                    360f,
                    120f
                ),
                ""
            );

            GUI.Label(
                new Rect(
                    Screen.width * 0.5f - 160f,
                    40f,
                    320f,
                    45f
                ),
                "조업 준비",
                centerStyle
            );

            if (GUI.Button(
                new Rect(
                    Screen.width * 0.5f - 100f,
                    90f,
                    200f,
                    45f
                ),
                "조업 시작"
            ))
            {
                flow.StartFishing();
            }

            return;
        }

        // 이전 Final Fishing 방식과의
        // 임시 호환 표시.
        if (flow.IsFinalFishing)
        {
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

        if (!flow.IsGameEnded)
        {
            return;
        }

        DrawResultScreen(
            flow
        );
    }

    // =========================================================
    // RESULT
    // =========================================================

    private void DrawResultScreen(
        PrototypeGameFlowManager flow)
    {
        RunManager run =
            RunManager.Instance;

        float panelWidth = 620f;
        float panelHeight = 500f;

        float x =
            Screen.width * 0.5f -
            panelWidth * 0.5f;

        float y =
            Screen.height * 0.5f -
            panelHeight * 0.5f;

        GUI.Box(
            new Rect(
                x,
                y,
                panelWidth,
                panelHeight
            ),
            ""
        );

        GUI.Label(
            new Rect(
                x + 20f,
                y + 25f,
                panelWidth - 40f,
                60f
            ),
            flow.ResultTitle,
            resultStyle
        );

        GUI.Label(
            new Rect(
                x + 20f,
                y + 85f,
                panelWidth - 40f,
                40f
            ),
            flow.ResultDescription,
            resultInfoStyle
        );

        GUI.Label(
            new Rect(
                x + 20f,
                y + 135f,
                panelWidth - 40f,
                55f
            ),
            $"어획 등급  {flow.CatchRank}",
            rankStyle
        );

        GUI.Label(
            new Rect(
                x + 50f,
                y + 200f,
                panelWidth - 100f,
                35f
            ),
            $"최종 어획률: " +
            $"{run.CatchRate * 100f:F1}%",
            resultInfoStyle
        );

        GUI.Label(
            new Rect(
                x + 50f,
                y + 240f,
                panelWidth - 100f,
                35f
            ),
            $"포획 수: " +
            $"{run.CapturedFishCount}마리",
            resultInfoStyle
        );

        GUI.Label(
            new Rect(
                x + 50f,
                y + 280f,
                panelWidth - 100f,
                35f
            ),
            $"보유 골드: " +
            $"{run.CurrentGold}G",
            resultInfoStyle
        );

        string jobName =
            "초보 어부";

        if (PrototypeJobManager.Instance != null)
        {
            jobName =
                PrototypeJobManager.Instance
                    .CurrentJobName;
        }

        GUI.Label(
            new Rect(
                x + 50f,
                y + 320f,
                panelWidth - 100f,
                35f
            ),
            $"전직: {jobName}",
            resultInfoStyle
        );

        BossEncounterController boss =
            BossEncounterController.Instance;

        if (boss != null)
        {
            string bossResult;

            if (flow.IsSuccess)
            {
                bossResult =
                    $"보스 포획: {boss.CurrentPass}차 회유";
            }
            else
            {
                bossResult =
                    $"보스 도주: {boss.CurrentPass}차 회유";
            }

            GUI.Label(
                new Rect(
                    x + 50f,
                    y + 360f,
                    panelWidth - 100f,
                    35f
                ),
                bossResult,
                resultInfoStyle
            );
        }

        if (GUI.Button(
            new Rect(
                x + panelWidth * 0.5f - 100f,
                y + 420f,
                200f,
                50f
            ),
            "다시 조업하기"
        ))
        {
            flow.RestartPrototype();
        }
    }
}