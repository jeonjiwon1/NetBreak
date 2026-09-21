#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class CombatVfxTests
{
    private readonly List<UnityEngine.Object> ownedObjects = new();

    private PrototypeGameFlowManager flow;
    private ItemEffectManager manager;
    private Camera testCamera;

    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;

        GameObject flowObject =
            Own(new GameObject("Combat VFX Test Flow"));

        flow = flowObject.AddComponent<PrototypeGameFlowManager>();
        InvokePrivate(flow, "Awake");
        flow.StartFishing();

        GameObject managerObject =
            Own(new GameObject("Combat VFX Test Manager"));

        manager = managerObject.AddComponent<ItemEffectManager>();
        InvokePrivate(manager, "Awake");
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;

        if (manager != null &&
            ItemEffectManager.Instance == manager)
        {
            InvokePrivate(manager, "OnDisable");
        }

        if (flow != null)
        {
            InvokePrivate(flow, "OnDisable");
        }

        if (testCamera != null)
        {
            testCamera.targetTexture = null;
        }

        for (int i = ownedObjects.Count - 1; i >= 0; i--)
        {
            if (ownedObjects[i] != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    ownedObjects[i]);
            }
        }

        ownedObjects.Clear();
    }

    [Test]
    public void SquidInkVfxUsesActualRodTargetsAndDeduplicatesColliders()
    {
        FishingRodController rod = CreateRod(Vector2.right);

        GameObject childCollider =
            Own(new GameObject("Duplicate Rod Collider"));
        childCollider.transform.SetParent(rod.transform, false);
        childCollider.AddComponent<BoxCollider2D>();

        SquidController first =
            CreateSquid(Vector2.zero, 2.5f, 1f);
        SquidController second =
            CreateSquid(Vector2.zero, 2.5f, 4f);

        ReleaseInk(first);

        Assert.That(manager.ActiveCombatVfxCount, Is.EqualTo(2));
        Assert.That(CountActiveLines("SquidInkTrajectoryVisual"), Is.EqualTo(1));
        Assert.That(CountActiveLines("SquidInkImpactVisual"), Is.EqualTo(1));

        LineRenderer trajectory =
            FindActiveLine("SquidInkTrajectoryVisual");
        Assert.That((Vector2)trajectory.GetPosition(0), Is.EqualTo(Vector2.zero));
        Assert.That((Vector2)trajectory.GetPosition(1), Is.EqualTo(Vector2.right));

        float firstDeadline =
            GetPrivateField<float>(rod, "specialDisabledUntil");

        ReleaseInk(second);

        Assert.That(
            GetPrivateField<float>(rod, "specialDisabledUntil"),
            Is.GreaterThan(firstDeadline + 2f));
        Assert.That(manager.ActiveCombatVfxCount, Is.EqualTo(4));
        Assert.That(
            rod.GetComponentsInChildren<TMPro.TextMeshPro>(true).Length,
            Is.EqualTo(1));
    }

    [Test]
    public void SquidInkWithoutRodTargetCreatesNoFakeVfx()
    {
        CreateRod(new Vector2(10f, 0f));
        SquidController squid =
            CreateSquid(Vector2.zero, 2.5f, 2.5f);

        ReleaseInk(squid);

        Assert.That(manager.ActiveCombatVfxCount, Is.Zero);
        Assert.That(CountActiveLines("SquidInkImpactVisual"), Is.Zero);
    }

    [Test]
    public void ElectricChainShowsOnlyActualHitsAndTracksStunState()
    {
        CreateMainCamera();
        BindInventory(CreateElementInventoryAtLevel(ItemElement.Electric, 6));

        FishController first =
            CreateFish(new Vector2(0f, 0f), 100f, true);
        FishController second =
            CreateFish(new Vector2(1f, 0f), 100f, true);

        manager.HandleCombatDamage(
            CreateDamageResult(
                first,
                CombatDamageContext.Tool("test.electric.tool", null),
                1f));

        Assert.That(first.CurrentResistance, Is.EqualTo(90f));
        Assert.That(second.CurrentResistance, Is.EqualTo(90f));
        Assert.That(CountActiveLines("ElectricChainDischargeVisual"), Is.EqualTo(1));
        Assert.That(CountActiveLines("ElectricChainHitVisual"), Is.EqualTo(2));
        Assert.That(CountActiveLines("ElectricStunStatusVisual"), Is.EqualTo(2));

        FishMovement firstMovement = first.GetComponent<FishMovement>();
        Assert.That(
            firstMovement.HasTimedSpeedModifier("electric_synergy.stun"),
            Is.True);

        firstMovement.RemoveTimedSpeedModifier("electric_synergy.stun");
        InvokePrivate(manager, "UpdateSynergyStatusVisuals");

        Assert.That(CountActiveLines("ElectricStunStatusVisual"), Is.EqualTo(1));

        first.Initialize(first.Data);
        Assert.That(CountActiveLines("ElectricStunStatusVisual"), Is.EqualTo(1));
    }

    [Test]
    public void ElectricEventWithNoEligibleTargetCreatesNoVisualOrDamage()
    {
        CreateMainCamera();
        BindInventory(CreateElementInventoryAtLevel(ItemElement.Electric, 2));

        FishController unavailable =
            CreateFish(Vector2.zero, 100f, true);
        unavailable.gameObject.SetActive(false);

        manager.HandleCombatDamage(
            CreateDamageResult(
                unavailable,
                CombatDamageContext.Tool("test.electric.empty", null),
                1f,
                true));

        Assert.That(unavailable.CurrentResistance, Is.EqualTo(100f));
        Assert.That(manager.ActiveCombatVfxCount, Is.Zero);
    }

    [Test]
    public void ThunderstormUsesDistinctVisualsOnlyForActualTargets()
    {
        CreateMainCamera();
        RunItemInventory inventory =
            CreateElementInventoryAtLevel(ItemElement.Electric, 10);
        BindInventory(inventory);

        FishController first =
            CreateFish(Vector2.zero, 100f, true);
        FishController second =
            CreateFish(Vector2.right, 100f, true);
        float expectedDamage = manager.GetElectricThunderstormDamage(
            manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                inventory));

        manager.HandleCombatDamage(
            CreateDamageResult(
                first,
                CombatDamageContext.Tool("test.thunderstorm.tool", null),
                1f));

        Assert.That(
            first.CurrentResistance,
            Is.EqualTo(100f - expectedDamage).Within(0.0001f));
        Assert.That(
            second.CurrentResistance,
            Is.EqualTo(100f - expectedDamage).Within(0.0001f));
        Assert.That(CountActiveLines("ElectricThunderstormVisual"), Is.EqualTo(2));
        Assert.That(CountActiveLines("ElectricThunderstormHitVisual"), Is.EqualTo(2));
        Assert.That(CountActiveLines("ElectricChainDischargeVisual"), Is.Zero);
    }

    [Test]
    public void SwordStylesFollowSuccessfulPrimaryAdditionalAndRainHits()
    {
        CreateMainCamera();
        RunItemInventory inventory =
            CreateElementInventoryAtLevel(ItemElement.Sword, 10);
        BindInventory(inventory);

        SwordSynergySettings settings =
            GetPrivateField<SwordSynergySettings>(manager, "swordSynergy");
        SetPrivateField(settings, "soulSlashRequiredHits", 1);
        SetPrivateField(settings, "swordRainRequiredActivations", 1);

        FishController primary =
            CreateFish(Vector2.zero, 200f, false);
        FishController secondary =
            CreateFish(Vector2.right, 200f, false);
        FishController rainOnly =
            CreateFish(Vector2.up, 200f, false);

        manager.HandleCombatDamage(
            CreateDamageResult(
                primary,
                CombatDamageContext.Tool("test.sword.tool", null),
                1f));

        Assert.That(CountActiveLines("SoulSlashVisual"), Is.EqualTo(1));
        Assert.That(CountActiveLines("AdditionalSoulSlashVisual"), Is.EqualTo(1));
        Assert.That(CountActiveLines("SwordRainVisual"), Is.EqualTo(3));
        Assert.That(primary.CurrentResistance, Is.LessThan(200f));
        Assert.That(secondary.CurrentResistance, Is.LessThan(200f));
        Assert.That(rainOnly.CurrentResistance, Is.LessThan(200f));

        float primaryAfter = primary.CurrentResistance;
        float secondaryAfter = secondary.CurrentResistance;
        float rainAfter = rainOnly.CurrentResistance;

        InvokePrivate(manager, "LateUpdate");
        InvokePrivate(manager, "LateUpdate");

        Assert.That(primary.CurrentResistance, Is.EqualTo(primaryAfter));
        Assert.That(secondary.CurrentResistance, Is.EqualTo(secondaryAfter));
        Assert.That(rainOnly.CurrentResistance, Is.EqualTo(rainAfter));
    }

    [Test]
    public void ColdWaveAndFreezeUseActualTargetAndClearOnPoolReturn()
    {
        CreateMainCamera();
        BindInventory(CreateElementInventoryAtLevel(ItemElement.Ice, 6));

        FishController captured =
            CreateFish(Vector2.zero, 10f, true);
        captured.gameObject.SetActive(false);

        FishController target =
            CreateFish(new Vector2(0.75f, 0f), 100f, true);

        manager.HandleCombatDamage(
            CreateDamageResult(
                captured,
                CombatDamageContext.Tool("test.ice.capture", null),
                10f,
                true,
                Vector2.zero));

        Assert.That(target.CurrentResistance, Is.EqualTo(100f));
        Assert.That(CountActiveLines("ColdWaveVisual"), Is.EqualTo(1));
        Assert.That(CountActiveLines("IceFreezeStatusVisual"), Is.EqualTo(1));
        Assert.That(
            target.GetComponent<FishMovement>()
                .HasTimedSpeedModifier("ice_synergy.freeze"),
            Is.True);

        target.Initialize(target.Data);

        Assert.That(CountActiveLines("IceFreezeStatusVisual"), Is.Zero);
        Assert.That(
            target.GetComponent<FishMovement>()
                .HasTimedSpeedModifier("ice_synergy.freeze"),
            Is.False);
    }

    [Test]
    public void IceCaptureWithoutSecondaryTargetCreatesNoFakeWave()
    {
        CreateMainCamera();
        BindInventory(CreateElementInventoryAtLevel(ItemElement.Ice, 2));

        FishController captured =
            CreateFish(Vector2.zero, 10f, true);
        captured.gameObject.SetActive(false);

        manager.HandleCombatDamage(
            CreateDamageResult(
                captured,
                CombatDamageContext.Tool("test.ice.empty", null),
                10f,
                true,
                Vector2.zero));

        Assert.That(manager.ActiveCombatVfxCount, Is.Zero);
        Assert.That(CountActiveLines("ColdWaveVisual"), Is.Zero);
        Assert.That(CountActiveLines("FrostBurstVisual"), Is.Zero);
    }

    [Test]
    public void FrostBurstVisualMatchesActualDamageTarget()
    {
        CreateMainCamera();
        BindInventory(CreateElementInventoryAtLevel(ItemElement.Ice, 10));

        IceSynergySettings settings =
            GetPrivateField<IceSynergySettings>(manager, "iceSynergy");
        SetPrivateField(settings, "frostBurstRequiredCaptures", 1);

        FishController captured =
            CreateFish(Vector2.zero, 10f, true);
        captured.gameObject.SetActive(false);

        FishController target =
            CreateFish(Vector2.right, 100f, true);
        float expectedDamage = manager.GetIceFrostBurstDamage(
            manager.GetSingleElementSynergyState(
                ItemElement.Ice,
                CreateElementInventoryAtLevel(ItemElement.Ice, 10)));

        manager.HandleCombatDamage(
            CreateDamageResult(
                captured,
                CombatDamageContext.Tool("test.ice.burst", null),
                10f,
                true,
                Vector2.zero));

        Assert.That(
            target.CurrentResistance,
            Is.EqualTo(100f - expectedDamage).Within(0.0001f));
        Assert.That(CountActiveLines("FrostBurstVisual"), Is.EqualTo(1));
        Assert.That(CountActiveLines("ColdWaveVisual"), Is.Zero);
    }

    [Test]
    public void VisualPoolCapsReusesPausesAndClearsWithoutChangingGameplay()
    {
        for (int i = 0; i < 60; i++)
        {
            manager.ShowSquidInkAttack(
                Vector2.zero,
                new Vector2(i * 0.1f, 0f));
        }

        Assert.That(manager.ActiveCombatVfxCount, Is.EqualTo(48));
        Assert.That(manager.CreatedCombatVfxCount, Is.EqualTo(48));

        object pool = GetPrivateField<object>(manager, "combatVfxPool");
        InvokeMethod(pool, "Tick", 0f);

        Assert.That(
            manager.ActiveCombatVfxCount,
            Is.EqualTo(48),
            "Scaled delta time 0 must pause transient visual expiry.");

        InvokeMethod(pool, "Tick", 1f);
        Assert.That(manager.ActiveCombatVfxCount, Is.Zero);

        manager.ShowSquidInkAttack(Vector2.zero, Vector2.one);
        Assert.That(manager.ActiveCombatVfxCount, Is.EqualTo(2));
        Assert.That(manager.CreatedCombatVfxCount, Is.EqualTo(48));

        flow.FailMiniBossEncounter();
        InvokePrivate(manager, "Update");

        Assert.That(manager.ActiveCombatVfxCount, Is.Zero);
        Assert.That(manager.CreatedCombatVfxCount, Is.EqualTo(48));
    }

    private void BindInventory(RunItemInventory inventory)
    {
        SetPrivateField(manager, "boundInventory", inventory);
    }

    private Camera CreateMainCamera()
    {
        GameObject cameraObject =
            Own(new GameObject("Combat VFX Test Camera"));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        testCamera = camera;

        RenderTexture target =
            Own(new RenderTexture(256, 256, 0));
        camera.targetTexture = target;
        return camera;
    }

    private FishingRodController CreateRod(Vector2 position)
    {
        GameObject rodObject =
            Own(new GameObject("Combat VFX Test Rod"));
        rodObject.transform.position = position;
        rodObject.AddComponent<SpriteRenderer>();
        rodObject.AddComponent<BoxCollider2D>();

        FishingRodController rod =
            rodObject.AddComponent<FishingRodController>();
        InvokePrivate(rod, "Awake");
        Physics2D.SyncTransforms();
        return rod;
    }

    private SquidController CreateSquid(
        Vector2 position,
        float range,
        float duration)
    {
        GameObject squidObject =
            Own(new GameObject("Combat VFX Test Squid"));
        squidObject.transform.position = position;

        FishController fish =
            squidObject.AddComponent<FishController>();
        SquidController squid =
            squidObject.AddComponent<SquidController>();
        InvokePrivate(fish, "Awake");
        InvokePrivate(squid, "Awake");

        FishData data = CreateFishData(
            FishSpecialType.Squid,
            24f,
            range,
            duration);
        fish.Initialize(data);
        InvokePrivate(squid, "OnEnable");
        Physics2D.SyncTransforms();
        return squid;
    }

    private FishController CreateFish(
        Vector2 position,
        float resistance,
        bool withMovement)
    {
        GameObject fishObject =
            Own(new GameObject("Combat VFX Test Fish"));
        fishObject.transform.position = position;
        fishObject.AddComponent<SpriteRenderer>();

        FishController fish =
            fishObject.AddComponent<FishController>();
        if (withMovement)
        {
            FishMovement movement =
                fishObject.AddComponent<FishMovement>();
            InvokePrivate(movement, "Awake");
        }

        InvokePrivate(fish, "Awake");
        fish.Initialize(
            CreateFishData(
                FishSpecialType.None,
                resistance,
                0f,
                0f));
        return fish;
    }

    private FishData CreateFishData(
        FishSpecialType specialType,
        float resistance,
        float inkRange,
        float inkDuration)
    {
        FishData data =
            Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject serialized = new(data);
        serialized.FindProperty("specialType").enumValueIndex = (int)specialType;
        serialized.FindProperty("maxResistance").floatValue = resistance;
        serialized.FindProperty("inkRange").floatValue = inkRange;
        serialized.FindProperty("inkDisableDuration").floatValue = inkDuration;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return data;
    }

    private static CombatDamageResult CreateDamageResult(
        FishController fish,
        CombatDamageContext context,
        float damage,
        bool captured = false,
        Vector2? hitPosition = null) =>
        new(
            fish,
            fish != null ? fish.LifecycleVersion : 0,
            context,
            damage,
            hitPosition ?? (fish != null
                ? (Vector2)fish.transform.position
                : Vector2.zero),
            captured);

    private static RunItemInventory CreateElementInventoryAtLevel(
        ItemElement element,
        int elementLevel)
    {
        RunItemInventory inventory = new();
        if (elementLevel <= 0)
        {
            return inventory;
        }

        string firstItemId = element switch
        {
            ItemElement.Electric => ItemCatalog.StormOrbId,
            ItemElement.Sword => ItemCatalog.SpectralScabbardId,
            _ => ItemCatalog.FrostSigilId
        };
        string secondItemId = element switch
        {
            ItemElement.Electric => ItemCatalog.CapacitorCoilId,
            ItemElement.Sword => ItemCatalog.AutonomousSwordArrayId,
            _ => ItemCatalog.FrostCrystalId
        };

        AssertAcquire(inventory, firstItemId);
        int firstLevel = Mathf.Min(
            elementLevel,
            ItemLevelLimit.DefaultMaximumLevel);
        UpgradeRepeatedly(
            inventory,
            firstItemId,
            firstLevel - 1);

        int remaining = elementLevel - firstLevel;
        if (remaining > 0)
        {
            AssertAcquire(inventory, secondItemId);
            UpgradeRepeatedly(
                inventory,
                secondItemId,
                remaining - 1);
        }

        return inventory;
    }

    private static void AssertAcquire(
        RunItemInventory inventory,
        string itemId)
    {
        Assert.That(
            ItemCatalog.TryGet(
                itemId,
                out ItemDefinition definition),
            Is.True);
        Assert.That(inventory.TryAcquire(definition), Is.True);
    }

    private static void UpgradeRepeatedly(
        RunItemInventory inventory,
        string itemId,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            Assert.That(
                inventory.TryUpgrade(
                    itemId,
                    ItemLevelLimit.DefaultFinite,
                    out ItemUpgradeResult result),
                Is.True);
            Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
        }
    }

    private int CountActiveLines(string objectName)
    {
        int count = 0;
        for (int i = 0; i < manager.transform.childCount; i++)
        {
            Transform child = manager.transform.GetChild(i);
            if (child.gameObject.activeSelf &&
                string.Equals(
                    child.name,
                    objectName,
                    StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    private LineRenderer FindActiveLine(string objectName)
    {
        for (int i = 0; i < manager.transform.childCount; i++)
        {
            Transform child = manager.transform.GetChild(i);
            if (child.gameObject.activeSelf &&
                string.Equals(
                    child.name,
                    objectName,
                    StringComparison.Ordinal))
            {
                return child.GetComponent<LineRenderer>();
            }
        }

        Assert.Fail($"Active visual not found: {objectName}");
        return null;
    }

    private static void ReleaseInk(SquidController squid)
    {
        Physics2D.SyncTransforms();
        InvokePrivate(squid, "ReleaseInk");
    }

    private T Own<T>(T value)
        where T : UnityEngine.Object
    {
        ownedObjects.Add(value);
        return value;
    }

    private static void InvokePrivate(
        object target,
        string methodName,
        params object[] arguments) =>
        InvokeMethod(target, methodName, arguments);

    private static void InvokeMethod(
        object target,
        string methodName,
        params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, methodName);
        method.Invoke(target, arguments);
    }

    private static T GetPrivateField<T>(
        object target,
        string fieldName)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance |
            BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        return (T)field.GetValue(target);
    }

    private static void SetPrivateField<T>(
        object target,
        string fieldName,
        T value)
    {
        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance |
            BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }
}
#endif
