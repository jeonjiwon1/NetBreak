using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
    [SerializeField] private TMP_Text runTimeText;

    [Header("Encounter")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Hotbar")]
    [FormerlySerializedAs("castNetText")]
    [SerializeField] private TMP_Text legacyCastNetText;

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
    private readonly Image[] hotbarCooldownOverlays =
        new Image[RunToolLoadout.SlotCount + 1];
    private RectTransform hotbarRoot;
    private LandingNetController landingNet;
    private BaitController bait;
    private NetPlacementController netPlacement;
    private FishingRodPlacementController rodPlacement;

    private void Awake()
    {
        landingNet = FindFirstObjectByType<LandingNetController>();
        bait = BaitController.Instance != null
            ? BaitController.Instance
            : FindFirstObjectByType<BaitController>();
        netPlacement = FindFirstObjectByType<NetPlacementController>();
        rodPlacement = FindFirstObjectByType<FishingRodPlacementController>();

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
        UpdateRunTimeInfo();
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

    private void UpdateRunTimeInfo()
    {
        if (runTimeText == null)
        {
            return;
        }

        float activeRunTime =
            PrototypeGameFlowManager.Instance != null
                ? PrototypeGameFlowManager.Instance.ActiveRunTime
                : 0f;
        int totalSeconds =
            Mathf.Max(
                0,
                Mathf.FloorToInt(activeRunTime)
            );

        runTimeText.text =
            $"플레이 시간: {totalSeconds / 60:00}:{totalSeconds % 60:00}";
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
        if (legacyCastNetText == null)
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
                i == 0,
                i
            );
        }

        legacyCastNetText.gameObject.SetActive(false);
    }

    private TMP_Text CreateHotbarSlot(
        string binding,
        bool isFixedTool,
        int slotIndex)
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

        GameObject overlayObject = new GameObject(
            $"HotbarCooldown_{binding}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlayObject.layer = gameObject.layer;
        overlayObject.transform.SetParent(slotObject.transform, false);

        Image overlay = overlayObject.GetComponent<Image>();
        overlay.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
        overlay.raycastTarget = false;
        hotbarCooldownOverlays[slotIndex] = overlay;

        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = new Vector2(0f, 1f);
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        TMP_Text slotText = Instantiate(
            legacyCastNetText,
            slotObject.transform
        );

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
        SetCooldownOverlay(
            0,
            landingNet != null
                ? landingNet.CooldownNormalized
                : 0f
        );

        for (int i = 0; i < RunToolLoadout.SlotCount; i++)
        {
            ToolId tool = loadout != null
                ? loadout.GetSlot(i)
                : ToolId.None;
            hotbarSlotTexts[i + 1].text =
                GetSlotText(
                    tool,
                    ActiveSlotBindings[i]
                );
            SetCooldownOverlay(
                i + 1,
                GetCooldownNormalized(tool)
            );
        }
    }

    private string GetSlotText(
        ToolId tool,
        string binding)
    {
        return tool == ToolId.None
            ? $"[{binding}]\n비어 있음"
            : $"[{binding}]\n{GetToolStatus(tool)}";
    }

    private string GetToolStatus(ToolId tool)
    {
        if (tool == ToolId.FishingRod && rodPlacement != null)
        {
            return
                $"낚싯대\n" +
                $"{rodPlacement.ActiveRodCount} / {rodPlacement.MaxActiveRods}\n" +
                $"다음: {rodPlacement.PlacementCost}G";
        }

        if (tool == ToolId.Net && netPlacement != null)
        {
            return
                $"그물\n" +
                $"{netPlacement.ActiveNetCount} / {netPlacement.MaxActiveNets}";
        }

        if (tool == ToolId.Bait && bait != null)
        {
            return bait.RemainingCooldown > 0f
                ? $"미끼\n{bait.RemainingCooldown:F1}초"
                : "미끼";
        }

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

    private float GetCooldownNormalized(ToolId tool)
    {
        return tool switch
        {
            ToolId.Bait when bait != null => bait.CooldownNormalized,
            ToolId.CastNet when castNet != null => castNet.CooldownNormalized,
            _ => 0f
        };
    }

    private void SetCooldownOverlay(
        int slotIndex,
        float normalizedCooldown)
    {
        Image overlay = hotbarCooldownOverlays[slotIndex];
        if (overlay == null)
        {
            return;
        }

        float fill = Mathf.Clamp01(normalizedCooldown);
        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = new Vector2(0f, 1f - fill);
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlay.enabled = fill > 0f;
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
