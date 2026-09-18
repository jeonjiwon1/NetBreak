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
    private static readonly Vector2 ElementLevelPosition = new(-240f, -242f);
    private static readonly Vector2 ElementLevelSize = new(200f, 84f);
    private static readonly Vector2 ElementTooltipPosition = new(-470f, -70f);
    private static readonly Vector2 ElementTooltipSize = new(680f, 650f);

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
            ItemHUD existingHud = existing.GetComponent<ItemHUD>();
            if (existingHud == null ||
                existing.GetComponent<ItemRewardCanvas>() == null)
            {
                Debug.LogError("기존 ItemSystemUI가 부분 구성 상태입니다. 사용자 UI를 보존하기 위해 변경하지 않았습니다.");
                return;
            }

            if (!EnsureElementLevelDisplay(existing, existingHud, FindFont()))
            {
                return;
            }

            if (!EnsureElementSynergyTooltip(existing, existingHud, FindFont()))
            {
                return;
            }

            EnsureManagers(runManagers[0]);
            existing.SetAsLastSibling();
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

            TMP_Text[] slotTexts = BuildHUD(root.transform, hud, font,
                out TMP_Text elementLevelText,
                out GameObject tooltipPanel,
                out TMP_Text tooltipText,
                out GameObject elementSynergyTooltipPanel,
                out TMP_Text elementSynergyTooltipText);
            BuildRewardModal(root.transform, font, out GameObject rewardPanel,
                out TMP_Text titleText, out Button[] buttons, out TMP_Text[] choiceTexts);

            WireHUD(
                hud,
                slotTexts,
                elementLevelText,
                tooltipPanel,
                tooltipText,
                elementSynergyTooltipPanel,
                elementSynergyTooltipText);
            WireRewardCanvas(rewardCanvas, rewardPanel, titleText, buttons, choiceTexts);
            EnsureManagers(runManagers[0]);

            tooltipPanel.SetActive(false);
            elementSynergyTooltipPanel.SetActive(false);
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
        out TMP_Text elementLevelText,
        out GameObject tooltipPanel,
        out TMP_Text tooltipText,
        out GameObject elementSynergyTooltipPanel,
        out TMP_Text elementSynergyTooltipText)
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

        elementLevelText = CreateElementLevelText(root, font);
        ConfigureElementLevelHover(elementLevelText, hud);

        tooltipPanel = CreateRect("ItemTooltip", root,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-450f, -150f), new Vector2(390f, 230f), new Vector2(1f, 1f));
        AddImage(tooltipPanel, PanelColor, false);
        tooltipText = CreateText("TooltipText", tooltipPanel.transform, font, 18f,
            TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
            new Vector2(18f, 16f), new Vector2(-18f, -16f));

        elementSynergyTooltipPanel = CreateElementSynergyTooltip(
            root,
            font,
            out elementSynergyTooltipText);
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
        TMP_Text elementLevelText,
        GameObject tooltipPanel,
        TMP_Text tooltipText,
        GameObject elementSynergyTooltipPanel,
        TMP_Text elementSynergyTooltipText)
    {
        SerializedObject serialized = new(hud);
        SetObjectArray(serialized.FindProperty("slotTexts"), slotTexts);
        serialized.FindProperty("elementLevelText").objectReferenceValue = elementLevelText;
        serialized.FindProperty("tooltipPanel").objectReferenceValue = tooltipPanel;
        serialized.FindProperty("tooltipText").objectReferenceValue = tooltipText;
        serialized.FindProperty("elementSynergyTooltipPanel").objectReferenceValue =
            elementSynergyTooltipPanel;
        serialized.FindProperty("elementSynergyTooltipText").objectReferenceValue =
            elementSynergyTooltipText;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static bool EnsureElementLevelDisplay(
        Transform root,
        ItemHUD hud,
        TMP_FontAsset font)
    {
        SerializedObject serialized = new(hud);
        SerializedProperty property = serialized.FindProperty("elementLevelText");
        TMP_Text elementLevelText = property.objectReferenceValue as TMP_Text;
        Transform namedDisplay = root.Find("ItemElementLevels");
        if (elementLevelText != null &&
            (!elementLevelText.transform.IsChildOf(root) ||
             (namedDisplay != null && namedDisplay != elementLevelText.transform)))
        {
            Debug.LogError(
                "ItemElementLevels 참조가 ItemSystemUI 밖에 있거나 같은 이름의 다른 오브젝트와 충돌합니다. 변경하지 않았습니다.");
            return false;
        }

        if (elementLevelText == null)
        {
            elementLevelText = namedDisplay != null
                ? namedDisplay.GetComponent<TMP_Text>()
                : CreateElementLevelText(root, font);
            if (elementLevelText == null)
            {
                Debug.LogError("기존 ItemElementLevels 오브젝트에 TMP_Text가 없어 설정을 중단했습니다.");
                return false;
            }

            Undo.RecordObject(hud, "Connect Item Element Level Display");
            property.objectReferenceValue = elementLevelText;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(hud);
        }

        ConfigureElementLevelText(elementLevelText, font);
        ConfigureElementLevelHover(elementLevelText, hud);
        return true;
    }

    private static TMP_Text CreateElementLevelText(Transform root, TMP_FontAsset font)
    {
        TMP_Text text = CreateText(
            "ItemElementLevels",
            root,
            font,
            17f,
            TextAlignmentOptions.TopLeft,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            ElementLevelPosition,
            ElementLevelSize);
        ConfigureElementLevelText(text, font);
        return text;
    }

    private static void ConfigureElementLevelText(TMP_Text text, TMP_FontAsset font)
    {
        RectTransform rect = text.rectTransform;
        Undo.RecordObject(rect, "Layout Item Element Levels");
        Undo.RecordObject(text, "Configure Item Element Levels");
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = ElementLevelPosition;
        rect.sizeDelta = ElementLevelSize;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.fontSize = 17f;
        text.enableAutoSizing = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = true;
        if (text.font == null && font != null)
        {
            text.font = font;
        }

        EditorUtility.SetDirty(rect);
        EditorUtility.SetDirty(text);
    }

    private static void ConfigureElementLevelHover(TMP_Text text, ItemHUD hud)
    {
        ItemElementLevelHover hover = text.GetComponent<ItemElementLevelHover>();
        if (hover == null)
        {
            hover = Undo.AddComponent<ItemElementLevelHover>(text.gameObject);
        }
        else
        {
            Undo.RecordObject(hover, "Configure Item Element Hover");
        }

        hover.Configure(hud, text);
        EditorUtility.SetDirty(hover);
    }

    private static bool EnsureElementSynergyTooltip(
        Transform root,
        ItemHUD hud,
        TMP_FontAsset font)
    {
        SerializedObject serialized = new(hud);
        SerializedProperty panelProperty =
            serialized.FindProperty("elementSynergyTooltipPanel");
        SerializedProperty textProperty =
            serialized.FindProperty("elementSynergyTooltipText");
        GameObject panel = panelProperty.objectReferenceValue as GameObject;
        TMP_Text text = textProperty.objectReferenceValue as TMP_Text;
        Transform namedPanel = root.Find("ElementSynergyTooltip");

        if ((panel != null && !panel.transform.IsChildOf(root)) ||
            (namedPanel != null && panel != null && namedPanel.gameObject != panel) ||
            (text != null && !text.transform.IsChildOf(root)))
        {
            Debug.LogError(
                "ElementSynergyTooltip 참조가 ItemSystemUI 밖에 있거나 같은 이름과 충돌합니다. 변경하지 않았습니다.");
            return false;
        }

        if (panel == null)
        {
            if (namedPanel != null)
            {
                panel = namedPanel.gameObject;
                text = namedPanel.Find("TooltipText")?.GetComponent<TMP_Text>();
                if (text == null)
                {
                    Debug.LogError(
                        "기존 ElementSynergyTooltip에 TooltipText TMP가 없어 설정을 중단했습니다.");
                    return false;
                }
            }
            else
            {
                panel = CreateElementSynergyTooltip(root, font, out text);
            }
        }
        else if (text == null)
        {
            text = panel.transform.Find("TooltipText")?.GetComponent<TMP_Text>();
            if (text == null)
            {
                Debug.LogError(
                    "연결된 ElementSynergyTooltip에 TooltipText TMP가 없어 설정을 중단했습니다.");
                return false;
            }
        }

        ConfigureElementSynergyTooltip(panel, text, font);
        Undo.RecordObject(hud, "Connect Element Synergy Tooltip");
        panelProperty.objectReferenceValue = panel;
        textProperty.objectReferenceValue = text;
        serialized.ApplyModifiedProperties();
        panel.SetActive(false);
        EditorUtility.SetDirty(hud);
        return true;
    }

    private static GameObject CreateElementSynergyTooltip(
        Transform root,
        TMP_FontAsset font,
        out TMP_Text text)
    {
        GameObject panel = CreateRect(
            "ElementSynergyTooltip",
            root,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            ElementTooltipPosition,
            ElementTooltipSize,
            new Vector2(1f, 1f));
        AddImage(panel, PanelColor, false);
        text = CreateText(
            "TooltipText",
            panel.transform,
            font,
            16.5f,
            TextAlignmentOptions.TopLeft,
            Vector2.zero,
            Vector2.one,
            new Vector2(22f, 20f),
            new Vector2(-22f, -20f));
        ConfigureElementSynergyTooltip(panel, text, font);
        return panel;
    }

    private static void ConfigureElementSynergyTooltip(
        GameObject panel,
        TMP_Text text,
        TMP_FontAsset font)
    {
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            throw new System.InvalidOperationException(
                "ElementSynergyTooltip requires a RectTransform.");
        }

        Undo.RecordObject(panelRect, "Layout Element Synergy Tooltip");
        Undo.RecordObject(text, "Configure Element Synergy Tooltip Text");
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = ElementTooltipPosition;
        panelRect.sizeDelta = ElementTooltipSize;

        Image image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = Undo.AddComponent<Image>(panel);
        }
        Undo.RecordObject(image, "Configure Element Synergy Tooltip Background");
        image.color = PanelColor;
        image.raycastTarget = false;

        RectTransform textRect = text.rectTransform;
        Undo.RecordObject(textRect, "Layout Element Synergy Tooltip Text");
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(22f, 20f);
        textRect.offsetMax = new Vector2(-22f, -20f);
        text.fontSize = 16.5f;
        text.enableAutoSizing = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        if (text.font == null && font != null)
        {
            text.font = font;
        }

        panel.transform.SetAsLastSibling();
        EditorUtility.SetDirty(panelRect);
        EditorUtility.SetDirty(textRect);
        EditorUtility.SetDirty(text);
        EditorUtility.SetDirty(image);
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
