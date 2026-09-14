using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeHUDCanvas : MonoBehaviour
{
    private static readonly string[] ActiveSlotBindings =
    {
        "Q", "W", "E", "R"
    };

    [Header("Run")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text captureText;
    [SerializeField] private TMP_Text catchRateText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;

    [Header("Encounter")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Hotbar")]
    [SerializeField] private TMP_Text castNetText;

    [Header("Job")]
    [SerializeField] private TMP_Text jobText;

    [Header("Preparation")]
    [SerializeField] private GameObject preparationPanel;
    [SerializeField] private Button startFishingButton;

    [Header("Announcement")]
    [SerializeField] private GameObject announcementPanel;
    [SerializeField] private TMP_Text announcementText;

    [Header("References")]
    [SerializeField] private FishSpawner fishSpawner;
    [SerializeField] private CastNetController castNet;

    private readonly TMP_Text[] hotbarSlotTexts =
        new TMP_Text[RunToolLoadout.SlotCount + 1];
    private RectTransform hotbarRoot;

    private void Awake()
    {
        BuildHotbar();

        if (startFishingButton != null)
        {
            startFishingButton.onClick.AddListener(
                HandleStartFishingClicked
            );
        }
    }

    private void Start()
    {
        RefreshImmediateState();
    }

    private void Update()
    {
        UpdateRunInfo();
        UpdateEncounterInfo();
        UpdateHotbar();
        UpdateJobInfo();

        UpdatePreparationUI();
        UpdateAnnouncementUI();
    }

    // =========================================================
    // RUN INFO
    // =========================================================

    private void UpdateRunInfo()
    {
        RunManager run =
            RunManager.Instance;

        if (run == null)
        {
            return;
        }

        if (goldText != null)
        {
            goldText.text =
                $"골드: {run.CurrentGold}";
        }

        if (captureText != null)
        {
            captureText.text =
                $"포획 수: {run.CapturedFishCount}";
        }

        if (catchRateText != null)
        {
            catchRateText.text =
                $"어획률: {run.CatchRate * 100f:F1}%";
        }

        if (levelText != null)
        {
            levelText.text =
                $"레벨: {run.CurrentLevel}";
        }

        if (expText != null)
        {
            expText.text =
                $"경험치: {run.CurrentExp} / " +
                $"{run.ExpToNextLevel}";
        }
    }

    // =========================================================
    // ENCOUNTER INFO
    // =========================================================

    private void UpdateEncounterInfo()
    {
        if (fishSpawner == null)
        {
            return;
        }

        if (stageText != null)
        {
            if (fishSpawner.HasStarted)
            {
                stageText.text =
                    $"조업 단계: " +
                    $"{fishSpawner.CurrentStageIndex} / " +
                    $"{fishSpawner.TotalStageCount}";
            }
            else
            {
                stageText.text =
                    "조업 단계: 준비";
            }
        }

        if (phaseText != null)
        {
            phaseText.text =
                $"현재 구간: " +
                $"{fishSpawner.CurrentPhaseName}";
        }
    }

    // =========================================================
    // HOTBAR
    // =========================================================

    private void BuildHotbar()
    {
        if (castNetText == null)
        {
            return;
        }

        GameObject rootObject = new GameObject(
            "DynamicHotbar",
            typeof(RectTransform),
            typeof(HorizontalLayoutGroup)
        );
        rootObject.layer = gameObject.layer;
        hotbarRoot = rootObject.GetComponent<RectTransform>();
        hotbarRoot.SetParent(transform, false);
        hotbarRoot.anchorMin = new Vector2(0.5f, 0f);
        hotbarRoot.anchorMax = new Vector2(0.5f, 0f);
        hotbarRoot.pivot = new Vector2(0.5f, 0f);
        hotbarRoot.anchoredPosition = new Vector2(0f, 24f);
        hotbarRoot.sizeDelta = new Vector2(1100f, 84f);

        HorizontalLayoutGroup layout =
            rootObject.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < hotbarSlotTexts.Length; i++)
        {
            string binding = i == 0
                ? "LMB"
                : ActiveSlotBindings[i - 1];
            hotbarSlotTexts[i] = CreateHotbarSlot(
                binding,
                i == 0
            );
        }
    }

    private TMP_Text CreateHotbarSlot(
        string binding,
        bool isFixedTool)
    {
        GameObject slotObject = new GameObject(
            $"HotbarSlot_{binding}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(LayoutElement)
        );
        slotObject.layer = gameObject.layer;
        slotObject.transform.SetParent(hotbarRoot, false);

        Image background = slotObject.GetComponent<Image>();
        background.color = isFixedTool
            ? new Color(0.10f, 0.28f, 0.42f, 0.88f)
            : new Color(0.04f, 0.08f, 0.14f, 0.82f);
        background.raycastTarget = false;

        LayoutElement layoutElement =
            slotObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 205f;
        layoutElement.preferredHeight = 84f;

        TMP_Text slotText;
        if (isFixedTool)
        {
            slotText = castNetText;
            slotText.transform.SetParent(slotObject.transform, false);
        }
        else
        {
            slotText = Instantiate(
                castNetText,
                slotObject.transform
            );
        }

        slotText.gameObject.name = $"HotbarText_{binding}";
        slotText.raycastTarget = false;
        slotText.alignment = TextAlignmentOptions.Center;
        slotText.fontSize = 22f;

        RectTransform textRect = slotText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(6f, 4f);
        textRect.offsetMax = new Vector2(-6f, -4f);

        return slotText;
    }

    private void UpdateHotbar()
    {
        if (hotbarSlotTexts[0] == null)
        {
            return;
        }

        RunToolLoadout loadout =
            RunManager.Instance != null
                ? RunManager.Instance.ToolSlots
                : null;

        hotbarSlotTexts[0].text =
            "[LMB]\n뜰채\n고정 도구";

        for (int i = 0; i < RunToolLoadout.SlotCount; i++)
        {
            hotbarSlotTexts[i + 1].text =
                GetSlotText(
                    loadout,
                    i,
                    ActiveSlotBindings[i]
                );
        }
    }

    private string GetSlotText(
        RunToolLoadout loadout,
        int slotIndex,
        string binding)
    {
        ToolId tool = loadout != null
            ? loadout.GetSlot(slotIndex)
            : ToolId.None;

        return tool == ToolId.None
            ? $"[{binding}]\n비어 있음"
            : $"[{binding}]\n{GetToolStatus(tool)}";
    }

    private string GetToolStatus(ToolId tool)
    {
        if (tool != ToolId.CastNet || castNet == null)
        {
            return GetToolName(tool);
        }

        if (castNet.MaxCharges > 1)
        {
            string cooldown = castNet.CurrentCharges == castNet.MaxCharges
                ? ""
                : $" (충전 {castNet.CooldownTimer:F1}초)";

            return
                $"투망 {castNet.CurrentCharges}/{castNet.MaxCharges}{cooldown}";
        }

        return castNet.IsReady
            ? "투망\n준비 완료"
            : $"투망\n{castNet.CooldownTimer:F1}초";
    }

    private static string GetToolName(ToolId tool)
    {
        return tool switch
        {
            ToolId.Bait => "미끼",
            ToolId.Net => "그물",
            ToolId.CastNet => "투망",
            ToolId.FishingRod => "낚싯대",
            ToolId.LandingNet => "뜰채",
            _ => "비어 있음"
        };
    }

    // =========================================================
    // JOB
    // =========================================================

    private void UpdateJobInfo()
    {
        if (jobText == null)
        {
            return;
        }

        PrototypeJobManager jobManager =
            PrototypeJobManager.Instance;

        if (jobManager == null)
        {
            jobText.text =
                "전직: 초보 어부";

            return;
        }

        jobText.text =
            $"전직: {jobManager.CurrentJobName}";
    }

    // =========================================================
    // PREPARATION
    // =========================================================

    private void UpdatePreparationUI()
    {
        if (preparationPanel == null)
        {
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        bool shouldShow =
            flow != null &&
            flow.IsPreparation;

        if (preparationPanel.activeSelf !=
            shouldShow)
        {
            preparationPanel.SetActive(
                shouldShow
            );
        }
    }

    private void HandleStartFishingClicked()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            !flow.IsPreparation)
        {
            return;
        }

        flow.StartFishing();

        if (preparationPanel != null)
        {
            preparationPanel.SetActive(
                false
            );
        }
    }

    // =========================================================
    // ANNOUNCEMENT
    // =========================================================

    private void UpdateAnnouncementUI()
    {
        if (announcementPanel == null ||
            announcementText == null)
        {
            return;
        }

        bool shouldShow =
            fishSpawner != null &&
            fishSpawner.IsShowingAnnouncement;

        if (announcementPanel.activeSelf !=
            shouldShow)
        {
            announcementPanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow)
        {
            return;
        }

        announcementText.text =
            fishSpawner.AnnouncementText;
    }

    // =========================================================
    // INITIAL STATE
    // =========================================================

    private void RefreshImmediateState()
    {
        UpdateHotbar();
        UpdatePreparationUI();
        UpdateAnnouncementUI();
    }

    private void OnDestroy()
    {
        if (startFishingButton != null)
        {
            startFishingButton.onClick.RemoveListener(
                HandleStartFishingClicked
            );
        }
    }
}
