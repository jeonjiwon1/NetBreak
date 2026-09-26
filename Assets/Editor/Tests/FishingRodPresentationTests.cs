#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class FishingRodPresentationTests
{
    private readonly List<UnityEngine.Object> owned = new();
    private PrototypeGameFlowManager flow;
    private ItemEffectManager effects;
    private FishingRodPresentationProfile profile;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        profile = AssetDatabase.LoadAssetAtPath<FishingRodPresentationProfile>(
            "Assets/Resources/FishingRodPresentation.asset");
        Assert.That(profile, Is.Not.Null);
        flow = Own(new GameObject("Rod Flow")).AddComponent<PrototypeGameFlowManager>();
        Invoke(flow, "Awake");
        flow.StartFishing();
        effects = Own(new GameObject("Rod VFX")).AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (effects != null && ItemEffectManager.Instance == effects)
            Invoke(effects, "OnDisable");
        if (flow != null && PrototypeGameFlowManager.Instance == flow)
            Invoke(flow, "OnDisable");
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
        owned.Clear();
    }

    [Test]
    public void RealAttackUsesChosenTargetAndExistingDamage()
    {
        FishingRodController rod = CreateRod(Vector2.zero);
        FishController near = CreateFish(new Vector2(.4f, 0f), 30f, 1);
        FishController valuable = CreateFish(new Vector2(1f, 0f), 30f, 9);
        FishController outside = CreateFish(new Vector2(1.8f, 0f), 30f, 99);
        Invoke(rod, "Attack");

        Assert.That(valuable.CurrentResistance, Is.EqualTo(26f));
        Assert.That(near.CurrentResistance, Is.EqualTo(30f));
        Assert.That(outside.CurrentResistance, Is.EqualTo(30f));
        FishingRodPresentation visual = rod.GetComponent<FishingRodPresentation>();
        Assert.That(visual.IsAttacking && visual.IsLineVisible, Is.True);
        Assert.That(rod.GetComponent<LineRenderer>().GetPosition(1),
            Is.EqualTo(valuable.transform.position));
        Assert.That(CountActive("FishingRodHitVisual"), Is.EqualTo(1));
        Assert.That((float)Field(effects, "lastFishingRodSoundTime"),
            Is.EqualTo(Time.time));
    }

    [Test]
    public void NoTargetOrMissingProfileDoesNotChangeGameplay()
    {
        FishingRodController rod = CreateRod(Vector2.zero);
        Invoke(rod, "Attack");
        Assert.That(rod.GetComponent<FishingRodPresentation>().IsAttacking, Is.False);
        Assert.That(CountActive("FishingRodHitVisual"), Is.Zero);
        FishController fish = CreateFish(new Vector2(.5f, 0f), 30f, 1);
        SetField(rod.GetComponent<FishingRodPresentation>(), "profile", null);
        Invoke(rod, "Attack");
        Assert.That(fish.CurrentResistance, Is.EqualTo(26f));
        Assert.That(CountActive("FishingRodHitVisual"), Is.Zero);
    }

    [Test]
    public void MultipleTargetsKeepOneLineAndIndependentHitPositions()
    {
        FishingRodController rod = CreateRod(Vector2.zero);
        rod.AddAdditionalTarget(1);
        FishController first = CreateFish(new Vector2(.4f, 0f), 30f, 9);
        FishController second = CreateFish(new Vector2(-.4f, 0f), 30f, 1);
        Invoke(rod, "Attack");
        Assert.That(first.CurrentResistance, Is.EqualTo(26f));
        Assert.That(second.CurrentResistance, Is.EqualTo(26f));
        Assert.That(CountActive("FishingRodHitVisual"), Is.EqualTo(2));
        Assert.That(rod.GetComponent<LineRenderer>().GetPosition(1),
            Is.EqualTo(first.transform.position));
    }

    [Test]
    public void PoolFullNeverBlocksDamage()
    {
        CombatVfxSettings settings = new();
        SetField(settings, "maximumActiveVisuals", 1);
        Type poolType = typeof(ItemEffectManager).Assembly.GetType("CombatVfxPool");
        object pool = Activator.CreateInstance(poolType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new object[] { effects.transform, settings }, null);
        Invoke(pool, "AcquirePersistent", "Important", Color.white, .1f, 2, 30);
        SetField(effects, "combatVfxPool", pool);
        FishingRodController rod = CreateRod(Vector2.zero);
        FishController fish = CreateFish(new Vector2(.4f, 0f), 30f, 1);
        Invoke(rod, "Attack");
        Assert.That(fish.CurrentResistance, Is.EqualTo(26f));
        Assert.That(CountActive("FishingRodHitVisual"), Is.Zero);
    }

    [Test]
    public void PauseAndDisableClearOnlyThisRodsVisualState()
    {
        FishingRodController first = CreateRod(Vector2.zero);
        FishingRodController second = CreateRod(new Vector2(4f, 0f));
        CreateFish(new Vector2(.4f, 0f), 30f, 1);
        CreateFish(new Vector2(4.4f, 0f), 30f, 1);
        Invoke(first, "Attack");
        Invoke(second, "Attack");
        FishingRodPresentation firstVisual = first.GetComponent<FishingRodPresentation>();
        FishingRodPresentation secondVisual = second.GetComponent<FishingRodPresentation>();
        Assert.That(firstVisual.IsLineVisible && secondVisual.IsLineVisible, Is.True);
        Time.timeScale = 0f;
        Invoke(firstVisual, "Update");
        Assert.That(firstVisual.IsLineVisible, Is.True);
        first.BeginReposition();
        Assert.That(firstVisual.IsAttacking || firstVisual.IsLineVisible, Is.False);
        Assert.That(secondVisual.IsLineVisible, Is.True);
        second.gameObject.SetActive(false);
        Invoke(secondVisual, "OnDisable");
        Assert.That(secondVisual.IsLineVisible, Is.False);
    }

    [Test]
    public void PoolReturnsHitAndAudioUsesGlobalDuplicateLimit()
    {
        FishingRodController rod = CreateRod(Vector2.zero);
        CreateFish(new Vector2(.4f, 0f), 30f, 1);
        Invoke(rod, "Attack");
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(1));
        Assert.That(effects.PlayFishingRodHitSound(profile), Is.False);
        object pool = Field(effects, "combatVfxPool");
        Invoke(pool, "Tick", .25f);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        Time.timeScale = 0f;
        Assert.That(effects.PlayFishingRodHitSound(profile), Is.False);
    }

    [Test]
    public void ImportedAssetsAndPrefabReferencesAreComplete()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Gear/FishingRod.prefab");
        Assert.That(prefab, Is.Not.Null);
        FishingRodController rod = CreateRod(Vector2.zero);
        Assert.That(rod.GetComponent<FishingRodPresentation>().Profile,
            Is.SameAs(profile));
        Assert.That(rod.GetComponent<SpriteRenderer>().sprite,
            Is.SameAs(profile.IdleSprite));
        Assert.That(profile.HasAttack && profile.HasHit, Is.True);
        Assert.That(profile.AttackFrames.Length, Is.EqualTo(3));
        Assert.That(profile.HitFrames.Length, Is.EqualTo(4));
        Assert.That(profile.HitClip, Is.Not.Null);
    }

    private FishingRodController CreateRod(Vector2 position)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Gear/FishingRod.prefab");
        GameObject obj = Own(UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity));
        FishingRodController rod = obj.GetComponent<FishingRodController>();
        Invoke(rod, "Awake");
        Invoke(obj.GetComponent<FishingRodPresentation>(), "Awake");
        Physics2D.SyncTransforms();
        return rod;
    }

    private FishController CreateFish(Vector2 position, float resistance, int value)
    {
        GameObject obj = Own(new GameObject("Rod Fish"));
        obj.transform.position = position;
        obj.AddComponent<CircleCollider2D>().radius = .15f;
        FishController fish = obj.AddComponent<FishController>();
        Invoke(fish, "Awake");
        FishData data = Own(ScriptableObject.CreateInstance<FishData>());
        SetField(data, "maxResistance", resistance);
        SetField(data, "catchValue", value);
        fish.Initialize(data);
        Physics2D.SyncTransforms();
        return fish;
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

    private static void SetField(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static object Invoke(object target, string name, params object[] args) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Invoke(target, args);
}
#endif
