using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class ItemSystemUIGenerator
{
    private const string MenuPath = "NETBREAK/UI/Setup Item System UI";
    private const string CanvasName = "GameCanvas";
    private const string RootName = "ItemSystemUI";
    private const string FontName = "NanumGothic-Bold SDF";

    private static readonly Color PanelColor = new(0.035f, 0.075f, 0.11f, 0.97f);
    private static readonly Color SurfaceColor = new(0.08f, 0.15f, 0.2f, 0.97f);
    private static readonly Color AccentColor = new(0.2f, 0.62f, 0.72f, 1f);
    private static readonly Color TextColor = new(0.93f, 0.97f, 1f, 1f);

    [MenuItem(MenuPath)]
    private static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Play Mode에서는 Item System UI를 설정할 수 없습니다.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(candidate => candidate.gameObject.scene == scene && candidate.name == CanvasName)
            .ToArray();
        RunManager[] runManagers = Object.FindObjectsByType<RunManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(candidate => candidate.gameObject.scene == scene)
            .ToArray();

        if (canvases.Length != 1 || runManagers.Length != 1)
        {
            Debug.LogError(
                $"Item System UI setup requires exactly one '{CanvasName}' and one RunManager. " +
                $"Found canvases: {canvases.Length}, RunManagers: {runManagers.Length}.");
            return;
        }

        ItemRewardManager[] managers = Object.FindObjectsByType<ItemRewardManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(candidate => candidate.gameObject.scene == scene)
            .ToArray();
        if (managers.Length > 1 ||
            (managers.Length == 1 && managers[0].gameObject != runManagers[0].gameObject))
        {
            Debug.LogError("RunManager 외부에 ItemRewardManager가 있거나 중복되어 설정을 중단했습니다.");
            return;
        }

        ItemEffectManager[] effectManagers = Object.FindObjectsByType<ItemEffectManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(candidate => candidate.gameObject.scene == scene)
            .ToArray();
        if (effectManagers.Length > 1 ||
            (effectManagers.Length == 1 && effectManagers[0].gameObject != runManagers[0].gameObject))
        {
            Debug.LogError("RunManager 외부에 ItemEffectManager가 있거나 중복되어 설정을 중단했습니다.");
            return;
        }

        Transform existing = canvases[0].transform.Find(RootName);
        if (existing != null)
        {
            if (existing.GetComponent<ItemHUD>() == null ||
                existing.GetComponent<ItemRewardCanvas>() == null)
            {
                Debug.LogError("기존 ItemSystemUI가 부분 구성 상태입니다. 사용자 UI를 보존하기 위해 변경하지 않았습니다.");
                return;
            }

            EnsureManagers(runManagers[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("Item System UI가 이미 설정되어 있습니다. 기존 UI를 재사용했습니다.", existing.gameObject);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup NETBREAK Item System UI");

        try
        {
            TMP_FontAsset font = FindFont();
            GameObject root = CreateRect(RootName, canvases[0].transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ItemHUD hud = Undo.AddComponent<ItemHUD>(root);
            ItemRewardCanvas rewardCanvas = Undo.AddComponent<ItemRewardCanvas>(root);

            TMP_Text[] slotTexts = BuildHUD(root.transform, hud, font, out GameObject tooltipPanel,
                out TMP_Text tooltipText);
            BuildRewardModal(root.transform, font, out GameObject rewardPanel,
                out TMP_Text titleText, out Button[] buttons, out TMP_Text[] choiceTexts);

            WireHUD(hud, slotTexts, tooltipPanel, tooltipText);
            WireRewardCanvas(rewardCanvas, rewardPanel, titleText, buttons, choiceTexts);
            EnsureManagers(runManagers[0]);

            tooltipPanel.SetActive(false);
            rewardPanel.SetActive(false);
            root.transform.SetAsLastSibling();
            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(rewardCanvas);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = root;

            string fontStatus = font != null ? font.name : "TMP 기본 폰트";
            Debug.Log(
                $"Item System UI 설정 완료. RunManager와 4슬롯 HUD/툴팁/필수 3택 모달을 연결했습니다. 폰트: {fontStatus}.",
                root);
        }
        catch (System.Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogException(exception);
        }
    }

    private static TMP_Text[] BuildHUD(
        Transform root,
        ItemHUD hud,
        TMP_FontAsset font,
        out GameObject tooltipPanel,
        out TMP_Text tooltipText)
    {
        GameObject hudPanel = CreateRect("ItemHUD", root,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -150f), new Vector2(420f, 86f), new Vector2(1f, 1f));
        AddImage(hudPanel, PanelColor, false);

        TMP_Text[] texts = new TMP_Text[RunItemInventory.Capacity];
        for (int i = 0; i < texts.Length; i++)
        {
            GameObject slot = CreateRect($"ItemSlot_{i + 1}", hudPanel.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(54f + i * 104f, 0f), new Vector2(96f, 68f));
            AddImage(slot, SurfaceColor, true);
            texts[i] = CreateText("Label", slot.transform, font, 17f,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(-8f, -6f));
            ItemHUDSlotHover hover = Undo.AddComponent<ItemHUDSlotHover>(slot);
            hover.Configure(hud, i);
        }

        tooltipPanel = CreateRect("ItemTooltip", root,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-450f, -150f), new Vector2(390f, 230f), new Vector2(1f, 1f));
        AddImage(tooltipPanel, PanelColor, false);
        tooltipText = CreateText("TooltipText", tooltipPanel.transform, font, 18f,
            TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
            new Vector2(18f, 16f), new Vector2(-18f, -16f));
        return texts;
    }

    private static void BuildRewardModal(
        Transform root,
        TMP_FontAsset font,
        out GameObject rewardPanel,
        out TMP_Text titleText,
        out Button[] buttons,
        out TMP_Text[] choiceTexts)
    {
        rewardPanel = CreateRect("ItemRewardModal", root,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddImage(rewardPanel, new Color(0f, 0f, 0f, 0.82f), true);

        titleText = CreateText("Title", rewardPanel.transform, font, 34f,
            TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -105f), new Vector2(900f, 100f));

        buttons = new Button[3];
        choiceTexts = new TMP_Text[3];
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject card = CreateRect($"ItemChoice_{i + 1}", rewardPanel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2((i - 1) * 400f, -10f), new Vector2(360f, 390f));
            Image image = AddImage(card, SurfaceColor, true);
            buttons[i] = Undo.AddComponent<Button>(card);
            buttons[i].targetGraphic = image;
            ColorBlock colors = buttons[i].colors;
            colors.highlightedColor = AccentColor;
            colors.pressedColor = new Color(0.12f, 0.42f, 0.52f, 1f);
            buttons[i].colors = colors;
            choiceTexts[i] = CreateText("ChoiceText", card.transform, font, 21f,
                TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
                new Vector2(22f, 22f), new Vector2(-22f, -22f));
        }

        TMP_Text footer = CreateText("Footer", rewardPanel.transform, font, 18f,
            TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 55f), new Vector2(950f, 48f));
        footer.text = "아이템은 도구와 독립적으로 작동하며 추가 입력을 사용하지 않습니다.";
    }

    private static void WireHUD(
        ItemHUD hud,
        TMP_Text[] slotTexts,
        GameObject tooltipPanel,
        TMP_Text tooltipText)
    {
        SerializedObject serialized = new(hud);
        SetObjectArray(serialized.FindProperty("slotTexts"), slotTexts);
        serialized.FindProperty("tooltipPanel").objectReferenceValue = tooltipPanel;
        serialized.FindProperty("tooltipText").objectReferenceValue = tooltipText;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireRewardCanvas(
        ItemRewardCanvas canvas,
        GameObject panel,
        TMP_Text title,
        Button[] buttons,
        TMP_Text[] texts)
    {
        SerializedObject serialized = new(canvas);
        serialized.FindProperty("rewardPanel").objectReferenceValue = panel;
        serialized.FindProperty("titleText").objectReferenceValue = title;
        SetObjectArray(serialized.FindProperty("choiceButtons"), buttons);
        SetObjectArray(serialized.FindProperty("choiceTexts"), texts);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray<T>(SerializedProperty property, T[] values)
        where T : Object
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void EnsureManagers(RunManager runManager)
    {
        ItemRewardManager manager = runManager.GetComponent<ItemRewardManager>();
        if (manager == null)
        {
            Undo.AddComponent<ItemRewardManager>(runManager.gameObject);
        }

        ItemEffectManager effectManager = runManager.GetComponent<ItemEffectManager>();
        if (effectManager == null)
        {
            Undo.AddComponent<ItemEffectManager>(runManager.gameObject);
        }
    }

    private static GameObject CreateRect(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2? pivot = null)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        return gameObject;
    }

    private static Image AddImage(GameObject gameObject, Color color, bool raycastTarget)
    {
        Image image = Undo.AddComponent<Image>(gameObject);
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        TMP_FontAsset font,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMaxOrSize)
    {
        GameObject gameObject = CreateRect(name, parent, anchorMin, anchorMax,
            Vector2.zero, Vector2.zero);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        if (anchorMin == anchorMax)
        {
            rect.anchoredPosition = offsetMin;
            rect.sizeDelta = offsetMaxOrSize;
        }
        else
        {
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMaxOrSize;
        }

        TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(gameObject);
        if (font != null)
        {
            text.font = font;
        }
        text.fontSize = fontSize;
        text.color = TextColor;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static TMP_FontAsset FindFont()
    {
        string[] guids = AssetDatabase.FindAssets($"{FontName} t:TMP_FontAsset");
        for (int i = 0; i < guids.Length; i++)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                AssetDatabase.GUIDToAssetPath(guids[i]));
            if (font != null && font.name == FontName)
            {
                return font;
            }
        }

        return null;
    }
}
