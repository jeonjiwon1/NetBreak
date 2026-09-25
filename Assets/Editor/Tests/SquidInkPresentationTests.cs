#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class SquidInkPresentationTests
{
    private readonly List<UnityEngine.Object> owned = new();
    private FishVisualProfile swim;
    private SquidInkPresentationProfile ink;
    private FishController fish;
    private FishVisualController visual;
    private SquidController squid;
    private FishingRodController rod;
    private ItemEffectManager effects;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
        Texture2D pixels = Own(new Texture2D(28, 1, TextureFormat.RGBA32, false));
        pixels.SetPixels(new Color[28]);
        pixels.Apply();
        Sprite[] sprites = new Sprite[28];
        for (int i = 0; i < sprites.Length; i++)
            sprites[i] = Own(Sprite.Create(pixels, new Rect(i, 0, 1, 1), new Vector2(.5f, .5f)));

        swim = Own(ScriptableObject.CreateInstance<FishVisualProfile>());
        ink = Own(ScriptableObject.CreateInstance<SquidInkPresentationProfile>());
        string[] directions = { "horizontalFrames", "verticalFrames", "diagonalFrames",
                                "diagonalNorthWestFrames" };
        Fill(swim, directions, sprites);
        Fill(ink, directions, sprites);
        SerializedObject inkData = new SerializedObject(ink);
        SerializedProperty puff = inkData.FindProperty("puffFrames");
        for (int i = 0; i < 4; i++)
        {
            puff.GetArrayElementAtIndex(i).objectReferenceValue = sprites[16 + i];
            inkData.FindProperty("projectileFrames").GetArrayElementAtIndex(i)
                .objectReferenceValue = sprites[20 + i];
            inkData.FindProperty("impactFrames").GetArrayElementAtIndex(i)
                .objectReferenceValue = sprites[24 + i];
        }
        inkData.ApplyModifiedPropertiesWithoutUndo();

        GameObject managerObject = Own(new GameObject("Ink Test Effects"));
        effects = managerObject.AddComponent<ItemEffectManager>();
        Invoke(effects, "Awake");

        GameObject fishObject = Own(new GameObject("Ink Test Squid"));
        fishObject.AddComponent<SpriteRenderer>();
        fishObject.AddComponent<CircleCollider2D>().radius = .5f;
        fish = fishObject.AddComponent<FishController>();
        fishObject.AddComponent<FishMovement>();
        squid = fishObject.AddComponent<SquidController>();
        Invoke(fish, "Awake");
        Invoke(squid, "Awake");
        visual = fishObject.GetComponent<FishVisualController>();
        SetField(squid, "presentationProfile", ink);
        FishData data = Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject fishData = new SerializedObject(data);
        fishData.FindProperty("specialType").enumValueIndex = (int)FishSpecialType.Squid;
        fishData.FindProperty("visualProfile").objectReferenceValue = swim;
        fishData.FindProperty("inkRange").floatValue = 2.5f;
        fishData.FindProperty("inkInterval").floatValue = 5f;
        fishData.FindProperty("firstInkDelay").floatValue = 2f;
        fishData.FindProperty("inkDisableDuration").floatValue = 2.5f;
        fishData.ApplyModifiedPropertiesWithoutUndo();
        fish.Initialize(data);

        GameObject rodObject = Own(new GameObject("Ink Test Rod"));
        rodObject.transform.position = Vector2.right;
        rodObject.AddComponent<SpriteRenderer>();
        BoxCollider2D rodCollider = rodObject.AddComponent<BoxCollider2D>();
        rodCollider.size = Vector2.one;
        rodCollider.isTrigger = true;
        rod = rodObject.AddComponent<FishingRodController>();
        Invoke(rod, "Awake");
        Physics2D.SyncTransforms();
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
        if (effects != null) Invoke(effects, "OnDisable");
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
        owned.Clear();
    }

    [Test]
    public void SuccessfulGameplayImmediatelyDisablesRodAndTriggersOnePresentation()
    {
        int events = 0;
        squid.InkPresentationTriggered += _ => events++;
        Release();
        BoxCollider2D probe = rod.GetComponent<BoxCollider2D>();
        Assert.That(events, Is.EqualTo(1),
            $"range={fish.Data.InkRange}, hits={Physics2D.OverlapCircleAll(fish.transform.position, fish.Data.InkRange).Length}, rod={rod.transform.position}, enabled={probe.enabled}, active={probe.gameObject.activeInHierarchy}, bounds={probe.bounds}, queryTriggers={Physics2D.queriesHitTriggers}");
        Assert.That(rod.IsInkInterferenceActive, Is.True);
        Assert.That(visual.IsPlayingSpecial, Is.True);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
    }

    [Test]
    public void NoAffectedTargetDoesNotStartAttackOrSound()
    {
        rod.transform.position = new Vector2(10f, 0f);
        Physics2D.SyncTransforms();
        int events = 0;
        squid.InkPresentationTriggered += _ => events++;
        Release();
        Assert.That(events, Is.Zero);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
    }

    [Test]
    public void SpecialLocksDirectionReleasesOnceAtFrameTwoAndReturnsToSwim()
    {
        AudioClip clip = Own(AudioClip.Create("ink release event", 4410, 1, 44100, false));
        SerializedObject sound = new SerializedObject(ink);
        sound.FindProperty("inkClip").objectReferenceValue = clip;
        sound.ApplyModifiedPropertiesWithoutUndo();
        typeof(FishMovement).GetField("<LastMovementDirection>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(fish.GetComponent<FishMovement>(), Vector2.left);
        visual.Tick(0f, Vector2.left);
        Release();
        Assert.That(visual.SpecialHeading, Is.EqualTo(FishVisualHeading.West));
        typeof(FishMovement).GetField("<LastMovementDirection>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(fish.GetComponent<FishMovement>(), Vector2.right);
        visual.Tick(.125f, Vector2.right);
        Assert.That(visual.SpecialFrame, Is.EqualTo(1));
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastSquidInkSoundTime")), Is.True);
        visual.Tick(.125f, Vector2.right);
        Assert.That(visual.SpecialFrame, Is.EqualTo(2));
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(2));
        Assert.That(CountActive("SquidInkBurstVisual"), Is.EqualTo(1));
        Assert.That(CountActive("SquidInkProjectileVisual"), Is.EqualTo(1));
        Assert.That(CountActive("SquidInkImpactVisual"), Is.Zero);
        Assert.That(CountActive("SquidInkTrajectoryVisual"), Is.Zero);
        SpriteRenderer renderer = fish.transform.Find("FishPixelVisual").GetComponent<SpriteRenderer>();
        Assert.That(renderer.sprite, Is.SameAs(ink.GetFrames(FishVisualSet.Horizontal)[2]));
        Assert.That(renderer.flipX && renderer.flipY, Is.True);
        Assert.That(float.IsNegativeInfinity((float)Field(effects, "lastSquidInkSoundTime")), Is.False);
        visual.Tick(.125f, Vector2.right);
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(2));
        visual.Tick(.125f, Vector2.right);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(visual.SpecialHeading, Is.EqualTo(FishVisualHeading.East));
        Assert.That(renderer.sprite, Is.SameAs(swim.HorizontalFrames[visual.FrameIndex]));
        Assert.That(renderer.flipX || renderer.flipY, Is.False);
    }

    [Test]
    public void PauseHoldsSpecialAndPoolThenReuseClearsState()
    {
        Release();
        visual.Tick(0f, Vector2.right);
        Assert.That(visual.SpecialFrame, Is.Zero);
        visual.Tick(.25f, Vector2.right);
        object pool = Field(effects, "combatVfxPool");
        Invoke(pool, "Tick", 0f);
        Assert.That(effects.ActiveCombatVfxCount, Is.EqualTo(2));
        Invoke(pool, "Tick", 1f);
        Assert.That(CountActive("SquidInkImpactVisual"), Is.EqualTo(1));
        Invoke(pool, "Tick", 1f);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        int created = effects.CreatedCombatVfxCount;
        effects.ShowSquidInkBurst(Vector2.zero, ink);
        Assert.That(effects.CreatedCombatVfxCount, Is.EqualTo(created));
        Invoke(pool, "Tick", 1f);
        fish.gameObject.SetActive(false);
        fish.Initialize(fish.Data);
        fish.gameObject.SetActive(true);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(visual.SpecialFrame, Is.Zero);
        Assert.That(visual.SpecialHeading, Is.EqualTo(FishVisualHeading.East));
    }

    [Test]
    public void ProjectileUsesSuccessfulTargetSnapshotThenSpawnsOneImpact()
    {
        Release();
        visual.Tick(.25f, Vector2.right);
        Assert.That(rod.IsInkInterferenceActive, Is.True);
        Transform projectile = FindActive("SquidInkProjectileVisual");
        Assert.That(projectile, Is.Not.Null);
        Assert.That((Vector2)projectile.position, Is.EqualTo(Vector2.zero));

        rod.transform.position = new Vector2(8f, 0f);
        rod.gameObject.SetActive(false);
        object pool = Field(effects, "combatVfxPool");
        Invoke(pool, "Tick", .11f);
        Assert.That(projectile.gameObject.activeSelf, Is.True);
        Assert.That(projectile.position.x, Is.EqualTo(.5f).Within(.001f));
        Assert.That(CountActive("SquidInkImpactVisual"), Is.Zero);

        Invoke(pool, "Tick", .11f);
        Assert.That(CountActive("SquidInkProjectileVisual"), Is.Zero);
        Assert.That(CountActive("SquidInkImpactVisual"), Is.EqualTo(1));
        Assert.That((Vector2)FindActive("SquidInkImpactVisual").position,
            Is.EqualTo(Vector2.right));
        Invoke(pool, "Tick", .3f);
        Assert.That(CountActive("SquidInkImpactVisual"), Is.Zero);
    }

    [Test]
    public void PauseAndRunResetRemoveProjectileWithoutDelayedImpact()
    {
        Release();
        visual.Tick(.25f, Vector2.right);
        object pool = Field(effects, "combatVfxPool");
        Invoke(pool, "Tick", 0f);
        Assert.That((Vector2)FindActive("SquidInkProjectileVisual").position,
            Is.EqualTo(Vector2.zero));
        Invoke(effects, "ClearRuntimeState", false);
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
        Invoke(pool, "Tick", 1f);
        Assert.That(CountActive("SquidInkImpactVisual"), Is.Zero);

        effects.ShowSquidInkAttack(Vector2.zero, Vector2.right, ink);
        Assert.That(CountActive("SquidInkProjectileVisual"), Is.EqualTo(1));
        Invoke(pool, "Tick", .22f);
        Assert.That(CountActive("SquidInkImpactVisual"), Is.EqualTo(1));
        Invoke(pool, "Clear");
        Assert.That(effects.ActiveCombatVfxCount, Is.Zero);
    }

    [Test]
    public void MissingPresentationProfileLeavesGameplayWorking()
    {
        SetField(squid, "presentationProfile", null);
        Assert.DoesNotThrow(Release);
        Assert.That(rod.IsInkInterferenceActive, Is.True);
        Assert.That(visual.IsPlayingSpecial, Is.False);
        Assert.That(CountActive("SquidInkBurstVisual"), Is.Zero);
    }

    [Test]
    public void OneShotSoundUsesGlobalCooldownAndMissingClipIsSafe()
    {
        AudioClip clip = Own(AudioClip.Create("ink test", 4410, 1, 44100, false));
        SerializedObject serialized = new SerializedObject(ink);
        serialized.FindProperty("inkClip").objectReferenceValue = clip;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Assert.That(effects.PlaySquidInkSound(ink), Is.True);
        Assert.That(effects.PlaySquidInkSound(ink), Is.False);
        Time.timeScale = 0f;
        Invoke(effects, "UpdateSquidInkAudioPause");
        Assert.That(Field(effects, "squidInkAudioPaused"), Is.EqualTo(true));
        Assert.That(effects.PlaySquidInkSound(ink), Is.False);
        Time.timeScale = 1f;
        Invoke(effects, "UpdateSquidInkAudioPause");
        Assert.That(Field(effects, "squidInkAudioPaused"), Is.EqualTo(false));
        serialized.Update();
        serialized.FindProperty("inkClip").objectReferenceValue = null;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Assert.That(effects.PlaySquidInkSound(ink), Is.False);
    }

    [Test]
    public void ImportedProfileLinksActionPuffAndSoundWithoutChangingSwim()
    {
        SquidInkPresentationProfile imported = AssetDatabase.LoadAssetAtPath<SquidInkPresentationProfile>(
            "Assets/Resources/SquidInkPresentation.asset");
        FishVisualProfile swimAsset = AssetDatabase.LoadAssetAtPath<FishVisualProfile>(
            "Assets/Art/Fish/Squid/Squid_VisualProfile.asset");
        Assert.That(imported, Is.Not.Null);
        Assert.That(imported.HasAnimation && imported.HasPuff &&
                    imported.HasProjectile && imported.HasImpact, Is.True);
        Assert.That(imported.InkClip, Is.Not.Null);
        Assert.That(imported.FramesPerSecond, Is.EqualTo(8f));
        Assert.That(swimAsset.IsValid, Is.True);
        Assert.That(swimAsset.HorizontalFrames[0], Is.Not.SameAs(imported.GetFrames(FishVisualSet.Horizontal)[0]));
    }

    private void Release()
    {
        Physics2D.SyncTransforms();
        Invoke(squid, "ReleaseInk");
    }

    private int CountActive(string name)
    {
        int count = 0;
        foreach (Transform child in effects.transform)
            if (child.gameObject.activeSelf && child.name == name) count++;
        return count;
    }

    private Transform FindActive(string name)
    {
        foreach (Transform child in effects.transform)
            if (child.gameObject.activeSelf && child.name == name) return child;
        return null;
    }

    private static void Fill(UnityEngine.Object target, string[] names, Sprite[] sprites)
    {
        SerializedObject serialized = new SerializedObject(target);
        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
            serialized.FindProperty(names[row]).GetArrayElementAtIndex(col).objectReferenceValue =
                sprites[row * 4 + col];
        serialized.ApplyModifiedPropertiesWithoutUndo();
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
