using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public readonly struct ToolInputState
{
    public bool Pressed { get; }
    public bool Released { get; }
    public bool Cancelled { get; }

    public ToolInputState(bool pressed, bool released, bool cancelled)
    {
        Pressed = pressed;
        Released = released;
        Cancelled = cancelled;
    }
}

// One sample before controller Updates prevents placement completion from also
// using the landing net, and keeps a release tied to the original slot press.
public sealed class ToolSlotInput
{
    private readonly RunToolLoadout loadout;
    private readonly ToolInputState[] states = new ToolInputState[RunToolLoadout.SlotCount];
    private readonly bool[] held = new bool[RunToolLoadout.SlotCount];
    private int revision;
    private int sampledFrame = -1;
    private bool worldPointerReserved;
    private bool blocked;

    public ToolSlotInput(RunToolLoadout loadout)
    {
        this.loadout = loadout;
        revision = loadout.Revision;
    }

    public static bool IsSelectionOrEndBlocked =>
        (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsOpen) ||
        (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsChoosingTool) ||
        (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsChoosingAugment) ||
        (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
        TacticalSkillManager.IsSelectionPendingOrActive ||
        (PrototypeGameFlowManager.Instance != null && PrototypeGameFlowManager.Instance.IsGameEnded);

    public static bool IsWorldPointerReserved =>
        TacticalSkillManager.IsTargeting ||
        RunManager.Instance == null || RunManager.Instance.ToolInput.PointerReserved;

    private bool PointerReserved => sampledFrame != Time.frameCount || blocked ||
        IsSelectionOrEndBlocked || worldPointerReserved;

    public static ToolInputState Read(ToolId tool)
    {
        if (RunManager.Instance == null)
        {
            return new ToolInputState(false, false, true);
        }

        return RunManager.Instance.ToolInput.GetState(tool);
    }

    public ToolInputState GetState(ToolId tool)
    {
        int slot = loadout.FindSlot(tool);
        if (slot < 0 || sampledFrame != Time.frameCount ||
            revision != loadout.Revision || IsSelectionOrEndBlocked ||
            TacticalSkillManager.IsTargeting)
        {
            return new ToolInputState(false, false, true);
        }

        return states[slot];
    }

    public void Sample()
    {
        sampledFrame = Time.frameCount;
        bool changed = revision != loadout.Revision;
        revision = loadout.Revision;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        blocked = keyboard == null || mouse == null || IsSelectionOrEndBlocked;
        bool cancel = blocked || changed ||
            (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
            (mouse != null && mouse.rightButton.wasPressedThisFrame);
        bool reposition = GearRepositionController.IsRepositioning ||
            GearRepositionController.IsRepositionModifierHeld;
        bool tacticalTargeting = TacticalSkillManager.IsTargeting;
        bool netMode = NetPlacementController.IsNetModeActive;
        bool rodMode = FishingRodPlacementController.IsRodModeActive;
        bool preparation = PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsPreparation;

        worldPointerReserved = netMode || rodMode || reposition || cancel ||
            TacticalSkillManager.IsTargeting;
        bool pressAccepted = false;
        bool placementPressed = false;
        for (int i = 0; i < RunToolLoadout.SlotCount; i++)
        {
            ToolId tool = loadout.GetSlot(i);
            ButtonControl key = GetSlotKey(keyboard, i);
            bool toolBlocked = cancel || reposition || tacticalTargeting || tool == ToolId.None ||
                (netMode && tool != ToolId.Net) || (rodMode && tool != ToolId.FishingRod) ||
                (preparation && tool != ToolId.Net && tool != ToolId.FishingRod);
            bool pressed = !toolBlocked && !pressAccepted && key != null && key.wasPressedThisFrame;
            bool released = !toolBlocked && held[i] && key != null && key.wasReleasedThisFrame;
            states[i] = new ToolInputState(pressed, released, toolBlocked);
            held[i] = !toolBlocked && (pressed || (held[i] && key != null && key.isPressed));
            pressAccepted |= pressed;
            if (pressed && (tool == ToolId.Net || tool == ToolId.FishingRod))
            {
                worldPointerReserved = true;
                placementPressed = true;
            }
        }

        // Starting placement cancels an existing cast aim even if its key is
        // released in this frame, regardless of controller Update order.
        int castSlot = loadout.FindSlot(ToolId.CastNet);
        if (placementPressed && castSlot >= 0)
        {
            states[castSlot] = new ToolInputState(false, false, true);
            held[castSlot] = false;
        }
    }

    private static ButtonControl GetSlotKey(Keyboard keyboard, int slot)
    {
        if (keyboard == null) return null;
        switch (slot)
        {
            case 0: return keyboard.qKey;
            case 1: return keyboard.wKey;
            default: return null;
        }
    }

    public static string GetBindingLabel(ToolId tool)
    {
        if (tool == ToolId.LandingNet) return "LMB";
        int slot = RunManager.Instance != null ? RunManager.Instance.ToolSlots.FindSlot(tool) : -1;
        switch (slot)
        {
            case 0: return "Q";
            case 1: return "W";
            default: return "미배치";
        }
    }
}
