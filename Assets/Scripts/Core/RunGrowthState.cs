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

public enum RunMilestoneType
{
    MiniBoss = 0,
    Boss = 1
}

public enum GrowthManagementPage
{
    SkillTree = 0,
    Item = 1
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
    private readonly HashSet<int> rewardedMiniBossAreas = new();
    private readonly HashSet<int> rewardedBossAreas = new();
    private readonly RunItemInventory itemInventory = new();
    private readonly RunCombinedSynergyState combinedSynergy;

    public RunGrowthState(
        float combinedSynergySwitchCooldown = 30f,
        CombinedSynergyUnlockSettings combinedSynergyUnlockSettings = null)
    {
        combinedSynergy = new RunCombinedSynergyState(
            combinedSynergySwitchCooldown,
            combinedSynergyUnlockSettings);
    }

    public RunSkillTreeProgress CoreTree => coreTree;
    public RunSkillTreeProgress PartnerTree => partnerTree;
    public RunItemInventory ItemInventory => itemInventory;
    public RunCombinedSynergyState CombinedSynergy => combinedSynergy;
    public ToolId SelectedCoreTool => coreTree.Tool;
    public ToolId SelectedPartnerTool => partnerTree.Tool;
    public int AvailableMasteryPoints { get; private set; }
    public int SpentMasteryPoints { get; private set; }
    public int CurrentArea { get; private set; } = 1;
    public GrowthManagementPage LastGrowthManagementPage { get; private set; } =
        GrowthManagementPage.SkillTree;
    public bool HasClearedCurrentAreaMiniBoss =>
        rewardedMiniBossAreas.Contains(CurrentArea);
    public bool HasClearedCurrentAreaBoss =>
        rewardedBossAreas.Contains(CurrentArea);
    public event Action<int> AvailableMasteryPointsChanged;
    public event Action<GrowthManagementPage> GrowthManagementPageChanged;

    public bool TrySetGrowthManagementPage(GrowthManagementPage page)
    {
        if (!Enum.IsDefined(typeof(GrowthManagementPage), page))
        {
            return false;
        }

        if (LastGrowthManagementPage == page)
        {
            return true;
        }

        LastGrowthManagementPage = page;
        GrowthManagementPageChanged?.Invoke(page);
        return true;
    }

    public int GetElementLevel(ItemElement element) =>
        itemInventory.GetElementLevel(element);

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

    public bool CanAcquireTool(
        GrowthToolRole role,
        ToolId tool,
        int masteryPointCost)
    {
        if (!Enum.IsDefined(typeof(GrowthToolRole), role) ||
            !IsActiveTool(tool) ||
            !CanSpendMasteryPoints(masteryPointCost))
        {
            return false;
        }

        RunSkillTreeProgress target = GetTree(role);
        RunSkillTreeProgress other = GetTree(
            role == GrowthToolRole.Core
                ? GrowthToolRole.Partner
                : GrowthToolRole.Core);

        return target.Tool == ToolId.None && other.Tool != tool;
    }

    public bool TryAcquireTool(
        GrowthToolRole role,
        ToolId tool,
        int masteryPointCost)
    {
        if (!CanAcquireTool(role, tool, masteryPointCost) ||
            masteryPointCost > int.MaxValue - SpentMasteryPoints)
        {
            return false;
        }

        AvailableMasteryPoints -= masteryPointCost;
        SpentMasteryPoints += masteryPointCost;
        GetTree(role).SelectTool(tool);
        AvailableMasteryPointsChanged?.Invoke(AvailableMasteryPoints);
        return true;
    }

    public bool TryGrantMasteryPoints(int amount)
    {
        if (amount <= 0 || amount > int.MaxValue - AvailableMasteryPoints)
        {
            return false;
        }

        AvailableMasteryPoints += amount;
        AvailableMasteryPointsChanged?.Invoke(AvailableMasteryPoints);
        return true;
    }

    public bool TryGrantMilestoneMasteryPoints(
        RunMilestoneType milestone,
        int area,
        int amount)
    {
        if (!Enum.IsDefined(typeof(RunMilestoneType), milestone) ||
            area <= 0 ||
            amount <= 0)
        {
            return false;
        }

        HashSet<int> rewardedAreas = milestone == RunMilestoneType.MiniBoss
            ? rewardedMiniBossAreas
            : rewardedBossAreas;

        if (rewardedAreas.Contains(area))
        {
            return false;
        }

        rewardedAreas.Add(area);
        if (TryGrantMasteryPoints(amount))
        {
            return true;
        }

        rewardedAreas.Remove(area);
        return false;
    }

    public bool HasClearedMilestone(
        RunMilestoneType milestone,
        int area)
    {
        if (!Enum.IsDefined(typeof(RunMilestoneType), milestone) || area <= 0)
        {
            return false;
        }

        return milestone == RunMilestoneType.MiniBoss
            ? rewardedMiniBossAreas.Contains(area)
            : rewardedBossAreas.Contains(area);
    }

    public bool TrySetCurrentArea(int area)
    {
        if (area <= 0 || area < CurrentArea)
        {
            return false;
        }

        CurrentArea = area;
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
        AvailableMasteryPointsChanged?.Invoke(AvailableMasteryPoints);
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
        if (!CanSpendMasteryPoints(cost) ||
            cost > int.MaxValue - SpentMasteryPoints)
        {
            return false;
        }

        AvailableMasteryPoints -= cost;
        SpentMasteryPoints += cost;
        progress.SetNodeRank(
            treeDefinition.TreeId,
            node.NodeId,
            currentRank + 1);
        AvailableMasteryPointsChanged?.Invoke(AvailableMasteryPoints);
        return true;
    }

    public bool TryPurchaseAbilityNodeRank(
        GrowthAbilitySlot slot,
        SkillTreeDefinition treeDefinition,
        string nodeId,
        SkillTreeProgressionContext context)
    {
        if (!Enum.IsDefined(typeof(GrowthAbilitySlot), slot) ||
            treeDefinition == null ||
            treeDefinition.Category != (slot == GrowthAbilitySlot.TacticalE
                ? GrowthTreeCategory.TacticalE
                : GrowthTreeCategory.SignatureR) ||
            !treeDefinition.TryGetNode(nodeId, out SkillTreeNodeDefinition node))
        {
            return false;
        }

        RunGrowthAbilityState ability = GetAbility(slot);
        if (!ability.IsUnlocked ||
            string.IsNullOrWhiteSpace(ability.EquippedAbilityId) ||
            !string.Equals(ability.EquippedAbilityId, treeDefinition.AbilityId,
                StringComparison.Ordinal) ||
            (!string.IsNullOrEmpty(ability.TreeId) &&
             !string.Equals(ability.TreeId, treeDefinition.TreeId,
                StringComparison.Ordinal)))
        {
            return false;
        }

        int currentRank = ability.GetNodeRank(node.NodeId);
        if (currentRank >= node.MaxRank ||
            currentRank >= node.GetCurrentRankLimit(context) ||
            !ArePrerequisitesMet(ability, node))
        {
            return false;
        }

        SkillTreeRankDefinition nextRank = node.GetRank(currentRank);
        int cost = nextRank?.MasteryPointCost ?? 0;
        if (nextRank == null || !CanSpendMasteryPoints(cost) ||
            cost > int.MaxValue - SpentMasteryPoints)
        {
            return false;
        }

        AvailableMasteryPoints -= cost;
        SpentMasteryPoints += cost;
        ability.SetNodeRank(treeDefinition.TreeId, node.NodeId, currentRank + 1);
        AvailableMasteryPointsChanged?.Invoke(AvailableMasteryPoints);
        return true;
    }

    public bool UnlockAbility(GrowthAbilitySlot slot) =>
        Enum.IsDefined(typeof(GrowthAbilitySlot), slot) &&
        GetAbility(slot).Unlock();

    public bool TryEquipAbility(GrowthAbilitySlot slot, string abilityId) =>
        Enum.IsDefined(typeof(GrowthAbilitySlot), slot) &&
        GetAbility(slot).TryEquip(abilityId);

    public bool TryUnlockAndEquipAbility(
        GrowthAbilitySlot slot,
        string abilityId) =>
        Enum.IsDefined(typeof(GrowthAbilitySlot), slot) &&
        GetAbility(slot).TryUnlockAndEquip(abilityId);

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

    private static bool ArePrerequisitesMet(
        RunGrowthAbilityState progress,
        SkillTreeNodeDefinition node)
    {
        IReadOnlyList<SkillTreeNodePrerequisite> prerequisites = node.Prerequisites;
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
    private readonly Dictionary<string, int> purchasedNodeRanks =
        new(StringComparer.Ordinal);

    public bool IsUnlocked { get; private set; }
    public string EquippedAbilityId { get; private set; } = string.Empty;
    public string TreeId { get; private set; } = string.Empty;
    public IReadOnlyDictionary<string, int> PurchasedNodeRanks => purchasedNodeRanks;

    public int GetNodeRank(string nodeId) =>
        !string.IsNullOrWhiteSpace(nodeId) &&
        purchasedNodeRanks.TryGetValue(nodeId, out int rank)
            ? rank
            : 0;

    internal void SetNodeRank(string treeId, string nodeId, int rank)
    {
        TreeId = treeId;
        purchasedNodeRanks[nodeId] = rank;
    }

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

    internal bool TryUnlockAndEquip(string abilityId)
    {
        if (IsUnlocked || string.IsNullOrWhiteSpace(abilityId))
        {
            return false;
        }

        IsUnlocked = true;
        EquippedAbilityId = abilityId;
        return true;
    }
}
