using UnityEngine;

public class PrototypeHUD : MonoBehaviour
{
    [SerializeField] private FishSpawner fishSpawner;

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

        RunManager run = RunManager.Instance;

        GUI.Label(
            new Rect(20, 20, 500, 40),
            $"°ñµå: {run.CurrentGold}",
            style
        );

        GUI.Label(
            new Rect(20, 55, 500, 40),
            $"Æ÷È¹ ¼ö: {run.CapturedFishCount}",
            style
        );

        GUI.Label(
            new Rect(20, 90, 500, 40),
            $"¾îÈ¹·ü: {run.CatchRate * 100f:F1}%",
            style
        );

        GUI.Label(
            new Rect(20, 125, 500, 40),
            $"·¹º§: {run.CurrentLevel}",
            style
        );

        GUI.Label(
            new Rect(20, 160, 500, 40),
            $"°æÇèÄ¡: {run.CurrentExp} / {run.ExpToNextLevel}",
            style
        );

        if (fishSpawner != null)
        {
            GUI.Label(
                new Rect(20, 195, 500, 40),
                $"¾î±º À¯ÀÔ: {fishSpawner.SpawnedSchoolCount} / " +
                $"{fishSpawner.TotalSchoolCount}",
                style
            );
        }

        DrawGameFlow();
    }

    private void DrawGameFlow()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null)
        {
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
                $"¸¶°¨ Á¶¾÷: {flow.FinalFishingTimer:F1}ÃÊ",
                centerStyle
            );
        }

        if (!flow.IsGameEnded)
        {
            return;
        }

        string resultText =
            flow.IsSuccess
            ? "Á¶¾÷ ¼º°ø!"
            : "Á¶¾÷ ½ÇÆÐ";

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
            $"ÃÖÁ¾ ¾îÈ¹·ü: " +
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
            $"±âÁØ ¾îÈ¹·ü: " +
            $"{flow.ClearCatchRate * 100f:F0}%",
            centerStyle
        );
    }
}