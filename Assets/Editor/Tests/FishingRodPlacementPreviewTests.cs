#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class FishingRodPlacementPreviewTests
{
    private readonly List<UnityEngine.Object> owned = new();
    private Camera camera;
    private Keyboard keyboard;
    private Mouse mouse;
    private RunManager run;
    private FishingRodController prefab;
    private FishingRodPlacementController placement;
    private Transform ghost;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        GameObject cameraObject = Own(new GameObject("Rod Preview Camera"));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;

        keyboard = InputSystem.AddDevice<Keyboard>("RodPreviewKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("RodPreviewMouse");
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        Send(Vector2.zero);

        run = Own(new GameObject("Rod Preview Run")).AddComponent<RunManager>();
        Invoke(run, "Awake");
        prefab = AssetDatabase.LoadAssetAtPath<FishingRodController>(
            "Assets/Prefabs/Gear/FishingRod.prefab");
        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.RangeVisual, Is.Not.Null);

        GameObject ghostObject = Own(new GameObject("Rod Ghost"));
        ghost = ghostObject.transform;
        ghost.localScale = new Vector3(.35f, .35f, 1f);
        SpriteRenderer ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        ghostRenderer.color = new Color(1f, 1f, 1f, .039215688f);

        placement = Own(new GameObject("Rod Placement"))
            .AddComponent<FishingRodPlacementController>();
        SetField(placement, "rodPrefab", prefab);
        SetField(placement, "placementPreview", ghost);
        Invoke(placement, "Awake");
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (placement != null) Invoke(placement, "OnDisable");
        if (run != null && RunManager.Instance == run) Invoke(run, "OnDisable");
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
        owned.Clear();
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        if (mouse != null) InputSystem.RemoveDevice(mouse);
    }

    [Test]
    public void EnteringModeShowsOnePixelGhostAndRangeWithoutGameplayComponents()
    {
        Assert.That(run.ToolSlots.TryAcquireToolAt(0, ToolId.FishingRod), Is.True);
        Assert.That(run.ToolSlots.FindSlot(ToolId.FishingRod), Is.EqualTo(0));
        Send(new Vector2(120f, 140f));
        run.ToolInput.Sample();
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.FishingRod).Cancelled, Is.False);
        // EditMode's synthetic keyboard state can be held without reporting
        // wasPressedThisFrame. Exercise the resolved placement input directly.
        Invoke(placement, "HandleModeInput", new ToolInputState(true, false, false));
        Transform range = Range();
        Assert.That(FishingRodPlacementController.IsRodModeActive, Is.True);
        Assert.That(ghost.gameObject.activeSelf && range.gameObject.activeSelf, Is.True);
        Assert.That(ghost.GetComponent<SpriteRenderer>().sprite,
            Is.SameAs(prefab.GetComponent<SpriteRenderer>().sprite));
        Assert.That(ghost.GetComponent<SpriteRenderer>().color.a, Is.GreaterThan(.5f));
        Assert.That(ghost.GetComponentsInChildren<FishingRodController>(true).Length, Is.Zero);
        Assert.That(ghost.GetComponentsInChildren<Collider2D>(true).Length, Is.Zero);
        Assert.That(ghost.GetComponentsInChildren<LineRenderer>(true).Length, Is.Zero);
        Assert.That(ghost.GetComponentsInChildren<AudioSource>(true).Length, Is.Zero);
        Invoke(placement, "StartPlacementMode");
        Assert.That(ghost.childCount, Is.EqualTo(1));
    }

    [Test]
    public void CursorAndRangeMoveTogetherAndUseUpgradedGameplayRange()
    {
        Send(new Vector2(100f, 120f));
        Invoke(placement, "StartPlacementMode");
        Vector3 first = ghost.position;
        Send(new Vector2(310f, 230f));
        Invoke(placement, "UpdatePreview");
        Transform range = Range();
        Vector3 expected = camera.ScreenToWorldPoint(new Vector2(310f, 230f));
        Assert.That(Vector2.Distance(ghost.position, expected), Is.LessThan(.001f));
        Assert.That(Vector2.Distance(first, ghost.position), Is.GreaterThan(.01f));
        Assert.That(range.position, Is.EqualTo(ghost.position));
        Assert.That(placement.PreviewCaptureRange, Is.EqualTo(prefab.CaptureRange));
        Assert.That(range.lossyScale.x, Is.EqualTo(prefab.CaptureRange * 2f).Within(.001f));

        placement.IncreaseRodRange(.5f);
        Invoke(placement, "UpdateRangePreview");
        Assert.That(placement.PreviewCaptureRange,
            Is.EqualTo(prefab.CaptureRange + .5f).Within(.001f));
        Assert.That(range.lossyScale.x,
            Is.EqualTo(placement.PreviewCaptureRange * 2f).Within(.001f));
    }

    [Test]
    public void ConfirmUsesVisiblePositionAndHidesBothPreviewParts()
    {
        Send(new Vector2(100f, 120f));
        Invoke(placement, "StartPlacementMode");
        Vector3 seen = ghost.position;
        Send(new Vector2(300f, 300f));
        Invoke(placement, "TryPlaceRod");
        Assert.That(placement.ActiveRodCount, Is.EqualTo(1));
        Assert.That(FishingRodPlacementController.IsRodModeActive, Is.False);
        Assert.That(ghost.gameObject.activeSelf || Range().gameObject.activeSelf, Is.False);
        FishingRodController placed = (FishingRodController)((List<FishingRodController>)
            Field(placement, "activeRods"))[0];
        Assert.That(placed.transform.position, Is.EqualTo(seen));
        Assert.That(placed.GetComponent<FishingRodPresentation>(), Is.Not.Null);
        owned.Add(placed.gameObject);
    }

    [Test]
    public void CancelPauseToolSwitchAndDisableLeaveNoPreview()
    {
        Invoke(placement, "StartPlacementMode");
        Invoke(placement, "CancelPlacementMode");
        AssertHidden();

        Invoke(placement, "StartPlacementMode");
        Time.timeScale = 0f;
        Invoke(placement, "Update");
        AssertHidden();
        Time.timeScale = 1f;

        run.ToolSlots.TryAcquireToolAt(0, ToolId.FishingRod);
        Invoke(placement, "StartPlacementMode");
        run.ToolSlots.TryAssignSlot(0, ToolId.None);
        run.ToolInput.Sample();
        Invoke(placement, "Update");
        AssertHidden();

        Invoke(placement, "StartPlacementMode");
        Invoke(placement, "OnDisable");
        AssertHidden();
    }

    [Test]
    public void OtherToolInQDoesNotEnterRodMode()
    {
        Assert.That(run.ToolSlots.TryAcquireToolAt(0, ToolId.Net), Is.True);
        Send(Vector2.zero);
        run.ToolInput.Sample();
        Send(Vector2.zero, Key.Q);
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.FishingRod).Pressed, Is.False);
        Assert.That(ToolSlotInput.Read(ToolId.FishingRod).Cancelled, Is.True);
        Invoke(placement, "Update");
        AssertHidden();
    }

    private Transform Range()
    {
        Transform range = (Transform)Field(placement, "rangePreview");
        Assert.That(range, Is.Not.Null);
        return range;
    }

    private void AssertHidden()
    {
        Assert.That(FishingRodPlacementController.IsRodModeActive, Is.False);
        Assert.That(ghost.gameObject.activeSelf, Is.False);
        Assert.That(Range().gameObject.activeSelf, Is.False);
    }

    private void Send(Vector2 position, params Key[] keys)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        InputSystem.Update();
    }

    private T Own<T>(T value) where T : UnityEngine.Object
    {
        owned.Add(value);
        return value;
    }

    private static object Field(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

    private static void SetField(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static object Invoke(object target, string name, params object[] args) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
        .Invoke(target, args);
}
#endif
