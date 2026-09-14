using System;

// Run data only: no scene objects, area rules, or meta unlocks.
public sealed class RunToolLoadout
{
    public const int SlotCount = 4;
    private readonly ToolId[] slots = new ToolId[SlotCount];

    public int Revision { get; private set; }

    public ToolId GetSlot(int index) =>
        index >= 0 && index < SlotCount ? slots[index] : ToolId.None;

    public int FindSlot(ToolId tool) =>
        tool == ToolId.None ? -1 : Array.IndexOf(slots, tool);

    // Moving an already slotted tool swaps the two slots; None clears a slot.
    public bool TryAssignSlot(int index, ToolId tool)
    {
        if (index < 0 || index >= SlotCount || tool == ToolId.LandingNet ||
            !Enum.IsDefined(typeof(ToolId), tool))
        {
            return false;
        }

        if (slots[index] == tool)
        {
            return true;
        }

        int previousIndex = FindSlot(tool);
        if (previousIndex >= 0)
        {
            slots[previousIndex] = slots[index];
        }

        slots[index] = tool;
        Revision++;
        return true;
    }
}
