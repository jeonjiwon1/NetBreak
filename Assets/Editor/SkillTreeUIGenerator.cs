using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class SkillTreeUIGenerator
{
    private const string MenuPath = "NETBREAK/UI/Generate Skill Tree UI";
    private const string PolishMenuPath = "NETBREAK/UI/Apply Skill Tree UI Polish";
    private const string CanvasName = "GameCanvas";
    private const string RootName = "SkillTreeUI";
    private const string FontName = "NanumGothic-Bold SDF";

    private static readonly Color PanelColor = new(0.035f, 0.075f, 0.11f, 0.97f);
    private static readonly Color SurfaceColor = new(0.08f, 0.15f, 0.2f, 0.96f);
    private static readonly Color ButtonColor = new(0.12f, 0.32f, 0.42f, 1f);
    private static readonly Color AccentColor = new(0.2f, 0.62f, 0.72f, 1f);
    private static readonly Color TextColor = new(0.93f, 0.97f, 1f, 1f);
    private static readonly Color MutedTextColor = new(0.72f, 0.82f, 0.87f, 1f);

    [MenuItem(MenuPath)]
    private static void Generate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            StopWithWarning("Play Mode에서는 Skill Tree UI를 생성할 수 없습니다.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            StopWithWarning("활성 Scene을 찾을 수 없습니다.");
            return;
        }

        Canvas[] matchingCanvases = Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Where(canvas => canvas.gameObject.scene == scene && canvas.name == CanvasName)
            .ToArray();

        if (matchingCanvases.Length != 1)
        {
            StopWithWarning(
                matchingCanvases.Length == 0
                    ? $"활성 Scene에서 '{CanvasName}'를 찾을 수 없습니다."
                    : $"활성 Scene에 '{CanvasName}'가 {matchingCanvases.Length}개 있어 대상을 결정할 수 없습니다.");
            return;
        }

        Canvas gameCanvas = matchingCanvases[0];
        if (!ValidateExistingState(scene, gameCanvas))
        {
            return;
        }

        EventSystem eventSystem = Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.gameObject.scene == scene);
        if (eventSystem == null || eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            StopWithWarning(
                "활성 Scene에 InputSystemUIInputModule이 연결된 EventSystem이 없습니다. " +
                "기존 입력 구성을 임의로 바꾸지 않기 위해 생성을 중단했습니다.");
            return;
        }

        TMP_FontAsset font = FindFont();
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Generate NETBREAK Skill Tree UI");

        try
        {
            GeneratedUI ui = BuildHierarchy(gameCanvas, font);
            WireSerializedReferences(ui);
            UnityEventTools.AddPersistentListener(
                ui.OpenButton.onClick,
                ui.CanvasController.ToggleTreeFromUI);

            ui.TreePanel.SetActive(false);
            ui.AcquisitionPanel.SetActive(false);
            ui.TemplatesRoot.SetActive(false);
            ui.Root.SetActive(true);

            EditorUtility.SetDirty(ui.CanvasController);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = ui.Root;

            string fontMessage = font != null
                ? $"폰트: {font.name} ({font.atlasPopulationMode})"
                : "NanumGothic-Bold SDF를 찾지 못해 TMP 기본 폰트를 사용했습니다.";
            Debug.Log(
                $"SkillTreeUIGenerator: '{gameCanvas.name}' 아래에 완전한 SkillTreeUI를 생성하고 연결했습니다. " +
                fontMessage,
                ui.Root);
        }
        catch (Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogError("SkillTreeUIGenerator 생성 실패:\n" + exception);
            EditorUtility.DisplayDialog(
                "Skill Tree UI 생성 실패",
                "생성 중 오류가 발생해 Undo로 되돌렸습니다. Console을 확인하세요.",
                "확인");
        }
    }

    [MenuItem(PolishMenuPath)]
    private static void ApplyPolishToExistingUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            StopWithWarning("Play Mode에서는 기존 Skill Tree UI를 수정할 수 없습니다.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Where(canvas => canvas.gameObject.scene == scene && canvas.name == CanvasName)
            .ToArray();
        if (canvases.Length != 1)
        {
            StopWithWarning($"활성 Scene에서 유일한 '{CanvasName}'를 찾을 수 없습니다.");
            return;
        }

        Transform[] roots = canvases[0].GetComponentsInChildren<Transform>(true)
            .Where(transform => transform.name == RootName)
            .ToArray();
        if (roots.Length != 1 || roots[0].parent != canvases[0].transform)
        {
            StopWithWarning($"'{CanvasName}/{RootName}' 구조를 유일하게 찾을 수 없습니다.");
            return;
        }

        SkillTreeCanvas canvasController = roots[0].GetComponent<SkillTreeCanvas>();
        if (canvasController == null)
        {
            StopWithWarning("기존 SkillTreeUI에 SkillTreeCanvas가 없어 안전하게 갱신할 수 없습니다.");
            return;
        }

        MasteryPointReminder[] reminderControllers =
            roots[0].GetComponents<MasteryPointReminder>();
        Transform[] reminderRoots = roots[0].Cast<Transform>()
            .Where(child => child.name == "MasteryPointReminder")
            .ToArray();
        bool hasNoReminder = reminderControllers.Length == 0 && reminderRoots.Length == 0;
        bool hasCompleteReminder = reminderControllers.Length == 1 && reminderRoots.Length == 1;
        if (!hasNoReminder && !hasCompleteReminder)
        {
            StopWithWarning(
                "MasteryPointReminder의 부분 구조 또는 중복 컴포넌트가 있어 사용자 편집을 보호하기 위해 중단했습니다.");
            return;
        }

        TMP_Text existingPointText = null;
        TMP_Text existingHintText = null;
        if (hasCompleteReminder)
        {
            TMP_Text[] reminderTexts = reminderRoots[0].GetComponentsInChildren<TMP_Text>(true);
            existingPointText = reminderTexts.SingleOrDefault(text => text.name == "PointText");
            existingHintText = reminderTexts.SingleOrDefault(text => text.name == "HintText");
            if (existingPointText == null || existingHintText == null)
            {
                StopWithWarning(
                    "기존 MasteryPointReminder의 PointText/HintText 구조가 모호해 자동 연결하지 않았습니다.");
                return;
            }
        }

        SkillTreeBranchView coreBranch = GetObjectReference<SkillTreeBranchView>(
            canvasController, "coreBranch");
        SkillTreeBranchView partnerBranch = GetObjectReference<SkillTreeBranchView>(
            canvasController, "partnerBranch");
        TMP_Text acquisitionTitle = GetObjectReference<TMP_Text>(
            canvasController, "acquisitionTitleText");
        if (coreBranch == null || partnerBranch == null || acquisitionTitle == null)
        {
            StopWithWarning("기존 SkillTreeCanvas의 필수 Branch/Acquisition 참조가 비어 있습니다.");
            return;
        }

        SkillTreeNodeView[] templates = new[]
            {
                GetObjectReference<SkillTreeNodeView>(coreBranch, "nodePrefab"),
                GetObjectReference<SkillTreeNodeView>(partnerBranch, "nodePrefab")
            }
            .Where(template => template != null)
            .Distinct()
            .ToArray();
        if (templates.Length == 0 || templates.Any(EditorUtility.IsPersistent))
        {
            StopWithWarning(
                "Scene 내부 Skill Tree Node Template을 찾을 수 없거나 Project Asset을 참조하고 있어 자동 수정하지 않았습니다.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Apply NETBREAK Skill Tree UI Polish");
        try
        {
            TMP_FontAsset font = FindFont();
            if (hasNoReminder)
            {
                CreateMasteryReminder(roots[0], font);
            }
            else
            {
                SetObjectReference(reminderControllers[0], "reminderRoot", reminderRoots[0].gameObject);
                SetObjectReference(reminderControllers[0], "pointText", existingPointText);
                SetObjectReference(reminderControllers[0], "hintText", existingHintText);
            }

            ApplyBranchLocalization(coreBranch, "주력 스킬 트리", "Lv2에서 해금");
            ApplyBranchLocalization(partnerBranch, "보조 스킬 트리", "Lv3에서 해금");
            SetText(acquisitionTitle, "주력 도구 선택");
            foreach (SkillTreeNodeView template in templates)
            {
                ApplyNodeTemplateReadability(template);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = roots[0].gameObject;
            Debug.Log(
                "SkillTreeUIGenerator: 기존 SkillTreeUI에 숙련 포인트 알림, 한국어 표기, Node 가독성 설정을 적용했습니다.",
                roots[0].gameObject);
        }
        catch (Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogError("SkillTreeUIGenerator 폴리시 적용 실패:\n" + exception);
            EditorUtility.DisplayDialog(
                "Skill Tree UI 폴리시 실패",
                "변경을 Undo로 되돌렸습니다. Console을 확인하세요.",
                "확인");
        }
    }

    private static bool ValidateExistingState(Scene scene, Canvas gameCanvas)
    {
        SkillTreeCanvas[] controllers = Object.FindObjectsByType<SkillTreeCanvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Where(controller => controller.gameObject.scene == scene)
            .ToArray();
        Transform[] namedRoots = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(transform => transform.name == RootName)
            .ToArray();

        if (controllers.Length == 0 && namedRoots.Length == 0)
        {
            return true;
        }

        if (controllers.Length == 1 && namedRoots.Length == 1 &&
            controllers[0].transform == namedRoots[0] &&
            namedRoots[0].parent == gameCanvas.transform)
        {
            Selection.activeGameObject = namedRoots[0].gameObject;
            EditorUtility.DisplayDialog(
                "Skill Tree UI가 이미 존재합니다",
                $"'{CanvasName}/{RootName}'에 SkillTreeCanvas가 이미 연결되어 있습니다. " +
                "사용자 UI를 덮어쓰지 않았습니다.",
                "확인");
            return false;
        }

        StopWithWarning(
            "SkillTreeUI 또는 SkillTreeCanvas의 부분 구조가 이미 존재하지만 안전하게 재사용할 수 없습니다. " +
            "기존 사용자 UI를 보호하기 위해 생성을 중단했습니다. Hierarchy에서 해당 구조를 확인하세요.");
        return false;
    }

    private static GeneratedUI BuildHierarchy(Canvas gameCanvas, TMP_FontAsset font)
    {
        GameObject root = CreateRect(
            RootName,
            gameCanvas.transform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        Undo.RegisterCreatedObjectUndo(root, "Generate NETBREAK Skill Tree UI");
        SkillTreeCanvas canvasController = root.AddComponent<SkillTreeCanvas>();
        CreateMasteryReminder(root.transform, font);

        Button openButton = CreateButton(
            "OpenTreeButton",
            root.transform,
            "스킬 트리 [Tab]",
            font,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-246f, 28f),
            new Vector2(-28f, 92f),
            ButtonColor,
            24f).Button;

        GameObject treePanel = CreateRect(
            "TreePanel",
            root.transform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        Image blocker = treePanel.AddComponent<Image>();
        blocker.color = PanelColor;
        blocker.raycastTarget = true;

        GameObject header = CreateRect(
            "Header",
            treePanel.transform,
            new Vector2(0.025f, 0.88f),
            new Vector2(0.975f, 0.98f),
            Vector2.zero,
            Vector2.zero);
        Image headerImage = header.AddComponent<Image>();
        headerImage.color = SurfaceColor;
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding = new RectOffset(24, 18, 10, 10);
        headerLayout.spacing = 18f;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;

        TextMeshProUGUI title = CreateLayoutText(
            "TitleText", header.transform, "숙련 스킬 트리", font, 32f,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 0f);
        title.GetComponent<LayoutElement>().preferredWidth = 260f;
        TextMeshProUGUI mastery = CreateLayoutText(
            "MasteryPointText", header.transform, "숙련 포인트: 0", font, 26f,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 0f);
        mastery.GetComponent<LayoutElement>().preferredWidth = 240f;
        TextMeshProUGUI notice = CreateLayoutText(
            "NoticeText", header.transform, "", font, 22f,
            FontStyles.Normal, TextAlignmentOptions.MidlineLeft, 0f, true);
        Button closeButton = CreateLayoutButton(
            "CloseButton", header.transform, "닫기", font, 120f, ButtonColor).Button;

        GameObject instruction = CreateRect(
            "NavigationHint",
            treePanel.transform,
            new Vector2(0.2f, 0.825f),
            new Vector2(0.8f, 0.875f),
            Vector2.zero,
            Vector2.zero);
        TextMeshProUGUI instructionText = instruction.AddComponent<TextMeshProUGUI>();
        ConfigureText(
            instructionText,
            "드래그: 이동  ·  마우스 휠: 확대/축소",
            font,
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            MutedTextColor);

        GameObject templatesRoot = CreateRect(
            "Templates",
            root.transform,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero);
        SkillTreeNodeView nodeTemplate = CreateNodeTemplate(templatesRoot.transform, font);
        ApplyNodeTemplateReadability(nodeTemplate);

        BranchObjects core = CreateBranch(
            "Core",
            treePanel.transform,
            font,
            nodeTemplate,
            gameCanvas,
            new Vector2(0.025f, 0.08f),
            new Vector2(0.495f, 0.815f));
        BranchObjects partner = CreateBranch(
            "Partner",
            treePanel.transform,
            font,
            nodeTemplate,
            gameCanvas,
            new Vector2(0.505f, 0.08f),
            new Vector2(0.975f, 0.815f));

        AcquisitionObjects acquisition = CreateAcquisitionPanel(treePanel.transform, font);

        return new GeneratedUI
        {
            Root = root,
            CanvasController = canvasController,
            TreePanel = treePanel,
            MasteryPointText = mastery,
            NoticeText = notice,
            CloseButton = closeButton,
            Core = core,
            Partner = partner,
            AcquisitionPanel = acquisition.Panel,
            AcquisitionTitle = acquisition.Title,
            AcquisitionButtons = acquisition.Buttons,
            AcquisitionLabels = acquisition.Labels,
            OpenButton = openButton,
            NodeTemplate = nodeTemplate,
            TemplatesRoot = templatesRoot
        };
    }

    private static BranchObjects CreateBranch(
        string prefix,
        Transform parent,
        TMP_FontAsset font,
        SkillTreeNodeView nodeTemplate,
        Canvas canvas,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject viewport = CreateRect(
            prefix + "Viewport",
            parent,
            anchorMin,
            anchorMax,
            new Vector2(8f, 8f),
            new Vector2(-8f, -8f));
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = SurfaceColor;
        viewportImage.raycastTarget = true;
        viewport.AddComponent<RectMask2D>();
        SkillTreeBranchView branch = viewport.AddComponent<SkillTreeBranchView>();
        SkillTreePanZoom panZoom = viewport.AddComponent<SkillTreePanZoom>();

        GameObject content = CreateRect(
            prefix + "Content",
            viewport.transform,
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(18f, -18f),
            new Vector2(-18f, -18f));
        RectTransform contentRect = (RectTransform)content.transform;
        contentRect.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(8, 8, 8, 18);
        contentLayout.spacing = 12f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI branchTitle = CreateLayoutText(
            prefix + "TitleText",
            content.transform,
            prefix == "Core" ? "주력 스킬 트리" : "보조 스킬 트리",
            font,
            30f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            54f);

        ButtonObjects rootButton = CreateLayoutButton(
            prefix + "RootButton",
            content.transform,
            "",
            font,
            0f,
            AccentColor);
        rootButton.Button.GetComponent<LayoutElement>().preferredHeight = 104f;
        VerticalLayoutGroup rootLayout = rootButton.Button.gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(12, 12, 8, 8);
        rootLayout.spacing = 2f;
        rootLayout.childAlignment = TextAnchor.MiddleCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;
        Object.DestroyImmediate(rootButton.Label.gameObject);
        TextMeshProUGUI rootTitle = CreateLayoutText(
            "RootTitleText", rootButton.Button.transform, "?", font, 34f,
            FontStyles.Bold, TextAlignmentOptions.Center, 48f);
        TextMeshProUGUI rootStatus = CreateLayoutText(
            "RootStatusText", rootButton.Button.transform,
            prefix == "Core" ? "Lv2에서 해금" : "Lv3에서 해금",
            font, 18f, FontStyles.Normal, TextAlignmentOptions.Center, 30f);

        GameObject nodeContainer = CreateRect(
            prefix + "NodeContainer",
            content.transform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        VerticalLayoutGroup nodeLayout = nodeContainer.AddComponent<VerticalLayoutGroup>();
        nodeLayout.spacing = 12f;
        nodeLayout.childAlignment = TextAnchor.UpperCenter;
        nodeLayout.childControlWidth = true;
        nodeLayout.childControlHeight = true;
        nodeLayout.childForceExpandWidth = true;
        nodeLayout.childForceExpandHeight = false;
        ContentSizeFitter nodeFitter = nodeContainer.AddComponent<ContentSizeFitter>();
        nodeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        SetObjectReference(branch, "branchTitleText", branchTitle);
        SetObjectReference(branch, "rootButton", rootButton.Button);
        SetObjectReference(branch, "rootTitleText", rootTitle);
        SetObjectReference(branch, "rootStatusText", rootStatus);
        SetObjectReference(branch, "nodeContainer", (RectTransform)nodeContainer.transform);
        SetObjectReference(branch, "nodePrefab", nodeTemplate);
        SetObjectReference(panZoom, "content", contentRect);
        SetObjectReference(panZoom, "canvas", canvas);

        return new BranchObjects
        {
            View = branch,
            PanZoom = panZoom,
            Content = contentRect
        };
    }

    private static SkillTreeNodeView CreateNodeTemplate(Transform parent, TMP_FontAsset font)
    {
        GameObject node = CreateRect(
            "SkillTreeNodeTemplate",
            parent,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);
        Image background = node.AddComponent<Image>();
        background.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        Button button = node.AddComponent<Button>();
        button.targetGraphic = background;
        LayoutElement layoutElement = node.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 248f;
        layoutElement.minHeight = 220f;
        VerticalLayoutGroup layout = node.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 12, 12);
        layout.spacing = 5f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateLayoutText(
            "TitleText", node.transform, "노드 이름", font, 23f,
            FontStyles.Bold, TextAlignmentOptions.TopLeft, 32f);
        TextMeshProUGUI description = CreateLayoutText(
            "DescriptionText", node.transform, "노드 설명과 랭크별 효과", font, 16f,
            FontStyles.Normal, TextAlignmentOptions.TopLeft, 112f);

        GameObject summaryRow = CreateRect(
            "SummaryRow", node.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        LayoutElement summaryElement = summaryRow.AddComponent<LayoutElement>();
        summaryElement.preferredHeight = 28f;
        HorizontalLayoutGroup summaryLayout = summaryRow.AddComponent<HorizontalLayoutGroup>();
        summaryLayout.spacing = 12f;
        summaryLayout.childControlWidth = true;
        summaryLayout.childControlHeight = true;
        summaryLayout.childForceExpandWidth = true;
        summaryLayout.childForceExpandHeight = true;
        TextMeshProUGUI rank = CreateLayoutText(
            "RankText", summaryRow.transform, "랭크 0/0", font, 16f,
            FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 0f, true);
        TextMeshProUGUI cost = CreateLayoutText(
            "CostText", summaryRow.transform, "다음 비용: -", font, 16f,
            FontStyles.Bold, TextAlignmentOptions.MidlineRight, 0f, true);
        TextMeshProUGUI lockText = CreateLayoutText(
            "LockText", node.transform, "잠금 조건", font, 15f,
            FontStyles.Normal, TextAlignmentOptions.TopLeft, 28f);
        lockText.color = new Color(1f, 0.72f, 0.42f, 1f);

        SkillTreeNodeView view = node.AddComponent<SkillTreeNodeView>();
        SetObjectReference(view, "purchaseButton", button);
        SetObjectReference(view, "background", background);
        SetObjectReference(view, "titleText", title);
        SetObjectReference(view, "descriptionText", description);
        SetObjectReference(view, "rankText", rank);
        SetObjectReference(view, "costText", cost);
        SetObjectReference(view, "lockText", lockText);
        return view;
    }

    private static AcquisitionObjects CreateAcquisitionPanel(Transform parent, TMP_FontAsset font)
    {
        GameObject panel = CreateRect(
            "AcquisitionPanel",
            parent,
            new Vector2(0.14f, 0.19f),
            new Vector2(0.86f, 0.76f),
            Vector2.zero,
            Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.025f, 0.08f, 0.12f, 0.99f);
        panelImage.raycastTarget = true;
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 24, 28);
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateLayoutText(
            "AcquisitionTitleText", panel.transform, "주력 도구 선택", font, 32f,
            FontStyles.Bold, TextAlignmentOptions.Center, 62f);
        TextMeshProUGUI guidance = CreateLayoutText(
            "GuidanceText", panel.transform,
            "도구를 선택해야 조업을 계속할 수 있습니다.", font, 19f,
            FontStyles.Normal, TextAlignmentOptions.Center, 34f);
        guidance.color = MutedTextColor;

        GameObject choiceRow = CreateRect(
            "ChoiceRow", panel.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        LayoutElement rowElement = choiceRow.AddComponent<LayoutElement>();
        rowElement.preferredHeight = 250f;
        HorizontalLayoutGroup rowLayout = choiceRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 18f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;

        Button[] buttons = new Button[3];
        TMP_Text[] labels = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            ButtonObjects choice = CreateLayoutButton(
                $"ChoiceButton{i + 1}",
                choiceRow.transform,
                $"후보 {i + 1}",
                font,
                0f,
                ButtonColor,
                true);
            choice.Label.fontSize = 21f;
            choice.Label.alignment = TextAlignmentOptions.Center;
            buttons[i] = choice.Button;
            labels[i] = choice.Label;
        }

        return new AcquisitionObjects
        {
            Panel = panel,
            Title = title,
            Buttons = buttons,
            Labels = labels
        };
    }

    private static void CreateMasteryReminder(Transform parent, TMP_FontAsset font)
    {
        GameObject reminderRoot = CreateRect(
            "MasteryPointReminder",
            parent,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(-300f, 124f),
            new Vector2(300f, 216f));
        Undo.RegisterCreatedObjectUndo(reminderRoot, "Create Mastery Point Reminder");
        Image background = reminderRoot.AddComponent<Image>();
        background.color = new Color(0.035f, 0.1f, 0.15f, 0.9f);
        background.raycastTarget = false;
        CanvasGroup canvasGroup = reminderRoot.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.ignoreParentGroups = false;

        VerticalLayoutGroup layout = reminderRoot.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 10, 10);
        layout.spacing = 2f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI pointText = CreateLayoutText(
            "PointText",
            reminderRoot.transform,
            "숙련 포인트 1개 사용 가능!",
            font,
            24f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            38f);
        TextMeshProUGUI hintText = CreateLayoutText(
            "HintText",
            reminderRoot.transform,
            "Tab — 스킬 트리 열기",
            font,
            17f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            28f);
        hintText.color = MutedTextColor;

        MasteryPointReminder reminder = parent.GetComponent<MasteryPointReminder>();
        if (reminder == null)
        {
            reminder = Undo.AddComponent<MasteryPointReminder>(parent.gameObject);
        }

        SetObjectReference(reminder, "reminderRoot", reminderRoot);
        SetObjectReference(reminder, "pointText", pointText);
        SetObjectReference(reminder, "hintText", hintText);
        reminderRoot.SetActive(false);
    }

    private static void ApplyBranchLocalization(
        SkillTreeBranchView branch,
        string title,
        string lockedStatus)
    {
        TMP_Text titleText = GetObjectReference<TMP_Text>(branch, "branchTitleText");
        TMP_Text statusText = GetObjectReference<TMP_Text>(branch, "rootStatusText");
        if (titleText == null || statusText == null)
        {
            throw new InvalidOperationException(
                $"{branch.name}의 제목 또는 Root 상태 TMP 참조가 비어 있습니다.");
        }

        SetText(titleText, title);
        SetText(statusText, lockedStatus);
    }

    private static void ApplyNodeTemplateReadability(SkillTreeNodeView template)
    {
        LayoutElement cardLayout = template.GetComponent<LayoutElement>();
        VerticalLayoutGroup cardGroup = template.GetComponent<VerticalLayoutGroup>();
        TMP_Text title = GetObjectReference<TMP_Text>(template, "titleText");
        TMP_Text description = GetObjectReference<TMP_Text>(template, "descriptionText");
        TMP_Text rank = GetObjectReference<TMP_Text>(template, "rankText");
        TMP_Text cost = GetObjectReference<TMP_Text>(template, "costText");
        TMP_Text lockText = GetObjectReference<TMP_Text>(template, "lockText");
        if (cardLayout == null || cardGroup == null || title == null || description == null ||
            rank == null || cost == null || lockText == null)
        {
            throw new InvalidOperationException(
                $"{template.name}이 생성기 표준 Node Template 구조와 다릅니다.");
        }

        RecordAndSet(cardLayout, () =>
        {
            cardLayout.minHeight = 252f;
            cardLayout.preferredHeight = 272f;
        });
        RecordAndSet(cardGroup, () =>
        {
            cardGroup.padding = new RectOffset(18, 18, 14, 14);
            cardGroup.spacing = 8f;
        });
        ApplyTextLayout(title, 25f, 38f, FontStyles.Bold, TextAlignmentOptions.TopLeft, 0f);
        ApplyTextLayout(description, 17f, 112f, FontStyles.Normal, TextAlignmentOptions.TopLeft, 7f);
        ApplyTextLayout(rank, 17f, 0f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, 0f);
        ApplyTextLayout(cost, 17f, 0f, FontStyles.Bold, TextAlignmentOptions.MidlineRight, 0f);
        ApplyTextLayout(lockText, 16f, 32f, FontStyles.Normal, TextAlignmentOptions.TopLeft, 3f);
    }

    private static void ApplyTextLayout(
        TMP_Text text,
        float fontSize,
        float preferredHeight,
        FontStyles style,
        TextAlignmentOptions alignment,
        float lineSpacing)
    {
        RecordAndSet(text, () =>
        {
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.lineSpacing = lineSpacing;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
        });

        LayoutElement element = text.GetComponent<LayoutElement>();
        if (element != null && preferredHeight > 0f)
        {
            RecordAndSet(element, () => element.preferredHeight = preferredHeight);
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        RecordAndSet(text, () => text.text = value);
    }

    private static void RecordAndSet(Object target, Action change)
    {
        Undo.RecordObject(target, "Apply NETBREAK Skill Tree UI Polish");
        change();
        EditorUtility.SetDirty(target);
    }

    private static T GetObjectReference<T>(Object target, string propertyName)
        where T : Object
    {
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        return property?.objectReferenceValue as T;
    }

    private static void WireSerializedReferences(GeneratedUI ui)
    {
        SerializedObject serialized = new(ui.CanvasController);
        serialized.FindProperty("treePanel").objectReferenceValue = ui.TreePanel;
        serialized.FindProperty("masteryPointText").objectReferenceValue = ui.MasteryPointText;
        serialized.FindProperty("noticeText").objectReferenceValue = ui.NoticeText;
        serialized.FindProperty("closeButton").objectReferenceValue = ui.CloseButton;
        serialized.FindProperty("coreBranch").objectReferenceValue = ui.Core.View;
        serialized.FindProperty("partnerBranch").objectReferenceValue = ui.Partner.View;
        serialized.FindProperty("acquisitionPanel").objectReferenceValue = ui.AcquisitionPanel;
        serialized.FindProperty("acquisitionTitleText").objectReferenceValue = ui.AcquisitionTitle;

        SerializedProperty choices = serialized.FindProperty("acquisitionChoices");
        choices.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            SerializedProperty choice = choices.GetArrayElementAtIndex(i);
            choice.FindPropertyRelative("button").objectReferenceValue = ui.AcquisitionButtons[i];
            choice.FindPropertyRelative("label").objectReferenceValue = ui.AcquisitionLabels[i];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
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

    private static TextMeshProUGUI CreateLayoutText(
        string name,
        Transform parent,
        string value,
        TMP_FontAsset font,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        float preferredHeight,
        bool flexibleWidth = false)
    {
        GameObject gameObject = CreateRect(
            name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI text = gameObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(text, value, font, fontSize, style, alignment, TextColor);
        LayoutElement element = gameObject.AddComponent<LayoutElement>();
        if (preferredHeight > 0f)
        {
            element.preferredHeight = preferredHeight;
        }

        element.flexibleWidth = flexibleWidth ? 1f : 0f;
        return text;
    }

    private static ButtonObjects CreateButton(
        string name,
        Transform parent,
        string label,
        TMP_FontAsset font,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color,
        float fontSize)
    {
        GameObject gameObject = CreateRect(
            name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        return ConfigureButton(gameObject, label, font, color, fontSize);
    }

    private static ButtonObjects CreateLayoutButton(
        string name,
        Transform parent,
        string label,
        TMP_FontAsset font,
        float preferredWidth,
        Color color,
        bool flexibleWidth = false)
    {
        GameObject gameObject = CreateRect(
            name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        LayoutElement element = gameObject.AddComponent<LayoutElement>();
        if (preferredWidth > 0f)
        {
            element.preferredWidth = preferredWidth;
        }

        element.flexibleWidth = flexibleWidth ? 1f : 0f;
        return ConfigureButton(gameObject, label, font, color, 22f);
    }

    private static ButtonObjects ConfigureButton(
        GameObject gameObject,
        string label,
        TMP_FontAsset font,
        Color color,
        float fontSize)
    {
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        Button button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        colors.disabledColor = new Color(0.2f, 0.24f, 0.26f, 0.7f);
        button.colors = colors;

        GameObject labelObject = CreateRect(
            "Label", gameObject.transform, Vector2.zero, Vector2.one,
            new Vector2(10f, 6f), new Vector2(-10f, -6f));
        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(
            text,
            label,
            font,
            fontSize,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            TextColor);
        return new ButtonObjects { Button = button, Label = text };
    }

    private static void ConfigureText(
        TextMeshProUGUI text,
        string value,
        TMP_FontAsset font,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color)
    {
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        if (font != null)
        {
            text.font = font;
        }
    }

    private static void SetObjectReference(Object target, string propertyName, Object value)
    {
        Undo.RecordObject(target, "Wire NETBREAK Skill Tree UI");
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            throw new InvalidOperationException(
                $"{target.GetType().Name}.{propertyName} 직렬화 필드를 찾을 수 없습니다.");
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
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

    private static void StopWithWarning(string message)
    {
        Debug.LogWarning("SkillTreeUIGenerator: " + message);
        EditorUtility.DisplayDialog("Skill Tree UI 생성 중단", message, "확인");
    }

    private sealed class GeneratedUI
    {
        public GameObject Root;
        public SkillTreeCanvas CanvasController;
        public GameObject TreePanel;
        public TMP_Text MasteryPointText;
        public TMP_Text NoticeText;
        public Button CloseButton;
        public BranchObjects Core;
        public BranchObjects Partner;
        public GameObject AcquisitionPanel;
        public TMP_Text AcquisitionTitle;
        public Button[] AcquisitionButtons;
        public TMP_Text[] AcquisitionLabels;
        public Button OpenButton;
        public SkillTreeNodeView NodeTemplate;
        public GameObject TemplatesRoot;
    }

    private sealed class BranchObjects
    {
        public SkillTreeBranchView View;
        public SkillTreePanZoom PanZoom;
        public RectTransform Content;
    }

    private sealed class AcquisitionObjects
    {
        public GameObject Panel;
        public TMP_Text Title;
        public Button[] Buttons;
        public TMP_Text[] Labels;
    }

    private sealed class ButtonObjects
    {
        public Button Button;
        public TextMeshProUGUI Label;
    }
}
