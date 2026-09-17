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
}
