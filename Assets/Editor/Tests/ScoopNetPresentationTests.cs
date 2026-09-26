#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class ScoopNetPresentationTests
{
    private readonly List<UnityEngine.Object> owned = new();
    private Keyboard keyboard;
    private Mouse mouse;
    private Camera camera;
    private RunManager run;
    private ItemEffectManager effects;
    private PrototypeAugmentManager augment;
    private LandingNetController scoop;
    private LandingNetPresentation visual;
    private SpriteRenderer range;
    private ScoopNetPresentationProfile profile;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        profile = AssetDatabase.LoadAssetAtPath<ScoopNetPresentationProfile>(
            "Assets/Resources/ScoopNetPresentation.asset");
        Assert.That(profile, Is.Not.Null, "Run Setup Scoop Net Presentation first.");
        GameObject cameraObject = Own(new GameObject("Scoop Test Camera"));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.5f;
        keyboard = InputSystem.AddDevice<Keyboard>("ScoopTestKeyboard");
        mouse = InputSystem.AddDevice<Mouse>("ScoopTestMouse");
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        run = Own(new GameObject("Scoop Test Run")).AddComponent<RunManager>();
        Invoke(run, "Awake");
        effects = Own(new GameObject("Scoop Test Effects")).AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");
        GameObject rangeObject = Own(new GameObject("Scoop Test Range"));
        range = rangeObject.AddComponent<SpriteRenderer>();
        GameObject scoopObject = Own(new GameObject("Scoop Test Controller"));
        scoop = scoopObject.AddComponent<LandingNetController>();
        Set(scoop, "rangeVisual", rangeObject.transform);
        Set(scoop, "capturePower", 3f);
        Set(scoop, "captureRadius", 1.1f);
        Set(scoop, "attackCooldown", .45f);
        Set(scoop, "maxTargets", 3);
        Invoke(scoop, "Awake");
        visual = scoop.GetComponent<LandingNetPresentation>();
        Assert.That(visual, Is.Not.Null);
        Assert.That(visual.Profile, Is.SameAs(profile));
        Send(Vector2.zero, false);
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (augment != null && PrototypeAugmentManager.Instance == augment)
            Invoke(augment, "OnDisable");
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

    [Test]
    public void LmbUsesSameCursorPositionAndActualCircleDamage()
    {
        FishController inside = CreateFish(new Vector2(.5f, 0f));
        FishController outside = CreateFish(new Vector2(1.6f, 0f));
        Send(Vector2.zero, true);
        run.ToolInput.Sample();
        Invoke(scoop, "Update");
        Assert.That(inside.CurrentResistance, Is.EqualTo(27f));
        Assert.That(outside.CurrentResistance, Is.EqualTo(30f));
        Assert.That(Vector2.Distance(visual.AimPosition, Vector2.zero), Is.LessThan(.001f));
        Assert.That(CountActive("ScoopNetHitVisual"), Is.EqualTo(1));
        Assert.That(scoop.RemainingCooldown, Is.GreaterThan(0f));
        Assert.That(scoop.AttackCooldown, Is.EqualTo(.45f));
    }

    [Test]
    public void HeldInputDuringCooldownDoesNotAttackOrReplay()
    {
        FishController fish = CreateFish(Vector2.zero);
        Send(Vector2.zero, true);
        run.ToolInput.Sample();
        Invoke(scoop, "Update");
        float soundTime = (float)Field(effects, "lastScoopNetSoundTime");
        Invoke(scoop, "Update");
        Assert.That(fish.CurrentResistance, Is.EqualTo(27f));
        Assert.That(CountActive("ScoopNetHitVisual"), Is.EqualTo(1));
        Assert.That((float)Field(effects, "lastScoopNetSoundTime"), Is.EqualTo(soundTime));
    }

    [Test]
    public void ActualRangeUpgradeChangesFeedbackWithoutSecondConstant()
    {
        Send(Vector2.zero, false);
        run.ToolInput.Sample();
        Invoke(scoop, "Update");
        Assert.That(range.enabled, Is.True);
        Assert.That(range.transform.localScale.x, Is.EqualTo(2.2f).Within(.0001f));
        scoop.IncreaseCaptureRadius(.4f);
        Assert.That(scoop.CaptureRadius, Is.EqualTo(1.5f));
        Assert.That(range.transform.localScale.x, Is.EqualTo(3f).Within(.0001f));
    }

    [Test]
    public void ThreeRealDamageTargetsMakeThreeHitVfxButOneSwingSound()
    {
        FishController a = CreateFish(new Vector2(-.5f, 0f));
        FishController b = CreateFish(new Vector2(.5f, 0f));
        FishController c = CreateFish(new Vector2(0f, .5f));
        FishController fourth = CreateFish(new Vector2(0f, -.6f));
        Physics2D.SyncTransforms();
        Invoke(scoop, "UseLandingNet", Vector3.zero);
        Assert.That(a.CurrentResistance + b.CurrentResistance + c.CurrentResistance + fourth.CurrentResistance,
            Is.EqualTo(111f));
        Assert.That(CountActive("ScoopNetHitVisual"), Is.EqualTo(3));
        List<Vector2> damaged = new();
        foreach (FishController fish in new[] { a, b, c, fourth })
            if (fish.CurrentResistance < 30f) damaged.Add(fish.transform.position);
        foreach (Transform child in effects.transform)
            if (child.gameObject.activeSelf && child.name == "ScoopNetHitVisual")
            {
                int index = damaged.FindIndex(position =>
                    Vector2.Distance(position, child.position) < .001f);
                Assert.That(index, Is.GreaterThanOrEqualTo(0));
                damaged.RemoveAt(index);
            }
        Assert.That(damaged, Is.Empty);
        Assert.That(visual.IsSwinging, Is.True);
        Assert.That((float)Field(effects, "lastScoopNetSoundTime"), Is.EqualTo(Time.time));
        Assert.That(effects.PlayScoopNetSwingSound(profile), Is.False);
    }

    [Test]
    public void MissSwingsAndSoundsWithoutTargetHitVfx()
    {
        Invoke(scoop, "UseLandingNet", Vector3.zero);
        Assert.That(visual.IsSwinging, Is.True);
        Assert.That(CountActive("ScoopNetHitVisual"), Is.Zero);
        Assert.That((float)Field(effects, "lastScoopNetSoundTime"), Is.EqualTo(Time.time));
        visual.Tick(profile.SwingDuration + .01f);
        Assert.That(visual.IsSwinging, Is.False);
    }

    [Test]
    public void SelectionAndPauseBlockBothGameplayAndPresentation()
    {
        FishController fish = CreateFish(Vector2.zero);
        augment = Own(new GameObject("Scoop Test Selection")).AddComponent<PrototypeAugmentManager>();
        Invoke(augment, "Awake");
        Set(augment, "showChoices", true);
        Send(Vector2.zero, true);
        run.ToolInput.Sample();
        Invoke(scoop, "Update");
        Assert.That(fish.CurrentResistance, Is.EqualTo(30f));
        Assert.That(visual.IsSwinging || range.enabled, Is.False);
        Assert.That(CountActive("ScoopNetHitVisual"), Is.Zero);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastScoopNetSoundTime")), Is.True);
        Set(augment, "showChoices", false);
        Time.timeScale = 0f;
        run.ToolInput.Sample();
        Invoke(scoop, "Update");
        Assert.That(fish.CurrentResistance, Is.EqualTo(30f));
        Assert.That(visual.IsSwinging || range.enabled, Is.False);
    }

    [Test]
    public void MissingProfileAndFullPoolDoNotChangeDamage()
    {
        FishController fish = CreateFish(Vector2.zero);
        Set(visual, "profile", null);
        Invoke(scoop, "UseLandingNet", Vector3.zero);
        Assert.That(fish.CurrentResistance, Is.EqualTo(27f));
        Assert.That(CountActive("ScoopNetHitVisual"), Is.Zero);
        Set(visual, "profile", profile);
        CombatVfxSettings settings = new();
        Set(settings, "maximumActiveVisuals", 1);
        Type poolType = typeof(ItemEffectManager).Assembly.GetType("CombatVfxPool");
        object pool = Activator.CreateInstance(poolType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { effects.transform, settings }, null);
        Invoke(pool, "AcquirePersistent", "Important", Color.white, .1f, 2, 30);
        Set(effects, "combatVfxPool", pool);
        Invoke(scoop, "UseLandingNet", Vector3.zero);
        Assert.That(fish.CurrentResistance, Is.EqualTo(24f));
        Assert.That(CountActive("ScoopNetHitVisual"), Is.Zero);
    }

    [Test]
    public void SwingPoolPauseAndRunResetClearTransientState()
    {
        CreateFish(Vector2.zero);
        Invoke(scoop, "UseLandingNet", Vector3.zero);
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(1));
        Time.timeScale = 0f;
        Invoke(visual, "Update");
        Assert.That(visual.IsSwinging, Is.True);
        Invoke(scoop, "Update");
        Assert.That(visual.IsSwinging || range.enabled, Is.False);
        Time.timeScale = 1f;
        Invoke(effects, "ClearRuntimeState", false);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastScoopNetSoundTime")), Is.True);
    }

    [Test]
    public void ImportedPrototypeAndVfxPoolReturnAreValid()
    {
        Assert.That(profile.HasSwing && profile.HasHit, Is.True);
        Assert.That(profile.SwingFrames.Length, Is.EqualTo(5));
        Assert.That(profile.HitFrames.Length, Is.EqualTo(4));
        Assert.That(profile.SwingClip, Is.Not.Null);
        CreateFish(Vector2.zero);
        Invoke(scoop, "UseLandingNet", Vector3.zero);
        object pool = Field(effects, "combatVfxPool");
        Invoke(pool, "Tick", profile.HitDuration + .01f);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
    }

    private FishController CreateFish(Vector2 position)
    {
        GameObject obj = Own(new GameObject("Scoop Test Fish"));
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

    private void Send(Vector2 world, bool pressed)
    {
        keyboard.MakeCurrent();
        mouse.MakeCurrent();
        Vector2 position = camera.WorldToScreenPoint(world);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.QueueStateEvent(mouse, new MouseState
        {
            position = position,
            buttons = (ushort)(pressed ? 1 : 0)
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
