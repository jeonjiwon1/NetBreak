using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillTreeDefinition",
    menuName = "NETBREAK/Growth/Skill Tree Definition")]
public sealed class SkillTreeDefinition : ScriptableObject
{
    [SerializeField] private string treeId;
    [SerializeField] private ToolId tool;
    [SerializeField] private GrowthToolRole role;
    [SerializeField] private SkillTreeNodeDefinition[] nodes =
        Array.Empty<SkillTreeNodeDefinition>();

    public string TreeId => treeId;
    public ToolId Tool => tool;
    public GrowthToolRole Role => role;
    public IReadOnlyList<SkillTreeNodeDefinition> Nodes =>
        nodes ?? Array.Empty<SkillTreeNodeDefinition>();

    public bool TryGetNode(string nodeId, out SkillTreeNodeDefinition node)
    {
        if (!string.IsNullOrWhiteSpace(nodeId))
        {
            for (int i = 0; i < (nodes?.Length ?? 0); i++)
            {
                SkillTreeNodeDefinition candidate = nodes[i];
                if (candidate != null &&
                    string.Equals(candidate.NodeId, nodeId, StringComparison.Ordinal))
                {
                    node = candidate;
                    return true;
                }
            }
        }

        node = null;
        return false;
    }
}

[Serializable]
public sealed class SkillTreeNodeDefinition
{
    [SerializeField] private string nodeId;
    [SerializeField] private string displayName;
    [TextArea] [SerializeField] private string description;
    [SerializeField] private SkillTreeNodePrerequisite[] prerequisites =
        Array.Empty<SkillTreeNodePrerequisite>();
    [SerializeField] private SkillTreeRankDefinition[] ranks =
        Array.Empty<SkillTreeRankDefinition>();

    public string NodeId => nodeId;
    public string DisplayName => displayName;
    public string Description => description;
    public IReadOnlyList<SkillTreeNodePrerequisite> Prerequisites =>
        prerequisites ?? Array.Empty<SkillTreeNodePrerequisite>();
    public int MaxRank => ranks?.Length ?? 0;

    public SkillTreeRankDefinition GetRank(int zeroBasedRankIndex) =>
        ranks != null && zeroBasedRankIndex >= 0 && zeroBasedRankIndex < ranks.Length
            ? ranks[zeroBasedRankIndex]
            : null;

    public int GetCurrentRankLimit(SkillTreeProgressionContext context)
    {
        int limit = 0;
        for (int i = 0; i < (ranks?.Length ?? 0); i++)
        {
            SkillTreeRankDefinition rank = ranks[i];
            if (rank == null || !rank.UnlockCondition.IsMet(context))
            {
                break;
            }

            limit++;
        }

        return limit;
    }
}

[Serializable]
public sealed class SkillTreeNodePrerequisite
{
    [SerializeField] private string nodeId;
    [Min(1)] [SerializeField] private int requiredRank = 1;

    public string NodeId => nodeId;
    public int RequiredRank => requiredRank;
}

[Serializable]
public sealed class SkillTreeRankDefinition
{
    [Min(1)] [SerializeField] private int masteryPointCost = 1;
    [SerializeField] private SkillTreeRankUnlockCondition unlockCondition;
    [SerializeField] private SkillTreeBalanceEffect[] balanceEffects =
        Array.Empty<SkillTreeBalanceEffect>();

    public int MasteryPointCost => masteryPointCost;
    public SkillTreeRankUnlockCondition UnlockCondition => unlockCondition;
    public IReadOnlyList<SkillTreeBalanceEffect> BalanceEffects =>
        balanceEffects ?? Array.Empty<SkillTreeBalanceEffect>();
}

[Serializable]
public struct SkillTreeRankUnlockCondition
{
    [Min(0)] [SerializeField] private int minimumPlayerLevel;
    [Min(0)] [SerializeField] private int minimumArea;
    [SerializeField] private bool requiresMiniBossClear;
    [SerializeField] private bool requiresBossClear;

    public bool IsMet(SkillTreeProgressionContext context) =>
        context.PlayerLevel >= minimumPlayerLevel &&
        context.CurrentArea >= minimumArea &&
        (!requiresMiniBossClear || context.HasClearedMiniBoss) &&
        (!requiresBossClear || context.HasClearedBoss);
}

[Serializable]
public sealed class SkillTreeBalanceEffect
{
    [SerializeField] private string effectId;
    [SerializeField] private float value;

    public string EffectId => effectId;
    public float Value => value;
}

public readonly struct SkillTreeProgressionContext
{
    public SkillTreeProgressionContext(
        int playerLevel,
        int currentArea,
        bool hasClearedMiniBoss,
        bool hasClearedBoss)
    {
        PlayerLevel = Math.Max(0, playerLevel);
        CurrentArea = Math.Max(0, currentArea);
        HasClearedMiniBoss = hasClearedMiniBoss;
        HasClearedBoss = hasClearedBoss;
    }

    public int PlayerLevel { get; }
    public int CurrentArea { get; }
    public bool HasClearedMiniBoss { get; }
    public bool HasClearedBoss { get; }
}
