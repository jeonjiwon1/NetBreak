#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class NetPresentationTests
{
    private readonly List<Object> owned = new();
    private Keyboard keyboard;
    private Mouse mouse;
    private RunManager run;
    private NetPlacementController placement;
    private Transform preview;
    private NetController prefab;
    private ItemEffectManager effects;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        GameObject cameraObject = Own(new GameObject("Net Test Camera"));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        keyboard = InputSystem.AddDevice<Keyboard>("NetTestKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("NetTestMouse");
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        Send(new Vector2(100f, 100f));
        run = Own(new GameObject("Net Test Run")).AddComponent<RunManager>();
        Invoke(run, "Awake");
        prefab = AssetDatabase.LoadAssetAtPath<NetController>("Assets/Prefabs/Gear/Net.prefab");
        Assert.That(prefab, Is.Not.Null);
        GameObject ghost = Own(new GameObject("Net Test Preview"));
        ghost.AddComponent<SpriteRenderer>().color = new Color(.7f, .34f, .11f, .039f);
        preview = ghost.transform;
        placement = Own(new GameObject("Net Test Placement")).AddComponent<NetPlacementController>();
        Set(placement, "netPrefab", prefab);
        Set(placement, "netPreview", preview);
        Set(placement, "thickness", .3f);
        Set(placement, "maxLength", 8f);
        Invoke(placement, "Awake");
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (placement != null) Invoke(placement, "OnDisable");
        if (effects != null && ItemEffectManager.Instance == effects) Invoke(effects, "OnDisable");
        if (run != null && RunManager.Instance == run) Invoke(run, "OnDisable");
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        owned.Clear();
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        if (mouse != null) InputSystem.RemoveDevice(mouse);
    }

    [TestCase(0)]
    [TestCase(1)]
    public void DynamicNetSlotShowsStartPreviewOnlyForNet(int slot)
    {
        Assert.That(run.ToolSlots.TryAcquireToolAt(slot, ToolId.Net), Is.True);
        run.ToolInput.Sample();
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.Net).Cancelled, Is.False);
        Invoke(placement, "HandleModeInput", new ToolInputState(true, false, false));
        Assert.That(NetPlacementController.IsNetModeActive, Is.True);
        Assert.That(preview.gameObject.activeSelf, Is.True);
        Assert.That(preview.GetComponentsInChildren<Collider2D>(true), Is.Empty);
        Assert.That(preview.GetComponentsInChildren<NetController>(true), Is.Empty);
        Assert.That(preview.GetComponentsInChildren<AudioSource>(true), Is.Empty);
        Assert.That(preview.GetComponent<NetPresentation>(), Is.Not.Null);
    }

    [Test]
    public void OtherToolCannotEnterNetMode()
    {
        Assert.That(run.ToolSlots.TryAcquireToolAt(0, ToolId.FishingRod), Is.True);
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.Net).Cancelled, Is.True);
        Invoke(placement, "Update");
        Assert.That(NetPlacementController.IsNetModeActive, Is.False);
        Assert.That(preview.gameObject.activeSelf, Is.False);
    }

    [Test]
    public void DragPreviewAndPlacedColliderUseSameTransform()
    {
        effects = Own(new GameObject("Net Test Effects")).AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");
        Send(new Vector2(120f, 140f));
        Invoke(placement, "StartPlacement");
        Send(new Vector2(300f, 230f));
        Invoke(placement, "UpdatePlacement");
        Vector3 center = preview.position;
        Quaternion rotation = preview.rotation;
        Vector3 scale = preview.localScale;
        Assert.That(scale.y, Is.EqualTo(.3f).Within(.0001f));
        Assert.That(scale.x, Is.GreaterThan(.5f));
        Assert.That(preview.GetComponentsInChildren<Collider2D>(true), Is.Empty);
        Invoke(placement, "FinishPlacement");
        Assert.That(preview.gameObject.activeSelf, Is.False);
        Assert.That(placement.ActiveNetCount, Is.EqualTo(1));
        NetController net = ((List<NetController>)Field(placement, "activeNets"))[0];
        owned.Add(net.gameObject);
        Assert.That(net.transform.position, Is.EqualTo(center));
        Assert.That(Quaternion.Angle(net.transform.rotation, rotation), Is.LessThan(.001f));
        Assert.That(net.transform.localScale, Is.EqualTo(scale));
        Assert.That(net.GetComponent<BoxCollider2D>().size, Is.EqualTo(Vector2.one));
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastNetPlaceSoundTime")), Is.False);
    }

    [Test]
    public void CancelPauseAndSlotChangeClearDragAndPreview()
    {
        Invoke(placement, "HandleModeInput", new ToolInputState(true, false, false));
        Invoke(placement, "StartPlacement");
        Invoke(placement, "HandleModeInput", new ToolInputState(true, false, false));
        AssertHidden();
        Assert.That(placement.ActiveNetCount, Is.Zero);

        Invoke(placement, "HandleModeInput", new ToolInputState(true, false, false));
        Time.timeScale = 0f;
        Invoke(placement, "Update");
        AssertHidden();
        Time.timeScale = 1f;

        run.ToolSlots.TryAcquireToolAt(0, ToolId.Net);
        Invoke(placement, "HandleModeInput", new ToolInputState(true, false, false));
        run.ToolSlots.TryAssignSlot(0, ToolId.None);
        run.ToolInput.Sample();
        Invoke(placement, "Update");
        AssertHidden();
    }

    [Test]
    public void TiledArtHasNoGameplayColliderAndDisabledNetKeepsDarkTint()
    {
        NetController net = Object.Instantiate(prefab);
        owned.Add(net.gameObject);
        Invoke(net, "Awake");
        net.Initialize(Vector2.zero, new Vector2(4f, 0f), .3f);
        NetPresentation art = net.GetComponent<NetPresentation>();
        Assert.That(art, Is.Not.Null);
        art.RefreshVisual();
        Assert.That(net.GetComponentsInChildren<Collider2D>().Length, Is.EqualTo(1));
        SpriteRenderer mesh = net.transform.Find("NetMeshVisual").GetComponent<SpriteRenderer>();
        Assert.That(mesh.drawMode, Is.EqualTo(SpriteDrawMode.Tiled));
        Assert.That(mesh.size.x, Is.EqualTo(4f).Within(.001f));
        Color active = mesh.color;
        net.DisableTemporarily(3f);
        art.RefreshVisual();
        Assert.That(net.IsOperational, Is.False);
        Assert.That(mesh.color.r, Is.LessThan(active.r));
        Assert.That(net.GetComponent<BoxCollider2D>().enabled, Is.False);
        Set(net, "specialDisabledUntil", Time.time - 1f);
        Invoke(net, "RefreshOperationalState");
        art.RefreshVisual();
        Assert.That(net.IsOperational, Is.True);
        Assert.That(mesh.color.r, Is.GreaterThan(active.r * .9f));
    }

    [Test]
    public void EachNetHasIndependentTileSize()
    {
        NetController first = Object.Instantiate(prefab);
        NetController second = Object.Instantiate(prefab);
        owned.Add(first.gameObject);
        owned.Add(second.gameObject);
        Invoke(first, "Awake");
        Invoke(second, "Awake");
        first.Initialize(Vector2.zero, new Vector2(2f, 0f), .3f);
        second.Initialize(Vector2.zero, new Vector2(5f, 0f), .3f);
        first.GetComponent<NetPresentation>().RefreshVisual();
        second.GetComponent<NetPresentation>().RefreshVisual();
        SpriteRenderer a = first.transform.Find("NetMeshVisual").GetComponent<SpriteRenderer>();
        SpriteRenderer b = second.transform.Find("NetMeshVisual").GetComponent<SpriteRenderer>();
        Assert.That(a, Is.Not.SameAs(b));
        Assert.That(a.size.x, Is.EqualTo(2f).Within(.001f));
        Assert.That(b.size.x, Is.EqualTo(5f).Within(.001f));
    }

    [Test]
    public void ContactVisualFollowsFirstSuccessfulSlowRegistrationOnly()
    {
        effects = Own(new GameObject("Net Test Effects")).AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");
        NetController net = Object.Instantiate(prefab);
        owned.Add(net.gameObject);
        Invoke(net, "Awake");
        GameObject fishObject = Own(new GameObject("Net Test Fish"));
        fishObject.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = fishObject.AddComponent<CircleCollider2D>();
        FishController fish = fishObject.AddComponent<FishController>();
        fishObject.AddComponent<FishMovement>();
        Invoke(fish, "Awake");
        FishData data = Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject serialized = new(data);
        serialized.FindProperty("maxResistance").floatValue = 20f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        fish.Initialize(data);
        int contacts = 0;
        net.FishContactPresented += (_, _) => contacts++;
        Invoke(net, "OnTriggerEnter2D", collider);
        Assert.That(contacts, Is.EqualTo(1));
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(1));
        Invoke(net, "OnTriggerStay2D", collider);
        Invoke(net, "OnTriggerStay2D", collider);
        Assert.That(contacts, Is.EqualTo(1));
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(1));
    }

    [Test]
    public void MissingProfileStillRegistersSlowAndAppliesResistanceDamage()
    {
        (NetController net, FishController fish, Collider2D collider) = CreateContactPair();
        Set(net, "netPresentation", null);
        float before = fish.CurrentResistance;
        Invoke(net, "OnTriggerEnter2D", collider);
        Invoke(net, "OnTriggerStay2D", collider);
        Assert.That(NetContacts(fish), Is.EqualTo(1));
        Assert.That(fish.CurrentResistance, Is.LessThan(before));
    }

    [Test]
    public void FullVfxPoolStillRegistersSlowAndAppliesResistanceDamage()
    {
        effects = Own(new GameObject("Net Test Effects")).AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");
        CombatVfxSettings settings = new();
        Set(settings, "maximumActiveVisuals", 1);
        System.Type poolType = typeof(ItemEffectManager).Assembly.GetType("CombatVfxPool");
        object pool = System.Activator.CreateInstance(poolType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { effects.transform, settings }, null);
        poolType.GetMethod("AcquirePersistent").Invoke(pool,
            new object[] { "ReservedHighPriority", Color.white, .01f, 2, 1 });
        Set(effects, "combatVfxPool", pool);
        (NetController net, FishController fish, Collider2D collider) = CreateContactPair();
        float before = fish.CurrentResistance;
        Invoke(net, "OnTriggerEnter2D", collider);
        Invoke(net, "OnTriggerStay2D", collider);
        Assert.That(NetContacts(fish), Is.EqualTo(1));
        Assert.That(fish.CurrentResistance, Is.LessThan(before));
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(1));
    }

    private (NetController, FishController, Collider2D) CreateContactPair()
    {
        NetController net = Object.Instantiate(prefab);
        owned.Add(net.gameObject);
        Invoke(net, "Awake");
        GameObject fishObject = Own(new GameObject("Net Contact Fish"));
        fishObject.AddComponent<SpriteRenderer>();
        Collider2D collider = fishObject.AddComponent<CircleCollider2D>();
        FishController fish = fishObject.AddComponent<FishController>();
        fishObject.AddComponent<FishMovement>();
        Invoke(fish, "Awake");
        FishData data = Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject serialized = new(data);
        serialized.FindProperty("maxResistance").floatValue = 20f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        fish.Initialize(data);
        return (net, fish, collider);
    }

    private static int NetContacts(FishController fish) =>
        ((System.Collections.IDictionary)Field(fish.GetComponent<FishMovement>(), "activeNets")).Count;

    private void AssertHidden()
    {
        Assert.That(NetPlacementController.IsNetModeActive, Is.False);
        Assert.That(placement.IsDragging, Is.False);
        Assert.That(preview.gameObject.activeSelf, Is.False);
    }

    private void Send(Vector2 position)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
        InputSystem.Update();
    }

    private T Own<T>(T value) where T : Object
    {
        owned.Add(value);
        return value;
    }

    private static object Field(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static object Invoke(object target, string name, params object[] args) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
        .Invoke(target, args);
}
#endif
