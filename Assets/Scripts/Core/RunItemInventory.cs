using System;
using System.Collections.Generic;

public sealed class RunItemInstance
{
    internal RunItemInstance(string itemId)
    {
        ItemId = itemId;
        Level = 1;
    }

    public string ItemId { get; }
    public int Level { get; private set; }

    internal bool TryIncreaseLevel()
    {
        if (Level < 1 || Level == int.MaxValue)
        {
            return false;
        }

        Level++;
        return true;
    }
}

public readonly struct ItemLevelLimit
{
    public const int DefaultMaximumLevel = 5;

    public ItemLevelLimit(int maximumLevel, bool isUnlimited)
    {
        MaximumLevel = Math.Max(1, maximumLevel);
        IsUnlimited = isUnlimited;
    }

    public int MaximumLevel { get; }
    public bool IsUnlimited { get; }

    public static ItemLevelLimit DefaultFinite =>
        new(DefaultMaximumLevel, false);
}

public enum ItemUpgradeResult
{
    Success = 0,
    InvalidItemId = 1,
    ItemNotOwned = 2,
    InvalidCurrentLevel = 3,
    MaximumLevelReached = 4,
    LevelOverflow = 5,
    ConfigurationUnavailable = 6
}

public sealed class RunItemInventory
{
    public const int Capacity = 4;

    private readonly List<RunItemInstance> ownedItems = new(Capacity);

    public IReadOnlyList<RunItemInstance> OwnedItems => ownedItems;
    public int Count => ownedItems.Count;
    public int AvailableSlotCount => Math.Max(0, Capacity - ownedItems.Count);
    public bool IsFull => ownedItems.Count >= Capacity;
    public event Action Changed;

    public bool Owns(string itemId) => GetOwned(itemId) != null;

    public RunItemInstance GetOwned(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        for (int i = 0; i < ownedItems.Count; i++)
        {
            if (string.Equals(ownedItems[i].ItemId, itemId, StringComparison.Ordinal))
            {
                return ownedItems[i];
            }
        }

        return null;
    }

    public bool TryAcquire(ItemDefinition definition)
    {
        if (definition == null || IsFull || Owns(definition.ItemId))
        {
            return false;
        }

        ownedItems.Add(new RunItemInstance(definition.ItemId));
        Changed?.Invoke();
        return true;
    }

    public bool CanUpgrade(
        string itemId,
        ItemLevelLimit levelLimit,
        out ItemUpgradeResult result)
    {
        if (!ItemCatalog.TryGet(itemId, out _))
        {
            result = ItemUpgradeResult.InvalidItemId;
            return false;
        }

        RunItemInstance owned = GetOwned(itemId);
        if (owned == null)
        {
            result = ItemUpgradeResult.ItemNotOwned;
            return false;
        }

        if (owned.Level < 1)
        {
            result = ItemUpgradeResult.InvalidCurrentLevel;
            return false;
        }

        if (owned.Level == int.MaxValue)
        {
            result = ItemUpgradeResult.LevelOverflow;
            return false;
        }

        if (!levelLimit.IsUnlimited && owned.Level >= levelLimit.MaximumLevel)
        {
            result = ItemUpgradeResult.MaximumLevelReached;
            return false;
        }

        result = ItemUpgradeResult.Success;
        return true;
    }

    public bool TryUpgrade(
        string itemId,
        ItemLevelLimit levelLimit,
        out ItemUpgradeResult result)
    {
        if (!CanUpgrade(itemId, levelLimit, out result))
        {
            return false;
        }

        RunItemInstance owned = GetOwned(itemId);
        if (owned == null || !owned.TryIncreaseLevel())
        {
            result = ItemUpgradeResult.LevelOverflow;
            return false;
        }

        Changed?.Invoke();
        result = ItemUpgradeResult.Success;
        return true;
    }

    public int GetElementLevel(ItemElement element)
    {
        if (!Enum.IsDefined(typeof(ItemElement), element))
        {
            return 0;
        }

        long total = 0;
        for (int i = 0; i < ownedItems.Count; i++)
        {
            RunItemInstance owned = ownedItems[i];
            if (!ItemCatalog.TryGet(owned.ItemId, out ItemDefinition definition) ||
                definition.Element != element || owned.Level < 1)
            {
                continue;
            }

            total += owned.Level;
            if (total >= int.MaxValue)
            {
                return int.MaxValue;
            }
        }

        return (int)total;
    }
}
