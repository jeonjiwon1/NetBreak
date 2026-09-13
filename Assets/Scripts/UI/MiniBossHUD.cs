using UnityEngine;

public class MiniBossHUD : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float panelWidth = 520f;
    [SerializeField] private float panelHeight = 85f;
    [SerializeField] private float topMargin = 20f;

    [Header("Feedback")]
    [SerializeField] private float rewardMessageDuration = 2.5f;

    private GUIStyle titleStyle;
    private GUIStyle resistanceStyle;
    private GUIStyle warningStyle;
    private GUIStyle rewardStyle;

    private string rewardMessage = "";
    private float rewardMessageEndTime;

    private void OnEnable()
    {
        MiniBossController.MiniBossCaptured +=
            HandleMiniBossCaptured;
    }

    private void OnDisable()
    {
        MiniBossController.MiniBossCaptured -=
            HandleMiniBossCaptured;
    }

    private void OnGUI()
    {
        EnsureStyles();

        DrawMiniBossResistance();
        DrawRewardFeedback();
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle =
            new GUIStyle(
                GUI.skin.label
            );

        titleStyle.alignment =
            TextAnchor.MiddleCenter;

        titleStyle.fontSize = 20;

        titleStyle.fontStyle =
            FontStyle.Bold;

        titleStyle.normal.textColor =
            Color.white;

        resistanceStyle =
            new GUIStyle(
                GUI.skin.label
            );

        resistanceStyle.alignment =
            TextAnchor.MiddleCenter;

        resistanceStyle.fontSize = 15;

        resistanceStyle.normal.textColor =
            Color.white;

        warningStyle =
            new GUIStyle(
                titleStyle
            );

        warningStyle.fontSize = 18;

        rewardStyle =
            new GUIStyle(
                titleStyle
            );

        rewardStyle.fontSize = 22;
    }

    private void DrawMiniBossResistance()
    {
        MiniBossController miniBoss =
            MiniBossController.ActiveMiniBoss;

        if (miniBoss == null ||
            !miniBoss.gameObject.activeInHierarchy)
        {
            return;
        }

        float x =
            Screen.width * 0.5f -
            panelWidth * 0.5f;

        float y =
            topMargin;

        GUI.Box(
            new Rect(
                x,
                y,
                panelWidth,
                panelHeight
            ),
            ""
        );

        string stateText = "";

        if (miniBoss.IsTelegraphing)
        {
            stateText =
                "  -  돌진 준비!";
        }
        else if (miniBoss.IsDashing)
        {
            stateText =
                "  -  돌진!";
        }

        GUI.Label(
            new Rect(
                x + 10f,
                y + 5f,
                panelWidth - 20f,
                28f
            ),
            $"{miniBoss.DisplayName}{stateText}",
            miniBoss.IsTelegraphing
                ? warningStyle
                : titleStyle
        );

        Rect backgroundRect =
            new Rect(
                x + 30f,
                y + 40f,
                panelWidth - 60f,
                25f
            );

        GUI.Box(
            backgroundRect,
            ""
        );

        float ratio =
            Mathf.Clamp01(
                miniBoss.ResistanceRatio
            );

        Rect fillRect =
            new Rect(
                backgroundRect.x + 2f,
                backgroundRect.y + 2f,
                (backgroundRect.width - 4f)
                * ratio,
                backgroundRect.height - 4f
            );

        Color oldColor =
            GUI.color;

        GUI.color =
            new Color(
                0.85f,
                0.25f,
                0.20f,
                1f
            );

        GUI.Box(
            fillRect,
            ""
        );

        GUI.color =
            oldColor;

        GUI.Label(
            backgroundRect,
            $"Resistance  " +
            $"{miniBoss.CurrentResistance:F0} / " +
            $"{miniBoss.MaxResistance:F0}",
            resistanceStyle
        );
    }

    private void DrawRewardFeedback()
    {
        if (Time.unscaledTime >=
            rewardMessageEndTime)
        {
            return;
        }

        GUI.Box(
            new Rect(
                Screen.width * 0.5f - 260f,
                120f,
                520f,
                60f
            ),
            ""
        );

        GUI.Label(
            new Rect(
                Screen.width * 0.5f - 250f,
                125f,
                500f,
                50f
            ),
            rewardMessage,
            rewardStyle
        );
    }

    private void HandleMiniBossCaptured(
        string fishName,
        int gold,
        int exp)
    {
        rewardMessage =
            $"{fishName} 포획!  " +
            $"보너스 +{gold}G / +{exp} EXP";

        rewardMessageEndTime =
            Time.unscaledTime +
            rewardMessageDuration;
    }
}