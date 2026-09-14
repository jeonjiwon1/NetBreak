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

    [MenuItem("NETBREAK/검증/STEP 10B 획득 UI 열기")]
    private static void ShowAcquisitionUI()
    {
        if (!EditorApplication.isPlaying || ToolAcquisitionManager.Instance == null ||
            !ToolAcquisitionManager.Instance.RequestToolAcquisition())
        {
            Debug.LogWarning("새 Main Play Mode에서 도구 획득 UI를 열 수 있습니다.");
        }
    }

    [MenuItem("NETBREAK/검증/STEP 10B 도구 획득")]
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
                    Debug.LogError("STEP 10B 검증 실패: " + exception);
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
                Debug.Log($"STEP 10B 도구 획득 통과: {assertions}개 확인\n" + string.Join("\n", results));
            }
            EditorApplication.isPlaying = false;
        }
    }

    private static IEnumerator Checks()
    {
        RunManager run = RunManager.Instance;
        RunToolLoadout slots = run.ToolSlots;
        ToolAcquisitionManager acquisition = run.ToolAcquisition;
        PrototypeSelectionCanvas canvas = Object.FindFirstObjectByType<PrototypeSelectionCanvas>();
        PrototypeHUDCanvas hud = Object.FindFirstObjectByType<PrototypeHUDCanvas>();
        BaitController bait = Object.FindFirstObjectByType<BaitController>();
        NetPlacementController net = Object.FindFirstObjectByType<NetPlacementController>();
        CastNetController cast = Object.FindFirstObjectByType<CastNetController>();
        FishingRodPlacementController rod = Object.FindFirstObjectByType<FishingRodPlacementController>();
        FishSpawner spawner = Object.FindFirstObjectByType<FishSpawner>();
        Require(acquisition != null && canvas != null && hud != null && bait != null && net != null && cast != null &&
            rod != null && spawner != null,
            "Main 획득/UI/도구 참조");
        float earlyDuration = GetValueField<float>(spawner, "earlyPhaseDuration");
        float introDuration = GetValueField<float>(spawner, "landingNetIntroDuration");
        float secondToolPracticeDuration = GetValueField<float>(spawner, "postSecondToolPracticeDuration");
        Require(earlyDuration == 75f && introDuration == 15f && secondToolPracticeDuration == 15f &&
            earlyDuration - introDuration - secondToolPracticeDuration == 45f,
            "Coast 초반 페이싱: 뜰채 15초/첫 도구 45초/두 도구 15초");
        SetValueField(spawner, "earlyPhaseDuration", 0.9f);
        SetValueField(spawner, "landingNetIntroDuration", 0.2f);
        SetValueField(spawner, "postSecondToolPracticeDuration", 0.2f);
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

        PrototypeGameFlowManager.Instance.StartFishing();
        yield return null;
        Require(spawner.HasStarted && run.TotalCatchValue > 0 && !acquisition.IsAcquisitionPending,
            "조업 시작 즉시 물고기 스폰 및 첫 획득 미표시");

        yield return WaitForAcquisitionReady(acquisition, 2f);
        yield return null;
        GameObject panel = GetField<GameObject>(canvas, "augmentSelectionPanel");
        Button[] buttons =
        {
            GetField<Button>(canvas, "augmentButton1"),
            GetField<Button>(canvas, "augmentButton2"),
            GetField<Button>(canvas, "augmentButton3")
        };
        TMP_Text[] texts =
        {
            GetField<TMP_Text>(canvas, "augmentText1"),
            GetField<TMP_Text>(canvas, "augmentText2"),
            GetField<TMP_Text>(canvas, "augmentText3")
        };
        Require(panel.activeSelf && acquisition.IsChoosingTool && acquisition.CanSelect,
            "뜰채 전용 도입 뒤 첫 획득 선택 UI 표시");
        Require(Enumerable.Range(0, 3).All(i => acquisition.GetChoice(i) != ToolId.None &&
            buttons[i].interactable && !string.IsNullOrWhiteSpace(texts[i].text)), "유효한 한국어 도구 선택지 3개");
        Require(texts[0].text.Contains("미끼") && texts[1].text.Contains("그물") && texts[2].text.Contains("투망"),
            "첫 선택지 한국어 표시");

        buttons[0].onClick.Invoke();
        yield return null;
        Require(slots.OwnsTool(ToolId.Bait) && slots.GetSlot(0) == ToolId.Bait &&
            Enumerable.Range(1, 3).All(i => slots.GetSlot(i) == ToolId.None), "첫 획득은 Q 빈 슬롯에 배치");
        Require(hotbarSlotTexts[1].text.Contains("[Q]") && hotbarSlotTexts[1].text.Contains("미끼") &&
            hotbarSlotTexts[2].text.Contains("[W]") && hotbarSlotTexts[2].text.Contains("비어 있음"),
            "첫 획득 Hotbar Q 갱신");
        yield return Send(point, 0);
        Require(ToolSlotInput.GetBindingLabel(ToolId.Bait) == "Q",
            "획득한 미끼의 실제 입력 바인딩은 Q");

        yield return Send(point, 0, Key.Q);
        Require(bait.IsActive, "첫 도구 연습 구간에서 실제 Q 사용");
        yield return new WaitForSecondsRealtime(0.1f);
        Require(!acquisition.IsAcquisitionPending,
            "첫 획득 직후 두 번째 획득을 연속 표시하지 않음");

        run.GrantBonusReward(0, run.ExpToNextLevel);
        yield return null;
        Require(run.CurrentLevel == 2 && !PrototypeAugmentManager.Instance.IsShowingChoices,
            "액티브 도구 2개 전 일반 증강 보류");

        yield return WaitForAcquisitionReady(acquisition, 2f);
        yield return null;
        Require(acquisition.IsChoosingTool && acquisition.CanSelect,
            $"첫 도구 연습 구간 뒤 두 번째 획득 표시 " +
            $"(choosing={acquisition.IsChoosingTool}, pending={acquisition.IsAcquisitionPending}, " +
            $"canSelect={acquisition.CanSelect}, stage={spawner.CurrentStageIndex}, " +
            $"owned={slots.OwnedActiveToolCount}, timeScale={Time.timeScale})");
        Require(Enumerable.Range(0, 3).All(i => acquisition.GetChoice(i) != ToolId.Bait &&
            acquisition.GetChoice(i) != ToolId.None), "두 번째 선택에서 소유 도구 제외");
        ToolId secondTool = acquisition.GetChoice(0);
        buttons[0].onClick.Invoke();
        yield return null;
        Require(slots.OwnsTool(secondTool) && slots.GetSlot(1) == secondTool && slots.GetSlot(0) == ToolId.Bait,
            "두 번째 획득은 W 빈 슬롯에 배치");
        Require(hotbarSlotTexts[1].text.Contains("[Q]") && hotbarSlotTexts[1].text.Contains("미끼") &&
            hotbarSlotTexts[2].text.Contains("[W]") &&
            hotbarSlotTexts[2].text.Contains(GetToolName(secondTool)),
            "두 번째 획득 Hotbar W 갱신");
        Require(!PrototypeAugmentManager.Instance.IsShowingChoices && run.CurrentLevel == 2,
            "두 번째 획득 직후 증강 미표시 및 보류 레벨 보존");

        yield return Send(point, 0, Key.W);
        Require(NetPlacementController.IsNetModeActive,
            "두 번째 도구 연습 구간에서 실제 W 사용");
        yield return Send(point, 2);
        Require(!NetPlacementController.IsNetModeActive,
            "두 번째 도구 ESC/RMB 취소 유지");

        yield return new WaitForSecondsRealtime(0.1f);
        Require(!PrototypeAugmentManager.Instance.IsShowingChoices && Time.timeScale == 1f,
            "두 도구 실제 플레이 구간 및 정상 시간 재개");

        yield return WaitForAugmentChoices(PrototypeAugmentManager.Instance, 2f);
        yield return null;
        Require(PrototypeAugmentManager.Instance.IsShowingChoices,
            "두 도구 연습 구간 뒤 보류된 일반 증강 허용");
        Require(CurrentAugmentCategoriesAreOwned(PrototypeAugmentManager.Instance, slots),
            "미소유 도구 증강 후보 제외");

        yield return new WaitForSecondsRealtime(0.5f);
        buttons[0].onClick.Invoke();
        yield return null;
        Require(spawner.CurrentStageIndex == 2 && Time.timeScale == 1f,
            "첫 증강 뒤 첫 대어군 단계 진행 및 시간 정상화");

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

    private static bool CurrentAugmentCategoriesAreOwned(
        PrototypeAugmentManager manager,
        RunToolLoadout slots)
    {
        FieldInfo choicesField = typeof(PrototypeAugmentManager).GetField(
            "currentChoices", BindingFlags.Instance | BindingFlags.NonPublic);
        Array choices = choicesField?.GetValue(manager) as Array;
        if (choices == null)
        {
            return false;
        }

        foreach (object choice in choices)
        {
            if (choice == null)
            {
                continue;
            }

            FieldInfo categoryField = choice.GetType().GetField(
                "Category", BindingFlags.Instance | BindingFlags.Public);
            if (categoryField?.GetValue(choice) is not AugmentCategory category)
            {
                return false;
            }

            if (category == AugmentCategory.General ||
                category == AugmentCategory.LandingNet)
            {
                continue;
            }

            ToolId requiredTool = category switch
            {
                AugmentCategory.Bait => ToolId.Bait,
                AugmentCategory.Net => ToolId.Net,
                AugmentCategory.CastNet => ToolId.CastNet,
                AugmentCategory.FishingRod => ToolId.FishingRod,
                _ => ToolId.None
            };

            if (requiredTool == ToolId.None ||
                !slots.OwnsTool(requiredTool))
            {
                return false;
            }
        }

        return true;
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

    private static T GetField<T>(object target, string name) where T : class
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(target) as T;
    }

    private static T GetValueField<T>(object target, string name) where T : struct
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(target) is T value)
        {
            return value;
        }

        throw new InvalidOperationException($"필드를 읽을 수 없습니다: {name}");
    }

    private static void SetValueField<T>(object target, string name, T value) where T : struct
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new InvalidOperationException($"필드를 찾을 수 없습니다: {name}");
        }

        field.SetValue(target, value);
    }

    private static IEnumerator Send(Vector2 position, ushort buttons, params Key[] keys)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = buttons });
        yield return null;
    }

    private static IEnumerator WaitForAcquisitionReady(
        ToolAcquisitionManager acquisition,
        float timeout)
    {
        float deadline = Time.realtimeSinceStartup + timeout;
        while (!acquisition.IsChoosingTool && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!acquisition.IsChoosingTool)
        {
            yield break;
        }

        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(mouse, new MouseState());
        yield return null;

        while (!acquisition.CanSelect && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }
    }

    private static IEnumerator WaitForAugmentChoices(
        PrototypeAugmentManager augment,
        float timeout)
    {
        float deadline = Time.realtimeSinceStartup + timeout;
        while (!augment.IsShowingChoices && Time.realtimeSinceStartup < deadline)
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
