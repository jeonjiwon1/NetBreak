using System;
using System.Collections.Generic;

public static class SkillTreeEffectIds
{
    public const string FishingRodPower = "fishing_rod_power";
    public const string FishingRodRange = "fishing_rod_range";
    public const string FishingRodCapacity = "fishing_rod_capacity";
    public const string FishingRodCostMultiplier = "fishing_rod_cost_multiplier";
    public const string NetLength = "net_length";
    public const string NetCount = "net_count";
    public const string NetCostMultiplier = "net_cost_multiplier";
    public const string NetPowerMultiplier = "net_power_multiplier";
    public const string CastNetPower = "cast_net_power";
    public const string CastNetRadius = "cast_net_radius";
    public const string CastNetCharges = "cast_net_charges";
    public const string BaitRadius = "bait_radius";
    public const string BaitDuration = "bait_duration";
}

public static class VerticalSliceSkillTreeDefaults
{
    public static IReadOnlyList<SkillTreeDefinition> CreateAll()
    {
        List<SkillTreeDefinition> definitions = new();
        AddForRole(definitions, ToolId.FishingRod, GrowthToolRole.Core);
        AddForRole(definitions, ToolId.Net, GrowthToolRole.Core);
        AddForRole(definitions, ToolId.CastNet, GrowthToolRole.Core);
        AddForRole(definitions, ToolId.FishingRod, GrowthToolRole.Partner);
        AddForRole(definitions, ToolId.Net, GrowthToolRole.Partner);
        AddForRole(definitions, ToolId.CastNet, GrowthToolRole.Partner);
        AddForRole(definitions, ToolId.Bait, GrowthToolRole.Partner);
        return definitions;
    }

    private static void AddForRole(
        ICollection<SkillTreeDefinition> definitions,
        ToolId tool,
        GrowthToolRole role)
    {
        string treeId = $"vs_{role.ToString().ToLowerInvariant()}_{tool.ToString().ToLowerInvariant()}";
        List<SkillTreeNodeDefinition> nodes = new();

        switch (tool)
        {
            case ToolId.FishingRod:
                nodes.Add(CreatePrimary(
                    "rod_power",
                    "강한 낚싯바늘",
                    "낚싯대의 포획력을 랭크마다 높입니다.",
                    SkillTreeEffectIds.FishingRodPower,
                    1f, 1.5f, 2f));
                nodes.Add(CreateSecondary(
                    "rod_range",
                    "긴 낚싯줄",
                    "낚싯대의 포획 범위를 0.5 늘립니다.",
                    "rod_power",
                    SkillTreeEffectIds.FishingRodRange,
                    0.5f));
                nodes.Add(CreateAdvanced(
                    "rod_capacity",
                    "다중 낚싯대 운용",
                    "미니보스 포획 후 낚싯대 설치 상한을 단계적으로 늘립니다.",
                    "rod_power",
                    SkillTreeEffectIds.FishingRodCapacity,
                    (1, 3f), (1, 3f), (2, 3f)));
                nodes.Add(CreateAdvanced(
                    "rod_economy",
                    "효율적인 낚싯대 설치",
                    "미니보스 포획 후 낚싯대 설치 비용을 단계적으로 줄입니다.",
                    "rod_capacity",
                    SkillTreeEffectIds.FishingRodCostMultiplier,
                    (1, 0.9486833f), (1, 0.9486833f)));
                break;

            case ToolId.Net:
                nodes.Add(CreatePrimary(
                    "net_length",
                    "질긴 그물실",
                    "설치 가능한 그물의 최대 길이를 늘립니다.",
                    SkillTreeEffectIds.NetLength,
                    1f, 1.5f, 2f));
                nodes.Add(new SkillTreeNodeDefinition(
                    "net_count",
                    "여분의 그물",
                    "그물 설치 상한을 늘립니다. 후속 랭크는 미니보스 포획 후 열립니다.",
                    new[] { new SkillTreeNodePrerequisite("net_length", 1) },
                    new[]
                    {
                        Rank(1, 5, SkillTreeEffectIds.NetCount, 1f),
                        AdvancedRank(1, SkillTreeEffectIds.NetCount, 2f),
                        AdvancedRank(2, SkillTreeEffectIds.NetCount, 4f)
                    }));
                nodes.Add(CreateAdvanced(
                    "net_economy",
                    "효율적인 그물 설치",
                    "미니보스 포획 후 그물 설치 비용을 단계적으로 줄입니다.",
                    "net_count",
                    SkillTreeEffectIds.NetCostMultiplier,
                    (1, 0.83666f), (1, 0.83666f)));
                nodes.Add(CreateAdvanced(
                    "net_power",
                    "강화 그물코",
                    "미니보스 포획 후 그물 포획력을 단계적으로 높입니다.",
                    "net_count",
                    SkillTreeEffectIds.NetPowerMultiplier,
                    (1, 1.73205f), (2, 1.73205f)));
                break;

            case ToolId.CastNet:
                nodes.Add(CreatePrimary(
                    "cast_power",
                    "무거운 투망",
                    "투망의 포획력을 랭크마다 높입니다.",
                    SkillTreeEffectIds.CastNetPower,
                    5f, 7f, 10f));
                nodes.Add(CreateSecondary(
                    "cast_radius",
                    "넓은 투망",
                    "투망의 포획 반경을 0.25 늘립니다.",
                    "cast_power",
                    SkillTreeEffectIds.CastNetRadius,
                    0.25f));
                nodes.Add(CreateAdvanced(
                    "cast_charges",
                    "여분의 투망",
                    "미니보스 포획 후 투망 최대 충전을 단계적으로 늘립니다.",
                    "cast_power",
                    SkillTreeEffectIds.CastNetCharges,
                    (1, 1f), (2, 1f)));
                break;

            case ToolId.Bait:
                nodes.Add(CreatePrimary(
                    "bait_radius",
                    "진한 미끼 향",
                    "미끼의 유인 반경을 랭크마다 늘립니다.",
                    SkillTreeEffectIds.BaitRadius,
                    0.75f, 1f, 1.25f));
                nodes.Add(CreateSecondary(
                    "bait_duration",
                    "오래가는 미끼",
                    "미끼의 지속시간을 1초 늘립니다.",
                    "bait_radius",
                    SkillTreeEffectIds.BaitDuration,
                    1f));
                break;

            default:
                return;
        }

        definitions.Add(SkillTreeDefinition.CreateRuntime(
            treeId,
            tool,
            role,
            nodes.ToArray()));
    }

    private static SkillTreeNodeDefinition CreatePrimary(
        string nodeId,
        string name,
        string description,
        string effectId,
        float rank1,
        float rank2,
        float rank3)
    {
        return new SkillTreeNodeDefinition(
            nodeId,
            name,
            description,
            Array.Empty<SkillTreeNodePrerequisite>(),
            new[]
            {
                Rank(1, 4, effectId, rank1),
                Rank(2, 6, effectId, rank2),
                Rank(3, 0, effectId, rank3, 2)
            });
    }

    private static SkillTreeNodeDefinition CreateSecondary(
        string nodeId,
        string name,
        string description,
        string prerequisiteId,
        string effectId,
        float value)
    {
        return new SkillTreeNodeDefinition(
            nodeId,
            name,
            description,
            new[] { new SkillTreeNodePrerequisite(prerequisiteId, 1) },
            new[] { Rank(1, 5, effectId, value) });
    }

    private static SkillTreeRankDefinition Rank(
        int cost,
        int minimumLevel,
        string effectId,
        float value,
        int minimumArea = 1)
    {
        return new SkillTreeRankDefinition(
            cost,
            new SkillTreeRankUnlockCondition(minimumLevel, minimumArea),
            new SkillTreeBalanceEffect(effectId, value));
    }

    private static SkillTreeNodeDefinition CreateAdvanced(
        string nodeId,
        string name,
        string description,
        string prerequisiteId,
        string effectId,
        params (int cost, float value)[] ranks)
    {
        SkillTreeRankDefinition[] rankDefinitions =
            new SkillTreeRankDefinition[ranks.Length];
        for (int i = 0; i < ranks.Length; i++)
        {
            rankDefinitions[i] = AdvancedRank(
                ranks[i].cost,
                effectId,
                ranks[i].value);
        }

        return new SkillTreeNodeDefinition(
            nodeId,
            name,
            description,
            new[] { new SkillTreeNodePrerequisite(prerequisiteId, 1) },
            rankDefinitions);
    }

    private static SkillTreeRankDefinition AdvancedRank(
        int cost,
        string effectId,
        float value) =>
        new(
            cost,
            new SkillTreeRankUnlockCondition(0, 1, miniBossClear: true),
            new SkillTreeBalanceEffect(effectId, value));
}
