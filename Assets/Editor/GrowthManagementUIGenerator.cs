using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class GrowthManagementUIGenerator
{
    private const string MenuPath =
        "NETBREAK/UI/Migrate Growth Window To Skill Tree + Item";
    private const string NavigationName = "GrowthNavigation";
    private const string SkillPageName = "SkillTreePage";
    private const string ItemPageName = "ItemPage";
    private const string FontName = "NanumGothic-Bold SDF";

    private static readonly Color PanelColor =
        new(0.035f, 0.075f, 0.11f, 0.98f);
    private static readonly Color SurfaceColor =
        new(0.08f, 0.15f, 0.2f, 0.98f);
    private static readonly Color ButtonColor =
        new(0.12f, 0.32f, 0.42f, 1f);
    private static readonly Color TextColor =
        new(0.93f, 0.97f, 1f, 1f);
    private static readonly Color MutedColor =
        new(0.7f, 0.79f, 0.84f, 1f);

    [MenuItem(MenuPath)]
    private static void Migrate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Stop("Play Mode에서는 성장 관리 UI를 이관할 수 없습니다.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        SkillTreeCanvas[] controllers = Object.FindObjectsByType<SkillTreeCanvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(candidate => candidate.gameObject.scene == scene)
            .ToArray();
        ItemHUD[] itemHuds = Object.FindObjectsByType<ItemHUD>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(candidate => candidate.gameObject.scene == scene)
            .ToArray();
        if (controllers.Length != 1 || itemHuds.Length != 1)
        {
            Stop(
                "활성 Scene에 SkillTreeCanvas와 기존 ItemHUD가 각각 정확히 하나 있어야 합니다. " +
                $"현재 SkillTreeCanvas {controllers.Length}개, ItemHUD {itemHuds.Length}개입니다.");
            return;
        }

        SkillTreeCanvas controller = controllers[0];
        GameObject treePanel = GetReference<GameObject>(controller, "treePanel");
        if (treePanel == null || controller.transform.parent == null ||
            treePanel.transform.parent != controller.transform)
        {
            Stop("기존 SkillTreeUI/TreePanel 참조가 예상 구조와 달라 이관을 중단했습니다.");
            return;
        }

        Transform navigation = treePanel.transform.Find(NavigationName);
        Transform skillPage = treePanel.transform.Find(SkillPageName);
        Transform itemPage = treePanel.transform.Find(ItemPageName);
        int existingCount = (navigation != null ? 1 : 0) +
            (skillPage != null ? 1 : 0) + (itemPage != null ? 1 : 0);
        if (existingCount > 0 && existingCount < 3)
        {
            Stop("성장 관리 페이지가 부분 구성 상태입니다. 사용자 편집을 보호하기 위해 변경하지 않았습니다.");
            return;
        }

        if (existingCount == 3)
        {
            GrowthItemPage itemView = itemPage.GetComponent<GrowthItemPage>();
            if (itemView == null ||
                GetReference<GameObject>(controller, "navigationRoot") != navigation.gameObject ||
                GetReference<GameObject>(controller, "skillTreePage") != skillPage.gameObject ||
                GetReference<GameObject>(controller, "itemPage") != itemPage.gameObject ||
                GetReference<GrowthItemPage>(controller, "itemPageView") != itemView)
            {
                Stop("기존 성장 관리 UI의 참조가 불완전하거나 이름이 충돌합니다.");
                return;
            }

            Selection.activeGameObject = controller.gameObject;
            Debug.Log("성장 관리 UI가 이미 안전하게 이관되어 있습니다.", controller);
            return;
        }

        if (!ValidateExistingSkillTree(controller, treePanel))
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Migrate NETBREAK Growth Management UI");
        try
        {
            TMP_FontAsset font = FindFont();
            Button closeButton = GetReference<Button>(controller, "closeButton");
            TMP_Text masteryText = GetReference<TMP_Text>(controller, "masteryPointText");
            TMP_Text noticeText = GetReference<TMP_Text>(controller, "noticeText");
            Transform header = closeButton != null ? closeButton.transform.parent : null;
            if (header == null || header.parent != treePanel.transform ||
                masteryText == null || noticeText == null ||
                masteryText.transform.parent != header ||
                noticeText.transform.parent != header)
            {
                throw new InvalidOperationException(
                    "기존 Header/닫기/숙련 포인트/공지 참조를 안전하게 보존할 수 없습니다.");
            }

            Transform[] existingChildren = treePanel.transform.Cast<Transform>()
                .Where(child => child != header)
                .ToArray();
            GameObject skillRoot = CreateRect(
                SkillPageName, treePanel.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            Undo.RegisterCreatedObjectUndo(skillRoot, "Create Skill Tree Page");

            for (int i = 0; i < existingChildren.Length; i++)
            {
                RectTransform rect = existingChildren[i] as RectTransform;
                RectSnapshot snapshot = new(rect);
                Undo.SetTransformParent(
                    existingChildren[i], skillRoot.transform,
                    "Wrap Existing Skill Tree Content");
                snapshot.Restore(rect);
                existingChildren[i].SetSiblingIndex(i);
            }

            GameObject itemRoot = CreateRect(
                ItemPageName, treePanel.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            Undo.RegisterCreatedObjectUndo(itemRoot, "Create Item Management Page");
            GrowthItemPage itemView = Undo.AddComponent<GrowthItemPage>(itemRoot);
            BuildItemPage(itemRoot.transform, itemView, font);

            GameObject navRoot = CreateRect(
                NavigationName, treePanel.transform,
                new Vector2(0.36f, 0.825f), new Vector2(0.64f, 0.875f),
                Vector2.zero, Vector2.zero);
            Undo.RegisterCreatedObjectUndo(navRoot, "Create Growth Navigation");
            AddImage(navRoot, SurfaceColor, true);
            Button skillButton = CreateButton(
                "SkillTreeTab", navRoot.transform, "스킬 트리", font,
                new Vector2(0f, 0f), new Vector2(0.5f, 1f),
                new Vector2(4f, 4f), new Vector2(-3f, -4f));
            Button itemButton = CreateButton(
                "ItemTab", navRoot.transform, "아이템", font,
                new Vector2(0.5f, 0f), new Vector2(1f, 1f),
                new Vector2(3f, 4f), new Vector2(-4f, -4f));

            SetReference(controller, "navigationRoot", navRoot);
            SetReference(controller, "skillTreeTabButton", skillButton);
            SetReference(controller, "itemTabButton", itemButton);
            SetReference(controller, "skillTreePage", skillRoot);
            SetReference(controller, "itemPage", itemRoot);
            SetReference(controller, "itemPageView", itemView);

            skillRoot.SetActive(true);
            itemRoot.SetActive(false);
            navRoot.SetActive(true);
            skillRoot.transform.SetAsFirstSibling();
            itemRoot.transform.SetSiblingIndex(1);
            header.SetSiblingIndex(2);
            navRoot.transform.SetAsLastSibling();

            TMP_Text headerTitle = header.GetComponentsInChildren<TMP_Text>(true)
                .SingleOrDefault(text => text.name == "TitleText");
            if (headerTitle == null)
            {
                throw new InvalidOperationException("Header/TitleText를 유일하게 찾을 수 없습니다.");
            }
            SetText(headerTitle, "성장 관리");
            Transform openButton = controller.transform.Find("OpenTreeButton");
            TMP_Text openButtonLabel = openButton != null
                ? openButton.GetComponentsInChildren<TMP_Text>(true)
                    .SingleOrDefault(text => text.name == "Label")
                : null;
            if (openButtonLabel != null)
            {
                SetText(openButtonLabel, "성장 관리 [Tab]");
            }

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(itemView);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = controller.gameObject;
            Debug.Log(
                "기존 SkillTreeUI를 스킬 트리/아이템 2페이지 성장 관리 창으로 이관했습니다. " +
                "기존 Q/W/E/R 오브젝트와 직렬화 참조는 그대로 보존했습니다.",
                controller);
        }
        catch (Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(
                "성장 관리 UI 이관 실패",
                "변경을 Undo로 되돌렸습니다. Console을 확인하세요.",
                "확인");
        }
    }

    private static bool ValidateExistingSkillTree(
        SkillTreeCanvas controller,
        GameObject treePanel)
    {
        SkillTreeBranchView[] branches =
        {
            GetReference<SkillTreeBranchView>(controller, "coreBranch"),
            GetReference<SkillTreeBranchView>(controller, "partnerBranch"),
            GetReference<SkillTreeBranchView>(controller, "tacticalBranch"),
            GetReference<SkillTreeBranchView>(controller, "signatureBranch")
        };
        if (branches.Any(branch => branch == null ||
            branch.transform.parent != treePanel.transform))
        {
            Stop("Q/W/E/R Branch 참조가 비어 있거나 TreePanel의 직접 자식이 아니어서 안전하게 감쌀 수 없습니다.");
            return false;
        }

        GameObject acquisitionPanel =
            GetReference<GameObject>(controller, "acquisitionPanel");
        if (acquisitionPanel == null ||
            acquisitionPanel.transform.parent != treePanel.transform)
        {
            Stop("기존 필수 도구 선택 모달을 찾을 수 없어 이관을 중단했습니다.");
            return false;
        }

        return true;
    }

    private static void BuildItemPage(
        Transform root,
        GrowthItemPage itemView,
        TMP_FontAsset font)
    {
        GameObject inventoryPanel = CreatePanel(
            "OwnedItems", root,
            new Vector2(0.025f, 0.53f), new Vector2(0.49f, 0.8f));
        CreateText("Title", inventoryPanel.transform, "보유 아이템", font, 23f,
            TextAlignmentOptions.Center,
            new Vector2(0f, 0.82f), new Vector2(1f, 0.98f));
        TMP_Text[] slotLabels = new TMP_Text[RunItemInventory.Capacity];
        for (int i = 0; i < slotLabels.Length; i++)
        {
            float left = 0.03f + i * 0.242f;
            GameObject slot = CreatePanel(
                $"OwnedItemSlot_{i + 1}", inventoryPanel.transform,
                new Vector2(left, 0.08f), new Vector2(left + 0.215f, 0.78f),
                SurfaceColor, true);
            slotLabels[i] = CreateText(
                "Label", slot.transform, $"슬롯 {i + 1}\n비어 있음", font, 17f,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one,
                new Vector2(8f, 8f), new Vector2(-8f, -8f));
            GrowthItemSlotHover hover = Undo.AddComponent<GrowthItemSlotHover>(slot);
            hover.Configure(itemView, i);
        }

        GameObject elementPanel = CreatePanel(
            "ElementSynergies", root,
            new Vector2(0.51f, 0.53f), new Vector2(0.975f, 0.8f));
        CreateText("Title", elementPanel.transform, "속성 레벨 / 단일 시너지", font, 23f,
            TextAlignmentOptions.Center,
            new Vector2(0f, 0.82f), new Vector2(1f, 0.98f));
        ItemElement[] elements =
        {
            ItemElement.Electric,
            ItemElement.Sword,
            ItemElement.Ice
        };
        GameObject[] elementRoots = new GameObject[elements.Length];
        TMP_Text[] elementLabels = new TMP_Text[elements.Length];
        for (int i = 0; i < elements.Length; i++)
        {
            float top = 0.77f - i * 0.22f;
            elementRoots[i] = CreatePanel(
                $"{elements[i]}Entry", elementPanel.transform,
                new Vector2(0.08f, top - 0.16f), new Vector2(0.92f, top),
                SurfaceColor, true);
            elementLabels[i] = CreateText(
                "Label", elementRoots[i].transform,
                $"{GetElementName(elements[i])} Lv.0", font, 19f,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            GrowthElementHover hover =
                Undo.AddComponent<GrowthElementHover>(elementRoots[i]);
            hover.Configure(itemView, elements[i]);
        }
        TMP_Text emptyElement = CreateText(
            "EmptyState", elementPanel.transform, "보유한 아이템이 없습니다.",
            font, 18f, TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.76f));
        emptyElement.color = MutedColor;

        GameObject combinedPanel = CreatePanel(
            "CombinedSynergies", root,
            new Vector2(0.025f, 0.045f), new Vector2(0.975f, 0.505f));
        CreateText("Title", combinedPanel.transform, "복합 시너지", font, 23f,
            TextAlignmentOptions.Center,
            new Vector2(0f, 0.88f), new Vector2(1f, 0.99f));
        int combinedCount = CombinedSynergyCatalog.All.Count;
        Button[] cardButtons = new Button[combinedCount];
        Image[] cardBackgrounds = new Image[combinedCount];
        TMP_Text[] cardLabels = new TMP_Text[combinedCount];
        for (int i = 0; i < combinedCount; i++)
        {
            float left = 0.018f + i * 0.329f;
            GameObject card = CreateRect(
                $"CombinedCard_{i + 1}", combinedPanel.transform,
                new Vector2(left, 0.43f), new Vector2(left + 0.305f, 0.85f),
                Vector2.zero, Vector2.zero);
            cardBackgrounds[i] = AddImage(card, SurfaceColor, true);
            cardButtons[i] = Undo.AddComponent<Button>(card);
            cardButtons[i].targetGraphic = cardBackgrounds[i];
            cardLabels[i] = CreateText(
                "Label", card.transform, string.Empty, font, 15.5f,
                TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
                new Vector2(14f, 12f), new Vector2(-14f, -12f));
        }

        GameObject detailPanel = CreatePanel(
            "CombinedDetail", combinedPanel.transform,
            new Vector2(0.025f, 0.075f), new Vector2(0.69f, 0.38f),
            new Color(0.055f, 0.105f, 0.14f, 0.96f), false);
        TMP_Text detailText = CreateText(
            "Text", detailPanel.transform, "복합 시너지 상세 설명", font, 15.5f,
            TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
            new Vector2(12f, 8f), new Vector2(-12f, -8f));
        TMP_Text cooldownText = CreateText(
            "CooldownText", combinedPanel.transform, "변경 대기 없음", font, 16f,
            TextAlignmentOptions.Center,
            new Vector2(0.715f, 0.25f), new Vector2(0.975f, 0.38f));
        Button confirmButton = CreateButton(
            "ConfirmButton", combinedPanel.transform, "이 복합 시너지로 변경", font,
            new Vector2(0.715f, 0.075f), new Vector2(0.975f, 0.23f),
            Vector2.zero, Vector2.zero);
        TMP_Text confirmLabel = confirmButton.GetComponentInChildren<TMP_Text>(true);
        confirmButton.interactable = false;

        GameObject itemTooltip = CreateTooltip(
            "GrowthItemTooltip", root, font, new Vector2(570f, 390f),
            out TMP_Text itemTooltipText, 16.5f);
        GameObject elementTooltip = CreateTooltip(
            "GrowthElementTooltip", root, font, new Vector2(680f, 650f),
            out TMP_Text elementTooltipText, 16.5f);

        SerializedObject serialized = new(itemView);
        SerializedProperty slots = serialized.FindProperty("itemSlots");
        slots.arraySize = slotLabels.Length;
        for (int i = 0; i < slotLabels.Length; i++)
        {
            slots.GetArrayElementAtIndex(i)
                .FindPropertyRelative("label").objectReferenceValue = slotLabels[i];
        }

        SerializedProperty entries = serialized.FindProperty("elementEntries");
        entries.arraySize = elements.Length;
        for (int i = 0; i < elements.Length; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("element").enumValueIndex = (int)elements[i];
            entry.FindPropertyRelative("root").objectReferenceValue = elementRoots[i];
            entry.FindPropertyRelative("label").objectReferenceValue = elementLabels[i];
        }

        SerializedProperty cards = serialized.FindProperty("combinedCards");
        cards.arraySize = combinedCount;
        for (int i = 0; i < combinedCount; i++)
        {
            SerializedProperty card = cards.GetArrayElementAtIndex(i);
            card.FindPropertyRelative("id").enumValueIndex =
                (int)CombinedSynergyCatalog.All[i].Id;
            card.FindPropertyRelative("button").objectReferenceValue = cardButtons[i];
            card.FindPropertyRelative("background").objectReferenceValue =
                cardBackgrounds[i];
            card.FindPropertyRelative("label").objectReferenceValue = cardLabels[i];
        }

        serialized.FindProperty("emptyElementText").objectReferenceValue = emptyElement;
        serialized.FindProperty("combinedDetailText").objectReferenceValue = detailText;
        serialized.FindProperty("combinedCooldownText").objectReferenceValue = cooldownText;
        serialized.FindProperty("confirmButton").objectReferenceValue = confirmButton;
        serialized.FindProperty("confirmButtonLabel").objectReferenceValue = confirmLabel;
        serialized.FindProperty("itemTooltipPanel").objectReferenceValue = itemTooltip;
        serialized.FindProperty("itemTooltipText").objectReferenceValue = itemTooltipText;
        serialized.FindProperty("elementTooltipPanel").objectReferenceValue = elementTooltip;
        serialized.FindProperty("elementTooltipText").objectReferenceValue = elementTooltipText;
        serialized.FindProperty("tooltipBounds").objectReferenceValue = root as RectTransform;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        itemTooltip.SetActive(false);
        elementTooltip.SetActive(false);
    }

    private static GameObject CreateTooltip(
        string name,
        Transform parent,
        TMP_FontAsset font,
        Vector2 size,
        out TMP_Text text,
        float fontSize)
    {
        GameObject panel = CreateRect(
            name, parent,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            -size * 0.5f, size * 0.5f);
        AddImage(panel, PanelColor, false);
        text = CreateText(
            "TooltipText", panel.transform, string.Empty, font, fontSize,
            TextAlignmentOptions.TopLeft, Vector2.zero, Vector2.one,
            new Vector2(18f, 16f), new Vector2(-18f, -16f));
        text.raycastTarget = false;
        return panel;
    }

    private static GameObject CreatePanel(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color? color = null,
        bool raycast = false)
    {
        GameObject panel = CreateRect(
            name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        AddImage(panel, color ?? PanelColor, raycast);
        return panel;
    }

    private static GameObject CreateRect(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.layer = parent.gameObject.layer;
        RectTransform rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
        return gameObject;
    }

    private static Image AddImage(GameObject target, Color color, bool raycast)
    {
        Image image = target.GetComponent<Image>() ?? target.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = raycast;
        return image;
    }

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        TMP_FontAsset font,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2? offsetMin = null,
        Vector2? offsetMax = null)
    {
        GameObject gameObject = CreateRect(
            name, parent, anchorMin, anchorMax,
            offsetMin ?? Vector2.zero, offsetMax ?? Vector2.zero);
        TextMeshProUGUI text = gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = TextColor;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        if (font != null)
        {
            text.font = font;
        }
        return text;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        TMP_FontAsset font,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject gameObject = CreateRect(
            name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = AddImage(gameObject, ButtonColor, true);
        Button button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(ButtonColor, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(ButtonColor, Color.black, 0.18f);
        colors.disabledColor = new Color(0.16f, 0.24f, 0.27f, 0.9f);
        button.colors = colors;
        CreateText(
            "Label", gameObject.transform, label, font, 18f,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one,
            new Vector2(8f, 5f), new Vector2(-8f, -5f));
        return button;
    }

    private static T GetReference<T>(Object target, string propertyName)
        where T : Object
    {
        SerializedObject serialized = new(target);
        return serialized.FindProperty(propertyName)?.objectReferenceValue as T;
    }

    private static void SetReference(
        Object target,
        string propertyName,
        Object value)
    {
        Undo.RecordObject(target, "Wire Growth Management UI");
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            throw new InvalidOperationException(
                $"{target.GetType().Name}.{propertyName} 필드를 찾을 수 없습니다.");
        }
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetText(TMP_Text text, string value)
    {
        Undo.RecordObject(text, "Localize Growth Management UI");
        text.text = value;
        EditorUtility.SetDirty(text);
    }

    private static TMP_FontAsset FindFont()
    {
        foreach (string guid in AssetDatabase.FindAssets($"{FontName} t:TMP_FontAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null && font.name == FontName)
            {
                return font;
            }
        }
        return null;
    }

    private static string GetElementName(ItemElement element) => element switch
    {
        ItemElement.Electric => "전기",
        ItemElement.Sword => "검",
        ItemElement.Ice => "얼음",
        _ => "알 수 없음"
    };

    private static void Stop(string message)
    {
        Debug.LogWarning("GrowthManagementUIGenerator: " + message);
        EditorUtility.DisplayDialog("성장 관리 UI 이관 중단", message, "확인");
    }

    private readonly struct RectSnapshot
    {
        private readonly Vector2 anchorMin;
        private readonly Vector2 anchorMax;
        private readonly Vector2 anchoredPosition;
        private readonly Vector2 sizeDelta;
        private readonly Vector2 pivot;
        private readonly Vector3 localScale;
        private readonly Quaternion localRotation;

        public RectSnapshot(RectTransform rect)
        {
            anchorMin = rect != null ? rect.anchorMin : Vector2.zero;
            anchorMax = rect != null ? rect.anchorMax : Vector2.one;
            anchoredPosition = rect != null ? rect.anchoredPosition : Vector2.zero;
            sizeDelta = rect != null ? rect.sizeDelta : Vector2.zero;
            pivot = rect != null ? rect.pivot : new Vector2(0.5f, 0.5f);
            localScale = rect != null ? rect.localScale : Vector3.one;
            localRotation = rect != null ? rect.localRotation : Quaternion.identity;
        }

        public void Restore(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }
            Undo.RecordObject(rect, "Preserve Skill Tree RectTransform");
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.pivot = pivot;
            rect.localScale = localScale;
            rect.localRotation = localRotation;
        }
    }
}
