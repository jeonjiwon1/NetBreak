#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class CastNetPresentationTests
{
    private readonly List<UnityEngine.Object> owned = new();
    private Keyboard keyboard;
    private Mouse mouse;
    private Camera camera;
    private RunManager run;
    private ItemEffectManager effects;
    private PrototypeAugmentManager selection;
    private CastNetController cast;
    private CastNetPresentation visual;
    private Transform area;
    private CastNetPresentationProfile profile;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        profile = AssetDatabase.LoadAssetAtPath<CastNetPresentationProfile>(
            "Assets/Resources/CastNetPresentation.asset");
        Assert.That(profile, Is.Not.Null, "Run Setup Cast Net Presentation first.");
        GameObject cameraObject = Own(new GameObject("Cast Net Test Camera"));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.5f;
        keyboard = InputSystem.AddDevice<Keyboard>("CastNetTestKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("CastNetTestMouse");
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        Send(Vector2.zero);
        run = Own(new GameObject("Cast Net Test Run")).AddComponent<RunManager>();
        Invoke(run, "Awake");
        effects = Own(new GameObject("Cast Net Test Effects")).AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");
        GameObject castObject = Own(new GameObject("Cast Net Test Controller"));
        GameObject areaObject = new("Cast Net Test Area");
        areaObject.transform.SetParent(castObject.transform);
        area = areaObject.transform;
        areaObject.AddComponent<SpriteRenderer>();
        cast = castObject.AddComponent<CastNetController>();
        Set(cast, "castVisual", area);
        Set(cast, "capturePower", 3f);
        Set(cast, "captureRadius", 1f);
        Set(cast, "cooldown", 7f);
        Invoke(cast, "Awake");
        visual = cast.GetComponent<CastNetPresentation>();
        Assert.That(visual.Profile, Is.SameAs(profile));
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (cast != null) Invoke(cast, "OnDisable");
        if (selection != null && PrototypeAugmentManager.Instance == selection)
            Invoke(selection, "OnDisable");
        if (effects != null && ItemEffectManager.Instance == effects)
            Invoke(effects, "OnDisable");
        if (run != null && RunManager.Instance == run)
            Invoke(run, "OnDisable");
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
        owned.Clear();
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        if (mouse != null) InputSystem.RemoveDevice(mouse);
    }

    [TestCase(0)]
    [TestCase(1)]
    public void QOrWRunSlotEntersAimAndUsesAuthoritativeRadius(int slot)
    {
        Assert.That(run.ToolSlots.TryAcquireToolAt(slot, ToolId.CastNet), Is.True);
        run.ToolInput.Sample();
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.CastNet).Cancelled, Is.False);
        Assert.That(ToolSlotInput.GetBindingLabel(ToolId.CastNet),
            Is.EqualTo(slot == 0 ? "Q" : "W"));
        Send(new Vector2(.4f, -.2f));
        Invoke(cast, "HandleCastNetInput", new ToolInputState(true, false, false));
        Assert.That(cast.IsAiming && visual.IsAiming && area.gameObject.activeSelf, Is.True);
        Assert.That(Vector2.Distance(visual.AimPosition, cast.CurrentAimPosition), Is.LessThan(.001f));
        Assert.That(visual.AimRadius, Is.EqualTo(cast.CaptureRadius));
        Assert.That(area.localScale.x, Is.EqualTo(2f).Within(.001f));
        Assert.That(visual.GetComponentsInChildren<Collider2D>(true), Is.Empty);
        Assert.That(visual.GetComponentsInChildren<AudioSource>(true), Is.Empty);
    }

    [Test]
    public void WrongToolAndUnavailableChargeDoNotEnterAim()
    {
        Assert.That(run.ToolSlots.TryAcquireToolAt(0, ToolId.Net), Is.True);
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.CastNet).Cancelled, Is.True);
        Assert.That(cast.IsAiming, Is.False);
        Set(cast, "currentCharges", 0);
        Invoke(cast, "HandleCastNetInput", new ToolInputState(true, false, false));
        Assert.That(visual.IsAiming, Is.False);
    }

    [Test]
    public void CursorMoveUpgradeConfirmAndMultiHitMatchActualDamage()
    {
        FishController a = CreateFish(new Vector2(.5f, 0f));
        FishController b = CreateFish(new Vector2(-.6f, 0f));
        FishController outside = CreateFish(new Vector2(1.8f, 0f));
        cast.IncreaseCaptureRadius(.2f);
        Send(new Vector2(-.5f, 0f));
        Invoke(cast, "HandleCastNetInput", new ToolInputState(true, false, false));
        Send(Vector2.zero);
        Invoke(cast, "HandleCastNetInput", new ToolInputState(false, false, false));
        Assert.That(visual.AimPosition, Is.EqualTo(Vector2.zero));
        Assert.That(visual.AimRadius, Is.EqualTo(1.2f).Within(.001f));
        Assert.That(area.localScale.x, Is.EqualTo(2.4f).Within(.001f));
        Invoke(cast, "HandleCastNetInput", new ToolInputState(false, true, false));
        Assert.That(CountActive("CastNetAreaVisual"), Is.Zero,
            "The area pulse follows the visible net opening.");
        visual.Tick(profile.OpeningDuration * .65f);
        Assert.That(a.CurrentResistance, Is.EqualTo(27f));
        Assert.That(b.CurrentResistance, Is.EqualTo(27f));
        Assert.That(outside.CurrentResistance, Is.EqualTo(30f));
        Assert.That(CountActive("CastNetAreaVisual"), Is.EqualTo(1));
        Assert.That(CountActive("CastNetHitVisual"), Is.EqualTo(2));
        Assert.That(cast.CurrentCharges, Is.Zero);
        Assert.That(cast.CooldownTimer, Is.EqualTo(7f).Within(.001f));
        Assert.That(visual.IsOpening, Is.True);
        Assert.That(visual.IsAiming, Is.False);
        Assert.That((float)Field(effects, "lastCastNetSoundTime"), Is.EqualTo(Time.time));
        Assert.That(effects.PlayCastNetSound(profile), Is.False);
        visual.Tick(profile.OpeningDuration + .01f);
        Assert.That(visual.IsOpening, Is.False);
    }

    [Test]
    public void MissHasAreaAndSoundWithoutFakeFishHit()
    {
        Invoke(cast, "UseCastNet", Vector2.zero, true, "cast_net");
        visual.Tick(profile.OpeningDuration * .65f);
        Assert.That(CountActive("CastNetAreaVisual"), Is.EqualTo(1));
        Assert.That(CountActive("CastNetHitVisual"), Is.Zero);
        Assert.That((float)Field(effects, "lastCastNetSoundTime"), Is.EqualTo(Time.time));
    }

    [Test]
    public void CancelPauseAndSlotChangeLeaveNoAimOrChargeCost()
    {
        Send(Vector2.zero);
        Invoke(cast, "HandleCastNetInput", new ToolInputState(true, false, false));
        Invoke(cast, "CancelAiming");
        Assert.That(cast.IsAiming || visual.IsAiming || area.gameObject.activeSelf, Is.False);
        Assert.That(cast.CurrentCharges, Is.EqualTo(1));
        Assert.That(cast.CooldownTimer, Is.Zero);
        Assert.That(CountActive("CastNetAreaVisual"), Is.Zero);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastCastNetSoundTime")), Is.True);

        Invoke(cast, "HandleCastNetInput", new ToolInputState(true, false, false));
        Time.timeScale = 0f;
        Invoke(cast, "Update");
        Assert.That(cast.IsAiming || visual.IsAiming, Is.False);
        Time.timeScale = 1f;
        run.ToolSlots.TryAcquireToolAt(0, ToolId.CastNet);
        Invoke(cast, "HandleCastNetInput", new ToolInputState(true, false, false));
        run.ToolSlots.TryAssignSlot(0, ToolId.None);
        run.ToolInput.Sample();
        Invoke(cast, "Update");
        Assert.That(cast.IsAiming || visual.IsAiming, Is.False);
    }

    [Test]
    public void MissingProfileAndFullPoolLeaveGameplayDamageIntact()
    {
        FishController fish = CreateFish(Vector2.zero);
        Set(visual, "profile", null);
        Invoke(cast, "UseCastNet", Vector2.zero, true, "cast_net");
        Assert.That(fish.CurrentResistance, Is.EqualTo(27f));
        Assert.That(CountActive("CastNetAreaVisual"), Is.Zero);
        Set(visual, "profile", profile);
        CombatVfxSettings settings = new();
        Set(settings, "maximumActiveVisuals", 1);
        Type poolType = typeof(ItemEffectManager).Assembly.GetType("CombatVfxPool");
        object pool = Activator.CreateInstance(poolType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { effects.transform, settings }, null);
        Invoke(pool, "AcquirePersistent", "Important", Color.white, .1f, 2, 30);
        Set(effects, "combatVfxPool", pool);
        Set(cast, "currentCharges", 1);
        Invoke(cast, "UseCastNet", Vector2.zero, true, "cast_net");
        visual.Tick(profile.OpeningDuration * .65f);
        Assert.That(fish.CurrentResistance, Is.EqualTo(24f));
        Assert.That(CountActive("CastNetAreaVisual"), Is.Zero);
        Assert.That(CountActive("CastNetHitVisual"), Is.Zero);
    }

    [Test]
    public void TacticalMultiplierUsesSameCursorAndActualHitArea()
    {
        FishController fish = CreateFish(new Vector2(1.5f, 0f));
        Send(Vector2.zero);
        Assert.That(cast.BeginTacticalAim(2f), Is.True);
        Assert.That(visual.AimRadius, Is.EqualTo(2f));
        Assert.That(area.localScale.x, Is.EqualTo(4f));
        Assert.That(cast.ConfirmTacticalCast(2f), Is.True);
        // E remains available even when this Run does not own Cast Net in Q/W.
        Invoke(cast, "Update");
        Assert.That(visual.IsOpening, Is.True);
        visual.Tick(profile.OpeningDuration * .65f);
        Assert.That(fish.CurrentResistance, Is.EqualTo(27f));
        Assert.That(CountActive("CastNetHitVisual"), Is.EqualTo(1));
        Assert.That(cast.CurrentCharges, Is.EqualTo(1));
        Assert.That(area.localScale.x, Is.EqualTo(2f));
    }

    [Test]
    public void PausedTacticalTargetingCannotConfirmOrPlayPresentation()
    {
        Send(Vector2.zero);
        Assert.That(cast.BeginTacticalAim(), Is.True);
        Time.timeScale = 0f;
        Invoke(cast, "Update");
        Assert.That(cast.ConfirmTacticalCast(), Is.False);
        Assert.That(cast.IsAiming || visual.IsAiming || visual.IsOpening, Is.False);
        Assert.That(CountActive("CastNetAreaVisual"), Is.Zero);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastCastNetSoundTime")),
            Is.True);
    }

    [Test]
    public void SelectionAndRunResetClearPreviewVfxAndAudioState()
    {
        selection = Own(new GameObject("Cast Net Test Selection"))
            .AddComponent<PrototypeAugmentManager>();
        Invoke(selection, "Awake");
        Set(selection, "showChoices", true);
        run.ToolSlots.TryAcquireToolAt(0, ToolId.CastNet);
        run.ToolInput.Sample();
        Assert.That(ToolSlotInput.Read(ToolId.CastNet).Cancelled, Is.True);
        Invoke(cast, "Update");
        Assert.That(cast.IsAiming || visual.IsAiming, Is.False);
        Assert.That(CountActive("CastNetAreaVisual"), Is.Zero);
        Set(selection, "showChoices", false);
        CreateFish(Vector2.zero);
        Invoke(cast, "UseCastNet", Vector2.zero, true, "cast_net");
        visual.Tick(profile.OpeningDuration * .65f);
        Assert.That(effects.ActiveCombatVfxCount, Is.GreaterThan(0));
        Invoke(cast, "OnDisable");
        Invoke(effects, "ClearRuntimeState", false);
        Assert.That(visual.IsAiming || visual.IsOpening, Is.False);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastCastNetSoundTime")),
            Is.True);
    }

    private FishController CreateFish(Vector2 position)
    {
        GameObject obj = Own(new GameObject("Cast Net Test Fish"));
        obj.transform.position = position;
        obj.AddComponent<CircleCollider2D>().radius = .12f;
        FishController fish = obj.AddComponent<FishController>();
        Invoke(fish, "Awake");
        FishData data = Own(ScriptableObject.CreateInstance<FishData>());
        Set(data, "maxResistance", 30f);
        fish.Initialize(data);
        Physics2D.SyncTransforms();
        return fish;
    }

    private void Send(Vector2 world, params Key[] keys)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        InputSystem.QueueStateEvent(mouse, new MouseState
        {
            position = camera.WorldToScreenPoint(world)
        });
        InputSystem.Update();
    }

    private int CountActive(string name)
    {
        int count = 0;
        foreach (Transform child in effects.transform)
            if (child.gameObject.activeSelf && child.name == name) count++;
        return count;
    }

    private T Own<T>(T value) where T : UnityEngine.Object
    {
        owned.Add(value);
        return value;
    }

    private static object Field(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static object Invoke(object target, string name, params object[] args) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Invoke(target, args);
}
#endif
