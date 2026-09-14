using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

// Run on a fresh Main Play session. Synthetic devices go through the normal
// Input System update and real controller Updates, not private action methods.
public static class ToolSlotFoundationValidation
{
    private static Keyboard keyboard;
    private static Mouse mouse;
    private static Keyboard previousKeyboard;
    private static Mouse previousMouse;
    private static bool running;
    private static int assertions;
    private static readonly List<string> results = new();

    [MenuItem("NETBREAK/검증/STEP 10A 입력 회귀")]
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
        keyboard = InputSystem.AddDevice<Keyboard>("STEP10AKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("STEP10AMouse");
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
                    Debug.LogError("STEP 10A 검증 실패: " + exception);
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
                Debug.Log($"STEP 10A 입력 회귀 통과: {assertions}개 확인\n" + string.Join("\n", results));
            }
            // Test-only runtime objects and input state disappear on leaving Play.
            EditorApplication.isPlaying = false;
        }
    }

    private static IEnumerator Checks()
    {
        RunManager run = RunManager.Instance;
        RunToolLoadout slots = run.ToolSlots;
        BaitController bait = Object.FindFirstObjectByType<BaitController>();
        NetPlacementController net = Object.FindFirstObjectByType<NetPlacementController>();
        CastNetController cast = Object.FindFirstObjectByType<CastNetController>();
        FishingRodPlacementController rod = Object.FindFirstObjectByType<FishingRodPlacementController>();
        FishSpawner spawner = Object.FindFirstObjectByType<FishSpawner>();
        Require(bait != null && net != null && cast != null && rod != null && spawner != null, "Main 도구 참조");
        var empty = new RunToolLoadout();
        Require(empty.OwnsTool(ToolId.LandingNet) &&
            Enumerable.Range(0, 4).All(i => empty.GetSlot(i) == ToolId.None), "기본 데이터는 뜰채 소유/빈 슬롯 4개");
        Require(!empty.TryAssignSlot(0, ToolId.Bait) &&
            !empty.TryAssignSlot(0, ToolId.LandingNet) && !empty.TryAssignSlot(-1, ToolId.Bait) &&
            !empty.TryAssignSlot(4, ToolId.Net), "LMB 고정 및 범위 검증");
        Require(slots.TryAcquireTool(ToolId.Bait) && slots.TryAcquireTool(ToolId.Net) &&
            slots.TryAcquireTool(ToolId.CastNet) && slots.TryAcquireTool(ToolId.FishingRod),
            "회귀 검증용 4도구 획득");
        Require(slots.GetSlot(0) == ToolId.Bait && slots.GetSlot(1) == ToolId.Net &&
            slots.GetSlot(2) == ToolId.CastNet && slots.GetSlot(3) == ToolId.FishingRod, "획득 순서 슬롯 배치");

        Vector2 a = Camera.main.WorldToScreenPoint(new Vector3(-3, -2, 0));
        Vector2 b = Camera.main.WorldToScreenPoint(new Vector3(-1, -2, 0));
        yield return Send(a, 0);
        yield return Send(a, 0, Key.Q);
        Require(!bait.IsActive, "준비 중 미끼 차단");
        yield return Send(a, 0, Key.W);
        Require(NetPlacementController.IsNetModeActive, "W 슬롯 2: 준비 중 배치 모드");
        yield return Send(a, 2);
        Require(!NetPlacementController.IsNetModeActive, "RMB 그물 취소");
        yield return Send(a, 0, Key.R);
        Require(FishingRodPlacementController.IsRodModeActive,
            $"R 슬롯 4: 준비 중 배치 모드 (binding={ToolSlotInput.GetBindingLabel(ToolId.FishingRod)}, " +
            $"active={rod.ActiveRodCount}, max={rod.MaxActiveRods}, currentKeyboard={Keyboard.current?.name})");
        yield return Send(a, 0, Key.Escape);
        Require(!FishingRodPlacementController.IsRodModeActive, "ESC 낚싯대 취소");
        yield return Send(a, 0);

        PrototypeGameFlowManager.Instance.StartFishing();
        spawner.StopAllCoroutines(); // Isolate input from timed encounters in this test session only.
        yield return Send(a, 0, Key.Q);
        Require(bait.IsActive && Vector2.Distance(bait.Position, new Vector2(-3, -2)) < 0.05f, "Q 슬롯 1: 미끼 위치/활성");
        yield return Send(a, 0);

        // A real FishController with existing data gives damage-based evidence for LMB/E.
        FishData data = AssetDatabase.FindAssets("t:FishData")
            .Select(g => AssetDatabase.LoadAssetAtPath<FishData>(AssetDatabase.GUIDToAssetPath(g)))
            .OrderByDescending(d => d.MaxResistance).First();
        var target = new GameObject("STEP10A 검증 물고기");
        target.transform.position = new Vector3(-3, -2, 0);
        target.AddComponent<BoxCollider2D>();
        FishController fish = target.AddComponent<FishController>();
        fish.Initialize(data);
        Physics2D.SyncTransforms();
        float resistance = fish.CurrentResistance;
        yield return Send(a, 1);
        Require(fish.CurrentResistance < resistance,
            $"LMB 뜰채: Resistance 감소 (mouse={Mouse.current?.position.ReadValue()}, " +
            $"reserved={ToolSlotInput.IsWorldPointerReserved}, before={resistance}, after={fish.CurrentResistance})");
        yield return Send(a, 0);
        resistance = fish.CurrentResistance;
        int charges = cast.CurrentCharges;
        yield return Send(a, 0, Key.E);
        Require(cast.IsAiming && cast.CurrentCharges == charges, "E 슬롯 3: 누름은 조준, 충전 미소모");
        yield return Send(a, 0, Key.E, Key.Escape);
        Require(!cast.IsAiming && cast.CurrentCharges == charges, "ESC 투망 취소");
        yield return Send(a, 0);
        Require(cast.CurrentCharges == charges, "취소 후 키 뗌 발동 방지");
        yield return Send(a, 0, Key.E);
        yield return Send(a, 2, Key.E);
        Require(!cast.IsAiming && cast.CurrentCharges == charges, "RMB 투망 취소");
        yield return Send(a, 0);
        yield return Send(a, 0, Key.E);
        yield return Send(a, 0);
        Require(!cast.IsAiming && cast.CurrentCharges == charges - 1 && fish.CurrentResistance < resistance,
            "E 키 뗌: 충전 1회 소모 및 실제 포획 피해");
        Object.Destroy(target);

        int gold = run.CurrentGold;
        yield return Send(a, 0, Key.W);
        yield return Send(a, 1);
        yield return Send(b, 1);
        Require(net.IsDragging, "그물 드래그 미리보기");
        yield return Send(b, 1, Key.Escape);
        yield return Send(b, 0);
        Require(!net.IsDragging && net.ActiveNetCount == 0 && run.CurrentGold == gold, "ESC 드래그 취소: 비용 없음");
        yield return Send(a, 0, Key.W);
        yield return Send(a, 1);
        yield return Send(b, 1);
        yield return Send(b, 0);
        Require(net.ActiveNetCount == 1 && run.CurrentGold < gold, "W 그물 설치 및 기존 비용 적용");

        Vector2 rodPoint = Camera.main.WorldToScreenPoint(new Vector3(3, -2, 0));
        gold = run.CurrentGold;
        yield return Send(rodPoint, 0, Key.R);
        yield return Send(rodPoint, 1);
        yield return Send(rodPoint, 0);
        Require(rod.ActiveRodCount == 1 && run.CurrentGold < gold, "R 낚싯대 설치 및 기존 비용 적용");
        FishingRodController placedRod = Object.FindFirstObjectByType<FishingRodController>();
        Vector3 original = placedRod.transform.position;
        Vector2 moved = Camera.main.WorldToScreenPoint(new Vector3(4, -1, 0));
        Vector2 baitBefore = bait.Position;
        yield return Send(rodPoint, 1, Key.LeftCtrl);
        Require(GearRepositionController.IsRepositioning && !placedRod.IsOperational, "Ctrl+LMB 재배치 시작/작동 중지");
        yield return Send(moved, 1, Key.LeftCtrl, Key.Q, Key.W, Key.E, Key.R);
        Require(GearRepositionController.IsRepositioning && !NetPlacementController.IsNetModeActive &&
            !FishingRodPlacementController.IsRodModeActive && !cast.IsAiming && bait.Position == baitBefore,
            "재배치 중 4슬롯 충돌 차단");
        yield return Send(moved, 1, Key.LeftCtrl, Key.Escape);
        yield return Send(moved, 0);
        Require(!GearRepositionController.IsRepositioning && placedRod.transform.position == original, "ESC 재배치 원위치 복원");
        yield return Send(rodPoint, 1, Key.LeftCtrl);
        yield return Send(moved, 1, Key.LeftCtrl);
        yield return Send(moved, 0);
        Require(!GearRepositionController.IsRepositioning && placedRod.transform.position != original, "재배치 완료");

        // Swapping proves keys identify slots rather than controllers.
        Require(slots.TryAssignSlot(0, ToolId.Net) && slots.GetSlot(1) == ToolId.Bait, "중복 없이 슬롯 교환");
        yield return Send(a, 0);
        yield return Send(a, 0, Key.Q);
        Require(NetPlacementController.IsNetModeActive, "교환 후 Q로 그물 실행");
        yield return Send(a, 2);
        slots.TryAssignSlot(0, ToolId.None);
        yield return Send(a, 0);
        yield return Send(a, 0, Key.Q);
        Require(!NetPlacementController.IsNetModeActive, "빈 Q 슬롯 무동작");
        slots.TryAssignSlot(0, ToolId.CastNet);
        yield return new WaitForSeconds(8f);
        yield return Send(a, 0);
        yield return Send(a, 0, Key.Q);
        Require(cast.IsAiming && ToolSlotInput.GetBindingLabel(ToolId.CastNet) == "Q", "투망 Q 재배정 및 키 안내");
        charges = cast.CurrentCharges;
        slots.TryAssignSlot(3, ToolId.CastNet);
        yield return Send(a, 0);
        Require(!cast.IsAiming && cast.CurrentCharges == charges, "조준 중 슬롯 교환: 취소 및 유령 발동 없음");
        yield return Send(a, 0, Key.R);
        Require(cast.IsAiming, "교환된 R 투망 조준");
        PrototypeAugmentManager.Instance.ShowChoices();
        yield return Send(a, 0);
        Require(!cast.IsAiming && cast.CurrentCharges == charges, "선택창 진입 시 조준 취소");
        yield return Send(a, 0, Key.Q, Key.W, Key.E, Key.R);
        Require(!cast.IsAiming && !NetPlacementController.IsNetModeActive &&
            !FishingRodPlacementController.IsRodModeActive, "선택 중 슬롯 입력 차단");
    }

    private static IEnumerator Send(Vector2 position, ushort buttons, params Key[] keys)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = buttons });
        yield return null;
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
