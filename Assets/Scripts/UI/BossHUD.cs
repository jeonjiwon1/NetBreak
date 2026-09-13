using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHUD : MonoBehaviour
{
    [Header("Boss Panel")]
    [SerializeField] private GameObject bossPanel;

    [SerializeField] private TMP_Text bossNameText;

    [SerializeField] private Image resistanceFill;

    [SerializeField] private TMP_Text resistanceText;

    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text passText;
    [SerializeField] private TMP_Text stateText;

    [Header("Escape Panel")]
    [SerializeField] private GameObject escapePanel;

    [SerializeField] private TMP_Text escapeTitleText;
    [SerializeField] private TMP_Text escapeRecoveryText;
    [SerializeField] private TMP_Text escapeNextText;

    private void Start()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }

        if (escapePanel != null)
        {
            escapePanel.SetActive(false);
        }
    }

    private void Update()
    {
        BossEncounterController boss =
            BossEncounterController.Instance;

        bool shouldShowBoss =
            boss != null &&
            boss.IsRunning;

        UpdateBossPanelVisibility(
            shouldShowBoss
        );

        if (!shouldShowBoss ||
            boss == null)
        {
            HideEscapePanel();
            return;
        }

        UpdateBossPanel(
            boss
        );

        UpdateEscapePanel(
            boss
        );
    }

    // =========================================================
    // BOSS PANEL VISIBILITY
    // =========================================================

    private void UpdateBossPanelVisibility(
        bool shouldShow)
    {
        if (bossPanel == null)
        {
            return;
        }

        if (bossPanel.activeSelf ==
            shouldShow)
        {
            return;
        }

        bossPanel.SetActive(
            shouldShow
        );
    }

    // =========================================================
    // BOSS PANEL
    // =========================================================

    private void UpdateBossPanel(
        BossEncounterController boss)
    {
        UpdateBossName(
            boss
        );

        UpdateResistance(
            boss
        );

        UpdatePhaseAndPass(
            boss
        );

        UpdateState(
            boss
        );
    }

    private void UpdateBossName(
        BossEncounterController boss)
    {
        if (bossNameText == null)
        {
            return;
        }

        bossNameText.text =
            boss.BossName;
    }

    private void UpdateResistance(
        BossEncounterController boss)
    {
        if (resistanceFill != null)
        {
            resistanceFill.fillAmount =
                Mathf.Clamp01(
                    boss.ResistanceRatio
                );
        }

        if (resistanceText != null)
        {
            resistanceText.text =
                $"저항력 " +
                $"{boss.CurrentResistance:F0} / " +
                $"{boss.MaxResistance:F0}";
        }
    }

    private void UpdatePhaseAndPass(
        BossEncounterController boss)
    {
        if (phaseText != null)
        {
            phaseText.text =
                $"Phase {boss.CurrentPhase} / " +
                $"{boss.MaxPhases}";
        }

        if (passText != null)
        {
            if (boss.IsFinalPass)
            {
                passText.text =
                    $"마지막 회유 " +
                    $"({boss.CurrentPass} / " +
                    $"{boss.MaxPasses})";
            }
            else
            {
                passText.text =
                    $"회유 " +
                    $"{boss.CurrentPass} / " +
                    $"{boss.MaxPasses}";
            }
        }
    }

    private void UpdateState(
        BossEncounterController boss)
    {
        if (stateText == null)
        {
            return;
        }

        stateText.text =
            GetBossStateText(
                boss
            );
    }

    private string GetBossStateText(
        BossEncounterController boss)
    {
        if (boss.IsBetweenPasses)
        {
            return "";
        }

        BossBehaviorController behavior =
            boss.ActiveBossBehavior;

        if (behavior == null)
        {
            return "";
        }

        if (behavior.IsTelegraphing)
        {
            if (boss.CurrentPhase >= 3)
            {
                return "회피 기동 - 돌진 준비!";
            }

            return "돌진 준비!";
        }

        if (behavior.IsRushing)
        {
            return "돌진!";
        }

        if (behavior.IsRecovering)
        {
            return "돌진 후 빈틈";
        }

        switch (boss.CurrentPhase)
        {
            case 1:
                return "기본 회유";

            case 2:
                return "격한 회유";

            case 3:
                return "난폭 회유";

            default:
                return "";
        }
    }

    // =========================================================
    // ESCAPE PANEL
    // =========================================================

    private void UpdateEscapePanel(
        BossEncounterController boss)
    {
        bool shouldShow =
            boss.IsBetweenPasses;

        if (escapePanel != null &&
            escapePanel.activeSelf !=
            shouldShow)
        {
            escapePanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow)
        {
            return;
        }

        if (escapeTitleText != null)
        {
            escapeTitleText.text =
                "보스가 도주했습니다.";
        }

        if (escapeRecoveryText != null)
        {
            escapeRecoveryText.text =
                $"저항력 " +
                $"{boss.LastResistanceBeforeRecovery:F0}" +
                " → " +
                $"{boss.LastResistanceAfterRecovery:F0}" +
                $" (+{boss.LastRecoveryAmount:F0})";
        }

        if (escapeNextText != null)
        {
            escapeNextText.text =
                $"다음 회유까지 " +
                $"{boss.BetweenPassRemaining:F1}초" +
                " | " +
                $"Phase {boss.CurrentPhase} 유지";
        }
    }

    private void HideEscapePanel()
    {
        if (escapePanel != null &&
            escapePanel.activeSelf)
        {
            escapePanel.SetActive(
                false
            );
        }
    }
}