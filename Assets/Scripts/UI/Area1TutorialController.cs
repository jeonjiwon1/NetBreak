using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Contextual presentation only. Gameplay owns every trigger and remains playable.
[DisallowMultipleComponent]
public sealed class Area1TutorialController : MonoBehaviour
{
    private enum Step { Goal, CoreTool, PartnerTool, TacticalSkill, BossRule }

    private const string KeyPrefix = "NetBreak.Area1Tutorial.v1.";
    private const float DisplaySeconds = 4.5f;
    private readonly Queue<(Step step, string message)> pending = new();
    private CanvasGroup group;
    private TMP_Text label;
    private bool showing;
    private float endTime;

    public static Area1TutorialController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        if (flow == null || flow.IsGameEnded)
        {
            ClearPending();
            return;
        }

        if (group == null) BuildPanel();
        if (group == null) return;

        if (SelectionOpen())
        {
            if (showing) endTime += Time.unscaledDeltaTime;
            group.alpha = 0f;
            return;
        }

        if (showing && Time.realtimeSinceStartup >= endTime)
            showing = false;

        if (!showing && pending.Count > 0)
        {
            var next = pending.Dequeue();
            label.text = next.message;
            PlayerPrefs.SetInt(Key(next.step), 1);
            PlayerPrefs.Save();
            showing = true;
            endTime = Time.realtimeSinceStartup + DisplaySeconds;
        }

        group.alpha = Mathf.MoveTowards(group.alpha, showing ? 1f : 0f,
            Time.unscaledDeltaTime * 5f);
    }

    public void NotifyFishingStarted() => Request(Step.Goal,
        "[LMB] 뜰채로 저항을 0으로 낮춰 포획하세요.");

    public void NotifyToolAcquired(GrowthToolRole role, ToolId tool)
    {
        string binding = role == GrowthToolRole.Core ? "Q" : "W";
        string action = tool switch
        {
            ToolId.Net => "선택 후 마우스로 드래그해 그물을 설치하세요.",
            ToolId.FishingRod => "선택 후 클릭해 낚싯대를 설치하세요.",
            ToolId.CastNet => "누른 채 조준하고 놓아 투망을 던지세요.",
            ToolId.Bait => "커서를 놓은 곳에 미끼를 던지세요.",
            _ => null
        };
        if (action == null) return;
        Request(role == GrowthToolRole.Core ? Step.CoreTool : Step.PartnerTool,
            $"[{binding}] {action}");
    }

    public void NotifyTacticalSkillAcquired() => Request(Step.TacticalSkill,
        "[E] 선택한 전술 스킬을 사용하세요.");

    public void NotifyBossStarted()
    {
        ClearPending();
        int passes = BossEncounterController.Instance != null
            ? BossEncounterController.Instance.MaxPasses : 3;
        Request(Step.BossRule, $"상어를 {passes}번의 회유 안에 포획하세요.");
    }

    public void ClearPending()
    {
        pending.Clear();
        showing = false;
        if (group != null) group.alpha = 0f;
    }

    public static void ResetProgress()
    {
        foreach (Step step in System.Enum.GetValues(typeof(Step)))
            PlayerPrefs.DeleteKey(Key(step));
        PlayerPrefs.Save();
        Instance?.ClearPending();
    }

    private void Request(Step step, string message)
    {
        if (PlayerPrefs.GetInt(Key(step), 0) != 0) return;
        foreach (var item in pending)
            if (item.step == step) return;
        pending.Enqueue((step, message));
    }

    private static string Key(Step step) => KeyPrefix + step;

    private static bool SelectionOpen() =>
        (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsOpen) ||
        (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsChoosingTool) ||
        (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
        (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
        TacticalSkillManager.IsSelectionPendingOrActive ||
        ItemRewardManager.IsSelectionPendingOrActive ||
        (PrototypeGameFlowManager.Instance != null && PrototypeGameFlowManager.Instance.IsBossRewardPending);

    private void BuildPanel()
    {
        PrototypeHUDCanvas hud = FindFirstObjectByType<PrototypeHUDCanvas>();
        if (hud == null) return;
        GameObject panel = new("Area1TutorialPanel", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.layer = hud.gameObject.layer;
        panel.transform.SetParent(hud.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 150f);
        rect.sizeDelta = new Vector2(640f, 64f);
        Area1HUDSkin.SetFrame(panel.GetComponent<Image>(), "ref_hud_board", false);
        group = panel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        GameObject textObject = new("TutorialText", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = panel.layer;
        textObject.transform.SetParent(rect, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 8f);
        textRect.offsetMax = new Vector2(-20f, -8f);
        label = textObject.GetComponent<TextMeshProUGUI>();
        Area1Typography.Apply(label, Area1Typography.Role.Body);
        label.fontSize = 23f;
        label.color = new Color32(247, 239, 207, 255);
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.raycastTarget = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
