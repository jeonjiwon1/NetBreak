using UnityEngine;

public class BossHUD : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float panelWidth = 600f;
    [SerializeField] private float panelHeight = 115f;
    [SerializeField] private float topMargin = 20f;

    [Header("Between Pass Feedback")]
    [SerializeField] private float escapePanelWidth = 520f;
    [SerializeField] private float escapePanelHeight = 90f;

    private GUIStyle bossNameStyle;
    private GUIStyle resistanceTextStyle;
    private GUIStyle passStyle;
    private GUIStyle finalPassStyle;
    private GUIStyle escapeTitleStyle;
    private GUIStyle escapeTextStyle;

    private void OnGUI()
    {
        EnsureStyles();

        BossEncounterController boss =
            BossEncounterController.Instance;

        if (boss == null ||
            !boss.IsRunning)
        {
            return;
        }

        DrawBossPanel(
            boss
        );

        if (boss.IsBetweenPasses)
        {
            DrawEscapePanel(
                boss
            );
        }
    }

    // =========================================================
    // STYLE
    // =========================================================

    private void EnsureStyles()
    {
        if (bossNameStyle != null)
        {
            return;
        }

        bossNameStyle =
            new GUIStyle(
                GUI.skin.label
            );

        bossNameStyle.alignment =
            TextAnchor.MiddleCenter;

        bossNameStyle.fontSize = 22;

        bossNameStyle.fontStyle =
            FontStyle.Bold;

        bossNameStyle.normal.textColor =
            Color.white;

        resistanceTextStyle =
            new GUIStyle(
                GUI.skin.label
            );

        resistanceTextStyle.alignment =
            TextAnchor.MiddleCenter;

        resistanceTextStyle.fontSize = 15;

        resistanceTextStyle.fontStyle =
            FontStyle.Bold;

        resistanceTextStyle.normal.textColor =
            Color.white;

        passStyle =
            new GUIStyle(
                GUI.skin.label
            );

        passStyle.alignment =
            TextAnchor.MiddleCenter;

        passStyle.fontSize = 16;

        passStyle.fontStyle =
            FontStyle.Bold;

        passStyle.normal.textColor =
            Color.white;

        finalPassStyle =
            new GUIStyle(
                passStyle
            );

        finalPassStyle.fontSize = 18;

        escapeTitleStyle =
            new GUIStyle(
                GUI.skin.label
            );

        escapeTitleStyle.alignment =
            TextAnchor.MiddleCenter;

        escapeTitleStyle.fontSize = 20;

        escapeTitleStyle.fontStyle =
            FontStyle.Bold;

        escapeTitleStyle.normal.textColor =
            Color.white;

        escapeTextStyle =
            new GUIStyle(
                GUI.skin.label
            );

        escapeTextStyle.alignment =
            TextAnchor.MiddleCenter;

        escapeTextStyle.fontSize = 16;

        escapeTextStyle.normal.textColor =
            Color.white;
    }

    // =========================================================
    // MAIN BOSS PANEL
    // =========================================================

    private void DrawBossPanel(
        BossEncounterController boss)
    {
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

        DrawBossName(
            boss,
            x,
            y
        );

        DrawResistanceBar(
            boss,
            x,
            y
        );

        DrawPassInfo(
            boss,
            x,
            y
        );
    }

    private void DrawBossName(
        BossEncounterController boss,
        float x,
        float y)
    {
        GUI.Label(
            new Rect(
                x + 10f,
                y + 5f,
                panelWidth - 20f,
                30f
            ),
            boss.BossName,
            bossNameStyle
        );
    }

    private void DrawResistanceBar(
        BossEncounterController boss,
        float x,
        float y)
    {
        Rect backgroundRect =
            new Rect(
                x + 40f,
                y + 42f,
                panelWidth - 80f,
                28f
            );

        GUI.Box(
            backgroundRect,
            ""
        );

        float ratio =
            Mathf.Clamp01(
                boss.ResistanceRatio
            );

        Rect fillRect =
            new Rect(
                backgroundRect.x + 2f,
                backgroundRect.y + 2f,
                (backgroundRect.width - 4f) *
                ratio,
                backgroundRect.height - 4f
            );

        Color previousColor =
            GUI.color;

        GUI.color =
            new Color(
                0.75f,
                0.15f,
                0.15f,
                1f
            );

        GUI.Box(
            fillRect,
            ""
        );

        GUI.color =
            previousColor;

        GUI.Label(
            backgroundRect,
            $"Resistance  " +
            $"{boss.CurrentResistance:F0} / " +
            $"{boss.MaxResistance:F0}",
            resistanceTextStyle
        );
    }

    private void DrawPassInfo(
        BossEncounterController boss,
        float x,
        float y)
    {
        string passText;

        GUIStyle style;

        if (boss.IsFinalPass)
        {
            passText =
                $"마지막 회유  " +
                $"({boss.CurrentPass} / {boss.MaxPasses})";

            style =
                finalPassStyle;
        }
        else
        {
            passText =
                $"회유  " +
                $"{boss.CurrentPass} / {boss.MaxPasses}";

            style =
                passStyle;
        }

        GUI.Label(
            new Rect(
                x + 10f,
                y + 78f,
                panelWidth - 20f,
                28f
            ),
            passText,
            style
        );
    }

    // =========================================================
    // ESCAPE / RECOVERY PANEL
    // =========================================================

    private void DrawEscapePanel(
        BossEncounterController boss)
    {
        float x =
            Screen.width * 0.5f -
            escapePanelWidth * 0.5f;

        float y =
            topMargin +
            panelHeight +
            15f;

        GUI.Box(
            new Rect(
                x,
                y,
                escapePanelWidth,
                escapePanelHeight
            ),
            ""
        );

        GUI.Label(
            new Rect(
                x + 10f,
                y + 5f,
                escapePanelWidth - 20f,
                30f
            ),
            "보스가 도주했습니다.",
            escapeTitleStyle
        );

        GUI.Label(
            new Rect(
                x + 10f,
                y + 35f,
                escapePanelWidth - 20f,
                28f
            ),
            $"Resistance  " +
            $"{boss.LastResistanceBeforeRecovery:F0} → " +
            $"{boss.LastResistanceAfterRecovery:F0}  " +
            $"(+{boss.LastRecoveryAmount:F0})",
            escapeTextStyle
        );

        GUI.Label(
            new Rect(
                x + 10f,
                y + 60f,
                escapePanelWidth - 20f,
                24f
            ),
            $"다음 회유까지 " +
            $"{boss.BetweenPassRemaining:F1}초",
            escapeTextStyle
        );
    }
}