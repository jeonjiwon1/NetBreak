using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class ToolAcquisitionValidation
{
    private static Keyboard keyboard;
    private static Mouse mouse;
    private static Keyboard previousKeyboard;
    private static Mouse previousMouse;
    private static bool running;
    private static int assertions;
    private static readonly List<string> results = new();

    [MenuItem("NETBREAK/검증/G2+G3 Skill Tree 열기")]
    private static void ShowAcquisitionUI()
    {
        if (!EditorApplication.isPlaying || SkillTreeManager.Instance == null)
        {
            Debug.LogWarning("새 Main Play Mode에서 Skill Tree를 열 수 있습니다.");
            return;
        }

        SkillTreeManager.Instance.OpenForBrowsing();
    }

    [MenuItem("NETBREAK/검증/G2+G3 Core Partner 획득")]
    private static void StartValidation()
    {
        if (running || !EditorApplication.isPlaying || RunManager.Instance == null ||
            PrototypeGameFlowManager.Instance == null || !PrototypeGameFlowManager.Instance.IsPreparation)
        {
            Debug.LogWarning("새 Main Play Mode의 조업 준비 상태에서 검증을 실행하세요.");
            return;
        }

        running = true;
        assertions = 0;
        results.Clear();
        previousKeyboard = Keyboard.current;
        previousMouse = Mouse.current;
        keyboard = InputSystem.AddDevice<Keyboard>("STEP10BKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("STEP10BMouse");
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        RunManager.Instance.StartCoroutine(Run());
    }

    private static IEnumerator Run()
    {
        IEnumerator checks = Checks();
        bool passed = false;
        try
        {
            while (true)
            {
                object step;
                try
                {
                    if (!checks.MoveNext())
                    {
                        passed = true;
                        break;
                    }
                    step = checks.Current;
                }
                catch (Exception exception)
                {
                    Debug.LogError("G2+G3 획득 검증 실패: " + exception);
                    break;
                }
                yield return step;
            }
        }
        finally
        {
            (checks as IDisposable)?.Dispose();
            Cleanup();
            if (passed)
            {
                Debug.Log($"G2+G3 Core/Partner 획득 통과: {assertions}개 확인\n" + string.Join("\n", results));
            }
            EditorApplication.isPlaying = false;
        }
    }

    private static IEnumerator Checks()
    {
        RunManager run = RunManager.Instance;
        RunToolLoadout slots = run.ToolSlots;
        RunGrowthState growth = run.GrowthState;
        SkillTreeManager tree = run.SkillTree;
        SkillTreeCanvas canvas = Object.FindFirstObjectByType<SkillTreeCanvas>();
        PrototypeHUDCanvas hud = Object.FindFirstObjectByType<PrototypeHUDCanvas>();
        BaitController bait = Object.FindFirstObjectByType<BaitController>();
        NetPlacementController net = Object.FindFirstObjectByType<NetPlacementController>();
        CastNetController cast = Object.FindFirstObjectByType<CastNetController>();
        FishingRodPlacementController rod = Object.FindFirstObjectByType<FishingRodPlacementController>();
        FishSpawner spawner = Object.FindFirstObjectByType<FishSpawner>();
        Require(growth != null && tree != null && canvas != null && hud != null && bait != null && net != null && cast != null &&
            rod != null && spawner != null,
            "Main 성장/UI/도구 참조");
        TMP_Text[] hotbarSlotTexts = GetField<TMP_Text[]>(hud, "hotbarSlotTexts");
        RectTransform hotbarRoot = GetField<RectTransform>(hud, "hotbarRoot");
        Require(slots.OwnsTool(ToolId.LandingNet) &&
            !slots.OwnsTool(ToolId.Bait) && !slots.OwnsTool(ToolId.Net) &&
            !slots.OwnsTool(ToolId.CastNet) && !slots.OwnsTool(ToolId.FishingRod), "새 Run은 뜰채만 소유");
        Require(Enumerable.Range(0, RunToolLoadout.SlotCount).All(i => slots.GetSlot(i) == ToolId.None),
            "새 Run Q/W/E/R 빈 슬롯");
        Require(hotbarRoot != null && hotbarRoot.anchorMin == new Vector2(0.5f, 0f) &&
            hotbarRoot.anchorMax == new Vector2(0.5f, 0f) && hotbarRoot.anchoredPosition.y > 0f,
            "Hotbar 하단 중앙 독립 배치");
        Require(hotbarSlotTexts != null && hotbarSlotTexts.Length == 5 &&
            hotbarSlotTexts[0].text.Contains("[LMB]") && hotbarSlotTexts[0].text.Contains("뜰채") &&
            hotbarSlotTexts[0].text.Contains("고정 도구") &&
            Enumerable.Range(1, 4).All(i => hotbarSlotTexts[i].text.Contains("비어 있음")),
            "Hotbar 시작 상태: LMB 뜰채와 빈 Q/W/E/R");
        Require(hotbarSlotTexts.All(text => !text.raycastTarget) &&
            hotbarRoot.GetComponentsInChildren<Image>().All(image => !image.raycastTarget),
            "Hotbar 포인터 비차단 및 고정 투망 안내 제거");
        Require(!slots.TryAssignSlot(0, ToolId.Bait), "미소유 도구 직접 슬롯 배치 차단");

        Vector2 point = Camera.main.WorldToScreenPoint(new Vector3(-3, -2, 0));
        yield return Send(point, 0, Key.Q, Key.W, Key.E, Key.R);
        Require(!bait.IsActive && !NetPlacementController.IsNetModeActive &&
            !FishingRodPlacementController.IsRodModeActive && !cast.IsAiming, "미소유 4도구 입력 차단");

        run.GrantBonusReward(0, run.ExpToNextLevel);
        yield return null;
        yield return WaitForTreeReady(tree, 2f);
        Require(run.CurrentLevel == 2 && growth.AvailableMasteryPoints == 1 &&
            tree.IsOpen && tree.IsMandatoryAcquisition &&
            tree.MandatoryRole == GrowthToolRole.Core && tree.CanInteract,
            "Lv2 Core 필수 획득과 숙련 포인트 1 지급");
        ToolId[] firstChoices = Enumerable.Range(0, tree.AcquisitionChoiceCount)
            .Select(tree.GetAcquisitionChoice).ToArray();
        Require(firstChoices.Length == 3 &&
            firstChoices.All(tool => tool == ToolId.FishingRod || tool == ToolId.Net || tool == ToolId.CastNet) &&
            firstChoices.Distinct().Count() == firstChoices.Length &&
            firstChoices.All(tool => !slots.OwnsTool(tool)),
            "Core 후보는 낚싯대/그물/투망 3개를 중복 없이 표시");

        ToolId firstTool = firstChoices[0];
        Require(tree.SelectAcquisitionChoice(0), "Core 선택 확정");
        yield return null;
        Require(growth.SelectedCoreTool == firstTool && growth.SelectedPartnerTool == ToolId.None &&
            growth.AvailableMasteryPoints == 0 && slots.OwnsTool(firstTool) &&
            slots.GetSlot(0) == firstTool && Enumerable.Range(1, 3).All(i => slots.GetSlot(i) == ToolId.None),
            "Core 확정은 1P를 사용하고 Q에 배치");
        Require(hotbarSlotTexts[1].text.Contains("[Q]") && hotbarSlotTexts[1].text.Contains(GetToolName(firstTool)) &&
            hotbarSlotTexts[2].text.Contains("[W]") && hotbarSlotTexts[2].text.Contains("비어 있음"),
            "Core 획득 Hotbar Q 갱신");
        Require(tree.TryClose(), "Core 획득 후 Tree 닫기 허용");
        yield return Send(point, 0);
        Require(ToolSlotInput.GetBindingLabel(firstTool) == "Q",
            "Core 실제 입력 바인딩은 Q");

        PrototypeGameFlowManager.Instance.StartFishing();
        yield return null;
        Require(spawner.HasStarted && run.TotalCatchValue > 0,
            "조업 시작 즉시 물고기 스폰");
        yield return Send(point, 0, Key.Q);
        Require(IsToolActive(firstTool, bait, net, cast, rod), "Core 도구 실제 Q 사용");
        yield return Send(point, 2);

        run.GrantBonusReward(0, run.ExpToNextLevel);
        yield return null;
        yield return WaitForTreeReady(tree, 2f);
        Require(run.CurrentLevel == 3 && growth.AvailableMasteryPoints == 1 &&
            tree.IsMandatoryAcquisition && tree.MandatoryRole == GrowthToolRole.Partner,
            "Lv3 Partner 필수 획득과 숙련 포인트 1 지급");
        ToolId[] secondChoices = Enumerable.Range(0, tree.AcquisitionChoiceCount)
            .Select(tree.GetAcquisitionChoice).ToArray();
        Require(secondChoices.Length == 3 && secondChoices.All(tool => tool != ToolId.None && tool != firstTool) &&
            secondChoices.Distinct().Count() == secondChoices.Length,
            "Partner 후보는 Core를 제외한 3개를 중복 없이 표시");
        ToolId secondTool = secondChoices[0];
        Require(tree.SelectAcquisitionChoice(0), "Partner 선택 확정");
        yield return null;
        Require(growth.SelectedPartnerTool == secondTool && secondTool != firstTool &&
            growth.AvailableMasteryPoints == 0 && slots.OwnsTool(secondTool) &&
            slots.GetSlot(1) == secondTool && slots.GetSlot(0) == firstTool,
            "Partner 확정은 1P를 사용하고 W에 배치");
        Require(hotbarSlotTexts[1].text.Contains("[Q]") && hotbarSlotTexts[1].text.Contains(GetToolName(firstTool)) &&
            hotbarSlotTexts[2].text.Contains("[W]") &&
            hotbarSlotTexts[2].text.Contains(GetToolName(secondTool)),
            "Partner 획득 Hotbar W 갱신");
        Require(tree.TryClose(), "Partner 획득 후 Tree 닫기 허용");

        yield return Send(point, 0, Key.W);
        Require(IsToolActive(secondTool, bait, net, cast, rod),
            "Partner 도구 실제 W 사용");
        yield return Send(point, 2);
        Require(!IsToolActive(secondTool, bait, net, cast, rod),
            "Partner 도구 ESC/RMB 취소 유지");

        run.GrantBonusReward(0, run.ExpToNextLevel);
        yield return null;
        Require(run.CurrentLevel == 4 && growth.AvailableMasteryPoints == 1 &&
            !PrototypeAugmentManager.Instance.IsShowingChoices && !tree.IsMandatoryAcquisition,
            "Lv4는 숙련 포인트만 지급하고 Random Augment를 표시하지 않음");
        Require(Time.timeScale == 1f, "획득 종료 뒤 정상 시간 재개");

        FishData data = AssetDatabase.FindAssets("t:FishData")
            .Select(g => AssetDatabase.LoadAssetAtPath<FishData>(AssetDatabase.GUIDToAssetPath(g)))
            .OrderByDescending(d => d.MaxResistance).First();
        var target = new GameObject("STEP10B 검증 물고기");
        target.transform.position = new Vector3(-3, -2, 0);
        target.AddComponent<BoxCollider2D>();
        FishController fish = target.AddComponent<FishController>();
        fish.Initialize(data);
        Physics2D.SyncTransforms();
        float resistance = fish.CurrentResistance;
        yield return Send(point, 1);
        Require(fish.CurrentResistance < resistance, "LMB 뜰채 유지");
        yield return Send(point, 0);
        Object.Destroy(target);
        Require(!GearRepositionController.IsRepositioning, "도구 획득 후 재배치 입력 상태 정상");
    }

    private static string GetToolName(ToolId tool)
    {
        return tool switch
        {
            ToolId.Bait => "미끼",
            ToolId.Net => "그물",
            ToolId.CastNet => "투망",
            ToolId.FishingRod => "낚싯대",
            _ => ""
        };
    }

    private static bool IsToolActive(
        ToolId tool,
        BaitController bait,
        NetPlacementController net,
        CastNetController cast,
        FishingRodPlacementController rod)
    {
        return tool switch
        {
            ToolId.Bait => bait.IsActive,
            ToolId.Net => NetPlacementController.IsNetModeActive,
            ToolId.CastNet => cast.IsAiming,
            ToolId.FishingRod => FishingRodPlacementController.IsRodModeActive,
            _ => false
        };
    }

    private static T GetField<T>(object target, string name) where T : class
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(target) as T;
    }

    private static IEnumerator Send(Vector2 position, ushort buttons, params Key[] keys)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = buttons });
        yield return null;
    }

    private static IEnumerator WaitForTreeReady(
        SkillTreeManager tree,
        float timeout)
    {
        float deadline = Time.realtimeSinceStartup + timeout;
        while ((!tree.IsOpen || !tree.IsMandatoryAcquisition) &&
            Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!tree.IsOpen || !tree.IsMandatoryAcquisition)
        {
            yield break;
        }

        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(mouse, new MouseState());
        yield return null;

        while (!tree.CanInteract && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }
    }

    private static void Require(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        assertions++;
        results.Add("통과: " + name);
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode) Cleanup();
    }

    private static void Cleanup()
    {
        if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
        if (previousKeyboard != null && previousKeyboard.added) previousKeyboard.MakeCurrent();
        if (previousMouse != null && previousMouse.added) previousMouse.MakeCurrent();
        keyboard = null;
        mouse = null;
        running = false;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }
}
