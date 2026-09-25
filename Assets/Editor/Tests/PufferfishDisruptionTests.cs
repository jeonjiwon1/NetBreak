#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class PufferfishDisruptionTests
{
    private readonly List<UnityEngine.Object> owned = new();
    private NetController net;
    private FishController fish;
    private Collider2D fishCollider;
    private FishVisualController visual;
    private ItemEffectManager effects;
    private PufferfishDisruptionProfile profile;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        profile = AssetDatabase.LoadAssetAtPath<PufferfishDisruptionProfile>(
            "Assets/Resources/PufferfishDisruption.asset");
        Assert.That(profile, Is.Not.Null);
        GameObject effectsObject = Own(new GameObject("Puffer Effects Test"));
        effects = effectsObject.AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");

        GameObject netObject = Own(new GameObject("Puffer Net Test"));
        netObject.AddComponent<SpriteRenderer>();
        BoxCollider2D box = netObject.AddComponent<BoxCollider2D>();
        box.size = Vector2.one;
        netObject.AddComponent<Rigidbody2D>();
        net = netObject.AddComponent<NetController>();
        Invoke(net, "Awake");

        GameObject fishObject = Own(new GameObject("Puffer Fish Test"));
        fishObject.transform.position = new Vector2(.2f, 0f);
        fishObject.AddComponent<SpriteRenderer>();
        fishCollider = fishObject.AddComponent<CircleCollider2D>();
        fish = fishObject.AddComponent<FishController>();
        fishObject.AddComponent<FishMovement>();
        Invoke(fish, "Awake");
        visual = fishObject.GetComponent<FishVisualController>();
        FishData data = Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject serialized = new SerializedObject(data);
        serialized.FindProperty("specialType").enumValueIndex = (int)FishSpecialType.Pufferfish;
        serialized.FindProperty("visualProfile").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<FishVisualProfile>(
                "Assets/Art/Fish/Pufferfish/Pufferfish_VisualProfile.asset");
        serialized.FindProperty("maxResistance").floatValue = 18f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        fish.Initialize(data);
        Physics2D.SyncTransforms();
        Assert.That(fishCollider.bounds.Intersects(box.bounds), Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (effects != null && ItemEffectManager.Instance == effects)
            Invoke(effects, "OnDisable");
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
        owned.Clear();
    }

    [Test]
    public void RealPufferContactDisablesImmediatelyAndTriggersOnce()
    {
        int events = 0;
        Vector2 hit = Vector2.positiveInfinity;
        net.PufferfishPresentationTriggered += (target, point) =>
        {
            Assert.That(net.IsOperational, Is.False);
            Assert.That(target, Is.SameAs(fish));
            events++;
            hit = point;
        };
        float resistance = fish.CurrentResistance;
        Trigger("OnTriggerEnter2D");
        Assert.That(net.IsOperational, Is.False);
        Assert.That((float)Field(net, "specialDisabledUntil"),
            Is.EqualTo(Time.time + 3f).Within(.001f));
        Assert.That(fish.CurrentResistance, Is.EqualTo(resistance));
        Assert.That((float)Field(net, "currentDamageMultiplier"), Is.EqualTo(1f));
        Assert.That(events, Is.EqualTo(1));
        Assert.That(Vector2.Distance(hit, net.transform.position), Is.LessThan(1f));
        Assert.That(visual.IsPlayingSpecial, Is.True);
        Assert.That(CountActive("PufferfishNetImpactVisual"), Is.EqualTo(1));
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastPufferfishSoundTime")),
            Is.False);
        Trigger("OnTriggerStay2D");
        Assert.That(events, Is.EqualTo(1));
        Assert.That(CountActive("PufferfishNetImpactVisual"), Is.EqualTo(1));
    }

    [Test]
    public void NonPufferOrAlreadyDisabledNetHasNoPresentation()
    {
        int events = 0;
        net.PufferfishPresentationTriggered += (_, _) => events++;
        SerializedObject data = new SerializedObject(fish.Data);
        data.FindProperty("specialType").enumValueIndex = (int)FishSpecialType.None;
        data.ApplyModifiedPropertiesWithoutUndo();
        Trigger("OnTriggerEnter2D");
        Assert.That(net.IsOperational, Is.True);
        Assert.That(events, Is.Zero);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        data.Update();
        data.FindProperty("specialType").enumValueIndex = (int)FishSpecialType.Pufferfish;
        data.ApplyModifiedPropertiesWithoutUndo();
        net.DisableTemporarily(3f);
        Trigger("OnTriggerStay2D");
        Assert.That(events, Is.Zero);
        Assert.That(visual.IsPlayingSpecial, Is.False);
    }

    [Test]
    public void SpecialAdvancesWithScaledTimeAndReturnsToCurrentSwim()
    {
        Trigger("OnTriggerEnter2D");
        Assert.That(visual.SpecialFrame, Is.Zero);
        visual.Tick(0f, Vector2.left);
        Assert.That(visual.SpecialFrame, Is.Zero);
        visual.Tick(1f / 12f, Vector2.left);
        Assert.That(visual.SpecialFrame, Is.EqualTo(1));
        typeof(FishMovement).GetField("<LastMovementDirection>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(fish.GetComponent<FishMovement>(), Vector2.left);
        visual.Tick(.26f, Vector2.left);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(visual.SpecialHeading, Is.EqualTo(FishVisualHeading.East));
        SpriteRenderer renderer = fish.transform.Find("FishPixelVisual").GetComponent<SpriteRenderer>();
        Assert.That(renderer.sprite, Is.SameAs(fish.Data.VisualProfile.HorizontalFrames[visual.FrameIndex]));
        Assert.That(renderer.flipX && renderer.flipY, Is.True);
    }

    [Test]
    public void ImpactPausesReturnsToPoolAndRunResetClearsIt()
    {
        Trigger("OnTriggerEnter2D");
        object pool = Field(effects, "combatVfxPool");
        Invoke(pool, "Tick", 0f);
        Assert.That(CountActive("PufferfishNetImpactVisual"), Is.EqualTo(1));
        int created = effects.CreatedCombatVfxCount;
        Invoke(pool, "Tick", .28f);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        effects.ShowPufferfishNetImpact(Vector2.zero, profile);
        Assert.That(effects.CreatedCombatVfxCount, Is.EqualTo(created));
        Invoke(effects, "ClearRuntimeState", false);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
    }

    [Test]
    public void MissingProfileAndFullPoolCannotDelayGameplay()
    {
        SetField(net, "pufferfishPresentation", null);
        Trigger("OnTriggerEnter2D");
        Assert.That(net.IsOperational, Is.False);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);

        Invoke(net, "RefreshOperationalState");
        SetField(net, "specialDisabledUntil", Time.time);
        Invoke(net, "RefreshOperationalState");
        SetField(net, "pufferfishPresentation", profile);
        CombatVfxSettings settings = new CombatVfxSettings();
        SetField(settings, "maximumActiveVisuals", 1);
        Type poolType = typeof(ItemEffectManager).Assembly.GetType("CombatVfxPool");
        object full = Activator.CreateInstance(poolType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { effects.transform, settings }, null);
        Invoke(full, "AcquirePersistent", "ImportantVisual", Color.white, .1f, 2, 30);
        SetField(effects, "combatVfxPool", full);
        Trigger("OnTriggerEnter2D");
        Assert.That(net.IsOperational, Is.False);
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(1));
        Assert.That(CountActive("PufferfishNetImpactVisual"), Is.Zero);
    }

    [Test]
    public void SoundHasGlobalCooldownAndFishReuseClearsSpecial()
    {
        Assert.That(effects.PlayPufferfishNetSound(profile), Is.True);
        Assert.That(effects.PlayPufferfishNetSound(profile), Is.False);
        Trigger("OnTriggerEnter2D");
        Assert.That(visual.IsPlayingSpecial, Is.True);
        fish.gameObject.SetActive(false);
        FishData pufferData = fish.Data;
        FishData ordinaryData = Own(ScriptableObject.CreateInstance<FishData>());
        fish.Initialize(ordinaryData);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        fish.Initialize(pufferData);
        fish.gameObject.SetActive(true);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(visual.SpecialFrame, Is.Zero);
        Time.timeScale = 0f;
        Invoke(effects, "UpdateSquidInkAudioPause");
        Assert.That(Field(effects, "squidInkAudioPaused"), Is.EqualTo(true));
        Assert.That(effects.PlayPufferfishNetSound(profile), Is.False);
        Time.timeScale = 1f;
        Invoke(effects, "UpdateSquidInkAudioPause");
        Assert.That(Field(effects, "squidInkAudioPaused"), Is.EqualTo(false));
    }

    [Test]
    public void ImportedPresentationHasAllSpritesAndAudio()
    {
        Assert.That(profile.HasAnimation && profile.HasImpact, Is.True);
        Assert.That(profile.SoundClip, Is.Not.Null);
        Assert.That(profile.FramesPerSecond, Is.EqualTo(12f));
        Assert.That(profile.SoundVolume, Is.EqualTo(.34f));
        Assert.That(fish.Data.VisualProfile.IsValid, Is.True);
        Assert.That(profile.GetFrames(FishVisualSet.Horizontal)[0],
            Is.Not.SameAs(fish.Data.VisualProfile.HorizontalFrames[0]));
    }

    private void Trigger(string name) => Invoke(net, name, fishCollider);

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

    private static void SetField(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static void Invoke(object target, string name, params object[] args) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Invoke(target, args);
}
#endif
