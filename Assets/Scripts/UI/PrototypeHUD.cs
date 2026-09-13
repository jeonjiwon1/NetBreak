using UnityEngine;

public class PrototypeHUD : MonoBehaviour
{
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;

    private GUIStyle centerStyle;
    private GUIStyle resultStyle;
    private GUIStyle resultInfoStyle;
    private GUIStyle rankStyle;

    private void Awake()
    {
        centerStyle =
            new GUIStyle();

        centerStyle.fontSize = 30;
        centerStyle.alignment =
            TextAnchor.MiddleCenter;

        centerStyle.normal.textColor =
            Color.white;

        resultStyle =
            new GUIStyle(
                centerStyle
            );

        resultStyle.fontSize = 42;
        resultStyle.fontStyle =
            FontStyle.Bold;

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

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow != null &&
            flow.IsGameEnded)
        {
            DrawResultScreen(
                flow
            );

            return;
        }

        // 기본 HUD:
        // PrototypeHUDCanvas 담당.
        //
        // 조업 준비:
        // PrototypeHUDCanvas 담당.
        //
        // Encounter Announcement:
        // PrototypeHUDCanvas 담당.

        DrawNetCost();
        DrawCastNetInfo();

        // Legacy Final Fishing이 아직 호출되는 경우만
        // 임시 표시를 유지한다.
        DrawLegacyFinalFishing();
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
                    $"보스 포획: " +
                    $"{boss.CurrentPass}차 회유";
            }
            else
            {
                bossResult =
                    $"보스 도주: " +
                    $"{boss.CurrentPass}차 회유";
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