using System;
using System.Collections.Generic;

public enum GrowthToolRole
{
    Core = 0,
    Partner = 1
}

public enum GrowthAbilitySlot
{
    TacticalE = 0,
    SignatureR = 1
}

// Run data only. G2+ systems will connect this state to rewards and the active loadout.
public sealed class RunGrowthState
{
    private readonly RunSkillTreeProgress coreTree =
        new(GrowthToolRole.Core);
    private readonly RunSkillTreeProgress partnerTree =
        new(GrowthToolRole.Partner);
    private readonly RunGrowthAbilityState tacticalAbility = new();
    private readonly RunGrowthAbilityState signatureAbility = new();

    public RunSkillTreeProgress CoreTree => coreTree;
    public RunSkillTreeProgress PartnerTree => partnerTree;
    public ToolId SelectedCoreTool => coreTree.Tool;
    public ToolId SelectedPartnerTool => partnerTree.Tool;
    public int AvailableMasteryPoints { get; private set; }
    public int SpentMasteryPoints { get; private set; }

    public RunSkillTreeProgress GetTree(GrowthToolRole role) => role switch
    {
        GrowthToolRole.Core => coreTree,
        GrowthToolRole.Partner => partnerTree,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    public RunGrowthAbilityState GetAbility(GrowthAbilitySlot slot) => slot switch
    {
        GrowthAbilitySlot.TacticalE => tacticalAbility,
        GrowthAbilitySlot.SignatureR => signatureAbility,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };

    public bool TrySelectTool(GrowthToolRole role, ToolId tool)
    {
        if (!Enum.IsDefined(typeof(GrowthToolRole), role) ||
            !IsActiveTool(tool))
        {
            return false;
        }

        RunSkillTreeProgress target = GetTree(role);
        RunSkillTreeProgress other = GetTree(
            role == GrowthToolRole.Core
                ? GrowthToolRole.Partner
                : GrowthToolRole.Core);

        if (target.Tool != ToolId.None || other.Tool == tool)
        {
            return false;
        }

        target.SelectTool(tool);
        return true;
    }

    public bool TryGrantMasteryPoints(int amount)
    {
        if (amount <= 0 || amount > int.MaxValue - AvailableMasteryPoints)
        {
            return false;
        }

        AvailableMasteryPoints += amount;
        return true;
    }

    public bool CanSpendMasteryPoints(int amount) =>
        amount > 0 && AvailableMasteryPoints >= amount;

    public bool TrySpendMasteryPoints(int amount)
    {
        if (!CanSpendMasteryPoints(amount) ||
            amount > int.MaxValue - SpentMasteryPoints)
        {
            return false;
        }

        AvailableMasteryPoints -= amount;
        SpentMasteryPoints += amount;
        return true;
    }

    public bool TryPurchaseNodeRank(
        GrowthToolRole role,
        SkillTreeDefinition treeDefinition,
        string nodeId,
        SkillTreeProgressionContext context)
    {
        if (!Enum.IsDefined(typeof(GrowthToolRole), role))
        {
            return false;
        }

        RunSkillTreeProgress progress = GetTree(role);
        if (treeDefinition == null ||
            treeDefinition.Role != role ||
            treeDefinition.Tool != progress.Tool ||
            string.IsNullOrWhiteSpace(treeDefinition.TreeId) ||
            (!string.IsNullOrEmpty(progress.TreeId) &&
             !string.Equals(
                 progress.TreeId,
                 treeDefinition.TreeId,
                 StringComparison.Ordinal)) ||
            !treeDefinition.TryGetNode(nodeId, out SkillTreeNodeDefinition node))
        {
            return false;
        }

        int currentRank = progress.GetNodeRank(node.NodeId);
        if (currentRank >= node.MaxRank ||
            currentRank >= node.GetCurrentRankLimit(context) ||
            !ArePrerequisitesMet(progress, node))
        {
            return false;
        }

        SkillTreeRankDefinition nextRank = node.GetRank(currentRank);
        if (nextRank == null)
        {
            return false;
        }

        int cost = nextRank.MasteryPointCost;
        if (!TrySpendMasteryPoints(cost))
        {
            return false;
        }

        progress.SetNodeRank(
            treeDefinition.TreeId,
            node.NodeId,
            currentRank + 1);
        return true;
    }

    public bool UnlockAbility(GrowthAbilitySlot slot) =>
        Enum.IsDefined(typeof(GrowthAbilitySlot), slot) &&
        GetAbility(slot).Unlock();

    public bool TryEquipAbility(GrowthAbilitySlot slot, string abilityId) =>
        Enum.IsDefined(typeof(GrowthAbilitySlot), slot) &&
        GetAbility(slot).TryEquip(abilityId);

    private static bool ArePrerequisitesMet(
        RunSkillTreeProgress progress,
        SkillTreeNodeDefinition node)
    {
        IReadOnlyList<SkillTreeNodePrerequisite> prerequisites =
            node.Prerequisites;

        for (int i = 0; i < prerequisites.Count; i++)
        {
            SkillTreeNodePrerequisite prerequisite = prerequisites[i];
            if (prerequisite == null ||
                string.IsNullOrWhiteSpace(prerequisite.NodeId) ||
                prerequisite.RequiredRank <= 0 ||
                progress.GetNodeRank(prerequisite.NodeId) < prerequisite.RequiredRank)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsActiveTool(ToolId tool) =>
        tool != ToolId.None &&
        tool != ToolId.LandingNet &&
        Enum.IsDefined(typeof(ToolId), tool);
}

public sealed class RunSkillTreeProgress
{
    private readonly Dictionary<string, int> purchasedNodeRanks =
        new(StringComparer.Ordinal);

    public RunSkillTreeProgress(GrowthToolRole role)
    {
        Role = role;
    }

    public GrowthToolRole Role { get; }
    public ToolId Tool { get; private set; } = ToolId.None;
    public string TreeId { get; private set; } = string.Empty;
    public IReadOnlyDictionary<string, int> PurchasedNodeRanks =>
        purchasedNodeRanks;

    public int GetNodeRank(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return 0;
        }

        return purchasedNodeRanks.TryGetValue(nodeId, out int rank)
            ? rank
            : 0;
    }

    internal void SelectTool(ToolId tool)
    {
        Tool = tool;
    }

    internal void SetNodeRank(string treeId, string nodeId, int rank)
    {
        TreeId = treeId;
        purchasedNodeRanks[nodeId] = rank;
    }
}

public sealed class RunGrowthAbilityState
{
    public bool IsUnlocked { get; private set; }
    public string EquippedAbilityId { get; private set; } = string.Empty;

    internal bool Unlock()
    {
        if (IsUnlocked)
        {
            return false;
        }

        IsUnlocked = true;
        return true;
    }

    internal bool TryEquip(string abilityId)
    {
        if (!IsUnlocked || string.IsNullOrWhiteSpace(abilityId))
        {
            return false;
        }

        EquippedAbilityId = abilityId;
        return true;
    }
}
