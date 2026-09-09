using UnityEngine;

public class PrototypeHUD : MonoBehaviour
{
    [SerializeField] private FishSpawner fishSpawner;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;

    private GUIStyle style;
    private GUIStyle centerStyle;
    private GUIStyle resultStyle;

    private void Awake()
    {
        style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        centerStyle = new GUIStyle(style);
        centerStyle.alignment = TextAnchor.MiddleCenter;
        centerStyle.fontSize = 30;

        resultStyle = new GUIStyle(centerStyle);
        resultStyle.fontSize = 42;
    }

    private void OnGUI()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        RunManager run =
            RunManager.Instance;

        GUI.Label(
            new Rect(20, 20, 500, 40),
            $"골드: {run.CurrentGold}",
            style
        );

        GUI.Label(
            new Rect(20, 55, 500, 40),
            $"포획 수: {run.CapturedFishCount}",
            style
        );

        GUI.Label(
            new Rect(20, 90, 500, 40),
            $"어획률: {run.CatchRate * 100f:F1}%",
            style
        );

        GUI.Label(
            new Rect(20, 125, 500, 40),
            $"레벨: {run.CurrentLevel}",
            style
        );

        GUI.Label(
            new Rect(20, 160, 500, 40),
            $"경험치: {run.CurrentExp} / {run.ExpToNextLevel}",
            style
        );

        if (fishSpawner != null)
        {
            GUI.Label(
                new Rect(20, 195, 500, 40),
                $"어군 유입: " +
                $"{fishSpawner.SpawnedSchoolCount} / " +
                $"{fishSpawner.TotalSchoolCount}",
                style
            );
        }

        if (castNet != null)
        {
            string castNetText;

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

            GUI.Label(
                new Rect(20, 230, 500, 40),
                castNetText,
                style
            );
        }
        if (netPlacement != null &&
    netPlacement.IsDragging)
        {
            GUI.Label(
                new Rect(20, 265, 500, 40),
                $"그물 설치 비용: " +
                $"{netPlacement.CurrentPlacementCost}G",
                style
            );
        }

        DrawCastNetInfo();
        DrawGameFlow();
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
                Screen.height - screenPosition.y;

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
                Screen.height - screenPosition.y;

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

        string resultText =
            flow.IsSuccess
            ? "조업 성공!"
            : "조업 실패";

        GUI.Box(
            new Rect(
                Screen.width * 0.5f - 250f,
                Screen.height * 0.5f - 130f,
                500f,
                260f
            ),
            ""
        );

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 230f,
                Screen.height * 0.5f - 100f,
                460f,
                70f
            ),
            resultText,
            resultStyle
        );

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 230f,
                Screen.height * 0.5f - 20f,
                460f,
                50f
            ),
            $"최종 어획률: " +
            $"{RunManager.Instance.CatchRate * 100f:F1}%",
            centerStyle
        );

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 230f,
                Screen.height * 0.5f + 35f,
                460f,
                50f
            ),
            $"기준 어획률: " +
            $"{flow.ClearCatchRate * 100f:F0}%",
            centerStyle
        );
    }
}