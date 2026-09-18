#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public sealed class ItemProgressionTests
{
    [Test]
    public void AcquiredItemStartsAtLevelOne()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);

        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(1));
    }

    [Test]
    public void SuccessfulUpgradeIncreasesLevelExactlyOnceAndRaisesOneChange()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
        int changes = 0;
        inventory.Changed += () => changes++;

        bool upgraded = inventory.TryUpgrade(
            ItemCatalog.StormOrbId,
            ItemLevelLimit.DefaultFinite,
            out ItemUpgradeResult result);

        Assert.That(upgraded, Is.True);
        Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(2));
        Assert.That(changes, Is.EqualTo(1));
    }

    [Test]
    public void UnownedItemCannotBeUpgradedAndStateDoesNotChange()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);

        bool upgraded = inventory.TryUpgrade(
            ItemCatalog.CapacitorCoilId,
            ItemLevelLimit.DefaultFinite,
            out ItemUpgradeResult result);

        Assert.That(upgraded, Is.False);
        Assert.That(result, Is.EqualTo(ItemUpgradeResult.ItemNotOwned));
        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(1));
        Assert.That(inventory.Count, Is.EqualTo(1));
    }

    [Test]
    public void DefaultFiniteMaximumPreventsLevelSix()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
        UpgradeRepeatedly(inventory, ItemCatalog.StormOrbId, ItemLevelLimit.DefaultFinite, 4);

        bool upgraded = inventory.TryUpgrade(
            ItemCatalog.StormOrbId,
            ItemLevelLimit.DefaultFinite,
            out ItemUpgradeResult result);

        Assert.That(upgraded, Is.False);
        Assert.That(result, Is.EqualTo(ItemUpgradeResult.MaximumLevelReached));
        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(5));
    }

    [Test]
    public void CustomFiniteMaximumIsRespected()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
        ItemLevelLimit limit = new(2, false);
        UpgradeRepeatedly(inventory, ItemCatalog.StormOrbId, limit, 1);

        Assert.That(
            inventory.TryUpgrade(ItemCatalog.StormOrbId, limit, out ItemUpgradeResult result),
            Is.False);
        Assert.That(result, Is.EqualTo(ItemUpgradeResult.MaximumLevelReached));
        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(2));
    }

    [Test]
    public void UnlimitedMaximumAllowsUpgradesBeyondLevelFive()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
        ItemLevelLimit limit = new(1, true);

        UpgradeRepeatedly(inventory, ItemCatalog.StormOrbId, limit, 6);

        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(7));
    }

    [Test]
    public void InvalidFiniteMaximumCannotCorruptOwnedState()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
        ItemLevelLimit invalidSerializedValue = new(0, false);

        bool upgraded = inventory.TryUpgrade(
            ItemCatalog.StormOrbId,
            invalidSerializedValue,
            out ItemUpgradeResult result);

        Assert.That(invalidSerializedValue.MaximumLevel, Is.EqualTo(1));
        Assert.That(upgraded, Is.False);
        Assert.That(result, Is.EqualTo(ItemUpgradeResult.MaximumLevelReached));
        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(1));
    }

    [Test]
    public void LoweredMaximumPreservesExistingOwnedLevel()
    {
        RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
        UpgradeRepeatedly(
            inventory,
            ItemCatalog.StormOrbId,
            new ItemLevelLimit(1, true),
            2);

        bool upgraded = inventory.TryUpgrade(
            ItemCatalog.StormOrbId,
            new ItemLevelLimit(2, false),
            out ItemUpgradeResult result);

        Assert.That(upgraded, Is.False);
        Assert.That(result, Is.EqualTo(ItemUpgradeResult.MaximumLevelReached));
        Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(3));
        Assert.That(inventory.Owns(ItemCatalog.StormOrbId), Is.True);
    }

    [TestCase(12f, 1, 0.2f, 12f)]
    [TestCase(12f, 2, 0.2f, 14.4f)]
    [TestCase(12f, 5, 0.2f, 21.6f)]
    [TestCase(16f, 5, 0.25f, 32f)]
    public void DamageScalingUsesAdditiveLevelOneBaseline(
        float baseline,
        int level,
        float bonusPerLevel,
        float expected)
    {
        Assert.That(
            ItemLevelScaling.CalculateAdditiveDamage(baseline, level, bonusPerLevel),
            Is.EqualTo(expected).Within(0.0001f));
    }

    [TestCase(2f, 1, 0.4f, 2f)]
    [TestCase(2f, 5, 0.4f, 3.6f)]
    [TestCase(3f, 5, 0.4f, 4.6f)]
    public void FrostDurationScalingUsesAdditiveSeconds(
        float baseline,
        int level,
        float secondsPerLevel,
        float expected)
    {
        Assert.That(
            ItemLevelScaling.CalculateAdditiveDuration(baseline, level, secondsPerLevel),
            Is.EqualTo(expected).Within(0.0001f));
    }

    [Test]
    public void ElementLevelSumsActualOwnedItemLevels()
    {
        RunGrowthState growth = new();
        AssertAcquire(growth.ItemInventory, ItemCatalog.StormOrbId);
        AssertAcquire(growth.ItemInventory, ItemCatalog.CapacitorCoilId);
        UpgradeRepeatedly(
            growth.ItemInventory,
            ItemCatalog.StormOrbId,
            ItemLevelLimit.DefaultFinite,
            2);
        UpgradeRepeatedly(
            growth.ItemInventory,
            ItemCatalog.CapacitorCoilId,
            ItemLevelLimit.DefaultFinite,
            1);

        Assert.That(growth.GetElementLevel(ItemElement.Electric), Is.EqualTo(5));
        Assert.That(growth.GetElementLevel(ItemElement.Sword), Is.EqualTo(0));
        Assert.That(growth.GetElementLevel(ItemElement.Ice), Is.EqualTo(0));
    }

    [Test]
    public void NewRunStateHasNoOwnershipOrItemLevels()
    {
        RunGrowthState previousRun = new();
        AssertAcquire(previousRun.ItemInventory, ItemCatalog.StormOrbId);
        UpgradeRepeatedly(
            previousRun.ItemInventory,
            ItemCatalog.StormOrbId,
            ItemLevelLimit.DefaultFinite,
            2);

        RunGrowthState newRun = new();

        Assert.That(previousRun.ItemInventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(3));
        Assert.That(newRun.ItemInventory.Count, Is.Zero);
        Assert.That(newRun.GetElementLevel(ItemElement.Electric), Is.Zero);
    }

    [Test]
    public void ManagerDefaultsAllItemsToFiniteLevelFive()
    {
        GameObject owner = new("Item Progression Test Manager");
        ItemEffectManager manager = owner.AddComponent<ItemEffectManager>();
        try
        {
            foreach (ItemDefinition item in ItemCatalog.All)
            {
                Assert.That(manager.TryGetLevelLimit(item.ItemId, out ItemLevelLimit limit), Is.True);
                Assert.That(limit.IsUnlimited, Is.False, item.ItemId);
                Assert.That(limit.MaximumLevel, Is.EqualTo(5), item.ItemId);
            }
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [TestCase(ItemCatalog.StormOrbId, 12f, 14.4f)]
    [TestCase(ItemCatalog.CapacitorCoilId, 8f, 9.6f)]
    [TestCase(ItemCatalog.SpectralScabbardId, 16f, 20f)]
    [TestCase(ItemCatalog.AutonomousSwordArrayId, 18f, 21.6f)]
    [TestCase(ItemCatalog.FrostSigilId, 2f, 2.4f)]
    [TestCase(ItemCatalog.FrostCrystalId, 3f, 3.4f)]
    public void ManagerRuntimeValueUsesUpgradedOwnedLevel(
        string itemId,
        float levelOneValue,
        float levelTwoValue)
    {
        GameObject owner = new("Item Effect Scaling Test Manager");
        ItemEffectManager manager = owner.AddComponent<ItemEffectManager>();
        try
        {
            RunItemInventory inventory = CreateInventoryWith(itemId);
            Assert.That(
                manager.GetEffectivePrimaryValue(itemId, inventory),
                Is.EqualTo(levelOneValue).Within(0.0001f));

            Assert.That(
                inventory.TryUpgrade(
                    itemId,
                    ItemLevelLimit.DefaultFinite,
                    out ItemUpgradeResult result),
                Is.True);
            Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
            Assert.That(
                manager.GetEffectivePrimaryValue(itemId, inventory),
                Is.EqualTo(levelTwoValue).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    private static RunItemInventory CreateInventoryWith(string itemId)
    {
        RunItemInventory inventory = new();
        AssertAcquire(inventory, itemId);
        return inventory;
    }

    private static void AssertAcquire(RunItemInventory inventory, string itemId)
    {
        Assert.That(ItemCatalog.TryGet(itemId, out ItemDefinition definition), Is.True);
        Assert.That(inventory.TryAcquire(definition), Is.True);
    }

    private static void UpgradeRepeatedly(
        RunItemInventory inventory,
        string itemId,
        ItemLevelLimit limit,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            Assert.That(inventory.TryUpgrade(itemId, limit, out ItemUpgradeResult result), Is.True);
            Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
        }
    }
}
#endif
