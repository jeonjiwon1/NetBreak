#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

public sealed class FishingRodInterferenceVisualTests
{
    private const string FontPath =
        "Assets/UI/Fonts/NanumGothic-Bold SDF.asset";

    private readonly List<UnityEngine.Object> ownedObjects = new();

    private PrototypeGameFlowManager flow;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;

        Assert.That(
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath),
            Is.Not.Null,
            "The existing Korean TMP font must be available for the world status label.");

        flow = CreateFlow("Fishing Rod Visual Test Flow");
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;

        for (int i = ownedObjects.Count - 1; i >= 0; i--)
        {
            if (ownedObjects[i] != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    ownedObjects[i]
                );
            }
        }

        ownedObjects.Clear();
    }

    [Test]
    public void InkInterferenceShowsOneKoreanStatusAndDarkensRod()
    {
        Color originalColor =
            new(0.4f, 0.7f, 0.3f, 0.85f);

        FishingRodController rod =
            CreateRod(Vector2.zero, originalColor);

        rod.DisableTemporarily(2.5f);

        TextMeshPro status = GetStatusText(rod);
        SpriteRenderer renderer = rod.GetComponent<SpriteRenderer>();

        Assert.That(rod.IsInkInterferenceActive, Is.True);
        Assert.That(rod.IsInkInterferenceVisible, Is.True);
        Assert.That(status.gameObject.activeSelf, Is.True);
        Assert.That(status.text, Is.EqualTo("먹물 방해"));
        Assert.That(status.font.name, Is.EqualTo("NanumGothic-Bold SDF"));
        Assert.That(
            rod.transform.Find("InkInterferenceStatus"),
            Is.EqualTo(status.transform),
            "The status object must be reused instead of duplicated.");
        Assert.That(renderer.color, Is.Not.EqualTo(originalColor));
    }

    [Test]
    public void ExpiredInterferenceHidesStatusAndRestoresOriginalColor()
    {
        Color originalColor =
            new(0.35f, 0.55f, 0.8f, 0.7f);

        FishingRodController rod =
            CreateRod(Vector2.zero, originalColor);

        rod.DisableTemporarily(2.5f);
        ForceInterferenceExpired(rod);

        Assert.That(rod.IsInkInterferenceActive, Is.False);
        Assert.That(rod.IsInkInterferenceVisible, Is.False);
        Assert.That(rod.IsOperational, Is.True);
        Assert.That(
            rod.GetComponent<SpriteRenderer>().color,
            Is.EqualTo(originalColor));
    }

    [Test]
    public void AttackStopsExactlyWhileInterferenceStatusIsVisible()
    {
        FishingRodController rod =
            CreateRod(Vector2.zero, Color.white);

        FishController target =
            CreateFish(new Vector2(0.25f, 0f), 100f, 100);

        float initialResistance =
            target.CurrentResistance;

        InvokePrivate(rod, "Update");

        Assert.That(
            target.CurrentResistance,
            Is.LessThan(initialResistance));

        target.Initialize(target.Data);
        rod.DisableTemporarily(2.5f);

        float disabledResistance =
            target.CurrentResistance;

        InvokePrivate(rod, "Update");

        Assert.That(rod.IsInkInterferenceVisible, Is.True);
        Assert.That(rod.IsOperational, Is.False);
        Assert.That(
            target.CurrentResistance,
            Is.EqualTo(disabledResistance));

        ForceInterferenceExpired(rod);

        Assert.That(rod.IsInkInterferenceVisible, Is.False);
        Assert.That(
            target.CurrentResistance,
            Is.LessThan(disabledResistance));
    }

    [Test]
    public void MultipleSquidsExtendOneStatusUntilLatestDeadline()
    {
        FishingRodController rod =
            CreateRod(Vector2.zero, Color.white);

        SquidController shortSquid =
            CreateSquid(Vector2.zero, 2.5f, 1f);

        SquidController longSquid =
            CreateSquid(Vector2.zero, 2.5f, 3f);

        ReleaseInk(shortSquid);
        float firstDeadline =
            GetPrivateField<float>(rod, "specialDisabledUntil");

        ReleaseInk(longSquid);
        float extendedDeadline =
            GetPrivateField<float>(rod, "specialDisabledUntil");

        ReleaseInk(shortSquid);

        Assert.That(
            extendedDeadline,
            Is.GreaterThan(firstDeadline + 1f));
        Assert.That(
            GetPrivateField<float>(rod, "specialDisabledUntil"),
            Is.EqualTo(extendedDeadline));
        Assert.That(rod.IsInkInterferenceVisible, Is.True);
        Assert.That(
            rod.GetComponentsInChildren<TextMeshPro>(true).Length,
            Is.EqualTo(1));

        UnityEngine.Object.DestroyImmediate(shortSquid.gameObject);
        UnityEngine.Object.DestroyImmediate(longSquid.gameObject);

        Assert.That(
            rod.IsInkInterferenceVisible,
            Is.True,
            "Capturing or pooling a squid must not shorten an already applied disable.");

        ForceInterferenceExpired(rod);
        Assert.That(rod.IsInkInterferenceVisible, Is.False);
    }

    [Test]
    public void UnaffectedRodKeepsNormalVisualAndOperation()
    {
        Color unaffectedColor =
            new(0.8f, 0.45f, 0.2f, 1f);

        FishingRodController nearbyRod =
            CreateRod(Vector2.zero, Color.white);

        FishingRodController distantRod =
            CreateRod(new Vector2(10f, 0f), unaffectedColor);

        SquidController squid =
            CreateSquid(Vector2.zero, 2.5f, 2.5f);

        ReleaseInk(squid);

        Assert.That(nearbyRod.IsInkInterferenceVisible, Is.True);
        Assert.That(distantRod.IsInkInterferenceVisible, Is.False);
        Assert.That(distantRod.IsOperational, Is.True);
        Assert.That(
            distantRod.GetComponent<SpriteRenderer>().color,
            Is.EqualTo(unaffectedColor));
    }

    [Test]
    public void DisablingRodCleansStatusAndRestoresColor()
    {
        Color originalColor =
            new(0.25f, 0.65f, 0.9f, 0.8f);

        FishingRodController rod =
            CreateRod(Vector2.zero, originalColor);

        TextMeshPro status = GetStatusText(rod);

        rod.DisableTemporarily(2.5f);
        Assert.That(status.gameObject.activeSelf, Is.True);

        rod.gameObject.SetActive(false);

        // Plain EditMode objects do not run the normal player lifecycle.
        InvokePrivate(rod, "OnDisable");

        Assert.That(status.gameObject.activeSelf, Is.False);
        Assert.That(
            rod.GetComponent<SpriteRenderer>().color,
            Is.EqualTo(originalColor));
    }

    [Test]
    public void RunEndCleansOldStatusAndNewRunRodStartsClean()
    {
        Color originalColor =
            new(0.7f, 0.6f, 0.2f, 1f);

        FishingRodController oldRod =
            CreateRod(Vector2.zero, originalColor);

        oldRod.DisableTemporarily(2.5f);
        Assert.That(oldRod.IsInkInterferenceVisible, Is.True);

        flow.FailMiniBossEncounter();
        InvokePrivate(oldRod, "Update");

        Assert.That(oldRod.IsInkInterferenceVisible, Is.False);
        Assert.That(
            oldRod.GetComponent<SpriteRenderer>().color,
            Is.EqualTo(originalColor));

        InvokePrivate(flow, "OnDisable");
        UnityEngine.Object.DestroyImmediate(flow.gameObject);
        Time.timeScale = 1f;

        flow = CreateFlow("New Run Flow");

        FishingRodController newRod =
            CreateRod(Vector2.zero, originalColor);

        Assert.That(newRod.IsInkInterferenceActive, Is.False);
        Assert.That(newRod.IsInkInterferenceVisible, Is.False);
        Assert.That(newRod.IsOperational, Is.True);
        Assert.That(
            newRod.GetComponent<SpriteRenderer>().color,
            Is.EqualTo(originalColor));
    }

    [Test]
    public void RapidReelingDoesNotBypassOrClearInterferenceVisual()
    {
        FishingRodController rod =
            CreateRod(Vector2.zero, Color.white);

        rod.SetTacticalAttackSpeedMultiplier(2f);
        rod.DisableTemporarily(2.5f);

        Assert.That(rod.IsOperational, Is.False);
        Assert.That(rod.IsInkInterferenceVisible, Is.True);
        Assert.That(
            GetPrivateField<float>(
                rod,
                "tacticalAttackSpeedMultiplier"),
            Is.EqualTo(2f));

        ForceInterferenceExpired(rod);

        Assert.That(rod.IsOperational, Is.True);
        Assert.That(rod.IsInkInterferenceVisible, Is.False);
        Assert.That(
            GetPrivateField<float>(
                rod,
                "tacticalAttackSpeedMultiplier"),
            Is.EqualTo(2f));
    }

    [Test]
    public void RepositionAndExternalColorRemainIndependentFromInkStatus()
    {
        Color initialColor =
            new(0.45f, 0.7f, 0.35f, 0.9f);

        Color externalColor =
            new(0.8f, 0.3f, 0.55f, 0.75f);

        FishingRodController rod =
            CreateRod(Vector2.zero, initialColor);

        SpriteRenderer renderer =
            rod.GetComponent<SpriteRenderer>();

        renderer.color = externalColor;
        InvokePrivate(rod, "Update");

        rod.BeginReposition();

        Assert.That(rod.IsOperational, Is.False);
        Assert.That(
            rod.IsInkInterferenceVisible,
            Is.False,
            "Repositioning is a different block and must not show the ink label.");

        rod.EndReposition(0f);

        Assert.That(rod.IsOperational, Is.True);
        Assert.That(renderer.color, Is.EqualTo(externalColor));

        rod.DisableTemporarily(2.5f);
        Assert.That(rod.IsInkInterferenceVisible, Is.True);

        ForceInterferenceExpired(rod);

        Assert.That(renderer.color, Is.EqualTo(externalColor));
    }

    private PrototypeGameFlowManager CreateFlow(
        string objectName)
    {
        GameObject flowObject =
            Own(new GameObject(objectName));

        PrototypeGameFlowManager result =
            flowObject.AddComponent<
                PrototypeGameFlowManager
            >();

        InvokePrivate(result, "Awake");
        result.StartFishing();
        return result;
    }

    private FishingRodController CreateRod(
        Vector2 position,
        Color color)
    {
        GameObject rodObject =
            Own(new GameObject("Test Fishing Rod"));

        rodObject.transform.position = position;

        SpriteRenderer renderer =
            rodObject.AddComponent<SpriteRenderer>();

        renderer.color = color;

        BoxCollider2D collider =
            rodObject.AddComponent<BoxCollider2D>();

        collider.isTrigger = true;
        collider.size = Vector2.one;

        FishingRodController rod =
            rodObject.AddComponent<
                FishingRodController
            >();

        InvokePrivate(rod, "Awake");
        Physics2D.SyncTransforms();
        return rod;
    }

    private FishController CreateFish(
        Vector2 position,
        float resistance,
        int catchValue)
    {
        GameObject fishObject =
            Own(new GameObject("Rod Target Fish"));

        fishObject.transform.position = position;
        fishObject.AddComponent<CircleCollider2D>().radius = 0.25f;

        FishController fish =
            fishObject.AddComponent<FishController>();

        InvokePrivate(fish, "Awake");

        fish.Initialize(
            CreateFishData(
                FishSpecialType.None,
                resistance,
                catchValue,
                0f,
                0f)
        );

        Physics2D.SyncTransforms();
        return fish;
    }

    private SquidController CreateSquid(
        Vector2 position,
        float inkRange,
        float disableDuration)
    {
        GameObject squidObject =
            Own(new GameObject("Test Squid"));

        squidObject.transform.position = position;

        FishController fish =
            squidObject.AddComponent<FishController>();

        SquidController squid =
            squidObject.AddComponent<SquidController>();

        InvokePrivate(fish, "Awake");
        InvokePrivate(squid, "Awake");

        fish.Initialize(
            CreateFishData(
                FishSpecialType.Squid,
                24f,
                0,
                inkRange,
                disableDuration)
        );

        InvokePrivate(squid, "OnEnable");
        Physics2D.SyncTransforms();
        return squid;
    }

    private FishData CreateFishData(
        FishSpecialType specialType,
        float resistance,
        int catchValue,
        float inkRange,
        float disableDuration)
    {
        FishData data =
            Own(ScriptableObject.CreateInstance<FishData>());

        SetPrivateField(data, "specialType", specialType);
        SetPrivateField(data, "maxResistance", resistance);
        SetPrivateField(data, "catchValue", catchValue);
        SetPrivateField(data, "inkRange", inkRange);
        SetPrivateField(
            data,
            "inkDisableDuration",
            disableDuration);

        return data;
    }

    private static TextMeshPro GetStatusText(
        FishingRodController rod)
    {
        Transform status =
            rod.transform.Find(
                "InkInterferenceStatus"
            );

        Assert.That(status, Is.Not.Null);

        TextMeshPro text =
            status.GetComponent<TextMeshPro>();

        Assert.That(text, Is.Not.Null);
        return text;
    }

    private static void ReleaseInk(
        SquidController squid)
    {
        Physics2D.SyncTransforms();
        InvokePrivate(squid, "ReleaseInk");
    }

    private static void ForceInterferenceExpired(
        FishingRodController rod)
    {
        SetPrivateField(
            rod,
            "specialDisabledUntil",
            Time.time - 0.01f);

        InvokePrivate(rod, "Update");
    }

    private T Own<T>(T value)
        where T : UnityEngine.Object
    {
        ownedObjects.Add(value);
        return value;
    }

    private static void InvokePrivate(
        object target,
        string methodName)
    {
        MethodInfo method =
            target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.That(method, Is.Not.Null, methodName);
        method.Invoke(target, null);
    }

    private static T GetPrivateField<T>(
        object target,
        string fieldName)
    {
        FieldInfo field =
            target.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.That(field, Is.Not.Null, fieldName);
        return (T)field.GetValue(target);
    }

    private static void SetPrivateField<T>(
        object target,
        string fieldName,
        T value)
    {
        FieldInfo field =
            target.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic
            );

        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }
}
#endif
