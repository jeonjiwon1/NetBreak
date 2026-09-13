using UnityEngine;

public class BossHUD : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float panelWidth = 600f;
    [SerializeField] private float panelHeight = 145f;
    [SerializeField] private float topMargin = 20f;

    [Header("Between Pass Feedback")]
    [SerializeField] private float escapePanelWidth = 520f;
    [SerializeField] private float escapePanelHeight = 90f;

    private GUIStyle bossNameStyle;
    private GUIStyle resistanceTextStyle;
    private GUIStyle infoStyle;
    private GUIStyle warningStyle;
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

        infoStyle =
            new GUIStyle(
                GUI.skin.label
            );

        infoStyle.alignment =
            TextAnchor.MiddleCenter;

        infoStyle.fontSize = 16;

        infoStyle.fontStyle =
            FontStyle.Bold;

        infoStyle.normal.textColor =
            Color.white;

        warningStyle =
            new GUIStyle(
                infoStyle
            );

        warningStyle.fontSize = 18;

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
    // MAIN PANEL
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

        DrawResistanceBar(
            boss,
            x,
            y
        );

        DrawPhaseAndPass(
            boss,
            x,
            y
        );

        DrawActionState(
            boss,
            x,
            y
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

    private void DrawPhaseAndPass(
        BossEncounterController boss,
        float x,
        float y)
    {
        string passText =
            boss.IsFinalPass
                ? $"마지막 회유 ({boss.CurrentPass} / {boss.MaxPasses})"
                : $"회유 {boss.CurrentPass} / {boss.MaxPasses}";

        string phaseText =
            $"Phase {boss.CurrentPhase} / {boss.MaxPhases}";

        GUI.Label(
            new Rect(
                x + 20f,
                y + 78f,
                panelWidth * 0.5f - 20f,
                28f
            ),
            phaseText,
            infoStyle
        );

        GUI.Label(
            new Rect(
                x + panelWidth * 0.5f,
                y + 78f,
                panelWidth * 0.5f - 20f,
                28f
            ),
            passText,
            boss.IsFinalPass
                ? warningStyle
                : infoStyle
        );
    }

    private void DrawActionState(
        BossEncounterController boss,
        float x,
        float y)
    {
        string stateText = "";

        BossBehaviorController behavior =
            boss.ActiveBossBehavior;

        if (!boss.IsBetweenPasses &&
            behavior != null)
        {
            if (behavior.IsTelegraphing)
            {
                stateText =
                    boss.CurrentPhase >= 3
                        ? "회피 기동 - 돌진 준비!"
                        : "돌진 준비!";
            }
            else if (behavior.IsRushing)
            {
                stateText =
                    "돌진!";
            }
            else if (behavior.IsRecovering)
            {
                stateText =
                    "돌진 후 빈틈";
            }
            else if (boss.CurrentPhase == 1)
            {
                stateText =
                    "기본 회유";
            }
            else if (boss.CurrentPhase == 2)
            {
                stateText =
                    "격한 회유";
            }
            else
            {
                stateText =
                    "난폭 회유";
            }
        }

        GUI.Label(
            new Rect(
                x + 10f,
                y + 108f,
                panelWidth - 20f,
                28f
            ),
            stateText,
            warningStyle
        );
    }

    // =========================================================
    // ESCAPE PANEL
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
            $"{boss.BetweenPassRemaining:F1}초  |  " +
            $"Phase {boss.CurrentPhase} 유지",
            escapeTextStyle
        );
    }
}