#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
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

    [Test]
    public void FourOwnedEligibleItemsProduceTwoDistinctUpgradeCandidates()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = new();
            AssertAcquire(inventory, ItemCatalog.StormOrbId);
            AssertAcquire(inventory, ItemCatalog.CapacitorCoilId);
            AssertAcquire(inventory, ItemCatalog.SpectralScabbardId);
            AssertAcquire(inventory, ItemCatalog.AutonomousSwordArrayId);

            List<ItemDefinition> candidates = ItemUpgradeRewardLogic.GenerateCandidates(
                inventory, manager, 2, new System.Random(7));

            Assert.That(candidates.Count, Is.EqualTo(2));
            Assert.That(candidates[0].ItemId, Is.Not.EqualTo(candidates[1].ItemId));
            Assert.That(inventory.Owns(candidates[0].ItemId), Is.True);
            Assert.That(inventory.Owns(candidates[1].ItemId), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void MaximumLevelItemIsExcludedFromUpgradeCandidates()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetInt(manager, "stormOrbMaximumLevel", 1);
            RunItemInventory inventory = new();
            AssertAcquire(inventory, ItemCatalog.StormOrbId);
            AssertAcquire(inventory, ItemCatalog.CapacitorCoilId);

            List<ItemDefinition> candidates = ItemUpgradeRewardLogic.GenerateCandidates(
                inventory, manager, 2, new System.Random(1));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].ItemId, Is.EqualTo(ItemCatalog.CapacitorCoilId));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void UnlimitedItemRemainsEligibleBeyondDefaultMaximum()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetBool(manager, "stormOrbUnlimitedMaximumLevel", true);
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
            for (int i = 0; i < 6; i++)
            {
                Assert.That(
                    manager.TryUpgradeItem(
                        ItemCatalog.StormOrbId, inventory, out ItemUpgradeResult result),
                    Is.True);
                Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
            }

            List<ItemDefinition> candidates = ItemUpgradeRewardLogic.GenerateCandidates(
                inventory, manager, 2, new System.Random(1));

            Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(7));
            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].ItemId, Is.EqualTo(ItemCatalog.StormOrbId));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void OnlyOneEligibleItemProducesOneUpgradeChoice()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetInt(manager, "stormOrbMaximumLevel", 1);
            RunItemInventory inventory = new();
            AssertAcquire(inventory, ItemCatalog.StormOrbId);
            AssertAcquire(inventory, ItemCatalog.FrostSigilId);

            List<ItemDefinition> candidates = ItemUpgradeRewardLogic.GenerateCandidates(
                inventory, manager, 2, new System.Random(2));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].ItemId, Is.EqualTo(ItemCatalog.FrostSigilId));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ZeroEligibleItemsDoNotBeginMandatoryUpgradeReward()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetInt(manager, "stormOrbMaximumLevel", 1);
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
            List<ItemDefinition> candidates = ItemUpgradeRewardLogic.GenerateCandidates(
                inventory, manager, 2, new System.Random(3));
            ItemUpgradeRewardState reward = new();

            Assert.That(candidates, Is.Empty);
            Assert.That(reward.TryBegin(candidates), Is.False);
            Assert.That(reward.IsPending, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void UnownedItemsAreNeverOfferedForUpgrade()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
            List<ItemDefinition> candidates = ItemUpgradeRewardLogic.GenerateCandidates(
                inventory, manager, 6, new System.Random(4));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].ItemId, Is.EqualTo(ItemCatalog.StormOrbId));
            Assert.That(
                ItemUpgradeRewardLogic.IsEligibleCandidate(
                    inventory, manager, ItemCatalog.CapacitorCoilId),
                Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void InvalidItemIdsAreNeverEligibleForUpgradeRewards()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);

            Assert.That(
                ItemUpgradeRewardLogic.IsEligibleCandidate(
                    inventory, manager, "invalid_item_id"),
                Is.False);
            Assert.That(
                ItemUpgradeRewardLogic.IsEligibleCandidate(inventory, manager, null),
                Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void OneRewardUpgradesExactlyOneOfferedItem()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = new();
            AssertAcquire(inventory, ItemCatalog.StormOrbId);
            AssertAcquire(inventory, ItemCatalog.CapacitorCoilId);
            ItemUpgradeRewardState reward = BeginReward(ItemCatalog.StormOrbId);

            ItemUpgradeConfirmationResult confirmation = reward.TryConfirm(
                ItemCatalog.StormOrbId, inventory, manager, out ItemUpgradeResult result);

            Assert.That(confirmation, Is.EqualTo(ItemUpgradeConfirmationResult.Success));
            Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
            Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(2));
            Assert.That(inventory.GetOwned(ItemCatalog.CapacitorCoilId).Level, Is.EqualTo(1));
            Assert.That(reward.IsConsumed, Is.True);
            Assert.That(reward.IsPending, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void RepeatedConfirmationCannotUpgradeTwice()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
            ItemUpgradeRewardState reward = BeginReward(ItemCatalog.StormOrbId);

            Assert.That(
                reward.TryConfirm(
                    ItemCatalog.StormOrbId, inventory, manager, out _),
                Is.EqualTo(ItemUpgradeConfirmationResult.Success));
            Assert.That(
                reward.TryConfirm(
                    ItemCatalog.StormOrbId, inventory, manager, out _),
                Is.EqualTo(ItemUpgradeConfirmationResult.RewardNotPending));
            Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void NonOfferedItemCannotBeSelected()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = new();
            AssertAcquire(inventory, ItemCatalog.StormOrbId);
            AssertAcquire(inventory, ItemCatalog.CapacitorCoilId);
            ItemUpgradeRewardState reward = BeginReward(ItemCatalog.StormOrbId);

            ItemUpgradeConfirmationResult confirmation = reward.TryConfirm(
                ItemCatalog.CapacitorCoilId, inventory, manager, out _);

            Assert.That(confirmation, Is.EqualTo(ItemUpgradeConfirmationResult.ItemNotOffered));
            Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(1));
            Assert.That(inventory.GetOwned(ItemCatalog.CapacitorCoilId).Level, Is.EqualTo(1));
            Assert.That(reward.IsPending, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void FailedUpgradeLeavesRewardAndItemStateConsistent()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
            ItemUpgradeRewardState reward = BeginReward(ItemCatalog.StormOrbId);
            SetInt(manager, "stormOrbMaximumLevel", 1);

            ItemUpgradeConfirmationResult confirmation = reward.TryConfirm(
                ItemCatalog.StormOrbId, inventory, manager, out ItemUpgradeResult result);

            Assert.That(
                confirmation,
                Is.EqualTo(ItemUpgradeConfirmationResult.ItemNoLongerEligible));
            Assert.That(result, Is.EqualTo(ItemUpgradeResult.MaximumLevelReached));
            Assert.That(inventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(1));
            Assert.That(reward.IsPending, Is.True);
            Assert.That(reward.IsConsumed, Is.False);
            Assert.That(reward.Contains(ItemCatalog.StormOrbId), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void SuccessfulRewardUpdatesLevelAndElementTotal()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunGrowthState growth = new();
            AssertAcquire(growth.ItemInventory, ItemCatalog.StormOrbId);
            ItemUpgradeRewardState reward = BeginReward(ItemCatalog.StormOrbId);

            Assert.That(
                reward.TryConfirm(
                    ItemCatalog.StormOrbId, growth.ItemInventory, manager, out _),
                Is.EqualTo(ItemUpgradeConfirmationResult.Success));

            Assert.That(growth.ItemInventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(2));
            Assert.That(growth.GetElementLevel(ItemElement.Electric), Is.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void NewRunResetClearsPendingUpgradeRewardState()
    {
        ItemUpgradeRewardState reward = BeginReward(ItemCatalog.StormOrbId);

        reward.Reset();
        RunGrowthState newRun = new();

        Assert.That(reward.IsPending, Is.False);
        Assert.That(reward.IsConsumed, Is.False);
        Assert.That(reward.OfferedCount, Is.Zero);
        Assert.That(newRun.ItemInventory.Count, Is.Zero);
    }

    [Test]
    public void AreaOneMilestonesDoNotGrantItemUpgrades()
    {
        RunGrowthState growth = new();
        AssertAcquire(growth.ItemInventory, ItemCatalog.StormOrbId);
        ItemUpgradeRewardState reward = new();

        Assert.That(
            growth.TryGrantMilestoneMasteryPoints(RunMilestoneType.MiniBoss, 1, 1),
            Is.True);
        Assert.That(
            growth.TryGrantMilestoneMasteryPoints(RunMilestoneType.Boss, 1, 2),
            Is.True);

        Assert.That(growth.ItemInventory.GetOwned(ItemCatalog.StormOrbId).Level, Is.EqualTo(1));
        Assert.That(growth.GetElementLevel(ItemElement.Electric), Is.EqualTo(1));
        Assert.That(reward.IsPending, Is.False);
    }

    [TestCase(0, false, false, false, false, false)]
    [TestCase(1, false, false, false, false, false)]
    [TestCase(2, true, false, false, false, false)]
    [TestCase(4, true, true, false, false, false)]
    [TestCase(6, true, true, true, false, false)]
    [TestCase(8, true, true, true, true, false)]
    [TestCase(10, true, true, true, true, true)]
    public void ElectricSingleElementTiersAreCumulativeAtDefaultThresholds(
        int elementLevel,
        bool level2,
        bool level4,
        bool level6,
        bool level8,
        bool level10)
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateElectricInventoryAtLevel(elementLevel);

            SingleElementSynergyState state = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                inventory);

            Assert.That(state.ElementLevel, Is.EqualTo(elementLevel));
            Assert.That(state.IsLevel2Active, Is.EqualTo(level2));
            Assert.That(state.IsLevel4Active, Is.EqualTo(level4));
            Assert.That(state.IsLevel6Active, Is.EqualTo(level6));
            Assert.That(state.IsLevel8Active, Is.EqualTo(level8));
            Assert.That(state.IsLevel10Active, Is.EqualTo(level10));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void OwnedElectricUpgradeChangesTierEligibilityImmediately()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(ItemCatalog.StormOrbId);
            Assert.That(
                manager.GetSingleElementSynergyState(ItemElement.Electric, inventory)
                    .IsLevel2Active,
                Is.False);

            Assert.That(
                manager.TryUpgradeItem(
                    ItemCatalog.StormOrbId,
                    inventory,
                    out ItemUpgradeResult result),
                Is.True);

            Assert.That(result, Is.EqualTo(ItemUpgradeResult.Success));
            Assert.That(
                manager.GetSingleElementSynergyState(ItemElement.Electric, inventory)
                    .IsLevel2Active,
                Is.True);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [TestCase(ItemCatalog.StormOrbId)]
    [TestCase(ItemCatalog.CapacitorCoilId)]
    public void EitherElectricItemAloneCanUnlockLevelSix(string itemId)
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(itemId);
            UpgradeRepeatedly(
                inventory,
                itemId,
                new ItemLevelLimit(1, true),
                5);

            SingleElementSynergyState state = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                inventory);

            Assert.That(inventory.GetElementLevel(ItemElement.Electric), Is.EqualTo(6));
            Assert.That(state.IsLevel2Active, Is.True);
            Assert.That(state.IsLevel4Active, Is.True);
            Assert.That(state.IsLevel6Active, Is.True);
            Assert.That(state.IsLevel8Active, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void NewRunAndRuntimeResetClearAllElectricSynergyState()
    {
        ElectricSynergyRuntimeState runtime = new();
        SynergyTargetKey key = new(7, 1);
        SingleElementSynergyState active = new SingleElementSynergyThresholds()
            .Evaluate(CreateElectricInventoryAtLevel(10), ItemElement.Electric);
        Assert.That(runtime.TryBeginStun(key, 1f, 1f, 3f), Is.True);
        Assert.That(
            runtime.TryBeginActivation(active, 1f, 7f, 15f, true, true),
            Is.EqualTo(ElectricSynergyActivation.Thunderstorm));

        runtime.Reset();
        RunGrowthState newRun = new();
        SingleElementSynergyState tiers = new SingleElementSynergyThresholds()
            .Evaluate(newRun.ItemInventory, ItemElement.Electric);

        Assert.That(runtime.StunLockoutCount, Is.Zero);
        Assert.That(runtime.TryBeginStun(key, 1f, 1f, 3f), Is.True);
        Assert.That(
            runtime.TryBeginActivation(active, 1f, 7f, 15f, true, true),
            Is.EqualTo(ElectricSynergyActivation.Thunderstorm));
        Assert.That(tiers.ElementLevel, Is.Zero);
        Assert.That(tiers.IsLevel2Active, Is.False);
    }

    [Test]
    public void CustomInspectorThresholdsAreUsedByManagerEvaluation()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetNestedInt(manager, "singleElementThresholds", "level2Threshold", 3);
            SetNestedInt(manager, "singleElementThresholds", "level4Threshold", 5);
            SetNestedInt(manager, "singleElementThresholds", "level6Threshold", 7);
            SetNestedInt(manager, "singleElementThresholds", "level8Threshold", 9);
            SetNestedInt(manager, "singleElementThresholds", "level10Threshold", 11);
            RunItemInventory inventory = CreateElectricInventoryAtLevel(8);

            SingleElementSynergyState state = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                inventory);

            Assert.That(state.IsLevel2Active, Is.True);
            Assert.That(state.IsLevel4Active, Is.True);
            Assert.That(state.IsLevel6Active, Is.True);
            Assert.That(state.IsLevel8Active, Is.False);
            Assert.That(state.IsLevel10Active, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ElectricLevel2TargetingIsDistinctEligibleAndBounded()
    {
        List<Object> cleanup = new();
        try
        {
            FishController first = CreateFish(FishSpecialType.None, 100f, cleanup);
            FishController second = CreateFish(FishSpecialType.Pufferfish, 100f, cleanup);
            FishController third = CreateFish(FishSpecialType.Squid, 100f, cleanup);
            FishController pooled = CreateFish(FishSpecialType.None, 100f, cleanup);
            first.transform.position = new Vector2(1f, 0f);
            second.transform.position = new Vector2(2f, 0f);
            third.transform.position = new Vector2(3f, 0f);
            pooled.transform.position = new Vector2(0.5f, 0f);
            pooled.gameObject.SetActive(false);

            List<FishController> candidates = new()
            {
                third,
                first,
                first,
                pooled,
                second
            };
            List<FishController> selected =
                ElectricSynergyTargeting.SelectDistinctEligibleTargets(
                    candidates,
                    Vector2.zero,
                    5f,
                    2);

            Assert.That(selected.Count, Is.EqualTo(2));
            Assert.That(selected[0], Is.SameAs(first));
            Assert.That(selected[1], Is.SameAs(second));
            Assert.That(selected, Is.Unique);
        }
        finally
        {
            DestroyAll(cleanup);
        }
    }

    [Test]
    public void ElectricLevelFourAddsOnlyConfiguredChainTargets()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetNestedInt(
                manager,
                "electricSynergy",
                "chainTargetCount",
                2);
            SetNestedInt(
                manager,
                "electricSynergy",
                "conductiveAdditionalTargets",
                1);
            SingleElementSynergyState levelTwo = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(2));
            SingleElementSynergyState levelFour = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(4));

            Assert.That(manager.GetElectricChainTargetCount(levelTwo), Is.EqualTo(2));
            Assert.That(manager.GetElectricChainTargetCount(levelFour), Is.EqualTo(3));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ElectricLevelEightMultipliesOnlySynergyDamageExactlyOnce()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SingleElementSynergyState levelSix = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(6));
            SingleElementSynergyState levelEight = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(8));
            float stormOrbLevelOne = manager.GetEffectivePrimaryValue(
                ItemCatalog.StormOrbId,
                1);

            Assert.That(manager.GetElectricChainDamage(levelSix), Is.EqualTo(10f));
            Assert.That(manager.GetElectricChainDamage(levelEight), Is.EqualTo(12.5f));
            Assert.That(manager.GetElectricThunderstormDamage(levelEight), Is.EqualTo(22.5f));
            Assert.That(stormOrbLevelOne, Is.EqualTo(12f));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [TestCase(FishSpecialType.MiniBoss)]
    [TestCase(FishSpecialType.Boss)]
    public void ElectricSynergyResistanceDamageIsNotAttenuatedForBosses(
        FishSpecialType specialType)
    {
        List<Object> cleanup = new();
        try
        {
            FishController fish = CreateFish(specialType, 100f, cleanup);

            fish.TakeCaptureDamage(
                25f,
                CombatDamageContext.ItemSynergy("test.electric", null));

            Assert.That(fish.CurrentResistance, Is.EqualTo(75f).Within(0.0001f));
        }
        finally
        {
            DestroyAll(cleanup);
        }
    }

    [Test]
    public void CrowdControlPolicyAttenuatesMiniBossAndBossIndependently()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetNestedFloat(
                manager,
                "synergyCrowdControl",
                "miniBossMultiplier",
                0.4f);
            SetNestedFloat(
                manager,
                "synergyCrowdControl",
                "bossMultiplier",
                0.65f);
            SingleElementSynergyState levelFour = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(4));
            SingleElementSynergyState levelSix = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(6));

            Assert.That(
                manager.GetElectricDischargeStunDuration(
                    levelFour,
                    FishSpecialType.None),
                Is.Zero);
            Assert.That(
                manager.GetElectricDischargeStunDuration(
                    levelSix,
                    FishSpecialType.None),
                Is.EqualTo(1f));
            Assert.That(
                manager.GetElectricDischargeStunDuration(
                    levelSix,
                    FishSpecialType.Squid),
                Is.EqualTo(1f));
            Assert.That(
                manager.GetElectricDischargeStunDuration(
                    levelSix,
                    FishSpecialType.MiniBoss),
                Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(
                manager.GetElectricDischargeStunDuration(
                    levelSix,
                    FishSpecialType.Boss),
                Is.EqualTo(0.65f).Within(0.0001f));

            SynergyCrowdControlPolicy policy = new(0.4f, 0.65f);
            Assert.That(
                policy.GetAdjustedAdditionalSlowPercentage(
                    0.1f,
                    FishSpecialType.MiniBoss),
                Is.EqualTo(0.04f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ElectricRestunLockoutStartsAfterStunEnds()
    {
        ElectricSynergyRuntimeState runtime = new();
        SynergyTargetKey key = new(11, 3);

        Assert.That(runtime.TryBeginStun(key, 10f, 1f, 3f), Is.True);
        Assert.That(runtime.TryBeginStun(key, 10.5f, 1f, 3f), Is.False);
        Assert.That(runtime.TryBeginStun(key, 13.99f, 1f, 3f), Is.False);
        Assert.That(runtime.TryBeginStun(key, 14f, 1f, 3f), Is.True);
    }

    [Test]
    public void ElectricStunModifierRemovalPreservesUnrelatedMovementControl()
    {
        GameObject owner = new("Synergy Movement Composition Test");
        try
        {
            owner.AddComponent<FishController>();
            FishMovement movement = owner.AddComponent<FishMovement>();
            movement.ApplyTimedSpeedModifier(ItemCatalog.FrostSigilId, 0.6f, 10f);
            movement.ApplyTimedSpeedModifier("electric_synergy.stun", 0f, 1f);

            movement.RemoveTimedSpeedModifier("electric_synergy.stun");

            Assert.That(
                movement.HasTimedSpeedModifier(ItemCatalog.FrostSigilId),
                Is.True);
            Assert.That(
                movement.HasTimedSpeedModifier("electric_synergy.stun"),
                Is.False);

            movement.InitializeSchoolMovement(0f);
            Assert.That(
                movement.HasTimedSpeedModifier(ItemCatalog.FrostSigilId),
                Is.False);
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ElectricCooldownsAndThunderstormPriorityUseScaledTime()
    {
        ElectricSynergyRuntimeState runtime = new();
        SingleElementSynergyState state = new SingleElementSynergyThresholds()
            .Evaluate(CreateElectricInventoryAtLevel(10), ItemElement.Electric);

        Assert.That(
            runtime.TryBeginActivation(state, 10f, 7f, 15f, true, true),
            Is.EqualTo(ElectricSynergyActivation.Thunderstorm));
        Assert.That(runtime.ChainDischargeReadyAt, Is.EqualTo(17f));
        Assert.That(
            runtime.TryBeginActivation(state, 10.1f, 7f, 15f, true, true),
            Is.EqualTo(ElectricSynergyActivation.None));
        Assert.That(
            runtime.TryBeginActivation(state, 17f, 7f, 15f, true, true),
            Is.EqualTo(ElectricSynergyActivation.ChainDischarge));
        Assert.That(
            runtime.TryBeginActivation(state, 25f, 7f, 15f, true, true),
            Is.EqualTo(ElectricSynergyActivation.Thunderstorm));
    }

    [Test]
    public void OnlyPositiveToolDamageCanTriggerIndependentSynergy()
    {
        List<Object> cleanup = new();
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            FishController fish = CreateFish(FishSpecialType.None, 100f, cleanup);
            CombatDamageResult toolHit = CreateDamageResult(
                fish,
                CombatDamageContext.Tool("test.tool", null),
                5f);
            CombatDamageResult itemHit = CreateDamageResult(
                fish,
                CombatDamageContext.Item(ItemCatalog.StormOrbId, null),
                5f);
            CombatDamageResult synergyHit = CreateDamageResult(
                fish,
                CombatDamageContext.ItemSynergy(ElectricLevelIdForTest(), null),
                5f);
            CombatDamageResult zeroDamage = CreateDamageResult(
                fish,
                CombatDamageContext.Tool("test.zero", null),
                0f);

            Assert.That(manager.CanProcessCombatTrigger(toolHit, 1f), Is.True);
            Assert.That(manager.CanProcessCombatTrigger(itemHit, 1f), Is.False);
            Assert.That(manager.CanProcessCombatTrigger(synergyHit, 1f), Is.False);
            Assert.That(manager.CanProcessCombatTrigger(zeroDamage, 1f), Is.False);

            ElectricSynergyRuntimeState runtime = new();
            SingleElementSynergyState state = manager.GetSingleElementSynergyState(
                ItemElement.Electric,
                CreateElectricInventoryAtLevel(2));
            Assert.That(
                runtime.TryBeginActivation(state, 1f, 7f, 15f, true, false),
                Is.EqualTo(ElectricSynergyActivation.ChainDischarge));
        }
        finally
        {
            DestroyAll(cleanup);
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ContinuousToolDamageKeepsSourceSpecificHalfSecondPolicy()
    {
        List<Object> cleanup = new();
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        GameObject firstSource = new("Continuous Source A");
        GameObject secondSource = new("Continuous Source B");
        cleanup.Add(firstSource);
        cleanup.Add(secondSource);
        try
        {
            FishController fish = CreateFish(FishSpecialType.None, 100f, cleanup);
            CombatDamageResult first = CreateDamageResult(
                fish,
                CombatDamageContext.Tool("net.dot", firstSource, true),
                1f);
            CombatDamageResult otherSource = CreateDamageResult(
                fish,
                CombatDamageContext.Tool("net.dot", secondSource, true),
                1f);

            Assert.That(manager.CanProcessCombatTrigger(first, 10f), Is.True);
            Assert.That(manager.CanProcessCombatTrigger(first, 10.49f), Is.False);
            Assert.That(manager.CanProcessCombatTrigger(otherSource, 10.49f), Is.True);
            Assert.That(manager.CanProcessCombatTrigger(first, 10.5f), Is.True);
        }
        finally
        {
            DestroyAll(cleanup);
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ElementHudShowsOnlyOwnedElementsOnePerLineAndRefreshesFromInventory()
    {
        RunItemInventory inventory = new();
        Assert.That(ItemHUD.BuildElementLevelText(inventory), Is.Empty);

        AssertAcquire(inventory, ItemCatalog.StormOrbId);
        Assert.That(ItemHUD.BuildElementLevelText(inventory), Is.EqualTo("전기 Lv.1"));

        UpgradeRepeatedly(
            inventory,
            ItemCatalog.StormOrbId,
            ItemLevelLimit.DefaultFinite,
            1);
        AssertAcquire(inventory, ItemCatalog.SpectralScabbardId);
        Assert.That(
            ItemHUD.BuildElementLevelText(inventory),
            Is.EqualTo("전기 Lv.2\n검 Lv.1"));
        Assert.That(ItemHUD.BuildElementLevelText(inventory), Does.Not.Contain("얼음"));

        RunItemInventory newRunInventory = new();
        Assert.That(ItemHUD.BuildElementLevelText(newRunInventory), Is.Empty);
    }

    [Test]
    public void ElementTooltipUsesConfiguredFiveTiersInAscendingOrder()
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            SetNestedInt(manager, "singleElementThresholds", "level2Threshold", 3);
            SetNestedInt(manager, "singleElementThresholds", "level4Threshold", 5);
            SetNestedInt(manager, "singleElementThresholds", "level6Threshold", 7);
            SetNestedInt(manager, "singleElementThresholds", "level8Threshold", 9);
            SetNestedInt(manager, "singleElementThresholds", "level10Threshold", 11);
            SetNestedFloat(manager, "electricSynergy", "chainCooldown", 9f);
            SetNestedFloat(manager, "electricSynergy", "chainDamage", 13f);
            RunItemInventory inventory = CreateElectricInventoryAtLevel(8);

            string tooltip = manager.BuildElementSynergyTooltipText(
                ItemElement.Electric,
                inventory);

            int level2 = tooltip.IndexOf("Lv.3 — 연쇄 방전", System.StringComparison.Ordinal);
            int level4 = tooltip.IndexOf("Lv.5 — 전도 확장", System.StringComparison.Ordinal);
            int level6 = tooltip.IndexOf("Lv.7 — 감전 방전", System.StringComparison.Ordinal);
            int level8 = tooltip.IndexOf("Lv.9 — 과충전", System.StringComparison.Ordinal);
            int level10 = tooltip.IndexOf("Lv.11 — 천둥 폭풍", System.StringComparison.Ordinal);
            Assert.That(level2, Is.GreaterThanOrEqualTo(0));
            Assert.That(level4, Is.GreaterThan(level2));
            Assert.That(level6, Is.GreaterThan(level4));
            Assert.That(level8, Is.GreaterThan(level6));
            Assert.That(level10, Is.GreaterThan(level8));
            Assert.That(tooltip, Does.Contain("9초마다"));
            Assert.That(tooltip, Does.Contain("저항력 피해 13"));
            Assert.That(tooltip, Does.Contain("연쇄 방전 [활성]"));
            Assert.That(tooltip, Does.Contain("과충전 [미해금]"));
        }
        finally
        {
            Object.DestroyImmediate(owner);
        }
    }

    [TestCase(ItemElement.Sword, ItemCatalog.SpectralScabbardId, "영혼 참격")]
    [TestCase(ItemElement.Ice, ItemCatalog.FrostSigilId, "냉기 파동")]
    public void UnimplementedElementTooltipNeverClaimsActive(
        ItemElement element,
        string itemId,
        string levelTwoName)
    {
        GameObject owner = CreateEffectManager(out ItemEffectManager manager);
        try
        {
            RunItemInventory inventory = CreateInventoryWith(itemId);
            UpgradeRepeatedly(inventory, itemId, ItemLevelLimit.DefaultFinite, 1);

            string tooltip = manager.BuildElementSynergyTooltipText(element, inventory);

            Assert.That(tooltip, Does.Contain(levelTwoName));
            Assert.That(tooltip, Does.Contain("조건 충족 · 구현 예정"));
            Assert.That(tooltip, Does.Not.Contain("[활성]"));
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

    private static RunItemInventory CreateElectricInventoryAtLevel(int elementLevel)
    {
        RunItemInventory inventory = new();
        if (elementLevel <= 0)
        {
            return inventory;
        }

        AssertAcquire(inventory, ItemCatalog.StormOrbId);
        int stormLevel = Mathf.Min(elementLevel, ItemLevelLimit.DefaultMaximumLevel);
        UpgradeRepeatedly(
            inventory,
            ItemCatalog.StormOrbId,
            ItemLevelLimit.DefaultFinite,
            stormLevel - 1);

        int remaining = elementLevel - stormLevel;
        if (remaining > 0)
        {
            AssertAcquire(inventory, ItemCatalog.CapacitorCoilId);
            UpgradeRepeatedly(
                inventory,
                ItemCatalog.CapacitorCoilId,
                ItemLevelLimit.DefaultFinite,
                remaining - 1);
        }

        return inventory;
    }

    private static FishController CreateFish(
        FishSpecialType specialType,
        float maximumResistance,
        ICollection<Object> cleanup)
    {
        FishData data = ScriptableObject.CreateInstance<FishData>();
        SerializedObject serialized = new(data);
        serialized.FindProperty("specialType").enumValueIndex = (int)specialType;
        serialized.FindProperty("maxResistance").floatValue = maximumResistance;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject owner = new($"Synergy Test Fish {specialType}");
        FishController fish = owner.AddComponent<FishController>();
        fish.Initialize(data);
        cleanup.Add(owner);
        cleanup.Add(data);
        return fish;
    }

    private static CombatDamageResult CreateDamageResult(
        FishController fish,
        CombatDamageContext context,
        float appliedDamage) =>
        new(
            fish,
            fish != null ? fish.LifecycleVersion : 0,
            context,
            appliedDamage,
            fish != null ? (Vector2)fish.transform.position : Vector2.zero,
            false);

    private static void DestroyAll(IReadOnlyList<Object> cleanup)
    {
        for (int i = cleanup.Count - 1; i >= 0; i--)
        {
            if (cleanup[i] != null)
            {
                Object.DestroyImmediate(cleanup[i]);
            }
        }
    }

    private static string ElectricLevelIdForTest() => "electric_synergy.level2";

    private static GameObject CreateEffectManager(out ItemEffectManager manager)
    {
        GameObject owner = new("Item Reward Upgrade Test Manager");
        manager = owner.AddComponent<ItemEffectManager>();
        return owner;
    }

    private static ItemUpgradeRewardState BeginReward(string itemId)
    {
        Assert.That(ItemCatalog.TryGet(itemId, out ItemDefinition definition), Is.True);
        ItemUpgradeRewardState reward = new();
        Assert.That(
            reward.TryBegin(new List<ItemDefinition> { definition }),
            Is.True);
        return reward;
    }

    private static void SetInt(Object target, string propertyName, int value)
    {
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, propertyName);
        property.intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(Object target, string propertyName, bool value)
    {
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        Assert.That(property, Is.Not.Null, propertyName);
        property.boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetNestedInt(
        Object target,
        string parentPropertyName,
        string propertyName,
        int value)
    {
        SerializedObject serialized = new(target);
        SerializedProperty parent = serialized.FindProperty(parentPropertyName);
        Assert.That(parent, Is.Not.Null, parentPropertyName);
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        Assert.That(property, Is.Not.Null, propertyName);
        property.intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetNestedFloat(
        Object target,
        string parentPropertyName,
        string propertyName,
        float value)
    {
        SerializedObject serialized = new(target);
        SerializedProperty parent = serialized.FindProperty(parentPropertyName);
        Assert.That(parent, Is.Not.Null, parentPropertyName);
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        Assert.That(property, Is.Not.Null, propertyName);
        property.floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
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
