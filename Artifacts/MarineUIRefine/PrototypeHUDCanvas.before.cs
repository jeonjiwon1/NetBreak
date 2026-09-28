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
        new TMP_Text[ActiveSlotBindings.Length + 1];
    private readonly Image[] hotbarCooldownOverlays =
        new Image[ActiveSlotBindings.Length + 1];
    private readonly Image[] hotbarIcons =
        new Image[ActiveSlotBindings.Length + 1];
    private Image[] speedButtonImages;
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

        speedButtonImages = Area1HUDSkin.ApplyStatic(transform,
            goldText, captureText, catchRateText, levelText, expText,
            stageText, phaseText, runTimeText,
            preparationPanel, startFishingButton);
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
        Area1HUDSkin.RefreshSpeedSelection(speedButtonImages,
            PrototypeGameFlowManager.Instance != null
                ? PrototypeGameFlowManager.Instance.CurrentTestSpeedMultiplier
                : 1f);
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
            goldText.text = run.CurrentGold.ToString();
        }

        if (captureText != null)
        {
            captureText.text = run.CapturedFishCount.ToString();
        }

        if (catchRateText != null)
        {
            catchRateText.text = $"{run.CatchRate * 100f:F1}%";
        }

        if (levelText != null)
        {
            levelText.text = run.CurrentLevel.ToString();
        }

        if (expText != null)
        {
            expText.text = $"{run.CurrentExp} / {run.ExpToNextLevel}";
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
                    $"{fishSpawner.CurrentStageIndex} / {fishSpawner.TotalStageCount}";
            }
            else
            {
                stageText.text = "준비";
            }
        }

        if (phaseText != null)
        {
            phaseText.text = fishSpawner.CurrentPhaseName;
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
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(HorizontalLayoutGroup)
        );
        rootObject.layer = gameObject.layer;
        hotbarRoot = rootObject.GetComponent<RectTransform>();
        hotbarRoot.SetParent(transform, false);
        hotbarRoot.anchorMin = new Vector2(0.5f, 0f);
        hotbarRoot.anchorMax = new Vector2(0.5f, 0f);
        hotbarRoot.pivot = new Vector2(0.5f, 0f);
        hotbarRoot.anchoredPosition = new Vector2(0f, 14f);
        hotbarRoot.sizeDelta = new Vector2(716f, 120f);
        Area1HUDSkin.SetFrame(rootObject.GetComponent<Image>(), "panel", false);

        HorizontalLayoutGroup layout =
            rootObject.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.padding = new RectOffset(10, 10, 7, 7);
        layout.spacing = 4f;
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

        Area1HUDSkin.DecorateHotbar(hotbarRoot);

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
        Area1HUDSkin.SetFrame(background,
            isFixedTool ? "selected_slot" : "slot", false);
        if (background.sprite != null)
        {
            background.color = isFixedTool
                ? Color.white
                : new Color(0.89f, 0.95f, 0.94f, 1f);
        }

        LayoutElement layoutElement =
            slotObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 136f;
        layoutElement.preferredHeight = 106f;

        GameObject overlayObject = new GameObject(
            $"HotbarCooldown_{binding}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlayObject.layer = gameObject.layer;
        overlayObject.transform.SetParent(slotObject.transform, false);

        Image overlay = overlayObject.GetComponent<Image>();
        overlay.color = new Color(0.02f, 0.06f, 0.10f, 0.68f);
        overlay.raycastTarget = false;
        hotbarCooldownOverlays[slotIndex] = overlay;

        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = new Vector2(0f, 1f);
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image icon = Area1HUDSkin.NewImage(
            $"HotbarIcon_{binding}", slotObject.transform,
            slotIndex == 0
                ? Area1HUDSkin.ToolIcon(ToolId.LandingNet)
                : slotIndex == 3
                    ? Area1HUDSkin.SkillIcon(false)
                    : slotIndex == 4
                        ? Area1HUDSkin.SkillIcon(true)
                        : Area1HUDSkin.ToolIcon(ToolId.None));
        icon.enabled = icon.sprite != null;
        icon.preserveAspect = true;
        Area1HUDSkin.Place(icon.rectTransform, new Vector2(0f, -12f),
            new Vector2(38f, 38f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f));
        hotbarIcons[slotIndex] = icon;

        Image keycap = Area1HUDSkin.NewImage(
            $"HotbarKey_{binding}", slotObject.transform,
            Area1HUDSkin.Frame("key"));
        Area1HUDSkin.SetFrame(keycap, "key", false);
        Area1HUDSkin.Place(keycap.rectTransform, new Vector2(6f, -5f),
            new Vector2(binding == "LMB" ? 47f : 29f, 23f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));

        TMP_Text keyText = Instantiate(legacyCastNetText, keycap.transform);
        keyText.gameObject.name = $"HotbarKeyText_{binding}";
        keyText.text = binding;
        keyText.fontSize = 15f;
        keyText.enableAutoSizing = false;
        keyText.enableWordWrapping = false;
        keyText.overflowMode = TextOverflowModes.Truncate;
        keyText.alignment = TextAlignmentOptions.Center;
        keyText.color = new Color(0.94f, 0.98f, 0.96f);
        keyText.raycastTarget = false;
        keyText.rectTransform.anchorMin = Vector2.zero;
        keyText.rectTransform.anchorMax = Vector2.one;
        keyText.rectTransform.offsetMin = Vector2.zero;
        keyText.rectTransform.offsetMax = Vector2.zero;

        TMP_Text slotText = Instantiate(
            legacyCastNetText,
            slotObject.transform
        );

        slotText.gameObject.name = $"HotbarText_{binding}";
        slotText.raycastTarget = false;
        slotText.alignment = TextAlignmentOptions.Center;
        slotText.fontSize = 16f;
        slotText.enableAutoSizing = true;
        slotText.fontSizeMin = 15f;
        slotText.fontSizeMax = 16f;
        slotText.enableWordWrapping = false;
        slotText.overflowMode = TextOverflowModes.Truncate;
        slotText.color = new Color(0.96f, 0.96f, 0.84f);

        RectTransform textRect = slotText.rectTransform;
        Area1HUDSkin.Place(textRect, new Vector2(0f, 4f),
            new Vector2(124f, 52f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f));

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

        hotbarSlotTexts[0].text = "뜰채\n고정 도구";
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
                GetSlotText(tool);
            if (hotbarIcons[i + 1] != null)
            {
                hotbarIcons[i + 1].sprite = Area1HUDSkin.ToolIcon(tool);
                hotbarIcons[i + 1].enabled =
                    hotbarIcons[i + 1].sprite != null;
            }
            SetCooldownOverlay(
                i + 1,
                GetCooldownNormalized(tool)
            );
        }

        TacticalSkillManager tactical = TacticalSkillManager.Instance;
        hotbarSlotTexts[3].text =
            tactical != null ? tactical.HotbarStatus : "잠김\n미니보스 보상";
        SetCooldownOverlay(
            3,
            tactical != null ? tactical.CooldownNormalized : 0f);

        SignatureSkillManager signature = SignatureSkillManager.Instance;
        hotbarSlotTexts[4].text =
            signature != null ? signature.HotbarStatus : "잠김\n보스 보상";
        SetCooldownOverlay(
            4,
            signature != null ? signature.CooldownNormalized : 0f);
    }

    private string GetSlotText(ToolId tool)
    {
        return tool == ToolId.None
            ? "비어 있음"
            : GetToolStatus(tool);
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
            string charges = $"투망 {castNet.CurrentCharges}/{castNet.MaxCharges}";
            return castNet.CurrentCharges == castNet.MaxCharges
                ? charges
                : $"{charges}\n충전 {castNet.CooldownTimer:F1}초";
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

        if (jobText.gameObject.activeSelf)
        {
            jobText.gameObject.SetActive(false);
        }
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
